using SECS_Host.Help;
using SECS_Host.Model;
using SECS_Host.SecsMsgClass;
using SECS_Host.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using XComPro.Library;

namespace SECS_Host.View_Model
{
    class VM_DialogRequestEquipSV : ViewModelBase
    {
        public delegate void CloseDelegate(object sender, System.EventArgs e);

        private XComProW m_XComPro;
        public CloseDelegate CloseWindow;
        private SecsMsgBase SecsMsg;

        private ObservableCollection<EquipStatus> _EquipCollection;
        public ObservableCollection<EquipStatus> EquipCollection
        {
            get
            {
                return _EquipCollection;
            }
            set
            {
                _EquipCollection = value;
                NotifyPropertyChange("EquipCollection");
            }
        }

        private String  _TRID;
        public String TRID
        {
            get { return _TRID; }
            set
            {
                if(_TRID != value)
                {
                    _TRID = value;
                    NotifyPropertyChange("TRID");
                }
            }
        }
        private String  _DSPER;
        public String DSPER
        {
            get { return _DSPER; }
            set
            {
                if (_DSPER != value)
                {
                    _DSPER = value;
                    NotifyPropertyChange("DSPER");
                }
            }
        }
        private String  _TOTSMP;
        public String TOTSMP
        {
            get { return _TOTSMP; }
            set
            {
                if (_TOTSMP != value)
                {
                    _TOTSMP = value;
                    NotifyPropertyChange("TOTSMP");
                }
            }
        }
        private String  _REPGSZ;
        public String REPGSZ
        {
            get { return _REPGSZ; }
            set
            {
                if (_REPGSZ != value)
                {
                    _REPGSZ = value;
                    NotifyPropertyChange("REPGSZ");
                }
            }
        }

        public AddLogDelegate AddLog;

        public void SetSecsMsg(SecsMsgBase _secsMsg)
        {
            SecsMsg = _secsMsg;
            SecsMsg.m_XComPro = m_XComPro;
            SecsMsg.AddLog = AddLog;
        }

        #region ==> SendRequestStatusCommand
        private ICommand _SendRequestStatusCommand;
        public ICommand SendRequestStatusCommand
        {
            get
            {
                if(_SendRequestStatusCommand == null)
                    _SendRequestStatusCommand = new RelayCommand(SendRequestStatus);
                return _SendRequestStatusCommand;
            }
        }
        private void SendRequestStatus(object obj)
        {
            if (0 < EquipCollection.Count)
            {
                SECSGenericData<List<SECSData>> sendDataList
                    = new SECSGenericData<List<SECSData>>()
                    {
                        Value = new List<SECSData>(),
                        MsgType = SECS_DataTypeCategory.LIST
                    };

                foreach (var v in (EquipCollection.Where(n => n.IsChecked == true)))
                {
                    sendDataList.Value.Add(new SECSGenericData<uint>() { Value = v.SVID, MsgType = SECS_DataTypeCategory.UINT4 });
                }

                SetSecsMsg(new SecsMsg_S1F3());
                SecsMsg.SetSendData(sendDataList);
                SecsMsg.Excute();


                //All Print
                //nReturn = m_XComPro.SetListItem(lSMsgId, EquipCollection.Count);
                //foreach (var v in EquipCollection)
                //{
                //    nReturn = m_XComPro.SetU4Item(lSMsgId, v.SVID);
                //}
            }

            //Dispose();
            CloseWindow(this, null);
        }
        #endregion

