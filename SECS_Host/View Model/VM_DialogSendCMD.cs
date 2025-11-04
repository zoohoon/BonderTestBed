using SECS_Host.Help;
using SECS_Host.Model;
using SECS_Host.SecsMsgClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using XComPro.Library;

namespace SECS_Host.View_Model
{
    public class VM_DialogSendCMD : ViewModelBase
    {
        public delegate void CloseDelegate(object sender, System.EventArgs e);

        private XComProW m_XComPro;
        public CloseDelegate CloseWindow;
        public AddLogDelegate AddLog;
        private SecsMsgBase SecsMsg;

        private ObservableCollection<RCMD_Data> _RCMD_Collection;
        public ObservableCollection<RCMD_Data> RCMD_Collection
        {
            get { return _RCMD_Collection; }
            set
            {
                _RCMD_Collection = value;
                NotifyPropertyChange(nameof(RCMD_Collection));
            }
        }

        private String _DiscriptionStr;
        public String DescriptionStr
        {
            get { return _DiscriptionStr; }
            set
            {
                if(_DiscriptionStr != value)
                {
                    _DiscriptionStr = value;
                    NotifyPropertyChange(nameof(DescriptionStr));
                }
            }
        }

        private String _DataID = "";
        public String DataID
        {
            get { return _DataID; }
            set
            {
                if (_DataID != value)
                {
                    _DataID = value;
                    NotifyPropertyChange(nameof(DataID));
                }
            }
        }

        private String _OBJSPEC = "";
        public String OBJSPEC
        {
            get { return _OBJSPEC; }
            set
            {
                if (_OBJSPEC != value)
                {
                    _OBJSPEC = value;
                    NotifyPropertyChange(nameof(OBJSPEC));
                }
            }
        }

        private RCMD_Data _SelectedData;
        public RCMD_Data SelectedData
        {
            get { return _SelectedData; }
            set
            {
                if (_SelectedData != value)
                {
                    _SelectedData = value;
                    NotifyPropertyChange(nameof(SelectedData));

                    DescriptionStr = value.Description;
                }
            }
        }

        public VM_DialogSendCMD(XComProW _m_XComPro)
        {
            this.m_XComPro = _m_XComPro;
            RCMD_Collection = new ObservableCollection<RCMD_Data>();
            DataID = "1";
        }

        public void SetSecsMsg(SecsMsgBase _secsMsg)
        {
            SecsMsg = _secsMsg;
            SecsMsg.m_XComPro = m_XComPro;
            SecsMsg.AddLog = AddLog;
        }

        private ICommand _GetDataFromXLSCommand;
        public ICommand GetDataFromXLSCommand
        {
            get
            {
                if (_GetDataFromXLSCommand == null)
                    _GetDataFromXLSCommand = new RelayCommand(GetDataFromXLS);
                return _GetDataFromXLSCommand;
            }
        }

        public void GetDataFromXLS(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel (*.xls)|*.xls";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                char delimiter = ';';
                List<string> tmp = Help_ExcelParsing.PasingFromExel_xls(openFileDialog.FileName, "Sheet1", delimiter);

                for (int i = 1; i < tmp.Count; i++)
                {
                    string[] split = tmp[i].Split(delimiter);
                    RCMD_Data element = new RCMD_Data()
                    {
                        RCMDName = split[0],
                        Description = split[1],
                        CPNAMEs = split[2],
                        CPValTypes = split[3],
                    };
                    element.SetCP();

                    RCMD_Collection.Add(element);
                }
            }
        }

        private ICommand _SendCMDCommand;
        public ICommand SendCMDCommand
        {
            get
            {
                if (_SendCMDCommand == null)
                    _SendCMDCommand = new RelayCommand(SendCMD);
                return _SendCMDCommand;
            }
        }

        public void SendCMD(object obj)
        {
            SetSecsMsg(new SecsMsg_S2F21());
            SecsMsg.SetSendData(new SECSGenericData<string>() { Value = SelectedData.CPNAMEs, MsgType = SECS_DataTypeCategory.ASCII });
            SecsMsg.Excute();
        }

        private ICommand _SendHostCMDCommand;
        public ICommand SendHostCMDCommand
        {
            get
            {
                if (_SendHostCMDCommand == null)
                    _SendHostCMDCommand = new RelayCommand(SendHostCMD);
                return _SendHostCMDCommand;
            }
        }

        /// <summary>
        /// L,2
        ///     1. <RCMD>
        ///     2. L,n # of parameters
        ///         1. L,2
        ///             1. <CPNAME1> parameter 1 name
        ///             2. <CPVAL1> parameter 1 value
        ///     .
        ///     .
        ///         n.L,2
        ///             1. <CPNAMEn> parameter n name
        ///             2. <CPVALn> parameter n value
        /// </summary>
        /// <param name="obj"></param>
        public void SendHostCMD(object obj)
        {
            SECSGenericData<List<SECSData>> tempSendData
             = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };

            tempSendData.Value.Add(new SECSGenericData<string>() { Value = SelectedData.RCMDName, MsgType = SECS_DataTypeCategory.ASCII });

