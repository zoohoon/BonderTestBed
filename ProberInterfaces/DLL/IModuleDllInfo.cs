using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public interface IModuleDllInfo
    {
        string DLLPath { get; set; }
        string AssemblyName { get; set; }
        ObservableCollection<string> ClassName { get; set; }
        int Version { get; set; }
        string ParamPath { get; set; }
        string ParamName { get; set; }
        bool EnableBackwardCompatibility { get; set; }
    }

    public interface IActionModuleDllInfo
    {
        string DLLPath { get; set; }
        string AssemblyName { get; set; }
        int Version { get; set; }
        bool EnableBackwardCompatibility { get; set; }
    }

    //[Serializable]
    //public class ActionInfos
    //{
    //    string CommandName { get; set; }

    //    AcitonInfo ActInfo { get; set; }
    //    string DLLPath { get; set; }
    //    string AssemblyName { get; set; }
    //    string ObjectName { get; set; }
    //    int Version { get; set; }
    //    string ParamPath { get; set; }
    //    string ParamName { get; set; }
    //    bool EnableBackwardCompatibility { get; set; }
    //}

    [Serializable]
    public class AssemblyInfo
    {
        public string AssemblyName { get; set; }
        public int Version { get; set; }

        public AssemblyInfo()
        {
            try
            {
                //this.AssemblyName = @"GPIBEvent.dll";
                //this.Version = 1000;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public AssemblyInfo(AssemblyInfo assemblyinfo)
        {
            try
            {
                this.AssemblyName = assemblyinfo.AssemblyName;
                this.Version = assemblyinfo.Version;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public AssemblyInfo(string assemblyname, int version)
        {
            try
            {
                this.AssemblyName = assemblyname;
                this.Version = version;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }


    [Serializable]
    public class AcitonInfo
    {
        public AcitonInfo()
        {

        }

        public string DLLPath { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public int Version { get; set; }

        public AcitonInfo(string dllpath, string assemblyname, string className, int version)
        {
            try
            {
                this.DLLPath = dllpath;
                this.AssemblyName = assemblyname;
                this.ClassName = className;
                this.Version = version;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable, DataContract]
    public class ModuleDllInfo : IModuleDllInfo, IParamNode
    {
        public ModuleDllInfo()
        {

        }

        public ModuleDllInfo(string assemblyname, int version, bool enablebackwardcompatibility)
        {
            try
            {
                this.AssemblyName = assemblyname;
                this.Version = version;
                this.EnableBackwardCompatibility = enablebackwardcompatibility;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleDllInfo(string assemblyname, string classname, int version, bool enablebackwardcompatibility)
        {
            try
            {
                this.AssemblyName = assemblyname;
                if (this.ClassName.Contains(classname))
                {
                    this.ClassName = new ObservableCollection<string>(this.ClassName.Distinct());
                }
                else
                {
                    this.ClassName.Add(classname);
                }

                this.Version = version;
                this.EnableBackwardCompatibility = enablebackwardcompatibility;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleDllInfo(string assemblyname, string classname)
        {
            try
            {
                this.AssemblyName = assemblyname;
                if (this.ClassName.Contains(classname))
                {
                    this.ClassName = new ObservableCollection<string>(this.ClassName.Distinct());
                }
                else
                {
                    this.ClassName.Add(classname);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleDllInfo(string dllpath, string assemblyname, string classname, int version, string parampath, string paramname, bool enablebackwardcompatibility) : this(assemblyname, classname, version, enablebackwardcompatibility)
        {
            try
            {
                this.DLLPath = dllpath;
                this.ParamPath = parampath;
                this.ParamName = paramname;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleDllInfo(string assemblyname, string namespacename, string classname, int version, bool enablebackwardcompatibility) : this(assemblyname, classname, version, enablebackwardcompatibility)
        {
            this.NameSpaceName = namespacename;
            this.AssemblyName = assemblyname;
            //this.ClassName.Add(classname);
            this.Version = version;
            this.EnableBackwardCompatibility = enablebackwardcompatibility;
        }

        //public ModuleDllInfo(string assemblyname, string classname, int version, bool enablebackwardcompatibility) : this(version, enablebackwardcompatibility)
        //{
        //    this.AssemblyName = assemblyname;
        //    this.ClassName.Add(classname);
        //}

        //public ModuleDllInfo(string dllpath, string assemblyname, string classname, int version, bool enablebackwardcompatibility) : this(version, enablebackwardcompatibility)
        //{
        //    this.DLLPath = dllpath;
        //    this.AssemblyName = assemblyname;
        //    this.ClassName.Add(classname);
        //}

        //public ModuleDllInfo(string dllpath, string assemblyname, ObservableCollection<string> classname, int version, string parampath, string paramname, bool enablebackwardcompatibility)
        //{
        //    this.DLLPath = dllpath;
        //    this.AssemblyName = assemblyname;
        //    this.ClassName = classname;
        //    this.Version = version;
        //    this.ParamPath = parampath;
        //    this.ParamName = paramname;
        //    this.EnableBackwardCompatibility = enablebackwardcompatibility;
        //}

        [ParamIgnore, DataMember]
        public string DLLPath { get; set; }
        [ParamIgnore, DataMember]
        public string AssemblyName { get; set; }
        [ParamIgnore, DataMember]
        public string NameSpaceName { get; set; }
        [ParamIgnore, DataMember]
        public ObservableCollection<string> ClassName { get; set; } = new ObservableCollection<string>();
        [ParamIgnore, DataMember]
        public int Version { get; set; }
        [ParamIgnore, DataMember]
        public string ParamPath { get; set; }
        [ParamIgnore, DataMember]
        public string ParamName { get; set; }
        [ParamIgnore, DataMember]
        public bool EnableBackwardCompatibility { get; set; }
        [ParamIgnore, DataMember]
        /// Loader Remote View의 경우 ProberSystem 과 크기를 맞춰주기 위해.
        public bool RemoteFlag { get; set; }
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public List<object> Nodes { get; set; }
    }

    public class ClassInfo : INotifyPropertyChanged
    {
        #region <remarks> PropertyChanged                           </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private string _AssemblyName;
        public string AssemblyName
        {
            get { return _AssemblyName; }
            set
            {
                if (value != _AssemblyName)
                {
                    _AssemblyName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ClassName;
        public string ClassName
        {
            get { return _ClassName; }
            set
            {
                if (value != _ClassName)
                {
                    _ClassName = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        public ClassInfo(string assemblyname, string classname)
        {
            this.AssemblyName = assemblyname;
            this.ClassName = classname;
        }

    }
}
