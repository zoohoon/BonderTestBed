using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using ProberInterfaces;
using LogModule;

namespace ProbeMotion
{
    public class Table_Constants
    {
        public const int MAX_ERR_TABLE = 512;

        public const int ERROR_TYPE_LINEAR = 1;
        public const int ERROR_TYPE_STRAIGHT = 2;
        public const int ERROR_TYPE_ANGULAR = 3;
        public const int ERROR_TYPE_ZA = 4;
        public const int ERROR_TYPE_ZH = 5;

        public const string ERROR_DATA_PATH = "c:\\PROBERPARAM\\ErrorData\\";
    }

    public class INIManager
    {
        private String m_strINIPath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(String section, String key, String val, String filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(String section, String key, String def, StringBuilder retVal, int size, String filePath);

        public INIManager(String INIPath)
        {
            m_strINIPath = INIPath;
        }

        public bool ExistINI()
        {

            return File.Exists(m_strINIPath);
        }

        public void WriteValue(String strSection, String strKey, String strValue)
        {
            WritePrivateProfileString(strSection, strKey, strValue, m_strINIPath);
        }

        public void DeleteSection(String strSection)
        {
            WritePrivateProfileString(strSection, null, null, m_strINIPath);
        }

        public string ReadValue(String strSection, String Key, String strDefault)
        {
            StringBuilder strValue = new StringBuilder(255);

            int i = GetPrivateProfileString(strSection, Key, strDefault, strValue, 255, m_strINIPath);

            return strValue.ToString();
        }
    }

    static class PROBER_INI_Constants
    {
        public const string OPUS_INI = "c:\\PROBERFILES\\Parameter\\opus.ini";
    }

    public class ErrorTable : INotifyPropertyChanged
    {
        // Speed Table - need initialize
        //double gSpeed[MAX_STAGE_AXIS] =
        //{
        //10000.0, 10000.0, 10000.0, 10000.0,
        //10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0
        //};

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ErrorTable()
        {
        }

        public double[] gSpeed = new double[] { 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0 };

        public double[] gAccel = new double[] { 1000000.0, 1000000.0, 1000000.0, 1000000.0, 1000000.0, 1000000.0, 1000000.0, 1000000.0, 1000000.0, 1000000.0, 1000000.0 };

        public double[] gJerk = new double[] { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0 };

        public double[] gJerkP = new double[] { 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0 };

        public int ReadError(short iaxis, short err_type)
        {
            SetErrorTable(iaxis, err_type, gEMC[iaxis].intSampleNum, gEMC[iaxis].lngSampleStep, gEMC[iaxis].lngSampleOrigin, gEMC[iaxis].lngTotalLength);

            return 1;
        }

        private double _m_sqr_angle;
        public double m_sqr_angle
        {
            get { return _m_sqr_angle; }
            set
            {
                if (value != this._m_sqr_angle)
                {
                    _m_sqr_angle = value;
                    NotifyPropertyChanged("APos");
                }
            }
        }

        private short _isInspectionArea = 0;
        public short isInspectionArea
        {
            get { return _isInspectionArea; }
            set
            {
                if (value != this._isInspectionArea)
                {
                    _isInspectionArea = value;
                    NotifyPropertyChanged("APos");
                }
            }
        }

        private short _FlagExtraTableOn = 0;
        public short FlagExtraTableOn
        {
            get { return _FlagExtraTableOn; }
            set
            {
                if (value != this._FlagExtraTableOn)
                {
                    _FlagExtraTableOn = value;
                    NotifyPropertyChanged("APos");
                }
            }
        }

        private short _FlagExtraTableOnPin = 0;
        public short FlagExtraTableOnPin
        {
            get { return _FlagExtraTableOnPin; }
            set
            {
                if (value != this._FlagExtraTableOnPin)
                {
                    _FlagExtraTableOnPin = value;
                    NotifyPropertyChanged("APos");
                }
            }
        }

        private short _FlagUseExtraTableForPin = 0;
        public short FlagUseExtraTableForPin
        {
            get { return _FlagUseExtraTableForPin; }
            set
            {
                if (value != this._FlagUseExtraTableForPin)
                {
                    _FlagUseExtraTableForPin = value;
                    NotifyPropertyChanged("APos");
                }
            }
        }

        private short _FlagErrorMapOn = 1;
        public short FlagErrorMapOn
        {
            get { return _FlagErrorMapOn; }
            set
            {
                if (value != this._FlagErrorMapOn)
                {
                    _FlagErrorMapOn = value;
                    NotifyPropertyChanged("APos");
                }
            }
        }

        public enum FlgCOMP_MODE
        {
            COMP_MODE_INVALID = -1,

            COMP_MODE_X_LIN = 0,
            COMP_MODE_X_ANG = 1,
            COMP_MODE_X_STR = 2,
            COMP_MODE_Y_LIN = 3,
            COMP_MODE_Y_ANG = 4,
            COMP_MODE_Y_STR = 5,
            COMP_MODE_Y_ZA = 6,
            COMP_MODE_Y_ZH = 7,

