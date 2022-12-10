namespace TC421
{
    using System;
    using System.Text;
    public static class ProtocolUsb
    {
        private static byte Header_One = 85;
        private static byte Header_Two = 170;
        private static int MaxBuff = 64;

        public static byte[] TimeSynchronization()
        {
            int year = DateTime.Now.Year;
            int num1 = 0;
            byte[] numArray1 = new byte[ProtocolUsb.MaxBuff];
            byte num2 = 0;
            byte[] numArray2 = numArray1;
            int index1 = num1;
            int num3 = index1 + 1;
            int headerOne = (int)ProtocolUsb.Header_One;
            numArray2[index1] = (byte)headerOne;
            byte[] numArray3 = numArray1;
            int index2 = num3;
            int num4 = index2 + 1;
            int headerTwo = (int)ProtocolUsb.Header_Two;
            numArray3[index2] = (byte)headerTwo;
            byte[] numArray4 = numArray1;
            int index3 = num4;
            int num5 = index3 + 1;
            numArray4[index3] = (byte)2;
            int num6 = num5 + 1 + 1;
            byte[] numArray5 = numArray1;
            int index4 = num6;
            int num7 = index4 + 1;
            numArray5[index4] = (byte)7;
            byte[] numArray6 = numArray1;
            int index5 = num7;
            int num8 = index5 + 1;
            int num9 = (int)(byte)((int)byte.MaxValue & year >> 8);
            numArray6[index5] = (byte)num9;
            byte[] numArray7 = numArray1;
            int index6 = num8;
            int num10 = index6 + 1;
            int num11 = (int)(byte)((int)byte.MaxValue & year);
            numArray7[index6] = (byte)num11;
            byte[] numArray8 = numArray1;
            int index7 = num10;
            int num12 = index7 + 1;
            int num13 = (int)(byte)((int)byte.MaxValue & DateTime.Now.Month);
            numArray8[index7] = (byte)num13;
            byte[] numArray9 = numArray1;
            int index8 = num12;
            int num14 = index8 + 1;
            int num15 = (int)(byte)((int)byte.MaxValue & DateTime.Now.Day);
            numArray9[index8] = (byte)num15;
            byte[] numArray10 = numArray1;
            int index9 = num14;
            int num16 = index9 + 1;
            int num17 = (int)(byte)((int)byte.MaxValue & DateTime.Now.Hour);
            numArray10[index9] = (byte)num17;
            byte[] numArray11 = numArray1;
            int index10 = num16;
            int num18 = index10 + 1;
            int num19 = (int)(byte)((int)byte.MaxValue & DateTime.Now.Minute);
            numArray11[index10] = (byte)num19;
            byte[] numArray12 = numArray1;
            int index11 = num18;
            int num20 = index11 + 1;
            int num21 = (int)(byte)((int)byte.MaxValue & DateTime.Now.Second);
            numArray12[index11] = (byte)num21;
            for (int index12 = 0; index12 < 13; ++index12)
                num2 += numArray1[index12];
            byte num22 = (byte)((uint)byte.MaxValue & (uint)num2);
            numArray1[ProtocolUsb.MaxBuff - 1] = num22;
            return numArray1;
        }

        public static byte[] ClearAllModel()
        {
            byte[] numArray = new byte[ProtocolUsb.MaxBuff];
            int index1 = 0;
            int num1 = index1 + 1;
            numArray[index1] = ProtocolUsb.Header_One;
            int index2 = num1;
            int num2 = index2 + 1;
            numArray[index2] = ProtocolUsb.Header_Two;
            int index3 = num2;
            int num3 = index3 + 1;
            numArray[index3] = (byte)5;
            int index4 = num3;
            int num4 = index4 + 1;
            numArray[index4] = (byte)254;
            numArray[ProtocolUsb.MaxBuff - 1] = (byte)2;
            return numArray;
        }

        public static byte[] ReadyPlayModel(string ModelName)
        {
            byte[] numArray1 = Encoding.Default.GetBytes(ModelName).Length <= 8 ? Encoding.Default.GetBytes(ModelName) : Encoding.Default.GetBytes(ModelName.Substring(0, 4));
            byte[] numArray2 = new byte[ProtocolUsb.MaxBuff];
            int num1 = 0;
            byte[] numArray3 = numArray2;
            int index1 = num1;
            int num2 = index1 + 1;
            int headerOne = (int)ProtocolUsb.Header_One;
            numArray3[index1] = (byte)headerOne;
            byte[] numArray4 = numArray2;
            int index2 = num2;
            int num3 = index2 + 1;
            int headerTwo = (int)ProtocolUsb.Header_Two;
            numArray4[index2] = (byte)headerTwo;
            byte[] numArray5 = numArray2;
            int index3 = num3;
            int num4 = index3 + 1;
            numArray5[index3] = (byte)6;
            byte[] numArray6 = numArray2;
            int index4 = num4;
            int num5 = index4 + 1;
            numArray6[index4] = (byte)1;
            byte[] numArray7 = numArray2;
            int index5 = num5;
            int num6 = index5 + 1;
            numArray7[index5] = (byte)0;
            byte[] numArray8 = numArray2;
            int index6 = num6;
            int num7 = index6 + 1;
            int length = (int)(byte)numArray1.Length;
            numArray8[index6] = (byte)length;
            byte[] numArray9 = numArray2;
            int index7 = num7;
            int index8 = index7 + 1;
            numArray9[index7] = (byte)0;
            numArray1.CopyTo((Array)numArray2, index8);
            byte num8 = 0;
            for (int index9 = 0; index9 < index8 + numArray1.Length; ++index9)
                num8 += numArray2[index9];
            numArray2[numArray2.Length - 1] = (byte)((uint)num8 & (uint)byte.MaxValue);
            return numArray2;
        }

        public static byte[] PlayModelValues(byte[] value)
        {
            if (value == null)
                return (byte[])null;
            byte[] numArray1 = new byte[ProtocolUsb.MaxBuff];
            byte[] numArray2 = new byte[6]
            {
                (byte) 85,
                (byte) 170,
                (byte) 6,
                (byte) 2,
                (byte) 0,
                (byte) value.Length
            };
            numArray2.CopyTo((Array)numArray1, 0);
            value.CopyTo((Array)numArray1, 6);
            byte num = 0;
            for (int index = 0; index < numArray2.Length + value.Length; ++index)
                num += numArray1[index];
            numArray1[numArray1.Length - 1] = (byte)((uint)num & (uint)byte.MaxValue);
            return numArray1;
        }

        public static byte[] EndTransmission()
        {
            byte[] numArray = new byte[ProtocolUsb.MaxBuff];
            numArray[0] = ProtocolUsb.Header_One;
            numArray[1] = ProtocolUsb.Header_Two;
            numArray[2] = (byte)3;
            numArray[3] = (byte)8;
            numArray[ProtocolUsb.MaxBuff - 1] = (byte)10;
            return numArray;
        }

        public static byte[] ReadyLoadModel(string ModelName, int ModelIndex)
        {
            byte[] numArray1 = Encoding.Default.GetBytes(ModelName).Length <= 8 ? Encoding.Default.GetBytes(ModelName) : Encoding.Default.GetBytes(ModelName.Substring(0, 4));
            byte[] numArray2 = new byte[ProtocolUsb.MaxBuff];
            int num1 = 0;
            byte[] numArray3 = numArray2;
            int index1 = num1;
            int num2 = index1 + 1;
            int headerOne = (int)ProtocolUsb.Header_One;
            numArray3[index1] = (byte)headerOne;
            byte[] numArray4 = numArray2;
            int index2 = num2;
            int num3 = index2 + 1;
            int headerTwo = (int)ProtocolUsb.Header_Two;
            numArray4[index2] = (byte)headerTwo;
            byte[] numArray5 = numArray2;
            int index3 = num3;
            int num4 = index3 + 1;
            numArray5[index3] = (byte)1;
            byte[] numArray6 = numArray2;
            int index4 = num4;
            int num5 = index4 + 1;
            numArray6[index4] = (byte)1;
            byte[] numArray7 = numArray2;
            int index5 = num5;
            int num6 = index5 + 1;
            numArray7[index5] = (byte)0;
            byte[] numArray8 = numArray2;
            int index6 = num6;
            int num7 = index6 + 1;
            int length = (int)(byte)numArray1.Length;
            numArray8[index6] = (byte)length;
            byte[] numArray9 = numArray2;
            int index7 = num7;
            int index8 = index7 + 1;
            int num8 = (int)(byte)ModelIndex;
            numArray9[index7] = (byte)num8;
            numArray1.CopyTo((Array)numArray2, index8);
            int num9 = 0;
            for (int index9 = 0; index9 < index8 + numArray1.Length; ++index9)
                num9 += (int)numArray2[index9];
            numArray2[ProtocolUsb.MaxBuff - 1] = (byte)(num9 & (int)byte.MaxValue);
            return numArray2;
        }

        public static byte[] LoadModelValue(byte[] value)
        {
            byte[] numArray1 = new byte[ProtocolUsb.MaxBuff];
            int num1 = 0;
            byte[] numArray2 = numArray1;
            int index1 = num1;
            int num2 = index1 + 1;
            int headerOne = (int)ProtocolUsb.Header_One;
            numArray2[index1] = (byte)headerOne;
            byte[] numArray3 = numArray1;
            int index2 = num2;
            int num3 = index2 + 1;
            int headerTwo = (int)ProtocolUsb.Header_Two;
            numArray3[index2] = (byte)headerTwo;
            byte[] numArray4 = numArray1;
            int index3 = num3;
            int num4 = index3 + 1;
            numArray4[index3] = (byte)1;
            byte[] numArray5 = numArray1;
            int index4 = num4;
            int num5 = index4 + 1;
            numArray5[index4] = (byte)2;
            byte[] numArray6 = numArray1;
            int index5 = num5;
            int num6 = index5 + 1;
            numArray6[index5] = (byte)0;
            byte[] numArray7 = numArray1;
            int index6 = num6;
            int index7 = index6 + 1;
            numArray7[index6] = (byte)8;
            value.CopyTo((Array)numArray1, index7);
            int num7 = 0;
            for (int index8 = 0; index8 < index7 + value.Length; ++index8)
                num7 += (int)numArray1[index8];
            numArray1[ProtocolUsb.MaxBuff - 1] = (byte)(num7 & (int)byte.MaxValue);
            return numArray1;
        }
    }
}
