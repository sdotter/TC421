namespace TC421
{
    using System;
    using System.CommandLine;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using TC421.Classes;

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
        private bool IsStopLoad;

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

        private async Task CallLoadModel(object obj)
        {
            //((FrmMain.DelegateLoadModel)((AsyncResult)result).AsyncDelegate).EndInvoke(result);
            this.LoadCurrentState = RetureDataStaus.RETURE_DATA_NONE;
            this.IsStopLoad = false;
            Thread.Sleep(1000);
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
                    await Task.Run(() => UdpHelper.Instance.SendBroadcast(ProtocolWifi.ReadyLoadModel(this.ProjectItem.ProjectName, 0))).ContinueWith(val => this.CallbackReceive(val.Result as byte[], (object)Ins.LOAD_MODEL_NAME));
                    break;
                case Ins.LOAD_MODEL_VALUE:
                    await Task.Run(() => UdpHelper.Instance.SendBroadcast(ProtocolWifi.LoadModelValue(sender as byte[]))).ContinueWith(val => this.CallbackReceive(val.Result as byte[], (object)Ins.LOAD_MODEL_VALUE));
                    break;
            }
        }

        private void SendUsbData(Ins ins, byte[] value = null)
        {
            int num;
            Task.Run(() => num = 6).Start();
        }

        public async Task WifiLoadModel()
        {
            Console.Write("Uploading: ");
            using (var progress = new ProgressBar())
            {
                this.SendWifi(Ins.LOAD_MODEL_NAME);
                this.LoadCurrentState = RetureDataStaus.RETURE_DATA_START;
                bool flag1 = false;
                int num1 = 0;
                byte[] sender = new byte[8];
                while (!flag1)
                {
                    if (this.LoadCurrentState == RetureDataStaus.RETURE_DATA_START)
                    {
                        this.LoadCurrentState = RetureDataStaus.RETURE_DATA_SUCCESS;
                        for (int index1 = 0; index1 < 48; ++index1)
                        {
                            if (this.IsStopLoad)
                                return;
                            bool flag2 = false;
                            bool flag3 = true;
                            int num2 = 0;
                            while (!flag2)
                            {
                                if (this.LoadCurrentState == RetureDataStaus.RETURE_DATA_SUCCESS || this.LoadCurrentState == RetureDataStaus.RETURE_DATA_FAIL)
                                {
                                    if (this.LoadCurrentState == RetureDataStaus.RETURE_DATA_FAIL)
                                    {
                                        if (num2++ > 20)
                                        {
                                            this.SendWifi(Ins.END_TRANSMISSION);
                                            this.IsStopLoad = true;
                                            return;
                                        }
                                        if (flag3)
                                        {
                                            int num3;
                                            if (index1 <= 0)
                                            {
                                                num3 = index1;
                                            }
                                            else
                                            {
                                                int num4 = num3 = index1 - 1;
                                            }
                                            index1 = num3;
                                        }
                                        flag3 = false;
                                    }
                                    int num5 = 6 * index1 + 1;
                                    int num6 = 0;
                                    byte[] numArray1 = sender;
                                    int index2 = num6;
                                    int num7 = index2 + 1;
                                    int num8 = (int)Convert.ToByte(index1);
                                    numArray1[index2] = (byte)num8;
                                    byte[] numArray2 = sender;
                                    int index3 = num7;
                                    int num9 = index3 + 1;
                                    int num10 = (int)Convert.ToByte(index1 / 2);
                                    numArray2[index3] = (byte)num10;
                                    byte[] numArray3 = sender;
                                    int index4 = num9;
                                    int num11 = index4 + 1;
                                    int num12 = (int)Convert.ToByte(index1 % 2 == 0 ? 0 : 30);
                                    numArray3[index4] = (byte)num12;
                                    byte[] numArray4 = sender;
                                    int index5 = num11;
                                    int num13 = index5 + 1;
                                    List<object> modelValues1 = this.ModelItem.ModelValues;
                                    int index6 = num5;
                                    int num14 = index6 + 1;
                                    int num15 = (int)Convert.ToByte(modelValues1[index6]);
                                    numArray4[index5] = (byte)num15;
                                    byte[] numArray5 = sender;
                                    int index7 = num13;
                                    int num16 = index7 + 1;
                                    List<object> modelValues2 = this.ModelItem.ModelValues;
                                    int index8 = num14;
                                    int num17 = index8 + 1;
                                    int num18 = (int)Convert.ToByte(modelValues2[index8]);
                                    numArray5[index7] = (byte)num18;
                                    byte[] numArray6 = sender;
                                    int index9 = num16;
                                    int num19 = index9 + 1;
                                    List<object> modelValues3 = this.ModelItem.ModelValues;
                                    int index10 = num17;
                                    int num20 = index10 + 1;
                                    int num21 = (int)Convert.ToByte(modelValues3[index10]);
                                    numArray6[index9] = (byte)num21;
                                    byte[] numArray7 = sender;
                                    int index11 = num19;
                                    int num22 = index11 + 1;
                                    List<object> modelValues4 = this.ModelItem.ModelValues;
                                    int index12 = num20;
                                    int index13 = index12 + 1;
                                    int num23 = (int)Convert.ToByte(modelValues4[index12]);
                                    numArray7[index11] = (byte)num23;
                                    byte[] numArray8 = sender;
                                    int index14 = num22;
                                    int num24 = index14 + 1;
                                    int num25 = (int)Convert.ToByte(this.ModelItem.ModelValues[index13]);
                                    numArray8[index14] = (byte)num25;

                                    progress.Report((double)(index1 + 1) / 48);
                                    this.SendWifi(Ins.LOAD_MODEL_VALUE, (object)sender);
                                    if (this.LoadCurrentState == RetureDataStaus.RETURE_DATA_SUCCESS)
                                    {
                                        flag2 = true;
                                        flag3 = true;
                                    }
                                    this.LoadCurrentState = RetureDataStaus.RETURE_DATA_WAIT;
                                }
                                if (num2++ > 20)
                                {
                                    this.SendWifi(Ins.END_TRANSMISSION);
                                    this.IsStopLoad = true;
                                    return;
                                }
                                Thread.Sleep(100);
                            }
                        }
                        this.SendWifi(Ins.END_TRANSMISSION);
                        flag1 = true;
                    }
                    if (num1++ > 10)
                    {
                        flag1 = true;
                        this.SendUsbData(Ins.END_TRANSMISSION);
                    }
                    Thread.Sleep(500);
                }
                Console.WriteLine($"\nUploading: Complete!");
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
                Console.Write($"{ThisDeviceMac}\n");
                KeepGoing = false;
            });

            rootCommand.SetHandler(async (uploadOptionValue, generateOptionValue) => {
                if (generateOptionValue != null)
                {
                    if (CreateEmptyModel(generateOptionValue.FullName))
                        Console.Write($"Created empty model: {generateOptionValue.FullName}\n");

                    KeepGoing = false;
                }
                else if (uploadOptionValue != string.Empty)
                {
                    this.ProjectItem = this.ProjectItem.Open(args[1]);
                    this.ModelItem = this.ProjectItem.ModelSet.ToList()[0].Value;
                    await this.SearchWifiDevice();
                    await this.WifiLoadModel().ContinueWith(async val => {
                        await this.CallLoadModel(val);
                        KeepGoing = false;
                    });
                }
            }, uploadOption, generateOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
