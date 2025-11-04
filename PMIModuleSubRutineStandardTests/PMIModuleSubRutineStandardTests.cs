using Autofac;
using FileSystem.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PMIModuleParameter;
using PMIModuleSubRutineStandard;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMIModuleSubRutineStandard.Tests
{
    [TestClass()]
    public class PMIModuleSubRutineStandardTests : TestBaseProberSystem
    {
        [TestMethod()]
        public void DoPMITest()
        {
            Container.Resolve<IFileManager>(new NamedParameter("ParamPath", "C:\\ProberSystem\\C01"));

            var stagesupervisor = this.StageSupervisor();

            var info = this.PMIModule().DoPMIInfo;
            // TODO : SET this.PMIModule().DoPMIInfo
            // 1. Info.ProcessedPMIMIndex
            // 2. 
            // 3. 

            if (info.ProcessedPMIMIndex == null)
            {
                info.ProcessedPMIMIndex = new AsyncObservableCollection<MachineIndex>();
            }

            var mi = new MachineIndex(22, 50);

            info.ProcessedPMIMIndex.Add(mi);
            info.LastPMIDieResult.MI = mi;
            info.LastPMIDieResult.UI = this.CoordinateManager().MachineIndexConvertToUserIndex(mi);

            var camtype = (this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam).ProcessingCamType.Value;

            string modulename = "PMIModuleSubRutineStandard";
            string path = @"C:\Logs\Test\PMI\";

            string filename_1  = "240429094458_WLN4G051ESF6_Dut#1_X#-22_Y#-50_MAP#3_TABLE#1_PMIGroup#1_Mode#Single_Ori.jpeg";

            // NOT FOUND
            //string filename_1  = "NOT_FOUND\\240406025534_GC1TZ043SEH2_Dut#272_X#-26_Y#-13_MAP#7_TABLE#1_PMIGroup#8_Mode#Single_Ori.jpeg";
            //string filename_1  = "NOT_FOUND\\240406025543_GC1TZ043SEH2_Dut#298_X#0_Y#-13_MAP#7_TABLE#1_PMIGroup#8_Mode#Single_Ori.jpeg";

            // X
            //string filename_2 = "240429094501_WLN4G051ESF6_Dut#1_X#-22_Y#-50_MAP#3_TABLE#1_PMIGroup#2_Mode#Single_Ori.jpeg";

            string filename_3  = "240429094512_WLN4G051ESF6_Dut#1_X#-22_Y#-50_MAP#3_TABLE#1_PMIGroup#6_Mode#Single_Ori.jpeg";   
            string filename_4  = "240429094515_WLN4G051ESF6_Dut#1_X#-22_Y#-50_MAP#3_TABLE#1_PMIGroup#7_Mode#Single_Ori.jpeg";   
            string filename_5  = "240429094518_WLN4G051ESF6_Dut#1_X#-22_Y#-50_MAP#3_TABLE#1_PMIGroup#8_Mode#Single_Ori.jpeg";   

            string filename_6  = "240429094533_WLN4G051ESF6_Dut#873_X#-44_Y#-25_MAP#3_TABLE#1_PMIGroup#1_Mode#Single_Ori.jpeg"; 

            // X
            //string filename_7  = "240429094539_WLN4G051ESF6_Dut#873_X#-44_Y#-25_MAP#3_TABLE#1_PMIGroup#3_Mode#Single_Ori.jpeg";

            string filename_8  = "240429094616_WLN4G051ESF6_Dut#917_X#0_Y#-25_MAP#3_TABLE#1_PMIGroup#3_Mode#Single_Ori.jpeg";
            string filename_9  = "240429094631_WLN4G051ESF6_Dut#917_X#0_Y#-25_MAP#3_TABLE#1_PMIGroup#6_Mode#Single_Ori.jpeg";

            // X
            //string filename_10 = "240429094652_WLN4G051ESF6_Dut#1786_X#-22_Y#0_MAP#3_TABLE#1_PMIGroup#0_Mode#Single_Ori.jpeg";
            
            // X
            //string filename_11 = "240429094658_WLN4G051ESF6_Dut#1786_X#-22_Y#0_MAP#3_TABLE#1_PMIGroup#1_Mode#Single_Ori.jpeg";

            string fullpath = Path.Combine(path, filename_1);
            this.VisionManager().SetParameter(camtype, modulename, fullpath);

            //fullpath = Path.Combine(path, filename_2);
            //this.VisionManager().SetParameter(camtype, modulename, fullpath);

            fullpath = Path.Combine(path, filename_3);
            this.VisionManager().SetParameter(camtype, modulename, fullpath);

            fullpath = Path.Combine(path, filename_4);
            this.VisionManager().SetParameter(camtype, modulename, fullpath);

            fullpath = Path.Combine(path, filename_5);
            this.VisionManager().SetParameter(camtype, modulename, fullpath);

            fullpath = Path.Combine(path, filename_6);
            this.VisionManager().SetParameter(camtype, modulename, fullpath);

            //fullpath = Path.Combine(path, filename_7);
            //this.VisionManager().SetParameter(camtype, modulename, fullpath);

            fullpath = Path.Combine(path, filename_8);
            this.VisionManager().SetParameter(camtype, modulename, fullpath);

            fullpath = Path.Combine(path, filename_9);
            this.VisionManager().SetParameter(camtype, modulename, fullpath);

            //fullpath = Path.Combine(path, filename_10);
            //this.VisionManager().SetParameter(camtype, modulename, fullpath);

            //fullpath = Path.Combine(path, filename_11);
            //this.VisionManager().SetParameter(camtype, modulename, fullpath);

            var result = this.PMIModule().DoPMI();
        }
    }
}