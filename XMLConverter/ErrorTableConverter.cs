using System;
using System.Collections.Generic;
using System.IO;
using ErrorParam;
using ProberErrorCode;
using LogModule;
using ProberInterfaces;

namespace XMLConverter
{
    public enum EnumAxisConstants
    {
        X = 1,
        Y = 2,
        Z = 3,
        C = 4,
        A = 5,
        U1 = 6,
        U2 = 7,
        W = 8,
        V = 9,
        CC = 10,
        NC = 11,
        MAX_STAGE_AXIS = 11
    }
    public class Table_Constants
    {
        public const int MAX_ERR_TABLE = 512;
        public const int ERROR_TYPE_LINEAR = 1;
        public const int ERROR_TYPE_STRAIGHT = 2;
        public const int ERROR_TYPE_ANGULAR = 3;
        public const int ERROR_TYPE_ZA = 4;
        public const int ERROR_TYPE_ZH = 5;
    }

 

    public class ErrorTableConverter: IFactoryModule
    {

        public FirstErrorTable FirstErrTable = new FirstErrorTable();
        public SecondErrorTable SecondErrTable = new SecondErrorTable();
    
     
        private double[] POS_TBL_OX_EXT = new double[Table_Constants.MAX_ERR_TABLE];

        // "D:\VM\FinalData\linear0.err"
        private double[,] TBL_X_LINEAR_EXT = new double[Table_Constants.MAX_ERR_TABLE, Table_Constants.MAX_ERR_TABLE];

        // "D:\VM\FinalData\straight0.err"
        private double[,] TBL_X_STRAIGHT_EXT = new double[Table_Constants.MAX_ERR_TABLE, Table_Constants.MAX_ERR_TABLE];

        // "D:\VM\FinalData\linear0.err"
        private double[] POS_TBL_X_EXT = new double[Table_Constants.MAX_ERR_TABLE];

        // "D:\VM\FinalData\linear1.err"
        private double[] POS_TBL_Y_EXT = new double[Table_Constants.MAX_ERR_TABLE];
    
        //string ErrorTable2DFilePath = "C:\\PROBERPARAM\\ErrorData\\ErrorTable2D.XML";

        public EventCodeEnum ConvertError1DTable(string filePath)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            retVal = LoadError1DTable();