            COMP_MODE_LAST = COMP_MODE_Y_STR + 1,
        }

        public struct TagERR_MAP_CONFIG
        {
            public int intSampleNum;
            public long lngSampleStep;
            public long lngSampleOrigin;
            public long lngTotalLength;
        }

        private INIManager mINIManager;


        // X
        public double[] TBL_X_LINEAR = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] TBL_X_STRAIGHT = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] TBL_X_ANGULAR = new double[Table_Constants.MAX_ERR_TABLE];

        public double[] POS_TBL_X_LINEAR = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_X_STRAIGHT = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_X_ANGULAR = new double[Table_Constants.MAX_ERR_TABLE];

        // Y
        public double[] TBL_Y_LINEAR = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] TBL_Y_STRAIGHT = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] TBL_Y_ANGULAR = new double[Table_Constants.MAX_ERR_TABLE];

        public double[] POS_TBL_Y_LINEAR = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_Y_STRAIGHT = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_Y_ANGULAR = new double[Table_Constants.MAX_ERR_TABLE];

        // Z
        public double[] TBL_Z_LINEAR = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] TBL_Z_STRAIGHT = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] TBL_ZY_ANGULAR = new double[Table_Constants.MAX_ERR_TABLE];

        public double[] POS_TBL_Z_LINEAR = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_Z_STRAIGHT = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_Z_ANGULAR = new double[Table_Constants.MAX_ERR_TABLE];

        public double[,] TBL_X_LINEAR_EXT = new double[Table_Constants.MAX_ERR_TABLE, Table_Constants.MAX_ERR_TABLE];
        public double[,] TBL_X_STRAIGHT_EXT = new double[Table_Constants.MAX_ERR_TABLE, Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_X_EXT = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_Y_EXT = new double[Table_Constants.MAX_ERR_TABLE];

        public double[,] TBL_X_LINEAR_EXT_FOR_PIN = new double[Table_Constants.MAX_ERR_TABLE, Table_Constants.MAX_ERR_TABLE];
        public double[,] TBL_X_STRAIGHT_EXT_FOR_PIN = new double[Table_Constants.MAX_ERR_TABLE, Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_X_EXT_FOR_PIN = new double[Table_Constants.MAX_ERR_TABLE];
        public double[] POS_TBL_Y_EXT_FOR_PIN = new double[Table_Constants.MAX_ERR_TABLE];

        public TagERR_MAP_CONFIG[] gEMC = new TagERR_MAP_CONFIG[5];

        public double PROB_AREA = 190000.0;

        public double MAX_X_STROKE = 300000.0;
        public double LINEAR_X_STEP = 75000.0;
        public double ANGULAR_X_STEP = 30000.0;
        public double STRAIGHT_X_STEP = 30000.0;
        public double LINEAR_X_START = 0.0;
        public double ANGULAR_X_START = 0.0;
        public double STRAIGHT_X_START = 0.0;

        double MAX_Y_STROKE = 600000.0;
        double LINEAR_Y_STEP = 75000.0;
        double ANGULAR_Y_STEP = 30000.0;
        double STRAIGHT_Y_STEP = 30000.0;
        double LINEAR_Y_START = 100000.0;
        double ANGULAR_Y_START = 0.0;
        double STRAIGHT_Y_START = 0.0;

        short MAX_TBL_X_STRAIGHT = 10;
        short MAX_TBL_X_LINEAR = 10;
        short MAX_TBL_X_ANGULAR = 12;

        short MAX_TBL_Y_STRAIGHT = 10;
        short MAX_TBL_Y_LINEAR = 10;
        short MAX_TBL_Y_ANGULAR = 20;

        short MAX_TBL_Z_ANGULAR = 12;
        double MAX_Z_STROKE = 20000.0;
        double ANGULAR_Z_STEP = 2000.0;
        double ANGULAR_Z_START = 0.0;

        //short MAX_TBL_C_LINEAR = 10;
        //double MAX_C_STROKE = 300000.0;
        //double MAX_C_STROKE = 75000.0;
        //double LINEAR_C_START = 0.0;

        public int LoadErrorMapConfig()
        {
            string strErrPath;

            // Get err table length
            var lineCount = 0;

            // X
            try
            {
                strErrPath = Table_Constants.ERROR_DATA_PATH + "linear0.err";

                if (File.Exists(strErrPath) == false)
                {
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"LoadErrorMapConfig() Function error: " + err.Message);

                throw;
            }

            lineCount = File.ReadLines(strErrPath).Count();

            gEMC[(int)EnumAxisConstants.X].intSampleNum = lineCount;

            mINIManager = new INIManager(PROBER_INI_Constants.OPUS_INI);

            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleStepX", "20000"), out gEMC[(int)EnumAxisConstants.X].lngSampleStep);
            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleOriginX", "190000"), out gEMC[(int)EnumAxisConstants.X].lngSampleOrigin);
            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "TotalLengthX", "380000"), out gEMC[(int)EnumAxisConstants.X].lngTotalLength);

            // Y
            try
            {
                strErrPath = Table_Constants.ERROR_DATA_PATH + "linear1.err";

                if (File.Exists(strErrPath) == false)
                {
                    return -1;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"LoadErrorMapConfig() Function error: " + err.Message);
                LoggerManager.Exception(err);


                throw;
            }

            lineCount = File.ReadLines(strErrPath).Count();

            gEMC[(int)EnumAxisConstants.Y].intSampleNum = lineCount;

            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleStepY", "20000"), out gEMC[(int)EnumAxisConstants.Y].lngSampleStep);
            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleOriginY", "-190000"), out gEMC[(int)EnumAxisConstants.Y].lngSampleOrigin);
            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "TotalLengthY", "740000"), out gEMC[(int)EnumAxisConstants.Y].lngTotalLength);

            // C
            int.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleNumC", "31"), out gEMC[(int)EnumAxisConstants.C].intSampleNum);
            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleStepC", "2000"), out gEMC[(int)EnumAxisConstants.C].lngSampleStep);
            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleOriginC", "-30000"), out gEMC[(int)EnumAxisConstants.C].lngSampleOrigin);
            long.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "TotalLengthC", "60000"), out gEMC[(int)EnumAxisConstants.C].lngTotalLength);

            return 1;
        }

        public int SetErrorTable(short iaxis, short err_type, int tbl_num, double step, double start, double max_str)
        {
            int i;

            int Result;
            try
            {

                string LoadErrorDataPath = Table_Constants.ERROR_DATA_PATH;

                Result = 1;

                // ERROR
                if (tbl_num > Table_Constants.MAX_ERR_TABLE) return (short)1;

                mINIManager = new INIManager(PROBER_INI_Constants.OPUS_INI);

                switch (iaxis)
                {
                    case (int)EnumAxisConstants.X:

                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleStepX", "10000"), out step);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleOriginX", "-190000"), out start);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "TotalLengthX", "380000"), out max_str);

                        switch (err_type)
                        {
                            case Table_Constants.ERROR_TYPE_LINEAR:
                                LoadErrorDataPath = LoadErrorDataPath + "Linear0.err";
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

                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleStepY", "10000"), out step);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleOriginY", "-190000"), out start);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "TotalLengthY", "720000"), out max_str);

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

                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleStepZ", "10000"), out step);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleOriginZ", "-22000"), out start);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "TotalLengthZ", "5000"), out max_str);

                        switch (err_type)
                        {
                            case Table_Constants.ERROR_TYPE_ANGULAR:
                                LoadErrorDataPath = LoadErrorDataPath + "Angular2.err";
                                break;
                        }

                        break;

                    case (int)EnumAxisConstants.C:

                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleStepC", "10000"), out step);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "SampleOriginC", "-30000"), out start);
                        double.TryParse(mINIManager.ReadValue("ERRMAPCONFIG", "TotalLengthC", "60000"), out max_str);

                        switch (err_type)
                        {
                            case Table_Constants.ERROR_TYPE_LINEAR:
                                LoadErrorDataPath = LoadErrorDataPath + "Linear3.err";
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
                        return -1;
                    }
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($"SetErrorTable() Function error: " + err.Message);
                    LoggerManager.Exception(err);


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

                                MAX_X_STROKE = max_str;
                                MAX_TBL_X_LINEAR = (short)tbl_num;
                                LINEAR_X_STEP = step;
                                LINEAR_X_START = start;

                                while ((line = file.ReadLine()) != null)
                                {
                                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    double.TryParse(parts[0], out TBL_X_LINEAR[i]);
                                    double.TryParse(parts[1], out POS_TBL_X_LINEAR[i]);

                                    i++;
                                }

                                file.Close();

                                break;

                            case Table_Constants.ERROR_TYPE_STRAIGHT:

                                MAX_X_STROKE = max_str;
                                MAX_TBL_X_STRAIGHT = (short)tbl_num;
                                STRAIGHT_X_STEP = step;
                                STRAIGHT_X_START = start;

                                while ((line = file.ReadLine()) != null)
                                {
                                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    double.TryParse(parts[0], out TBL_X_STRAIGHT[i]);
                                    double.TryParse(parts[1], out POS_TBL_X_STRAIGHT[i]);

                                    i++;
                                }

                                file.Close();

                                break;

                            case Table_Constants.ERROR_TYPE_ANGULAR:

                                MAX_X_STROKE = max_str;
                                MAX_TBL_X_ANGULAR = (short)tbl_num;
                                ANGULAR_X_STEP = step;
                                ANGULAR_X_START = start;

                                while ((line = file.ReadLine()) != null)
                                {
                                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    double.TryParse(parts[0], out TBL_X_ANGULAR[i]);
                                    double.TryParse(parts[1], out POS_TBL_X_ANGULAR[i]);

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

                                MAX_Y_STROKE = max_str;
                                MAX_TBL_Y_LINEAR = (short)tbl_num;
                                LINEAR_Y_STEP = step;
                                LINEAR_Y_START = start;

                                while ((line = file.ReadLine()) != null)
                                {
                                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    double.TryParse(parts[0], out TBL_Y_LINEAR[i]);
                                    double.TryParse(parts[1], out POS_TBL_Y_LINEAR[i]);

                                    i++;
                                }

                                file.Close();

                                break;

                            case Table_Constants.ERROR_TYPE_STRAIGHT:

                                MAX_Y_STROKE = max_str;
                                MAX_TBL_Y_STRAIGHT = (short)tbl_num;
                                STRAIGHT_Y_STEP = step;
                                STRAIGHT_Y_START = start;

                                while ((line = file.ReadLine()) != null)
                                {
                                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    double.TryParse(parts[0], out TBL_Y_STRAIGHT[i]);
                                    double.TryParse(parts[1], out POS_TBL_Y_STRAIGHT[i]);

                                    i++;
                                }

                                file.Close();

                                break;

                            case Table_Constants.ERROR_TYPE_ANGULAR:

                                MAX_Y_STROKE = max_str;
                                MAX_TBL_Y_ANGULAR = (short)tbl_num;
                                ANGULAR_Y_STEP = step;
                                ANGULAR_Y_START = start;

                                while ((line = file.ReadLine()) != null)
                                {
                                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    double.TryParse(parts[0], out TBL_Y_ANGULAR[i]);
                                    double.TryParse(parts[1], out POS_TBL_Y_ANGULAR[i]);

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

                                MAX_Z_STROKE = max_str;
                                MAX_TBL_Z_ANGULAR = (short)tbl_num;
                                ANGULAR_Z_STEP = step;
                                ANGULAR_Z_START = start;

                                while ((line = file.ReadLine()) != null)
                                {
                                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    //double.TryParse(parts[0], out TBL_Y_ZH[i]);
                                    //double.TryParse(parts[1], out POS_TBL_Y_ZH[i]);

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

                if ((iaxis == (int)EnumAxisConstants.X) && (err_type == Table_Constants.ERROR_TYPE_LINEAR))
                {
                    int LoadReturnValue = 0;

                    LoadReturnValue = LoadErrTableData();

                    if (LoadReturnValue == 1)
                    {
                        FlagExtraTableOn = 1;

                        LoggerManager.Debug($"SetErrorTable(): FlagExtraTablen = 1");
                    }
                    else
                    {
                        FlagExtraTableOn = 0;

                        LoggerManager.Debug($"SetErrorTable(): FlagExtraTablen = 0");
                    }

                    LoadReturnValue = LoadErrTableDataForPin();

                    if (LoadReturnValue == 1)
                    {
                        FlagExtraTableOnPin = 1;

                        //sprintf(buf, "[SetErrorTable] FlagExtraTableOnPin = 1\n");

                        //TRASE(buf);
                    }
                    else
                    {
                        FlagExtraTableOnPin = 0;

                        //sprintf(buf, "[seterrortable] flagextratableonpin = 0\n");

                        //trase(buf);
                    }

                }
                // Pass
                Result = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        public int LoadErrTableData()
        {
            //LoggerManager.Debug($string.Format("MainWindow(): Initilizing error. Err = {0}"), );

            string LoadPath;
            try
            {

                LoadPath = "C:\\PROBERPARAM\\ErrorData\\ExtraXtabledata.dat";

                // Open Error Table
                if (File.Exists(LoadPath) == false)
                {
                    return -1;
                }

                long fileSize = new FileInfo(LoadPath).Length;

                // Check File Size
                if (fileSize != (sizeof(double) * ((2 * Table_Constants.MAX_ERR_TABLE * Table_Constants.MAX_ERR_TABLE) + (2 * Table_Constants.MAX_ERR_TABLE))))
                {
                    //sprintf(buf, "[LoadErrTableData] File Size Error\n");
                    //TRASE(buf);

                    return 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
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

            return 1;
        }

        public int LoadErrTableDataForPin()
        {
            string LoadPath;
            try
            {

                LoadPath = "C:\\PROBERPARAM\\ErrorData\\ExtraXtabledataPin.dat";

                // Open Error Table
                if (File.Exists(LoadPath) == false)
                {
                    return -1;
                }

                long fileSize = new FileInfo(LoadPath).Length;

                // Check File Size
                if (fileSize != (sizeof(double) * ((2 * Table_Constants.MAX_ERR_TABLE * Table_Constants.MAX_ERR_TABLE) + (2 * Table_Constants.MAX_ERR_TABLE))))
                {
                    //sprintf(buf, "[LoadErrTableData] File Size Error\n");
                    //TRASE(buf);

                    return 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            try
            {
                FileInfo fi = new FileInfo(@LoadPath);

                BinaryReader br = new BinaryReader(fi.OpenRead());

                // (1) TBL_X_LINEAR_EXT_FOR_PIN

                int k = 0;
                int j = 0;

                long ReadCount = Table_Constants.MAX_ERR_TABLE * Table_Constants.MAX_ERR_TABLE;

                for (int i = 0; i < ReadCount; i++)
                {
                    TBL_X_LINEAR_EXT_FOR_PIN[k, j] = br.ReadDouble();

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

                // (3) POS_TBL_X_EXT_FOR_PIN
                for (int i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                {
                    POS_TBL_X_EXT_FOR_PIN[i] = br.ReadDouble();
                }

                // (4) POS_TBL_Y_EXT_FOR_PIN
                for (int i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                {
                    POS_TBL_Y_EXT_FOR_PIN[i] = br.ReadDouble();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"LoadErrTableDataForPin() Function error: " + err.Message);
            }
            return 1;
        }

        public int GetErrTableIndex(FlgCOMP_MODE comp_mode, double ref_p)
        {
            int i;
            try
            {

                i = 0;

                switch (comp_mode)
                {
                    case FlgCOMP_MODE.COMP_MODE_X_LIN:
                        {
                            for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                            {
                                if ((POS_TBL_X_LINEAR[i] <= ref_p) && (POS_TBL_X_LINEAR[i + 1] > ref_p))
                                {
                                    break;
                                }
                            }

                            break;
                        }

                    case FlgCOMP_MODE.COMP_MODE_X_ANG:
                        {
                            for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                            {
                                if ((POS_TBL_X_ANGULAR[i] <= ref_p) && (POS_TBL_X_ANGULAR[i + 1] > ref_p))
                                {
                                    break;
                                }
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_X_STR:
                        {
                            for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                            {
                                if ((POS_TBL_X_STRAIGHT[i] <= ref_p) && (POS_TBL_X_STRAIGHT[i + 1] > ref_p))
                                {
                                    break;
                                }
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_LIN:
                        {
                            for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                            {
                                if ((POS_TBL_Y_LINEAR[i] <= ref_p) && (POS_TBL_Y_LINEAR[i + 1] > ref_p))
                                {
                                    break;
                                }
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_ANG:
                        {
                            for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                            {
                                if ((POS_TBL_Y_ANGULAR[i] <= ref_p) && (POS_TBL_Y_ANGULAR[i + 1] > ref_p))
                                {
                                    break;
                                }
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_STR:
                        {
                            for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                            {
                                if ((POS_TBL_Y_STRAIGHT[i] <= ref_p) && (POS_TBL_Y_STRAIGHT[i + 1] > ref_p))
                                {
                                    break;
                                }
                            }

                            break;
                        }
                    //case FlgCOMP_MODE.COMP_MODE_Y_ZA:
                    //    {
                    //        for (i=0; i<Table_Constants.MAX_ERR_TABLE ; i++)
                    //        {
                    //            if((POS_TBL_Y_ZA[i] <= ref_p) && (POS_TBL_Y_ZA[i + 1] > ref_p))
                    //            {
                    //                break;
                    //            }
                    //        }

                    //        break;
                    //}
                    //case FlgCOMP_MODE.COMP_MODE_Y_ZH:
                    //    {
                    //        for (i=0; i<Table_Constants.MAX_ERR_TABLE ; i++)
                    //        {
                    //            if((POS_TBL_Y_ZH[i] <= ref_p) && (POS_TBL_Y_ZH[i + 1] > ref_p))
                    //            {
                    //                break;
                    //            }
                    //        }

                    //        break;
                    //    }
                    default:
                        {
                            break;
                        }
                }

                if (i >= Table_Constants.MAX_ERR_TABLE)
                {
                    i = Table_Constants.MAX_ERR_TABLE - 1;
                }
                else if (i < 0)
                {
                    i = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return i;
        }

        public int GetErrTableXIndexExt(double refX)
        {
            int i;
            try
            {

                i = 0;

                for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                {
                    if ((POS_TBL_X_EXT[i] <= refX) && (POS_TBL_X_EXT[i + 1] > refX))
                    {
                        break;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return i;
        }

        public int GetErrTableYIndexExt(double refY)
        {
            int i;
            try
            {

                i = 0;

                for (i = 0; i < Table_Constants.MAX_ERR_TABLE; i++)
                {
                    if ((POS_TBL_Y_EXT[i] <= refY) && (POS_TBL_Y_EXT[i + 1] > refY))
                    {
                        break;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return i;
        }

        double GetErrStep(FlgCOMP_MODE comp_mode, long index)
        {
            double step;
            try
            {

                step = 0;

                if (index < 0)
                {
                    step = POS_TBL_X_LINEAR[1] - POS_TBL_X_LINEAR[0];
                    return step;
                }
                else if (index > Table_Constants.MAX_ERR_TABLE)
                {
                    step = POS_TBL_X_LINEAR[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_LINEAR[Table_Constants.MAX_ERR_TABLE - 1];
                    return step;
                }

                switch (comp_mode)
                {
                    case FlgCOMP_MODE.COMP_MODE_X_LIN:
                        {
                            step = POS_TBL_X_LINEAR[index + 1] - POS_TBL_X_LINEAR[index];
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_X_ANG:
                        {
                            step = POS_TBL_X_ANGULAR[index + 1] - POS_TBL_X_ANGULAR[index];
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_X_STR:
                        {
                            step = POS_TBL_X_STRAIGHT[index + 1] - POS_TBL_X_STRAIGHT[index];
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_LIN:
                        {
                            step = POS_TBL_Y_LINEAR[index + 1] - POS_TBL_Y_LINEAR[index];
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_ANG:
                        {
                            step = POS_TBL_Y_ANGULAR[index + 1] - POS_TBL_Y_ANGULAR[index];
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_STR:
                        {
                            step = POS_TBL_Y_STRAIGHT[index + 1] - POS_TBL_Y_STRAIGHT[index];
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                if (step < 0)
                {
                    step = step * -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return step;
        }

        double GetErrStepExt(int Xindex, int Yindex)
        {
            double step;
            try
            {

                step = 0;

                if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
                {
                    if (Xindex < 0)
                    {
                        step = POS_TBL_X_EXT_FOR_PIN[1] - POS_TBL_X_EXT_FOR_PIN[0];
                        return step;
                    }
                    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
                    {
                        step = POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE - 1];
                        return step;
                    }
                    else
                    {
                        step = POS_TBL_X_EXT_FOR_PIN[Xindex + 1] - POS_TBL_X_EXT_FOR_PIN[Xindex];
                    }
                }
                else
                {
                    if (Xindex < 0)
                    {
                        step = POS_TBL_X_EXT[1] - POS_TBL_X_EXT[0];
                        return step;
                    }
                    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
                    {
                        step = POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE - 1];
                        return step;
                    }
                    else
                    {
                        step = POS_TBL_X_EXT[Xindex + 1] - POS_TBL_X_EXT[Xindex];
                    }

                }

                if (step < 0)
                {
                    step = step * -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return step;
        }

        public double GetLinearErrorX(double a_x_ref)
        {
            int tbl_index;
            double step;
            double error = 0, tmp;
            try
            {


                tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_X_LIN, a_x_ref);
                step = (int)GetErrStep(FlgCOMP_MODE.COMP_MODE_X_LIN, tbl_index);

                if (tbl_index > (MAX_TBL_X_LINEAR - 2) || tbl_index < 0) return 0.0;

                if (isInspectionArea == 0)
                {
                    tmp = a_x_ref - POS_TBL_X_LINEAR[tbl_index];

                    error = (TBL_X_LINEAR[tbl_index + 1] - TBL_X_LINEAR[tbl_index]) * (tmp / step)
                            + TBL_X_LINEAR[tbl_index];
                }
                else
                {
                    //tmp = a_x_ref - POS_TBL_X_LINEAR_PROB[tbl_index];

                    //error = (TBL_X_LINEAR_PROB[tbl_index + 1]-TBL_X_LINEAR_PROB[tbl_index])* (tmp/step)
                    //  + TBL_X_LINEAR_PROB[tbl_index];
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return error;
        }
        public double GetLinearErrorY(double a_y_ref)
        {
            double step;
            int tbl_index;
            double error, tmp;
            try
            {

                tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_Y_LIN, a_y_ref);
                step = (int)GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_LIN, tbl_index);

                if (tbl_index > (MAX_TBL_Y_LINEAR - 2) || tbl_index < 0) return 0.0;

                tmp = a_y_ref - POS_TBL_Y_LINEAR[tbl_index];

                error = (TBL_Y_LINEAR[tbl_index + 1] - TBL_Y_LINEAR[tbl_index]) * (tmp / step)
                        + TBL_Y_LINEAR[tbl_index];

                //if (isInspectionArea == 0)
                //{
                //    error = (TBL_Y_LINEAR[tbl_index + 1] - TBL_Y_LINEAR[tbl_index]) * (tmp / step)
                //            + TBL_Y_LINEAR[tbl_index];
                //}
                //else
                //{
                //    error = (TBL_Y_LINEAR[tbl_index + 1] - TBL_Y_LINEAR[tbl_index]) * (tmp / step)
                //            + TBL_Y_LINEAR[tbl_index];
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return error;
        }

        public double GetLinearErrorXExt(double a_x_ref, double a_y_ref)
        {
            int tbl_indexY;
            int tbl_indexX;

            double step;
            double error, tmp;
            try
            {

                double tmpPoint1;
                double tmppoint2;

                tbl_indexY = GetErrTableYIndexExt(a_y_ref);
                tbl_indexX = GetErrTableXIndexExt(a_x_ref);

                if (tbl_indexX > (Table_Constants.MAX_ERR_TABLE - 2) || tbl_indexX < 0) return 0.0;
                if (tbl_indexY > (Table_Constants.MAX_ERR_TABLE - 2) || tbl_indexY < 0) return 0.0;

                step = GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_LIN, tbl_indexY);

                if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
                {
                    tmp = a_y_ref - POS_TBL_Y_EXT_FOR_PIN[tbl_indexY];

                    tmpPoint1 = (TBL_X_LINEAR_EXT_FOR_PIN[tbl_indexY + 1, tbl_indexX] - TBL_X_LINEAR_EXT_FOR_PIN[tbl_indexY, tbl_indexX]) * (tmp / step)
                        + TBL_X_LINEAR_EXT_FOR_PIN[tbl_indexY, tbl_indexX];

                    tmppoint2 = (TBL_X_LINEAR_EXT_FOR_PIN[tbl_indexY + 1, tbl_indexX + 1] - TBL_X_LINEAR_EXT_FOR_PIN[tbl_indexY, tbl_indexX + 1]) * (tmp / step)
                        + TBL_X_LINEAR_EXT_FOR_PIN[tbl_indexY, tbl_indexX + 1];

                }
                else
                {

                    tmp = a_y_ref - POS_TBL_Y_EXT[tbl_indexY];

                    tmpPoint1 = (TBL_X_LINEAR_EXT[tbl_indexY + 1, tbl_indexX] - TBL_X_LINEAR_EXT[tbl_indexY, tbl_indexX]) * (tmp / step)
                        + TBL_X_LINEAR_EXT[tbl_indexY, tbl_indexX];

                    tmppoint2 = (TBL_X_LINEAR_EXT[tbl_indexY + 1, tbl_indexX + 1] - TBL_X_LINEAR_EXT[tbl_indexY, tbl_indexX + 1]) * (tmp / step)
                        + TBL_X_LINEAR_EXT[tbl_indexY, tbl_indexX + 1];
                }

                step = GetErrStepExt(tbl_indexX, tbl_indexY);

                if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
                {
                    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
                }
                else
                {
                    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
                }

                error = (tmppoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return error;
        }

        public double GetStraightnessErrorX(double a_y_ref)
        {
            double step;
            int tbl_index;
            double error, tmp;
            try
            {

                tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_Y_STR, a_y_ref);
                step = (int)GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_STR, tbl_index);

                if (tbl_index > (MAX_TBL_Y_STRAIGHT - 2) || tbl_index < 0) return 0.0;

                tmp = a_y_ref - POS_TBL_Y_STRAIGHT[tbl_index];
                error = (TBL_Y_STRAIGHT[tbl_index + 1] - TBL_Y_STRAIGHT[tbl_index]) * (tmp / step) + TBL_Y_STRAIGHT[tbl_index];

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return error;
        }
        public double GetStraightnessErrorYExt(double a_x_ref, double a_y_ref)
        {
            int tbl_indexY;
            int tbl_indexX;
            double step;
            double error, tmp;
            try
            {

                double tmpPoint1;
                double tmppoint2;

                tbl_indexY = GetErrTableYIndexExt(a_y_ref);
                tbl_indexX = GetErrTableXIndexExt(a_x_ref);

                if (tbl_indexX > (Table_Constants.MAX_ERR_TABLE - 2) || tbl_indexX < 0) return 0.0;

                if (tbl_indexY > (Table_Constants.MAX_ERR_TABLE - 2) || tbl_indexY < 0) return 0.0;

                step = GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_LIN, tbl_indexY);

                if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
                {
                    tmp = a_y_ref - POS_TBL_Y_EXT_FOR_PIN[tbl_indexY];

                    tmpPoint1 = (TBL_X_STRAIGHT_EXT_FOR_PIN[tbl_indexY + 1, tbl_indexX] - TBL_X_STRAIGHT_EXT_FOR_PIN[tbl_indexY, tbl_indexX]) * (tmp / step)
                        + TBL_X_STRAIGHT_EXT_FOR_PIN[tbl_indexY, tbl_indexX];

                    tmppoint2 = (TBL_X_STRAIGHT_EXT_FOR_PIN[tbl_indexY + 1, tbl_indexX + 1] - TBL_X_STRAIGHT_EXT_FOR_PIN[tbl_indexY, tbl_indexX + 1]) * (tmp / step)
                        + TBL_X_STRAIGHT_EXT_FOR_PIN[tbl_indexY, tbl_indexX + 1];
                }
                else
                {
                    tmp = a_y_ref - POS_TBL_Y_EXT[tbl_indexY];

                    tmpPoint1 = (TBL_X_STRAIGHT_EXT[tbl_indexY + 1, tbl_indexX] - TBL_X_STRAIGHT_EXT[tbl_indexY, tbl_indexX]) * (tmp / step)
                        + TBL_X_STRAIGHT_EXT[tbl_indexY, tbl_indexX];

                    tmppoint2 = (TBL_X_STRAIGHT_EXT[tbl_indexY + 1, tbl_indexX + 1] - TBL_X_STRAIGHT_EXT[tbl_indexY, tbl_indexX + 1]) * (tmp / step)
                        + TBL_X_STRAIGHT_EXT[tbl_indexY, tbl_indexX + 1];
                }

                step = GetErrStepExt(tbl_indexX, tbl_indexY);

                if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
                {
                    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
                }
                else
                {
                    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
                }

                error = (tmppoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return error;
        }

        private double DegreeToRadian(double angle)
        {
            try
            {
                return Math.PI * angle / 180.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private double RadianToDegree(double angle)
        {
            try
            {
                return angle * (180.0 / Math.PI);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        double GetSqaurenessErrorY(double x_ref, double y_ref)
        {
            double comp_y;
            double ang_err = GetAngularErrorY(y_ref);

            try
            {
                //comp_y = (x_ref) * sin(rad(((ang_err + m_sqr_angle) / 10000.0)));

                comp_y = (x_ref) * Math.Sin(DegreeToRadian(((ang_err + m_sqr_angle) / 10000.0)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return (comp_y);
        }

        public double GetAngularErrorX(double a_x_ref)
        {
            double step;
            int tbl_index;
            double error = 0.0, tmp;
            try
            {

                tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_X_ANG, a_x_ref);
                step = (int)GetErrStep(FlgCOMP_MODE.COMP_MODE_X_ANG, tbl_index);


                if (tbl_index > (MAX_TBL_X_ANGULAR - 2) || tbl_index < 0) return 0.0;

                if (isInspectionArea == 0)
                {
                    tmp = a_x_ref - POS_TBL_X_ANGULAR[tbl_index];

                    error = (TBL_X_ANGULAR[tbl_index + 1] - TBL_X_ANGULAR[tbl_index]) * (tmp / step)
                            + TBL_X_ANGULAR[tbl_index];
                }
                else
                {
                    //tmp = a_x_ref - POS_TBL_X_ANGULAR_PROB[tbl_index];
                    //error = (TBL_X_ANGULAR_PROB[tbl_index + 1] - TBL_X_ANGULAR_PROB[tbl_index]) * (tmp / step)
                    //        + TBL_X_ANGULAR_PROB[tbl_index];

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return error;
        }

        public double GetAngularErrorY(double a_y_ref)
        {
            double step;
            int tbl_index;
            double error = 0.0, tmp;
            try
            {

                tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_Y_ANG, a_y_ref);
                step = (int)GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_ANG, tbl_index);

                if (tbl_index > (MAX_TBL_Y_ANGULAR - 2) || tbl_index < 0) return 0.0;

                tmp = a_y_ref - POS_TBL_Y_ANGULAR[tbl_index];

                error = (TBL_Y_ANGULAR[tbl_index + 1] - TBL_Y_ANGULAR[tbl_index]) * (tmp / step)
                        + TBL_Y_ANGULAR[tbl_index];

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return error;
        }

        public double GetAngularErrorC(double c_ref)
        {
            //double a_ref = c_ref - LINEAR_C_START; 
            //double step = LINEAR_C_STEP;
            //int tbl_index = (int)(a_ref / step);
            //double error = 0.0, tmp;
            try
            {

                //if (tbl_index > (MAX_TBL_C_LINEAR - 2) || tbl_index < 0) return 0.0;

                //tmp = a_ref - (step * tbl_index);

                //error = (theta_err_map[tbl_index + 1] - theta_err_map[tbl_index]) * (tmp / step)
                //        + theta_err_map[tbl_index];

                //return error;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0.0;
        }


        public double GetCompErrorX(double x_ref, double y_ref)
        {
            double comp_x = 0.0;
            try
            {

                if (y_ref > PROB_AREA)  // Modified by Yang 121204 (Prbing area => Inspection area)
                {
                    isInspectionArea = 1;
                }
                else
                {
                    isInspectionArea = 0;
                }

                if (FlagExtraTableOn == 1)
                {
                    comp_x = GetLinearErrorXExt(x_ref, y_ref) +
                             GetStraightnessErrorX(y_ref);
                    //+ GetSqaurenessErrorX();
                }
                else
                {

                    //comp_x = GetLinearErrorX(x_ref) +
                    //         GetStraightnessErrorX(y_ref);
                    //+ GetSqaurenessErrorX();

                    // Test
                    comp_x = GetLinearErrorX(x_ref);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return comp_x;
        }
        public double GetCompErrorY(double x_ref, double y_ref)
        {
            double comp_y = 0.0;
            try
            {

                if (y_ref > PROB_AREA)
                {
                    isInspectionArea = 1;
                }
                else
                {
                    isInspectionArea = 0;
                }

                if (FlagExtraTableOn == 1)
                {
                    comp_y = GetLinearErrorY(y_ref) + GetStraightnessErrorYExt(x_ref, y_ref) + GetSqaurenessErrorY(x_ref, y_ref);
                }
                else
                {
                    //comp_y = GetLinearErrorY(y_ref) +
                    //         GetStraightnessErrorY(x_ref) + GetSqaurenessErrorY(x_ref, y_ref);
                }
                //rad((GetAngularErrorX(x_ref)+ GetAngularErrorY(y_ref))/10000)*(x_value_pin - x_ref) ;  /* added by Shoun Mar. 2008 */
                //isInspectionArea = 0;		Removed by Yang 130522

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return comp_y;
        }

        public double GetCompErrorC(double x_ref, double y_ref, double z_ref, double c_ref)
        {   // 1/10000 deg
            double comp_c;
            try
            {

                if (y_ref > PROB_AREA)
                {
                    isInspectionArea = 1;
                }
                else
                {
                    isInspectionArea = 0;
                }

                //comp_c = GetAngularErrorX(x_ref) +
                //         GetAngularErrorY(y_ref) +
                //         GetAngularErrorZ(z_ref) +
                //         GetAngularErrorC(c_ref);

                comp_c = GetAngularErrorX(x_ref) +
                            GetAngularErrorY(y_ref) +
                            GetAngularErrorC(c_ref);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return comp_c;
        }

    }
}
