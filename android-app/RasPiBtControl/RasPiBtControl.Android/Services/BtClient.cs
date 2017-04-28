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
using System.IO;
using Android.Bluetooth;
using RasPiBtControl.Services;
using RasPiBtControl.Model;

namespace RasPiBtControl.Droid.Services
{
    /// <summary>
    /// Bluetooth client
    /// </summary>
    public class BtClient : IBtClient
    {
        private MainActivity _mainActivity;
        private BluetoothDevice btDevice;

        public bool IsConnected => btDevice != null;

        public BtClient(MainActivity mainActivity)
        {
            this._mainActivity = mainActivity;
        }

        /// <summary>
        /// Connects to a bluetooth device
        /// </summary>
        /// <param name="deviceAddress">MAC address</param>
        /// <returns></returns>
        public bool Connect(string deviceAddress)
        {
            btDevice = BluetoothAdapter.DefaultAdapter.BondedDevices.FirstOrDefault(d => d.Address.Equals(deviceAddress));

            return btDevice != null;
        }

        /// <summary>
        /// Disconnects from the device
        /// </summary>
        public void Disconnect()
        {
            btDevice = null;
        }

        /// <summary>
        /// Sends data to the currently connected device
        /// </summary>
        /// <param name="data">Data to send</param>
        public void SendData(string data)
        {
            if (btDevice == null)
                return;

            try
            {
                // Create a RFCOMM socket and connect
                var socket = btDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString(BtDeviceInfo.RequiredServiceID));
                socket.Connect();

                // Setup receiving data before sending any data to the server
                var streamAndData = new StreamAndData(socket.InputStream);
                streamAndData.stream.BeginRead(streamAndData.data, 0, streamAndData.data.Length, new AsyncCallback((ar) =>
                {
                    var asyncState = (StreamAndData)ar.AsyncState;
                    var bytesRead = streamAndData.stream.EndRead(ar);
                    var incomingData = Encoding.ASCII.GetString(streamAndData.data, 0, bytesRead);

                    ReceivedData?.Invoke(this, incomingData);

                }), streamAndData);

                // Send the data
                using (var sw = new StreamWriter(socket.OutputStream))
                {
                    sw.Write(data);
                }
            }
            catch (Exception ex)
            {
                ReceivedData?.Invoke(this, $"msg:{ex.ToString()}");
            }
        }

        // Event triggered when we received data from the server
        public event EventHandler<string> ReceivedData;

        // Class used to hold the stream and the data read from it
        public class StreamAndData
        {
            public Stream stream;
            public byte[] data;

            public StreamAndData(Stream stream)
            {
                this.stream = stream;
                this.data = new byte[1024];
            }
        }
    }
}