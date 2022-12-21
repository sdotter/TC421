namespace TC421
{
    using System;
    using System.CommandLine;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

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

        public Main() { }

        public static bool KeepGoing = true;

        public bool CreateEmptyModel(string? filename = null)
        {
            ModelItem modelItem = new ModelItem()
            {
                ModelItemName = "ModelName"
            };

            this.ProjectItem.ModelSet.Add(modelItem.ModelItemName, modelItem);
            this.ModelItem = modelItem;

            return this.ProjectItem.Save(filename);
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

        public async Task<int> DoWork(string[] args)
        {
            KeepGoing = !args.Contains("--help");

            var rootCommand = new RootCommand("Reverse engineering for TC421 (TimeControl) for controlling your TC421 led controller remotely");
            var generateOption = new Option<FileInfo?>("--generate", description: "Generate empty model file (as template).", getDefaultValue: () => null);
            var uploadOption = new Option<string>("--upload", description: "Upload model to TC421 controller.");
            var sync = new Command("sync", description: "Synchronize time.");
            var macAddr = new Command("mac", description: "Get MAC from device.");

            rootCommand.AddOption(generateOption);
            rootCommand.Add(sync);
            rootCommand.Add(macAddr);
            rootCommand.AddOption(uploadOption);

            sync.SetHandler(async () =>
            {
                await this.SearchWifiDevice();
                await this.SendWifi(Ins.TIME_SYNCHRONIZATION).ContinueWith(val => {
                    Console.WriteLine("Synchronizing time finished!");
                });
                KeepGoing = false;
            });

            macAddr.SetHandler(async () =>
            {
                Console.Write("Get MAC from device: ");
                await this.SearchWifiDevice();
                Console.Write(ThisDeviceMac);
                KeepGoing = false;
            });

            rootCommand.SetHandler((uploadOptionValue, generateOptionValue) => {
                if (generateOptionValue != null)
                {
                    if (CreateEmptyModel(generateOptionValue.FullName))
                        Console.Write($"Created empty model: {generateOptionValue.FullName}");

                    KeepGoing = false;
                }
                else if (uploadOptionValue != string.Empty) this.ProjectItem = this.ProjectItem.Open(args[2]);
            }, uploadOption, generateOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
