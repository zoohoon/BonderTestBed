using LoaderBase;
using LoaderCore.GPProcessModules.CardChangeToCardARM;
using LoaderCore.GPProcessModules.CloseFoupCover;
using System.Collections.Generic;

namespace LoaderCore
{
    public class GP_ProcessModulePakage : ILoaderProcessModulePakage
    {
        private List<ILoaderProcessModule> _ProcModuleList;
        public List<ILoaderProcessModule> ProcModuleList
        {
            get { return _ProcModuleList; }
            set
            {
                if (value != _ProcModuleList)
                {
                    _ProcModuleList = value;
                }
            }
        }
        public GP_ProcessModulePakage()
        {
            ProcModuleList = new List<ILoaderProcessModule>();
            ProcModuleList.Add(new GP_ARMToBuffer());
            ProcModuleList.Add(new GP_ARMToChuck());
            ProcModuleList.Add(new GP_ARMToFixedTray());
            ProcModuleList.Add(new GP_ARMToInspectionTray());
            ProcModuleList.Add(new GP_ARMToPreAlign());
            ProcModuleList.Add(new GP_ARMToSlot());
            ProcModuleList.Add(new GP_BufferToARM());
            ProcModuleList.Add(new GP_CardARMToCardBuffer());
            ProcModuleList.Add(new GP_CardARMToCardChange());
            ProcModuleList.Add(new GP_CardARMToCardTray());

            ProcModuleList.Add(new GP_CardBufferToCardARM());
            ProcModuleList.Add(new GP_CardChangeToCardARM());
            ProcModuleList.Add(new GP_CardTrayToCardARM());

            ProcModuleList.Add(new GP_ChuckToARM());
            ProcModuleList.Add(new GP_InspectionTrayToARM());
            ProcModuleList.Add(new GP_ReadCognexOCR());
            ProcModuleList.Add(new GP_OCRToPreAlign());
            ProcModuleList.Add(new GP_PreAlign());
            ProcModuleList.Add(new GP_PreAlignToARM());
            ProcModuleList.Add(new GP_PreAlignToOCR());
            ProcModuleList.Add(new GP_SensorScanCassette());
            ProcModuleList.Add(new GP_SlotToARM());

            ProcModuleList.Add(new GP_FixedTrayToARM());
            ProcModuleList.Add(new GP_CloseFoupCover());
        }
    }

}
