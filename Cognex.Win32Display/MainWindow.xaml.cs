
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Interop;

namespace Cognex.Win32Display
{
    //using LogModule;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IntPtr HostHandle { get; set; }
        private const int _WM_COPYDATA = 0x4A;

        //==> [NOT USE] : Cognex Module Display, Online, Live Mode 설정 등을 제어
        private InsightDisplayController _InsightDisplayControler;

        //==> Module 과 통신하기 위한 Controller, Key 는 IP다.
        private Dictionary<String, InsightNativeController> _InsightNativeControlleres;
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                this.Loaded += MainWindow_Loaded;
                InitControl();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        public void InitControl()
        {
            try
            {
                _InsightDisplayControler = new InsightDisplayController();
                _InsightNativeControlleres = new Dictionary<string, InsightNativeController>();

                WindowsFormsHost winFormHost = new WindowsFormsHost();
                winFormHost.Child = _InsightDisplayControler.CvsDisplay;
                CvsGrid.Children.Add(winFormHost);
            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != _WM_COPYDATA)
                return IntPtr.Zero;
            try
            {

                try
                {
                    COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                    String[] split = cds.lpData.Split(';');

                    COPYDATASTRUCT_KEYS key;
                    if (Enum.TryParse<COPYDATASTRUCT_KEYS>(split[0], out key) == false)
                        return IntPtr.Zero;

                    String ip;
                    bool connected;
                    String val = split[1];
                    String[] valSplit;
                    switch (key)
                    {
                        case COPYDATASTRUCT_KEYS.Setup:
                            SendMessage(cds.dwData, COPYDATASTRUCT_KEYS.Setup, "OK");
                            break;
                        case COPYDATASTRUCT_KEYS.ConnectDisplay:
                            //==> admin정보까지 같이 얻어오고 싶다면 Parsing 처리로 해야 할 듯
                            ip = val;
                            connected = _InsightDisplayControler.Connect(ip, "admin", "");
                            SendMessage(cds.dwData, COPYDATASTRUCT_KEYS.ConnectDisplay, connected.ToString());
                            break;
                        case COPYDATASTRUCT_KEYS.ConnectNative://==> 연결 함
                            ip = val;
                            if (_InsightNativeControlleres.ContainsKey(ip))
                            {
                                connected = true;
                            }
                            else
                            {
                                InsightNativeController insightNativeController = new InsightNativeController();
                                connected = insightNativeController.Connect(ip, "admin", "");
                                if (connected)
                                {
                                    _InsightNativeControlleres.Add(ip, insightNativeController);
                                }
                            }
                            SendMessage(cds.dwData, COPYDATASTRUCT_KEYS.ConnectNative, connected.ToString());//==> 연결이 되었다고 응답을 해줌.
                            break;
                        case COPYDATASTRUCT_KEYS.DisconnectDisplay:
                            _InsightDisplayControler.DisConnect();
                            break;
                        case COPYDATASTRUCT_KEYS.DisconnectNative:
                            foreach (var item in _InsightNativeControlleres)//==> 모든 모듈의 연결을 끊는다.
                                item.Value.DisConnect();
                            break;
                        case COPYDATASTRUCT_KEYS.Online:
                            _InsightDisplayControler.SwitchOnlineMode();
                            break;
                        case COPYDATASTRUCT_KEYS.Live:
                            _InsightDisplayControler.SwitchLiveMode();
                            break;
                        case COPYDATASTRUCT_KEYS.Graphics:
                            bool flag = Convert.ToBoolean(val);
                            _InsightDisplayControler.SwitchGraphics(flag);

                            break;
                        case COPYDATASTRUCT_KEYS.SpreadSheet:
                            _InsightDisplayControler.SwitchSpreadSheet();
                            break;
                        case COPYDATASTRUCT_KEYS.NativeCommand://==> Native Command를 수행함.
                            String reponse = String.Empty;
                            valSplit = val.Split('/');
                            if (valSplit.Length != 2)
                            {
                                SendMessage(cds.dwData, COPYDATASTRUCT_KEYS.NativeCommand, reponse);
                                break;
                            }
                            String command = valSplit[0];//==> Command
                            ip = valSplit[1];//==> IP
                            if (_InsightNativeControlleres.ContainsKey(ip))
                                reponse = _InsightNativeControlleres[ip].SendCommand(command);//==> Cognex Module에 Command를 수행
                            SendMessage(cds.dwData, COPYDATASTRUCT_KEYS.NativeCommand, reponse);//==> 수행 결과를 응답으로 보냄
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                handled = true;

            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
            return IntPtr.Zero;
        }
        //==> ProberSystem.exe에 Message를 보내는 함수
        private void SendMessage(IntPtr handle, COPYDATASTRUCT_KEYS key, string data)
        {
            try
            {
                string msgData = string.Format("{0};{1}", key, data);

                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                cds.cbData = Encoding.Default.GetByteCount(msgData) + 1;
                cds.lpData = msgData;

                SendMessage(handle, _WM_COPYDATA, 0, ref cds);
            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        private void Test()
        {
            try
            {
                System.Drawing.Bitmap bitmap = _InsightDisplayControler.CvsDisplay.GetBitmap();
                IntPtr hBitmap = bitmap.GetHbitmap();

                System.Windows.Media.Imaging.BitmapImage bitmapImage = (System.Windows.Media.Imaging.BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(hBitmap);
                //image.Source = bitmapImage;
            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, ref COPYDATASTRUCT lParam);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DeleteObject(IntPtr hObject);
    }
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }
}
