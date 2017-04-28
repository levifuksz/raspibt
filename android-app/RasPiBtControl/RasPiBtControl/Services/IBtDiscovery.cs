using RasPiBtControl.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasPiBtControl.Services
{
    public interface IBtDiscovery : INotifyPropertyChanged
    {
        bool IsBusy { get; }
        List<BtDeviceInfo> PairedDevices { get; }

        void Refresh();
    }
}
