using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VisionTestViewModel;

namespace BWaferMapTest
{
    public class WaferMapTest : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public object PreViewTarget { get; set; }

        private object _ViewTarget;
        public object ViewTarget
        {
            get { return _ViewTarget; }
            set
            {
                if (value != _ViewTarget)
                {
                    PreViewTarget = _ViewTarget;
                    _ViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }
        private WaferObject Wafer 
        { 
            get 
            { 
                return (WaferObject)this.StageSupervisor().WaferObject; 
            } 
        }
        // 원본 DieType 저장용
        private Dictionary<(int MapX, int MapY), DieTypeEnum> _OriginalDieTypeMap = new Dictionary<(int, int), DieTypeEnum>();

        public EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                // 웨이퍼맵 띄움
                ViewTarget = Wafer;
                if (this.ViewTarget is IWaferObject)
                {
                    this.ViewTarget = Wafer;
                }

                UpdateCurrentDies();
            }
            catch (Exception err)
            {
                throw;
            }
            return ret;
        }

        #region Button
        // 스캔 결과 전체 Test_Die 목록
        private List<(int MapX, int MapY, long UserX, long UserY)> _CurrentDies = new List<(int MapX, int MapY, long UserX, long UserY)>();
        // 스캔 결과 Mark_Die 목록
        private List<(int MapX, int MapY, long UserX, long UserY)> _MarkedDies = new List<(int MapX, int MapY, long UserX, long UserY)>();

        private ScanMode GetCurrentScanMode()
        {
            switch (ScanModeIndex)
            {
                case 0:
                    {
                        return ScanMode.HorizontalSnake;
                    }

                case 1:
                    {
                        return ScanMode.HorizontalSnakeReverse;
                    }

                case 2:
                    {
                        return ScanMode.VerticalSnake;
                    }

                case 3:
                    {
                        return ScanMode.VerticalSnakeReverse;
                    }

                default:
                    {
                        return ScanMode.HorizontalSnake;
                    }
            }
        }

        private void UpdateCurrentDies()
        {
            RestoreOriginalDieType();

            _CurrentDies.Clear();

            ReadMap(_CurrentDies, GetCurrentScanMode());
        }

        private int _ScanModeIndex = 0;
        public int ScanModeIndex
        {
            get => _ScanModeIndex;

            set
            {
                _ScanModeIndex = value;

                RaisePropertyChanged();

                UpdateCurrentDies();
            }
        }

        private AsyncCommand _XOddNumberCommand;
        public ICommand XOddNumberCommand
        {
            get
            {
                if (null == _XOddNumberCommand) _XOddNumberCommand = new AsyncCommand(XOddNumberFunc);
                return _XOddNumberCommand;
            }
        }

