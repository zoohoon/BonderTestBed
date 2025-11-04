using ErrorParam;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Xml.Serialization;

namespace ErrorMapping
{
    public class ErrorMappingManager : INotifyPropertyChanged, IFactoryModule, IModule
    {
        
        //private string ErrorMappingParamFilePath = @"C:\ProberSystem\Parameters\VisionMapping\VisionMappingParameter.json";
        public  string ERROR_DATA_PATH = "c:\\PROBERPARAM\\ErrorData\\";

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IStageSupervisor StageSupervisor;
        private IWaferAligner WaferAligner;
        private IVisionManager VisionManager;
        private IMotionManager MotionManager;

        private ErrorMappingParam _ErrMappingParam;
        public ErrorMappingParam ErrMappingParam
        {
            get { return _ErrMappingParam; }
            set
            {
                if (value != _ErrMappingParam)
                {
                    _ErrMappingParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<ErrorDataTable> _ErrorDataXTable = new List<ErrorDataTable>();
        public List<ErrorDataTable> ErrorDataXTable
        {
            get { return _ErrorDataXTable; }
            set
            {
                if (value != _ErrorDataXTable)
                {
                    _ErrorDataXTable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<ErrorDataTable> _ErrorDataYTable = new List<ErrorDataTable>();
        public List<ErrorDataTable> ErrorDataYTable
        {
            get { return _ErrorDataYTable; }
            set
            {
                if (value != _ErrorDataYTable)
                {
                    _ErrorDataYTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ErrorMappingCamera _CurrentMappingCam;
        public ErrorMappingCamera CurrentMappingCam
        {
            get { return _CurrentMappingCam; }
            set
            {
                if (value != _CurrentMappingCam)
                {
                    _CurrentMappingCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private FirstErrorTable _ErrorCompTable;
        public FirstErrorTable ErrorCompTable
        {
            get { return _ErrorCompTable; }
            set
            {
                if (value != _ErrorCompTable)
                {
                    _ErrorCompTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MakingErrorTable _MakingErrTable = new MakingErrorTable();
        public MakingErrorTable MakingErrTable
        {
            get { return _MakingErrTable; }
            set
            {
                if (value != _MakingErrTable)
                {
                    _MakingErrTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CurrentCamPosX;
        public double CurrentCamPosX
        {
            get { return _CurrentCamPosX; }
            set
            {
                if (value != _CurrentCamPosX)
                {
                    _CurrentCamPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CurrentCamPosY;
        public double CurrentCamPosY
        {
            get { return _CurrentCamPosY; }
            set
            {
                if (value != _CurrentCamPosY)
                {
                    _CurrentCamPosY = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int CurCamIndex = 0;
        private void DefalutSetting()
        {
            try
            {
            ErrorCompTable = new FirstErrorTable();

            //X ErrorTable
            _ErrorDataXTable.Add(new ErrorDataTable());
            _ErrorDataXTable.Add(new ErrorDataTable());
            _ErrorDataXTable.Add(new ErrorDataTable());
            _ErrorDataXTable.Add(new ErrorDataTable());

            //Y ErrorTable
            _ErrorDataYTable.Add(new ErrorDataTable());
            _ErrorDataYTable.Add(new ErrorDataTable());
            _ErrorDataYTable.Add(new ErrorDataTable());
            _ErrorDataYTable.Add(new ErrorDataTable());
            _ErrorDataYTable.Add(new ErrorDataTable());
            _ErrorDataYTable.Add(new ErrorDataTable());
            //_ErrorDataYTable.Add(new ErrorDataTable());
            //_ErrorDataYTable.Add(new ErrorDataTable());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private string filePath = "";
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    StageSupervisor = this.StageSupervisor();
                    WaferAligner = this.WaferAligner();
                    VisionManager = this.VisionManager();
                    MotionManager = this.MotionManager();

                    LoadErrorMappingParameter();

                    foreach (ErrorMappingCamera cam in ErrMappingParam.ErrorMappingCameras)
                    {
                        cam.InitModule();
                    }

                    filePath = this.FileManager().GetSystemRootPath()+@"\VisionMapping";
                    ERROR_DATA_PATH = filePath + @"\ErrorData\";
                    ErrMappingParam.PMParam_X = new PatternInfomation(filePath + @"\Pattern\MapCam_X");
                    ErrMappingParam.PMParam_Y = new PatternInfomation(filePath + @"\Pattern\MapCam_Y");

                    _CurrentMappingCam = ErrMappingParam.ErrorMappingCameras[0];

                    CurrentCamPosX = ErrMappingParam.ErrorMappingCameras[0].CamPos.X.Value;
                    CurrentCamPosY = ErrMappingParam.ErrorMappingCameras[0].CamPos.Y.Value;

                    DefalutSetting();

                    _ErrorCompTable = (FirstErrorTable)MotionManager.ErrorManager.CompensationModule.GetErrorTable();

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
        public void TransferCurrentCam(int index)
        {
            try
            {
            CurCamIndex = index;
            _CurrentMappingCam = ErrMappingParam.ErrorMappingCameras[index];
            CurrentCamPosX = ErrMappingParam.ErrorMappingCameras[index].CamPos.X.Value;
            CurrentCamPosY = ErrMappingParam.ErrorMappingCameras[index].CamPos.Y.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void LoadErrorMappingParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                string ParamFilePath = this.FileManager().GetSystemRootPath() + @"\VisionMapping\VisionMappingParameter.json";
                if (Directory.Exists(System.IO.Path.GetDirectoryName(ParamFilePath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ParamFilePath));
                }

                if (File.Exists(ParamFilePath) == false)
                {
                    this._ErrMappingParam = new ErrorMappingParam();
                    _ErrMappingParam.DefalutSetting();
                    retVal = Extensions_IParam.SaveParameter(null, this._ErrMappingParam, null, ParamFilePath);
                }

                IParam tmpParam = null;
                retVal = this.LoadParameter(ref tmpParam, typeof(ErrorMappingParam),null, ParamFilePath);
                if(retVal == EventCodeEnum.NONE)
                {
                    this.ErrMappingParam = tmpParam as ErrorMappingParam;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("LoadLoaderAxesParameter(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }
        }
        private int LoadErrorMappingXData(int index)
        {
            int retVal = -1;
            try
            {


                string HorXParamPath = String.Format(filePath + @"\ErrorData\VM_X{0}_HOR(X).err", index);
                string VerXParamPath = String.Format(filePath + @"\ErrorData\VM_X{0}_VER(Y).err", index);
                if (Directory.Exists(System.IO.Path.GetDirectoryName(HorXParamPath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(HorXParamPath));
                }

                if (File.Exists(HorXParamPath) == false && File.Exists(VerXParamPath) == false)
                {
                    return retVal;
                }
                ErrorDataXTable[index - 1].ErrorData_HOR.Clear();
                ErrorDataXTable[index - 1].ErrorData_VER.Clear();


                MesureErrorDataRead(HorXParamPath, ErrorDataXTable[index - 1].ErrorData_HOR);

                MesureErrorDataRead(VerXParamPath, ErrorDataXTable[index - 1].ErrorData_VER);


                double[] horPoss;
                if (index % 2 == 1)
                {
                    this.ErrorDataXTable[index - 1].ErrorData_HOR.Reverse();
                    this.ErrorDataXTable[index - 1].ErrorData_VER.Reverse();

                }
                horPoss = new double[this.ErrorDataXTable[index - 1].ErrorData_HOR.Count];
                for (int i = 0; i < this.ErrorDataXTable[index - 1].ErrorData_HOR.Count; i++)
                {
                    horPoss[i] = this.ErrorDataXTable[index - 1].ErrorData_HOR[i].XPos;

                }

                retVal = 0;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("LoadErrorMappingXData(int index): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                retVal = -1;
            }
            retVal = 0;
            return retVal;


        }

        private int LoadErrorMappingYData(int index)
        {
            int retVal = -1;
            try
            {


                string HorXParamPath = String.Format(filePath + @"\ErrorData\VM_Y{0}_HOR(Y).err", index);
                string VerXParamPath = String.Format(filePath + @"\ErrorData\VM_Y{0}_VER(X).err", index);
                if (Directory.Exists(System.IO.Path.GetDirectoryName(HorXParamPath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(HorXParamPath));
                }

                if (File.Exists(HorXParamPath) == false && File.Exists(VerXParamPath) == false)
                {
                    return retVal;
                }
                ErrorDataYTable[index - 1].ErrorData_HOR.Clear();
                ErrorDataYTable[index - 1].ErrorData_VER.Clear();


                MesureErrorDataRead(HorXParamPath, ErrorDataYTable[index - 1].ErrorData_HOR);

                MesureErrorDataRead(VerXParamPath, ErrorDataYTable[index - 1].ErrorData_VER);



                //if (index % 2 == 1)
                //{
                //    this.ErrorDataYTable[index - 1].ErrorData_HOR.Reverse();
                //    this.ErrorDataYTable[index - 1].ErrorData_VER.Reverse();
                //}
                retVal = 0;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("LoadErrorMappingXData(int index): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                retVal = -1;
            }
            retVal = 0;
            return retVal;


        }

        public void SaveErrorMappingParameter()
        {
            try
            {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                string ParamFilePath = this.FileManager().GetSystemRootPath() + @"\VisionMapping\VisionMappingParameter.json";
                retVal = Extensions_IParam.SaveParameter(null, this._ErrMappingParam, null, ParamFilePath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        //public int MeasureErrorDataX()
        //{
        //    int retVal = -1;
        //    try
        //    {
        //        for (int i = 0; i < ErrorDataXTable.Count; i++)
        //        {
        //            retVal = LoadErrorMappingXData(i + 1);
        //            if (retVal != 0) return retVal;
        //        }
        //        for (int i = 0; i < ErrorDataXTable.Count; i++)
        //        {

        //        }


        //    }
        //    catch (Exception err)
        //    {

        //    }
        //    return retVal;
        //}
        //public int MeasureErrorDataY()
        //{
        //    int retVal = 0;

        //    return retVal;
        //}

        public void CurrentMoveToVMPos(int axisNum) //0=> X  , 1=>Y
        {
            try
            {
                VisionManager.StopGrab(PrevCam);
                VisionManager.StartGrab(CurrentMappingCam.CamType, this);

                PrevCam = CurrentMappingCam.CamType;
                CatCoordinates vmPos = null;
            if (axisNum == 0)
            {
                vmPos = CurrentMappingCam.VMPos_X;
            }
            else if (axisNum == 1)
            {
                vmPos = CurrentMappingCam.VMPos_Y;
            }


            if (vmPos.Z.Value >= -3000)
            {
                return;
            }

           MotionManager.StageMove(vmPos.X.Value, vmPos.Y.Value, vmPos.Z.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        EnumProberCam PrevCam = EnumProberCam.MAP_1_CAM;
        public void CurrentMoveToVMPosOPUSV(int axisNum) //0=> X  , 1=>Y
        {
            try
            {
                VisionManager.StopGrab(PrevCam);
                VisionManager.StartGrab(CurrentMappingCam.CamType, this);

                PrevCam = CurrentMappingCam.CamType;
                CatCoordinates vmPos = null;
                if (axisNum == 0)
                {
                    vmPos = CurrentMappingCam.VMPos_X;
                }
                else if (axisNum == 1)
                {
                    vmPos = CurrentMappingCam.VMPos_Y;
                }


                if (vmPos.Z.Value >= -3000)
                {
                    return;
                }

                this.StageSupervisor().StageModuleState.VMViewMove(vmPos.X.Value, vmPos.Y.Value, vmPos.Z.Value);
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }
        public int SaveMesureErrorData(int axisIndex, int camNum)
        {
            int retVal = 0;
            string FilePath_HOR = filePath + @"\ErrorData\";
            string FilePath_VER = filePath + @"\ErrorData\";
            string axisNum = "";

            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(FilePath_HOR)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath_HOR));
                }


                if (axisIndex == 0)
                {
                    axisNum = "VM_X" + (camNum + 1).ToString();
                    FilePath_HOR += axisNum + "_HOR(X).err";
                    FilePath_VER += axisNum + "_VER(Y).err";
                    MesureErrorDataWrite(FilePath_HOR, this.ErrorDataXTable[camNum].ErrorData_HOR);
                    MesureErrorDataWrite(FilePath_VER, this.ErrorDataXTable[camNum].ErrorData_VER);

                }
                else if (axisIndex == 1)
                {
                    axisNum = "VM_Y" + (camNum + 1).ToString();
                    FilePath_HOR += axisNum + "_HOR(Y).err";
                    FilePath_VER += axisNum + "_VER(X).err";
                    MesureErrorDataWrite(FilePath_HOR, this.ErrorDataYTable[camNum].ErrorData_HOR);
                    MesureErrorDataWrite(FilePath_VER, this.ErrorDataYTable[camNum].ErrorData_VER);
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("SaveMesureErrorData(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }
            return retVal;
        }
        public int MesureErrorDataWrite(String path, List<ErrorData> errorData)
        {
            int retVal = -1;
            try
            {
            string line;
            StreamWriter file = new System.IO.StreamWriter(path);
            foreach (ErrorData data in errorData)
            {
                line = string.Format("{0,-15:0.0}  {1,-15:0.0}  {2,-15:0.0}  {3,-15:0.0}  {4,-15:0.0}  {5,-15:0.0}", data.ErrorValue,
                    data.XPos, data.YPos, data.ZPos, data.RelPosX, data.RelPosY);
                file.WriteLine(line);
            }
            file.Close();
            retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public int MakeErrorDataWrite(String path, ObservableCollection<ErrorParameter> errorData)
        {
            int retVal = -1;
            try
            {
            string line;
            StreamWriter file = new System.IO.StreamWriter(path);
            foreach (ErrorParameter data in errorData)
            {
                line = string.Format("{0,-15:0.0}  {1,-15:0.0} ", data.ErrorValue,
                    data.Position);
                file.WriteLine(line);
            }
            file.Close();
            retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public int MesureErrorDataRead(String path, List<ErrorData> errorData)
        {
            int retVal = -1;
            try
            {
            string line;
            double errorValue = 0;
            double xPos = 0;
            double yPos = 0;
            double zPos = 0;
            double relXPos = 0;
            double relYPos = 0;
            int i = 0;

            StreamReader file = new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (double.TryParse(parts[0], out errorValue) == false)
                {
                    break;
                }
                double.TryParse(parts[1], out xPos);
                double.TryParse(parts[2], out yPos);
                double.TryParse(parts[3], out zPos);
                double.TryParse(parts[4], out relXPos);
                double.TryParse(parts[5], out relYPos);
                errorData.Add(new ErrorData());
                errorData[i].ErrorValue = errorValue;
                errorData[i].XPos = xPos;
                errorData[i].YPos = yPos;
                errorData[i].ZPos = zPos;
                errorData[i].RelPosX = relXPos;
                errorData[i].RelPosY = relYPos;
                i++;
            }

            file.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public int SaveMakeErrorDataX()
        {
            int retVal = 0;
            try
            {
            string FilePath_Angular = ERROR_DATA_PATH + "angular0.err";
            string FilePath_Linear = ERROR_DATA_PATH + "linear0.err";
            string FilePath_Straight = ERROR_DATA_PATH + "straight0.err";

            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(FilePath_Angular)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath_Angular));
                }
                MakeErrorDataWrite(FilePath_Angular, ErrorCompTable.TBL_X_ANGULAR);
                MakeErrorDataWrite(FilePath_Linear, ErrorCompTable.TBL_X_LINEAR);
                MakeErrorDataWrite(FilePath_Straight, ErrorCompTable.TBL_X_STRAIGHT);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("SaveMesureErrorData(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public int SaveMakeErrorDataY()
        {
            int retVal = 0;
            string FilePath_Angular = ERROR_DATA_PATH + "angular1.err";
            string FilePath_Linear = ERROR_DATA_PATH + "linear1.err";
            string FilePath_Straight = ERROR_DATA_PATH + "straight1.err";

            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(FilePath_Angular)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath_Angular));
                }
                MakeErrorDataWrite(FilePath_Angular, ErrorCompTable.TBL_Y_ANGULAR);
                MakeErrorDataWrite(FilePath_Linear, ErrorCompTable.TBL_Y_LINEAR);
                MakeErrorDataWrite(FilePath_Straight, ErrorCompTable.TBL_Y_STRAIGHT);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("SaveMesureErrorData(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }
            return retVal;
        }
        public int MakeErrorDataX()
        {
            int retVal = -1;
            try
            {

                for (int i = 1; i <= ErrorDataXTable.Count; i++)
                {
                    retVal = LoadErrorMappingXData(i);
                    if (retVal != 0)
                    {
                        return retVal;
                    }
                }

                //CamNum  (3)   (4)  =>  (1)  (2)
                //        (1)   (2)      (3)  (4)
                int Cam1 = 0;
                int Cam2 = 1;
                int Cam3 = 2;
                int Cam4 = 3;
                double compOff = 0;
                double temp_rad_for_st_low = 0;
                double temp_rad_for_st_high = 0;
                double offSetS = 0;
                double offSetL = 0;
                double offSetA = 0;
                int DataNumX1 = ErrorDataXTable[Cam1].ErrorData_HOR.Count;
                int DataNumX2 = ErrorDataXTable[Cam2].ErrorData_HOR.Count;
                int StartIndexXhigh = 0;
                int EndindexXhigh = DataNumX2 - 1;
                int StartIndexXlow = 0;
                int EndIndexXlow = DataNumX1 - 1;
                MakingErrTable = new MakingErrorTable();
                MakingErrTable.ANGULAR_X_HIGH = new List<double>();
                MakingErrTable.ANGULAR_X_LOW = new List<double>();
                MakingErrTable.STRAIGHT_X_HIGH = new List<double>();
                MakingErrTable.STRAIGHT_X_LOW = new List<double>();
                MakingErrTable.LINEAR_X_HIGH = new List<double>();
                MakingErrTable.LINEAR_X_LOW = new List<double>();
                MakingErrTable.POS_ERROR_X_HIGH = new List<double>();
                MakingErrTable.POS_ERROR_X_Low = new List<double>();
                ErrorCompTable = new FirstErrorTable();
                ErrorCompTable.TBL_X_ANGULAR = new ErrorParamList();
                ErrorCompTable.TBL_X_LINEAR = new ErrorParamList();
                ErrorCompTable.TBL_X_STRAIGHT = new ErrorParamList();

                try
                {

                    for (int i = StartIndexXlow; i <= EndIndexXlow; i++)
                    {
                        MakingErrTable.LINEAR_X_LOW.Add(0);
                        MakingErrTable.POS_ERROR_X_Low.Add(0);
                        MakingErrTable.ANGULAR_X_LOW.Add(0);
                        MakingErrTable.STRAIGHT_X_LOW.Add(0);

                        MakingErrTable.LINEAR_X_LOW[i] =
                            ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.Y.Value) * ErrorDataXTable[Cam1].ErrorData_HOR[i].ErrorValue)
                            + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.Y.Value) * ErrorDataXTable[Cam3].ErrorData_HOR[i].ErrorValue))
                            / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.Y.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.Y.Value));

                        MakingErrTable.POS_ERROR_X_Low[i] = (ErrorDataXTable[Cam1].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam3].ErrorData_HOR[i].XPos) / 2;

                        temp_rad_for_st_low = -(ErrorDataXTable[Cam1].ErrorData_HOR[i].ErrorValue - ErrorDataXTable[Cam3].ErrorData_HOR[i].ErrorValue) /
                            (ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.Y.Value - ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.Y.Value);

                        MakingErrTable.ANGULAR_X_LOW[i] = (temp_rad_for_st_low * 180 * 10000) / Math.PI;

                        MakingErrTable.STRAIGHT_X_LOW[i] = (ErrorDataXTable[Cam1].ErrorData_VER[i].ErrorValue + ErrorDataXTable[Cam3].ErrorData_VER[i].ErrorValue) / 2;
                        //MakingErrTable.STRAIGHT_X_LOW[i] = (MakingErrTable.STRAIGHT_X_LOW[i]
                        //        - (ErrorDataXTable[Cam1].ErrorData_VER[i].RelPosX + ErrorDataXTable[Cam3].ErrorData_VER[i].RelPosX) / 20000 * MakingErrTable.ANGULAR_X_LOW[i] * Math.PI / 180);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"Error occurred while parsing error datas. Err = {err}");
                }

                string line;
                string FilePath_Angular = ERROR_DATA_PATH + "RawAngularLo0.err";
                StreamWriter file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if(MakingErrTable.POS_ERROR_X_Low.Count == MakingErrTable.ANGULAR_X_LOW.Count)
                    {
                        for (int i = 0; i < MakingErrTable.POS_ERROR_X_Low.Count; i++)
                        {
                            line = $"{MakingErrTable.ANGULAR_X_LOW[i],-15:0.00}          {MakingErrTable.POS_ERROR_X_Low[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }

                    //foreach (double data in MakingErrTable.ANGULAR_X_LOW)
                    //{
                    //    line = $"{data}";
                    //    file.WriteLine(line);
                    //}
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
                finally
                {
                    file.Close();
                }

                for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
                {
                    MakingErrTable.LINEAR_X_HIGH.Add(0);
                    MakingErrTable.POS_ERROR_X_HIGH.Add(0);
                    MakingErrTable.ANGULAR_X_HIGH.Add(0);
                    MakingErrTable.STRAIGHT_X_HIGH.Add(0);

                    MakingErrTable.LINEAR_X_HIGH[i] =
                        (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.Y.Value) * ErrorDataXTable[Cam2].ErrorData_HOR[i].ErrorValue
                        + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.Y.Value) * ErrorDataXTable[Cam4].ErrorData_HOR[i].ErrorValue)
                        / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.Y.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.Y.Value));

                    MakingErrTable.POS_ERROR_X_HIGH[i] = (ErrorDataXTable[Cam2].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam4].ErrorData_HOR[i].XPos) / 2;

                    temp_rad_for_st_high = -(ErrorDataXTable[Cam2].ErrorData_HOR[i].ErrorValue - ErrorDataXTable[Cam4].ErrorData_HOR[i].ErrorValue) /
                        (ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.Y.Value - ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.Y.Value);

                    MakingErrTable.ANGULAR_X_HIGH[i] = (temp_rad_for_st_high * 180 * 10000) / Math.PI;

                    MakingErrTable.STRAIGHT_X_HIGH[i] = (ErrorDataXTable[Cam2].ErrorData_VER[i].ErrorValue + ErrorDataXTable[Cam4].ErrorData_VER[i].ErrorValue) / 2;
                    //MakingErrTable.STRAIGHT_X_HIGH[i] = (MakingErrTable.STRAIGHT_X_HIGH[i]
                    //- (ErrorDataXTable[Cam2].ErrorData_VER[i].RelPosX + ErrorDataXTable[Cam4].ErrorData_VER[i].RelPosX) / 20000 * MakingErrTable.ANGULAR_X_HIGH[i] * Math.PI / 180);

                }

                FilePath_Angular = ERROR_DATA_PATH + "RawAngularHi0.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_X_HIGH.Count == MakingErrTable.ANGULAR_X_HIGH.Count)
                    {
                        for (int i = 0; i < MakingErrTable.POS_ERROR_X_HIGH.Count; i++)
                        {
                            line = $"{MakingErrTable.ANGULAR_X_HIGH[i],-15:0.00}          {MakingErrTable.POS_ERROR_X_HIGH[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
                finally
                {
                    file.Close();
                }

                offSetS = MakingErrTable.STRAIGHT_X_LOW[0];
                offSetL = MakingErrTable.LINEAR_X_LOW[0];
                offSetA = MakingErrTable.ANGULAR_X_LOW[0];

                for (int i = StartIndexXlow; i <= EndIndexXlow; i++)
                {
                    MakingErrTable.STRAIGHT_X_LOW[i] -= offSetS;
                    MakingErrTable.ANGULAR_X_LOW[i] -= offSetA;
                    MakingErrTable.LINEAR_X_LOW[i] -= offSetL;
                }

                offSetS = MakingErrTable.STRAIGHT_X_HIGH[0];
                offSetL = MakingErrTable.LINEAR_X_HIGH[0];
                offSetA = MakingErrTable.ANGULAR_X_HIGH[0];

                for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
                {
                    MakingErrTable.STRAIGHT_X_HIGH[i] -= offSetS;
                    MakingErrTable.ANGULAR_X_HIGH[i] -= offSetA;
                    MakingErrTable.LINEAR_X_HIGH[i] -= offSetL;
                }

                int End_Idx_Low = DataNumX1 - 1;
                double End_Pos_Low = ErrorDataXTable[Cam1].ErrorData_HOR[End_Idx_Low].XPos;
                int No_Overlap_High = 0;
                double Overlap_Amount = 0;

                ///check
                for (int i = 0; i < DataNumX2; i++)
                {
                    if (End_Pos_Low < ErrorDataXTable[Cam2].ErrorData_HOR[i].XPos)
                    {
                        No_Overlap_High = i;
                        break;
                    }
                }
                if (No_Overlap_High < 1)
                {
                    //error
                    //메세지 "Can not connect X low and high data"
                    retVal = -1;
                    return retVal;
                }
                Overlap_Amount = (ErrorDataXTable[Cam1].ErrorData_HOR[End_Idx_Low].XPos - ErrorDataXTable[Cam2].ErrorData_HOR[No_Overlap_High - 1].XPos) /
                    (ErrorDataXTable[Cam2].ErrorData_HOR[No_Overlap_High].XPos - ErrorDataXTable[Cam2].ErrorData_HOR[No_Overlap_High - 1].XPos);

                double tmpIntpval_0 = 0;
                double tmpIntpval_1 = 0;

                for (int i = 0; i <= No_Overlap_High - 1; i++)
                {
                    tmpIntpval_0 += MakingErrTable.LINEAR_X_HIGH[i];
                }
                for (int i = End_Idx_Low; i >= End_Idx_Low - No_Overlap_High + 1; i--)
                {
                    tmpIntpval_1 += MakingErrTable.LINEAR_X_LOW[i];
                }


                if (No_Overlap_High > 0)
                {
                    compOff = (tmpIntpval_1 - tmpIntpval_0) / No_Overlap_High;
                }
                else
                {
                    compOff = 0;
                }

                for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
                {
                    MakingErrTable.LINEAR_X_HIGH[i] += compOff;
                }

                for (int i = StartIndexXlow; i <= EndIndexXlow; i++)
                {
                    ErrorCompTable.TBL_X_LINEAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_X_LINEAR[i].ErrorValue = MakingErrTable.LINEAR_X_LOW[i];
                    ErrorCompTable.TBL_X_LINEAR[i].Position = (ErrorDataXTable[Cam1].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam3].ErrorData_HOR[i].XPos) / 2;

                }
                for (int i = No_Overlap_High; i <= EndindexXhigh; i++)
                {
                    ErrorCompTable.TBL_X_LINEAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_X_LINEAR[i + End_Idx_Low - No_Overlap_High + 1].ErrorValue = MakingErrTable.LINEAR_X_HIGH[i];
                    ErrorCompTable.TBL_X_LINEAR[i + End_Idx_Low - No_Overlap_High + 1].Position = (ErrorDataXTable[Cam2].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam4].ErrorData_HOR[i].XPos) / 2;

                }

                tmpIntpval_0 = 0;
                tmpIntpval_1 = 0;

                for (int i = 0; i <= No_Overlap_High - 1; i++)
                {
                    tmpIntpval_0 += MakingErrTable.ANGULAR_X_HIGH[i];
                }
                for (int i = End_Idx_Low; i >= End_Idx_Low - No_Overlap_High + 1; i--)
                {
                    tmpIntpval_1 += MakingErrTable.ANGULAR_X_LOW[i];
                }

                if (No_Overlap_High > 0)
                {
                    compOff = (tmpIntpval_1 - tmpIntpval_0) / No_Overlap_High;
                }
                else
                {
                    compOff = 0;
                }

                for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
                {
                    MakingErrTable.ANGULAR_X_HIGH[i] += compOff;
                }


                var angleDriftOffset = MakingErrTable.ANGULAR_X_LOW[EndIndexXlow] - MakingErrTable.ANGULAR_X_HIGH[No_Overlap_High];
            //for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
            //{
            //    MakingErrTable.ANGULAR_X_HIGH[i] += angleDriftOffset;
            //}
                double partialSum = 0.0;
                double devLow = 0.0;
                double devHigh = 0.0;
                double devDiff = 0.0;
                for (int i = StartIndexXlow; i <= EndIndexXlow - No_Overlap_High; i++)
                {
                    ErrorCompTable.TBL_X_ANGULAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_X_ANGULAR[i].ErrorValue = MakingErrTable.ANGULAR_X_LOW[i];
                    ErrorCompTable.TBL_X_ANGULAR[i].Position = (ErrorDataXTable[Cam1].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam3].ErrorData_HOR[i].XPos) / 2;

                }
                partialSum = 0.0;
                for (int i = EndIndexXlow - No_Overlap_High; i <= EndIndexXlow; i++)
                {
                    partialSum += MakingErrTable.ANGULAR_X_LOW[i];
                }
                devLow = partialSum / (MakingErrTable.POS_ERROR_X_Low[EndIndexXlow - No_Overlap_High] - MakingErrTable.POS_ERROR_X_Low[EndIndexXlow]);
                devLow = partialSum / No_Overlap_High;
                for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
                {
                    ErrorCompTable.TBL_X_ANGULAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_X_ANGULAR[i + End_Idx_Low - No_Overlap_High + 1].ErrorValue = MakingErrTable.ANGULAR_X_HIGH[i];
                    ErrorCompTable.TBL_X_ANGULAR[i + End_Idx_Low - No_Overlap_High + 1].Position = (ErrorDataXTable[Cam2].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam4].ErrorData_HOR[i].XPos) / 2;
                }

                partialSum = 0.0;
                for (int i = StartIndexXhigh; i <= No_Overlap_High; i++)
                {
                    partialSum += MakingErrTable.ANGULAR_X_HIGH[i];
                }
                devHigh = partialSum / (MakingErrTable.POS_ERROR_X_HIGH[StartIndexXhigh] - MakingErrTable.POS_ERROR_X_HIGH[No_Overlap_High]);
                devHigh = partialSum / No_Overlap_High;
                devDiff = devLow - devHigh;
                var compAngleOffset = devDiff;
                for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
                {
                    ErrorCompTable.TBL_X_ANGULAR[i + End_Idx_Low - No_Overlap_High + 1].ErrorValue += compAngleOffset;
                }


                LoggerManager.Debug($"X Low to Mid angle difference = {devDiff}.");
                for (int i = No_Overlap_High; i <= EndindexXhigh; i++)
                {
                    double prevStr = MakingErrTable.STRAIGHT_X_HIGH[i];
                    var strCompByAngleDiff = (MakingErrTable.POS_ERROR_X_HIGH[i] - MakingErrTable.POS_ERROR_X_HIGH[0]) * compAngleOffset * Math.PI / 180.0 / 10000.0;
                    //MakingErrTable.STRAIGHT_X_HIGH[i] -= ((ErrorDataXTable[Cam2].ErrorData_HOR[i].RelPosX + ErrorDataXTable[Cam4].ErrorData_HOR[i].RelPosX) / 2)
                    //    * ErrorCompTable.TBL_X_ANGULAR[i + EndIndexXlow - No_Overlap_High + 1].ErrorValue
                    //    * Math.PI / 180 / 10000;
                    MakingErrTable.STRAIGHT_X_HIGH[i] += strCompByAngleDiff;
                    LoggerManager.Debug($"X Straightness error Mid @X={MakingErrTable.POS_ERROR_X_HIGH[i]} compensated by angle offset = {strCompByAngleDiff,-5:0.0000}: Prev = {prevStr,-5:0.00}, Compensated = {MakingErrTable.STRAIGHT_X_HIGH[i],-5:0.00}");

                }

                tmpIntpval_0 = 0;
                tmpIntpval_1 = 0;

                for (int i = 0; i <= No_Overlap_High - 1; i++)
                {
                    tmpIntpval_0 += MakingErrTable.STRAIGHT_X_HIGH[i];
                }
                for (int i = End_Idx_Low; i >= End_Idx_Low - No_Overlap_High + 1; i--)
                {
                    tmpIntpval_1 += MakingErrTable.STRAIGHT_X_LOW[i];
                }


                if (No_Overlap_High > 0)
                {
                    compOff = (tmpIntpval_1 - tmpIntpval_0) / No_Overlap_High;
                }
                else
                {
                    compOff = 0;
                }
                compOff = compOff - devDiff * Math.PI / 180.0;
                for (int i = StartIndexXhigh; i <= EndindexXhigh; i++)
                {
                    MakingErrTable.STRAIGHT_X_HIGH[i] += compOff;
                }

                FilePath_Angular = ERROR_DATA_PATH + "RawStraightHi0.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_X_HIGH.Count == MakingErrTable.STRAIGHT_X_HIGH.Count)
                    {
                        for (int i = 0; i < MakingErrTable.POS_ERROR_X_HIGH.Count; i++)
                        {
                            line = $"{MakingErrTable.STRAIGHT_X_HIGH[i],-15:0.00}          {MakingErrTable.POS_ERROR_X_HIGH[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
                finally
                {
                    file.Close();
                }

                FilePath_Angular = ERROR_DATA_PATH + "RawStraightLo0.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_X_Low.Count == MakingErrTable.STRAIGHT_X_LOW.Count)
                    {
                        for (int i = 0; i < MakingErrTable.POS_ERROR_X_Low.Count; i++)
                        {
                            line = $"{MakingErrTable.STRAIGHT_X_LOW[i],-15:0.00}          {MakingErrTable.POS_ERROR_X_Low[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
                finally
                {
                    file.Close();
                }

                for (int i = StartIndexXlow; i <= EndIndexXlow; i++)
                {
                    ErrorCompTable.TBL_X_STRAIGHT.Add(new ErrorParameter());
                    ErrorCompTable.TBL_X_STRAIGHT[i].ErrorValue = MakingErrTable.STRAIGHT_X_LOW[i];
                    ErrorCompTable.TBL_X_STRAIGHT[i].Position = (ErrorDataXTable[Cam1].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam3].ErrorData_HOR[i].XPos) / 2;

                }
                for (int i = StartIndexXhigh + No_Overlap_High; i <= EndindexXhigh; i++)
                {
                    ErrorCompTable.TBL_X_STRAIGHT.Add(new ErrorParameter());
                    ErrorCompTable.TBL_X_STRAIGHT[i + End_Idx_Low - No_Overlap_High + 1].ErrorValue = MakingErrTable.STRAIGHT_X_HIGH[i];
                    ErrorCompTable.TBL_X_STRAIGHT[i + End_Idx_Low - No_Overlap_High + 1].Position = (ErrorDataXTable[Cam2].ErrorData_HOR[i].XPos + ErrorDataXTable[Cam4].ErrorData_HOR[i].XPos) / 2;

                }

                int DataUpperBound = DataNumX1 + DataNumX2 - No_Overlap_High - 1;

                double DeviationS = ErrorCompTable.TBL_X_STRAIGHT[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_X_STRAIGHT[0].ErrorValue;
                double DeviationL = ErrorCompTable.TBL_X_LINEAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_X_LINEAR[0].ErrorValue;
                double DeviationA = ErrorCompTable.TBL_X_ANGULAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_X_ANGULAR[0].ErrorValue;

                //for (int i = 0; i <= DataUpperBound; i++)
                //{
                //    ErrorCompTable.TBL_X_LINEAR[i].ErrorValue = ErrorCompTable.TBL_X_LINEAR[i].ErrorValue
                //        - DeviationL *
                //        ((ErrorCompTable.TBL_X_LINEAR[i].Position - ErrorCompTable.TBL_X_LINEAR[0].Position) / (ErrorCompTable.TBL_X_LINEAR[DataUpperBound - 2].Position - ErrorCompTable.TBL_X_LINEAR[0].Position));
                //}
                //for (int i = 0; i <= DataUpperBound; i++)
                //{
                //    ErrorCompTable.TBL_X_STRAIGHT[i].ErrorValue = ErrorCompTable.TBL_X_STRAIGHT[i].ErrorValue
                //        - DeviationS * ((ErrorCompTable.TBL_X_STRAIGHT[i].Position - ErrorCompTable.TBL_X_STRAIGHT[0].Position)
                //        / (ErrorCompTable.TBL_X_STRAIGHT[DataUpperBound - 2].Position - ErrorCompTable.TBL_X_STRAIGHT[0].Position));
                //}
                //for (int i = 0; i <= DataUpperBound; i++)
                //{
                //    ErrorCompTable.TBL_X_ANGULAR[i].ErrorValue = ErrorCompTable.TBL_X_ANGULAR[i].ErrorValue
                //        - DeviationA * ((ErrorCompTable.TBL_X_ANGULAR[i].Position - ErrorCompTable.TBL_X_ANGULAR[0].Position)
                //        / (ErrorCompTable.TBL_X_ANGULAR[DataUpperBound - 2].Position - ErrorCompTable.TBL_X_ANGULAR[0].Position));
                //}
                SaveMakeErrorDataX();
            //_ErrorCompTable.TBL_X_LINEAR.Add()
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public int MakeErrorDataY()
        {
            int retVal = -1;
            try
            {
            for (int i = 1; i <= ErrorDataYTable.Count; i++)
            {
                retVal = LoadErrorMappingYData(i);
                if (retVal != 0)
                {
                    return retVal;
                }
            }

            int Cam1 = 0;
            int Cam2 = 1;
            int Cam3 = 2;
            int Cam4 = 3;
            int Cam5 = 4;
            int Cam6 = 5;
            int Cam7 = 6;
            int Cam8 = 7;

            int DataNumY1 = ErrorDataYTable[Cam1].ErrorData_HOR.Count;
            int DataNumY2 = ErrorDataYTable[Cam2].ErrorData_HOR.Count;
            int DataNumY3 = ErrorDataYTable[Cam3].ErrorData_HOR.Count;
            int DataNumY4 = ErrorDataYTable[Cam4].ErrorData_HOR.Count;
            int DataNumY5 = ErrorDataYTable[Cam5].ErrorData_HOR.Count;
            int DataNumY6 = ErrorDataYTable[Cam6].ErrorData_HOR.Count;
            int DataNumY7 = ErrorDataYTable[Cam7].ErrorData_HOR.Count;
            int DataNumY8 = ErrorDataYTable[Cam8].ErrorData_HOR.Count;


            if(DataNumY1>= DataNumY2)
            {
                DataNumY1 = DataNumY2;
            }
            if (DataNumY3 >= DataNumY4)
            {
                DataNumY3 = DataNumY4;
            }
            if (DataNumY5 >= DataNumY6)
            {
                DataNumY5 = DataNumY6;
            }
            if (DataNumY7 >= DataNumY8)
            {
                DataNumY7 = DataNumY8;
            }

            //' [ 1. Get linear/angular/straight from each data ]
            int StartIndexYlow = 0;
            int EndIndexYlow = DataNumY1 - 1;
            int StartIndexYmid = 0;
            int EndIndexYmid = DataNumY3 - 1;
            int StartIndexYhigh = 0;
            int EndIndexYhigh = DataNumY5 - 1;
            int StartIndexYmid2 = 0;
            int EndIndexYmid2 = DataNumY7 - 1;

            double temp_rad_for_st_low = 0;
            double temp_rad_for_st_mid = 0;
            double temp_rad_for_st_mid2 = 0;
            double temp_rad_for_st_high = 0;
            double[] tmpintpval = new double[20];
            double CompOff = 0;
            ErrorCompTable.TBL_Y_LINEAR = new ErrorParamList();
            ErrorCompTable.TBL_Y_STRAIGHT = new ErrorParamList();
            ErrorCompTable.TBL_Y_ANGULAR = new ErrorParamList();
            for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            {
                MakingErrTable.LINEAR_Y_LOW.Add(0);
                MakingErrTable.ANGULAR_Y_LOW.Add(0);
                MakingErrTable.STRAIGHT_Y_LOW.Add(0);


                MakingErrTable.LINEAR_Y_LOW[i] = ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.X.Value) * ErrorDataYTable[Cam1].ErrorData_VER[i].ErrorValue)
                    + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.X.Value) * ErrorDataYTable[Cam2].ErrorData_VER[i].ErrorValue))
                    / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.X.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.X.Value));

                temp_rad_for_st_low = -(((ErrorDataYTable[Cam2].ErrorData_VER[i].ErrorValue - ErrorDataYTable[Cam1].ErrorData_VER[i].ErrorValue))
                    / (ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.X.Value - ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.X.Value));
                MakingErrTable.ANGULAR_Y_LOW[i] = temp_rad_for_st_low * 180 * 10000 / Math.PI;

                MakingErrTable.STRAIGHT_Y_LOW[i] = (ErrorDataYTable[Cam1].ErrorData_HOR[i].ErrorValue + ErrorDataYTable[Cam2].ErrorData_HOR[i].ErrorValue) / 2;
                MakingErrTable.STRAIGHT_Y_LOW[i] = (MakingErrTable.STRAIGHT_Y_LOW[i] + ((ErrorDataYTable[Cam1].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam2].ErrorData_HOR[i].RelPosY) / 20000)
                    * MakingErrTable.ANGULAR_Y_LOW[i] * Math.PI / 180);

            }

            for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
            {
                MakingErrTable.LINEAR_Y_MID.Add(0);
                MakingErrTable.ANGULAR_Y_MID.Add(0);
                MakingErrTable.STRAIGHT_Y_MID.Add(0);



                MakingErrTable.LINEAR_Y_MID[i] = ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.X.Value) * ErrorDataYTable[Cam3].ErrorData_VER[i].ErrorValue)
                 + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.X.Value) * ErrorDataYTable[Cam4].ErrorData_VER[i].ErrorValue))
                 / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.X.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.X.Value));



                temp_rad_for_st_mid = -(((ErrorDataYTable[Cam4].ErrorData_VER[i].ErrorValue - ErrorDataYTable[Cam3].ErrorData_VER[i].ErrorValue))
                / (ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.X.Value - ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.X.Value));

                MakingErrTable.ANGULAR_Y_MID[i] = temp_rad_for_st_mid * 180 * 10000 / Math.PI;

                MakingErrTable.STRAIGHT_Y_MID[i] = (ErrorDataYTable[Cam3].ErrorData_HOR[i].ErrorValue + ErrorDataYTable[Cam4].ErrorData_HOR[i].ErrorValue) / 2;

                MakingErrTable.STRAIGHT_Y_MID[i] = (MakingErrTable.STRAIGHT_Y_MID[i] + ((ErrorDataYTable[Cam3].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam4].ErrorData_HOR[i].RelPosY) / 20000)
                 * MakingErrTable.ANGULAR_Y_MID[i] * Math.PI / 180);



            }
            for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            {
                MakingErrTable.LINEAR_Y_HIGH.Add(0);
                MakingErrTable.ANGULAR_Y_HIGH.Add(0);
                MakingErrTable.STRAIGHT_Y_HIGH.Add(0);




                MakingErrTable.LINEAR_Y_HIGH[i] = ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam6].CamPos.X.Value) * ErrorDataYTable[Cam5].ErrorData_VER[i].ErrorValue)
               + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam5].CamPos.X.Value) * ErrorDataYTable[Cam6].ErrorData_VER[i].ErrorValue))
               / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam5].CamPos.X.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam6].CamPos.X.Value));

                temp_rad_for_st_high = -((ErrorDataYTable[Cam6].ErrorData_VER[i].ErrorValue - ErrorDataYTable[Cam5].ErrorData_VER[i].ErrorValue))
                / (ErrMappingParam.ErrorMappingCameras[Cam5].CamPos.X.Value - ErrMappingParam.ErrorMappingCameras[Cam6].CamPos.X.Value);

                MakingErrTable.ANGULAR_Y_HIGH[i] = temp_rad_for_st_high * 180 * 10000 / Math.PI;

                MakingErrTable.STRAIGHT_Y_HIGH[i] = (ErrorDataYTable[Cam5].ErrorData_HOR[i].ErrorValue + ErrorDataYTable[Cam6].ErrorData_HOR[i].ErrorValue) / 2;

                MakingErrTable.STRAIGHT_Y_HIGH[i] = (MakingErrTable.STRAIGHT_Y_HIGH[i] + ((ErrorDataYTable[Cam5].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam6].ErrorData_HOR[i].RelPosY) / 20000)
             * MakingErrTable.ANGULAR_Y_HIGH[i] * Math.PI / 180);



            }


            for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
            {
                MakingErrTable.LINEAR_Y_MID2.Add(0);
                MakingErrTable.ANGULAR_Y_MID2.Add(0);
                MakingErrTable.STRAIGHT_Y_MID2.Add(0);


                MakingErrTable.LINEAR_Y_MID2[i] = ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam8].CamPos.X.Value) * ErrorDataYTable[Cam7].ErrorData_VER[i].ErrorValue)
            + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam7].CamPos.X.Value) * ErrorDataYTable[Cam8].ErrorData_VER[i].ErrorValue))
            / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam7].CamPos.X.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam7].CamPos.X.Value));