            if (retVal == EventCodeEnum.NODATA)
            {
                return retVal;
            }
            else
            {
                try
                {
                    retVal = Extensions_IParam.SaveParameter(null, FirstErrTable, null, filePath);
                }
                catch (Exception err)
                {
                    retVal = EventCodeEnum.PARAM_ERROR;

                    //LoggerManager.Error($String.Format("LoadMotionParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public SecondErrorTable ResetError2DTable()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            IParam tmpParam = null;
            RetVal = this.LoadParameter(ref tmpParam, typeof(FirstErrorTable));

            if (RetVal == EventCodeEnum.NONE)
            {
                FirstErrTable = tmpParam as FirstErrorTable;
                Convert2DTable();
            }

            return SecondErrTable;
        }
        public EventCodeEnum ConvertError2DTable(string filePath)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            RetVal = LoadError1DTable();

            if(RetVal == EventCodeEnum.NODATA)
            {
                return RetVal;
            }

            RetVal = LoadErrTableData(this.FileManager().GetSystemRootPath() + @"\ErrorData\\ExtraXtabledata.dat");

            if (RetVal == EventCodeEnum.NODATA)
            {
                return RetVal;
            }

            CalculateOffsetTable();

            try
            {
                RetVal = Extensions_IParam.SaveParameter(null, SecondErrTable, null, filePath);

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("LoadMotionParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }
        public EventCodeEnum ConvertError2DPinTable(string filePath)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            retVal = LoadError1DTable();

            if (retVal == EventCodeEnum.NODATA)
            {
                return retVal;
            }
            retVal = LoadErrTableData(this.FileManager().GetSystemRootPath() + @"\ErrorData\ExtraXtabledataPin.dat");

            if (retVal == EventCodeEnum.NODATA)
            {
                return retVal;
            }

            CalculateOffsetTable();

            try
            {
                retVal = Extensions_IParam.SaveParameter(null, SecondErrTable, null, filePath);

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PARAM_ERROR; ;
                //LoggerManager.Error($String.Format("LoadMotionParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        private EventCodeEnum LoadError1DTable()
        {
            EventCodeEnum Result = EventCodeEnum.UNDEFINED;
            try
            {
                FirstErrTable.TBL_X_LINEAR.Clear();
                FirstErrTable.TBL_X_STRAIGHT.Clear();
                FirstErrTable.TBL_X_ANGULAR.Clear();
                FirstErrTable.TBL_Y_LINEAR.Clear();
                FirstErrTable.TBL_Y_STRAIGHT.Clear();
                FirstErrTable.TBL_Y_ANGULAR.Clear();
                for (int iaxis = 1; iaxis <= 2; iaxis++)
            {
                for (int err_type = 1; err_type <= 3; err_type++)
                {
                    int i;
                    string LoadErrorDataPath = this.FileManager().GetSystemRootPath()+@"\VisionMapping\ErrorData\";

                    switch (iaxis)
                    {
                        case (int)EnumAxisConstants.X:
                            switch (err_type)
                            {
                                case Table_Constants.ERROR_TYPE_LINEAR:
                                    LoadErrorDataPath = LoadErrorDataPath + "linear0.err";
                                    break;
                                case Table_Constants.ERROR_TYPE_STRAIGHT:
                                    LoadErrorDataPath = LoadErrorDataPath + "Straight0.err";
                                    break;
                                case Table_Constants.ERROR_TYPE_ANGULAR:
                                    LoadErrorDataPath = LoadErrorDataPath + "Angular0.err";
                                    break;
                            }

                            break;

                        case (int)EnumAxisConstants.Y:
                            switch (err_type)
                            {
                                case Table_Constants.ERROR_TYPE_LINEAR:
                                    LoadErrorDataPath = LoadErrorDataPath + "Linear1.err";
                                    break;
                                case Table_Constants.ERROR_TYPE_STRAIGHT:
                                    LoadErrorDataPath = LoadErrorDataPath + "Straight1.err";
                                    break;
                                case Table_Constants.ERROR_TYPE_ANGULAR:
                                    LoadErrorDataPath = LoadErrorDataPath + "Angular1.err";
                                    break;
                            }
                            break;
                        case (int)EnumAxisConstants.Z:
                            switch (err_type)
                            {
                                case Table_Constants.ERROR_TYPE_ANGULAR:
                                    LoadErrorDataPath = LoadErrorDataPath + "Angular2Ex.err";
                                    break;
                            }

                            break;

                        case (int)EnumAxisConstants.C:
                            switch (err_type)
                            {
                                case Table_Constants.ERROR_TYPE_LINEAR:
                                    LoadErrorDataPath = LoadErrorDataPath + "Linear3Ex.err";
                                    break;
                            }

                            break;
                        default:
                            break;
                    }

                    // File  Check
                    try
                    {
                        if (File.Exists(LoadErrorDataPath) == false)
                        {
                            return EventCodeEnum.NODATA;
                        }
                        else
                        {
                            Result = EventCodeEnum.NONE;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"SetErrorTable() Function error: " + err.Message);

                        throw;
                    }


                    string line;
                    StreamReader file = new System.IO.StreamReader(@LoadErrorDataPath);

                    i = 0;

                    switch (iaxis)
                    {
                        case (int)EnumAxisConstants.X:

                            switch (err_type)
                            {
                                case Table_Constants.ERROR_TYPE_LINEAR:

                                    while ((line = file.ReadLine()) != null)
                                    {
                                        FirstErrTable.TBL_X_LINEAR.Add(new ErrorParameter());
                                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        double.TryParse(parts[0], out FirstErrTable.TBL_X_LINEAR[i].ErrorValue);
                                        double.TryParse(parts[1], out FirstErrTable.TBL_X_LINEAR[i].Position);

                                        i++;
                                    }

                                    file.Close();

                                    break;

                                case Table_Constants.ERROR_TYPE_STRAIGHT:
                                    while ((line = file.ReadLine()) != null)
                                    {
                                        FirstErrTable.TBL_X_STRAIGHT.Add(new ErrorParameter());
                                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        double.TryParse(parts[0], out FirstErrTable.TBL_X_STRAIGHT[i].ErrorValue);
                                        double.TryParse(parts[1], out FirstErrTable.TBL_X_STRAIGHT[i].Position);

                                        i++;
                                    }

                                    file.Close();

                                    break;

                                case Table_Constants.ERROR_TYPE_ANGULAR:


                                    while ((line = file.ReadLine()) != null)
                                    {
                                        FirstErrTable.TBL_X_ANGULAR.Add(new ErrorParameter());
                                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        double.TryParse(parts[0], out FirstErrTable.TBL_X_ANGULAR[i].ErrorValue);
                                        double.TryParse(parts[1], out FirstErrTable.TBL_X_ANGULAR[i].Position);

                                        i++;
                                    }

                                    file.Close();

                                    break;

                                default:
                                    break;
                            }

                            break;

                        case (int)EnumAxisConstants.Y:

                            switch (err_type)
                            {
                                case Table_Constants.ERROR_TYPE_LINEAR:

                                    while ((line = file.ReadLine()) != null)
                                    {
                                        FirstErrTable.TBL_Y_LINEAR.Add(new ErrorParameter());
                                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        double.TryParse(parts[0], out FirstErrTable.TBL_Y_LINEAR[i].ErrorValue);
                                        double.TryParse(parts[1], out FirstErrTable.TBL_Y_LINEAR[i].Position);

                                        i++;
                                    }

                                    file.Close();

                                    break;

                                case Table_Constants.ERROR_TYPE_STRAIGHT:



                                    while ((line = file.ReadLine()) != null)
                                    {
                                        FirstErrTable.TBL_Y_STRAIGHT.Add(new ErrorParameter());
                                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        double.TryParse(parts[0], out FirstErrTable.TBL_Y_STRAIGHT[i].ErrorValue);
                                        double.TryParse(parts[1], out FirstErrTable.TBL_Y_STRAIGHT[i].Position);

                                        i++;
                                    }

                                    file.Close();

                                    break;

                                case Table_Constants.ERROR_TYPE_ANGULAR:



                                    while ((line = file.ReadLine()) != null)
                                    {
                                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        FirstErrTable.TBL_Y_ANGULAR.Add(new ErrorParameter());
                                        double.TryParse(parts[0], out FirstErrTable.TBL_Y_ANGULAR[i].ErrorValue);
                                        double.TryParse(parts[1], out FirstErrTable.TBL_Y_ANGULAR[i].Position);

                                        i++;
                                    }

                                    file.Close();

                                    break;

                                default:
                                    break;
                            }

                            break;

                        case (int)EnumAxisConstants.Z:

                            switch (err_type)
                            {
                                case Table_Constants.ERROR_TYPE_LINEAR:

                                    return (short)0;

                                case Table_Constants.ERROR_TYPE_STRAIGHT:

                                    return (short)0;

                                case Table_Constants.ERROR_TYPE_ANGULAR:

                                    while ((line = file.ReadLine()) != null)
                                    {
                                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        i++;
                                    }

                                    file.Close();

                                    break;

                                default:
                                    break;
                            }

                            break;

                        default:
                            break;
                    }        

                    Result = EventCodeEnum.NONE;
                }
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return Result;
        }
        public EventCodeEnum LoadErrTableData(string LoadPath)
        {
            //LoggerManager.Debug($string.Format("MainWindow(): Initilizing error. Err = {0}"), );
            // Open Error Table
            if (File.Exists(LoadPath) == false)
            {
                return EventCodeEnum.NODATA;
            }

            long fileSize = new FileInfo(LoadPath).Length;

            // Check File Size
            if (fileSize != (sizeof(double) * ((2 * Table_Constants.MAX_ERR_TABLE * Table_Constants.MAX_ERR_TABLE) + (2 * Table_Constants.MAX_ERR_TABLE))))
            {
                //sprintf(buf, "[LoadErrTableData] File Size Error\n");
                //TRASE(buf);

                return EventCodeEnum.NODATA;
            }

            try
            {
                FileInfo fi = new FileInfo(@LoadPath);

                BinaryReader br = new BinaryReader(fi.OpenRead());

                // (1) TBL_X_LINEAR_EXT

                int k = 0;
                int j = 0;

                long ReadCount = Table_Constants.MAX_ERR_TABLE * Table_Constants.MAX_ERR_TABLE;

                for (int i = 0; i < ReadCount; i++)
                {
                    TBL_X_LINEAR_EXT[k, j] = br.ReadDouble();

                    k++;

                    if ((i != 0) && ((i + 1) % (Table_Constants.MAX_ERR_TABLE)) == 0)
                    {
                        k = 0;
                        j++;
                    }
                }

                k = 0;
                j = 0;

                // (2) TBL_X_STRAIGHT_EXT
                for (long i = 0; i < ReadCount; i++)
                {
                    TBL_X_STRAIGHT_EXT[k, j] = br.ReadDouble();

                    k++;

                    if ((i != 0) && ((i + 1) % (Table_Constants.MAX_ERR_TABLE)) == 0)
                    {
                        k = 0;
                        j++;
                    }
                }

                double tmpMinPos = 999999;
                double tmpMaxPos = -999999;

                // (3) POS_TBL_X_EXT
                for (int i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                {
                    POS_TBL_X_EXT[i] = br.ReadDouble();

                    if (POS_TBL_X_EXT[i] < tmpMinPos)
                    {
                        tmpMinPos = POS_TBL_X_EXT[i];
                    }

                    if (POS_TBL_X_EXT[i] > tmpMaxPos)
                    {
                        tmpMaxPos = POS_TBL_X_EXT[i];
                    }
                }

                tmpMinPos = 999999;
                tmpMaxPos = -999999;

                // (4) POS_TBL_Y_EXT
                for (int i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                {
                    POS_TBL_Y_EXT[i] = br.ReadDouble();

                    if (POS_TBL_Y_EXT[i] < tmpMinPos)
                    {
                        tmpMinPos = POS_TBL_Y_EXT[i];
                    }

                    if (POS_TBL_Y_EXT[i] > tmpMaxPos)
                    {
                        tmpMaxPos = POS_TBL_Y_EXT[i];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"LoadErrTableData() Function error: " + err.Message);
            }

            return EventCodeEnum.NONE;
        }

        public void Convert2DTable()
        {
            try
            {

            int tmpxCnt_X2 = 0;

            int tmpxCnt_Y2 = 0;
   
            int Cnt_1D_X;

            int Cnt_1D_Y;

            SecondErrTable = new SecondErrorTable();
            Cnt_1D_X = FirstErrTable.TBL_X_LINEAR.Count;


            Cnt_1D_Y = FirstErrTable.TBL_Y_LINEAR.Count;

            for (tmpxCnt_Y2 = 0; tmpxCnt_Y2 < Cnt_1D_Y; tmpxCnt_Y2++)
            {
                SecondErrTable.TBL_OX_LINEAR.Add(new ErrorParameter2D());
                SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].ListY = new List<ErrorParameter2D_X>();
                SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].PositionY = FirstErrTable.TBL_Y_LINEAR[tmpxCnt_Y2].Position;


                SecondErrTable.TBL_OX_STRAIGHT.Add(new ErrorParameter2D());
                SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].ListY = new List<ErrorParameter2D_X>();
                SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].PositionY = FirstErrTable.TBL_Y_STRAIGHT[tmpxCnt_Y2].Position;

                for (tmpxCnt_X2 = 0; tmpxCnt_X2 < Cnt_1D_X; tmpxCnt_X2++)
                {
                    var xLinear = new ErrorParameter2D_X(FirstErrTable.TBL_X_LINEAR[tmpxCnt_X2].Position, FirstErrTable.TBL_X_LINEAR[tmpxCnt_X2].ErrorValue);
                    SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].ListY.Add(xLinear);
                    var xStraight = new ErrorParameter2D_X(FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X2].Position, FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X2].ErrorValue);
                    SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].ListY.Add(xStraight);
                }
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public EventCodeEnum CalculateOffsetTable()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            int tmpxCnt_X1 = 0;
            int tmpxCnt_X2 = 0;

            //int tmpxCnt_Y1 = 0;
            int tmpxCnt_Y2 = 0;

            //double EXT_MIN_POS;
            //double EXT_MAX_POS;

            double EXT_Prev_POS;
            double EXT_Next_POS;

            int Cnt_1D_X;
          //  int Cnt_2D_X;

            int Cnt_1D_Y;
          //  int Cnt_2D_Y;


            int i = 0, j = 0;

            //int OffsetCount;

            int type;

            double tmpErrorValue;

            int LastIndex_X1;

            // Get extra data count

            while ((POS_TBL_X_EXT[i + 1] - POS_TBL_X_EXT[i]) > 0)
            {
                i++;
            }
           // Cnt_2D_X = i + 1;
            Cnt_1D_X = FirstErrTable.TBL_X_LINEAR.Count;
            
            while ((POS_TBL_Y_EXT[j + 1] - POS_TBL_Y_EXT[j]) > 0)
            {
                j++;
            }

            Cnt_1D_Y = FirstErrTable.TBL_Y_LINEAR.Count;
            //Cnt_2D_Y = j + 1;

            ////

            // (1) Linear

            type = Table_Constants.ERROR_TYPE_LINEAR;

            for (tmpxCnt_Y2 = 0; tmpxCnt_Y2 < Cnt_1D_Y; tmpxCnt_Y2++)
            {
                SecondErrTable.TBL_OX_LINEAR.Add(new ErrorParameter2D(POS_TBL_Y_EXT[tmpxCnt_Y2]));
                SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].ListY = new List<ErrorParameter2D_X>();

                LastIndex_X1 = 0;

                for (tmpxCnt_X2 = 0; tmpxCnt_X2 < Cnt_1D_X; tmpxCnt_X2++)
                {
                    EXT_Prev_POS = POS_TBL_X_EXT[tmpxCnt_X2];
                    EXT_Next_POS = POS_TBL_X_EXT[tmpxCnt_X2 + 1];

                    for (tmpxCnt_X1 = LastIndex_X1; tmpxCnt_X1 < Cnt_1D_X; tmpxCnt_X1++)
                    {
                        if ((FirstErrTable.TBL_X_LINEAR[tmpxCnt_X1].Position < EXT_Prev_POS))
                        {

                        }
                        else if (FirstErrTable.TBL_X_LINEAR[tmpxCnt_X1].Position == EXT_Prev_POS)
                        {
                            AddOffsetValue(type, EXT_Prev_POS, TBL_X_LINEAR_EXT[tmpxCnt_Y2, tmpxCnt_X2] - FirstErrTable.TBL_X_LINEAR[tmpxCnt_X1].ErrorValue);
                        }
                        else if ((FirstErrTable.TBL_X_LINEAR[tmpxCnt_X1].Position > EXT_Prev_POS) && (FirstErrTable.TBL_X_LINEAR[tmpxCnt_X1].Position < EXT_Next_POS))
                        {
                            AddOffsetValue(type, EXT_Prev_POS, TBL_X_LINEAR_EXT[tmpxCnt_Y2, tmpxCnt_X2]);

                            tmpErrorValue = CalculateOffsetValue(type, tmpxCnt_X1, tmpxCnt_Y2, tmpxCnt_X2);

                            AddOffsetValue(type, FirstErrTable.TBL_X_LINEAR[tmpxCnt_X2].Position, tmpErrorValue);
                        }
                        else if (FirstErrTable.TBL_X_LINEAR[tmpxCnt_X1].Position == EXT_Next_POS)
                        {
                            AddOffsetValue(type, EXT_Next_POS, TBL_X_LINEAR_EXT[tmpxCnt_Y2, tmpxCnt_X2 + 1] - FirstErrTable.TBL_X_LINEAR[tmpxCnt_X1].ErrorValue);
                        }
                        else
                        {
                            LastIndex_X1 = tmpxCnt_X1;

                            break;
                        }
                    }
                }
            }

            // (2) Straightness

            type = Table_Constants.ERROR_TYPE_STRAIGHT;

            for (tmpxCnt_Y2 = 0; tmpxCnt_Y2 < Cnt_1D_Y; tmpxCnt_Y2++)
            {
                SecondErrTable.TBL_OX_STRAIGHT.Add(new ErrorParameter2D(POS_TBL_Y_EXT[tmpxCnt_Y2]));
                SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].ListY = new List<ErrorParameter2D_X>();

                LastIndex_X1 = 0;

                for (tmpxCnt_X2 = 0; tmpxCnt_X2 < Cnt_1D_X; tmpxCnt_X2++)
                {
                    EXT_Prev_POS = POS_TBL_X_EXT[tmpxCnt_X2];
                    EXT_Next_POS = POS_TBL_X_EXT[tmpxCnt_X2 + 1];

                    for (tmpxCnt_X1 = LastIndex_X1; tmpxCnt_X1 < Cnt_1D_X; tmpxCnt_X1++)
                    {
                        if ((FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X1].Position < EXT_Prev_POS))
                        {

                        }
                        else if (FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X1].Position == EXT_Prev_POS)
                        {
                            AddOffsetValue(type, EXT_Prev_POS, TBL_X_STRAIGHT_EXT[tmpxCnt_Y2, tmpxCnt_X2] - FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X1].ErrorValue);
                        }
                        else if ((FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X1].Position > EXT_Prev_POS) && (FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X1].Position < EXT_Next_POS))
                        {
                            AddOffsetValue(type, EXT_Prev_POS, TBL_X_STRAIGHT_EXT[tmpxCnt_Y2, tmpxCnt_X2]);

                            tmpErrorValue = CalculateOffsetValue(type, tmpxCnt_X1, tmpxCnt_Y2, tmpxCnt_X2);

                            AddOffsetValue(type, FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X2].Position, tmpErrorValue);
                        }
                        else if (FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X1].Position == EXT_Next_POS)
                        {
                            AddOffsetValue(type, EXT_Next_POS, TBL_X_STRAIGHT_EXT[tmpxCnt_Y2, tmpxCnt_X2 + 1] - FirstErrTable.TBL_X_STRAIGHT[tmpxCnt_X1].ErrorValue);
                        }
                        else
                        {
                            LastIndex_X1 = tmpxCnt_X1;

                            break;
                        }
                    }
                }
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public void AddOffsetValue(int Type, double PosX, double Evalue)
        {
            try
            {
                if (Type == Table_Constants.ERROR_TYPE_LINEAR)
                {
                    SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].ListY.Add(new ErrorParameter2D_X(PosX, Evalue));
                }
                else if (Type == Table_Constants.ERROR_TYPE_STRAIGHT)
                {
                    SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].ListY.Add(new ErrorParameter2D_X(PosX, Evalue));
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public double CalculateOffsetValue(int Type, int Index_1D, int Index_2D_Y, int Index_2D_X)
        {
            double Offset = 0;

            if (Type == Table_Constants.ERROR_TYPE_LINEAR)
            {
                try
                {
                // Calculation Offset value

                Offset = (((FirstErrTable.TBL_X_LINEAR[Index_1D].Position - POS_TBL_X_EXT[Index_2D_X]) / (POS_TBL_X_EXT[Index_2D_X + 1] - POS_TBL_X_EXT[Index_2D_X])) *
                         (TBL_X_LINEAR_EXT[Index_2D_Y, Index_2D_X + 1] - TBL_X_LINEAR_EXT[Index_2D_Y, Index_2D_X])) +
                         TBL_X_LINEAR_EXT[Index_2D_Y, Index_2D_X];
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                     throw;
                }
            }
            else if (Type == Table_Constants.ERROR_TYPE_STRAIGHT)
            {
                try
                {

                Offset = (((FirstErrTable.TBL_X_STRAIGHT[Index_1D].Position - POS_TBL_X_EXT[Index_2D_X]) / (POS_TBL_X_EXT[Index_2D_X + 1] - POS_TBL_X_EXT[Index_2D_X])) *
                         (TBL_X_STRAIGHT_EXT[Index_2D_Y, Index_2D_X + 1] - TBL_X_STRAIGHT_EXT[Index_2D_Y, Index_2D_X])) +
                         TBL_X_STRAIGHT_EXT[Index_2D_Y, Index_2D_X];
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                     throw;
                }
            }

            return Offset;
        }
    }
}
