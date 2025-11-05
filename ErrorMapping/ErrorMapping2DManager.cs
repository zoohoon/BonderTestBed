using ErrorCompensation;
using ErrorParam;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XMLConverter;

namespace ErrorMapping
{
    public class ErrorMapping2DManager : INotifyPropertyChanged, IFactoryModule, IModule
    {

        //private string ErrorMappingParamFilePath = @"C:\ProberSystem\Parameters\VisionMapping\VisionMappingParameter.json";
        public const string ERROR_DATA_PATH = "c:\\PROBERPARAM\\ErrorData\\";

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private ErrorTableConverter ErrConvert = new ErrorTableConverter();
        public SecondErrorTable ErrorTable2D;

        public ErrorMapping2DManager()
        {
            try
            {
                if(this.MotionManager() != null)
                {
                    var compModule = this.MotionManager().ErrorManager.CompensationModule as ErrorCompensationModule;
                    ErrorTable2D = compModule.GetCurrentSecondErrorTable();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetErrorTable(SecondErrorTable table)
        {
            ErrorTable2D = table;
        }

        public void ResetErrorTable()
        {
            try
            {
                ErrorTable2D = ErrConvert.ResetError2DTable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SaveErrorTable()
        {
            try
            {
                if (ErrorTable2D != null)
                {
                    var compModule = this.MotionManager().ErrorManager.CompensationModule as ErrorCompensationModule;
                    compModule.SaveSecondErrorTable(ErrorTable2D);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadErrorTable()
        {
            try
            {
                if (ErrorTable2D != null)
                {
                    var compModule = this.MotionManager().ErrorManager.CompensationModule as ErrorCompensationModule;
                    compModule.LoadSecondErrorTable();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum ErrorMappingDataConvert(double dieSizeX, double dieSizeY, double positionX, double positionY)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var compModule = this.MotionManager().ErrorManager.CompensationModule as ErrorCompensationModule;
                compModule.LoadSecondErrorTable();

                retVal = compModule.ErrorMappingDataConvert(dieSizeX, dieSizeY, positionX, positionY);
                ErrorTable2D = compModule.GetErrorTable2D();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }



    }
}
