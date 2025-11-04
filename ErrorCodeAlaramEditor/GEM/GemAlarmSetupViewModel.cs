using System;
using System.Linq;

namespace EventCodeEditor.GEM
{
    using Autofac;
    using GEMModule;
    using LogModule;
    using NotifyModule.Parameter;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class GemAlarmSetupViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Property
        public Autofac.IContainer Container => this.GetContainer();
        private IGEMModule GemModule => Container.Resolve<IGEMModule>();
        public GEMAlarmParameter GEMAlarmParam { get; set; }
        private NotifySystemParameter NotifySystemParameter { get; set; }

        private GemAlarmDiscriptionParam _SelectedAlarmParam;
        public GemAlarmDiscriptionParam SelectedAlarmParam
        {
            get { return _SelectedAlarmParam; }
            set
            {
                if (value != _SelectedAlarmParam)
                {
                    _SelectedAlarmParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _AlarmID;
        public string GemAlarmID
        {
            get { return _AlarmID; }
            set
            {
                if (value != _AlarmID)
                {
                    _AlarmID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _GemAlarmRaiseOnlyLotEnable;
        public bool GemAlarmRaiseOnlyLotEnable
        {
            get { return _GemAlarmRaiseOnlyLotEnable; }
            set
            {
                if (value != _GemAlarmRaiseOnlyLotEnable)
                {
                    _GemAlarmRaiseOnlyLotEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _GemAlarmRaiseOnlyLotDisable;
        public bool GemAlarmRaiseOnlyLotDisable
        {
            get { return _GemAlarmRaiseOnlyLotDisable; }
            set
            {
                if (value != _GemAlarmRaiseOnlyLotDisable)
                {
                    _GemAlarmRaiseOnlyLotDisable = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Command & Method
        public void PageSwitched()
        {
            try
            {
                if(this.NotifyManager().NotifySysParam != null)
                {
                    NotifySystemParameter = this.NotifyManager().NotifySysParam as NotifySystemParameter;
                }

                if (GemModule.GemAlarmSysParam != null)
                {
                    GEMAlarmParam = GemModule.GemAlarmSysParam as GEMAlarmParameter;
                }
                GemAlarmRaiseOnlyLotEnable = true;
                GemAlarmRaiseOnlyLotDisable = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _AddAlarmCommand;
        public ICommand AddAlarmCommand
        {
            get
            {
                if (null == _AddAlarmCommand) _AddAlarmCommand = new RelayCommand(AddAlarmCommandFunc);
                return _AddAlarmCommand;
            }
        }
        private void AddAlarmCommandFunc()
        {
            try
            {
                long id = 0;
                long.TryParse(GemAlarmID, out id);
                if (id != 0 & (GEMAlarmParam.GemAlramInfos.Where(param => param.AlaramID == id).FirstOrDefault() == null))
                {
                    GEMAlarmParam.GemAlramInfos.Add(new GemAlarmDiscriptionParam() { AlaramID = id, RaiseOnlyLot = GemAlarmRaiseOnlyLotEnable });
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _DeleteAlarmCommand;
        public ICommand DeleteAlarmCommand
        {
            get
            {
                if (null == _DeleteAlarmCommand) _DeleteAlarmCommand = new RelayCommand(DeleteAlarmCommandFunc);
                return _DeleteAlarmCommand;
            }
        }
        private void DeleteAlarmCommandFunc()
        {
            try
            {
                if(SelectedAlarmParam != null)
                {
                    GEMAlarmParam.GemAlramInfos.Remove(SelectedAlarmParam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SaveParamCommand;
        public ICommand SaveParamCommand
        {
            get
            {
                if (null == _SaveParamCommand) _SaveParamCommand = new RelayCommand(SaveParamCommandFunc);
                return _SaveParamCommand;
            }
        }
        private void SaveParamCommandFunc()
        {
            GemModule.SaveSysParameter();
        }

        #endregion
    }
}
