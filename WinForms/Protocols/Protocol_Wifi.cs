namespace TC421
{
    using System;
    using System.Text;
    public class Protocol_Wifi
    {
        private static int MaxBuff = 64;

        public static byte[] SearchDevice()
        {
            byte[] numArray = new byte[Protocol_Wifi.MaxBuff];
            numArray[0] = (byte)85;
            numArray[1] = (byte)170;
            numArray[2] = (byte)23;
            numArray[5] = (byte)3;
            numArray[6] = (byte)221;
            numArray[7] = (byte)238;
            numArray[8] = byte.MaxValue;
            numArray[61] = (byte)227;
            return numArray;
        }

        public static byte[] TimeSynchronization()
        {
            byte[] numArray = Protocol_Usb.TimeSynchronization();
            numArray[Protocol_Wifi.MaxBuff - 3] = numArray[Protocol_Wifi.MaxBuff - 1];
            numArray[Protocol_Wifi.MaxBuff - 1] = (byte)0;
            Protocol_Wifi.GetDeviceMacBytes().CopyTo((Array)numArray, Protocol_Wifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] ClearAllModel()
        {
            byte[] numArray = Protocol_Usb.ClearAllModel();
            numArray[Protocol_Wifi.MaxBuff - 3] = numArray[Protocol_Wifi.MaxBuff - 1];
            numArray[Protocol_Wifi.MaxBuff - 1] = (byte)0;
            Protocol_Wifi.GetDeviceMacBytes().CopyTo((Array)numArray, Protocol_Wifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] ReadyPlayModel(string ModelName)
        {
            byte[] numArray = Protocol_Usb.ReadyPlayModel(ModelName);
            numArray[Protocol_Wifi.MaxBuff - 3] = numArray[Protocol_Wifi.MaxBuff - 1];
            numArray[Protocol_Wifi.MaxBuff - 1] = (byte)0;
            Protocol_Wifi.GetDeviceMacBytes().CopyTo((Array)numArray, Protocol_Wifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] PlayModelValues(byte[] value)
        {
            byte[] numArray = Protocol_Usb.PlayModelValues(value);
            numArray[Protocol_Wifi.MaxBuff - 3] = numArray[Protocol_Wifi.MaxBuff - 1];
            numArray[Protocol_Wifi.MaxBuff - 1] = (byte)0;
            Protocol_Wifi.GetDeviceMacBytes().CopyTo((Array)numArray, Protocol_Wifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] EndTransmission()
        {
            byte[] numArray = Protocol_Usb.EndTransmission();
            numArray[Protocol_Wifi.MaxBuff - 3] = numArray[Protocol_Wifi.MaxBuff - 1];
            numArray[Protocol_Wifi.MaxBuff - 1] = (byte)0;
            Protocol_Wifi.GetDeviceMacBytes().CopyTo((Array)numArray, Protocol_Wifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] ReadyLoadModel(string ModelName, int ModelIndex)
        {
            byte[] numArray = Protocol_Usb.ReadyLoadModel(ModelName, ModelIndex);
            numArray[Protocol_Wifi.MaxBuff - 3] = numArray[Protocol_Wifi.MaxBuff - 1];
            numArray[Protocol_Wifi.MaxBuff - 1] = (byte)0;
            Protocol_Wifi.GetDeviceMacBytes().CopyTo((Array)numArray, Protocol_Wifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] LoadModelValue(byte[] value)
        {
            byte[] numArray = Protocol_Usb.LoadModelValue(value);
            numArray[Protocol_Wifi.MaxBuff - 3] = numArray[Protocol_Wifi.MaxBuff - 1];
            numArray[Protocol_Wifi.MaxBuff - 1] = (byte)0;
            Protocol_Wifi.GetDeviceMacBytes().CopyTo((Array)numArray, Protocol_Wifi.MaxBuff - 15);
            return numArray;
        }

        private static byte[] GetDeviceMacBytes() => Encoding.Default.GetBytes(UdpHelper.Instance.DeviceMac);
    }
}