                temp_rad_for_st_mid2 = -(((ErrorDataYTable[Cam8].ErrorData_VER[i].ErrorValue - ErrorDataYTable[Cam7].ErrorData_VER[i].ErrorValue))
                / (ErrMappingParam.ErrorMappingCameras[Cam7].CamPos.X.Value - ErrMappingParam.ErrorMappingCameras[Cam8].CamPos.X.Value));

                MakingErrTable.ANGULAR_Y_MID2[i] = temp_rad_for_st_mid2 * 180 * 10000 / Math.PI;

                MakingErrTable.STRAIGHT_Y_MID2[i] = (ErrorDataYTable[Cam7].ErrorData_HOR[i].ErrorValue + ErrorDataYTable[Cam8].ErrorData_HOR[i].ErrorValue) / 2;


                MakingErrTable.STRAIGHT_Y_MID2[i] = (MakingErrTable.STRAIGHT_Y_MID2[i] + ((ErrorDataYTable[Cam7].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam8].ErrorData_HOR[i].RelPosY) / 20000)
            * MakingErrTable.ANGULAR_Y_MID2[i] * Math.PI / 180);


            }




            int End_Idx_Low = 0;
            double End_Pos_Low = 0;

            int End_Idx_Mid = 0;
            double End_Pos_Mid = 0;

            int End_Idx_Mid2 = 0;
            double End_Pos_Mid2 = 0;

            int UnOvlpNum_Mid = 0;
            int UnOvlpNum_Mid2 = 0;
            int UnOvlpNum_High = 0;

            double Low2Mid_Overlap_Amount = 0;
            double Mid2High_Overlap_Amount = 0;
            double Mid2Mid_Overlap_Amount = 0;

            double[] Low2Mid_IntpVal = new double[100];
            double[] Mid2High_IntpVal = new double[100];
            double[] Mid2Mid_IntpVal = new double[100];


            double OffsetS = MakingErrTable.STRAIGHT_Y_LOW[0];
            double OffsetL = MakingErrTable.LINEAR_Y_LOW[0];
            double OffsetA = MakingErrTable.ANGULAR_Y_LOW[0];

            for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            {
                MakingErrTable.STRAIGHT_Y_LOW[i] = MakingErrTable.STRAIGHT_Y_LOW[i] - OffsetS;
                MakingErrTable.LINEAR_Y_LOW[i] = MakingErrTable.LINEAR_Y_LOW[i] - OffsetL;
                MakingErrTable.ANGULAR_Y_LOW[i] = MakingErrTable.ANGULAR_Y_LOW[i] - OffsetA;
            }

            OffsetS = MakingErrTable.STRAIGHT_Y_MID[0];
            OffsetL = MakingErrTable.LINEAR_Y_MID[0];
            OffsetA = MakingErrTable.ANGULAR_Y_MID[0];

            for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
            {
                MakingErrTable.STRAIGHT_Y_MID[i] = MakingErrTable.STRAIGHT_Y_MID[i] - OffsetS;
                MakingErrTable.LINEAR_Y_MID[i] = MakingErrTable.LINEAR_Y_MID[i] - OffsetL;
                MakingErrTable.ANGULAR_Y_MID[i] = MakingErrTable.ANGULAR_Y_MID[i] - OffsetA;
            }

            OffsetS = MakingErrTable.STRAIGHT_Y_MID2[0];
            OffsetL = MakingErrTable.LINEAR_Y_MID2[0];
            OffsetA = MakingErrTable.ANGULAR_Y_MID2[0];

            for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
            {
                MakingErrTable.STRAIGHT_Y_MID2[i] = MakingErrTable.STRAIGHT_Y_MID2[i] - OffsetS;
                MakingErrTable.LINEAR_Y_MID2[i] = MakingErrTable.LINEAR_Y_MID2[i] - OffsetL;
                MakingErrTable.ANGULAR_Y_MID2[i] = MakingErrTable.ANGULAR_Y_MID2[i] - OffsetA;
            }

            OffsetS = MakingErrTable.STRAIGHT_Y_HIGH[0];
            OffsetL = MakingErrTable.LINEAR_Y_HIGH[0];
            OffsetA = MakingErrTable.ANGULAR_Y_HIGH[0];

            for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            {
                MakingErrTable.STRAIGHT_Y_HIGH[i] = MakingErrTable.STRAIGHT_Y_HIGH[i] - OffsetS;
                MakingErrTable.LINEAR_Y_HIGH[i] = MakingErrTable.LINEAR_Y_HIGH[i] - OffsetL;
                MakingErrTable.ANGULAR_Y_HIGH[i] = MakingErrTable.ANGULAR_Y_HIGH[i] - OffsetA;
            }
            End_Idx_Low = DataNumY1 - 1;
            End_Pos_Low = (ErrorDataYTable[Cam1].ErrorData_VER[End_Idx_Low].YPos + ErrorDataYTable[Cam2].ErrorData_VER[End_Idx_Low].YPos) / 2;




            for (int i = 0; i <= DataNumY3; i++)
            {
                if (End_Pos_Low < (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2)
                {
                    UnOvlpNum_Mid = i;
                    break;
                }
            }
            if (UnOvlpNum_Mid < 1)
            {
                retVal = -1;
                return retVal;
            }

            Low2Mid_Overlap_Amount = ((ErrorDataYTable[Cam1].ErrorData_VER[End_Idx_Low].YPos - ErrorDataYTable[Cam3].ErrorData_VER[UnOvlpNum_Mid - 1].YPos)
                                   / (ErrorDataYTable[Cam3].ErrorData_VER[UnOvlpNum_Mid].YPos - ErrorDataYTable[Cam3].ErrorData_VER[UnOvlpNum_Mid - 1].YPos));

            End_Idx_Mid = DataNumY3 - 1;
            End_Pos_Mid = (ErrorDataYTable[Cam3].ErrorData_VER[End_Idx_Mid].YPos + ErrorDataYTable[Cam4].ErrorData_VER[End_Idx_Mid].YPos) / 2;

            for (int i = 0; i <= DataNumY7; i++)
            {
                if (End_Pos_Mid < (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2)
                {
                    UnOvlpNum_Mid2 = i;
                    break;
                }
            }
            if (UnOvlpNum_Mid2 < 1)
            {
                retVal = -1;
                return retVal;
            }

            Mid2Mid_Overlap_Amount = ((ErrorDataYTable[Cam3].ErrorData_VER[End_Idx_Mid].YPos - ErrorDataYTable[Cam7].ErrorData_VER[UnOvlpNum_Mid2 - 1].YPos)
                                   / (ErrorDataYTable[Cam7].ErrorData_VER[UnOvlpNum_Mid2].YPos - ErrorDataYTable[Cam7].ErrorData_VER[UnOvlpNum_Mid2 - 1].YPos));


            //[ 2-3. Calculate linear compensation offset of Low to Mid range ]

            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.LINEAR_Y_MID[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Low; i >= End_Idx_Low - UnOvlpNum_Mid + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.LINEAR_Y_LOW[i];
            }

            if (UnOvlpNum_Mid > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid;
            }
            else
            {
                CompOff = 0;
            }

            for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
            {
                MakingErrTable.LINEAR_Y_MID[i] = MakingErrTable.LINEAR_Y_MID[i] + CompOff;
            }


            /////
            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.LINEAR_Y_MID2[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Mid; i >= End_Idx_Mid - UnOvlpNum_Mid2 + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.LINEAR_Y_MID[i];
            }

            if (UnOvlpNum_Mid2 > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid2;
            }
            else
            {
                CompOff = 0;
            }
            for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
            {
                MakingErrTable.LINEAR_Y_MID2[i] = MakingErrTable.LINEAR_Y_MID2[i] + CompOff;
            }
            //' [ 2-5. Glue each data for Linear1 ]
            int DataNum_Tot = 0;

            for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            {

                ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = MakingErrTable.LINEAR_Y_LOW[i];
                ErrorCompTable.TBL_Y_LINEAR[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }
            for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
            {
                ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.LINEAR_Y_MID[i];
                ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }
            for (int i = UnOvlpNum_Mid2; i <= EndIndexYmid2; i++)
            {
                ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].ErrorValue = MakingErrTable.LINEAR_Y_MID2[i];
                ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].Position = (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }




            End_Idx_Mid2 = DataNumY7 - 1;
            End_Pos_Mid2 = (ErrorDataYTable[Cam7].ErrorData_VER[End_Idx_Mid2].YPos + ErrorDataYTable[Cam8].ErrorData_VER[End_Idx_Mid2].YPos) / 2;



            for (int i = 0; i <= DataNum_Tot; i++)
            {
                if (End_Pos_Mid2 < (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2)
                {
                    UnOvlpNum_High = i;
                    break;
                }
            }
            if (UnOvlpNum_High < 1)
            {
                retVal = -1;
                return retVal;
            }

            Mid2High_Overlap_Amount = ((ErrorCompTable.TBL_Y_LINEAR[End_Idx_Mid2].Position - ErrorDataYTable[Cam5].ErrorData_VER[UnOvlpNum_High - 1].YPos)
                                   / (ErrorDataYTable[Cam5].ErrorData_VER[UnOvlpNum_High].YPos - ErrorDataYTable[Cam5].ErrorData_VER[UnOvlpNum_High - 1].YPos));



            //' [ 2-7. Calculate linear compensation offset of Low + Mid to High range ]
            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.LINEAR_Y_HIGH[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Mid2; i >= End_Idx_Mid2 - UnOvlpNum_High + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.LINEAR_Y_MID2[i];
            }

            if (UnOvlpNum_High > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_High;
            }
            else
            {
                CompOff = 0;
            }


            for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            {
                MakingErrTable.LINEAR_Y_HIGH[i] = MakingErrTable.LINEAR_Y_HIGH[i] + CompOff;
            }


            // ' [ 2-8. Glue each data for Linear1 ]
            int j = 0;
            for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
            {
                ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_LINEAR[DataNum_Tot + j].ErrorValue = MakingErrTable.LINEAR_Y_HIGH[i];
                ErrorCompTable.TBL_Y_LINEAR[DataNum_Tot + j].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                j++;
            }

            //'-- [   ANGULAR CONNECT  ] --

            // ' [ 3-1. Calculate angular compensation offset of Low to Mid range ]

            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.ANGULAR_Y_MID[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Low; i >= End_Idx_Low - UnOvlpNum_Mid + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.ANGULAR_Y_LOW[i];
            }

            if (UnOvlpNum_Mid > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid;
            }
            else
            {
                CompOff = 0;
            }


            for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
            {
                MakingErrTable.ANGULAR_Y_MID[i] = MakingErrTable.ANGULAR_Y_MID[i] + CompOff;
            }

            //' [ 3-2. Calculate angular compensation offset of Mid to mid2 range ]


            /////
            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.ANGULAR_Y_MID2[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Mid; i >= End_Idx_Mid - UnOvlpNum_Mid2 + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.ANGULAR_Y_MID[i];
            }

            if (UnOvlpNum_Mid2 > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid2;
            }
            else
            {
                CompOff = 0;
            }

            for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
            {
                MakingErrTable.ANGULAR_Y_MID2[i] = MakingErrTable.ANGULAR_Y_MID2[i] + CompOff;
            }

            //   ' [ 3-3. Glue each data for angular1 ]


            DataNum_Tot = 0;

            for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            {

                ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue = MakingErrTable.ANGULAR_Y_LOW[i];
                ErrorCompTable.TBL_Y_ANGULAR[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }
            for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
            {
                ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.ANGULAR_Y_MID[i];
                ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }
            for (int i = UnOvlpNum_Mid2; i <= EndIndexYmid2; i++)
            {
                ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].ErrorValue = MakingErrTable.ANGULAR_Y_MID2[i];
                ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].Position = (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }

            //' [ 3-5. Calculate angular compensation offset of Mid2 to High range ]

            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.ANGULAR_Y_HIGH[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Mid2; i >= End_Idx_Mid2 - UnOvlpNum_High + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.ANGULAR_Y_MID2[i];
            }

            if (UnOvlpNum_High > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_High;
            }
            else
            {
                CompOff = 0;
            }


            for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            {
                MakingErrTable.ANGULAR_Y_HIGH[i] = MakingErrTable.ANGULAR_Y_HIGH[i] + CompOff;
            }


            // ' [ 3-6. Glue each data for angular1 ]
            j = 0;
            for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
            {
                ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_ANGULAR[DataNum_Tot + j].ErrorValue = MakingErrTable.ANGULAR_Y_HIGH[i];
                ErrorCompTable.TBL_Y_ANGULAR[DataNum_Tot + j].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                j++;
            }

            //' [ 4-1. Calculate straightness compensation offset of Low to Mid range ]

            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.STRAIGHT_Y_MID[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Low; i >= End_Idx_Low - UnOvlpNum_Mid + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.STRAIGHT_Y_LOW[i];
            }

            if (UnOvlpNum_Mid > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid;
            }
            else
            {
                CompOff = 0;
            }


            for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
            {
                MakingErrTable.STRAIGHT_Y_MID[i] = MakingErrTable.STRAIGHT_Y_MID[i] + CompOff;
            }

            //' [ 4-2. Calculate straightness compensation offset of Mid to mid2 range ]


            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.STRAIGHT_Y_MID2[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Mid; i >= End_Idx_Mid - UnOvlpNum_Mid2 + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.STRAIGHT_Y_MID[i];
            }

            if (UnOvlpNum_Mid2 > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid2;
            }
            else
            {
                CompOff = 0;
            }

            for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
            {
                MakingErrTable.STRAIGHT_Y_MID2[i] = MakingErrTable.STRAIGHT_Y_MID2[i] + CompOff;
            }

            // [ 4-3. Glue each data for straightness1 ]


            DataNum_Tot = 0;

            for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            {

                ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = MakingErrTable.STRAIGHT_Y_LOW[i];
                ErrorCompTable.TBL_Y_STRAIGHT[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }
            for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
            {
                ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.STRAIGHT_Y_MID[i];
                ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }
            for (int i = UnOvlpNum_Mid2; i <= EndIndexYmid2; i++)
            {
                ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].ErrorValue = MakingErrTable.STRAIGHT_Y_MID2[i];
                ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].Position = (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2;
                DataNum_Tot++;
            }


            //' [ 4-5. Calculate straightness compensation offset of Low + Mid to High range ]

            tmpintpval[0] = 0;
            for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            {
                tmpintpval[0] = tmpintpval[0] + MakingErrTable.STRAIGHT_Y_HIGH[i];
            }
            tmpintpval[1] = 0;

            for (int i = End_Idx_Mid2; i >= End_Idx_Mid2 - UnOvlpNum_High + 1; i--)
            {
                tmpintpval[1] = tmpintpval[1] + MakingErrTable.STRAIGHT_Y_MID2[i];
            }

            if (UnOvlpNum_High > 0)
            {
                CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_High;
            }
            else
            {
                CompOff = 0;
            }


            for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            {
                MakingErrTable.STRAIGHT_Y_HIGH[i] = MakingErrTable.STRAIGHT_Y_HIGH[i] + CompOff;
            }



            //  ' [ 4-6. Glue each data for straightness1 ]
            j = 0;
            for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
            {
                ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                ErrorCompTable.TBL_Y_STRAIGHT[DataNum_Tot + j].ErrorValue = MakingErrTable.STRAIGHT_Y_HIGH[i];
                ErrorCompTable.TBL_Y_STRAIGHT[DataNum_Tot + j].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                j++;
            }

            DataNum_Tot = DataNum_Tot + j - 1;

            int DataUpperBound = DataNum_Tot;

            double DeviationL = ErrorCompTable.TBL_Y_LINEAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_LINEAR[0].ErrorValue;
            double deviationZ = ErrorCompTable.TBL_Y_ANGULAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_ANGULAR[0].ErrorValue;
            double deviationS = ErrorCompTable.TBL_Y_STRAIGHT[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_STRAIGHT[0].ErrorValue;

            for (int i = 0; i <= DataUpperBound; i++)
            {
                ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue
                    - deviationS * ((ErrorCompTable.TBL_Y_STRAIGHT[i].Position - ErrorCompTable.TBL_Y_STRAIGHT[0].Position)
                    / (ErrorCompTable.TBL_Y_STRAIGHT[DataUpperBound - 2].Position - ErrorCompTable.TBL_Y_STRAIGHT[0].Position));
            }

            #region //MakeErrorData_Y_EX
            //  for (int i=0;i<=UnOvlpNum_Mid-1;i++)
            //  {
            //      Low2Mid_IntpVal[i] = MakingErrTable.ZangleYMid[i] + (MakingErrTable.ZangleYMid[i + 1] - MakingErrTable.ZangleYMid[i]) * Low2Mid_Overlap_Amount;
            //  }

            //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
            //  {
            //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.ZangleYLow[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_Mid;

            //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
            //  {
            //      MakingErrTable.ZangleYLow[i] = MakingErrTable.ZangleYLow[i] + CompOff;
            //  }
            //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
            //  {
            //      Low2Mid_IntpVal[i] = MakingErrTable.ZheightYMid[i] + (MakingErrTable.ZheightYMid[i + 1] - MakingErrTable.ZheightYMid[i]) * Low2Mid_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
            //  {
            //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.ZheightYLow[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_Mid;
            //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
            //  {
            //      MakingErrTable.ZheightYLow[i] = MakingErrTable.ZheightYLow[i] + CompOff;
            //  }

            //  //////////////////////////////////////////////////////////////////

            //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            //  {
            //      Mid2Mid_IntpVal[i] = MakingErrTable.ZangleYMid2[i] + (MakingErrTable.ZangleYMid2[i + 1] - MakingErrTable.ZangleYMid2[i]) * Mid2Mid_Overlap_Amount;
            //  }
            //CompOff = 0;

            //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2Mid_IntpVal[i] - MakingErrTable.ZangleYMid2[i + (DataNumY3 - 1) - (UnOvlpNum_Mid2 - 1)]);
            //  }

            //  CompOff = CompOff / UnOvlpNum_Mid2;

            //  for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
            //  {
            //      MakingErrTable.ZangleYMid2[i] = MakingErrTable.ZangleYMid2[i] - CompOff;
            //  }
            //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            //  {
            //      Mid2Mid_IntpVal[i] = MakingErrTable.ZheightYMid2[i] + (MakingErrTable.ZheightYMid2[i + 1] - MakingErrTable.ZheightYMid2[i]) * Mid2Mid_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2Mid_IntpVal[i] - MakingErrTable.ZheightYMid2[i + (DataNumY3 - 1) - (UnOvlpNum_Mid2 - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_Mid2;
            //  for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
            //  {
            //      MakingErrTable.ZheightYMid2[i] = MakingErrTable.ZheightYMid2[i] - CompOff;
            //  }


            //  //[ 2-6. Calculate linear compensation offset of Mid2 to High range ]

            //  for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            //  {

            //      ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = MakingErrTable.LINEAR_Y_LOW[i];
            //      ErrorCompTable.TBL_Y_LINEAR[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
            //  {
            //      ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.LINEAR_Y_MID[i];
            //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
            //  {
            //      ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].ErrorValue = MakingErrTable.LINEAR_Y_HIGH[i];
            //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
            //  }


            //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
            //  {
            //      Mid2High_IntpVal[i] = MakingErrTable.ZangleYHigh[i] + (MakingErrTable.ZangleYHigh[i + 1] - MakingErrTable.ZangleYHigh[i]) * Mid2High_Overlap_Amount;
            //  }
            //  CompOff = 0;

            //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.ZangleYHigh[i + (DataNumY7 - 1) - (UnOvlpNum_High - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_High;

            //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            //  {
            //      MakingErrTable.ZangleYHigh[i] = MakingErrTable.ZangleYHigh[i] - CompOff;
            //  }
            //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
            //  {
            //      Mid2High_IntpVal[i] = MakingErrTable.ZheightYHigh[i] + (MakingErrTable.ZheightYHigh[i + 1] - MakingErrTable.ZheightYHigh[i]) * Mid2High_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.ZheightYHigh[i + (DataNumY7 - 1) - (UnOvlpNum_High - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_High;
            //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            //  {
            //      MakingErrTable.ZheightYHigh[i] = MakingErrTable.ZheightYHigh[i] - CompOff;
            //  }
            //  //[ 2-7. Glue each data for Z ]

            //  for (int i=StartIndexYlow;i<=EndIndexYlow;i++)
            //  {
            //      MakingErrTable.Zangle1.Add(0);
            //      MakingErrTable.Zangle1pos.Add(0);
            //      MakingErrTable.Zheight1.Add(0);
            //      MakingErrTable.Zheight1pos.Add(0);

            //      MakingErrTable.Zangle1[i] = MakingErrTable.ZangleYLow[i];
            //      MakingErrTable.Zangle1pos[i] = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
            //      MakingErrTable.Zheight1[i] = MakingErrTable.ZheightYLow[i];
            //      MakingErrTable.Zheight1pos[i] = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
            //  {
            //      MakingErrTable.Zangle1.Add(0);
            //      MakingErrTable.Zangle1pos.Add(0);
            //      MakingErrTable.Zheight1.Add(0);
            //      MakingErrTable.Zheight1pos.Add(0);

            //      MakingErrTable.Zangle1[i+EndIndexYlow-UnOvlpNum_Mid+1] = MakingErrTable.ZangleYMid[i];
            //      MakingErrTable.Zangle1pos[i + EndIndexYlow - UnOvlpNum_Mid + 1] = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
            //      MakingErrTable.Zheight1[i + EndIndexYlow - UnOvlpNum_Mid + 1] = MakingErrTable.ZheightYMid[i];
            //      MakingErrTable.Zheight1pos[i + EndIndexYlow - UnOvlpNum_Mid + 1] = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_Mid2; i <= EndIndexYmid2; i++)
            //  {
            //      MakingErrTable.Zangle1.Add(0);
            //      MakingErrTable.Zangle1pos.Add(0);
            //      MakingErrTable.Zheight1.Add(0);
            //      MakingErrTable.Zheight1pos.Add(0);

            //      MakingErrTable.Zangle1[i + EndIndexYlow+ EndIndexYmid-UnOvlpNum_Mid-UnOvlpNum_Mid2+2] = MakingErrTable.ZangleYMid2[i];
            //      MakingErrTable.Zangle1pos[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2] = (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2;
            //      MakingErrTable.Zheight1[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2] = MakingErrTable.ZheightYMid2[i];
            //      MakingErrTable.Zheight1pos[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2] = (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
            //  {
            //      MakingErrTable.Zangle1.Add(0);
            //      MakingErrTable.Zangle1pos.Add(0);
            //      MakingErrTable.Zheight1.Add(0);
            //      MakingErrTable.Zheight1pos.Add(0);

            //      MakingErrTable.Zangle1[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2-UnOvlpNum_Mid-UnOvlpNum_Mid2-UnOvlpNum_High+ 3] = MakingErrTable.ZangleYHigh[i];
            //      MakingErrTable.Zangle1pos[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2 - UnOvlpNum_Mid - UnOvlpNum_Mid2 - UnOvlpNum_High + 3] = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
            //      MakingErrTable.Zheight1[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2 - UnOvlpNum_Mid - UnOvlpNum_Mid2 - UnOvlpNum_High + 3] = MakingErrTable.ZheightYHigh[i];
            //      MakingErrTable.Zheight1pos[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2 - UnOvlpNum_Mid - UnOvlpNum_Mid2 - UnOvlpNum_High + 3] = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
            //  }
            //  //  [ 2-8. Calculate linear compensation offset of Low to Mid range ]

            //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
            //  {
            //      Low2Mid_IntpVal[i] = MakingErrTable.LINEAR_Y_MID[i] + (MakingErrTable.LINEAR_Y_MID[i + 1] - MakingErrTable.LINEAR_Y_MID[i]) * Low2Mid_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
            //  {
            //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.LINEAR_Y_LOW[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_Mid;
            //  for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            //  {
            //      MakingErrTable.LINEAR_Y_LOW[i] = MakingErrTable.LINEAR_Y_LOW[i] + CompOff;
            //  }

            //  //[ 2-9. Calculate linear compensation offset of Mid to Mid2 range ]

            //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            //  {
            //      Mid2Mid_IntpVal[i] = MakingErrTable.LINEAR_Y_MID2[i] + (MakingErrTable.LINEAR_Y_MID2[i + 1] - MakingErrTable.LINEAR_Y_MID2[i]) * Mid2Mid_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2Mid_IntpVal[i] - MakingErrTable.LINEAR_Y_MID[i + (DataNumY3 - 1) - (UnOvlpNum_Mid2 - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_Mid2;
            //  for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
            //  {
            //      MakingErrTable.LINEAR_Y_MID[i] = MakingErrTable.LINEAR_Y_MID[i] - CompOff;
            //  }

            //  //[ 2-10. Calculate linear compensation offset of Mid2 to High range ]
            //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            //  {
            //      Mid2High_IntpVal[i] = MakingErrTable.LINEAR_Y_HIGH[i] + (MakingErrTable.LINEAR_Y_HIGH[i + 1] - MakingErrTable.LINEAR_Y_HIGH[i]) * Mid2High_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.LINEAR_Y_MID[i + (DataNumY7 - 1) - (UnOvlpNum_High - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_High;
            //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            //  {
            //      MakingErrTable.LINEAR_Y_HIGH[i] = MakingErrTable.LINEAR_Y_HIGH[i] - CompOff;
            //  }




            // // ' [ 3. Link each Angular data ]
            // //' [ 3-1. Calculate angular compensation offset of Low to Mid range ]
            // for(int i=0;i<=UnOvlpNum_Mid-1;i++)
            //  {
            //      Low2Mid_IntpVal[i] = MakingErrTable.ANGULAR_Y_MID[i] + (MakingErrTable.ANGULAR_Y_MID[i + 1] - MakingErrTable.ANGULAR_Y_MID[i]) * Low2Mid_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
            //  {
            //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.LINEAR_Y_LOW[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_Mid;
            //  for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
            //  {
            //      MakingErrTable.LINEAR_Y_LOW[i] = MakingErrTable.LINEAR_Y_LOW[i] + CompOff;
            //  }

            //  //[ 3-2. Calculate angular compensation offset of Mid to High range ]
            //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            //  {
            //      Mid2High_IntpVal[i] = MakingErrTable.ANGULAR_Y_HIGH[i] + (MakingErrTable.ANGULAR_Y_HIGH[i + 1] - MakingErrTable.ANGULAR_Y_HIGH[i]) * Mid2High_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.ANGULAR_Y_MID[i + (DataNumY3 - 1) - (UnOvlpNum_High - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_High;
            //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            //  {
            //      MakingErrTable.ANGULAR_Y_HIGH[i] = MakingErrTable.ANGULAR_Y_HIGH[i] - CompOff;
            //  }

            //  //[ 3-3. Glue each data for Linear1 ]
            //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
            //  {
            //      ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue = MakingErrTable.ANGULAR_Y_LOW[i];
            //      ErrorCompTable.TBL_Y_ANGULAR[i].Position= (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
            //  {
            //      ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.ANGULAR_Y_MID[i];
            //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
            //  {
            //      ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].ErrorValue = MakingErrTable.ANGULAR_Y_HIGH[i];
            //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
            //  }

            //  // [ 4. Link each Straightness data ]

            //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
            //  {
            //      MakingErrTable.STRAIGHT_Y_LOW[i]=(MakingErrTable.STRAIGHT_Y_LOW[i]+((ErrorDataYTable[Cam1].ErrorData_VER[i].RelPosY+ ErrorDataYTable[Cam2].ErrorData_VER[i].RelPosY)/20000)*ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue*Math.PI/180);
            //  }
            //  for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
            //  {
            //      MakingErrTable.STRAIGHT_Y_MID[i] = (MakingErrTable.STRAIGHT_Y_MID[i] + ((ErrorDataYTable[Cam3].ErrorData_VER[i].RelPosY + ErrorDataYTable[Cam4].ErrorData_VER[i].RelPosY) / 20000) * ErrorCompTable.TBL_Y_ANGULAR[i+EndIndexYlow-UnOvlpNum_Mid+1].ErrorValue * Math.PI / 180);
            //  }
            //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            //  {
            //      MakingErrTable.STRAIGHT_Y_HIGH[i] = (MakingErrTable.STRAIGHT_Y_HIGH[i] + ((ErrorDataYTable[Cam5].ErrorData_VER[i].RelPosY + ErrorDataYTable[Cam6].ErrorData_VER[i].RelPosY) / 20000) * ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow +EndIndexYmid- UnOvlpNum_Mid -UnOvlpNum_High+ 2].ErrorValue * Math.PI / 180);
            //  }


            //  //        Open "D:\VM\ErrorData2\StraightYlow.err" For Output As #11
            //  //    For i = 0 To EndIndexYlow
            //  //        Print #11, F2S1(StraightYlow(i)), F2S1((Pos_Y1_Y(i) + Pos_Y2_Y(i)) / 2)
            //  //    Next i
            //  //Close #11

            //  //Open "D:\VM\ErrorData2\StraightYmid.err" For Output As #11
            //  //    For i = 0 To EndIndexYmid
            //  //        Print #11, F2S1(StraightYmid(i)), F2S1((Pos_Y3_Y(i) + Pos_Y4_Y(i)) / 2)
            //  //    Next i
            //  //Close #11

            //  //Open "D:\VM\ErrorData2\StraightYhigh.err" For Output As #11
            //  //    For i = 0 To EndIndexYhigh
            //  //        Print #11, F2S1(StraightYhigh(i)), F2S1((Pos_Y5_Y(i) + Pos_Y6_Y(i)) / 2)
            //  //    Next i
            //  //Close #11

            //  //

            //  MakeStraightYWrite(0);
            //  MakeStraightYWrite(1);
            //  MakeStraightYWrite(2);


            //  // [ 4-1. Calculate straight compensation offset of Low to Mid range ]

            //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
            //  {
            //      Low2Mid_IntpVal[i] = MakingErrTable.STRAIGHT_Y_MID[i] + (MakingErrTable.STRAIGHT_Y_MID[i + 1] - MakingErrTable.STRAIGHT_Y_MID[i]) * Low2Mid_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
            //  {
            //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.STRAIGHT_Y_LOW[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_Mid;

            //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
            //  {
            //      MakingErrTable.STRAIGHT_Y_LOW[i] = MakingErrTable.STRAIGHT_Y_LOW[i] + CompOff;
            //  }

            //  for(int i=0;i<=UnOvlpNum_High-1;i++)
            //  {
            //      Mid2High_IntpVal[i] = MakingErrTable.STRAIGHT_Y_HIGH[i] + (MakingErrTable.STRAIGHT_Y_HIGH[i + 1] - MakingErrTable.STRAIGHT_Y_HIGH[i]) * Mid2High_Overlap_Amount;
            //  }
            //  CompOff = 0;
            //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
            //  {
            //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.STRAIGHT_Y_MID[i + (DataNumY3 - 1) - (UnOvlpNum_High - 1)]);
            //  }
            //  CompOff = CompOff / UnOvlpNum_High;


            //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
            //  {
            //      MakingErrTable.STRAIGHT_Y_HIGH[i] = MakingErrTable.STRAIGHT_Y_HIGH[i] + CompOff;
            //  }

            //  //' [ 4-3. Glue each data for Linear1 ]

            //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
            //  {
            //      ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = MakingErrTable.STRAIGHT_Y_LOW[i];
            //      ErrorCompTable.TBL_Y_STRAIGHT[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for(int i=UnOvlpNum_Mid;i<=EndIndexYmid;i++)
            //  {
            //      ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_STRAIGHT[i+EndIndexYlow-UnOvlpNum_Mid+1].ErrorValue = MakingErrTable.STRAIGHT_Y_MID[i];
            //      ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
            //  }
            //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
            //  {
            //      ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
            //      ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow+EndIndexYmid - UnOvlpNum_Mid-UnOvlpNum_High + 2].ErrorValue = MakingErrTable.STRAIGHT_Y_HIGH[i];
            //      ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
            //  }

            //  //  '-Straight data link end-
            //  double OffsetS;
            //  double OffsetL;
            //  double OffsetA;
            //  double OffsetZ;
            //  double DeviationS;
            //  double DeviationL;
            //  double DeviationZ;

            //  int DataUpperBound;

            //  DataUpperBound = DataNumY1 + DataNumY3 + DataNumY5 - UnOvlpNum_Mid - UnOvlpNum_High - 1;

            //  OffsetS = ErrorCompTable.TBL_Y_STRAIGHT[0].ErrorValue;
            //  OffsetL = ErrorCompTable.TBL_Y_LINEAR[0].ErrorValue;
            //  OffsetA = ErrorCompTable.TBL_Y_ANGULAR[0].ErrorValue;
            //  OffsetZ = MakingErrTable.Zangle1[0];

            //  double OffsetZH;

            //  OffsetZH = MakingErrTable.Zheight1[0];

            //  for(int i=0;i<=DataUpperBound;i++)
            //  {
            //      ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue - OffsetS;
            //      ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue = ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue - OffsetA;
            //      ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue - OffsetL;
            //      MakingErrTable.Zangle1[i] = MakingErrTable.Zangle1[i] - OffsetZ;
            //      MakingErrTable.Zheight1[i] = MakingErrTable.Zheight1[i] - OffsetZH;
            //  }

            //  DeviationS = ErrorCompTable.TBL_Y_STRAIGHT[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_STRAIGHT[0].ErrorValue;
            //  DeviationL = ErrorCompTable.TBL_Y_LINEAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_LINEAR[0].ErrorValue;
            //  DeviationZ = MakingErrTable.Zheight1[DataUpperBound - 2] - MakingErrTable.Zheight1[0];

            //  //for(int i=0;i<=DataUpperBound;i++)
            //  //{
            //  //    ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue - DeviationL * 
            //  //         ((ErrorCompTable.TBL_Y_LINEAR[i].Position - ErrorCompTable.TBL_Y_LINEAR[0].Position) 
            //  //         / (ErrorCompTable.TBL_Y_LINEAR[DataUpperBound - 2].Position - ErrorCompTable.TBL_Y_LINEAR[0].Position));
            //  //    MakingErrTable.Zheight1[i]= MakingErrTable.Zheight1[i]-DeviationL*
            //  //         ((MakingErrTable.Zheight1pos[i] - MakingErrTable.Zheight1pos[0])
            //  //         / (MakingErrTable.Zheight1pos[DataUpperBound - 2] - MakingErrTable.Zheight1pos[0]));
            //  //}
            #endregion
            SaveMakeErrorDataY();
            retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public int MakeOPUSVErrorDataY()
        {
            int retVal = -1;
            try
            {
                for (int i = 1; i <= ErrorDataYTable.Count; i++)
                {
                    retVal = LoadErrorMappingYData(i);
                    if (retVal != 0)
                    {
                        return retVal;
                    }
                }

                int Cam1 = 0;
                int Cam2 = 1;
                int Cam3 = 2;
                int Cam4 = 3;
                int Cam5 = 4;
                int Cam6 = 5;

                int DataNumY1 = ErrorDataYTable[Cam1].ErrorData_HOR.Count;
                int DataNumY2 = ErrorDataYTable[Cam2].ErrorData_HOR.Count;
                int DataNumY3 = ErrorDataYTable[Cam3].ErrorData_HOR.Count;
                int DataNumY4 = ErrorDataYTable[Cam4].ErrorData_HOR.Count;
                int DataNumY5 = ErrorDataYTable[Cam5].ErrorData_HOR.Count;
                int DataNumY6 = ErrorDataYTable[Cam6].ErrorData_HOR.Count;


                if (DataNumY1 >= DataNumY2)
                {
                    DataNumY1 = DataNumY2;
                }
                if (DataNumY3 >= DataNumY4)
                {
                    DataNumY3 = DataNumY4;
                }
                if (DataNumY5 >= DataNumY6)
                {
                    DataNumY5 = DataNumY6;
                }

                //' [ 1. Get linear/angular/straight from each data ]
                int StartIndexYlow = 0;
                int EndIndexYlow = DataNumY1 - 1;
                int StartIndexYmid = 0;
                int EndIndexYmid = DataNumY3 - 1;
                int StartIndexYmid2 = 0;
                int EndIndexYmid2 = DataNumY5 - 1;

                double partialSum = 0.0;
                double devLow = 0.0;
                double devMid = 0.0;
                double devHigh = 0.0;
                double devDiff = 0.0;

                double temp_rad_for_st_low = 0;
                double temp_rad_for_st_mid = 0;
                double temp_rad_for_st_mid2 = 0;
                //double temp_rad_for_st_high = 0;
                double[] tmpintpval = new double[20];
                double CompOff = 0;
                ErrorCompTable.TBL_Y_LINEAR = new ErrorParamList();
                ErrorCompTable.TBL_Y_STRAIGHT = new ErrorParamList();
                ErrorCompTable.TBL_Y_ANGULAR = new ErrorParamList();

                MakingErrTable.POS_ERROR_Y_Low = new List<double>();
                MakingErrTable.POS_ERROR_Y_Mid = new List<double>();
                MakingErrTable.POS_ERROR_Y_HIGH = new List<double>();


                MakingErrTable.LINEAR_Y_LOW = new List<double>();
                MakingErrTable.ANGULAR_Y_LOW = new List<double>();
                MakingErrTable.STRAIGHT_Y_LOW = new List<double>();
                MakingErrTable.LINEAR_Y_MID = new List<double>();
                MakingErrTable.ANGULAR_Y_MID = new List<double>();
                MakingErrTable.STRAIGHT_Y_MID = new List<double>();
                MakingErrTable.LINEAR_Y_MID2 = new List<double>();
                MakingErrTable.ANGULAR_Y_MID2 = new List<double>();
                MakingErrTable.STRAIGHT_Y_MID2 = new List<double>();


                for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                {
                    MakingErrTable.LINEAR_Y_LOW.Add(0);
                    MakingErrTable.ANGULAR_Y_LOW.Add(0);
                    MakingErrTable.STRAIGHT_Y_LOW.Add(0);
                    MakingErrTable.POS_ERROR_Y_Low.Add(0);

                        MakingErrTable.LINEAR_Y_LOW[i] = 
                        ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.X.Value) * ErrorDataYTable[Cam1].ErrorData_VER[i].ErrorValue)
                        + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.X.Value) * ErrorDataYTable[Cam2].ErrorData_VER[i].ErrorValue))
                        / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.X.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.X.Value));

                    //MakingErrTable.LINEAR_Y_LOW[i] = (ErrorDataYTable[Cam1].ErrorData_VER[i].ErrorValue + ErrorDataYTable[Cam2].ErrorData_VER[i].ErrorValue) / 2;

                    temp_rad_for_st_low = -(((ErrorDataYTable[Cam2].ErrorData_VER[i].ErrorValue - ErrorDataYTable[Cam1].ErrorData_VER[i].ErrorValue))
                        / (ErrMappingParam.ErrorMappingCameras[Cam1].CamPos.X.Value - ErrMappingParam.ErrorMappingCameras[Cam2].CamPos.X.Value));
                    MakingErrTable.ANGULAR_Y_LOW[i] = temp_rad_for_st_low * 180 * 10000 / Math.PI;

                    MakingErrTable.STRAIGHT_Y_LOW[i] = (ErrorDataYTable[Cam1].ErrorData_HOR[i].ErrorValue + ErrorDataYTable[Cam2].ErrorData_HOR[i].ErrorValue) / 2;
                    //MakingErrTable.STRAIGHT_Y_LOW[i] = (MakingErrTable.STRAIGHT_Y_LOW[i] - ((ErrorDataYTable[Cam1].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam2].ErrorData_HOR[i].RelPosY) / 20000)
                    //    * MakingErrTable.ANGULAR_Y_LOW[i] * Math.PI / 180);

                    MakingErrTable.POS_ERROR_Y_Low[i] = (ErrorDataYTable[Cam2].ErrorData_VER[i].YPos + ErrorDataYTable[Cam1].ErrorData_VER[i].YPos) / 2;
                }
                string line;
                string FilePath_Angular = ERROR_DATA_PATH + "RawAngularLo1.err";
                StreamWriter file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_Y_Low.Count == MakingErrTable.ANGULAR_Y_LOW.Count)
                    {
                        for (int i = 0; i < MakingErrTable.ANGULAR_Y_LOW.Count; i++)
                        {
                            line = $"{MakingErrTable.ANGULAR_Y_LOW[i],-15:0.00}          {MakingErrTable.POS_ERROR_Y_Low[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }

                    //foreach (double data in MakingErrTable.ANGULAR_X_LOW)
                    //{
                    //    line = $"{data}";
                    //    file.WriteLine(line);
                    //}
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    file.Close();
                }
                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    MakingErrTable.LINEAR_Y_MID.Add(0);
                    MakingErrTable.ANGULAR_Y_MID.Add(0);
                    MakingErrTable.STRAIGHT_Y_MID.Add(0);
                    MakingErrTable.POS_ERROR_Y_Mid.Add(0);
                    MakingErrTable.LINEAR_Y_MID[i] = ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.X.Value) * ErrorDataYTable[Cam3].ErrorData_VER[i].ErrorValue)
                     + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.X.Value) * ErrorDataYTable[Cam4].ErrorData_VER[i].ErrorValue))
                     / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.X.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.X.Value));
                    temp_rad_for_st_mid = -(((ErrorDataYTable[Cam4].ErrorData_VER[i].ErrorValue - ErrorDataYTable[Cam3].ErrorData_VER[i].ErrorValue))
                    / (ErrMappingParam.ErrorMappingCameras[Cam3].CamPos.X.Value - ErrMappingParam.ErrorMappingCameras[Cam4].CamPos.X.Value));

                    MakingErrTable.ANGULAR_Y_MID[i] = temp_rad_for_st_mid * 180 * 10000 / Math.PI;
                    MakingErrTable.STRAIGHT_Y_MID[i] = (ErrorDataYTable[Cam3].ErrorData_HOR[i].ErrorValue + ErrorDataYTable[Cam4].ErrorData_HOR[i].ErrorValue) / 2;
                    //MakingErrTable.STRAIGHT_Y_MID[i] = (MakingErrTable.STRAIGHT_Y_MID[i] - ((ErrorDataYTable[Cam3].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam4].ErrorData_HOR[i].RelPosY) / 20000)
                    //    * MakingErrTable.ANGULAR_Y_MID[i] * Math.PI / 180);
                    MakingErrTable.POS_ERROR_Y_Mid[i] = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                }
                FilePath_Angular = ERROR_DATA_PATH + "RawAngularMid1.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_Y_Mid.Count == MakingErrTable.ANGULAR_Y_MID.Count)
                    {
                        for (int i = 0; i < MakingErrTable.ANGULAR_Y_MID.Count; i++)
                        {
                            line = $"{MakingErrTable.ANGULAR_Y_MID[i],-15:0.00}          {MakingErrTable.POS_ERROR_Y_Mid[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
                finally
                {
                    file.Close();
                }

                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    MakingErrTable.LINEAR_Y_MID2.Add(0);
                    MakingErrTable.ANGULAR_Y_MID2.Add(0);
                    MakingErrTable.STRAIGHT_Y_MID2.Add(0);
                    MakingErrTable.POS_ERROR_Y_HIGH.Add(0);
                    MakingErrTable.LINEAR_Y_MID2[i] = ((Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam6].CamPos.X.Value) * ErrorDataYTable[Cam5].ErrorData_VER[i].ErrorValue)
                                                    + (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam5].CamPos.X.Value) * ErrorDataYTable[Cam6].ErrorData_VER[i].ErrorValue))
                                                    / (Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam5].CamPos.X.Value) + Math.Abs(ErrMappingParam.ErrorMappingCameras[Cam5].CamPos.X.Value));
                    temp_rad_for_st_mid2 = -(((ErrorDataYTable[Cam6].ErrorData_VER[i].ErrorValue - ErrorDataYTable[Cam5].ErrorData_VER[i].ErrorValue))
                    / (ErrMappingParam.ErrorMappingCameras[Cam5].CamPos.X.Value - ErrMappingParam.ErrorMappingCameras[Cam6].CamPos.X.Value));

                    MakingErrTable.ANGULAR_Y_MID2[i] = temp_rad_for_st_mid2 * 180 * 10000 / Math.PI;

                    MakingErrTable.STRAIGHT_Y_MID2[i] = (ErrorDataYTable[Cam5].ErrorData_HOR[i].ErrorValue + ErrorDataYTable[Cam6].ErrorData_HOR[i].ErrorValue) / 2;
                    //MakingErrTable.STRAIGHT_Y_MID2[i] = (MakingErrTable.STRAIGHT_Y_MID2[i] - ((ErrorDataYTable[Cam5].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam6].ErrorData_HOR[i].RelPosY) / 20000)
                    //                                    * MakingErrTable.ANGULAR_Y_MID2[i] * Math.PI / 180);

                    MakingErrTable.POS_ERROR_Y_HIGH[i] = (ErrorDataYTable[Cam6].ErrorData_VER[i].YPos + ErrorDataYTable[Cam5].ErrorData_VER[i].YPos) / 2;
                }
                FilePath_Angular = ERROR_DATA_PATH + "RawAngularHi1.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_Y_HIGH.Count == MakingErrTable.ANGULAR_Y_MID2.Count)
                    {
                        for (int i = 0; i < MakingErrTable.ANGULAR_Y_MID2.Count; i++)
                        {
                            line = $"{MakingErrTable.ANGULAR_Y_MID2[i],-15:0.00}          {MakingErrTable.POS_ERROR_Y_HIGH[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
                finally
                {
                    file.Close();
                }
                int End_Idx_Low = 0;
                double End_Pos_Low = 0;

                int End_Idx_Mid = 0;
                double End_Pos_Mid = 0;

                int End_Idx_Mid2 = 0;
                double End_Pos_Mid2 = 0;

                int UnOvlpNum_Mid = 0;
                int UnOvlpNum_Mid2 = 0;
                int UnOvlpNum_High = 0;

                double Low2Mid_Overlap_Amount = 0;
                //double Mid2High_Overlap_Amount = 0;
                double Mid2Mid_Overlap_Amount = 0;

                double[] Low2Mid_IntpVal = new double[100];
                double[] Mid2High_IntpVal = new double[100];
                double[] Mid2Mid_IntpVal = new double[100];


                double OffsetS = MakingErrTable.STRAIGHT_Y_LOW[0];
                double OffsetL = MakingErrTable.LINEAR_Y_LOW[0];
                double OffsetA = MakingErrTable.ANGULAR_Y_LOW[0];

                for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                {
                    MakingErrTable.STRAIGHT_Y_LOW[i] = MakingErrTable.STRAIGHT_Y_LOW[i] - OffsetS;
                    MakingErrTable.LINEAR_Y_LOW[i] = MakingErrTable.LINEAR_Y_LOW[i] - OffsetL;
                    MakingErrTable.ANGULAR_Y_LOW[i] = MakingErrTable.ANGULAR_Y_LOW[i] - OffsetA;
                }

                OffsetS = MakingErrTable.STRAIGHT_Y_MID[0];
                OffsetL = MakingErrTable.LINEAR_Y_MID[0];
                OffsetA = MakingErrTable.ANGULAR_Y_MID[0];

                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    MakingErrTable.STRAIGHT_Y_MID[i] = MakingErrTable.STRAIGHT_Y_MID[i] - OffsetS;
                    MakingErrTable.LINEAR_Y_MID[i] = MakingErrTable.LINEAR_Y_MID[i] - OffsetL;
                    MakingErrTable.ANGULAR_Y_MID[i] = MakingErrTable.ANGULAR_Y_MID[i] - OffsetA;
                }

                OffsetS = MakingErrTable.STRAIGHT_Y_MID2[0];
                OffsetL = MakingErrTable.LINEAR_Y_MID2[0];
                OffsetA = MakingErrTable.ANGULAR_Y_MID2[0];

                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    MakingErrTable.STRAIGHT_Y_MID2[i] = MakingErrTable.STRAIGHT_Y_MID2[i] - OffsetS;
                    MakingErrTable.LINEAR_Y_MID2[i] = MakingErrTable.LINEAR_Y_MID2[i] - OffsetL;
                    MakingErrTable.ANGULAR_Y_MID2[i] = MakingErrTable.ANGULAR_Y_MID2[i] - OffsetA;
                }

                //OffsetS = MakingErrTable.STRAIGHT_Y_HIGH[0];
                //OffsetL = MakingErrTable.LINEAR_Y_HIGH[0];
                //OffsetA = MakingErrTable.ANGULAR_Y_HIGH[0];

                End_Idx_Low = DataNumY1 - 1;
                End_Pos_Low = (ErrorDataYTable[Cam1].ErrorData_VER[End_Idx_Low].YPos + ErrorDataYTable[Cam2].ErrorData_VER[End_Idx_Low].YPos) / 2;




                for (int i = 0; i <= DataNumY3; i++)
                {
                    if (End_Pos_Low < (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2)
                    {
                        UnOvlpNum_Mid = i;
                        break;
                    }
                }
                if (UnOvlpNum_Mid < 1)
                {
                    retVal = -1;
                    return retVal;
                }

                Low2Mid_Overlap_Amount = ((ErrorDataYTable[Cam1].ErrorData_VER[End_Idx_Low].YPos - ErrorDataYTable[Cam3].ErrorData_VER[UnOvlpNum_Mid - 1].YPos)
                                       / (ErrorDataYTable[Cam3].ErrorData_VER[UnOvlpNum_Mid].YPos - ErrorDataYTable[Cam3].ErrorData_VER[UnOvlpNum_Mid - 1].YPos));

                End_Idx_Mid = DataNumY3 - 1;
                End_Pos_Mid = (ErrorDataYTable[Cam3].ErrorData_VER[End_Idx_Mid].YPos + ErrorDataYTable[Cam4].ErrorData_VER[End_Idx_Mid].YPos) / 2;

                for (int i = 0; i <= DataNumY5; i++)
                {
                    if (End_Pos_Mid < (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2)
                    {
                        UnOvlpNum_Mid2 = i;
                        break;
                    }
                }
                if (UnOvlpNum_Mid2 < 1)
                {
                    retVal = -1;
                    return retVal;
                }

                Mid2Mid_Overlap_Amount = ((ErrorDataYTable[Cam3].ErrorData_VER[End_Idx_Mid].YPos - ErrorDataYTable[Cam5].ErrorData_VER[UnOvlpNum_Mid2 - 1].YPos)
                                       / (ErrorDataYTable[Cam5].ErrorData_VER[UnOvlpNum_Mid2].YPos - ErrorDataYTable[Cam5].ErrorData_VER[UnOvlpNum_Mid2 - 1].YPos));


                //[ 2-3. Calculate linear compensation offset of Low to Mid range ]

                tmpintpval[0] = 0;
                for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
                {
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.LINEAR_Y_MID[i];
                }
                tmpintpval[1] = 0;

                for (int i = End_Idx_Low; i >= End_Idx_Low - UnOvlpNum_Mid + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.LINEAR_Y_LOW[i];
                }

                if (UnOvlpNum_Mid > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid;
                }
                else
                {
                    CompOff = 0;
                }
                
                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    MakingErrTable.LINEAR_Y_MID[i] = MakingErrTable.LINEAR_Y_MID[i] + CompOff;
                }


                /////
                tmpintpval[0] = 0;
                for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                {
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.LINEAR_Y_MID2[i];
                }
                tmpintpval[1] = 0;

                for (int i = End_Idx_Mid; i >= End_Idx_Mid - UnOvlpNum_Mid2 + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.LINEAR_Y_MID[i];
                }

                if (UnOvlpNum_Mid2 > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid2;
                }
                else
                {
                    CompOff = 0;
                }
                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    MakingErrTable.LINEAR_Y_MID2[i] = MakingErrTable.LINEAR_Y_MID2[i] + CompOff;
                }
                //' [ 2-5. Glue each data for Linear1 ]
                int DataNum_Tot = 0;

                for (int i = StartIndexYlow; i <= EndIndexYlow - UnOvlpNum_Mid; i++)
                {

                    ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = MakingErrTable.LINEAR_Y_LOW[i];
                    ErrorCompTable.TBL_Y_LINEAR[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }
                for (int i = StartIndexYmid; i <= EndIndexYmid - UnOvlpNum_Mid2; i++)
                {
                    ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.LINEAR_Y_MID[i];
                    ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }
                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].ErrorValue = MakingErrTable.LINEAR_Y_MID2[i];
                    ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }




                End_Idx_Mid2 = DataNumY5 - 1;
                End_Pos_Mid2 = (ErrorDataYTable[Cam5].ErrorData_VER[End_Idx_Mid2].YPos + ErrorDataYTable[Cam6].ErrorData_VER[End_Idx_Mid2].YPos) / 2;






                //' [ 2-7. Calculate linear compensation offset of Low + Mid to High range ]
                tmpintpval[0] = 0;
                for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                {
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.LINEAR_Y_HIGH[i];
                }
                tmpintpval[1] = 0;

                for (int i = End_Idx_Mid2; i >= End_Idx_Mid2 - UnOvlpNum_High + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.LINEAR_Y_MID2[i];
                }

                if (UnOvlpNum_High > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_High;
                }
                else
                {
                    CompOff = 0;
                }


           

                //'-- [   ANGULAR CONNECT  ] --

                // ' [ 3-1. Calculate angular compensation offset of Low to Mid range ]

                tmpintpval[0] = 0;
                for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
                {
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.ANGULAR_Y_MID[i];
                }
                tmpintpval[1] = 0;

                for (int i = End_Idx_Low; i >= End_Idx_Low - UnOvlpNum_Mid + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.ANGULAR_Y_LOW[i];
                }

                if (UnOvlpNum_Mid > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid;
                }
                else
                {
                    CompOff = 0;
                }


                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    MakingErrTable.ANGULAR_Y_MID[i] = MakingErrTable.ANGULAR_Y_MID[i] + CompOff;
                }




                //' [ 3-2. Calculate angular compensation offset of Mid to mid2 range ]


                /////
                tmpintpval[0] = 0;
                double low2MidCompOff = CompOff;
                for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                {
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.ANGULAR_Y_MID2[i];
                }
                tmpintpval[1] = 0;

                for (int i = End_Idx_Mid; i >= End_Idx_Mid - UnOvlpNum_Mid2 + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.ANGULAR_Y_MID[i];
                }

                if (UnOvlpNum_Mid2 > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid2;
                }
                else
                {
                    CompOff = 0;
                }
                //CompOff += low2MidCompOff;
                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    MakingErrTable.ANGULAR_Y_MID2[i] = MakingErrTable.ANGULAR_Y_MID2[i] + CompOff;
                }

                //   ' [ 3-3. Glue each data for angular1 ]


                DataNum_Tot = 0;

                for (int i = StartIndexYlow; i <= EndIndexYlow - UnOvlpNum_Mid; i++)
                {
                    ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue = MakingErrTable.ANGULAR_Y_LOW[i];
                    ErrorCompTable.TBL_Y_ANGULAR[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }
                // Calculate Low deviation
                partialSum = 0.0;
                for (int i = EndIndexYlow - UnOvlpNum_Mid; i <= EndIndexYlow; i++)
                {
                    partialSum += MakingErrTable.ANGULAR_Y_LOW[i];
                }
                devLow = partialSum / (MakingErrTable.POS_ERROR_Y_Low[EndIndexYlow - UnOvlpNum_Mid] - MakingErrTable.POS_ERROR_Y_Low[EndIndexYlow]);
                devLow = partialSum / UnOvlpNum_Mid;
                for (int i = StartIndexYmid; i <= EndIndexYmid - UnOvlpNum_Mid2; i++)
                {
                    ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.ANGULAR_Y_MID[i];
                    ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }
                
                // Calculate Low to Mid deviation
                partialSum = 0.0;
                for (int i = StartIndexYmid; i <= UnOvlpNum_Mid; i++)
                {
                    partialSum += MakingErrTable.ANGULAR_Y_MID[i];
                }
                devMid = partialSum / (MakingErrTable.POS_ERROR_Y_Mid[StartIndexYmid] - MakingErrTable.POS_ERROR_Y_Mid[UnOvlpNum_Mid]);
                devMid = partialSum / UnOvlpNum_Mid;
                LoggerManager.Debug($"Trend value = {devLow - devMid:E3}, DevMid = {devMid:E3}, DevLow = {devLow:E3}");

                devDiff = devLow - devMid;
                double low2MidDevDiff = devDiff;
                // Apply Low to Mid deviation differences
                for (int i = EndIndexYlow - UnOvlpNum_Mid + 1; i < ErrorCompTable.TBL_Y_ANGULAR.Count; i++)
                {
                    double prevAngular = ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue;
                    ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue += low2MidDevDiff;
                }
                // Calculate Mid to High deviation
                partialSum = 0.0;
                for (int i = EndIndexYmid - UnOvlpNum_Mid2; i <= EndIndexYmid; i++)
                {
                    partialSum += MakingErrTable.ANGULAR_Y_MID[i];
                }
                devMid = partialSum / (MakingErrTable.POS_ERROR_Y_Mid[EndIndexYmid - UnOvlpNum_Mid2] - MakingErrTable.POS_ERROR_Y_Mid[EndIndexYmid]);
                devMid = partialSum / UnOvlpNum_Mid2;
                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].ErrorValue = MakingErrTable.ANGULAR_Y_MID2[i];
                    ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }
                // Calculate Mid2(High) deviation
                partialSum = 0.0;
                for (int i = StartIndexYmid2; i <= UnOvlpNum_Mid2; i++)
                {
                    partialSum += MakingErrTable.ANGULAR_Y_MID2[i];
                }
                devHigh = partialSum / (MakingErrTable.POS_ERROR_Y_HIGH[StartIndexYmid2] - MakingErrTable.POS_ERROR_Y_HIGH[UnOvlpNum_Mid2]);
                devHigh = partialSum / UnOvlpNum_Mid2;
                LoggerManager.Debug($"Trend value = {(devHigh + devMid) / 2 - devMid:E3}, DevHigh = {devHigh:E3}, DevMid = {devMid:E3}");
                //devMid = MakingErrTable.ANGULAR_Y_MID[EndIndexYmid] - MakingErrTable.ANGULAR_Y_MID[EndIndexYmid - UnOvlpNum_Mid2];
                //devHigh = MakingErrTable.ANGULAR_Y_MID2[UnOvlpNum_Mid2] - MakingErrTable.ANGULAR_Y_MID2[StartIndexYmid2];
                devDiff = devMid - devHigh;
                double Mid2HiDevDiff = devDiff + low2MidDevDiff;
                // Apply Mid to High deviation differences
                for (int i = EndIndexYlow - UnOvlpNum_Mid + EndIndexYmid - UnOvlpNum_Mid2 + 2; i < ErrorCompTable.TBL_Y_ANGULAR.Count; i++)
                {
                    double prevAngular = ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue;

                    //ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue -= devDiff * (ErrorCompTable.TBL_Y_ANGULAR[EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid2 + 1].Position - ErrorCompTable.TBL_Y_ANGULAR[i].Position);
                    ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue += Mid2HiDevDiff;
                    //LoggerManager.Debug($"Y Angular error high @Y={ErrorCompTable.TBL_Y_ANGULAR[i].Position} compensated by deviation diff. {devDiff,-9:E3}: Prev = {prevAngular,-9:0.0000}, Compensated = {ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue,-9:0.0000}");
                }


                // Adjust straightness
                //for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                //{
                //    MakingErrTable.STRAIGHT_Y_LOW[i] = (MakingErrTable.STRAIGHT_Y_LOW[i] + ((ErrorDataYTable[Cam1].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam2].ErrorData_HOR[i].RelPosY) / 20000)
                //        * MakingErrTable.ANGULAR_Y_LOW[i] * Math.PI / 180);
                //}

                //for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                //{
                //    MakingErrTable.STRAIGHT_Y_MID[i] = (MakingErrTable.STRAIGHT_Y_MID[i] + ((ErrorDataYTable[Cam3].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam4].ErrorData_HOR[i].RelPosY) / 20000)
                //        * MakingErrTable.ANGULAR_Y_MID[i] * Math.PI / 180);
                //}
                //for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                //{
                //    MakingErrTable.STRAIGHT_Y_MID2[i] = (MakingErrTable.STRAIGHT_Y_MID2[i] + ((ErrorDataYTable[Cam5].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam6].ErrorData_HOR[i].RelPosY) / 20000)
                //                                        * MakingErrTable.ANGULAR_Y_MID2[i] * Math.PI / 180);
                //}

                //' [ 3-5. Calculate angular compensation offset of Mid2 to High range ]


                tmpintpval[0] = 0;
                for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                {
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.ANGULAR_Y_HIGH[i];
                }
                tmpintpval[1] = 0;

                for (int i = End_Idx_Mid2; i >= End_Idx_Mid2 - UnOvlpNum_High + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.ANGULAR_Y_MID2[i];
                }

                if (UnOvlpNum_High > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_High;
                }
                else
                {
                    CompOff = 0;
                }

                //' [ 4-1. Calculate straightness compensation offset of Low to Mid range ]
                FilePath_Angular = ERROR_DATA_PATH + "RawStraightnessHi1.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_Y_HIGH.Count == MakingErrTable.STRAIGHT_Y_MID2.Count)
                    {
                        for (int i = 0; i < MakingErrTable.STRAIGHT_Y_MID2.Count; i++)
                        {
                            line = $"{MakingErrTable.STRAIGHT_Y_MID2[i],-15:0.00}          {MakingErrTable.POS_ERROR_Y_HIGH[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    file.Close();
                }
                FilePath_Angular = ERROR_DATA_PATH + "RawStraightnessMid1.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_Y_Mid.Count == MakingErrTable.STRAIGHT_Y_MID.Count)
                    {
                        for (int i = 0; i < MakingErrTable.STRAIGHT_Y_MID.Count; i++)
                        {
                            line = $"{MakingErrTable.STRAIGHT_Y_MID[i],-15:0.00}          {MakingErrTable.POS_ERROR_Y_Mid[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    file.Close();
                }
                FilePath_Angular = ERROR_DATA_PATH + "RawStraightnessLow1.err";
                file = new System.IO.StreamWriter(FilePath_Angular);
                try
                {
                    if (MakingErrTable.POS_ERROR_Y_Low.Count == MakingErrTable.STRAIGHT_Y_LOW.Count)
                    {
                        for (int i = 0; i < MakingErrTable.STRAIGHT_Y_LOW.Count; i++)
                        {
                            line = $"{MakingErrTable.STRAIGHT_Y_LOW[i],-15:0.00}          {MakingErrTable.POS_ERROR_Y_Low[i],-15:0.00}";
                            file.WriteLine(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    file.Close();
                }

                //Compensate straightness with aligned angulars
                //for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                //{
                //    double prevStr = MakingErrTable.STRAIGHT_Y_LOW[i];
                //    //MakingErrTable.STRAIGHT_Y_LOW[i] = (MakingErrTable.STRAIGHT_Y_LOW[i] - ((ErrorDataYTable[Cam1].ErrorData_HOR[i].RelPosY + ErrorDataYTable[Cam2].ErrorData_HOR[i].RelPosY) / 20000)
                //    //* ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue * Math.PI / 180);

                //    var strCompByAngleDiff = (MakingErrTable.POS_ERROR_Y_Low[i] - MakingErrTable.POS_ERROR_Y_Low[0]) * devLow * Math.PI / 180.0 / 10000.0;
                //    MakingErrTable.STRAIGHT_Y_LOW[i] = MakingErrTable.STRAIGHT_Y_LOW[i] - strCompByAngleDiff;
                //    LoggerManager.Debug($"Y Straightness error Mid @Y={MakingErrTable.POS_ERROR_Y_Mid[i]} compensated by angle offset = {strCompByAngleDiff,-9:0.00}: Prev = {prevStr,-9:0.00}, Compensated = {MakingErrTable.STRAIGHT_Y_LOW[i],-9:0.00}");
                //}
                var ovlrLengthMid = Math.Abs(ErrorCompTable.TBL_Y_ANGULAR[EndIndexYlow - UnOvlpNum_Mid + 1].Position - ErrorCompTable.TBL_Y_ANGULAR[EndIndexYlow + 1].Position);
                var compAngleOffset = low2MidDevDiff;
                LoggerManager.Debug($"Y Low to Mid angle difference = {low2MidDevDiff}.");
                for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
                {
                    double prevStr = MakingErrTable.STRAIGHT_Y_MID[i];
                    var strCompByAngleDiff = (MakingErrTable.POS_ERROR_Y_Mid[i] - MakingErrTable.POS_ERROR_Y_Mid[0]) * compAngleOffset * Math.PI / 180.0 / 10000.0;
                    MakingErrTable.STRAIGHT_Y_MID[i] = MakingErrTable.STRAIGHT_Y_MID[i] - strCompByAngleDiff;
                    LoggerManager.Debug($"Y Straightness error Mid @Y={MakingErrTable.POS_ERROR_Y_Mid[i]} compensated by angle offset = {strCompByAngleDiff,-9:0.00}: Prev = {prevStr,-9:0.00}, Compensated = {MakingErrTable.STRAIGHT_Y_MID[i],-9:0.00}");

                    // Start from Mid1 overlapped angular.

                }
                var ovlrLengthHi = Math.Abs(ErrorCompTable.TBL_Y_ANGULAR[EndIndexYlow - UnOvlpNum_Mid + EndIndexYmid - UnOvlpNum_Mid2 + 2].Position 
                    - ErrorCompTable.TBL_Y_ANGULAR[EndIndexYlow - UnOvlpNum_Mid + EndIndexYmid + 2].Position);
                compAngleOffset = Mid2HiDevDiff;
                LoggerManager.Debug($"Y Mid to High angle difference = {Mid2HiDevDiff}.");
                for (int i = UnOvlpNum_Mid2; i <= EndIndexYmid2; i++)
                {
                    double prevStr = MakingErrTable.STRAIGHT_Y_MID2[i];
                    var strCompByAngleDiff = (MakingErrTable.POS_ERROR_Y_HIGH[i] - MakingErrTable.POS_ERROR_Y_HIGH[0]) * compAngleOffset * Math.PI / 180.0 / 10000.0;
                    MakingErrTable.STRAIGHT_Y_MID2[i] = MakingErrTable.STRAIGHT_Y_MID2[i] - strCompByAngleDiff;
                    //Start from Mid2 overlapped angular.
                    LoggerManager.Debug($"Y Straightness error High @Y={MakingErrTable.POS_ERROR_Y_HIGH[i]} compensated by angle offset = {strCompByAngleDiff,-9:0.00}: Prev = {prevStr,-9:0.00}, Compensated = {MakingErrTable.STRAIGHT_Y_MID2[i],-9:0.00}");

                }
                
                OffsetS = MakingErrTable.STRAIGHT_Y_MID[0];
                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    MakingErrTable.STRAIGHT_Y_MID[i] = MakingErrTable.STRAIGHT_Y_MID[i] - OffsetS;
                }
                OffsetS = MakingErrTable.STRAIGHT_Y_MID2[0];
                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    MakingErrTable.STRAIGHT_Y_MID2[i] = MakingErrTable.STRAIGHT_Y_MID2[i] - OffsetS;
                }

                tmpintpval[0] = 0;
                int iterCounter = 0;
                for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
                {
                    iterCounter++;
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.STRAIGHT_Y_MID[i];
                }
                iterCounter = 0;
                tmpintpval[1] = 0;
                for (int i = End_Idx_Low; i >= End_Idx_Low - UnOvlpNum_Mid + 1; i--)
                {
                    iterCounter++;
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.STRAIGHT_Y_LOW[i];
                }

                if (UnOvlpNum_Mid > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid;
                }
                else
                {
                    CompOff = 0;
                }
                // R * Theta: 원의 호, um 단위, 1/10000 도 단위 이므로 10000000 / 10000 = 100
                //CompOff -= low2MidDevDiff / ovlrLengthMid * Math.PI / 180.0 * ovlrLengthMid;// * 1000000 / 10000;
                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    MakingErrTable.STRAIGHT_Y_MID[i] = MakingErrTable.STRAIGHT_Y_MID[i] + CompOff;
                }

                for (int i = 0; i < UnOvlpNum_Mid; i++)
                {
                    tmpintpval[3] = tmpintpval[3] + MakingErrTable.STRAIGHT_Y_MID[i];
                }
                for (int i = End_Idx_Low - UnOvlpNum_Mid; i < End_Idx_Low; i++)
                {
                    tmpintpval[4] = tmpintpval[4] + MakingErrTable.STRAIGHT_Y_LOW[i];
                }

                var midRamp = tmpintpval[3] / UnOvlpNum_Mid;
                var lowRamp = tmpintpval[4] / UnOvlpNum_Mid;
                var rampOffset = (midRamp - lowRamp);
                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    //MakingErrTable.STRAIGHT_Y_MID[i] = MakingErrTable.STRAIGHT_Y_MID[i] - rampOffset;
                }
                //' [ 4-2. Calculate straightness compensation offset of Mid to mid2 range ]


                tmpintpval[0] = 0;
                for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                {
                    tmpintpval[0] = tmpintpval[0] + MakingErrTable.STRAIGHT_Y_MID2[i];
                }
                tmpintpval[1] = 0;

                for (int i = End_Idx_Mid; i >= End_Idx_Mid - UnOvlpNum_Mid2 + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.STRAIGHT_Y_MID[i];
                }

                if (UnOvlpNum_Mid2 > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_Mid2;
                }
                else
                {
                    CompOff = 0;
                }
                // R * Theta: 원의 호, um 단위, 1/10000 도 단위 이므로 10000000 / 10000 = 100, Low to Mid offset은 이미 적용 되어 있으므로 제거함(Mid2HiDevDiff - low2MidDevDiff)
                //CompOff -= (Mid2HiDevDiff - low2MidDevDiff) / ovlrLengthHi * Math.PI / 180.0 * ovlrLengthHi;// * 1000000 / 10000;
                for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                {
                    MakingErrTable.STRAIGHT_Y_MID2[i] = MakingErrTable.STRAIGHT_Y_MID2[i] + CompOff;
                }
                tmpintpval[3] = 0;
                tmpintpval[4] = 0;
                for (int i = 0; i < UnOvlpNum_Mid2; i++)
                {
                    tmpintpval[3] = tmpintpval[3] + MakingErrTable.STRAIGHT_Y_MID2[i];
                }
                for (int i = End_Idx_Mid - UnOvlpNum_Mid2; i < End_Idx_Mid; i++)
                {
                    tmpintpval[4] = tmpintpval[4] + MakingErrTable.STRAIGHT_Y_MID[i];
                }
                midRamp = tmpintpval[3] / UnOvlpNum_Mid;
                lowRamp = tmpintpval[4] / UnOvlpNum_Mid;
                rampOffset = (midRamp - lowRamp);
                for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                {
                    //MakingErrTable.STRAIGHT_Y_MID2[i] = MakingErrTable.STRAIGHT_Y_MID2[i] - rampOffset;
                }
                // [ 4-3. Glue each data for straightness1 ]


                DataNum_Tot = 0;

                for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                {

                    ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = MakingErrTable.STRAIGHT_Y_LOW[i];
                    ErrorCompTable.TBL_Y_STRAIGHT[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }
                for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
                {
                    ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.STRAIGHT_Y_MID[i];
                    ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }
                for (int i = UnOvlpNum_Mid2; i <= EndIndexYmid2; i++)
                {
                    ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                    ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].ErrorValue = MakingErrTable.STRAIGHT_Y_MID2[i];
                    ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                    DataNum_Tot++;
                }


                //' [ 4-5. Calculate straightness compensation offset of Low + Mid to High range ]

                tmpintpval[0] = 0;
                //for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                //{
                //    tmpintpval[0] = tmpintpval[0] + MakingErrTable.STRAIGHT_Y_HIGH[i];
                //}
                tmpintpval[1] = 0;

                for (int i = End_Idx_Mid2; i >= End_Idx_Mid2 - UnOvlpNum_High + 1; i--)
                {
                    tmpintpval[1] = tmpintpval[1] + MakingErrTable.STRAIGHT_Y_MID2[i];
                }

                if (UnOvlpNum_High > 0)
                {
                    CompOff = (tmpintpval[1] - tmpintpval[0]) / UnOvlpNum_High;
                }
                else
                {
                    CompOff = 0;
                }


        

                int DataUpperBound = DataNum_Tot;

                double DeviationL = ErrorCompTable.TBL_Y_LINEAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_LINEAR[0].ErrorValue;
                double deviationZ = ErrorCompTable.TBL_Y_ANGULAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_ANGULAR[0].ErrorValue;
                double deviationS = ErrorCompTable.TBL_Y_STRAIGHT[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_STRAIGHT[0].ErrorValue;


                //for (int i = 0; i <= DataUpperBound - 1; i++)
                //{
                //    ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue
                //        - DeviationL *
                //        ((ErrorCompTable.TBL_Y_LINEAR[i].Position - ErrorCompTable.TBL_Y_LINEAR[0].Position) / (ErrorCompTable.TBL_Y_LINEAR[DataUpperBound - 2].Position - ErrorCompTable.TBL_Y_LINEAR[0].Position));
                //}

                //for (int i = 0; i <= DataUpperBound - 1; i++)
                //{
                //    ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue
                //        - deviationS * ((ErrorCompTable.TBL_Y_STRAIGHT[i].Position - ErrorCompTable.TBL_Y_STRAIGHT[0].Position)
                //        / (ErrorCompTable.TBL_Y_STRAIGHT[DataUpperBound - 2].Position - ErrorCompTable.TBL_Y_STRAIGHT[0].Position));
                //}

                //for (int i = 0; i <= DataUpperBound - 1; i++)
                //{
                //    ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue = ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue
                //        - deviationZ * ((ErrorCompTable.TBL_Y_ANGULAR[i].Position - ErrorCompTable.TBL_Y_ANGULAR[0].Position)
                //        / (ErrorCompTable.TBL_Y_ANGULAR[DataUpperBound - 2].Position - ErrorCompTable.TBL_Y_ANGULAR[0].Position));
                //}


                #region //MakeErrorData_Y_EX
                //  for (int i=0;i<=UnOvlpNum_Mid-1;i++)
                //  {
                //      Low2Mid_IntpVal[i] = MakingErrTable.ZangleYMid[i] + (MakingErrTable.ZangleYMid[i + 1] - MakingErrTable.ZangleYMid[i]) * Low2Mid_Overlap_Amount;
                //  }

                //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
                //  {
                //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.ZangleYLow[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_Mid;

                //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
                //  {
                //      MakingErrTable.ZangleYLow[i] = MakingErrTable.ZangleYLow[i] + CompOff;
                //  }
                //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
                //  {
                //      Low2Mid_IntpVal[i] = MakingErrTable.ZheightYMid[i] + (MakingErrTable.ZheightYMid[i + 1] - MakingErrTable.ZheightYMid[i]) * Low2Mid_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
                //  {
                //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.ZheightYLow[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_Mid;
                //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
                //  {
                //      MakingErrTable.ZheightYLow[i] = MakingErrTable.ZheightYLow[i] + CompOff;
                //  }

                //  //////////////////////////////////////////////////////////////////

                //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                //  {
                //      Mid2Mid_IntpVal[i] = MakingErrTable.ZangleYMid2[i] + (MakingErrTable.ZangleYMid2[i + 1] - MakingErrTable.ZangleYMid2[i]) * Mid2Mid_Overlap_Amount;
                //  }
                //CompOff = 0;

                //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2Mid_IntpVal[i] - MakingErrTable.ZangleYMid2[i + (DataNumY3 - 1) - (UnOvlpNum_Mid2 - 1)]);
                //  }

                //  CompOff = CompOff / UnOvlpNum_Mid2;

                //  for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                //  {
                //      MakingErrTable.ZangleYMid2[i] = MakingErrTable.ZangleYMid2[i] - CompOff;
                //  }
                //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                //  {
                //      Mid2Mid_IntpVal[i] = MakingErrTable.ZheightYMid2[i] + (MakingErrTable.ZheightYMid2[i + 1] - MakingErrTable.ZheightYMid2[i]) * Mid2Mid_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2Mid_IntpVal[i] - MakingErrTable.ZheightYMid2[i + (DataNumY3 - 1) - (UnOvlpNum_Mid2 - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_Mid2;
                //  for (int i = StartIndexYmid2; i <= EndIndexYmid2; i++)
                //  {
                //      MakingErrTable.ZheightYMid2[i] = MakingErrTable.ZheightYMid2[i] - CompOff;
                //  }


                //  //[ 2-6. Calculate linear compensation offset of Mid2 to High range ]

                //  for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                //  {

                //      ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = MakingErrTable.LINEAR_Y_LOW[i];
                //      ErrorCompTable.TBL_Y_LINEAR[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
                //  {
                //      ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.LINEAR_Y_MID[i];
                //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
                //  {
                //      ErrorCompTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].ErrorValue = MakingErrTable.LINEAR_Y_HIGH[i];
                //      ErrorCompTable.TBL_Y_LINEAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                //  }


                //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
                //  {
                //      Mid2High_IntpVal[i] = MakingErrTable.ZangleYHigh[i] + (MakingErrTable.ZangleYHigh[i + 1] - MakingErrTable.ZangleYHigh[i]) * Mid2High_Overlap_Amount;
                //  }
                //  CompOff = 0;

                //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.ZangleYHigh[i + (DataNumY7 - 1) - (UnOvlpNum_High - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_High;

                //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
                //  {
                //      MakingErrTable.ZangleYHigh[i] = MakingErrTable.ZangleYHigh[i] - CompOff;
                //  }
                //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
                //  {
                //      Mid2High_IntpVal[i] = MakingErrTable.ZheightYHigh[i] + (MakingErrTable.ZheightYHigh[i + 1] - MakingErrTable.ZheightYHigh[i]) * Mid2High_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i < UnOvlpNum_High - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.ZheightYHigh[i + (DataNumY7 - 1) - (UnOvlpNum_High - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_High;
                //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
                //  {
                //      MakingErrTable.ZheightYHigh[i] = MakingErrTable.ZheightYHigh[i] - CompOff;
                //  }
                //  //[ 2-7. Glue each data for Z ]

                //  for (int i=StartIndexYlow;i<=EndIndexYlow;i++)
                //  {
                //      MakingErrTable.Zangle1.Add(0);
                //      MakingErrTable.Zangle1pos.Add(0);
                //      MakingErrTable.Zheight1.Add(0);
                //      MakingErrTable.Zheight1pos.Add(0);

                //      MakingErrTable.Zangle1[i] = MakingErrTable.ZangleYLow[i];
                //      MakingErrTable.Zangle1pos[i] = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                //      MakingErrTable.Zheight1[i] = MakingErrTable.ZheightYLow[i];
                //      MakingErrTable.Zheight1pos[i] = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
                //  {
                //      MakingErrTable.Zangle1.Add(0);
                //      MakingErrTable.Zangle1pos.Add(0);
                //      MakingErrTable.Zheight1.Add(0);
                //      MakingErrTable.Zheight1pos.Add(0);

                //      MakingErrTable.Zangle1[i+EndIndexYlow-UnOvlpNum_Mid+1] = MakingErrTable.ZangleYMid[i];
                //      MakingErrTable.Zangle1pos[i + EndIndexYlow - UnOvlpNum_Mid + 1] = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                //      MakingErrTable.Zheight1[i + EndIndexYlow - UnOvlpNum_Mid + 1] = MakingErrTable.ZheightYMid[i];
                //      MakingErrTable.Zheight1pos[i + EndIndexYlow - UnOvlpNum_Mid + 1] = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_Mid2; i <= EndIndexYmid2; i++)
                //  {
                //      MakingErrTable.Zangle1.Add(0);
                //      MakingErrTable.Zangle1pos.Add(0);
                //      MakingErrTable.Zheight1.Add(0);
                //      MakingErrTable.Zheight1pos.Add(0);

                //      MakingErrTable.Zangle1[i + EndIndexYlow+ EndIndexYmid-UnOvlpNum_Mid-UnOvlpNum_Mid2+2] = MakingErrTable.ZangleYMid2[i];
                //      MakingErrTable.Zangle1pos[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2] = (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2;
                //      MakingErrTable.Zheight1[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2] = MakingErrTable.ZheightYMid2[i];
                //      MakingErrTable.Zheight1pos[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_Mid2 + 2] = (ErrorDataYTable[Cam7].ErrorData_VER[i].YPos + ErrorDataYTable[Cam8].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
                //  {
                //      MakingErrTable.Zangle1.Add(0);
                //      MakingErrTable.Zangle1pos.Add(0);
                //      MakingErrTable.Zheight1.Add(0);
                //      MakingErrTable.Zheight1pos.Add(0);

                //      MakingErrTable.Zangle1[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2-UnOvlpNum_Mid-UnOvlpNum_Mid2-UnOvlpNum_High+ 3] = MakingErrTable.ZangleYHigh[i];
                //      MakingErrTable.Zangle1pos[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2 - UnOvlpNum_Mid - UnOvlpNum_Mid2 - UnOvlpNum_High + 3] = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                //      MakingErrTable.Zheight1[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2 - UnOvlpNum_Mid - UnOvlpNum_Mid2 - UnOvlpNum_High + 3] = MakingErrTable.ZheightYHigh[i];
                //      MakingErrTable.Zheight1pos[i + EndIndexYlow + EndIndexYmid + EndIndexYmid2 - UnOvlpNum_Mid - UnOvlpNum_Mid2 - UnOvlpNum_High + 3] = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                //  }
                //  //  [ 2-8. Calculate linear compensation offset of Low to Mid range ]

                //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
                //  {
                //      Low2Mid_IntpVal[i] = MakingErrTable.LINEAR_Y_MID[i] + (MakingErrTable.LINEAR_Y_MID[i + 1] - MakingErrTable.LINEAR_Y_MID[i]) * Low2Mid_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
                //  {
                //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.LINEAR_Y_LOW[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_Mid;
                //  for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                //  {
                //      MakingErrTable.LINEAR_Y_LOW[i] = MakingErrTable.LINEAR_Y_LOW[i] + CompOff;
                //  }

                //  //[ 2-9. Calculate linear compensation offset of Mid to Mid2 range ]

                //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                //  {
                //      Mid2Mid_IntpVal[i] = MakingErrTable.LINEAR_Y_MID2[i] + (MakingErrTable.LINEAR_Y_MID2[i + 1] - MakingErrTable.LINEAR_Y_MID2[i]) * Mid2Mid_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_Mid2 - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2Mid_IntpVal[i] - MakingErrTable.LINEAR_Y_MID[i + (DataNumY3 - 1) - (UnOvlpNum_Mid2 - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_Mid2;
                //  for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                //  {
                //      MakingErrTable.LINEAR_Y_MID[i] = MakingErrTable.LINEAR_Y_MID[i] - CompOff;
                //  }

                //  //[ 2-10. Calculate linear compensation offset of Mid2 to High range ]
                //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                //  {
                //      Mid2High_IntpVal[i] = MakingErrTable.LINEAR_Y_HIGH[i] + (MakingErrTable.LINEAR_Y_HIGH[i + 1] - MakingErrTable.LINEAR_Y_HIGH[i]) * Mid2High_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.LINEAR_Y_MID[i + (DataNumY7 - 1) - (UnOvlpNum_High - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_High;
                //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
                //  {
                //      MakingErrTable.LINEAR_Y_HIGH[i] = MakingErrTable.LINEAR_Y_HIGH[i] - CompOff;
                //  }




                // // ' [ 3. Link each Angular data ]
                // //' [ 3-1. Calculate angular compensation offset of Low to Mid range ]
                // for(int i=0;i<=UnOvlpNum_Mid-1;i++)
                //  {
                //      Low2Mid_IntpVal[i] = MakingErrTable.ANGULAR_Y_MID[i] + (MakingErrTable.ANGULAR_Y_MID[i + 1] - MakingErrTable.ANGULAR_Y_MID[i]) * Low2Mid_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_Mid - 1; i++)
                //  {
                //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.LINEAR_Y_LOW[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_Mid;
                //  for (int i = StartIndexYlow; i <= EndIndexYlow; i++)
                //  {
                //      MakingErrTable.LINEAR_Y_LOW[i] = MakingErrTable.LINEAR_Y_LOW[i] + CompOff;
                //  }

                //  //[ 3-2. Calculate angular compensation offset of Mid to High range ]
                //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                //  {
                //      Mid2High_IntpVal[i] = MakingErrTable.ANGULAR_Y_HIGH[i] + (MakingErrTable.ANGULAR_Y_HIGH[i + 1] - MakingErrTable.ANGULAR_Y_HIGH[i]) * Mid2High_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.ANGULAR_Y_MID[i + (DataNumY3 - 1) - (UnOvlpNum_High - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_High;
                //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
                //  {
                //      MakingErrTable.ANGULAR_Y_HIGH[i] = MakingErrTable.ANGULAR_Y_HIGH[i] - CompOff;
                //  }

                //  //[ 3-3. Glue each data for Linear1 ]
                //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
                //  {
                //      ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue = MakingErrTable.ANGULAR_Y_LOW[i];
                //      ErrorCompTable.TBL_Y_ANGULAR[i].Position= (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_Mid; i <= EndIndexYmid; i++)
                //  {
                //      ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].ErrorValue = MakingErrTable.ANGULAR_Y_MID[i];
                //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
                //  {
                //      ErrorCompTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].ErrorValue = MakingErrTable.ANGULAR_Y_HIGH[i];
                //      ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                //  }

                //  // [ 4. Link each Straightness data ]

                //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
                //  {
                //      MakingErrTable.STRAIGHT_Y_LOW[i]=(MakingErrTable.STRAIGHT_Y_LOW[i]+((ErrorDataYTable[Cam1].ErrorData_VER[i].RelPosY+ ErrorDataYTable[Cam2].ErrorData_VER[i].RelPosY)/20000)*ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue*Math.PI/180);
                //  }
                //  for (int i = StartIndexYmid; i <= EndIndexYmid; i++)
                //  {
                //      MakingErrTable.STRAIGHT_Y_MID[i] = (MakingErrTable.STRAIGHT_Y_MID[i] + ((ErrorDataYTable[Cam3].ErrorData_VER[i].RelPosY + ErrorDataYTable[Cam4].ErrorData_VER[i].RelPosY) / 20000) * ErrorCompTable.TBL_Y_ANGULAR[i+EndIndexYlow-UnOvlpNum_Mid+1].ErrorValue * Math.PI / 180);
                //  }
                //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
                //  {
                //      MakingErrTable.STRAIGHT_Y_HIGH[i] = (MakingErrTable.STRAIGHT_Y_HIGH[i] + ((ErrorDataYTable[Cam5].ErrorData_VER[i].RelPosY + ErrorDataYTable[Cam6].ErrorData_VER[i].RelPosY) / 20000) * ErrorCompTable.TBL_Y_ANGULAR[i + EndIndexYlow +EndIndexYmid- UnOvlpNum_Mid -UnOvlpNum_High+ 2].ErrorValue * Math.PI / 180);
                //  }


                //  //        Open "D:\VM\ErrorData2\StraightYlow.err" For Output As #11
                //  //    For i = 0 To EndIndexYlow
                //  //        Print #11, F2S1(StraightYlow(i)), F2S1((Pos_Y1_Y(i) + Pos_Y2_Y(i)) / 2)
                //  //    Next i
                //  //Close #11

                //  //Open "D:\VM\ErrorData2\StraightYmid.err" For Output As #11
                //  //    For i = 0 To EndIndexYmid
                //  //        Print #11, F2S1(StraightYmid(i)), F2S1((Pos_Y3_Y(i) + Pos_Y4_Y(i)) / 2)
                //  //    Next i
                //  //Close #11

                //  //Open "D:\VM\ErrorData2\StraightYhigh.err" For Output As #11
                //  //    For i = 0 To EndIndexYhigh
                //  //        Print #11, F2S1(StraightYhigh(i)), F2S1((Pos_Y5_Y(i) + Pos_Y6_Y(i)) / 2)
                //  //    Next i
                //  //Close #11

                //  //

                //  MakeStraightYWrite(0);
                //  MakeStraightYWrite(1);
                //  MakeStraightYWrite(2);


                //  // [ 4-1. Calculate straight compensation offset of Low to Mid range ]

                //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
                //  {
                //      Low2Mid_IntpVal[i] = MakingErrTable.STRAIGHT_Y_MID[i] + (MakingErrTable.STRAIGHT_Y_MID[i + 1] - MakingErrTable.STRAIGHT_Y_MID[i]) * Low2Mid_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for(int i=0;i<=UnOvlpNum_Mid-1;i++)
                //  {
                //      CompOff = CompOff + (Low2Mid_IntpVal[i] - MakingErrTable.STRAIGHT_Y_LOW[i + (DataNumY1 - 1) - (UnOvlpNum_Mid - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_Mid;

                //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
                //  {
                //      MakingErrTable.STRAIGHT_Y_LOW[i] = MakingErrTable.STRAIGHT_Y_LOW[i] + CompOff;
                //  }

                //  for(int i=0;i<=UnOvlpNum_High-1;i++)
                //  {
                //      Mid2High_IntpVal[i] = MakingErrTable.STRAIGHT_Y_HIGH[i] + (MakingErrTable.STRAIGHT_Y_HIGH[i + 1] - MakingErrTable.STRAIGHT_Y_HIGH[i]) * Mid2High_Overlap_Amount;
                //  }
                //  CompOff = 0;
                //  for (int i = 0; i <= UnOvlpNum_High - 1; i++)
                //  {
                //      CompOff = CompOff + (Mid2High_IntpVal[i] - MakingErrTable.STRAIGHT_Y_MID[i + (DataNumY3 - 1) - (UnOvlpNum_High - 1)]);
                //  }
                //  CompOff = CompOff / UnOvlpNum_High;


                //  for (int i = StartIndexYhigh; i <= EndIndexYhigh; i++)
                //  {
                //      MakingErrTable.STRAIGHT_Y_HIGH[i] = MakingErrTable.STRAIGHT_Y_HIGH[i] + CompOff;
                //  }

                //  //' [ 4-3. Glue each data for Linear1 ]

                //  for(int i=StartIndexYlow;i<=EndIndexYlow;i++)
                //  {
                //      ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = MakingErrTable.STRAIGHT_Y_LOW[i];
                //      ErrorCompTable.TBL_Y_STRAIGHT[i].Position = (ErrorDataYTable[Cam1].ErrorData_VER[i].YPos + ErrorDataYTable[Cam2].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for(int i=UnOvlpNum_Mid;i<=EndIndexYmid;i++)
                //  {
                //      ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_STRAIGHT[i+EndIndexYlow-UnOvlpNum_Mid+1].ErrorValue = MakingErrTable.STRAIGHT_Y_MID[i];
                //      ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow - UnOvlpNum_Mid + 1].Position = (ErrorDataYTable[Cam3].ErrorData_VER[i].YPos + ErrorDataYTable[Cam4].ErrorData_VER[i].YPos) / 2;
                //  }
                //  for (int i = UnOvlpNum_High; i <= EndIndexYhigh; i++)
                //  {
                //      ErrorCompTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                //      ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow+EndIndexYmid - UnOvlpNum_Mid-UnOvlpNum_High + 2].ErrorValue = MakingErrTable.STRAIGHT_Y_HIGH[i];
                //      ErrorCompTable.TBL_Y_STRAIGHT[i + EndIndexYlow + EndIndexYmid - UnOvlpNum_Mid - UnOvlpNum_High + 2].Position = (ErrorDataYTable[Cam5].ErrorData_VER[i].YPos + ErrorDataYTable[Cam6].ErrorData_VER[i].YPos) / 2;
                //  }

                //  //  '-Straight data link end-
                //  double OffsetS;
                //  double OffsetL;
                //  double OffsetA;
                //  double OffsetZ;
                //  double DeviationS;
                //  double DeviationL;
                //  double DeviationZ;

                //  int DataUpperBound;

                //  DataUpperBound = DataNumY1 + DataNumY3 + DataNumY5 - UnOvlpNum_Mid - UnOvlpNum_High - 1;

                //  OffsetS = ErrorCompTable.TBL_Y_STRAIGHT[0].ErrorValue;
                //  OffsetL = ErrorCompTable.TBL_Y_LINEAR[0].ErrorValue;
                //  OffsetA = ErrorCompTable.TBL_Y_ANGULAR[0].ErrorValue;
                //  OffsetZ = MakingErrTable.Zangle1[0];

                //  double OffsetZH;

                //  OffsetZH = MakingErrTable.Zheight1[0];

                //  for(int i=0;i<=DataUpperBound;i++)
                //  {
                //      ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue = ErrorCompTable.TBL_Y_STRAIGHT[i].ErrorValue - OffsetS;
                //      ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue = ErrorCompTable.TBL_Y_ANGULAR[i].ErrorValue - OffsetA;
                //      ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue - OffsetL;
                //      MakingErrTable.Zangle1[i] = MakingErrTable.Zangle1[i] - OffsetZ;
                //      MakingErrTable.Zheight1[i] = MakingErrTable.Zheight1[i] - OffsetZH;
                //  }

                //  DeviationS = ErrorCompTable.TBL_Y_STRAIGHT[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_STRAIGHT[0].ErrorValue;
                //  DeviationL = ErrorCompTable.TBL_Y_LINEAR[DataUpperBound - 2].ErrorValue - ErrorCompTable.TBL_Y_LINEAR[0].ErrorValue;
                //  DeviationZ = MakingErrTable.Zheight1[DataUpperBound - 2] - MakingErrTable.Zheight1[0];

                //  //for(int i=0;i<=DataUpperBound;i++)
                //  //{
                //  //    ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue = ErrorCompTable.TBL_Y_LINEAR[i].ErrorValue - DeviationL * 
                //  //         ((ErrorCompTable.TBL_Y_LINEAR[i].Position - ErrorCompTable.TBL_Y_LINEAR[0].Position) 
                //  //         / (ErrorCompTable.TBL_Y_LINEAR[DataUpperBound - 2].Position - ErrorCompTable.TBL_Y_LINEAR[0].Position));
                //  //    MakingErrTable.Zheight1[i]= MakingErrTable.Zheight1[i]-DeviationL*
                //  //         ((MakingErrTable.Zheight1pos[i] - MakingErrTable.Zheight1pos[0])
                //  //         / (MakingErrTable.Zheight1pos[DataUpperBound - 2] - MakingErrTable.Zheight1pos[0]));
                //  //}
                #endregion
                SaveMakeErrorDataY();
                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }



        public int MakeStraightYWrite(int index) //0=> low ,  1=>mid,   2=>high
        {
            int retVal = -1;
            try
            {
            string dirPath = @"C:\ProberSystem\Parameters\VisionMapping\ErrorData2\";
            string filePath = "";
            List<double> errValues = null;
            List<double> postions = new List<double>();
            switch (index)
            {
                case 0:
                    filePath = "StraightYlow.err";
                    errValues = MakingErrTable.STRAIGHT_Y_LOW;
                    for (int i = 0; i < MakingErrTable.STRAIGHT_Y_LOW.Count; i++)
                    {
                        postions.Add(0);
                        postions[i] = (ErrorDataYTable[0].ErrorData_VER[i].YPos + ErrorDataYTable[1].ErrorData_VER[i].YPos) / 2;
                    }

                    break;
                case 1:
                    filePath = "StraightYmid.err";
                    errValues = MakingErrTable.STRAIGHT_Y_MID;
                    for (int i = 0; i < MakingErrTable.STRAIGHT_Y_MID.Count; i++)
                    {
                        postions.Add(0);
                        postions[i] = (ErrorDataYTable[2].ErrorData_VER[i].YPos + ErrorDataYTable[3].ErrorData_VER[i].YPos) / 2;
                    }
                    break;
                case 2:
                    filePath = "StraightYhigh.err";
                    errValues = MakingErrTable.STRAIGHT_Y_HIGH;
                    for (int i = 0; i < MakingErrTable.STRAIGHT_Y_HIGH.Count; i++)
                    {
                        postions.Add(0);
                        postions[i] = (ErrorDataYTable[4].ErrorData_VER[i].YPos + ErrorDataYTable[5].ErrorData_VER[i].YPos) / 2;
                    }
                    break;
                default:
                    break;
            }
            dirPath += filePath;

            string line;
            StreamWriter file = new System.IO.StreamWriter(dirPath);
            for (int i = 0; i < errValues.Count; i++)
            {
                line = string.Format("{0,-15:0.000}  {1,-15:0.000} ", errValues[i], postions[i]);
                file.WriteLine(line);
            }
            file.Close();
            retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public void DeInitModule()
        {
        }

        #region pattern
        //public async void RegPattern(int GrabSizeX,int GrabSizeY)
        //{
        //    FocusParameter focusParam = new FocusParameter(WaferAligner.WADeviceFile.WaferFocusParam);
        //    focusParam.FocusingCam = CurrentMappingCam.CamType;

        //    await Task.Run(() => WaferAligner.WaferFocusingModule.Focusing_Retry(
        //        FocusingType.WAFER,
        //        focusParam,
        //        false,//==> Light Change
        //        false,//==> brute force retry
        //        false));//==> find potential

        //    ImageBuffer image = null;
        //    VisionManager.RegistPatt(
        //       CurrentMappingCam.CamType, GrabSizeX, GrabSizeY,
        //       (int)((GrabSizeX / 2) - (VMSetupDisPlay.TargetRectangleWidth / 2)),
        //       (int)((GrabSizeX / 2) - (VMSetupDisPlay.TargetRectangleHeight / 2)),
        //       (int)VMSetupDisPlay.TargetRectangleWidth, (int)VMSetupDisPlay.TargetRectangleHeight,
        //       0, ErrMappingParam.PMParam.PatternPath);

        //    double xPos, yPos;
        //    PatterMatching(out xPos, out yPos);
        //    CurrentMappingCam.VMPos.X.Value = xPos;
        //    CurrentMappingCam.VMPos.Y.Value = yPos;
        //    CurrentMappingCam.VMPos.Z.Value = MotionManager.GetAxis(EnumAxisConstants.Z).Status.Position.Command;
        //    CurrentMappingCam.RelPos = new CatCoordinates(CurrentMappingCam.VMPos.X.Value + CurrentMappingCam.CamPos.X.Value, CurrentMappingCam.VMPos.Y.Value + CurrentMappingCam.CamPos.Y.Value);
        //    MotionManager.StageMove(xPos, yPos);
        //    SaveErrorMappingParameter();
        //}
        //private int PatterMatching(out double ptPosX, out double ptPosY)
        //{
        //    int retVal = 0;
        //    ptPosX = 0;
        //    ptPosY = 0;
        //    try
        //    {
        //        var camType = CurrentMappingCam.CamType;
        //        PMResult pmresult = VisionManager.Patten_Matching(camType, new PMParameter(), ErrMappingParam.PMParam.PatternPath, 0, 0);

        //        if (pmresult.ResultParam.Count == 1)
        //        {
        //            double machinex = MotionManager.GetAxis(EnumAxisConstants.X).Status.Position.Command;
        //            double machiney = MotionManager.GetAxis(EnumAxisConstants.Y).Status.Position.Command;

        //            double ptxpos = pmresult.ResultParam[0].XPoss;
        //            double ptypos = pmresult.ResultParam[0].YPoss;

        //            double offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
        //            ptPosX = machinex + (offsetx * VMSetupDisPlay.AssignedCamera.Param.RatioX.Value);

        //            double offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
        //            ptPosY = machiney - (offsety * VMSetupDisPlay.AssignedCamera.Param.RatioY.Value);

        //            return 1;
        //        }
        //        else
        //        {
        //            retVal = -1;
        //            return retVal;
        //        }
        //    }
        //    catch
        //    {
        //        retVal = -1;
        //        return retVal;
        //    }
        //}
        //private int PatterMatchingOffset(out double ptOffsetPosX, out double ptOffsetPosY)
        //{
        //    int retVal = 0;
        //    ptOffsetPosX = 0;
        //    ptOffsetPosY = 0;
        //    try
        //    {
        //        var camType = CurrentMappingCam.CamType;
        //        PMResult pmresult = VisionManager.Patten_Matching(camType, new PMParameter(), ErrMappingParam.PMParam.PatternPath, 0, 0);

        //        if (pmresult.ResultParam.Count == 1)
        //        {

        //            double ptxpos = pmresult.ResultParam[0].XPoss;
        //            double ptypos = pmresult.ResultParam[0].YPoss;

        //            double offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
        //            ptOffsetPosX = -(offsetx * VMSetupDisPlay.AssignedCamera.Param.RatioX.Value);

        //            double offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
        //            ptOffsetPosY = (offsety * VMSetupDisPlay.AssignedCamera.Param.RatioY.Value);

        //            return 1;
        //        }
        //        else
        //        {
        //            retVal = -1;
        //            return retVal;
        //        }
        //    }
        //    catch
        //    {
        //        retVal = -1;
        //        return retVal;
        //    }
        //}
        #endregion

    }
    public class ErrorMappingParam : INotifyPropertyChanged, IParam
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private double _DieSizeX;
        public double DieSizeX
        {
            get { return _DieSizeX; }
            set
            {
                if (value != _DieSizeX)
                {
                    _DieSizeX = value;
                    NotifyPropertyChanged("DieSizeX");
                }
            }
        }
        private double _DieSizeY;
        public double DieSizeY
        {
            get { return _DieSizeY; }
            set
            {
                if (value != _DieSizeY)
                {
                    _DieSizeY = value;
                    NotifyPropertyChanged("DieSizeY");
                }
            }
        }
        private PatternInfomation _PMParam_X;
        [XmlIgnore, JsonIgnore]
        public PatternInfomation PMParam_X
        {
            get { return _PMParam_X; }
            set
            {
                if (value != _PMParam_X)
                {
                    _PMParam_X = value;
                    NotifyPropertyChanged("PMParam_X");
                }
            }
        }
        private PatternInfomation _PMParam_Y;
        [XmlIgnore, JsonIgnore]
        public PatternInfomation PMParam_Y
        {
            get { return _PMParam_Y; }
            set
            {
                if (value != _PMParam_Y)
                {
                    _PMParam_Y = value;
                    NotifyPropertyChanged("PMParam_Y");
                }
            }
        }



        private ObservableCollection<ErrorMappingCamera> _ErrorMappingCameras;
        public ObservableCollection<ErrorMappingCamera> ErrorMappingCameras
        {
            get { return _ErrorMappingCameras; }
            set
            {
                if (value != _ErrorMappingCameras)
                {
                    _ErrorMappingCameras = value;
                    NotifyPropertyChanged("ErrorMappingCameras");
                }
            }
        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "VisionMapping";

        public string FileName { get; } = "VisionMappingParameter.json";

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        = new List<object>();

        public void DefalutSetting()
        {
            try
            {
            _ErrorMappingCameras = new ObservableCollection<ErrorMappingCamera>();
            _DieSizeX = 7172.6;
            _DieSizeY = 8463;
            _ErrorMappingCameras.Add(new ErrorMappingCamera(1, new CatCoordinates(-85100, -100000), new CatCoordinates(20525.8906768073, -46890.014937716995, -28165.627823243947, -467.646431260821), new CatCoordinates(20525.8906768073, -46890.014937716995, -12965.627823243947, -467.646431260821), EnumProberCam.MAP_1_CAM));
            _ErrorMappingCameras.Add(new ErrorMappingCamera(2, new CatCoordinates(84900, -100000), new CatCoordinates(-16730.786908506641, -46902.060649461069, -28165.143829442603, -467.646431260821), new CatCoordinates(-16730.786908506641, -46902.060649461069, -12626.143829442603, -467.646431260821), EnumProberCam.MAP_2_CAM));
            _ErrorMappingCameras.Add(new ErrorMappingCamera(3, new CatCoordinates(-85100, 100000), new CatCoordinates(21773.417362254608, 17634.970417119996, -28165.955501571303, -467.646431260821), new CatCoordinates(21773.417362254608, 17634.970417119996, -12643.955501571303, -467.646431260821), EnumProberCam.MAP_3_CAM));
            _ErrorMappingCameras.Add(new ErrorMappingCamera(4, new CatCoordinates(84900, 100000), new CatCoordinates(-16915.018654439718, 17696.884685563204, -28165.676463127098, -467.646431260821), new CatCoordinates(-16915.018654439718, 17696.884685563204, -12427.676463127098, -467.646431260821), EnumProberCam.MAP_4_CAM));
            _ErrorMappingCameras.Add(new ErrorMappingCamera(5, new CatCoordinates(-84700, 477000), new CatCoordinates(), new CatCoordinates(), EnumProberCam.MAP_5_CAM));
            _ErrorMappingCameras.Add(new ErrorMappingCamera(6, new CatCoordinates(84700, 477000), new CatCoordinates(), new CatCoordinates(), EnumProberCam.MAP_6_CAM));
            _ErrorMappingCameras.Add(new ErrorMappingCamera(7, new CatCoordinates(-85000, 277500), new CatCoordinates(), new CatCoordinates(), EnumProberCam.MAP_7_CAM));
            _ErrorMappingCameras.Add(new ErrorMappingCamera(8, new CatCoordinates(85000, 277500), new CatCoordinates(), new CatCoordinates(), EnumProberCam.MAP_8_CAM));
            //_ErrorMappingCameras[0].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam1_BAndW.mmo");
            //_ErrorMappingCameras[1].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam2_BAndW.mmo");
            //_ErrorMappingCameras[2].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam3_BAndW.mmo");
            //_ErrorMappingCameras[3].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam4_BAndW.mmo");
            //_ErrorMappingCameras[4].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam5_BAndW.mmo");
            //_ErrorMappingCameras[5].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam6_BAndW.mmo");
            //_ErrorMappingCameras[6].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam7_BAndW.mmo");
            //_ErrorMappingCameras[7].PMParam = new PatternInfomation(@"C:\ProberSystem\Parameters\VisionMapping\Pattern\MapCam8_BAndW.mmo");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }
    }


    public class RelPosConverter : IValueConverter
    {
        private CatCoordinates coord;

        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            ICamera camera = (ICamera)value;
            ICoordinateManager coordmanager;
            EnumProberCam cam = (EnumProberCam)value;
            try
            {
                if (parameter != null)
                {
                    if (parameter is ICoordinateManager)
                    {
                        coordmanager = (ICoordinateManager)parameter;
                        switch (cam)
                        {
                            case EnumProberCam.WAFER_HIGH_CAM:
                                coord = coordmanager.WaferHighChuckConvert.CurrentPosConvert();
                                break;
                            case EnumProberCam.WAFER_LOW_CAM:
                                coord = coordmanager.WaferLowChuckConvert.CurrentPosConvert();
                                break;
                            case EnumProberCam.PIN_HIGH_CAM:
                                coord = coordmanager.PinHighPinConvert.CurrentPosConvert();
                                break;
                            case EnumProberCam.PIN_LOW_CAM:
                                coord = coordmanager.PinLowPinConvert.CurrentPosConvert();
                                break;
                        }
                        coord.X.Value = Math.Truncate(coord.X.Value);
                        coord.Y.Value = Math.Truncate(coord.Y.Value);
                        coord.Z.Value = Math.Truncate(coord.Z.Value);
                    }
                }
                else
                {
                    coord.X.Value = 0.0;
                    coord.Y.Value = 0.0;
                    coord.Z.Value = 0.0;
                }

            }
            catch (Exception err)
            {
                coord.X.Value = 0.0;
                coord.Y.Value = 0.0;
                coord.Z.Value = 0.0;
                LoggerManager.Exception(err);
            }

            return coord;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }
}
