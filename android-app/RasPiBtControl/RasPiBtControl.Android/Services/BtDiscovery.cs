using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RasPiBtControl.Services;
using RasPiBtControl.Model;
using System.ComponentModel;
using Android.Bluetooth;

namespace RasPiBtControl.Droid.Services
{
    /// <summary>
    /// Used to discover paired devices and their services
    /// </summary>
    public class BtDiscovery : BroadcastReceiver, IBtDiscovery
    {
        private MainActivity _mainActivity;

        public bool IsBusy { get; set; }
        public List<BtDeviceInfo> PairedDevices { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public BtDiscovery(MainActivity mainActivity)
        {
            // Register self as receiver for BT adapter state change and service discovery responses
            this._mainActivity = mainActivity;
            this._mainActivity.RegisterReceiver(this, new IntentFilter(BluetoothAdapter.ActionStateChanged));
            this._mainActivity.RegisterReceiver(this, new IntentFilter(BluetoothDevice.ActionUuid));

            this.PairedDevices = new List<BtDeviceInfo>();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PairedDevices"));
        }

        /// <summary>
        /// Refreshes the list of paired devices
        /// </summary>
        public void Refresh()
        {
            IsBusy = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBusy"));

            var btAdapter = BluetoothAdapter.DefaultAdapter;

            // Make sure adapter is enabled first
            if (!btAdapter.IsEnabled)
            {
                btAdapter.Enable();
            }
            else
            {
                DoRefresh();
            }
        }

        /// <summary>
        /// Handle any broadcast messages
        /// </summary>
        /// <param name="context"></param>
        /// <param name="intent"></param>
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(BluetoothAdapter.ActionStateChanged))
            {
                // If bluetooth state changed we can refresh the list of paired devices
                DoRefresh();
            }
            else if (intent.Action.Equals(BluetoothDevice.ActionUuid))
            {
                // Handle the result of bluetooth service discovery on the current device
                var actualDevice = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice).JavaCast<BluetoothDevice>();
                if (actualDevice == null)
                {
                    IsBusy = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBusy"));
                    return;
                }

                var scannedDevice = this.PairedDevices.FirstOrDefault(d => d.Address.Equals(actualDevice.Address));
                if (scannedDevice == null)
                {
                    IsBusy = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBusy"));
                    return;
                }

                scannedDevice.ServicesDiscovered = true;
                var uuids = actualDevice.GetUuids();
                if (uuids != null)
                {
                    scannedDevice.HasRequiredServiceID = uuids.Any(p => p.ToString().Equals(BtDeviceInfo.RequiredServiceID));
                }

                if (!scannedDevice.HasRequiredServiceID)
                {
                    ScanNextDeviceServices();
                }
                else
                {
                    IsBusy = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBusy"));
                }
            }
        }

        /// <summary>
        /// Actual method to refresh the list of paired devices
        /// </summary>
        private void DoRefresh()
        {
            var btAdapter = BluetoothAdapter.DefaultAdapter;

            if (btAdapter.IsEnabled)
            {
                this.PairedDevices = new List<BtDeviceInfo>();

                foreach (var device in btAdapter.BondedDevices)
                {
                    this.PairedDevices.Add(new BtDeviceInfo()
                    {
                        Name = device.Name,
                        Address = device.Address
                    });
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PairedDevices"));
                ScanNextDeviceServices();
            }
            else
            {
                this.PairedDevices = new List<BtDeviceInfo>();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PairedDevices"));
            }
        }

        /// <summary>
        /// Discovers advertised bluetooth services on a device at a time
        /// </summary>
        private void ScanNextDeviceServices()
        {
            var deviceToScan = this.PairedDevices.FirstOrDefault(d => !d.ServicesDiscovered);

            if (deviceToScan == null)
            {
                IsBusy = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBusy"));
                return;
            }

            var actualDevice = BluetoothAdapter.DefaultAdapter.BondedDevices
                .FirstOrDefault(d => d.Address.Equals(deviceToScan.Address));

            if (actualDevice == null)
            {
                IsBusy = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBusy"));
                return;
            }

            var uuids = actualDevice.GetUuids();

            if (uuids != null)
            {
                deviceToScan.HasRequiredServiceID = uuids.Any(p => p.ToString().Equals(BtDeviceInfo.RequiredServiceID));
            }

            if (deviceToScan.HasRequiredServiceID)
            {
                IsBusy = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBusy"));
                return;
            }
            else
            {
                actualDevice.FetchUuidsWithSdp();
            }
        }
    }
}