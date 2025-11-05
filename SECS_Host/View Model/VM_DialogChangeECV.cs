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
    class VM_DialogChangeECV : ViewModelBase
    {
        public delegate void CloseDelegate(object sender, System.EventArgs e);

        private XComProW m_XComPro;
        public CloseDelegate CloseWindow;
        public AddLogDelegate AddLog;
        private SecsMsgBase SecsMsg;

        private ObservableCollection<ECVStatus> _EquipCollection;
        public ObservableCollection<ECVStatus> EquipCollection
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

        public VM_DialogChangeECV(XComProW _xComPro)
        {
            m_XComPro = _xComPro;
            _EquipCollection = new ObservableCollection<ECVStatus>();
        }

        public void SetSecsMsg(SecsMsgBase _secsMsg)
        {
            SecsMsg = _secsMsg;
            SecsMsg.m_XComPro = m_XComPro;
            SecsMsg.AddLog = AddLog;
        }

        public void SetECDCollection(SECSData SecsStruct)
        {
            EquipCollection.Clear();

            if (SecsStruct != null)
            {
                foreach (var v in (SecsStruct as SECSGenericData<List<SECSData>>)?.Value)
                {
                    SECSGenericData<List<SECSData>> tempList = (SECSGenericData<List<SECSData>>)v;
                    ECVStatus tmpEquip = new ECVStatus();
                    tmpEquip.IsChecked = false;

                    tmpEquip.SetECD(tempList);

                    EquipCollection.Add(tmpEquip);
                }
            }

            (SecsStruct as SECSGenericData<List<SECSData>>)?.Value.Clear();
            //SecsStruct.Count = 0;
            SecsStruct = null;
        }

        #region ==> Send New EquipConstant Command
        private ICommand _SendNewEquipConstantCommand;
        public ICommand SendNewEquipConstantCommand
        {
            get
            {
                if (_SendNewEquipConstantCommand == null)
                    _SendNewEquipConstantCommand = new RelayCommand(SendNewEquipConstant);
                return _SendNewEquipConstantCommand;
            }
        }
        private void SendNewEquipConstant(object obj)
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
                    SECSGenericData<List<SECSData>> elementDataList
                        = new SECSGenericData<List<SECSData>>()
                        {
                            Value = new List<SECSData>(),
                            MsgType = SECS_DataTypeCategory.LIST
                        };

                    elementDataList.Value.Add(SECSData.MakeSECSDataFromStr(v.ECID, SECS_DataTypeCategory.UINT4));
                    elementDataList.Value.Add(v.GetECData());

                    sendDataList.Value.Add(elementDataList);
                }

                SetSecsMsg(new SecsMsg_S2F15());
                SecsMsg.SetSendData(sendDataList);
                SecsMsg.Excute();
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
            CloseWindow(this, null);
        }
        #endregion

        public void Dispose()
        {
            if (EquipCollection != null)
            {
                foreach (var v in EquipCollection)
                {
                    v.Dispose();
                }
                EquipCollection.Clear();
                EquipCollection = null;
            }
        }
    }

    public class ECVStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChange(string propString)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propString));
            }
        }
        public enum ECDataValueName
        {
            NONE = -1, ECID = 0, ECNAME = 1,
            ECMIN = 2, ECMAX = 3, ECDEF = 4,
            UNITS = 5, ECV = 6, ISCHECKED = 10
        }

        public void SetECD(SECSGenericData<List<SECSData>> tempList)
        {
            ecID = tempList.Value[(int)ECDataValueName.ECID];
            ecName = tempList.Value[(int)ECDataValueName.ECNAME];
            ecMin = tempList.Value[(int)ECDataValueName.ECMIN];
            ecMax = tempList.Value[(int)ECDataValueName.ECMAX];
            ecDef = tempList.Value[(int)ECDataValueName.ECDEF];
            units = tempList.Value[(int)ECDataValueName.UNITS];
            _ECV = tempList.Value[(int)ECDataValueName.ECV];
        }

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

        private SECSData ecName;
        public String ECName
        {
            get { return ecName.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    ecName.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange("ECName");
                }
            }
        }

        private SECSData ecID;
        public String ECID
        {
            get
            { return ecID.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    ecID.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange("ECID");
                }
            }
        }

        private SECSData ecMin;
        public String ECMin
        {
            get { return ecMin.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    ecMin.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange("ECMin");
                }
            }
        }

        private SECSData ecMax;
        public String ECMax
        {
            get { return ecMax.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    ecMax.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange("ECMax");
                }
            }
        }

        private SECSData ecDef;
        public String ECDef
        {
            get { return ecDef.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    ecDef.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange("ECDef");
                }
            }
        }

        private SECSData units;
        public String Units
        {
            get { return units.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    units.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange("Units");
                }
            }
        }

        private SECSData _ECV;
        public String ECV
        {
            get { return _ECV.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    try
                    {
                        _ECV.ChangeSECSDataFromStr(value);
                    }
                    catch
                    {
                        ECV = _ECV.ToString();
                    }
                    NotifyPropertyChange("ECV");
                }
            }
        }

        public SECSData GetECData()
        {
            return _ECV;
        }

        public void Dispose()
        {
            ecName = null;
            ecID = null; ;
            ecMin = null;
            ecMax = null;
            ECDef = null;
            Units = null;
            ECV = null;
        }
    }
}
