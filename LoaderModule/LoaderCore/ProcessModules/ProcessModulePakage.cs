using LoaderBase;
using System.Collections.Generic;

namespace LoaderCore
{
    public class ProcessModulePakage : ILoaderProcessModulePakage
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
        public ProcessModulePakage()
        {
            ProcModuleList = new List<ILoaderProcessModule>();
            ProcModuleList.Add(new ARMToChuck());
            ProcModuleList.Add(new ARMToFixedTray());
            ProcModuleList.Add(new ARMToInspectionTray());
            ProcModuleList.Add(new ARMToPreAlign());
            ProcModuleList.Add(new ARMToSlot());
            ProcModuleList.Add(new ChuckToARM());
            ProcModuleList.Add(new FixedTrayToARM());
            ProcModuleList.Add(new InspectionTrayToARM());
            ProcModuleList.Add(new ReadCognexOCR());
            ProcModuleList.Add(new ReadSemicsOCR());
            ProcModuleList.Add(new OCRToPreAlign());
            ProcModuleList.Add(new PreAlign());
            ProcModuleList.Add(new PreAlignToARM());
            ProcModuleList.Add(new PreAlignToOCR());
            ProcModuleList.Add(new CameraScanCassette());
            ProcModuleList.Add(new SensorScanCassette());
            ProcModuleList.Add(new SlotToARM());
        }
    }




}