        private async Task XOddNumberFunc()
        {
            try
            {
                RestoreOriginalDieType();

                ChangeDieTypeToMark(_CurrentDies, die => die.UserX % 2 == 1);
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private AsyncCommand _XEvenNumberCommand;
        public ICommand XEvenNumberCommand
        {
            get
            {
                if (null == _XEvenNumberCommand) _XEvenNumberCommand = new AsyncCommand(XEvenNumberFunc);
                return _XEvenNumberCommand;
            }
        }

        private async Task XEvenNumberFunc()
        {
            try
            {
                RestoreOriginalDieType();

                ChangeDieTypeToMark(_CurrentDies, die => die.UserX % 2 == 0);
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private AsyncCommand _YOddNumberCommand;
        public ICommand YOddNumberCommand
        {
            get
            {
                if (null == _YOddNumberCommand) _YOddNumberCommand = new AsyncCommand(YOddNumberFunc);
                return _YOddNumberCommand;
            }
        }

        private async Task YOddNumberFunc()
        {
            try
            {
                RestoreOriginalDieType();

                ChangeDieTypeToMark(_CurrentDies, die => die.UserY % 2 == 1);
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private AsyncCommand _YEvenNumberCommand;
        public ICommand YEvenNumberCommand
        {
            get
            {
                if (null == _YEvenNumberCommand) _YEvenNumberCommand = new AsyncCommand(YEvenNumberFunc);
                return _YEvenNumberCommand;
            }
        }

        private async Task YEvenNumberFunc()
        {
            try
            {
                RestoreOriginalDieType();

                ChangeDieTypeToMark(_CurrentDies, die => die.UserY % 2 == 0);
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private int? _JumpNumber = 0;
        public int? JumpNumber
        {
            get { return _JumpNumber; }
            set
            {
                if (value != _JumpNumber)
                {
                    _JumpNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _JumpNumberCommand;
        public ICommand JumpNumberCommand
        {
            get
            {
                if (null == _JumpNumberCommand) _JumpNumberCommand = new AsyncCommand(JumpNumberFunc);
                return _JumpNumberCommand;
            }
        }

        private async Task JumpNumberFunc()
        {
            try
            {
                if (JumpNumber <= 0)
                {
                    return;
                }

                RestoreOriginalDieType();

                ChangeJumpDieTypeToMark(_CurrentDies, (die, index) => index % JumpNumber == 0);
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private AsyncCommand _ChuckMoveCommand;
        public ICommand ChuckMoveCommand
        {
            get
            {
                if (null == _ChuckMoveCommand) _ChuckMoveCommand = new AsyncCommand(ChuckMoveFunc);
                return _ChuckMoveCommand;
            }
        }
        private async Task ChuckMoveFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            ProbeAxisObject xaxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);

            LoggerManager.PinLog("WaferMapTest.cs - ChuckMoveFunc() Start");
            try
            {
                MachineCoordinate mccoord = new MachineCoordinate();
                WaferCoordinate wafercoord = new WaferCoordinate();

                VisionTestViewModelBase visionTestViewModelBase = new VisionTestViewModelBase();

                // 등록한 센터값 가져오기, 편의성때문에 공용 필드 변수는 static 설정 함
                double tmpCenterX = VisionTestViewModelBase.centerX;
                double tmpCenterY = VisionTestViewModelBase.centerY;

                HashSet<(int MapX, int MapY)> markedDieSet = _MarkedDies.Select(die => (die.MapX, die.MapY)).ToHashSet();

                // MARK_DIE 제외한 TEST_DIE 대상
                var targetDies = _CurrentDies.Where(die => markedDieSet.Contains((die.MapX, die.MapY)) == false).ToList();

                double zpos = 0;
                double zposOrigin = VisionTestViewModelBase.setPointCenZ;
                if (tmpCenterX != 0 && tmpCenterY != 0)  // center 미등록시 미실행
                {
                    bool isFirstDie = true;
                    double prevPosX = 0;
                    double prevPosY = 0;
                    int logCount = 0;
                    foreach (var die in targetDies)
                    {
                        LoggerManager.PinLog($"Process : {logCount + 1} / {targetDies.Count}");
                        var pos = CalcDiePosition(die.MapX, die.MapY, tmpCenterX, tmpCenterY);

                        mccoord.X.Value = pos.PosX;
                        mccoord.Y.Value = pos.PosY;

                        int moveMultiple = 1;       // 이동 배수 계산 (1~3값 반환)

                        if (isFirstDie == false) // 첫번째 위치인 경우 실행 안함
                        {
                            // *이동할 위치 계산 부분 (이전위치 vs 목표위치)
                            moveMultiple = CalcMoveMultiple(prevPosX, prevPosY, pos.PosX, pos.PosY);
                        }

                        // 현재 위치 저장 및 플래그 변경
                        prevPosX = pos.PosX;
                        prevPosY = pos.PosY;
                        isFirstDie = false;

                        // 속도, 가감속 값 기본값 저장
                        double defaultXSpeed = xaxis.Param.Speed.Value;
                        double defaultXAccel = xaxis.Param.Acceleration.Value;
                        double defaultXAJerk = xaxis.Param.AccelerationJerk.Value;
                        double defaultXDccel = xaxis.Param.Decceleration.Value;
                        double defaultXDJerk = xaxis.Param.DeccelerationJerk.Value;

                        double defaultYSpeed = yaxis.Param.Speed.Value;
                        double defaultYAccel = yaxis.Param.Acceleration.Value;
                        double defaultYAJerk = yaxis.Param.AccelerationJerk.Value;
                        double defaultYDccel = yaxis.Param.Decceleration.Value;
                        double defaultYDJerk = yaxis.Param.DeccelerationJerk.Value;

                        // *속도, 가감속 변경값 적용
                        ApplyMoveSpeed(xaxis, yaxis, moveMultiple, 
                            defaultXSpeed, defaultXAccel, defaultXAJerk, defaultXDccel, defaultXDJerk,
                            defaultYSpeed, defaultYAccel, defaultYAJerk, defaultYDccel, defaultYDJerk);

                        // 이동하려는 posX, posY에 해당하는 Z 계산값 (목표위치)
                        zpos = visionTestViewModelBase.GetZFromPoints(pos.PosX, pos.PosY, zposOrigin, VisionTestViewModelBase.points);

                        // 계산된 Z값 절충 (목표위치)
                        double resultzpos = visionTestViewModelBase.GetZValue(zpos, zposOrigin);

                        // 최종 Z값 할당 (목표위치)
                        mccoord.Z.Value = resultzpos;

                        // 좌표 변환 (목표위치)
                        wafercoord = this.CoordinateManager().WaferHighChuckConvert.Convert(mccoord);

                        // 위치, 속도, 가감속 값 로그표시
                        LoggerManager.PinLog($"WaferMapTest - ChuckMoveFunc() - WaferHighViewMove : X = {pos.PosX} , Y = {pos.PosY} , Z = {resultzpos} , Multiple = {moveMultiple}");
                        LoggerManager.PinLog($"X Speed = {xaxis.Param.Speed.Value}, X Accel = {xaxis.Param.Acceleration.Value}");
                        LoggerManager.PinLog($"Y Speed = {yaxis.Param.Speed.Value}, Y Accel = {yaxis.Param.Acceleration.Value}");

                        // *****이동
                        LoggerManager.PinLog("WaferMapTest - ChuckMoveFunc() - WaferHighViewMove : Start");
                        ret = this.StageSupervisor().StageModuleState.WaferHighViewMove(wafercoord.X.Value, wafercoord.Y.Value, wafercoord.Z.Value);
                        LoggerManager.PinLog("WaferMapTest - ChuckMoveFunc() - WaferHighViewMove : End");

                        // *속도, 가감속 변경값 원상복구
                        RestoreMoveSpeed(xaxis, yaxis, 
                            defaultXSpeed, defaultXAccel, defaultXAJerk, defaultXDccel, defaultXDJerk,
                            defaultYSpeed, defaultYAccel, defaultYAJerk, defaultYDccel, defaultYDJerk);

                        if (ret == EventCodeEnum.NONE)
                        {
                            // 이미지 저장, SaveImageFunc_Index을 static으로 변경
                            LoggerManager.PinLog("WaferMapTest - ChuckMoveFunc() - SaveImage : Start");
                            visionTestViewModelBase.SaveImageFunc_Index(00, resultzpos, die.UserX + 1, die.UserY + 1, true);
                            LoggerManager.PinLog("WaferMapTest - ChuckMoveFunc() - SaveImage : End");
                        }
                        else
                        {
                            return;
                        }

                        // *****기준 Z 위치로 원복
                        mccoord.Z.Value = zposOrigin;
                        wafercoord = this.CoordinateManager().WaferHighChuckConvert.Convert(mccoord);
                        LoggerManager.PinLog("WaferMapTest - ChuckMoveFunc() - WaferHighViewMove : Z Oringin Start");
                        ret = this.StageSupervisor().StageModuleState.WaferHighViewMove(wafercoord.X.Value, wafercoord.Y.Value, wafercoord.Z.Value);
                        LoggerManager.PinLog("WaferMapTest - ChuckMoveFunc() - WaferHighViewMove : Z Origin End");

                        logCount++;
                    }
                }
                else
                {
                    LoggerManager.Debug("WaferMapTest.cs - ChuckMoveFunc -> CenterX, CenterY 미등록으로 실행 안됨");
                }
            }
            catch (Exception err)
            {
                throw;
            }
            finally
            {
                // 속도 관련 파라미터 원상복구
            }

            LoggerManager.PinLog("WaferMapTest.cs - ChuckMoveFunc() End");
        }

        private int _RandomCount = 0;
        public int RandomCount
        {
            get { return _RandomCount; }
            set
            {
                if (value != _RandomCount)
                {
                    _RandomCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _RandomMapCommand;
        public ICommand RandomMapCommand
        {
            get
            {
                if (null == _RandomMapCommand) _RandomMapCommand = new AsyncCommand(RandomMapFunc);
                return _RandomMapCommand;
            }
        }
        private async Task RandomMapFunc()
        {
            try
            {
                RestoreOriginalDieType();

                _MarkedDies.Clear();

                Random rand = new Random();

                var shuffled = _CurrentDies.OrderBy(x => rand.Next()).ToList();

                var targetDies = shuffled.Take(RandomCount).ToList();

                foreach (var die in targetDies)
                {
                    int mapX = die.MapX;
                    int mapY = die.MapY;

                    if (_OriginalDieTypeMap.ContainsKey((mapX, mapY)) == false)
                    {
                        _OriginalDieTypeMap.Add((mapX, mapY), Wafer.WaferDevObject.Info.DIEs[mapX, mapY].DieType.Value);
                    }

                    Wafer.WaferDevObject.Info.DIEs[mapX, mapY].DieType.Value = DieTypeEnum.MARK_DIE;

                    _MarkedDies.Add(die);
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }
        #endregion

        public enum ScanMode
        {
            // 1 : 가로 스네이크 , 디폴트값
            HorizontalSnake = 0,

            // 2 : 가로 스네이크 Reverse
            HorizontalSnakeReverse = 1,

            // 3 : 세로 스네이크
            VerticalSnake = 2,

            // 4 : 세로 스네이크 Reverse
            VerticalSnakeReverse = 3
        }

        #region Function
        public void ReadMap(List<(int MapX, int MapY, long UserX, long UserY)> dies, ScanMode mode)
        {
            try
            {
                int xNumber = ((PMIInfo)Wafer.PMIInfo).MapWidth;
                int yNumber = ((PMIInfo)Wafer.PMIInfo).MapHeight;

                switch (mode)
                {
                    // 0 : →↓←↓
                    // 짝수줄 : 좌 -> 우
                    // 홀수줄 : 우 -> 좌
                    case ScanMode.HorizontalSnake:
                        {
                            for (int y = 0; y < yNumber; y++)
                            {
                                // 짝수 Row
                                if (y % 2 == 0)
                                {
                                    for (int x = 0; x < xNumber; x++)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                                // 홀수 Row
                                else
                                {
                                    for (int x = xNumber - 1; x >= 0; x--)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                            }

                            break;
                        }

                    // 1 : →↑←↑
                    // 아래 -> 위 순회
                    // 짝수줄 : 좌 -> 우
                    // 홀수줄 : 우 -> 좌
                    case ScanMode.HorizontalSnakeReverse:
                        {
                            for (int y = yNumber - 1; y >= 0; y--)
                            {
                                // 실제 Row Index 기준
                                if (((yNumber - 1) - y) % 2 == 0)
                                {
                                    for (int x = 0; x < xNumber; x++)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                                else
                                {
                                    for (int x = xNumber - 1; x >= 0; x--)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                            }

                            break;
                        }

                    // 2 : ↓→↑→
                    // 짝수열 : 위 -> 아래
                    // 홀수열 : 아래 -> 위
                    case ScanMode.VerticalSnake:
                        {
                            for (int x = 0; x < xNumber; x++)
                            {
                                // 짝수 Column
                                if (x % 2 == 0)
                                {
                                    for (int y = 0; y < yNumber; y++)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                                // 홀수 Column
                                else
                                {
                                    for (int y = yNumber - 1; y >= 0; y--)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                            }

                            break;
                        }

                    // 3 : ↓←↑←
                    // 우 -> 좌 순회
                    // 짝수열 : 위 -> 아래
                    // 홀수열 : 아래 -> 위
                    case ScanMode.VerticalSnakeReverse:
                        {
                            for (int x = xNumber - 1; x >= 0; x--)
                            {
                                // 실제 Column Index 기준
                                if (((xNumber - 1) - x) % 2 == 0)
                                {
                                    for (int y = 0; y < yNumber; y++)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                                else
                                {
                                    for (int y = yNumber - 1; y >= 0; y--)
                                    {
                                        AddDie(x, y, dies);
                                    }
                                }
                            }

                            break;
                        }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void AddDie(int x, int y, List<(int MapX, int MapY, long UserX, long UserY)> dies)
        {
            Element<DieTypeEnum> dieType = Wafer.WaferDevObject.Info.DIEs[x, y].DieType;

            if (dieType.Value == DieTypeEnum.TEST_DIE)
            {
                long userX = Wafer.WaferDevObject.Info.DIEs[x, y].DieIndex.XIndex;
                long userY = Wafer.WaferDevObject.Info.DIEs[x, y].DieIndex.YIndex;

                dies.Add((x, y, userX, userY));
            }
        }
        public void ChangeDieTypeToMark(
                            List<(int MapX, int MapY, long UserX, long UserY)> dies,
                            Func<(int MapX, int MapY, long UserX, long UserY), bool> condition)
        {
            try
            {
                _MarkedDies.Clear();

                var targetDies = dies.Where(condition).ToList();

                for (int i = 0; i < targetDies.Count; i++)
                {
                    int mapX = targetDies[i].MapX;
                    int mapY = targetDies[i].MapY;

                    if (_OriginalDieTypeMap.ContainsKey((mapX, mapY)) == false)
                    {
                        _OriginalDieTypeMap.Add((mapX, mapY),Wafer.WaferDevObject.Info.DIEs[mapX, mapY].DieType.Value);
                    }

                    Wafer.WaferDevObject.Info.DIEs[mapX, mapY].DieType.Value = DieTypeEnum.MARK_DIE;

                    // 이동 대상(마크다이) 저장
                    _MarkedDies.Add(targetDies[i]);
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }
        public void ChangeJumpDieTypeToMark(
                                    List<(int MapX, int MapY, long UserX, long UserY)> dies,
                                    Func<(int MapX, int MapY, long UserX, long UserY), int, bool> condition)
        {
            try
            {
                _MarkedDies.Clear();

                var targetDies = dies.Where((die, index) => condition(die, index)).ToList();

                for (int i = 0; i < targetDies.Count; i++)
                {
                    int mapX = targetDies[i].MapX;
                    int mapY = targetDies[i].MapY;

                    if (_OriginalDieTypeMap.ContainsKey((mapX, mapY)) == false)
                    {
                        _OriginalDieTypeMap.Add((mapX, mapY), Wafer.WaferDevObject.Info.DIEs[mapX, mapY].DieType.Value);
                    }

                    Wafer.WaferDevObject.Info.DIEs[mapX, mapY].DieType.Value = DieTypeEnum.MARK_DIE;

                    // 이동 대상(마크다이) 저장
                    _MarkedDies.Add(targetDies[i]);
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }
        public void RestoreOriginalDieType()
        {
            // WaferMap 마크표시 원본으로 초기화
            try
            {
                foreach (var item in _OriginalDieTypeMap)
                {
                    int mapX = item.Key.MapX;
                    int mapY = item.Key.MapY;

                    Wafer.WaferDevObject.Info.DIEs[mapX, mapY].DieType.Value = item.Value;
                }

                _OriginalDieTypeMap.Clear();
            }
            catch (Exception err)
            {
                throw;
            }
        }
        private (double PosX, double PosY) CalcDiePosition(int mapX, int mapY, double waferCenterX, double waferCenterY)
        {
            int centerX = 13;
            int centerY = 13;

            double pitchX = 10080;
            double pitchY = 10080;

            int offsetX = mapX - centerX;
            int offsetY = mapY - centerY;

            // 인덱스 +방향 = 척의 -방향
            double posX = waferCenterX - (offsetX * pitchX);
            double posY = waferCenterY - (offsetY * pitchY);

            return (posX, posY);
        }
        private int CalcMoveMultiple(double prevPosX, double prevPosY, double currentPosX, double currentPosY)
        {
            try
            {
                double distanceX = currentPosX - prevPosX;
                double distanceY = currentPosY - prevPosY;

                double distance = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2));

                LoggerManager.PinLog($"Prev({prevPosX}, {prevPosY}) -> Current({currentPosX}, {currentPosY})");
                LoggerManager.PinLog($"DistanceX : {distanceX}, DistanceY : {distanceY}, TotalDistance : {distance}");

                double pitch = 10080;

                if (distance <= pitch)
                {
                    return 1;
                }
                else if (distance <= pitch * 2)
                {
                    return 2;
                }
                else if (distance <= pitch * 3)
                {
                    return 3;
                }
                else
                {
                    return 3;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void ApplyMoveSpeed(ProbeAxisObject xaxis, ProbeAxisObject yaxis, int multiple,
                                    double defaultXSpeed, double defaultXAccel, double defaultXAJerk, double defaultXDccel ,double defaultXDJerk,
                                    double defaultYSpeed, double defaultYAccel, double defaultYAJerk, double defaultYDccel ,double defaultYDJerk)
        {
            try
            {
                if(multiple > 3)
                {
                    return;
                }

                if(multiple == 1)
                {
                    // no work
                }
                else if(multiple == 2)
                {
                    xaxis.Param.Speed.Value = defaultXSpeed * multiple;
                    xaxis.Param.Acceleration.Value = defaultXAccel * multiple;
                    xaxis.Param.AccelerationJerk.Value = defaultXAJerk * 1.7;       // AJerk 수정
                    xaxis.Param.Decceleration.Value = defaultXDccel * multiple;
                    xaxis.Param.DeccelerationJerk.Value = defaultXDJerk * 1.7;      // DJerk 수정

                    yaxis.Param.Speed.Value = defaultYSpeed * multiple;
                    yaxis.Param.Acceleration.Value = defaultYAccel * 2.42;          // Accel 수정
                    yaxis.Param.AccelerationJerk.Value = defaultYAJerk * multiple;
                    yaxis.Param.Decceleration.Value = defaultYDccel * 2.42;         // Dccel 수정
                    yaxis.Param.DeccelerationJerk.Value = defaultYDJerk * multiple;
                }
                else
                {
                    xaxis.Param.Speed.Value = defaultXSpeed * multiple;
                    xaxis.Param.Acceleration.Value = defaultXAccel * multiple;
                    xaxis.Param.AccelerationJerk.Value = defaultXAJerk * 2;         // AJerk 수정
                    xaxis.Param.Decceleration.Value = defaultXDccel * multiple;
                    xaxis.Param.DeccelerationJerk.Value = defaultXDJerk * 2;        // DJerk 수정

                    yaxis.Param.Speed.Value = defaultYSpeed * multiple;
                    yaxis.Param.Acceleration.Value = defaultYAccel * 2.5;           // Accel 수정
                    yaxis.Param.AccelerationJerk.Value = defaultYAJerk * multiple;
                    yaxis.Param.Decceleration.Value = defaultYDccel * 2.5;          // Dccel 수정
                    yaxis.Param.DeccelerationJerk.Value = defaultYDJerk * multiple;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void RestoreMoveSpeed(ProbeAxisObject xaxis, ProbeAxisObject yaxis,
                                      double defaultXSpeed, double defaultXAccel, double defaultXAJerk, double defaultXDccel, double defaultXDJerk,
                                      double defaultYSpeed, double defaultYAccel, double defaultYAJerk, double defaultYDccel, double defaultYDJerk)
        {
            try
            {
                xaxis.Param.Speed.Value = defaultXSpeed;
                xaxis.Param.Acceleration.Value = defaultXAccel;
                xaxis.Param.AccelerationJerk.Value = defaultXAJerk;
                xaxis.Param.Decceleration.Value = defaultXDccel;
                xaxis.Param.DeccelerationJerk.Value = defaultXDJerk;

                yaxis.Param.Speed.Value = defaultYSpeed;
                yaxis.Param.Acceleration.Value = defaultYAccel;
                yaxis.Param.AccelerationJerk.Value = defaultYAJerk;
                yaxis.Param.Decceleration.Value = defaultYDccel;
                yaxis.Param.DeccelerationJerk.Value = defaultYDJerk;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
