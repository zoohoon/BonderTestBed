using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using RequestInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ProberInterfaces.ResultMap
{
    [Serializable]
    public abstract class NamingComponentBase : IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IParamNode

        public string Genealogy { get; set; }
        public List<object> Nodes { get; set; }
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

        #endregion

        [JsonIgnore]
        public INamer namer { get; set; }

        public abstract string Run();

        public RequestBase Formatter { get; set; }
    }

    [Serializable]
    public class NamingConstantComponent : NamingComponentBase
    {
        private Element<string> _constantValue = new Element<string>();
        public Element<string> ConstantValue
        {
            get { return _constantValue; }
            set
            {
                if (value != _constantValue)
                {
                    _constantValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NamingConstantComponent(string constantval)
        {
            this.ConstantValue.Value = constantval;
        }

        public override string Run()
        {
            return ConstantValue.Value;
        }
    }

    [Serializable]
    public class NamingProberPropertyComponent : NamingComponentBase
    {
        private Element<EnumProberMapProperty> _propertyKey = new Element<EnumProberMapProperty>();
        public Element<EnumProberMapProperty> PropertyKey
        {
            get { return _propertyKey; }
            set
            {
                if (value != _propertyKey)
                {
                    _propertyKey = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO : Request로 로직 구성.
        // EX) : Branch 구성 및 Format 변경 등.
        //public RequestBase reqeust { get; set; }

        public NamingProberPropertyComponent(EnumProberMapProperty name)
        {
            this.PropertyKey.Value = name;
        }

        public override string Run()
        {
            object obj = null;
            string retval = string.Empty;

            try
            {
                bool IsExist = false;

                IsExist = namer.ProberMapDictionary.TryGetValue(PropertyKey.Value, out obj);

                if (IsExist == true)
                {
                    if (Formatter != null)
                    {
                        Formatter.Argument = obj;
                        var ret = Formatter.Run();

                        if (ret == EventCodeEnum.NONE)
                        {
                            if (Formatter.Result != null)
                            {
                                retval = Formatter.Result.ToString();
                            }
                            else
                            {
                                retval = string.Empty;
                            }
                        }
                        else
                        {
                            retval = string.Empty;

                            LoggerManager.Error($"[NamingProberPropertyComponent], Run() : Formatter result is {ret}");
                        }
                    }
                    else
                    {
                        // Normal case
                        retval = obj.ToString();
                    }
                }
                else
                {
                    LoggerManager.Error($"[NamingProberPropertyComponent], Run() : IsExist is false.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class NamingSuffixComponent : NamingComponentBase
    {
        private Element<NamingSuffixType> _namingSuffixType = new Element<NamingSuffixType>();
        public Element<NamingSuffixType> NamingSuffixType
        {
            get { return _namingSuffixType; }
            set
            {
                if (value != _namingSuffixType)
                {
                    _namingSuffixType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _suffixLowerlimit = new Element<int>();
        public Element<int> SuffixLowerlimit
        {
            get { return _suffixLowerlimit; }
            set
            {
                if (value != _suffixLowerlimit)
                {
                    _suffixLowerlimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _suffixUpperlimit = new Element<int>();
        public Element<int> SuffixUpperlimit
        {
            get { return _suffixUpperlimit; }
            set
            {
                if (value != _suffixUpperlimit)
                {
                    _suffixUpperlimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void SetLimit(string format)
        {
            try
            {
                // RULE : 설정 된 SuffixType에 유효한 format으로 입력되었는지 확인.
                // 기본 룰 : A-Z

                SuffixLowerlimit.Value = 0;
                SuffixUpperlimit.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public NamingSuffixComponent(NamingSuffixType suffixType, string limitformat)
        {
            NamingSuffixType.Value = suffixType;

            SetLimit(limitformat);
        }

        public override string Run()
        {
            string retval = string.Empty;

            try
            {
                if (Validation() == true)
                {
                    // TODO : LOGIC
                    retval = "";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool Validation()
        {
            bool retval = false;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class NamingSeperatorComponent : NamingComponentBase
    {
        private Element<string> _seperatorValue = new Element<string>();
        public Element<string> SeperatorValue
        {
            get { return _seperatorValue; }
            set
            {
                if (value != _seperatorValue)
                {
                    _seperatorValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NamingSeperatorComponent(string seperator)
        {
            SeperatorValue.Value = seperator;
        }

        // 
        public override string Run()
        {
            string retval = string.Empty;

            try
            {
                if (Validation() == true)
                {
                    //retval = Convert.ToString(Seperator.Value);
                    retval = SeperatorValue.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool Validation()
        {
            bool retval = false;

            try
            {
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
