namespace TC421
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    public class UdpHelper
    {
        private UdpClient _UdpClient;
        private string _DeviceMac;
        private static UdpHelper _UdpHelper;
        private IPEndPoint _EndPoint;

        public static UdpHelper Instance => UdpHelper._UdpHelper ?? (UdpHelper._UdpHelper = new UdpHelper());

        public string DeviceMac
        {
            get => this._DeviceMac;
            set => this._DeviceMac = value;
        }

        public UdpClient UdpClient
        {
            get => this._UdpClient;
            private set => this._UdpClient = value;
        }

        public IPEndPoint EndPoint
        {
            get => this._EndPoint;
            set => this._EndPoint = value;
        }

        private UdpHelper()
        {
            try
            {
                this.UdpClient = new UdpClient(5000, AddressFamily.InterNetwork);
            }
            catch
            {
                try
                {
                    this.UdpClient = new UdpClient(8000, AddressFamily.InterNetwork);
                }
                catch
                {
                }
            }
        }

        public void Send(byte[] data) => this.UdpClient.Send(data, data.Length, this._EndPoint);

        public byte[] SendBroadcast(byte[] data)
        {
            this.UdpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 5000));
            byte[] numArray;
            do
            {
                IAsyncResult asyncResult = this.UdpClient.BeginReceive((AsyncCallback)null, (object)null);
                int num = 0;
                while (!asyncResult.IsCompleted && num++ < 5)
                    Thread.Sleep(100);
                if (asyncResult.IsCompleted)
                    numArray = this.UdpClient.EndReceive(asyncResult, ref this._EndPoint);
                else
                    return (byte[])null;
            }
            while (numArray[0] != (byte)136 || numArray[1] != (byte)0);
            return numArray;
        }

        public void SendBroadcastNone(byte[] data) => this.UdpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 5000));
    }
}
