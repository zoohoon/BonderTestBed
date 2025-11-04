using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProberInterfaces;
using System.IO;
using System.Windows.Controls;
using Autofac;
using System.Collections.ObjectModel;
using LogModule;

namespace DllImporter
{
    class DllItem
    {
        public string FullName { get; set; }

        public string DllName { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return DllName;
        }
    }
    public class DllImporter : IDllImporter
    {
        private List<Assembly> AssemList = new List<Assembly>();
        DllItem Dll = new DllItem();
        //private Dictionary<DllItem, IPinAlgorithm> regAlgo;

        public void init()
        {
            try
            {
                AssemList = new List<Assembly>();
                Dll = new DllItem();
                //regAlgo = new Dictionary<DllItem, IPinAlgorithm>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ObservableCollection<T> Assignable<T>(Assembly assemlist)
        {
            var interfacetype = typeof(T);
            //T instance = default(T);
            ObservableCollection<T> instance = new ObservableCollection<T>();
            Type typeOfAssignable = null;
            try
            {
                if (!assemlist.IsDynamic)
                {
                    foreach (Type type in assemlist.GetExportedTypes())
                    {
                        typeOfAssignable = type;
                        if (interfacetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                        {
                            instance.Add((T)Activator.CreateInstance(type));
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, $"Type of instance is {typeOfAssignable}");
            }

            return instance;
        }


        public List<string> GetAssignableInterfaceName(Assembly assembly)
        {
            List<string> instance = new List<string>();

            try
            {
                Type t = assembly.GetType();
                foreach (var type in assembly.GetTypes())
                {
                    Type[] ifaces = type.GetInterfaces();
                    foreach (Type itype in ifaces)
                    {
                        instance.Add(itype.Name);
                    }

                }
            }
            catch (Exception err)
            {
                throw err;
                //LoggerManager.Error($err, "GetAssignableInterfaceName() : Error occurred.");

            }
            return instance;
        }

        public bool CompairInterfaceName(List<string> interfaces, string name)
        {
            bool retVal = false;
            try
            {
                foreach (var iface in interfaces)
                {
                    if (name == iface)
                    {
                        retVal = true;
                        break;
                    }
                }
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
        }




        public List<T> Assignable2<T>(Assembly assemlist)
        {
            var interfacetype = typeof(T);
            T instance = default(T);

            List<T> instanceList = default(List<T>);

            try
            {
                foreach (Type type in assemlist.GetExportedTypes())
                {
                    if (interfacetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                    {
                        instance = (T)Activator.CreateInstance(type);
                        instanceList.Add(instance);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return instanceList;
        }

        public T Assignable<T>(Assembly assemlist, string name)
        {
            var interfacetype = typeof(T);
            T instance = default(T);

            try
            {
                foreach (Type type in assemlist.GetExportedTypes())
                {
                    if (interfacetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && type.Name == name)
                    {
                        instance = (T)Activator.CreateInstance(type);
                    }

                    if (instance != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return instance;
        }

        public T Assignable<T>(Assembly assemlist, string name, IContainer container)
        {
            var interfacetype = typeof(T);
            T instance = default(T);

            try
            {
                foreach (Type type in assemlist.GetExportedTypes())
                {
                    if (interfacetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && type.Name == name)
                    {
                        instance = (T)Activator.CreateInstance(type, container);
                    }

                    if (instance != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return instance;
        }

        public T Assignable<T>(Assembly assemlist, string name, IContainer container, List<string> EventInfo)
        {
            var interfacetype = typeof(T);
            T instance = default(T);

            try
            {
                foreach (Type type in assemlist.GetExportedTypes())
                {
                    if (interfacetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && type.Name == name)
                    {
                        instance = (T)Activator.CreateInstance(type, container, EventInfo);
                    }

                    if (instance != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return instance;
        }

        public T Assignable<T>(Assembly assemlist, string name, IContainer container, string STBAlias)
        {
            var interfacetype = typeof(T);
            T instance = default(T);

            try
            {
                foreach (Type type in assemlist.GetExportedTypes())
                {
                    if (interfacetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && type.Name == name)
                    {
                        instance = (T)Activator.CreateInstance(type, container, STBAlias);
                    }

                    if (instance != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return instance;
        }

        public T Assignable<T>(Assembly assemlist, string name, List<object> Paramlist)
        {
            var interfacetype = typeof(T);
            T instance = default(T);

            try
            {
                foreach (Type type in assemlist.GetExportedTypes())
                {
                    if (interfacetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && type.Name == name)
                    {
                        instance = (T)Activator.CreateInstance(type, Paramlist);
                    }

                    if (instance != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return instance;
        }

        public string GetUIFileName(string dllname)
        {
            try
            {
                string ui_folder_path = @"C:\workspace\UCDemo";
                var uidlls = Directory.GetFiles(ui_folder_path, dllname + "UI.dll");
                return uidlls.FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Tuple<bool, Assembly> LoadDLL(ModuleDllInfo DLLInfo)
        {
            Tuple<bool, Assembly> RetVal = null;

            try
            {
                string LoadDLLPath;

                //bool LoadFlag = false;

                //String strFolder = System.IO.Directory.GetCurrentDirectory();
                String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                //LoadDLLPath = strFolder + "\\" + DLLInfo.AssemblyName;
                LoadDLLPath = Path.Combine(strFolder, DLLInfo.AssemblyName);

                if (File.Exists(LoadDLLPath) == false)
                {
                    // ERROR
                    LoggerManager.Error($"[DllImporter], LoadDLL() : File not exist, Path = {LoadDLLPath}");
                }
                else
                {
                    var DLL = Assembly.LoadFrom(LoadDLLPath);
                    string version = DLL.GetName().Version.ToString();

                    try
                    {
                        RetVal = new Tuple<bool, Assembly>(true, DLL);

                        //int Iversion = Convert.ToInt32(version);

                        //// Check Version 
                        //if (Iversion > DLLInfo.Version)
                        //{
                        //    // ERROR ?
                        //}
                        //else if (Iversion == DLLInfo.Version)
                        //{
                        //    LoadFlag = true;
                        //}
                        //else
                        //{
                        //    if (DLLInfo.EnableBackwardCompatibility == true)
                        //    {
                        //        LoadFlag = true;
                        //    }
                        //}

                        //if (LoadFlag == true)
                        //{
                        //    RetVal = new Tuple<bool, Assembly>(true, DLL);
                        //}
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public Tuple<bool, Assembly> LoadDLL(string DLLPath, AssemblyInfo DLLInfo)
        {
            Tuple<bool, Assembly> RetVal = null;

            try
            {
                string LoadDLLPath;

                //bool LoadFlag = false;

                //String strFolder = System.IO.Directory.GetCurrentDirectory();
                String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                //LoadDLLPath = strFolder + "\\" + DLLPath + "\\" + DLLInfo.AssemblyName;
                LoadDLLPath = Path.Combine(strFolder, DLLPath, DLLInfo.AssemblyName);

                if (File.Exists(LoadDLLPath) == false)
                {
                    // ERROR
                }
                else
                {
                    var DLL = Assembly.LoadFile(LoadDLLPath);
                    string version = DLL.GetName().Version.ToString();

                    try
                    {
                        RetVal = new Tuple<bool, Assembly>(true, DLL);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public Tuple<bool, Assembly> LoadDLL(string DLLFullPath)
        {
            Tuple<bool, Assembly> RetVal = null;

            try
            {
                string LoadDLLPath;


                LoadDLLPath = DLLFullPath;

                if (File.Exists(LoadDLLPath) == false)
                {
                    // ERROR
                }
                else
                {
                    var DLL = Assembly.LoadFile(LoadDLLPath);
                    string version = DLL.GetName().Version.ToString();

                    try
                    {
                        RetVal = new Tuple<bool, Assembly>(true, DLL);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }


        //public Tuple<bool, Assembly> LoadDLL(AcitonInfo DLLInfo)
        //{
        //    Tuple<bool, Assembly> RetVal = null;

        //    try
        //    {
        //        string LoadDLLPath;

        //        //bool LoadFlag = false;

        //        String strFolder = System.IO.Directory.GetCurrentDirectory();

        //        LoadDLLPath = strFolder + "\\" + DLLInfo.DLLPath + "\\" + DLLInfo.AssemblyName;

        //        if (File.Exists(LoadDLLPath) == false)
        //        {
        //            // ERROR
        //        }
        //        else
        //        {
        //            var DLL = Assembly.LoadFile(LoadDLLPath);
        //            string version = DLL.GetName().Version.ToString();

        //            try
        //            {
        //                RetVal = new Tuple<bool, Assembly>(true, DLL);

        //                //int Iversion = Convert.ToInt32(version);

        //                //// Check Version 
        //                //if (Iversion > DLLInfo.Version)
        //                //{
        //                //    // ERROR ?
        //                //}
        //                //else if (Iversion == DLLInfo.Version)
        //                //{
        //                //    LoadFlag = true;
        //                //}
        //                //else
        //                //{
        //                //    if (DLLInfo.EnableBackwardCompatibility == true)
        //                //    {
        //                //        LoadFlag = true;
        //                //    }
        //                //}

        //                //if (LoadFlag == true)
        //                //{
        //                //    RetVal = new Tuple<bool, Assembly>(true, DLL);
        //                //}
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }

        //    return RetVal;
        //}


        public Tuple<bool, object> LoadInstance(ModuleDllInfo DLLInfo)
        {
            Tuple<bool, object> RetVal = null;

            try
            {
                string LoadDLLPath;

                //String strFolder = System.IO.Directory.GetCurrentDirectory();
                String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                //LoadDLLPath = strFolder + "\\" + DLLInfo.AssemblyName;
                LoadDLLPath = Path.Combine(strFolder, DLLInfo.AssemblyName);

                if (File.Exists(LoadDLLPath) == false)
                {
                    // ERROR
                }
                else
                {
                    object obj = null;
                    Assembly DLL = Assembly.LoadFrom(LoadDLLPath);
                    string version = DLL.GetName().Version.ToString();

                    try
                    {
                        if(DLL != null)
                        {
                            foreach (Type type in DLL.GetExportedTypes())
                            {
                                if(DLLInfo.ClassName[0].Equals(type.Name))
                                {
                                    obj = Activator.CreateInstance(type);
                                }
                            }
                        }
                        RetVal = new Tuple<bool, object>(true, obj);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        public T LoadUserControlDll<T>(T type)
        {
            var Type = type;
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"C:";
                ofd.DefaultExt = ".dll";
                ofd.Filter =
                    "DLL files(*.dll)| *.dll|" +
                    "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" +
                    "All files (*.*)|*.*";
                ofd.Multiselect = false;

                Nullable<bool> result = ofd.ShowDialog();
                if (result == true)
                {
                    Dll.FullName = ofd.FileName;
                    Dll.DllName = System.IO.Path.GetFileName(Dll.FullName);
                    Dll.FileName = Path.GetFileNameWithoutExtension(Dll.FullName);
                    var assem = Assembly.LoadFile(Dll.FullName);
                    //Type = Assignable<T>(assem);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Type;
        }
        public UserControl LoadUserControlDll()
        {
            System.Windows.Controls.UserControl uc = null;
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"C:";
                ofd.DefaultExt = ".dll";
                ofd.Filter =
                    "DLL files(*.dll)| *.dll|" +
                    "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" +
                    "All files (*.*)|*.*";
                ofd.Multiselect = false;

                Nullable<bool> result = ofd.ShowDialog();
                if (result == true)
                {
                    Dll.FullName = ofd.FileName;
                    Dll.DllName = System.IO.Path.GetFileName(Dll.FullName);
                    Dll.FileName = Path.GetFileNameWithoutExtension(Dll.FullName);
                    var assem = Assembly.LoadFile(Dll.FullName);
                    //uc = Assignable<UserControl>(assem);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return uc;
        }
    }
}
