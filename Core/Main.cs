namespace TC421
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    internal class Main
    {
        private static string ThisDeviceMac = string.Empty;

        public enum Ins
        {
            SEARCH_DEVICE,
            TIME_SYNCHRONIZATION,
            CLEAR_DEVICE_MODEL,
            PLAY_MODEL_NAME,
            PLAY_MODEL_VALUE,
            END_TRANSMISSION,
            LOAD_MODEL_NAME,
            LOAD_MODEL_VALUE,
        }

        public enum RetureDataStaus
        {
            RETURE_DATA_SUCCESS,
            RETURE_DATA_FAIL,
            RETURE_DATA_WAIT,
            RETURE_DATA_NONE,
            RETURE_DATA_START,
        }

        private RetureDataStaus PlayCurrentState = RetureDataStaus.RETURE_DATA_NONE;
        private RetureDataStaus LoadCurrentState = RetureDataStaus.RETURE_DATA_NONE;

        private async Task SearchWifiDevice()
        {
            var searchDeviceTask = Task.Run(() => UdpHelper.Instance.SendBroadcast(ProtocolWifi.SearchDevice()));
            var callbackTask = searchDeviceTask.ContinueWith(val =>
            {
                byte[] sourceArray = val.Result as byte[];
                if (sourceArray != null)
                {
                    if (sourceArray[0] != (byte)136 || sourceArray[2] != (byte)23)
                        return;

                    byte[] numArray = new byte[12];
                    Array.Copy((Array)sourceArray, 4, (Array)numArray, 0, 12);
                    string macStr = Encoding.Default.GetString(numArray);

                    ThisDeviceMac = macStr;
                    UdpHelper.Instance.DeviceMac = macStr;
                }
            });

            await searchDeviceTask;
            await callbackTask;
        }

        private ProjectItem _ProjectItem;
        public ProjectItem ProjectItem
        {
            get => this._ProjectItem ?? (this._ProjectItem = ProjectItem.Instance);
            set => this._ProjectItem = value;
        }

        private ModelItem _ModelItem;
        public ModelItem ModelItem
        {
            set => this._ModelItem = value;
            get => this._ModelItem;
        }

        public void Play()
        {
            this.SendWifi(Ins.PLAY_MODEL_NAME);
            Thread.Sleep(100);

            int[,] ChannelValue = new int[3, 5];
            byte[] sender = new byte[5];

            for (int n = 0; n < 48; n++)
            {
                int dn1 = 6 * n + 1;
                int index1 = 6 * (n + 1) + 1;

                ChannelValue[0, 0] = ChannelValue[2, 0] = (int)this.ModelItem.ModelValues[dn1];
                ChannelValue[0, 1] = ChannelValue[2, 1] = (int)this.ModelItem.ModelValues[dn1 + 1];
                ChannelValue[0, 2] = ChannelValue[2, 2] = (int)this.ModelItem.ModelValues[dn1 + 2];
                ChannelValue[0, 3] = ChannelValue[2, 3] = (int)this.ModelItem.ModelValues[dn1 + 3];
                ChannelValue[0, 4] = ChannelValue[2, 4] = (int)this.ModelItem.ModelValues[dn1 + 4];

            }
        }

        public bool KeepGoing { get; set; }

        public Main()
        {
            KeepGoing = true;
        }

        public void temp()
        {
            ModelItem modelItem1 = new ModelItem()
            {
                ModelItemName = "TestModelFilename.txt"
            };

            this.ProjectItem.ModelSet.Add(modelItem1.ModelItemName, modelItem1);
            this.ProjectItem.Save();
            this.ModelItem = modelItem1;
        }

        private async Task CallbackReceive(byte[] numArray, object obj)
        {
            int num = obj is Ins ? 1 : 0;
            Ins ins = num != 0 ? (Ins)obj : Ins.SEARCH_DEVICE;

            if (num == 0)
                return;

            switch (ins)
            {
                case Ins.TIME_SYNCHRONIZATION:
                    if (numArray != null && numArray[4] == (byte)133)
                    {
                        break;
                    }
                    break;
                case Ins.CLEAR_DEVICE_MODEL:
                    if (numArray != null && numArray[4] == (byte)130)
                    {
                        break;
                    }
                    break;
                case Ins.LOAD_MODEL_NAME:
                    if (numArray != null && numArray[4] == (byte)128)
                    {
                        LoadCurrentState = RetureDataStaus.RETURE_DATA_START;
                        break;
                    }
                    this.LoadCurrentState = RetureDataStaus.RETURE_DATA_FAIL;
                    break;
                case Ins.LOAD_MODEL_VALUE:
                    if (numArray != null && numArray[4] == (byte)129)
                    {
                        LoadCurrentState = RetureDataStaus.RETURE_DATA_SUCCESS;
                        break;
                    }
                    LoadCurrentState = RetureDataStaus.RETURE_DATA_FAIL;
                    break;
            }
        }


        public async Task SendWifi(Ins ins, object sender = null)
        {
            switch (ins)
            {
                case Ins.TIME_SYNCHRONIZATION:
                    await Task.Run(() => UdpHelper.Instance.SendBroadcast(ProtocolWifi.TimeSynchronization())).ContinueWith(val => this.CallbackReceive(val.Result as byte[], (object)Ins.TIME_SYNCHRONIZATION));
                    break;
                case Ins.CLEAR_DEVICE_MODEL:
                    await Task.Run(() => UdpHelper.Instance.SendBroadcast(ProtocolWifi.ClearAllModel())).ContinueWith(val => this.CallbackReceive(val.Result as byte[], (object)Ins.CLEAR_DEVICE_MODEL));
                    break;
                case Ins.PLAY_MODEL_NAME:
                    await Task.Run(() => UdpHelper.Instance.SendBroadcastNone(ProtocolWifi.ReadyPlayModel("TODO: FIX" /*this.ModelList.SelectedItem.Text*/)));
                    break;
                case Ins.PLAY_MODEL_VALUE:
                    await Task.Run(() => UdpHelper.Instance.SendBroadcastNone(ProtocolWifi.PlayModelValues(sender as byte[])));
                    break;
                case Ins.END_TRANSMISSION:
                    await Task.Run(() => UdpHelper.Instance.SendBroadcast(ProtocolWifi.EndTransmission())).ContinueWith(val => this.CallbackReceive(val.Result as byte[], (object)Ins.END_TRANSMISSION));
                    break;
                case Ins.LOAD_MODEL_NAME:
                    //new DelegateSendResult(UdpHelper.Instance.SendBroadcast).BeginInvoke(ProtocolWifi.ReadyLoadModel(this.ModelList.SelectedItem.Text, this.ModelList.Items.FindIndex((Predicate<DSkinDynamicListBoxItem>)(m => m == this.ModelList.SelectedItem))), new AsyncCallback(this.CallbackReceive), (object)Ins.LOAD_MODEL_NAME);
                    break;
                case Ins.LOAD_MODEL_VALUE:
                    await Task.Run(() => UdpHelper.Instance.SendBroadcast(ProtocolWifi.LoadModelValue(sender as byte[]))).ContinueWith(val => this.CallbackReceive(val.Result as byte[], (object)Ins.LOAD_MODEL_VALUE));
                    break;
            }
        }

        public async void DoWork()
        {
            await this.SearchWifiDevice();
            await this.SendWifi(Ins.TIME_SYNCHRONIZATION);

            Console.WriteLine(ThisDeviceMac);

            this.ProjectItem = this.ProjectItem.Open();

            while (KeepGoing)
            {  
                Thread.Sleep(100);
            }
        }
    }
}
