using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PI; // PI_GCS2 라이브러리 (스테이지 제어용 SDK)
using System.Windows.Threading;
using System.Collections.ObjectModel;
using ProberViewModel.ViewModel;
using CUI;
using System.Net.Sockets;
using System.Windows;
using System.IO;
using System.Net; // 서버 설정을 위해 추가

namespace ProberViewModel.ViewModel
{
    /// <summary>
    /// 6축 나노 스테이지 제어를 위한 ViewModel 클래스
    /// INotifyPropertyChanged를 상속받아 UI와의 데이터 바인딩을 지원함
    /// </summary>
    public class NanoStage6AxisViewModel : INotifyPropertyChanged
    {
        private int _id = -1;              // 연결된 컨트롤러의 ID (음수면 미연결)
        private bool _isReading = false;  // 현재 위치 읽기 루프 실행 여부

        // 단위 변환 상수 (Degree <-> Micro-radian)
        private const double DegToUrad = (Math.PI / 180.0) * 1000000.0;
        private const double UradToDeg = 1.0 / DegToUrad;


        private TcpListener _server; // 클라이언트 접속 대기용
        private NetworkStream _stream; // 통신 스트림 저장용
        private TcpClient _connectedClient;
        private bool _isWaitingForResponse = false; // 응답 대기 중인가?

        private ManualJogView.Jog6AxisWindow _jogWindow;
        #region 1. 속성 (Monitoring & Logs) - UI 실시간 표시 데이터

        // 현재 위치 정보 (X, Y, Z축)
        private double _currentPosX; public double CurrentPosX { get => _currentPosX; set { _currentPosX = value; OnPropertyChanged(); } }
        private double _currentPosY; public double CurrentPosY { get => _currentPosY; set { _currentPosY = value; OnPropertyChanged(); } }
        private double _currentPosZ; public double CurrentPosZ { get => _currentPosZ; set { _currentPosZ = value; OnPropertyChanged(); } }

