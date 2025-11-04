using LogModule;
using PopupControlElementControl;
using ProberErrorCode;
using ProberInterfaces;
using System;

namespace PopupControlViewModel
{
    public class GPIBModuleInfoElement : ModuleInfoElement
    {
        public GPIBModuleInfoElement()
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
                    this.ModuleName = "GPIB";
                    this.CommunicationModule = this.GPIB();

                    GPIBModuleInfoList Content = new GPIBModuleInfoList();
                    Content.SetContainer(this.GetContainer());
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
