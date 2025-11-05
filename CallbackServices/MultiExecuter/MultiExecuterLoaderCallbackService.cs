using LogModule;
using ProberInterfaces;
using LoaderBase.Communication;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System;

namespace CallbackServices
{

    public class MultiExecuterLoaderCallbackService : ILoaderCommunicationManagerCallback, INotifyPropertyChanged, IFactoryModule
    {
       
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

       
        private InstanceContext _InstanceContext;
        public InstanceContext InstanceContext
        {
            get { return _InstanceContext; }
            set { _InstanceContext = value; }
        }

        private IMultiExecuter _MultiExecuter;
        public IMultiExecuter MultiExecuter {
            get { return _MultiExecuter; }
            set { _MultiExecuter = value; }
        }

        #endregion

        #region //..Creator & Init
        public MultiExecuterLoaderCallbackService(IMultiExecuter multiExecutor)
        {
            InstanceContext = new InstanceContext(this);
            if (_MultiExecuter == null)
            {
                _MultiExecuter = multiExecutor;
            }

        }

        public InstanceContext GetInstanceContext()
        {
            
            return InstanceContext;
        }
        #endregion

        #region //..Method

        public bool IsServiceAvailable()
        {
            return true;
        }


        public void LoaderExit()
        {
            _MultiExecuter.DisConnectLoader();

        }

 
        #endregion
    }
}

