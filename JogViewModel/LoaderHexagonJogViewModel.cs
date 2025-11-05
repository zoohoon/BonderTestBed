using System;

namespace JogViewModelModule
{
    using Autofac;
    using LoaderBase.Communication;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Enum;

    public class LoaderHexagonJogViewModel : IHexagonJogViewModel
    {
        private Autofac.IContainer _Container;
        public EnumAxisConstants AxisForMapping { get; set; }
        public bool SetMoveZOffsetEnable { get; set; }
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return _Container.Resolve<ILoaderCommunicationManager>();
            }
        }
        private IRemoteMediumProxy remoteMediumProxy => LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        public LoaderHexagonJogViewModel()
        {

        }

        public void SetContainer(Autofac.IContainer container)
        {
            _Container = container;
        }

        public void StickIndexMove(JogParam parameter, bool setzoffsetenable)
        {
            try
            {
                remoteMediumProxy.StickIndexMove(parameter, setzoffsetenable);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StickStepMove(JogParam parameter)
        {
            try
            {
                remoteMediumProxy.StickStepMove(parameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
