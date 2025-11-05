using SECS_Host.Help;
using SECS_Host.Model;
using SECS_Host.SecsMsgClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using XComPro.Library;

namespace SECS_Host.View_Model
{
    public class VM_DialogAlarmList : ViewModelBase
    {
        public delegate void CloseDelegate(object sender, System.EventArgs e);

        private XComProW m_XComPro;
        public CloseDelegate CloseWindow;
        public AddLogDelegate AddLog;
        private SecsMsgBase SecsMsg;

        private ObservableCollection<AlarmElement> _AlarmCollection;
        public ObservableCollection<AlarmElement> AlarmCollection
        {
            get
            {
                return _AlarmCollection;
            }
            set
            {
                _AlarmCollection = value;
                NotifyPropertyChange(nameof(AlarmCollection));
            }
        }

        public VM_DialogAlarmList(XComProW _xComPro)
        {
            m_XComPro = _xComPro;
            _AlarmCollection = new ObservableCollection<AlarmElement>();

            //AlarmElement test = new AlarmElement();
            //test.SetAlarm(new SECSGenericData<List<SECSData>>()
            //{
            //    Value = new List<SECSData>()    {   new SECSGenericData<string>() { Value = "e1" },
            //                                        new SECSGenericData<string>() { Value = "e2" },
            //                                        new SECSGenericData<string>() { Value = "e3" }
            //                                    }
            //});

            //_AlarmCollection.Add(test);
        }

        public void SetSecsMsg(SecsMsgBase _secsMsg)
        {
            SecsMsg = _secsMsg;
            SecsMsg.m_XComPro = m_XComPro;
            SecsMsg.AddLog = AddLog;
        }

        public void SetAlarmCollection(SECSData SecsStruct)
        {
            AlarmCollection.Clear();

            if (SecsStruct != null)
            {
                foreach (var v in (SecsStruct as SECSGenericData<List<SECSData>>)?.Value)
                {
                    SECSGenericData<List<SECSData>> tempList = (SECSGenericData<List<SECSData>>)v;
                    AlarmElement tmpEquip = new AlarmElement();

                    tmpEquip.SetAlarm(tempList);
                    tmpEquip.dlgt_ChangeAlarmAble = ChangeAlarmAble;

                    AlarmCollection.Add(tmpEquip);
                }
            }

            (SecsStruct as SECSGenericData<List<SECSData>>)?.Value.Clear();
            //SecsStruct.Count = 0;
            //SecsStruct = null;
        }

        public void ChangeAlarmAble(AlarmElement ae)
        {
            SECSGenericData<List<SECSData>> sendDataList
                = new SECSGenericData<List<SECSData>>()
                {
                    Value = new List<SECSData>(),
                    MsgType = SECS_DataTypeCategory.LIST
                };

            byte bIsAble = 0;
            if ((ae.GetSECSData_IsAble() as SECSGenericData<bool>)?.Value == false)
                bIsAble = 0;
            else
                bIsAble = 1;

            sendDataList.Value.Add(new SECSGenericData<byte>() { Value = bIsAble, MsgType = SECS_DataTypeCategory.BINARY });
            sendDataList.Value.Add(ae.GetSECSData_ALID());

            SetSecsMsg(new SecsMsg_S5F3());
            SecsMsg.SetSendData(sendDataList);
            SecsMsg.Excute();
        }

        private ICommand _CloseWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                if (_CloseWindowCommand == null)
                    _CloseWindowCommand = new RelayCommand(CloseWindowFunc);
                return _CloseWindowCommand;
            }
        }

        public void CloseWindowFunc(object obj)
        {
            Dispose();
            CloseWindow(this, null);
        }

        public void Dispose()
        {
            if (AlarmCollection != null)
            {
                foreach (var v in AlarmCollection)
                {
                    v.Dispose();
                }
                AlarmCollection.Clear();
                AlarmCollection = null;
            }
        }
    }

    public class AlarmElement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChange(string propString)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propString));
        }

        public enum AlarmValueName
        {
            NONE = -1, ALCD = 0,
            ALID = 1, ALTX = 2, ISABLE = 3
        }

        public void SetAlarm(SECSGenericData<List<SECSData>> tempList)
        {
            alCD = tempList.Value[(int)AlarmValueName.ALCD];
            alID = tempList.Value[(int)AlarmValueName.ALID];
            alTX = tempList.Value[(int)AlarmValueName.ALTX];

            if (3 < tempList.Value.Count)
                isAble = tempList.Value[(int)AlarmValueName.ISABLE];
            else
                isAble = new SECSGenericData<bool>() { Value = false, MsgType = SECS_DataTypeCategory.BOOL };
        }

        private SECSData alID;
        public String ALID
        {
            get { return alID.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    alID.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange(nameof(ALID));
                }
            }
        }
        public SECSData GetSECSData_ALID()
        {
            return alID;
        }

        private SECSData alCD;
        public String ALCD
        {
            get
            { return alCD.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    alCD.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange(nameof(ALCD));
                }
            }
        }
        public SECSData GetSECSData_ALCD()
        {
            return alCD;
        }

        private SECSData isAble;
        public String IsAble
        {
            get { return isAble?.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    isAble.ChangeSECSDataFromStr(value);
                    NotifyPropertyChange(nameof(IsAble));
                }
            }
        }
        public SECSData GetSECSData_IsAble()
        {
            return isAble;
        }

        private SECSData alTX;
        public String ALTX
        {
            get {   return alTX.ToString(); }
            set
            {
                if (value == null)
                    value = null;
                else
                {
                    try
                    {
                        alTX.ChangeSECSDataFromStr(value);
                    }
                    catch
                    {
                        ALTX = alTX.ToString();
                    }
                    NotifyPropertyChange(nameof(ALTX));
                }
            }
        }
        public SECSData GetSECSData_ALTX()
        {
            return alTX;
        }

        public delegate void Delegate_SendAlarm(AlarmElement ae);

        public Delegate_SendAlarm dlgt_ChangeAlarmAble = null;
        private ICommand _ChangeAlarmAbleCommand;
        public ICommand ChangeAlarmAbleCommand
        {
            get
            {
                if (_ChangeAlarmAbleCommand == null)
                    _ChangeAlarmAbleCommand = new RelayCommand(ChangeAlarmAble);
                return _ChangeAlarmAbleCommand;
            }
        }

        public void ChangeAlarmAble(object obj)
        {
            if (dlgt_ChangeAlarmAble != null)
            {
                if((isAble as SECSGenericData<bool>)?.Value == false)
                    IsAble = bool.TrueString;
                else
                    IsAble = bool.FalseString;
                dlgt_ChangeAlarmAble(this);
            }
        }

        //public delegate 

        public void Dispose()
        {
            alID = null;
            alCD = null;
            alTX = null;
            dlgt_ChangeAlarmAble = null;
        }
    }
}
