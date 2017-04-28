using System;
using RasPiBtControl.Model;
using RasPiBtControl.Services;
using ReactiveUI;
using Splat;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Xamarin.Forms;
using System.Reactive;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace RasPiBtControl.UI
{
    public class LandingPageViewModel : ReactiveObject
    {
        private IProgressDialogService _progressDialogService;
        private IBtDiscovery _btDiscovery;
        private IBtClient _btClient;

        private List<BtDeviceInfo> _pairedDevices;
        /// <summary>
        /// List of paired devices
        /// </summary>
        public List<BtDeviceInfo> PairedDevices
        {
            get { return _pairedDevices; }
            set { this.RaiseAndSetIfChanged(ref _pairedDevices, value); }
        }

        private BtDeviceInfo _selectedDevice;
        /// <summary>
        /// Current selected device
        /// </summary>
        public BtDeviceInfo SelectedDevice
        {
            get { return _selectedDevice; }
            set { this.RaiseAndSetIfChanged(ref _selectedDevice, value); }
        }

        private string _message;
        /// <summary>
        /// Last message from server
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }

        /// <summary>
        /// List of supported operations by the server
        /// </summary>
        public ObservableCollection<string> Operations { get; set; }

        /// <summary>
        /// Refreshes the list of paired devices
        /// </summary>
        public ReactiveCommand<Unit, Unit> Refresh { get; set; }

        /// <summary>
        /// Executes an operation from the available list
        /// </summary>
        public ReactiveCommand<string, Unit> Execute { get; set; }

        public LandingPageViewModel()
        {
            _progressDialogService = Locator.Current.GetService<IProgressDialogService>();
            _btDiscovery = Locator.Current.GetService<IBtDiscovery>();
            _btClient = Locator.Current.GetService<IBtClient>();

            _btClient.ReceivedData += _btClient_ReceivedData;

            Operations = new ObservableCollection<string>();
            
            // Subscribe to the update of paired devices property
            this.WhenAnyValue(vm => vm._btDiscovery.PairedDevices)
                .Subscribe((pairedDevices) =>
                {
                    PairedDevices = pairedDevices;
                });

            // Display a progress dialog when the bluetooth discovery takes place
            this.WhenAnyValue(vm => vm._btDiscovery.IsBusy)
                .DistinctUntilChanged()
                .Subscribe((isBusy) =>
                {
                    if(isBusy)
                    {
                        _progressDialogService.Show("Please wait...");
                    }
                    else
                    {
                        _progressDialogService.Hide();
                    }
                });

            // The refresh command
            Refresh = ReactiveCommand.Create(() =>
            {
                _btDiscovery.Refresh();
            });

            // Handle the selection of a device from the list
            this.WhenAnyValue(vm => vm.SelectedDevice)                
                .Subscribe((device) =>
                {
                    if (device == null)
                    {
                        return;
                    }

                    if(!device.HasRequiredServiceID)
                    {
                        Message = "Device not supported or service not running";
                        return;
                    }

                    if(_btClient.Connect(device.Address))
                    {
                        // Fetch supported operations from the server
                        _btClient.SendData("getop");
                    }
                    else
                    {
                        Message = "Unable to connect to device";
                        return;
                    }
                });

            // Execute an operation
            this.Execute = ReactiveCommand.Create<string>((operation) =>
            {
                _btClient.SendData(operation);
            });

            Refresh.Execute().Subscribe();
        }

        /// <summary>
        /// Handles data received from server
        /// </summary>
        /// <param name="sender">Client instance</param>
        /// <param name="data">The data</param>
        private void _btClient_ReceivedData(object sender, string data)
        {
            if(data.StartsWith("msg"))
            {
                var parts = data.Split(':');

                Message = parts[1];
            }
            else if(data.StartsWith("op"))
            {
                var parts = data.Split(':');
                var operations = parts[1].Split(',');

                Operations.Clear();
                foreach(var op in operations)
                {
                    Operations.Add(op);
                }
            }
            // Insert more handlers here
            else
            {
                Message = $"Unknown response {data}";
            }
        }
    }
}
