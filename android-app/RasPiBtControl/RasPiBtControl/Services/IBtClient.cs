using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasPiBtControl.Services
{
    public interface IBtClient
    {
        bool IsConnected { get; }
        bool Connect(string deviceAddress);
        void Disconnect();
        void SendData(string data);
        event EventHandler<string> ReceivedData;
    }
}
