namespace TC421
{
    using System;
    using System.Text;
    public class ProtocolWifi
    {
        private static int MaxBuff = 64;

        public static byte[] SearchDevice()
        {
            byte[] numArray = new byte[ProtocolWifi.MaxBuff];
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
            byte[] numArray = ProtocolUsb.TimeSynchronization();
            numArray[ProtocolWifi.MaxBuff - 3] = numArray[ProtocolWifi.MaxBuff - 1];
            numArray[ProtocolWifi.MaxBuff - 1] = (byte)0;
            ProtocolWifi.GetDeviceMacBytes().CopyTo((Array)numArray, ProtocolWifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] ClearAllModel()
        {
            byte[] numArray = ProtocolUsb.ClearAllModel();
            numArray[ProtocolWifi.MaxBuff - 3] = numArray[ProtocolWifi.MaxBuff - 1];
            numArray[ProtocolWifi.MaxBuff - 1] = (byte)0;
            ProtocolWifi.GetDeviceMacBytes().CopyTo((Array)numArray, ProtocolWifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] ReadyPlayModel(string ModelName)
        {
            byte[] numArray = ProtocolUsb.ReadyPlayModel(ModelName);
            numArray[ProtocolWifi.MaxBuff - 3] = numArray[ProtocolWifi.MaxBuff - 1];
            numArray[ProtocolWifi.MaxBuff - 1] = (byte)0;
            ProtocolWifi.GetDeviceMacBytes().CopyTo((Array)numArray, ProtocolWifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] PlayModelValues(byte[] value)
        {
            byte[] numArray = ProtocolUsb.PlayModelValues(value);
            numArray[ProtocolWifi.MaxBuff - 3] = numArray[ProtocolWifi.MaxBuff - 1];
            numArray[ProtocolWifi.MaxBuff - 1] = (byte)0;
            ProtocolWifi.GetDeviceMacBytes().CopyTo((Array)numArray, ProtocolWifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] EndTransmission()
        {
            byte[] numArray = ProtocolUsb.EndTransmission();
            numArray[ProtocolWifi.MaxBuff - 3] = numArray[ProtocolWifi.MaxBuff - 1];
            numArray[ProtocolWifi.MaxBuff - 1] = (byte)0;
            ProtocolWifi.GetDeviceMacBytes().CopyTo((Array)numArray, ProtocolWifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] ReadyLoadModel(string ModelName, int ModelIndex)
        {
            byte[] numArray = ProtocolUsb.ReadyLoadModel(ModelName, ModelIndex);
            numArray[ProtocolWifi.MaxBuff - 3] = numArray[ProtocolWifi.MaxBuff - 1];
            numArray[ProtocolWifi.MaxBuff - 1] = (byte)0;
            ProtocolWifi.GetDeviceMacBytes().CopyTo((Array)numArray, ProtocolWifi.MaxBuff - 15);
            return numArray;
        }

        public static byte[] LoadModelValue(byte[] value)
        {
            byte[] numArray = ProtocolUsb.LoadModelValue(value);
            numArray[ProtocolWifi.MaxBuff - 3] = numArray[ProtocolWifi.MaxBuff - 1];
            numArray[ProtocolWifi.MaxBuff - 1] = (byte)0;
            ProtocolWifi.GetDeviceMacBytes().CopyTo((Array)numArray, ProtocolWifi.MaxBuff - 15);
            return numArray;
        }

        private static byte[] GetDeviceMacBytes() => Encoding.Default.GetBytes(UdpHelper.Instance.DeviceMac);
    }
}
