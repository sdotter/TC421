namespace TC421
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Runtime.Remoting.Messaging;

    public partial class frmMain : Form
    {
        private static string ThisDeviceMac = string.Empty;

        private delegate byte[] DeleteSearch(byte[] data);
        private delegate byte[] DelegateSendResult(byte[] data);
        private delegate void DelegatePlay();
        private delegate void DelegateLoadModel();

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

        private void SearchWifiDevice() => new DeleteSearch(UdpHelper.Instance.SendBroadcast).BeginInvoke(ProtocolWifi.SearchDevice(), new AsyncCallback(this.CallBackSearch), (object)null);
        
        private void CallBackSearch(IAsyncResult result)
        {
            byte[] sourceArray = ((DeleteSearch)((AsyncResult)result).AsyncDelegate).EndInvoke(result);
            if (sourceArray != null)
            {
                if (sourceArray[0] != (byte)136 || sourceArray[2] != (byte)23)
                    return;

                byte[] numArray = new byte[12];
                Array.Copy((Array)sourceArray, 4, (Array)numArray, 0, 12);
                string macStr = Encoding.Default.GetString(numArray);

                this.Invoke(new ThreadStart(() =>
                {
                    ThisDeviceMac = macStr;
                    UdpHelper.Instance.DeviceMac = macStr;
                    this.listBox1.Items.Add(string.Format("MAC ADDRESS: {0}", (object)macStr));
                }));
            }
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
        
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.SearchWifiDevice();

            Task.Run(() => {
                Thread.Sleep(1000);
                this.SendWifi(Ins.TIME_SYNCHRONIZATION);
            });
            
            this.ProjectItem = this.ProjectItem.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ModelItem modelItem1 = new ModelItem()
            {
                ModelItemName = "TestModelFilename.txt"
            };

            this.ProjectItem.ModelSet.Add(modelItem1.ModelItemName, modelItem1);
            this.ProjectItem.Save();
            this.ModelItem = modelItem1;
        }

        private void CallbackReceive(IAsyncResult result)
        {
            byte[] numArray = ((DelegateSendResult)((AsyncResult)result).AsyncDelegate).EndInvoke(result);
            object asyncState = result.AsyncState;

            if (asyncState == null)
                return;

            object obj = asyncState;
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

        public void SendWifi(Ins ins, object sender = null)
        {
            switch (ins)
            {
                case Ins.TIME_SYNCHRONIZATION:
                    new DelegateSendResult(UdpHelper.Instance.SendBroadcast).BeginInvoke(ProtocolWifi.TimeSynchronization(), new AsyncCallback(this.CallbackReceive), (object)Ins.TIME_SYNCHRONIZATION);
                    break;
                case Ins.CLEAR_DEVICE_MODEL:
                    new DelegateSendResult(UdpHelper.Instance.SendBroadcast).BeginInvoke(ProtocolWifi.ClearAllModel(), new AsyncCallback(this.CallbackReceive), (object)Ins.CLEAR_DEVICE_MODEL);
                    break;
                case Ins.PLAY_MODEL_NAME:
                    //UdpHelper.Instance.SendBroadcastNone(ProtocolWifi.ReadyPlayModel(this.ModelList.SelectedItem.Text));
                    break;
                case Ins.PLAY_MODEL_VALUE:
                    UdpHelper.Instance.SendBroadcastNone(ProtocolWifi.PlayModelValues(sender as byte[]));
                    break;
                case Ins.END_TRANSMISSION:
                    new DelegateSendResult(UdpHelper.Instance.SendBroadcast).BeginInvoke(ProtocolWifi.EndTransmission(), new AsyncCallback(this.CallbackReceive), (object)Ins.END_TRANSMISSION);
                    break;
                case Ins.LOAD_MODEL_NAME:
                    //new DelegateSendResult(UdpHelper.Instance.SendBroadcast).BeginInvoke(ProtocolWifi.ReadyLoadModel(this.ModelList.SelectedItem.Text, this.ModelList.Items.FindIndex((Predicate<DSkinDynamicListBoxItem>)(m => m == this.ModelList.SelectedItem))), new AsyncCallback(this.CallbackReceive), (object)Ins.LOAD_MODEL_NAME);
                    break;
                case Ins.LOAD_MODEL_VALUE:
                    new DelegateSendResult(UdpHelper.Instance.SendBroadcast).BeginInvoke(ProtocolWifi.LoadModelValue(sender as byte[]), new AsyncCallback(this.CallbackReceive), (object)Ins.LOAD_MODEL_VALUE);
                    break;
            }
        }
    }
}