        #region ==> Start SV Trace
        private ICommand _StartSVTraceCommand;
        public ICommand StartSVTraceCommand
        {
            get
            {
                if (_StartSVTraceCommand == null)
                    _StartSVTraceCommand = new RelayCommand(StartSVTrace);
                return _StartSVTraceCommand;
            }
        }
        private void StartSVTrace(object obj)
        {
            if(0 < EquipCollection.Count)
            {
                //Check Validation. TRID, DSPER, TOTSMP, REPGSZ
                //1. have value. if don't have value then input "0" or "000000".
                TRID = TRID == null ? "0" : TRID == "" ? "0" : TRID;
                DSPER = DSPER == null ? "000000" : DSPER == "" ? "000000" : DSPER;
                TOTSMP = TOTSMP == null ? "0" : TOTSMP == "" ? "0" : TOTSMP;
                REPGSZ = REPGSZ == null ? "0" : REPGSZ == "" ? "0" : REPGSZ;

                //2. If the length of the DSPER value is less than 6, fill it with zeros.

                if (DSPER.Length < 6)
                {
                    DSPER = string.Format("{0:D6}", DSPER);
                }
                else if (6 < DSPER.Length && DSPER.Length < 8)
                {
                    DSPER = string.Format("{0:D8}", DSPER);
                }

                SECSGenericData<List<SECSData>> sendDataList
                    = new SECSGenericData<List<SECSData>>()
                    {
                        Value = new List<SECSData>(),
                        MsgType = SECS_DataTypeCategory.LIST
                    };

                sendDataList.Value.Add(SECSData.MakeU1FromStr(TRID));
                sendDataList.Value.Add(SECSData.MakeAsciiFromStr(DSPER));
                sendDataList.Value.Add(SECSData.MakeU4FromStr(TOTSMP));
                sendDataList.Value.Add(SECSData.MakeU4FromStr(REPGSZ));

                SECSGenericData<List<SECSData>> elementDataList
                    = new SECSGenericData<List<SECSData>>()
                    {
                        Value = new List<SECSData>(),
                        MsgType = SECS_DataTypeCategory.LIST
                    };

                if (0 < EquipCollection.Count)
                {
                    foreach (var v in (EquipCollection.Where(n => n.IsChecked == true)))
                    {
                        elementDataList.Value.Add(SECSData.MakeU4FromStr(v.SVID.ToString()));
                    }

                    sendDataList.Value.Add(elementDataList);
                }

                SetSecsMsg(new SecsMsg_S2F23());
                SecsMsg.SetSendData(sendDataList);
                SecsMsg.Excute();
            }
            else
            {
                AddLog("Don't Check SVCollection");
            }
            CloseWindow(this, null);
        }
        #endregion

        #region ==> DialogCancelCommand
        private ICommand _DialogCancelCommand;
        public ICommand DialogCancelCommand
        {
            get
            {
                if (_DialogCancelCommand == null)
                    _DialogCancelCommand = new RelayCommand(DialogCancel);
                return _DialogCancelCommand;
            }
        }
        private void DialogCancel(object obj)
        {
            //Dispose();
            CloseWindow(this, null);
        }
        #endregion


        public void Dispose()
        {
            if(EquipCollection != null)
            {
                EquipCollection.Clear();
                EquipCollection = null;
            }
        }

        public VM_DialogRequestEquipSV(XComProW _XComPro)
        {
            m_XComPro = _XComPro;
            _EquipCollection = new ObservableCollection<EquipStatus>();
            TRID = "5";
            DSPER = "000002";
            TOTSMP = "1";
            REPGSZ = "1";
            //TRID = "0";
            //DSPER = "000000";
            //TOTSMP = "0";
            //REPGSZ = "0";
        }

        public void SetEquipCollection(SECSData SecsStruct)
        {
            if (SecsStruct != null)
            {
                foreach (var v in (SecsStruct as SECSGenericData<List<SECSData>>)?.Value)
                {
                    EquipStatus tmpEquip = new EquipStatus();
                    SECSGenericData<List<SECSData>> tempList = (SECSGenericData<List<SECSData>>)v;
                    uint tmpIDString = 0;

                    tmpEquip.IsChecked = false;

                    if (3 <= tempList.Value.Count)
                    {
                        tmpEquip.SVNAME = ((SECSGenericData<string>)tempList.Value[(int)EquipStatus.EquipDataValueName.SVNAME]).Value;

                        if (uint.TryParse(((SECSGenericData<uint>)tempList.Value[(int)EquipStatus.EquipDataValueName.SVID]).Value.ToString(), out tmpIDString))
                        {
                            tmpEquip.SVID = tmpIDString;
                            EquipCollection.Add(tmpEquip);
                        }
                    }
                }
            }
        }
    }

    public class EquipStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChange(string propString)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propString));
            }
        }
        public enum EquipDataValueName
        { NONE = -1, SVID = 0, SVNAME = 1, ISCHECKED = 10 }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyPropertyChange("IsChecked");
            }
        }

        private String svNAME;
        public String SVNAME
        {
            get { return svNAME; }
            set
            {
                svNAME = value;
                NotifyPropertyChange("svNAME");
            }
        }

        private uint svID;
        public uint SVID
        {
            get { return svID; }
            set
            {
                svID = value;
                NotifyPropertyChange("SVID");
            }
        }
    }

}