        // 회전축 현재 위치 (Rot X, Y, Z - uRad 단위)
        private double _currentPosRotX;
        public double CurrentPosRotX { get => _currentPosRotX; set { _currentPosRotX = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentPosRotXDeg)); } }

        private double _currentPosRotY;
        public double CurrentPosRotY { get => _currentPosRotY; set { _currentPosRotY = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentPosRotYDeg)); } }

        private double _currentPosRotZ;
        public double CurrentPosRotZ { get => _currentPosRotZ; set { _currentPosRotZ = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentPosRotZDeg)); } }

        // UI 표시를 위해 uRad을 Degree로 변환한 읽기 전용 속성
        public double CurrentPosRotXDeg => CurrentPosRotX * UradToDeg;
        public double CurrentPosRotYDeg => CurrentPosRotY * UradToDeg;
        public double CurrentPosRotZDeg => CurrentPosRotZ * UradToDeg;

        private ObservableCollection<string> _logMessages6Axis = new ObservableCollection<string>();
        public ObservableCollection<string> LogMessages6Axis { get => _logMessages6Axis; set { _logMessages6Axis = value; OnPropertyChanged(); } }
        #endregion

        #region 2. 속성 (Target Position & Status) - 이동 목표값 및 상태

        // ?? [조건 일치] 선형축(1, 2, 4) 목표값은 소수점 3자리로 변경하여 정밀 매칭 및 반올림 처리
        private double _targetPosX; public double TargetPosX { get => _targetPosX; set { _targetPosX = Math.Round(value, 3); OnPropertyChanged(); } }
        private double _targetPosY; public double TargetPosY { get => _targetPosY; set { _targetPosY = Math.Round(value, 3); OnPropertyChanged(); } }
        private double _targetPosZ; public double TargetPosZ { get => _targetPosZ; set { _targetPosZ = Math.Round(value, 3); OnPropertyChanged(); } }

        // 회전축 목표값 (Degree 입력 시 자동으로 uRad 변환 - 5자리용 데이터 인터페이스)
        private double _targetPosRotXDeg; public double TargetPosRotXDeg { get => _targetPosRotXDeg; set { _targetPosRotXDeg = value; TargetPosRotX = value * DegToUrad; OnPropertyChanged(); } }
        private double _targetPosRotYDeg; public double TargetPosRotYDeg { get => _targetPosRotYDeg; set { _targetPosRotYDeg = value; TargetPosRotY = value * DegToUrad; OnPropertyChanged(); } }
        private double _targetPosRotZDeg; public double TargetPosRotZDeg { get => _targetPosRotZDeg; set { _targetPosRotZDeg = value; TargetPosRotZ = value * DegToUrad; OnPropertyChanged(); } }

        // 실제 컨트롤러에 전달될 회전축 목표값 (uRad)
        private double _targetPosRotX; public double TargetPosRotX { get => _targetPosRotX; set { _targetPosRotX = value; OnPropertyChanged(); } }
        private double _targetPosRotY; public double TargetPosRotY { get => _targetPosRotY; set { _targetPosRotY = value; OnPropertyChanged(); } }
        private double _targetPosRotZ; public double TargetPosRotZ { get => _targetPosRotZ; set { _targetPosRotZ = value; OnPropertyChanged(); } }

        // ?? [추가] 6개 축별 개별 JOG Step 데이터 저장 속성 (선형 3자리 / 회전 5자리 초기값 기본 셋팅)
        private double _jogStepX = 1.000; public double JogStepX { get => _jogStepX; set { _jogStepX = value; OnPropertyChanged(); } }
        private double _jogStepY = 1.000; public double JogStepY { get => _jogStepY; set { _jogStepY = value; OnPropertyChanged(); } }
        private double _jogStepZ = 1.000; public double JogStepZ { get => _jogStepZ; set { _jogStepZ = value; OnPropertyChanged(); } }
        private double _jogStepRotX = 0.00100; public double JogStepRotX { get => _jogStepRotX; set { _jogStepRotX = value; OnPropertyChanged(); } }
        private double _jogStepRotY = 0.00100; public double JogStepRotY { get => _jogStepRotY; set { _jogStepRotY = value; OnPropertyChanged(); } }
        private double _jogStepRotZ = 0.00100; public double JogStepRotZ { get => _jogStepRotZ; set { _jogStepRotZ = value; OnPropertyChanged(); } }

        // 연결 상태 문자열
        private string _status = "Disconnected";
        public string ConnectionStatus { get => _status; set { _status = value; OnPropertyChanged(); } }
        #endregion

        #region ?? [추가] 통신 관련 속성 및 커맨드
        // 통신 로그용 컬렉션
        private ObservableCollection<string> _commLogMessages = new ObservableCollection<string>();
        public ObservableCollection<string> CommLogMessages { get => _commLogMessages; set { _commLogMessages = value; OnPropertyChanged(); } }

        #endregion

        #region 3. 커맨드 (UI 버튼과 연결됨)
        public ICommand ConnectCommand { get; }      // 연결
        public ICommand DisconnectCommand { get; }   // 해제
        public ICommand MoveCommand { get; }         // 개별 축 이동
        public ICommand HomeCommand { get; }         // 원점 이동 (0.0 이동)
        public ICommand ErrorClearCommand { get; }   // 로그 삭제
        public ICommand TotalMoveCommand { get; }    // 6축 동시 이동
        public ICommand JogCommand { get; }         // ?? [추가] 6축 개별 조그 상대 기동 커맨드
        public ICommand RequestVisionCommand { get; }

        public ICommand JogButtonCommand { get; } // 이 명령을 Jog 버튼에 연결하세요

        // NanoStage6AxisViewModel.cs 내부
        public ICommand ManualCommand { get; }

        // 커맨드 속성 추가
        public ICommand WaferCommand { get; }
        public ICommand DieCommand { get; }

        // 뷰모델에 추가
        private bool _isSimpleMode;
        public bool IsSimpleMode
        {
            get => _isSimpleMode;
            set { _isSimpleMode = value; OnPropertyChanged(); }
        }

        #endregion

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public NanoStage6AxisViewModel()
        {
            ConnectCommand = new localRelayCommand(p => Connect());
            DisconnectCommand = new localRelayCommand(p => Disconnect());
            MoveCommand = new localRelayCommand(p => { _ = MoveAsync(p?.ToString()); });
            HomeCommand = new localRelayCommand(p => Home(p?.ToString()));
            ErrorClearCommand = new localRelayCommand(p => ErrorClear());
            TotalMoveCommand = new localRelayCommand(p => { _ = TotalMoveAsync(); });
 
            // ?? 아래와 같이 2개의 버튼 커맨드만 명확히 구분하세요
            ManualCommand = new localRelayCommand(p => OpenManualJogWindow(false)); // Full 모드
            JogButtonCommand = new localRelayCommand(p => OpenManualJogWindow(true));  // Simple 모드

            WaferCommand = new localRelayCommand(async p =>
            {
                await SendVisionCommand("WAFER");
            });

            DieCommand = new localRelayCommand(async p =>
            {
                await SendVisionCommand("DIE");
            });

            // ?? JogCommand 초기화는 딱 한 번만!
            JogCommand = new localRelayCommand(p => ExecuteJog(p?.ToString()));
            //_ = StartServerAsync(); // [추가] 서버 모드 시작
            RequestVisionCommand = new localRelayCommand(p =>
            {
                if (_server == null)
                {
                    _ = StartServerAsync();
                    AddCommLog(">>> [Server] 서버 시작됨. 비전 장비 접속 대기 중...");
                }
                else
                {
                    AddCommLog(">>> [Server] 서버가 이미 실행 중입니다.");
                }
            });
        }

        private void ExecuteJog(string parameter) // param: "1-", "1+"
        {
            if (string.IsNullOrEmpty(parameter) || parameter.Length < 2) return;

            string axis = parameter.Substring(0, 1);
            string direction = parameter.Substring(1, 1);
            double step = 0;

            // 축별 Step 매핑
            switch (axis)
            {
                case "1": step = JogStepX; break;
                case "2": step = JogStepY; break;
                case "4": step = JogStepZ; break;
                case "5": step = JogStepRotX; break;
                case "6": step = JogStepRotY; break;
                case "3": step = JogStepRotZ; break;
            }

            // JogAsync는 비동기 호출을 위한 별도 로직이므로, 
            // 실제 이동 로직은 JogAsync에 param을 넘겨서 재사용하는 것이 좋습니다.
            _ = JogAsync(parameter);
        }

        //private void OpenManualJogWindow()
        //{
        //    System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        // 6축 조그 전용 뷰 생성 (예: Jog6AxisWindow)
        //        var jogWindow = new ManualJogView.Jog6AxisWindow();

        //        // 현재 6축 뷰모델(this)을 그대로 넘겨주어 바인딩 공유
        //        jogWindow.DataContext = this;

        //        jogWindow.Owner = System.Windows.Application.Current.MainWindow;
        //        jogWindow.Show(); // 모달리스로 띄워 메인창과 동시 조작 가능
        //    });
        //}
        private void OpenManualJogWindow(bool isSimpleMode)
        {
            this.IsSimpleMode = isSimpleMode;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // 2. 창이 이미 떠 있는지 확인
                if (_jogWindow != null)
                {
                    try
                    {
                        // 떠 있다면 닫고 새로 생성 (또는 Focus만 줄 수 있음)
                        _jogWindow.Close();
                    }
                    catch { }
                }

                // 3. 창 새로 생성 및 참조 저장
                _jogWindow = new ManualJogView.Jog6AxisWindow { DataContext = this };
                _jogWindow.Owner = System.Windows.Application.Current.MainWindow;

                // 창이 닫힐 때 참조를 null로 초기화하여 메모리 관리
                _jogWindow.Closed += (s, e) => _jogWindow = null;

                _jogWindow.Show();
            });
        }

        #region ?? [추가] 통신 동작 로직
        // 💡 공통 통신 메서드 (명령어 전송 로직)
        private async Task SendVisionCommand(string cmd)
        {
            // 서버가 안 켜져 있으면 시작
            if (_server == null)
            {
                _ = StartServerAsync();
                AddCommLog(">>> [Server] 서버 시작됨. 비전 접속 대기...");
                return;
            }

            // 클라이언트 접속 여부 확인 후 전송
            if (_connectedClient != null && _connectedClient.Connected)
            {
                try
                {
                    // 💡 응답 대기 상태 시작
                    _isWaitingForResponse = true;

                    NetworkStream stream = _connectedClient.GetStream();
                    byte[] data = Encoding.ASCII.GetBytes(cmd + "\n");
                    await stream.WriteAsync(data, 0, data.Length);
                    AddCommLog($">>> Sent: {cmd}");
                }
                catch (Exception ex) { AddCommLog($"!!! 전송 실패: {ex.Message}"); }
            }
            else
            {
                AddCommLog("!!! 비전 장비가 접속되어 있지 않습니다.");
            }
        }

        public async Task StartServerAsync()
        {
            if (_server != null) return;

            _server = new TcpListener(IPAddress.Any, 5000);
            _server.Start();
            AddCommLog(">>> Server Started (5000). Waiting for client...");

            while (true)
            {
                // 접속 승인 및 클라이언트 저장
                _connectedClient = await _server.AcceptTcpClientAsync();
                AddCommLog(">>> Client Connected.");
                _ = ListenForDataAsync(_connectedClient);
            }
        }

        private async Task ListenForDataAsync(TcpClient client)
        {
            try
            {
                using (var reader = new StreamReader(client.GetStream(), Encoding.ASCII))
                {
                    while (client.Connected)
                    {
                        // 비전 장비로부터 오는 응답을 대기
                        string response = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(response)) break;

                        AddCommLog($"<<< Received Response: {response}");

                        // 💡 내가 보낸 명령에 대한 응답일 때만 파싱 수행
                        if (_isWaitingForResponse)
                        {
                            // 1. WAFER 명령에 대한 응답 (OK)
                            if (response.StartsWith("OK"))
                            {
                                // 응답이 OK인 경우 (데이터가 더 있다면 파싱)
                                string[] parts = response.Split(',');
                                if (parts.Length == 3) // 예: OK,10.123,20.456
                                {
                                    double dx = double.Parse(parts[1]);
                                    double dy = double.Parse(parts[2]);

                                    AddLog($"[Vision Result] 좌표 수신: X={dx}, Y={dy}");

                                    double currentX = CurrentPosX;
                                    double currentY = CurrentPosY;

                                    // 상대 이동거리 dx, dy를 더해 새로운 목표 좌표 계산
                                    TargetPosX = currentX + dx;
                                    TargetPosY = currentY + dy;

                                    // 💡 실시간 이동 로직
                                    // 1. 목표 위치 계산 (현재 위치 + 수신된 이동량)
                                    TargetPosX = dx;
                                    TargetPosY = dy;

                                    // 2. 비동기로 이동 커맨드 실행 (MoveAsync는 내부에서 GCS2.MOV를 호출함)
                                    _ = MoveAsync("1"); // X축 이동
                                    _ = MoveAsync("2"); // Y축 이동

                                    AddLog($"[Auto Move] 스테이지 목표 위치로 이동 시작...");
                                }
                                else
                                {
                                    AddLog("[Vision Result] WAFER 응답 확인: OK");
                                }
                                // 💡 처리 완료 후 대기 상태 해제
                                _isWaitingForResponse = false;
                            }
                            else if (response.StartsWith("NG"))
                            {
                                AddLog($"[Vision Error] 비전 응답 실패: {response}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddCommLog($"!!! 통신 에러: {ex.Message}");
            }
            finally
            {
                _connectedClient = null;
            }
        }

        private void AddCommLog(string message)
        {
            // 1. UI 요소(ListBox)는 반드시 메인 UI 스레드에서만 접근 가능하므로 Dispatcher 사용
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                string time = DateTime.Now.ToString("[HH:mm:ss]"); // 현재 시간 문자열 생성

                // 2. 최신 로그가 맨 위에 오도록 컬렉션의 0번 인덱스에 추가
                CommLogMessages.Insert(0, $"{time} {message}");

                // 3. 로그 개수가 100개를 초과하면 메모리 보호를 위해 가장 오래된 로그 삭제
                if (CommLogMessages.Count > 100) CommLogMessages.RemoveAt(100);
            });
        }
        #endregion

        #region 4. 동작 로직 (핵심 기능)

        private void AddLog(string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                string time = DateTime.Now.ToString("[HH:mm:ss]");
                LogMessages6Axis.Insert(0, $"{time} [6 - Axis] {message}");
                if (LogMessages6Axis.Count > 100) LogMessages6Axis.RemoveAt(100);
            });
        }

        /// <summary>
        /// ?? [추가] 6축 하드웨어 매트릭스 기반 실시간 피드백 연산 조그 액션
        /// </summary>
        private async Task JogAsync(string param)
        {
            if (_id < 0 || string.IsNullOrEmpty(param) || param.Length < 2)
            {
                AddLog("Error: Not Connected or Invalid Jog Parameter.");
                return;
            }

            // 파라미터 분해 (축 번호와 방향 기호 검출)
            string axis = param.Substring(0, 1);
            string direction = param.Substring(1, 1);

            double currentFeedback = 0;
            double step = 0;

            // 6축 매핑 스펙 정밀 분류 수립
            switch (axis)
            {
                case "1": currentFeedback = CurrentPosX; step = JogStepX; break;
                case "2": currentFeedback = CurrentPosY; step = JogStepY; break;
                case "4": currentFeedback = CurrentPosZ; step = JogStepZ; break;
                case "5": currentFeedback = CurrentPosRotXDeg; step = JogStepRotX; break; // deg 단위 가감산
                case "6": currentFeedback = CurrentPosRotYDeg; step = JogStepRotY; break;
                case "3": currentFeedback = CurrentPosRotZDeg; step = JogStepRotZ; break;
                default: return;
            }

            // 조그 목표치 가감산
            double targetDegOrUm = (direction == "+") ? currentFeedback + step : currentFeedback - step;

            // 회전축(3, 5, 6) 통신 패킷 데이터 규격 uRad 인코딩 변환 처리
            double finalTarget = targetDegOrUm;
            if (axis == "3" || axis == "5" || axis == "6")
            {
                finalTarget = targetDegOrUm * DegToUrad;
            }

            if (!CheckSoftLimit(axis, finalTarget)) return;

            try
            {
                GCS2.SVO(_id, axis, new int[] { 1 });
                if (GCS2.MOV(_id, axis, new double[] { finalTarget }) == 1)
                {
                    // 선형축은 3자리, 회전축은 5자리 분기 로그 출력 포맷팅 적용
                    string format = (axis == "1" || axis == "2" || axis == "4") ? "F3" : "F5";
                    AddLog($"Axis {axis} Jog {direction} ({step}) Started. Target: {targetDegOrUm.ToString(format)}");
                    await MeasureExactMoveTimeMs(axis);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Jog Execution Error: {ex.Message}");
            }
        }

        private void Connect()
        {
            if (_id >= 0)
            {
                // 기존 연결이 열려있다면 안전하게 연결 해제 후 재시도
                GCS2.CloseConnection(_id);
                _id = -1;
            }

            _id = -1;
            AddLog("Opening Interface Dialog...");

            // ?? [수정 완료] 창 내려감 현상을 유발하던 복잡한 windowHandle 로직을 완전히 제거하고,
            // 원래 사용하시던 직관적인 빈 문자열 스펙으로 다이렉트 호출합니다.
            _id = GCS2.InterfaceSetupDlg("");

            if (_id >= 0)
            {
                ConnectionStatus = "Connected [6 - Axis]";
                AddLog("Controller Connected.");
                GCS2.SVO(_id, "1 2 3 4 5 6", new int[] { 1, 1, 1, 1, 1, 1 });
                _isReading = true;
                Task.Run(() => ReadLoop());
            }
            else
            {
                _id = -1;
                ConnectionStatus = "Connect Failed";
                AddLog("Error: Connection Failed.");
            }
        }

        private async Task ReadLoop()
        {
            double[] pos = new double[6];
            while (_isReading && _id >= 0)
            {
                if (GCS2.qPOS(_id, "1 2 3 4 5 6", pos) == 1)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentPosX = pos[0];
                        CurrentPosY = pos[1];
                        CurrentPosRotZ = pos[2];
                        CurrentPosZ = pos[3];
                        CurrentPosRotX = pos[4];
                        CurrentPosRotY = pos[5];
                    });
                }
                await Task.Delay(300);
            }
        }

        private async Task MoveAsync(string axis)
        {
            if (_id < 0) { AddLog("Error: Not Connected."); return; }
            double target = GetTargetByAxis(axis);

            if (!CheckSoftLimit(axis, target)) return;

            try
            {
                GCS2.SVO(_id, axis, new int[] { 1 });
                if (GCS2.MOV(_id, axis, new double[] { target }) == 1)
                {
                    AddLog($"Axis {axis} Moving to {target:F4}...");
                    double moveTimeMs = await MeasureExactMoveTimeMs(axis);
                    AddLog($"Axis {axis} Move Done. Time: {moveTimeMs:F2} ms");
                }
            }
            catch (Exception ex) { AddLog($"Error: {ex.Message}"); }
        }

        private async Task TotalMoveAsync()
        {
            if (_id < 0) { AddLog("Error: Not Connected."); return; }
            double[] targets = { TargetPosX, TargetPosY, TargetPosRotZ, TargetPosZ, TargetPosRotX, TargetPosRotY };

            for (int i = 1; i <= 6; i++)
                if (!CheckSoftLimit(i.ToString(), targets[i - 1])) return;

            if (GCS2.MOV(_id, "1 2 3 4 5 6", targets) == 1)
            {
                AddLog("Total Move Started...");
                double moveTimeMs = await MeasurePosBasedMoveTimeMs("1 2 3 4 5 6", targets);
                AddLog($"Total Move Done. Time: {moveTimeMs:F2} ms");
            }
        }

        private bool CheckSoftLimit(string axis, double target)
        {
            double[] minLimit = new double[1];
            double[] maxLimit = new double[1];
            if (GCS2.qTMN(_id, axis, minLimit) == 0 || GCS2.qTMX(_id, axis, maxLimit) == 0) return false;

            int axisNum = int.Parse(axis);
            double margin = (axisNum == 1 || axisNum == 2 || axisNum == 4) ? 0.0 : 0.1;

            if (target < minLimit[0] + margin || target > maxLimit[0] - margin)
            {
                AddLog($"[Limit Error] Axis {axis}: Range({minLimit[0]}~{maxLimit[0]}), Input:{target:F4}");
                return false;
            }
            return true;
        }

        private async Task<double> MeasureExactMoveTimeMs(string axis)
        {
            int axisCount = axis.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int[] isMoving = new int[axisCount];
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < 5000)
            {
                if (GCS2.IsMoving(_id, axis, isMoving) == 0) break;
                if (isMoving.All(m => m == 0)) break;
                await Task.Delay(5);
            }
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        private async Task<double> MeasurePosBasedMoveTimeMs(string axis, double[] targets)
        {
            int axisCount = targets.Length;
            double[] currentPos = new double[axisCount];
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < 5000)
            {
                if (GCS2.qPOS(_id, axis, currentPos) == 1)
                {
                    bool allIn = true;
                    for (int i = 0; i < axisCount; i++)
                    {
                        double tolerance = 10.0;
                        if (Math.Abs(currentPos[i] - targets[i]) > tolerance) { allIn = false; break; }
                    }
                    if (allIn) break;
                }
                await Task.Delay(5);
            }
            return sw.Elapsed.TotalMilliseconds;
        }

        private double GetTargetByAxis(string axis)
        {
            switch (axis)
            {
                case "1": return TargetPosX;
                case "2": return TargetPosY;
                case "3": return TargetPosRotZ;
                case "4": return TargetPosZ;
                case "5": return TargetPosRotX;
                case "6": return TargetPosRotY;
                default: return 0;
            }
        }

        private void Home(string axis)
        {
            if (_id >= 0)
            {
                GCS2.MOV(_id, axis, new double[] { 0.0 });
                AddLog($"Axis {axis} Homing...");
            }
        }

        private void Disconnect()
        {
            _isReading = false;
            if (_id >= 0) GCS2.CloseConnection(_id);
            _id = -1;
            ConnectionStatus = "Disconnected";
            AddLog("Controller Disconnected.");
        }

        private void ErrorClear()
        {
            LogMessages6Axis.Clear();
            AddLog("Log Cleared.");
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public class localRelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public localRelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
    }
}