            SECSGenericData<List<SECSData>> RCMDList
             = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };

            foreach(var v in SelectedData.CP)
            {
                SECSGenericData<List<SECSData>> elementList
                = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };
                elementList.Value.Add(new SECSGenericData<string>() { Value = v.CPNAME, MsgType = SECS_DataTypeCategory.ASCII });
                elementList.Value.Add(SECSData.MakeSECSDataFromStr(v.CPVal, v.GetCPValType()));

                RCMDList.Value.Add(elementList);
            }
            tempSendData.Value.Add(RCMDList);

            SetSecsMsg(new SecsMsg_S2F41());
            SecsMsg.SetSendData(tempSendData);
            SecsMsg.Excute();
        }

        private ICommand _EnhancedRemoteCMOCommand;
        public ICommand EnhancedRemoteCMOCommand
        {
            get
            {
                if (_EnhancedRemoteCMOCommand == null)
                    _EnhancedRemoteCMOCommand = new RelayCommand(EnhancedRemoteCMO);
                return _EnhancedRemoteCMOCommand;
            }
        }

        public void EnhancedRemoteCMO(object obj)
        {
            SECSGenericData<List<SECSData>> tempSendData
             = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };

            tempSendData.Value.Add(new SECSGenericData<uint>() { Value = uint.Parse(DataID), MsgType = SECS_DataTypeCategory.UINT4 });
            tempSendData.Value.Add(new SECSGenericData<string>() { Value = OBJSPEC, MsgType = SECS_DataTypeCategory.ASCII });
            tempSendData.Value.Add(new SECSGenericData<string>() { Value = SelectedData.RCMDName, MsgType = SECS_DataTypeCategory.ASCII });

            SECSGenericData<List<SECSData>> RCMDList
             = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };

            foreach (var v in SelectedData.CP)
            {
                SECSGenericData<List<SECSData>> elementList
                = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };
                elementList.Value.Add(new SECSGenericData<string>() { Value = v.CPNAME, MsgType = SECS_DataTypeCategory.ASCII });
                elementList.Value.Add(SECSData.MakeSECSDataFromStr(v.CPVal, v.GetCPValType()));

                RCMDList.Value.Add(elementList);
            }
            tempSendData.Value.Add(RCMDList);

            SetSecsMsg(new SecsMsg_S2F49());
            SecsMsg.SetSendData(tempSendData);
            SecsMsg.Excute();
        }
    }

    public class RCMD_Data : ViewModelBase
    {
        public enum RCMD_ENUM
        {
            RCMD_NAME = 0,
            DESCRIPTION = 1,
            CPNAME = 2,
            CPValType = 3
        }

        public RCMD_Data()
        {
        }

        private string _RCMDName;
        public string RCMDName
        {
            get { return _RCMDName; }
            set
            {
                if(_RCMDName != value)
                {
                    _RCMDName = value;
                    NotifyPropertyChange(nameof(RCMDName));
                }
            }
        }
        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (_Description != value)
                {
                    _Description = value;
                    NotifyPropertyChange(nameof(Description));
                }
            }
        }
        private string _CPNAMEs;
        public string CPNAMEs
        {
            get { return _CPNAMEs; }
            set
            {
                if (_CPNAMEs != value)
                {
                    _CPNAMEs = value;
                    NotifyPropertyChange(nameof(CPNAMEs));
                }
            }
        }
        private string _CPValTypes;
        public string CPValTypes
        {
            get { return _CPValTypes; }
            set
            {
                if (_CPValTypes != value)
                {
                    _CPValTypes = value;
                    NotifyPropertyChange(nameof(CPValTypes));
                }
            }
        }

        public class CPClass : ViewModelBase
        {
            private string _CPNAME = "";
            public string CPNAME
            {
                get { return _CPNAME; }
                set
                {
                    if (_CPNAME != value)
                    {
                        _CPNAME = value;
                        NotifyPropertyChange(nameof(CPNAME));
                    }
                }
            }
            private SECS_DataTypeCategory _CPValType = SECS_DataTypeCategory.NONE;
            public string CPValType
            {
                get { return _CPValType.ToString(); }
                set
                {
                    _CPValType = EnumConverter.ParseDataTypeCategory(value);
                    NotifyPropertyChange(nameof(CPValType));
                }
            }
            public SECS_DataTypeCategory GetCPValType()
            {
                return _CPValType;
            }

            private string _CPVal = "";
            public string CPVal
            {
                get { return _CPVal; }
                set
                {
                    if (_CPVal != value)
                    {
                        if(_CPValType == SECS_DataTypeCategory.ASCII ||
                            _CPValType == SECS_DataTypeCategory.JIS8)
                            _CPVal = value;
                        else
                        {
                            if (value.All(char.IsDigit))
                                _CPVal = value;
                            else
                                CPVal = "";
                        }
                        NotifyPropertyChange(nameof(CPVal));
                    }
                }
            }
        }

        private ObservableCollection<CPClass> _CP = new ObservableCollection<CPClass>();
        public ObservableCollection<CPClass> CP
        {
            get { return _CP; }
            set
            {
                _CP = value;
                NotifyPropertyChange(nameof(_CP));
            }
        }

        public void SetCP()
        {
            if (CPNAMEs != null && CPValTypes != null)
            {
                String[] splitCPNames = CPNAMEs.Split(',');
                String[] splitCPTypes = CPValTypes.Split(',');

                if (splitCPNames != null && 0 < splitCPNames.Length &&
                    (splitCPNames.Length == splitCPTypes.Length))
                {
                    if(splitCPNames[0] != "")
                    {
                        for (int i = 0; i < splitCPNames.Length; i++)
                        {
                            CP.Add(new CPClass { CPNAME = splitCPNames[i], CPValType = splitCPTypes[i] });
                        }
                    }
                }
                else
                {

                }
            }
        }
    }
}