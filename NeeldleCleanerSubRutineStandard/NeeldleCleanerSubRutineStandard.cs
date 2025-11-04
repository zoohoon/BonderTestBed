using System;
using System.Collections.Generic;
using System.Linq;
using ProberInterfaces.Param;

namespace NeeldleCleanerSubRutineStandardModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.NeedleClean;
    using NeedleCleanerModuleParameter;
    using System.ComponentModel;
    using SubstrateObjects;
    using System.Runtime.CompilerServices;

    public class NeeldleCleanerSubRutineStandard : INeeldleCleanerSubRoutineStandard, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;


        private IStateModule _NeedleCleanModule;
        public IStateModule NeedleCleanModule
        {
            get { return _NeedleCleanModule; }
            set
            {
                if (value != _NeedleCleanModule)
                {
                    _NeedleCleanModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NeedleCleanObject NC
        {
            get { return this.StageSupervisor().NCObject as NeedleCleanObject; }
        }

        private NeedleCleanSystemParameter _NCParam;
        public NeedleCleanSystemParameter NCParam
        {
            get { return (NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam; }
            set
            {
                if (value != _NCParam)
                {
                    _NCParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    var stage = this.StageSupervisor();
                    NeedleCleanModule = this.NeedleCleaner();
                    var ncModule = (IHasDevParameterizable)NeedleCleanModule;

                    NCParam = stage.NCObject.NCSysParam_IParam as NeedleCleanSystemParameter;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return;
        }

        public bool IsTimeToCleaning(int ncNum)
        {

            try
            {
                // 여기 이 조건들은 포커싱 함수에도 동일하게 들어가 있어야 한다.    
                if (NC.NCSysParam.CleanUnitAttached.Value == true)
                {
                    if (ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1)
                    {
                        NeedleCleanDeviceParameter ncDevParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;
                        if (ncDevParam.SheetDevs[ncNum].Enabled.Value == true)
                        {
                            if (NeedleCleanModule.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {   
                                //니들 클리닝은 핀패드 매치를 볼 필요가 있는가?? 현재 GP에서는 웨이퍼 얼라인과 핀얼라인 됐을때
                                //핀패드 매치를 할 수 있는데 
                                // NC 전에 핀 얼라인이 먼저 되어야 한다.
                                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() != AlignStateEnum.DONE)
                                {
                                    return false;
                                }

                                // 현재 상태 및 인터벌 확인
                                if (ncDevParam.SheetDevs[ncNum].EnableCleaningLotInterval.Value == true &&
                                NC.NCSheetVMDefs[ncNum].FlagCleaningForCurrentLot == false)
                                {
                                    return true;
                                }

                                // 아직 한번도 클리닝을 하지 않았던 경우, 인터벌 안 따지고 바로 하게 되므로 이를 방지하기 위하여 현재 카운트 값으로 초기화 해 준다.
                                if (NC.NCSysParam.SheetDefs[ncNum].MarkedWaferCountVal == 0) NC.NCSysParam.SheetDefs[ncNum].MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount;
                                if (NC.NCSysParam.SheetDefs[ncNum].MarkedDieCountVal == 0) NC.NCSysParam.SheetDefs[ncNum].MarkedDieCountVal = (long)this.LotOPModule().SystemInfo.DieCount;

                                if (ncDevParam.SheetDevs[ncNum].EnableCleaningWaferInterval.Value == true && this.LotOPModule().LotInfo.UnProcessedWaferCount() > 0 &&
                                    ncDevParam.SheetDevs[ncNum].CleaningWaferInterval.Value <= ((long)this.LotOPModule().SystemInfo.WaferCount - NC.NCSysParam.SheetDefs[ncNum].MarkedWaferCountVal))
                                {
                                    return true;
                                }

                                if (ncDevParam.SheetDevs[ncNum].EnableCleaningDieInterval.Value == true &&
                                        ncDevParam.SheetDevs[ncNum].CleaningDieInterval.Value <= ((long)this.LotOPModule().SystemInfo.DieCount - NC.NCSysParam.SheetDefs[ncNum].MarkedDieCountVal))
                                {
                                    return true;
                                }

                                // 내 인터벌이 설정되어 있지 않더라도 다른 NC 인터벌 설정에 내가 포함되어 있으면 해야 한다.
                                for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                                {
                                    if (i != ncNum)
                                    {
                                        if (ncDevParam.SheetDevs[ncNum].EnableAssistanceInterval.Value == true &&
                                            ncDevParam.SheetDevs[ncNum].CleaningAssistanceInterval.Count > 0)
                                        {
                                            foreach (var list in ncDevParam.SheetDevs[ncNum].CleaningAssistanceInterval)
                                            {
                                                if (list.Value == ncNum && ncDevParam.SheetDevs[list.Value].Enabled.Value == true)
                                                {
                                                    int index = ncDevParam.SheetDevs[ncNum].CleaningAssistanceInterval[0].Value;
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }                               
                            }
                            else
                            {
                                // Manual NC
                                if (NC.NCSysParam.ManualNC.EnableCleaning.Value[ncNum] == true)
                                {
                                    return true;
                                }
                            }
                        }                        
                    }
                }
                return false;

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - IsTimeToCleaning() : Error occured.");
                throw err;

            }

        }

        public bool IsNCSensorON()
        {
            bool touch_sensor_on = false;

            try
            {
                IORet ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DINC_SENSOR, out touch_sensor_on);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Debug($"NeedleCleanProcessor - IsNCSensorON() : {EventCodeEnum.GP_CardChange_INIT_FAIL}");
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - IsNCSensorON() : Error occured.");
                LoggerManager.Exception(err);
            }
            return touch_sensor_on;
        }

        public bool IsCleanPadUP()
        {
            //if (NeedleCleanSysParam.NC_TYPE.Value == NeedleCleanSystemParameter.NC_MachineType.MOTOR_NC)
            try
            {
                //if (NCParam.NC_TYPE.Value == MOTOR_NC)
                if (NCParam.NC_TYPE.Value == NC_MachineType.MOTOR_NC)
                {
                    // Motorized NC                    
                    double tmpVal = 0;
                    this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref tmpVal);

                    if (tmpVal <= NCParam.CleanUnitDownPos.Value)
                    {   // Down position
                        return false;
                    }
                    else
                    {   // Up position
                        return true;
                    }
                }
                else
                {
                    // Cylinder Type NC
                    if (this.IOManager().IO.Inputs.DICLEANUNITUP_1.Value == true && this.IOManager().IO.Inputs.DICLEANUNITUP_0.Value == false)
                    { return true; }
                    else
                    { return false; }                    
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                //LoggerManager.Debug(err);
                LoggerManager.Exception(err);
                throw err;
            }
            
        }

        public bool IsCleanPadDown()
        {
            try
            {
                if (NCParam.NC_TYPE.Value == NC_MachineType.MOTOR_NC)
                {
                    // Motorized NC                    
                    double tmpVal = 0;
                    this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref tmpVal);

                    if (tmpVal <= NCParam.CleanUnitDownPos.Value)
                    {   // Down position                        
                        return true;
                    }
                    else
                    {   // Up position
                        return false;
                    }
                }
                else
                {
                    // Cylinder Type NC
                    if (this.IOManager().IO.Inputs.DICLEANUNITUP_0.Value == true && this.IOManager().IO.Inputs.DICLEANUNITUP_1.Value == false)
                    { return true; }
                    else
                    { return false; }                    
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                //LoggerManager.Debug(err);
                LoggerManager.Exception(err);
                throw err;
            }

        }

        public EventCodeEnum WaitForCleanPadUp()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            double tmpVal = 0;

            try
            {
                if (NCParam.NC_TYPE.Value == NC_MachineType.MOTOR_NC)
                {
                    this.MotionManager().WaitForAxisMotionDone(this.MotionManager().GetAxis(EnumAxisConstants.PZ), 30000);
                    this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref tmpVal);

                    if (tmpVal <= NCParam.CleanUnitDownPos.Value)
                    {
                        return EventCodeEnum.NEEDLE_CLEANING_UNIT_UP_FAILURE;
                    }
                    else
                    { return EventCodeEnum.NONE; }                        
                }
                else
                {
                    if (this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DICLEANUNITUP_1, true, 30000) == -1)
                    {
                        // error 
                        return EventCodeEnum.NEEDLE_CLEANING_UNIT_UP_FAILURE;
                    }
                    else
                    {
                        return EventCodeEnum.NONE;
                    }
                }                    
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - WaitForCleanPadUp() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return RetVal;
        }

        public EventCodeEnum WaitForCleanPadDown()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            double tmpVal = 0;

            try
            {
                if (NCParam.NC_TYPE.Value == NC_MachineType.MOTOR_NC)
                {
                    this.MotionManager().WaitForAxisMotionDone(this.MotionManager().GetAxis(EnumAxisConstants.PZ), 30000);
                    this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref tmpVal);

                    if (tmpVal > NCParam.CleanUnitDownPos.Value)
                    {
                        LoggerManager.Debug($"Clean unit down failure : cur pos = {tmpVal}, down pos = {NCParam.CleanUnitDownPos.Value}");
                        return EventCodeEnum.NEEDLE_CLEANING_UNIT_DOWN_FAILURE;
                    }
                    else
                    { return EventCodeEnum.NONE; }
                }
                else
                {
                    if (this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DICLEANUNITUP_0, true, 30000) == -1)
                    {
                        // error 
                        return EventCodeEnum.NEEDLE_CLEANING_UNIT_DOWN_FAILURE;
                    }
                    else
                    {
                        return EventCodeEnum.NONE;
                    }
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - WaitForCleanPadDown() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return RetVal;
        }

        // Not use for opus5
        public EventCodeEnum CleanPadUP(bool bWait)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                RetVal = this.StageSupervisor().StageModuleState.NCPadUp();

                if (bWait == true)
                {
                    RetVal = WaitForCleanPadUp();
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - CleanPadUP() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return RetVal;
        }

        public EventCodeEnum CleanPadDown(bool bWait)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                RetVal = this.StageSupervisor().StageModuleState.NCPadDown();   

                if (bWait == true)
                {
                    RetVal = WaitForCleanPadDown();
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - CleanPadDown() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }

            return RetVal;
        }

        public double GetMeasuredNcPadHeight(int ncNum, double posX, double posY)
        {
                double[] pointD = new double[4];
                pointD[0] = 0;
            try
            {
                pointD[1] = 0;
                pointD[2] = 0;

                NCCoordinate[] tmpPoint = new NCCoordinate[3];

                tmpPoint[0] = new NCCoordinate();
                tmpPoint[1] = new NCCoordinate();
                tmpPoint[2] = new NCCoordinate();
                
                double degree1 = 0.0;
                double degree2 = 0.0;
                double degree3 = 0.0;

                int i, j;
                bool bfound = false;

                /*                
                HeightProfile 함수 사용시 주의사항 !!!

                현재 위치에서 구하고자 하는 높이가 있을 때 사용하는데 만약 현재 위치를 포함하는 3점을 구할 수 없는 경우 즉, 높이를 알고있는 영역 바깥의 높이를
                알고자 하는 경우에 어설프게 평면의 방정식을 사용해서 높이를 구했다가는 엄청 뻥튀기 된 값이 나올 수 있다.

                1) 리스트에 존재하는 높이 값들이 모두 가까운 한 점에 모여 있는 경우, 즉 평면의 넓이가 너무 작은 경우
                2) 평면에서부터 현재 위치까지의 거리가 너무 먼 경우
                3) 구해진 평면이 정삼각형에 가깝지 않고 매우 얇고 길게 직선에 가까운 모양일 경우 (즉 X나 Y 둘중 하나가 매우 작은 경우)

                높이가 저장되는 리스트의 각 항목의 X/Y 위치가 일정치 않다고 가정하면 위의 각 경우에 대해서 항상 적합한 적당한 리미트를 설정하는것이 매우 어렵기 때문에
                여기에서는 현재 위치를 포함하는 평면이 존재하지 않을 경우 평면의 방정식을 사용하지 않고 그냥 가까운 한 점의 높이를 구하여 그 값을 사용한다.

                따라서 올바르게 높이를 구하기 위해서는 반드시 '항상' 현재 위치를 포함하는 평면이 나와야 한다.
                그렇기 때문에 이 함수가 불리기 전에 반드시 '항상' 현재 위치를 포함하는 평면이 존재하도록 리스트가 초기화 되어 있어야 한다.

                값을 초기화 할 때에는 사용 가능한 실제 X/Y 영역보다 훨씬 큰 영역에 대해 널널하게 먼 영역을 지정해 주도록 한다.
                그래야 실제 포커싱 한 영역 바로 바깥 부분의 높이를 계산할 때 높이 값이 외곽부분에서 급격하게 변하는 것을 막을 수 있다.
                예를 들어 웨이퍼 얼라인 높이를 설정하는 경우, 실제 사용가능한 넓이(= 반지름 150,000인 원의 넓이)보다 2배 이상 큰 영역을 모두 포함하는 8개의 포인트 높이를
                지정하여 사용한다.
                
                +                        +                          + <-- 가운데 X의 높이를 알게 되는 순간 십자 위치에 각 높이 지정을 미리 해둔다


                                         
                                     ########  <-- 실제 사용 영역
                                    ##########
                                   ###### #####
                +                 ###### X #####                    +
                                   ###### #####
                                    ##########
                                     ########



                +                        +                          +


                */




                if (NC.NCSheetVMDefs[ncNum].Heights.Count <= 0)
                {
                    // 포커싱 하기 전 상황
                    return 0;
                }

                List<NCCoordinate> ncCoordinates = NC.NCSheetVMDefs[ncNum].Heights.ToList<NCCoordinate>();

                // 리스트 내의 점들의 위치를 현재 위치에서 가까운 순으로 정렬한다.
                ncCoordinates.Sort(delegate (NCCoordinate nc_ccord1, NCCoordinate nc_coord2)
                {
                    if (Distance2D(posX, posY, nc_ccord1.X.Value, nc_ccord1.Y.Value) > Distance2D(posX, posY, nc_coord2.X.Value, nc_coord2.Y.Value)) return 1;
                    if (Distance2D(posX, posY, nc_ccord1.X.Value, nc_ccord1.Y.Value) < Distance2D(posX, posY, nc_coord2.X.Value, nc_coord2.Y.Value)) return -1;
                    return 0;
                });

                NC.NCSheetVMDefs[ncNum].Heights.Clear();
                foreach (var point in ncCoordinates)
                {
                    NC.NCSheetVMDefs[ncNum].Heights.Add(point);
                }

                //NC.NCSysParam.SheetDefs[ncNum].Heights.Sort(delegate(NCCoordinate nc_ccord1, NCCoordinate nc_coord2)
                //{
                //    if (Distance2D(posX, posY, nc_ccord1.X.Value, nc_ccord1.Y.Value) > Distance2D(posX, posY, nc_coord2.X.Value, nc_coord2.Y.Value)) return 1;
                //    if (Distance2D(posX, posY, nc_ccord1.X.Value, nc_ccord1.Y.Value) < Distance2D(posX, posY, nc_coord2.X.Value, nc_coord2.Y.Value)) return -1;
                //    return 0;
                //});


                // 1. 가장 가까운 점을 고른다
                tmpPoint[0].X.Value = NC.NCSheetVMDefs[ncNum].Heights[0].X.Value;
                tmpPoint[0].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[0].Y.Value;
                tmpPoint[0].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[0].Z.Value;
                

                // 2. 현재 위치와 첫 번째 점 사이의 각도를 구한다.
                degree1 = ((Math.Atan2(tmpPoint[0].Y.Value - posY, tmpPoint[0].X.Value - posX)) * (180 / Math.PI));
                if (degree1 < 0) degree1 = 360 + degree1;

                // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                degree1 = degree1 + 180;        
                if (degree1 >= 360) degree1 = degree1 - 360;

                for (i = 1; i <= NC.NCSheetVMDefs[ncNum].Heights.Count - 1; i++)
                {
                    if (bfound == true) break;

                    degree2 = (Math.Atan2(NC.NCSheetVMDefs[ncNum].Heights[i].Y.Value - posY, NC.NCSheetVMDefs[ncNum].Heights[i].X.Value - posX)) * (180 / Math.PI);
                    if (degree2 < 0) degree2 = 360 + degree2;
                    // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                    degree2 = degree2 + 180;
                    if (degree2 >= 360) degree2 = degree2 - 360;

                    // 3. 세 번째의 점을 골라 필요한 영역에 존재하는 지 확인한다.
                    for (j = i + 1; j <= NC.NCSheetVMDefs[ncNum].Heights.Count - 1; j++)
                    {
                        degree3 = (Math.Atan2(NC.NCSheetVMDefs[ncNum].Heights[j].Y.Value - posY, NC.NCSheetVMDefs[ncNum].Heights[j].X.Value - posX)) * (180 / Math.PI);
                        if (degree3 < 0) degree3 = 360 + degree3;
                        
                        // 첫번째 고른 점과 두번째 고른 점의 각도 차이가 180도 이상 발생한다는 뜻은 세번째 점을 고를 때 360도를 넘어서 존재할 수 있다는 뜻이다. 따라서 조건식에 주의해야 한다.
                        if (Math.Abs(degree2 - degree1) < 180)
                        {
                            if (degree2 > degree1)
                            {
                                if (degree3 > degree1 && degree3 < degree2)
                                {
                                    tmpPoint[1].X.Value = NC.NCSheetVMDefs[ncNum].Heights[i].X.Value;
                                    tmpPoint[1].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Y.Value;
                                    tmpPoint[1].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Z.Value;

                                    tmpPoint[2].X.Value = NC.NCSheetVMDefs[ncNum].Heights[j].X.Value;
                                    tmpPoint[2].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Y.Value;
                                    tmpPoint[2].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (degree3 > degree2 && degree3 < degree1)
                                {
                                    tmpPoint[1].X.Value = NC.NCSheetVMDefs[ncNum].Heights[i].X.Value;
                                    tmpPoint[1].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Y.Value;
                                    tmpPoint[1].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Z.Value;

                                    tmpPoint[2].X.Value = NC.NCSheetVMDefs[ncNum].Heights[j].X.Value;
                                    tmpPoint[2].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Y.Value;
                                    tmpPoint[2].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (degree2 > degree1)
                            {
                                if ((degree3 > degree2 && degree3 < 360) || (degree3 < degree1))
                                {
                                    tmpPoint[1].X.Value = NC.NCSheetVMDefs[ncNum].Heights[i].X.Value;
                                    tmpPoint[1].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Y.Value;
                                    tmpPoint[1].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Z.Value;

                                    tmpPoint[2].X.Value = NC.NCSheetVMDefs[ncNum].Heights[j].X.Value;
                                    tmpPoint[2].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Y.Value;
                                    tmpPoint[2].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                            else
                            {
                                if ((degree3 > degree1 && degree3 < 360) || (degree3 < degree2))
                                {
                                    tmpPoint[1].X.Value = NC.NCSheetVMDefs[ncNum].Heights[i].X.Value;
                                    tmpPoint[1].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Y.Value;
                                    tmpPoint[1].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[i].Z.Value;

                                    tmpPoint[2].X.Value = NC.NCSheetVMDefs[ncNum].Heights[j].X.Value;
                                    tmpPoint[2].Y.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Y.Value;
                                    tmpPoint[2].Z.Value = NC.NCSheetVMDefs[ncNum].Heights[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                        }
                    }                    
                }
                
                if (bfound == false)
                {
                    // 현재 포인트를 포함하는 삼각형이 존재하지 않는다. 그냥 가까운거 한 점의 높이를 리턴한다.
                    // (현재 위치를 포함하는 평면이 존재하지 않으므로 평면의 거리가 멀거나 매우 얇은 평면이거나 할 경우
                    // 높이값이 엄청 뻥튀기 될 위험이 있으므로 평면의 방정식을 돌리지 않고 그냥 가까운 점의 높이를 리턴하여 사용한다)
                    return tmpPoint[0].Z.Value;

                    //tmpPoint[1].X.Value = NC.NCSysParam.SheetDefs[ncNum].Heights[1].X.Value;
                    //tmpPoint[1].Y.Value = NC.NCSysParam.SheetDefs[ncNum].Heights[1].Y.Value;
                    //tmpPoint[1].Z.Value = NC.NCSysParam.SheetDefs[ncNum].Heights[1].Z.Value;

                    //tmpPoint[2].X.Value = NC.NCSysParam.SheetDefs[ncNum].Heights[2].X.Value;
                    //tmpPoint[2].Y.Value = NC.NCSysParam.SheetDefs[ncNum].Heights[2].Y.Value;
                    //tmpPoint[2].Z.Value = NC.NCSysParam.SheetDefs[ncNum].Heights[2].Z.Value;
                }

                pointD[0] = tmpPoint[0].Y.Value * (tmpPoint[1].Z.Value - tmpPoint[2].Z.Value) + 
                            tmpPoint[1].Y.Value * (tmpPoint[2].Z.Value - tmpPoint[0].Z.Value) + 
                            tmpPoint[2].Y.Value * (tmpPoint[0].Z.Value - tmpPoint[1].Z.Value);

                pointD[1] = tmpPoint[0].Z.Value * (tmpPoint[1].X.Value - tmpPoint[2].X.Value) + 
                            tmpPoint[1].Z.Value * (tmpPoint[2].X.Value - tmpPoint[0].X.Value) +
                            tmpPoint[2].Z.Value * (tmpPoint[0].X.Value - tmpPoint[1].X.Value);

                pointD[2] = tmpPoint[0].X.Value * (tmpPoint[1].Y.Value - tmpPoint[2].Y.Value) + 
                            tmpPoint[1].X.Value * (tmpPoint[2].Y.Value - tmpPoint[0].Y.Value) +
                            tmpPoint[2].X.Value * (tmpPoint[0].Y.Value - tmpPoint[1].Y.Value);

                pointD[3] = -tmpPoint[0].X.Value * ((tmpPoint[1].Y.Value * tmpPoint[2].Z.Value) - (tmpPoint[2].Y.Value * tmpPoint[1].Z.Value)) -
                             tmpPoint[1].X.Value * ((tmpPoint[2].Y.Value * tmpPoint[0].Z.Value) - (tmpPoint[0].Y.Value * tmpPoint[2].Z.Value)) -
                             tmpPoint[2].X.Value * ((tmpPoint[0].Y.Value * tmpPoint[1].Z.Value) - (tmpPoint[1].Y.Value * tmpPoint[0].Z.Value));

                if (pointD[2] != 0)
                {
                    return -(pointD[0] * posX + pointD[1] * posY + pointD[3]) / pointD[2];
                }
                else
                {
                    return 0;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanerSubRutineStandard() - GetMeasuredNcPadHeight() : Error occured.");
    
                return 0;
            }

        }

        private double Distance2D(double X1, double Y1, double X2, double Y2)
        {
            return Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
        }

        public NCCoordinate ReadNcCurPosForWaferCam(int ncNum)
        {
            NCCoordinate nccoord = new NCCoordinate();
            try
            {
                nccoord = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();
                nccoord.X.Value = nccoord.X.Value - NCParam.SheetDefs[ncNum].Offset.Value.X.Value;
                nccoord.Y.Value = nccoord.Y.Value - NCParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                nccoord.Z.Value = nccoord.Z.Value - NCParam.SheetDefs[ncNum].Offset.Value.Z.Value;                   
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - ReadNcCurPosForWaferCam() : Error occured.");
                throw err;                
            }

            return nccoord;
        }

        public NCCoordinate ReadNcCurPosForPin(int ncNum)
        {
            NCCoordinate nccoord = new NCCoordinate();
            PinCoordinate pincoord = new PinCoordinate();

            try
            {
                pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                nccoord = this.CoordinateManager().WaferHighNCPadConvert.CurrentCleaningPosConvert(pincoord);
                nccoord.X.Value = nccoord.X.Value - NCParam.SheetDefs[ncNum].Offset.Value.X.Value;
                nccoord.Y.Value = nccoord.Y.Value - NCParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                nccoord.Z.Value = nccoord.Z.Value - NCParam.SheetDefs[ncNum].Offset.Value.Z.Value;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - ReadNcCurPosForPin() : Error occured.");
                throw err;
            }

            return nccoord;
        }

        public NCCoordinate ReadNcCurPosForSensor(int ncNum)
        {
            NCCoordinate nccoord = new NCCoordinate();
            PinCoordinate pincoord = new PinCoordinate();

            try
            {
                pincoord.X.Value = NCParam.SensorPos.Value.X.Value;
                pincoord.Y.Value = NCParam.SensorPos.Value.Y.Value;
                pincoord.Z.Value = NCParam.SensorPos.Value.Z.Value;

                nccoord = this.CoordinateManager().WaferHighNCPadConvert.CurrentCleaningPosConvert(pincoord);
                nccoord.X.Value = nccoord.X.Value - NCParam.SheetDefs[ncNum].Offset.Value.X.Value;
                nccoord.Y.Value = nccoord.Y.Value - NCParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                nccoord.Z.Value = nccoord.Z.Value - NCParam.SheetDefs[ncNum].Offset.Value.Z.Value;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - ReadNcCurPosForSensor() : Error occured.");
                throw err;
            }

            return nccoord;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
    }
}
