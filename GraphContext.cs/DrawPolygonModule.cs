using System;

namespace GraphContext.cs
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public class DrawPolygonModule : ControlDrawableBase, INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region //..Property

        
        #endregion

        public override EventCodeEnum Draw(IDisplay display, ImageBuffer img)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                   
                //}));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Draw() : Error occured");
                LoggerManager.Exception(err);
            }
            return retval;
        }

    }
}
