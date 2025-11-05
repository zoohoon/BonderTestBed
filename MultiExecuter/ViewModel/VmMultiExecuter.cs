
using CommunicationModule;
using LogModule;
using MultiExecuter.Model;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace MultiExecuter.ViewModel
{
    using MultiLauncherLoaderProxy;
    using CallbackServices;
    public class ProcessCreator
    {

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateProcess(
            string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateProcThreadAttribute(
            IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue,
            IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InitializeProcThreadAttributeList(
            IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        public static bool CreateProcess(int parentProcessId, string exename, string path, string cmdline, out int pid)
        {
            const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
            const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;

            var pInfo = new PROCESS_INFORMATION();
            var sInfoEx = new STARTUPINFOEX();
            sInfoEx.StartupInfo.cb = Marshal.SizeOf(sInfoEx);
            IntPtr lpValue = IntPtr.Zero;

            try
            {
                if (parentProcessId > 0)
                {
                    var lpSize = IntPtr.Zero;
                    var success = InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
                    if (success || lpSize == IntPtr.Zero)
                    {
                        pid = -1;
                        return false;
                    }

                    sInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                    success = InitializeProcThreadAttributeList(sInfoEx.lpAttributeList, 1, 0, ref lpSize);
                    if (!success)
                    {
                        pid = -1;
                        return false;
                    }

                    var parentHandle = Process.GetProcessById(parentProcessId).Handle;
                    // This value should persist until the attribute list is destroyed using the DeleteProcThreadAttributeList function
                    lpValue = Marshal.AllocHGlobal(IntPtr.Size);
                    Marshal.WriteIntPtr(lpValue, parentHandle);

                    success = UpdateProcThreadAttribute(
                        sInfoEx.lpAttributeList,
                        0,
                        (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS,
                        lpValue,
                        (IntPtr)IntPtr.Size,
                        IntPtr.Zero,
                        IntPtr.Zero);
                    if (!success)
                    {
                        pid = -1;
                        return false;
                    }
                }

                var pSec = new SECURITY_ATTRIBUTES();
                var tSec = new SECURITY_ATTRIBUTES();
                pSec.nLength = Marshal.SizeOf(pSec);
                tSec.nLength = Marshal.SizeOf(tSec);
                var lpApplicationName = path;
                pid = -1;
                return CreateProcess(lpApplicationName, cmdline, ref pSec, ref tSec, false, EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, null, ref sInfoEx, out pInfo);
            }
            finally
            {
                // Free the attribute list
                if (sInfoEx.lpAttributeList != IntPtr.Zero)
                {
                    DeleteProcThreadAttributeList(sInfoEx.lpAttributeList);
                    Marshal.FreeHGlobal(sInfoEx.lpAttributeList);
                }
                Marshal.FreeHGlobal(lpValue);
                pid = pInfo.dwProcessId;
                // Close process and thread handles
                if (pInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hProcess);
                }
                if (pInfo.hThread != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hThread);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class VmMultiExecuter : IMultiExecuter, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private static EndPoint localEP;
        private static EndPoint remoteEP;
        private static Socket udpSocket;



        public IMultiExecuterCallback ServiceCallBack { get; private set; }


        private readonly string ReadyTitle = "POSIsReady";
        //private readonly string MainTitle = "POS";

        private MultiExecuterParam _Param;
        public MultiExecuterParam Param
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    NotifyPropertyChanged(nameof(Param));
                }
            }
        }

        private DiskItem _DItem;
        public DiskItem DItem
        {
            get { return _DItem; }
            set
            {
                if (value != _DItem)
                {
                    _DItem = value;
                    NotifyPropertyChanged(nameof(DItem));
                }
            }
        }

        private ServiceHost _ServiceHostOverTCP;

        public ServiceHost ServiceHostOverTCP
        {
            get { return _ServiceHostOverTCP; }
            set { _ServiceHostOverTCP = value; }
        }



        private string _CellNo;
        public string CellNo
        {
            get { return _CellNo; }
            set
            {
                if (value != _CellNo)
                {
                    _CellNo = value;
                    NotifyPropertyChanged(nameof(CellNo));
                }
            }
        }


        private int _SelectIdx;
        public int SelectIdx
        {
            get { return _SelectIdx; }
            set
            {
                if (value != _SelectIdx)
                {
                    _SelectIdx = value;
                    NotifyPropertyChanged(nameof(SelectIdx));
                }
            }
        }



        private int _TabIdx;
        public int TabIdx
        {
            get { return _TabIdx; }
            set
            {
                if (value != _TabIdx)
                {
                    _TabIdx = value;
                    NotifyPropertyChanged(nameof(TabIdx));

                }


            }
        }


        private int MonitoringInterValInms = 100;

        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        System.Timers.Timer _monitoringTimer;

        bool bStopUpdateThread;
        //Thread UpdateThread;



        private void _monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                areUpdateEvent.Set();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public void _DriveUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                UpdateDiskInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private void InitServiceDrive()
        {
            try
            {
                DItem = new DiskItem();
                DItem.DiskItemCollection = new ObservableCollection<DiskItem>();

                UpdateDiskInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        //public event System.ComponentModel.CancelEventHandler Closing;

        string filepath = @"C:\ProberSystem";
        string filrname = "MultiLauncherConfig";
        string JsonFullPath = string.Empty;
        public VmMultiExecuter()
        {
            try
            {
                JsonFullPath = $"{filepath}\\{filrname}.json";

                LoadParam();

                FindLauncherIndex();

                MakePOSProcessList();

                CheckAvaUDPComm();

                InitServiceHosts(Param.IP, Param.Port);

                ConnectLoader();

                //DiskThread = new Thread(new ThreadStart(UpdateDiskThread));
                //DiskThread.Start();

                _monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                _monitoringTimer.Start();

                Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);

                bStopUpdateThread = false;

                InitServiceDrive();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //UpdateThread = new Thread(new ThreadStart(UpdateProc));
            //UpdateThread.Name = this.GetType().Name;
            //UpdateThread.Start();
            //ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));

            // startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            //startWatch.Start();

            //ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));

            //stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
            //stopWatch.Start();

            // Console.WriteLine("Press any key to exit");

            // while (!Console.KeyAvailable) System.Threading.Thread.Sleep(50);

            //startWatch.Stop();
            //stopWatch.Stop();

        }

        public void CheckAvaUDPComm()
        {
            try
            {

                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                localEP = new IPEndPoint(IPAddress.Any, 13000);
                remoteEP = new IPEndPoint(IPAddress.Broadcast, 13000);

                udpSocket.Bind(localEP);//소켓에 주소를 할당

                byte[] buffer = new byte[512];
                udpSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(MessageCallback), buffer);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                if (udpSocket != null)
                {
                    udpSocket.Close();
                }
            }
        }

        private void MessageCallback(IAsyncResult aResult)
        {
            try
            {
                byte[] tmpReceiveData;
                tmpReceiveData = new byte[512];
                tmpReceiveData = (byte[])aResult.AsyncState;

                if (tmpReceiveData[0] != 0)
                {
                    int size = udpSocket.EndReceiveFrom(aResult, ref remoteEP);
                    string receiveMessage;
                    byte[] sendData = new byte[512];
                    string[] splitMessage;

                    if (size > 0)
                    {

                        byte[] receiveData = new byte[512];
                        receiveData = (byte[])aResult.AsyncState;
                        receiveMessage = Encoding.UTF8.GetString(receiveData, 0, size);
                        splitMessage = receiveMessage.Split(new char[] { '_' });
                        if (splitMessage[0] == "POSIsReady")
                        {
                            foreach (var item in Param.ExecuteItemCollection)
                            {
                                if (item.CellNum.ToString() == splitMessage[1])
                                {
                                    item.Accessible = true;
                                    item.CellPrevState = item.CellState;
                                    item.CellState = EnumCellProcessState.Running;
                                }
                            }
                        }
                    }
                    byte[] buffer = new byte[512];
                    udpSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(MessageCallback), buffer);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        bool loaderconnect = false;
        [DllImport("user32.dll")]
        public static extern int SetWindowText(IntPtr hWnd, string text);
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("If MultiExecuter is terminated, the communication connection with Loader will be disconnected. \rDo you still want to quit MultiExecuter?", "Warning",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                // message box에서 cancel을 누른 경우에는 종료하지 않고, ok를 누른 경우에만 아래 로직을 타서 종료하도록 처리함.
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                loaderconnect = false;
                bStopUpdateThread = false;

                if(_DriveUpdateTimer != null)
                {
                    _DriveUpdateTimer = null;
                }

                if (ServiceHostOverTCP != null)
                {
                    if (ServiceCallBack != null)
                    {
                        ServiceCallBack.DisConnect(first_cellnum);
                    }

                    if (loaderproxy != null)
                    {
                        loaderproxy.DeInitService();
                    }

                    ServiceCallBack = null;

                    if (ServiceHostOverTCP.State != CommunicationState.Faulted)
                    {
                        try
                        {
                            ServiceHostOverTCP.Close();
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            ServiceHostOverTCP.Abort();
                        }
                    }
                }

                if (udpSocket != null)
                {
                    udpSocket.Close();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public string GetLocalIPAddress()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        IPInterfaceProperties adapterProperties = ni.GetIPProperties();
                        GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;

                        //foreach (GatewayIPAddressInformation address in addresses)
                        //{
                        if (addresses.Count != 0)
                        {
                            foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    return ip.Address.ToString();
                                }
                            }
                        }
                        //}

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        static void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Process stopped: {0}", e.NewEvent.Properties["ProcessName"].Value);
        }

        static void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Process started: {0}", e.NewEvent.Properties["ProcessName"].Value);
        }

        int first_cellnum;
        public void FindLauncherIndex()
        {
            try
            {
                first_cellnum = Param.ExecuteItemCollection[0].CellNum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LoadParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            //string FullPath = ParamPath;

            try
            {
                //var JsonFullPath = Path.ChangeExtension(FullPath, ".json");

                bool deserializeResult = false;
                object deserializeObj = null;

                if (Directory.Exists(Path.GetDirectoryName(JsonFullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(JsonFullPath));
                }

                bool existfile = false;

                existfile = File.Exists(JsonFullPath);


                if (existfile == false)
                {
                    Param = new MultiExecuterParam();
                    Param.ExecuteItemCollection = new ObservableCollection<ExecuteItem>();
                    Param.SetDefaultParam();

                    //Param.ExePath = @"C:\Users\Alvin\source\repos\NewRepo2\bin\x64\Debug\ProberSystem.exe";
                    //Param.ExePath = System.Environment.CurrentDirectory + @"\ProberSystem.exe";

                    SaveParam(null);
                }


                else
                {
                    deserializeResult = SerializeManager.Deserialize(JsonFullPath, out deserializeObj, typeof(MultiExecuterParam));
                    RetVal = deserializeResult ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        Param = deserializeObj as MultiExecuterParam;
                        SaveParam(null);

                    }
                }

                //IParam tmpParam = null;
                //RetVal = Extensions_IParam.LoadParameter(null, ref tmpParam, typeof(MultiExecuterParam), null, FullPath);
                //if (RetVal == EventCodeEnum.NONE)
                //{
                //    Param = tmpParam as MultiExecuterParam;
                //}

                //foreach (var v in Param.ExecuteItemCollection)
                //{
                //    v.RequestSave += SaveParam;
                //}
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _KillCommand;
        public ICommand KillCommand
        {
            get
            {
                if (null == _KillCommand) _KillCommand = new AsyncCommand(KillCommandFunc);
                return _KillCommand;
            }
        }

        private Task KillCommandFunc()
        {
            try
            {
                // ISSD-2039 [MICRON] 안전한 MULTI EXECUTER 사용 #Usability
                // MultiExcuter에서 kill 버튼을 누르면, 경고 문구와 함께 종료할 것인지를 묻는 message box를 띄우도록 처리함.
                var result = MessageBox.Show("If the cell is currently running, killing the cell can be dangerous.\rDo you still want to kill the cell?", "Warning",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                // message box에서 cancel을 누른 경우에는 종료하지 않고, ok를 누른 경우에만 아래 로직을 타서 종료하도록 처리함.
                if (result == MessageBoxResult.Cancel)
                {
                    return Task.CompletedTask;
                }

                foreach (var pos in Param.ExecuteItemCollection)
                {
                    //Close 해당 구성 요소에 연결된 리소스를 모두 해제합니다.
                    //Dispose(Boolean) 이 프로세스에서 사용하는 리소스를 모두 해제합니다.
                    //CloseMainWindow 주 창에 닫기 메시지를 보내 사용자 인터페이스가 있는 프로세스를 닫습니다.
                    //Kill 연결된 프로세스를 즉시 중지합니다.
                    //Refresh 	프로세스 구성 요소 내에 캐시되어 있는 연결된 프로세스 정보를 모두 삭제합니다.

                    if (pos.process != null & pos.IsChecked & pos.Accessible == true)
                    {
                        pos.IsConnected = false;
                        pos.Accessible = false;
                        pos.process.CloseMainWindow();
                        pos.process.Kill();
                        pos.process.Close();
                        pos.process.Refresh();
                        pos.process = null;


                    }

                    //pos.process.Kill();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private RelayCommand<object> _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (null == _SaveCommand) _SaveCommand = new RelayCommand<object>(SaveParam);
                return _SaveCommand;
            }
        }
        private void SaveParam(object param)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                if (Directory.Exists(Path.GetDirectoryName(JsonFullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(JsonFullPath));
                }

                bool serializeResult = false;
                serializeResult = SerializeManager.Serialize(JsonFullPath, Param);
                RetVal = serializeResult ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;

                if (timerconnect == true) //Param revised
                {
                    _DriveUpdateTimer.Stop();
                    UpdateDiskInfo();
                    _DriveUpdateTimer = new System.Timers.Timer(Param.Time * 60000);
                    _DriveUpdateTimer.Elapsed += _DriveUpdateTimer_Elapsed;
                    _DriveUpdateTimer.Start();
                }

                //SaveElementParmeter(param);

                //    RetVal = Extensions_IParam.SaveParameter(null, Param, null, FullPath);
                //    if (RetVal != EventCodeEnum.NONE)
                //    {
                //        throw new Exception($"[{this.GetType().Name} - SaveAutoTiltSysFile] Faile SaveParameter");
                //    }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _AddExecuteItemCommand;
        public ICommand AddExecuteItemCommand
        {
            get
            {

                if (null == _AddExecuteItemCommand) _AddExecuteItemCommand = new RelayCommand<object>(AddExecuteItem);


                return _AddExecuteItemCommand;
            }

        }


        public void AddExecuteItem(object obj)
        {
            try
            {
                int cno = int.Parse(CellNo);

                if ((cno > 0) &&
                    (cno <= 12) && TabIdx == 0
                    )
                {
                    ExecuteItem addItem = new ExecuteItem(cno);
                    //addItem.RequestSave += SaveParam;
                    Param.ExecuteItemCollection.Add(addItem);

                }


            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }

        public async Task GetDiskInfo()
        {
            try
            {
                await Task.Run(() => UpdateDiskInfo());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        string PC_Name = Environment.MachineName;
        bool timerconnect = false;
        private object querylock = new object();
        public void UpdateDiskInfo()
        {
            try
            {
                lock (querylock)
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    
                    foreach (DriveInfo d in allDrives)
                    {
                        if (d.IsReady == true && d.DriveType == DriveType.Fixed)
                        {
                            int AvailableSpace = (int)(d.TotalFreeSpace / 1024 / 1024 / 1000);
                            int TotalSpace = (int)(((d.TotalSize) / 1024) / 1024) / 1000; //전체 용량
                            int UsageSpace = (int)(((d.TotalSize - d.TotalFreeSpace) / 1024) / 1024) / 1000; //사용된 용량
                            int Percent = (int)((float)((float)UsageSpace / (float)TotalSpace) * 100);

                            var cur_drive = DItem.DiskItemCollection.Where(item => item.DriveName == d.Name).FirstOrDefault();

                            if (cur_drive != null)
                            {
                                cur_drive.UsageSpace = UsageSpace;
                                cur_drive.AvailableSpace = AvailableSpace;
                                cur_drive.TotalSpace = TotalSpace;
                                cur_drive.Percent = Percent;
                            }
                            else
                            {
                                DiskItem drive = new DiskItem(name: d.Name, label: d.VolumeLabel, uspace: UsageSpace, aspace: AvailableSpace, tspace: TotalSpace, percent: Percent);
                                DItem.DiskItemCollection.Add(drive);
                            }
                            
                            if (Percent >= Param.Limit)
                            {
                                if (ServiceCallBack != null)
                                {
                                    try
                                    {
                                        ServiceCallBack.MessageSend(first_cellnum, PC_Name, d.Name);
                                        timerconnect = true;
                                    }
                                    catch (Exception err)
                                    {
                                        ServiceCallBack = null;
                                        LoggerManager.Exception(err);
                                    }

                                }
                            }

                            if (ServiceCallBack != null)
                            {
                                try
                                {
                                    ServiceCallBack.InfoSend(first_cellnum, PC_Name, d.Name, UsageSpace.ToString(), AvailableSpace.ToString(), TotalSpace.ToString(), Percent.ToString());
                                }
                                catch (Exception err)
                                {
                                    ServiceCallBack = null;
                                    LoggerManager.Exception(err);
                                }
                            }
                            else if (ServiceCallBack == null)
                            {
                                timerconnect = false;
                                if (_DriveUpdateTimer != null)
                                {
                                    _DriveUpdateTimer.Stop();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _RemoveAllCommand;
        public ICommand RemoveAllCommand
        {
            get
            {
                if (null == _RemoveAllCommand) _RemoveAllCommand = new RelayCommand<object>(RemoveAllCommandFunc);
                return _RemoveAllCommand;
            }
        }

        public void RemoveAllCommandFunc(object obj)
        {
            try
            {
                Param.ExecuteItemCollection.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _RemoveExecuteItemCommand;
        public ICommand RemoveExecuteItemCommand
        {
            get
            {
                if (null == _RemoveExecuteItemCommand) _RemoveExecuteItemCommand = new RelayCommand<object>(RemoveExecuteItem);
                return _RemoveExecuteItemCommand;
            }
        }

        public void RemoveExecuteItem(object obj)
        {
            try
            {

                foreach (var item in Param.ExecuteItemCollection)
                {
                    if (item.IsChecked == true)
                    {
                        Param.ExecuteItemCollection.Remove(item);
                    }
                }
                SelectIdx = -1;

                //Param.ExecuteItemCollection[SelectIdx].RequestSave -= SaveParam;
                //SaveParam();


            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _StartProcessCommand;
        public ICommand StartProcessCommand
        {
            get
            {
                if (null == _StartProcessCommand) _StartProcessCommand = new AsyncCommand(StartProcess);
                return _StartProcessCommand;
            }
        }

        //public List<Process> processes = new List<Process>();

        private string ProcessName = "ProberSystem";

        public string GetWindowTitle(int processId)
        {
            Process process = Process.GetProcesses().ToList<Process>().Find(proc => proc.Id == processId);
            if (process != null)
                return Process.GetProcessById(processId).MainWindowTitle;
            else
                return null;
        }





        private void UpdateProc()
        {
            try
            {
                while (bStopUpdateThread == true)
                {
                    try
                    {
                        //foreach (var item in Param.ExecuteItemCollection)
                        //{
                        //if (item.process != null)
                        //{
                        //    string tmpName = GetWindowTitle(item.process.Id);

                        //    string subTitle = "POS [";

                        //    if (tmpName != null)
                        //    {
                        //        if (tmpName == ReadyTitle + $"{item.CellNum}" || tmpName.Contains(subTitle))
                        //        {
                        //            item.Accessible = true;
                        //            item.CellPrevState = item.CellState;
                        //            item.CellState = EnumCellProcessState.Running;
                        //        }
                        //        else
                        //        {
                        //            item.Accessible = false;
                        //            item.CellPrevState = item.CellState;
                        //            item.CellState = EnumCellProcessState.UNKNOWN;
                        //            if (tmpName == null)
                        //            {
                        //                item.IsConnected = false;
                        //                item.process.Close();
                        //                item.process = null;

                        //            }

                        //        }
                        //    }
                        //    else
                        //    {
                        //        item.Accessible = false;
                        //        item.CellPrevState = item.CellState;
                        //        item.CellState = EnumCellProcessState.UNKNOWN;
                        //        if (tmpName == null)
                        //        {
                        //            item.IsConnected = false;
                        //            item.process.Close();
                        //            item.process = null;

                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    item.Accessible = false;
                        //    item.CellPrevState = item.CellState;
                        //    item.CellState = EnumCellProcessState.UNKNOWN;

                        //}
                        //}

                        // Get a list of running processes
                        //Process[] processlist = Process.GetProcesses();

                        Process[] POSlist = Process.GetProcessesByName(ProcessName);

                        //if (POSlist.Count() > 0)
                        //{
                        foreach (var pos in POSlist)
                        {
                            ExecuteItem cell = null;
                            if (Param.ExecuteItemCollection.Where(x => x.process != null).Count() == Param.ExecuteItemCollection.Count())
                            {
                                cell = Param.ExecuteItemCollection.Where(x => x.process.Id == pos.Id).FirstOrDefault();
                                if (cell != null)
                                {
                                    cell.ConnectCheck = true;
                                }
                            }
                            else
                            {
                                cell = Param.ExecuteItemCollection.Where(x => x.process != null).Where(x => x.process.Id == pos.Id).FirstOrDefault();
                                if (cell != null)
                                {
                                    cell.ConnectCheck = true;
                                }
                            }

                            if (cell != null)
                            {
                                cell.IsConnected = true;
                                cell.CellPrevState = cell.CellState;
                                cell.CellState = EnumCellProcessState.Running;
                            }
                        }

                        foreach (var item in Param.ExecuteItemCollection)
                        {
                            if (!item.ConnectCheck)
                            {
                                item.Accessible = false;
                                item.IsConnected = false;
                                item.CellPrevState = item.CellState;
                                item.CellState = EnumCellProcessState.UNKNOWN;
                            }
                            item.ConnectCheck = false;
                        }
                        //}
                        //else
                        //{
                        //    foreach (var item in Param.ExecuteItemCollection)
                        //    {
                        //        item.IsConnected = false;
                        //        item.CellPrevState = item.CellState;
                        //        item.CellState = EnumCellProcessState.UNKNOWN;
                        //    }
                        //}
                        //_monitoringTimer.Interval = 100;
                        //areUpdateEvent.WaitOne(1000);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private void MakePOSProcessList_Disk()
        //{
        //    var list = Process.GetProcesses();
        //    foreach (var item in list)
        //    {
        //        for (int i=0; i<Param.ExecuteItemCollection_Disk.Count; i++)
        //        {
        //            if (item.MainWindowTitle == "POSIsReady" + $"{Param.ExecuteItemCollection_Disk[i]}")
        //            {
        //                if (Param.ExecuteItemCollection_Disk[i].process == null)
        //                {
        //                    Param.ExecuteItemCollection_Disk[i].process = new Process();
        //                    Param.ExecuteItemCollection_Disk[i].process = item;

        //                }
        //            }
        //        }
        //    }
        //}

        private void MakePOSProcessList()
        {
            try
            {
                var list = Process.GetProcesses();
                foreach (var item in list)
                {

                    for (int i = 0; i < Param.ExecuteItemCollection.Count; i++)
                    {
                        if (item.MainWindowTitle == "POSIsReady" + $"{Param.ExecuteItemCollection[i].CellNum}")
                        {
                            if (Param.ExecuteItemCollection[i].process == null)
                            {
                                Param.ExecuteItemCollection[i].process = new Process();
                                Param.ExecuteItemCollection[i].process = item;
                                Param.ExecuteItemCollection[i].CellPrevState = Param.ExecuteItemCollection[i].CellState;
                                Param.ExecuteItemCollection[i].CellState = EnumCellProcessState.Running;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// 만약 Parameter를 잘 못가져오면 FileManager.xml 을 확인/삭제해보자.
        /// 프로그램이 실행이 안되면 MultiExecuter.exe와 ProberSystem.exe가 같은 경로인지 확인해보자.
        /// </summary>
        /// <param name="obj"></param>
        public Task StartProcess()
        {
            try
            {
                foreach (var v in Param.ExecuteItemCollection)
                {
                    if (v.IsChecked == true)
                    {
                        if (v.Accessible == false)
                        {
                            v.process = null;
                        }

                        if (v.process == null)
                        {
                            //v.process = new Process();
                            //v.process.StartInfo = new ProcessStartInfo(Param.ExePath, $"[path]{v.Path}");
                            //v.process.Start();
                            //ProcessStartInfo psi = new ProcessStartInfo();
                            //psi.FileName = Param.ExePath;
                            //psi.Arguments = $"[path]{v.Path}";
                            //v.process = Process.Start(psi);
                            var myId = Process.GetCurrentProcess().Id;
                            int pidNum;
                            ProcessCreator.CreateProcess(myId, ReadyTitle + $"{v.CellNum}", Param.ExePath, $"[path]{v.Path}", out pidNum);

                            bool complete = false;
                            while (true)
                            {
                                var list = Process.GetProcesses();
                                var cellProcesses = list.Where(x => x.ProcessName == "ProberSystem");
                                foreach (var cellproc in cellProcesses)
                                {
                                    //delays.DelayFor(30);
                                    System.Threading.Thread.Sleep(30);
                                    if (cellproc.Id == pidNum)
                                    {
                                        v.process = cellproc;
                                        complete = true;
                                        break;
                                    }
                                }
                                //foreach (var item in list)
                                //{
                                //    delays.DelayFor(30);
                                //    if (item.MainWindowTitle == "POSIsReady" + $"{v.CellNum}")
                                //    {
                                //        v.process = item;
                                //        complete = true;
                                //        break;
                                //    }
                                //}
                                if (complete)
                                {
                                    System.Threading.Thread.Sleep(5000);
                                    break;
                                }
                                //delays.DelayFor(100);
                                System.Threading.Thread.Sleep(100);
                                //string tmpName = GetWindowTitle(23388);
                                //SetWindowText(Cell.process.MainWindowHandle, "POSIsReady" + $"{Cell.CellNum}");
                                //if (tmpName == ReadyTitle + $"{Cell.CellNum}")
                                //  break;
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private RelayCommand<object> _FindPathCommand;
        public ICommand FindPathCommand
        {
            get
            {
                if (null == _FindPathCommand) _FindPathCommand = new RelayCommand<object>(FindPath);
                return _FindPathCommand;
            }
        }


        private RelayCommand<object> _FindPathCommand_Disk;
        public ICommand FindPathCommand_Disk
        {
            get
            {
                if (null == _FindPathCommand_Disk) _FindPathCommand_Disk = new RelayCommand<object>(FindPath);
                return _FindPathCommand_Disk;
            }
        }


        private void FindPath(object obj)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.OpenFileDialog() { Filter = "EXE files (*.exe)|*.exe" })
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (dialog.FileName != "")
                    {

                        Param.ExePath = dialog.FileName;

                    }
                }
                SaveParam(null);
                //SaveParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitServiceHosts(string ip, int port)
        {
            string baseURI = $"net.tcp://{ip}";
            ServiceHostOverTCP = new ServiceHost(this, new Uri[] { new Uri($"{baseURI}:{port}/POS") });

            try
            {
                NetTcpBinding tcpBinding = new NetTcpBinding()
                {
                    OpenTimeout = new TimeSpan(0, 3, 0),
                    CloseTimeout = new TimeSpan(0, 3, 0),
                    SendTimeout = new TimeSpan(0, 3, 30),
                    ReceiveTimeout = TimeSpan.MaxValue,

                    ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = false }
                };
                tcpBinding.Security.Mode = SecurityMode.None;
                ServiceHostOverTCP.AddServiceEndpoint(
                    typeof(IMultiExecuter),
                    tcpBinding,
                    "MultiExecuterService");
                ServiceHostOverTCP.Open();
                ServiceHostOverTCP.Faulted += Host_Faulted;
                ServiceHostOverTCP.Closed += Host_Closed;

                Debug.Print("Service started. Available in following endpoints");
                foreach (var serviceEndpoint in ServiceHostOverTCP.Description.Endpoints)
                {
                    Debug.Print($"Service End point :{serviceEndpoint.ListenUri.AbsoluteUri}");
                }
            }
            catch (Exception err)
            {

                Debug.Print($"InitServiceHost() Error occurred. Err = {err.Message}");
            }
        }
        private void Host_Faulted(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug($"VmMultiExecuter.Host_Faulted(): Service Channel Faulted.");
                (ServiceHostOverTCP as ICommunicationObject).Open();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }

        private void Host_Closed(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug($"VmMultiExecuter.Host_Closed(): Service Channel Closed.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void StartCell(int cellNum)
        {
            try
            {
                var Cell = Param.ExecuteItemCollection.Where(x => x.CellNum == cellNum).FirstOrDefault();

                bool cellprocessRunning = false;
                if (Cell != null)
                {
                    if (Cell.process != null)
                    {
                        var list = Process.GetProcesses();
                        foreach (var item in list)
                        {

                            if (Cell.process.MainWindowTitle == item.MainWindowTitle)
                            {
                                cellprocessRunning = true;
                                break;
                            }
                        }
                    }
                    if (cellprocessRunning == false)
                    {
                        if (Cell.process != null)
                        {
                            Cell.process.Refresh();
                            Cell.process = null;
                        }
                    }
                    if (Cell.process == null)
                    {
                        //ProcessStartInfo psi = new ProcessStartInfo();
                        //psi.FileName = Param.ExePath;
                        //psi.Arguments = $"[path]{Cell.Path}";
                        //Cell.process = Process.Start(psi);
                        var myId = Process.GetCurrentProcess().Id;
                        int pidNum;
                        ProcessCreator.CreateProcess(myId, ReadyTitle + $"{Cell.CellNum}", Param.ExePath, $"[path]{Cell.Path}", out pidNum);

                        //ProcessStartInfo psi = new ProcessStartInfo();
                        //psi.FileName = Param.ExePath;
                        //psi.Arguments = $"[path]{Cell.Path}";
                        //psi.UseShellExecute = false;
                        //Cell.process = Process.Start(psi);
                        bool complete = false;
                        while (true)
                        {
                            var list = Process.GetProcesses();
                            foreach (var item in list)
                            {
                                if (item.MainWindowTitle == "POSIsReady" + $"{Cell.CellNum}")
                                {
                                    Cell.process = item;
                                    complete = true;
                                    break;
                                }
                            }
                            if (complete)
                            {
                                break;
                            }

                            //string tmpName = GetWindowTitle(23388);
                            //SetWindowText(Cell.process.MainWindowHandle, "POSIsReady" + $"{Cell.CellNum}");
                            //if (tmpName == ReadyTitle + $"{Cell.CellNum}")
                            //  break;
                        }

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void StartCellStageList(List<int> stageindex)
        {
            try
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < stageindex.Count; i++)
                    {
                        var Cell = Param.ExecuteItemCollection.Where(x => x.CellNum == stageindex[i]).FirstOrDefault();

                        bool cellprocessRunning = false;
                        if (Cell != null)
                        {
                            if (Cell.process != null)
                            {
                                var list = Process.GetProcesses();
                                foreach (var item in list)
                                {

                                    if (Cell.process.MainWindowTitle == item.MainWindowTitle)
                                    {
                                        cellprocessRunning = true;
                                        break;
                                    }
                                }
                            }
                            if (cellprocessRunning == false)
                            {
                                if (Cell.process != null)
                                {
                                    Cell.process.Refresh();
                                    Cell.process = null;
                                }
                            }
                            if (Cell.process == null)
                            {
                                //ProcessStartInfo psi = new ProcessStartInfo();
                                //psi.FileName = Param.ExePath;
                                //psi.Arguments = $"[path]{Cell.Path}";
                                //Cell.process = Process.Start(psi);
                                var myId = Process.GetCurrentProcess().Id;
                                int pidNum;
                                ProcessCreator.CreateProcess(myId, ReadyTitle + $"{Cell.CellNum}", Param.ExePath, $"[path]{Cell.Path}", out pidNum);

                                //ProcessStartInfo psi = new ProcessStartInfo();
                                //psi.FileName = Param.ExePath;
                                //psi.Arguments = $"[path]{Cell.Path}";
                                //psi.UseShellExecute = false;
                                //Cell.process = Process.Start(psi);
                                bool complete = false;
                                while (true)
                                {
                                    var list = Process.GetProcesses();
                                    var cellProcesses = list.Where(x => x.ProcessName == "ProberSystem");
                                    foreach (var cellproc in cellProcesses)
                                    {
                                        //delays.DelayFor(30);
                                        Thread.Sleep(30);
                                        if (cellproc.Id == pidNum)
                                        {
                                            Cell.process = cellproc;
                                            complete = true;
                                            break;
                                        }
                                    }
                                    if (complete)
                                    {
                                        System.Threading.Thread.Sleep(5000);
                                        break;
                                    }
                                    //delays.DelayFor(100);
                                    Thread.Sleep(100);
                                    //string tmpName = GetWindowTitle(23388);
                                    //SetWindowText(Cell.process.MainWindowHandle, "POSIsReady" + $"{Cell.CellNum}");
                                    //if (tmpName == ReadyTitle + $"{Cell.CellNum}")
                                    //  break;
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
        }

        public void ExitCell(int cellNum)
        {
            try
            {
                var Cell = Param.ExecuteItemCollection.Where(x => x.CellNum == cellNum).FirstOrDefault();

                if (Cell != null)
                {
                    if (Cell.process != null)
                    {
                        Cell.process.CloseMainWindow();
                        Cell.process.Kill();
                        Cell.process.Close();
                        Cell.process.Refresh();
                        Cell.process = null;

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void StartAllCells()
        {
            try
            {
                foreach (var v in Param.ExecuteItemCollection)
                {
                    v.process.Start();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
        }

        public bool GetCellConnectedInfo(int cellNum)
        {
            try
            {
                var Cell = Param.ExecuteItemCollection.Where(x => x.CellNum == cellNum).FirstOrDefault();

                if (Cell != null)
                {
                    return Cell.IsConnected;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
            
        }

        public bool GetCellAccessible(int cellNum)
        {
            try
            {
                var Cell = Param.ExecuteItemCollection.Where(x => x.CellNum == cellNum).FirstOrDefault();

                if (Cell != null)
                {
                    return Cell.Accessible;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }


        private void Channel_Faulted(object sender, EventArgs e)
        {
            try
            {
                ServiceCallBack = null;
                loaderconnect = false;
                timerconnect = false;
                bStopUpdateThread = false;

                if (loaderproxy != null)
                {
                    loaderproxy.DeInitService();
                }

                LoggerManager.Error($"LoaderCommunication Client Channel faulted.");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumCellProcessState GetCellState(int cellNum)
        {
            EnumCellProcessState retstate = EnumCellProcessState.UNKNOWN;
            try
            {
                var Cell = Param.ExecuteItemCollection.Where(x => x.CellNum == cellNum).FirstOrDefault();
                if (Cell != null)
                {
                    retstate = Cell.CellState;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retstate;
        }

        public MultiLauncherLoaderProxy loaderproxy { get; set; }
        private MultiExecuterLoaderCallbackService multiExecuterLoaderCallbackService { get; set; }
        public void ConnectLoader()
        {
            try
            {

                if (Param.LIP != null)
                {
                    multiExecuterLoaderCallbackService = new MultiExecuterLoaderCallbackService(this);
                    var context = multiExecuterLoaderCallbackService.GetInstanceContext();
                    loaderproxy = new MultiLauncherLoaderProxy(ServicePort.MultiLauncherControlServicePort, context, Param.LIP);

                    if (CommunicationManager.CheckAvailabilityCommunication(Param.LIP, ServicePort.MultiLauncherControlServicePort)) //Loader ON
                    {
                        loaderconnect = true;
                        loaderproxy.LoaderInitService(first_cellnum);
                    }

                    //Loader OFF
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void DisConnectLoader()
        {
            try
            {
                ServiceCallBack = null;
                loaderconnect = false;
                timerconnect = false;
                bStopUpdateThread = false;

                if (loaderproxy != null)
                {
                    loaderproxy.DeInitService();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private System.Timers.Timer _DriveUpdateTimer;
        public Task InitService()
        {
            try
            {
                OperationContext.Current.Channel.Faulted += Channel_Faulted;
                ServiceCallBack = OperationContext.Current.GetCallbackChannel<IMultiExecuterCallback>();
                LoggerManager.Debug($"launcher service initialized.");

                if (loaderconnect == false)
                {
                    ConnectLoader();
                }

                if (timerconnect == false)
                {
                    _DriveUpdateTimer = new System.Timers.Timer(Param.Time * 60000);
                    _DriveUpdateTimer.Elapsed += _DriveUpdateTimer_Elapsed;
                    _DriveUpdateTimer.Start();
                }

            }
            catch (Exception err)
            {
                Debug.Print($"InitService() Error occurred. Err = {err.Message}");
            }
            return Task.CompletedTask;
        }

    }
}

