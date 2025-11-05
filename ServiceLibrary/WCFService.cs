using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Service;
using ServiceLibrary.Service;
using System;
using System.ComponentModel;
using System.IO;
using System.ServiceModel;

namespace ClientLibrary
{
    public class WCFService : IClientService, IClientDuplexCallback, INotifyPropertyChanged, IFactoryModule
    {
        private ClientClient service;
        private ClientDuplexClient callback;


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyProperytyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public WCFService()
        {

        }

        public void CheckClientConnect()
        {
        }

        public void OnGetDataEvent(RemoteFileInfo input)
        {
            try
            {
                int fileCount = input.FileNames.Length;
                string uploadFolder = Path.Combine(this.FileManager().GetRootParamPath(), "temp");

                string dllPath = null;
                string xmlPath = null;

                if (1 < fileCount)
                {
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    dllPath = Path.Combine(uploadFolder, input.FileNames[0]);
                    xmlPath = Path.Combine(uploadFolder, input.FileNames[1]);

                    if (File.Exists(dllPath))
                    {
                        System.IO.File.Delete(dllPath);
                    }

                    if (File.Exists(dllPath))
                    {
                        System.IO.File.Delete(xmlPath);
                    }

                    FileStream stream = new FileStream(dllPath, FileMode.Create);
                    using (BinaryWriter bw = new BinaryWriter(stream))
                    {
                        bw.Write(input.FileBuffer, 0, input.FileLengths[0]);
                    }
                    if (File.Exists(xmlPath))
                    {
                        System.IO.File.Delete(xmlPath);
                    }
                    stream = new FileStream(xmlPath, FileMode.Create);
                    using (BinaryWriter bw = new BinaryWriter(stream))
                    {
                        bw.Write(input.FileBuffer, input.FileLengths[0], input.FileLengths[1]);
                    }
                    stream.Close();

                    ModuleInfoProcess(dllPath, xmlPath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ModuleInfoProcess(string DllPath, string ParamPath)
        {
            try
            {
                ModuleDllInfo moduleDllInfo = null;
                LoadModuleDllInfo(ParamPath, ref moduleDllInfo);
                string uploadFolder = Path.Combine(this.FileManager().GetRootParamPath(), "temp");
                if (moduleDllInfo != null)
                {
                    moduleDllInfo.DLLPath = DllPath;
                    moduleDllInfo.ParamPath = ParamPath;
                    moduleDllInfo.ParamName = Path.Combine(uploadFolder, moduleDllInfo.ParamName);
                    IHasDll dllModule = this.LotOPModule() as IHasDll;
                    if (dllModule != null)
                    {
                        dllModule.InsertDllInfo(moduleDllInfo);
                    }
                    //foreach (var registration in Container.ComponentRegistry.Registrations)
                    //{
                    //    string moduleStr = registration.Activator.ToString();

                    //    if (moduleStr!=null && moduleStr.Contains( moduleDllInfo.ModuleName))
                    //    {
                    //        foreach (var service in registration.Services)
                    //        {

                    //                IHasDll dllModule = Container.Resolve<ILotOPModule>() as IHasDll;
                    //                if (dllModule != null)
                    //                {
                    //                    dllModule.InsertDllInfo(moduleDllInfo);
                    //                }

                    //        }
                    //    }
                    //foreach (var service in registration.Services)
                    //{
                    //    if (service.Description.ToString() == moduleDllInfo.ModuleName)
                    //    {
                    //        IHasDll dllModule = registration as IHasDll;
                    //        if (dllModule != null)
                    //        {
                    //            dllModule.InsertDllInfo(moduleDllInfo);
                    //        }
                    //    }
                    //}
                    //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool Connect()
        {
            bool retval = false;
            try
            {

                if (service == null && callback == null)
                {
                    retval = SetService();
                }
                else
                {
                    if (service.State == CommunicationState.Closed && callback.State == CommunicationState.Closed)
                    {
                        retval = SetService();
                    }
                    else
                    {
                    }

                    retval = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        private bool SetService()
        {
            bool retval = false;

            try
            {
                System.ServiceModel.BasicHttpBinding bindindg = new BasicHttpBinding();
                bindindg.MaxBufferPoolSize = 2147483647;
                bindindg.MaxBufferSize = 2147483647;
                bindindg.MaxReceivedMessageSize = 2147483647;
                bindindg.SendTimeout = TimeSpan.MaxValue;
                bindindg.ReceiveTimeout = TimeSpan.MaxValue;
                EndpointAddress endpointAddress = new EndpointAddress("http://localhost:10237/SimpleService");
                service = new ClientClient(bindindg, endpointAddress);

                System.ServiceModel.NetTcpBinding Callbackbinding = new NetTcpBinding();
                Callbackbinding.MaxBufferPoolSize = 2147483647;
                Callbackbinding.MaxBufferSize = 2147483647;
                Callbackbinding.MaxReceivedMessageSize = 2147483647;
                Callbackbinding.SendTimeout = TimeSpan.MaxValue;
                Callbackbinding.ReceiveTimeout = TimeSpan.MaxValue;
                EndpointAddress CallbackEndpointAddress = new EndpointAddress("net.tcp://localhost:10238/SimpleService");
                InstanceContext context = new InstanceContext(this);
                callback = new ClientDuplexClient(context, Callbackbinding, CallbackEndpointAddress);
                callback.RegisterClientCallback();

                retval = true;
            }
            catch
            {
                retval = false;
            }

            return retval;
        }

        public bool DisConnect()
        {
            bool retval = false;
            try
            {

                callback.Close();
                service.Close();

                service = null;
                callback = null;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum LoadModuleDllInfo(string filename, ref ModuleDllInfo moduleDllInfo)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string uploadFolder = this.FileManager().GetRootParamPath();
            string FullPath = Path.Combine(uploadFolder, "temp", filename);

            try
            {
                if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                }

                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(ModuleDllInfo), null, FullPath);

                moduleDllInfo = tmpParam as ModuleDllInfo;

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[WCFService] LoadModuleDllInfo(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }
    }



}
