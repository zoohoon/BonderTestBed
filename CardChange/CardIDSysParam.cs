using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using LogModule;
using Newtonsoft.Json;
using Focusing;

namespace CardChange
{
    /// <summary>
    /// Validate 함수
    /// </summary>
    public class CardLoadCondition
    {
        public CardLoadConditionTypeEnum ValidationType { get; set; }
        public CardTestModeEnum CardTestMode { get; set; }
        public List<CardLoadDetailCondition> DetailConditions { get; set; }
        public bool Validate(string cardid, out string msg)
        {
            bool result = true;
            msg = string.Empty;

            try
            {
                if (CardTestMode == CardTestModeEnum.InternalValidation)
                {
                    if (DetailConditions == null || DetailConditions.Count == 0)
                    {
                        result = false;
                        msg = "DetailConditions is missing or empty.";
                    }
                    else
                    {
                        LoggerManager.Debug($"Proceed to InternalValidation mode");

                        foreach (var condition in DetailConditions)
                        {
                            if (string.IsNullOrEmpty(cardid))
                            {
                                result = false;
                                if (!msg.Contains($"Card ID input is NULL.\n"))
                                {
                                    msg += $"Card ID input is NULL.\n";
                                }
                                continue;
                            }
                            else if (condition.FormatCondition.StartIndex + condition.FormatCondition.RangeLength > cardid.Length)
                            {
                                result = false;
                                string lengthMsg = $"The currently entered card ID [{cardid}] is short. Please write longer than '{condition.FormatCondition.StartIndex + condition.FormatCondition.RangeLength}' characters.\n";
                                if (!msg.Contains(lengthMsg))
                                {
                                    msg += lengthMsg;
                                }
                                continue;
                            }

                            string substring = cardid.Substring(condition.FormatCondition.StartIndex, condition.FormatCondition.RangeLength);

                            if (condition.ConditionStatus)
                            {
                                if (substring == condition.CardIDChecker)
                                {
                                    LoggerManager.Debug($"[CardIDSysparam] Validation successful: Current card ID '{substring}' matches condition substring '{condition.CardIDChecker}'.");
                                }
                                else
                                {
                                    result = false;
                                    string validationFailMsg = $"Validation failed: The current card ID '{substring}' does not match the success condition '{condition.CardIDChecker}'.\n";
                                    if (!msg.Contains(validationFailMsg))
                                    {
                                        msg += validationFailMsg;
                                    }
                                    LoggerManager.Debug($"[CardIDSysparam_Error] Validation failed: Current card ID '{substring}' does not match '{condition.CardIDChecker}'.");
                                }

                            }
                            else
                            {
                                if (substring == condition.CardIDChecker)
                                {
                                    result = false;
                                    string validationFailMsg = $"Validation failed: Current card ID '{substring}' matches '{condition.CardIDChecker}', but failed because Validation failed condition.\n";
                                    if (!msg.Contains(validationFailMsg))
                                    {
                                        msg += validationFailMsg;
                                    }
                                    LoggerManager.Debug($"[CardIDSysparam_Error] Validation failed: Current card ID '{substring}' matches '{condition.CardIDChecker}', but failed because Validation failed condition.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    // TODO : 향후 필요할 때, 개발
                    LoggerManager.Debug($"Proceed to ExternalValidation mode");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                result = false;
                msg = "[CardIDSysparam_Error] Validation failed due to an unknown error.";
            }
            return result;
        }


    }

    /// <summary>
    /// Cardid System Param
    /// </summary>
    public class CardLoadDetailCondition
    {
        public string CardIDChecker { get; set; }
        public bool ConditionStatus { get; set; }
        public FormatCondition FormatCondition { get; set; }
    }
    public class FormatCondition
    {
        public int StartIndex { get; set; }
        public int RangeLength { get; set; }
    }

    [Serializable]
    public class CardIDSysParam : INotifyPropertyChanged, IParamNode, ISystemParameterizable
    {
        #region PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IParamNode
        public string FilePath { get; } = "CardChange";
        public string FileName { get; } = "CardIDValidation.json";
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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        #endregion

        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<CardLoadCondition> _CardIDValidationsList;
        public List<CardLoadCondition> CardIDValidationsList
        {
            get { return _CardIDValidationsList; }
            set
            {
                if (_CardIDValidationsList != value)
                {
                    _CardIDValidationsList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = false;

                if (CardIDValidationsList == null)
                {
                    CardIDValidationsList = new List<CardLoadCondition>();
                }

                // TODO : 
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetElementMetaData()
        {

        }
        
    }
}
