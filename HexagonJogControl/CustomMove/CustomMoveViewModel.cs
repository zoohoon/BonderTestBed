namespace CustomMoveViewModel
{
    using System;
    using Autofac;
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using VirtualKeyboardControl;
    using LoaderBase.Communication;

    public class CustomMoveVM : INotifyPropertyChanged, IFactoryModule
    {
        public CustomMoveVM()
        {
          
        }
       
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return _Container.Resolve<ILoaderCommunicationManager>();
            }
        }
        private IRemoteMediumProxy CurStageObj => LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
        private double _CustomXDistance = 0;
        public double CustomXDistance
        {
            get { return _CustomXDistance; }
            set
            {
                if (value != _CustomXDistance)
                {
                    _CustomXDistance = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CustomYDistance = 0;
        public double CustomYDistance
        {
            get { return _CustomYDistance; }
            set
            {
                if (value != _CustomYDistance)
                {
                    _CustomYDistance = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CustomZDistance = 0;
        public double CustomZDistance
        {
            get { return _CustomZDistance; }
            set
            {
                if (value != _CustomZDistance)
                {
                    _CustomZDistance = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> CustomXDistanceTextBoxClickCommand
        private RelayCommand _CustomXDistanceTextBoxClickCommand;
        public ICommand CustomXDistanceTextBoxClickCommand
        {
            get
            {
                if (null == _CustomXDistanceTextBoxClickCommand) _CustomXDistanceTextBoxClickCommand = new RelayCommand(CustomXDistanceTextBoxClickCommandFunc);
                return _CustomXDistanceTextBoxClickCommand;
            }
        }
        private void CustomXDistanceTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CustomXDistance.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (val >= 10000)
                    {
                        val = 9999;
                    }
                    else if (val <= -10000)
                    {
                        val = -9999;
                    }
                    CustomXDistance = val;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> CustomYDistanceTextBoxClickCommand
        private RelayCommand _CustomYDistanceTextBoxClickCommand;
        public ICommand CustomYDistanceTextBoxClickCommand
        {
            get
            {
                if (null == _CustomYDistanceTextBoxClickCommand) _CustomYDistanceTextBoxClickCommand = new RelayCommand(CustomYDistanceTextBoxClickCommandFunc);
                return _CustomYDistanceTextBoxClickCommand;
            }
        }
        private void CustomYDistanceTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CustomYDistance.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if(val >= 10000)
                    {
                        val = 9999;
                    }
                    else if(val <= -10000)
                    {
                        val = -9999;
                    }
                    CustomYDistance = val;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> CustomZDistanceTextBoxClickCommand
        private RelayCommand _CustomZDistanceTextBoxClickCommand;
        public ICommand CustomZDistanceTextBoxClickCommand
        {
            get
            {
                if (null == _CustomZDistanceTextBoxClickCommand) _CustomZDistanceTextBoxClickCommand = new RelayCommand(CustomZDistanceTextBoxClickCommandFunc);
                return _CustomZDistanceTextBoxClickCommand;
            }
        }
        private void CustomZDistanceTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CustomXDistance.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (val >= 10000)
                    {
                        val = 9999;
                    }
                    else if (val <= -10000)
                    {
                        val = -9999;
                    }
                    CustomZDistance = val;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion



        private RelayCommand _MoveCustomJogCommand;
        public RelayCommand MoveCustomJogCommand
        {
            get
            {
                if (null == _MoveCustomJogCommand)
                    _MoveCustomJogCommand = new RelayCommand(MoveCustomJogCmdFunc);
                return _MoveCustomJogCommand;
            }
        }
        private void MoveCustomJogCmdFunc()
        {
            try
            {
                CurStageObj.StageSupervisor().StageModuleState.StageRelMove(CustomXDistance, CustomYDistance);
                LoggerManager.Debug($"[CustomMoveVM]MoveCustomJogCmdFunc(): X = {CustomXDistance}, Y = {CustomYDistance}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}