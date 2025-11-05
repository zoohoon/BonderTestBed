using CsvHelper;
using LogModule;
using PMIModuleParameter;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using System;
using System.IO;
using System.Linq;

namespace PMIModuleLoggerStandard
{
    public class PMILoggerStandard : IPMIModuleLogger, IFactoryModule
    {
        private PMIModuleDevParam PMIDevParam;

        private string FileFullPath { get; set; } = string.Empty;
        private string Extension { get; set; } = ".csv";
        private int ParamChangeCount { get; set; } = 0;

        public bool Initialized { get; set; } = false;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    PMIDevParam = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;

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
        public PMILoggerStandard()
        {

        }

        private EventCodeEnum PrintPMIHeaderInfo(string Filepath)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                using (TextWriter writer = File.CreateText(Filepath))
                {
                    var csvWriter = new CsvWriter(writer);

                    //
                    // Write one line.
                    //
                    writer.WriteLine("Header Test");
                    //
                    // Write two strings.
                    //
                    writer.Write("A ");
                    writer.Write("B ");
                    //
                    // Write the default newline.
                    //
                    writer.Write(writer.NewLine);

                    //var myObj = new MyCustomClass
                    //{
                    //    Prop1 = "one",
                    //    Prop2 = 2
                    //};

                    //// You can write a single record.
                    //csvWriter.WriteRecord(myObj);

                    //// You can also write a collection of records.
                    //var myRecords = new List<MyCustomClass> { myObj };
                    //csvWriter.WriteRecords(myRecords);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private string GetFullPath()
        {
            string retval = string.Empty;
            string folder = string.Empty;
            string filename = string.Empty;

            try
            {
                // TODO: 

                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if ((this.FileManager().GetLogRootPath() != null) &&
                                        (this.LotOPModule().LotInfo.LotName.Value != null) &&
                                        (this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value != null)
                                        )
                    {
                        folder = Path.Combine(
                            this.FileManager().GetLogRootPath(),
                            "PMI_Log",
                            this.LotOPModule().LotInfo.LotName.Value,
                            this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value);

                        if (Directory.Exists(Path.GetDirectoryName(folder)) == false)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(folder));
                        }

                        filename = string.Concat(
                            this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value,
                            ".pmi",
                            ParamChangeCount.ToString(),
                            Extension
                            );

                        retval = Path.Combine(folder, filename);

                        if (File.Exists(filename) == false)
                        {
                            PrintPMIHeaderInfo(filename);
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"Unknown Error");
                    }
                }
                else
                {
                    if ((this.FileManager().GetLogRootPath() != null))
                    {
                        string time = DateTime.Now.ToString("yyyyMMddHHmmss");

                        folder = Path.Combine(
                                        this.FileManager().GetLogRootPath(),
                                        "PMI_Log",
                                        "Manual"
                                        );

                        if (Directory.Exists(folder) == false)
                        {
                            Directory.CreateDirectory(folder);
                        }

                        filename = string.Concat(
                            time,
                            ".pmi",
                            Extension
                            );

                        retval = Path.Combine(folder, filename);

                        if (File.Exists(retval) == false)
                        {
                            PrintPMIHeaderInfo(retval);
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"Unknown Error");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public string GetPMIFileFullPath()
        {
            try
            {
                if(FileFullPath == string.Empty)
                {
                    FileFullPath = GetFullPath();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return FileFullPath;
        }

        public EventCodeEnum RecordPMIResultPerDie(MachineIndex MI)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (GetPMIFileFullPath() != string.Empty)
                {
                    IDeviceObject dev = null;

                    dev = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.ToList<DeviceObject>().Find(
                            x => x.DieIndexM.XIndex.Equals(MI.XIndex) && x.DieIndexM.YIndex.Equals(MI.YIndex));

                    if (dev == null)
                    {
                        LoggerManager.Error($"Unknown Error");
                    }
                    else
                    {
                        // TODO: 
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
        }
    }
}
