using NLog;
using ProberInterfaces;
using ProberInterfaces.WaferAlign;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ProberInterfaces.Enum;
using ProberInterfaces.Error;
using RelayCommandBase;
using System.Windows.Input;
using ErrorMapping;
using ProberErrorCode;

namespace ProberSystem.UserControls.VisionMapping
{
    public class VisionMappingSetupBase : IMainScreenView, INotifyPropertyChanged
    {
        readonly Guid _PageGUID = new Guid("55FF5FBD-95DB-1006-F8A0-3512C831B2F5");
        public Guid ViewGUID { get { return _PageGUID; } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

      
        

        //ProberViewModel Model;
   
        private ErrorMappingManager _ErrMappingManager;
        public ErrorMappingManager ErrMappingManager
        {
            get { return _ErrMappingManager; }
        }

        //public VisionMappingSetupBase(ProberViewModel model)
        //{
        //    Model = model;
        //}
        public ErrorCodeEnum InitModule()
        {
            _ErrMappingManager = new ErrorMappingManager();
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;

            ErrMappingManager.InitModule();

            return RetVal;
        }
        public void DeInitModule()
        {

        }
        private RelayCommand<object> _CmdVisionCam1;
        public ICommand CmdVisionCam1
        {
            get
            {
                if (null == _CmdVisionCam1) _CmdVisionCam1 = new RelayCommand<object>(CmdVisionCam1Change);
                return _CmdVisionCam1;
            }
        }

        public string ViewModelType => throw new NotImplementedException();

        private void CmdVisionCam1Change(object noparam)
        {
            ErrMappingManager.TransferCurrentCam(0);
            this.VisionManager().StartGrab(ErrMappingManager.CurrentMappingCam.CamType);
        }

        public ErrorCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }

        public ErrorCodeEnum InitPage(object parameter = null)
        {
            return ErrorCodeEnum.NONE;
        }
    }
}
