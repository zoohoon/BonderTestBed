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

namespace ProberViewModel.ViewModel // 💡 6축 뷰모델과 동일하게 네임스페이스 경로 일치
{
    /// <summary>
    /// 3축 나노 스테이지 제어를 위한 ViewModel 클래스
    /// INotifyPropertyChanged를 상속받아 UI와의 데이터 바인딩을 지원함
    /// </summary>
    public class NanoStageViewModel : INotifyPropertyChanged
    {
        private int _id = -1;
        private bool _isReading = false;

        // 단위 변환 상수 (Degree <-> Micro-radian)
        private const double DegToUrad = (Math.PI / 180.0) * 1000000.0;
        private const double UradToDeg = 1.0 / DegToUrad;

        #region 1. 속성 (Monitoring & Logs) - UI 실시간 표시 데이터
        private double _currentPosZ;
        public double CurrentPosZ { get => _currentPosZ; set { _currentPosZ = value; OnPropertyChanged(); } }

        private double _currentPosRotX;
        public double CurrentPosRotX { get => _currentPosRotX; set { _currentPosRotX = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentPosRotXDeg)); } }

        private double _currentPosRotY;
        public double CurrentPosRotY { get => _currentPosRotY; set { _currentPosRotY = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentPosRotYDeg)); } }

        public double CurrentPosRotXDeg => CurrentPosRotX * UradToDeg;
        public double CurrentPosRotYDeg => CurrentPosRotY * UradToDeg;

        // 로그 메시지 리스트 (단독 인스턴스에서 자체 확보하도록 초기화)
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();
        #endregion

        #region 2. 속성 (Target Position & Status) - 이동 목표값 및 상태
        private double _targetPosZ;
        public double TargetPosZ { get => _targetPosZ; set { _targetPosZ = Math.Round(value, 4); OnPropertyChanged(); } }

        private double _targetPosRotXDeg;
        public double TargetPosRotXDeg { get => _targetPosRotXDeg; set { _targetPosRotXDeg = value; TargetPosRotX = value * DegToUrad; OnPropertyChanged(); } }

        private double _targetPosRotYDeg;
        public double TargetPosRotYDeg { get => _targetPosRotYDeg; set { _targetPosRotYDeg = value; TargetPosRotY = value * DegToUrad; OnPropertyChanged(); } }

        private double _targetPosRotX; public double TargetPosRotX { get => _targetPosRotX; set { _targetPosRotX = value; OnPropertyChanged(); } }
        private double _targetPosRotY; public double TargetPosRotY { get => _targetPosRotY; set { _targetPosRotY = value; OnPropertyChanged(); } }

        // 💡 [추가] 각 축별 개별 JOG Step 데이터 저장 속성 (XAML TextBox와 양방향 바인딩)
        private double _jogStepZ = 1.0;
        public double JogStepZ { get => _jogStepZ; set { _jogStepZ = value; OnPropertyChanged(); } }

        private double _jogStepRotX = 0.001;
        public double JogStepRotX { get => _jogStepRotX; set { _jogStepRotX = value; OnPropertyChanged(); } }

        private double _jogStepRotY = 0.001;
        public double JogStepRotY { get => _jogStepRotY; set { _jogStepRotY = value; OnPropertyChanged(); } }

        private string _status = "Disconnected";
        public string ConnectionStatus { get => _status; set { _status = value; OnPropertyChanged(); } }
        #endregion

        #region 3. 커맨드 (UI 버튼과 연결됨)
        public ICommand ConnectCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ErrorClearCommand { get; }
        public ICommand TotalMoveCommand { get; }
        public ICommand JogCommand { get; } // 💡 [추가] 상대 조그 이동 커맨드
        #endregion

        /// <summary>
        /// 기본 생성자: 외부 파라미터 종속성 없이 단독 뉴 개설 가능하도록 변경
        /// </summary>
        public NanoStageViewModel()
        {
            ConnectCommand = new localRelayCommand(p => Connect());
            DisconnectCommand = new localRelayCommand(p => Disconnect());

            // 비동기 컨텍스트 충돌 빨간줄 방지 우회 구성
            MoveCommand = new localRelayCommand(p => { _ = MoveAsync(p?.ToString()); });
            HomeCommand = new localRelayCommand(p => Home(p?.ToString()));

            ErrorClearCommand = new localRelayCommand(p => ErrorClear());
            TotalMoveCommand = new localRelayCommand(p => { _ = TotalMoveAsync(); });

            // 💡 [추가] 조그 커맨드 할당
            JogCommand = new localRelayCommand(p => { _ = JogAsync(p?.ToString()); });
        }

        #region 4. 동작 로직 (핵심 기능)
        private void AddLog(string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                string time = DateTime.Now.ToString("[HH:mm:ss]");
                LogMessages.Insert(0, $"{time} [3 - Axis] {message}");
                if (LogMessages.Count > 100) LogMessages.RemoveAt(100);
            });
        }

        /// <summary>
        /// 💡 [추가] 실시간 피드백 피칭 기반 상대 조그 이동 처리 시퀀스
        /// </summary>
        private async Task JogAsync(string param)
        {
            if (_id < 0 || string.IsNullOrEmpty(param) || param.Length < 2)
            {
                AddLog("Error: Not Connected or Invalid Jog Parameter.");
                return;
            }

            // 파라미터 파싱 (예: "1+" -> axis: "1", direction: "+")
            string axis = param.Substring(0, 1);
            string direction = param.Substring(1, 1);

            double currentFeedback = 0;
            double step = 0;

            // 현재 축의 최신 피드백 수치 및 텍스트박스 입력 스텝값 매칭
            switch (axis)
            {
                case "1":
                    currentFeedback = CurrentPosZ;
                    step = JogStepZ;
                    break;
                case "2":
                    currentFeedback = CurrentPosRotXDeg; // 사용자 입력 편의를 위해 deg 단위 기준 연산
                    step = JogStepRotX;
                    break;
                case "3":
                    currentFeedback = CurrentPosRotYDeg;
                    step = JogStepRotY;
                    break;
                default:
                    return;
            }

            // 방향성 조율 (+ 스텝 / - 스텝 연산)
            double targetDegOrUm = (direction == "+") ? currentFeedback + step : currentFeedback - step;

            // 하드웨어 실제 전송용 변수 가공 (회전축 2, 3번은 uRad 스펙 변환 주입 필요)
            double finalTarget = targetDegOrUm;
            if (axis == "2" || axis == "3")
            {
                finalTarget = targetDegOrUm * DegToUrad;
            }

            // 소프트웨어 제한 가동 범위 검증
            if (!CheckSoftLimit(axis, finalTarget)) return;

            try
            {
                GCS2.SVO(_id, axis, new int[] { 1 });
                if (GCS2.MOV(_id, axis, new double[] { finalTarget }) == 1)
                {
                    AddLog($"Axis {axis} Jog {direction} ({step}) Started. Target: {targetDegOrUm:F5}");
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
                ForceDisconnect();
            }

            _id = -1;
            AddLog("Opening Interface Dialog...");

            // 💡 [조치 완료] 창 내려감 방지용 핸들 로직을 완전히 걷어내고 
            // 원래 의도하셨던 직관적인 빈 문자열 스펙으로 다이렉트 호출합니다.
            int newId = GCS2.InterfaceSetupDlg("");

            if (newId >= 0)
            {
                _id = newId;
                ConnectionStatus = "Connected [3 - Axis]";
                AddLog($"Controller Connected. ID: {_id}");
                GCS2.SVO(_id, "1 2 3", new int[] { 1, 1, 1 });
                _isReading = true;
                Task.Run(() => ReadLoop());
            }
            else
            {
                _id = -1;
                ConnectionStatus = "Disconnected";
                AddLog("Connection Cancelled or Failed.");
            }
        }

        private void Disconnect()
        {
            _isReading = false;
            if (_id >= 0)
            {
                try
                {
                    GCS2.SVO(_id, "1 2 3", new int[] { 0, 0, 0 });
                    GCS2.CloseConnection(_id);
                }
                catch { }
                finally { _id = -1; }
            }
            ConnectionStatus = "Disconnected";
            AddLog("Controller Disconnected.");
        }

        private void ForceDisconnect()
        {
            _isReading = false;
            if (_id >= 0) { GCS2.CloseConnection(_id); _id = -1; }
        }

        private async Task MoveAsync(string axis)
        {
            if (_id < 0) { AddLog("Error: Not Connected."); return; }
            double target;
            switch (axis)
            {
                case "1": target = TargetPosZ; break;
                case "2": target = TargetPosRotX; break;
                case "3": target = TargetPosRotY; break;
                default: return;
            }

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
            if (!CheckSoftLimit("1", TargetPosZ) || !CheckSoftLimit("2", TargetPosRotX) || !CheckSoftLimit("3", TargetPosRotY)) return;

            double[] targets = { TargetPosZ, TargetPosRotX, TargetPosRotY };
            if (GCS2.MOV(_id, "1 2 3", targets) == 1)
            {
                AddLog("Total Move Started...");
                double moveTimeMs = await MeasurePosBasedMoveTimeMs("1 2 3", targets);
                AddLog($"Total Move Done. Time: {moveTimeMs:F2} ms");
            }
        }

        private bool CheckSoftLimit(string axis, double target)
        {
            if (_id < 0) return false;
            double[] minLimit = new double[1], maxLimit = new double[1];
            if (GCS2.qTMN(_id, axis, minLimit) == 0 || GCS2.qTMX(_id, axis, maxLimit) == 0) return false;

            double margin = (axis == "1") ? 0.0 : 100.0;
            if (target < minLimit[0] + margin || target > maxLimit[0] - margin)
            {
                AddLog($"[Limit Error] Axis {axis}: Range({minLimit[0]}~{maxLimit[0]}), Input:{target:F4}");
                return false;
            }
            return true;
        }

        private void Home(string axisIndex)
        {
            if (_id < 0) return;
            GCS2.SVO(_id, axisIndex, new int[] { 1 });
            if (GCS2.MOV(_id, axisIndex, new double[] { 0.0 }) == 1)
                AddLog($"Axis {axisIndex} Homing to 0...");
        }

        private void ErrorClear()
        {
            LogMessages.Clear();
            AddLog("Log Cleared.");
        }

        private async Task ReadLoop()
        {
            double[] pos = new double[3];
            while (_isReading)
            {
                if (_id < 0) break;
                if (GCS2.qPOS(_id, "1 2 3", pos) == 1)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentPosZ = pos[0];
                        CurrentPosRotX = pos[1];
                        CurrentPosRotY = pos[2];
                    });
                }
                await Task.Delay(100);
            }
        }

        private async Task<double> MeasureExactMoveTimeMs(string axis)
        {
            int[] isMoving = new int[1];
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 5000)
            {
                if (GCS2.IsMoving(_id, axis, isMoving) == 0 || isMoving[0] == 0) break;
                await Task.Delay(5);
            }
            return sw.Elapsed.TotalMilliseconds;
        }

        private async Task<double> MeasurePosBasedMoveTimeMs(string axis, double[] targets)
        {
            double[] currentPos = new double[3];
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 5000)
            {
                if (GCS2.qPOS(_id, axis, currentPos) == 1)
                {
                    bool done = true;
                    for (int i = 0; i < 3; i++)
                    {
                        if (Math.Abs(currentPos[i] - targets[i]) > 10.0) { done = false; break; }
                    }
                    if (done) break;
                }
                await Task.Delay(5);
            }
            return sw.Elapsed.TotalMilliseconds;
        }
        #endregion

        // PropertyChanged 이벤트 구현
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 💡 6축 코드와 연동성을 맞추기 위해 로컬 릴레이 커맨드 구현 적용
        /// </summary>
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