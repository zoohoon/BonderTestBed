using System;
using LogModule;
using PopupControlElementControl;
using ProberErrorCode;
using ProberInterfaces;

namespace PopupControlViewModel
{
    public class ChillerModuleInfoElement : ModuleInfoElement
    {
        public ChillerModuleInfoElement()
        {
        }

        public override void DeInitModule()
        {

        }
        public override bool Initialized { get; set; } = false;

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.ModuleName = "Chiller";
                    this.CommunicationModule = this.TempController();

                    ChillerModuleInfoList Content = new ChillerModuleInfoList();
                    Content.SetContainer(Container);
                    this.Content = Content;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
