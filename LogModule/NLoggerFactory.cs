using System;
using System.Collections.Generic;

namespace LogModule
{
    using LogModule.LoggerParam;
    using LogModule.LoggerRule;
    using NLog;
    using NLog.Config;
    public class NLoggerControllerResourceFactory
    {
        public LoggingConfiguration _Config;
        public Dictionary<EnumLoggerType, Logger> NLoggerDic { get; set; }
        public Dictionary<EnumLoggerType, NLoggerParam> NLoggerParamDic { get; set; }
        public Dictionary<EnumLoggerType, NLoggerRuleConfiger> NLoggerRuleDic { get; set; }
        public Dictionary<EnumLoggerType, String> NLoggerHeaderDic { get; set; }
        private List<EnumLoggerType> _RegisteredLoggerList;
        public NLoggerControllerResourceFactory()
        {
            _Config = new LoggingConfiguration();
            NLoggerDic = new Dictionary<EnumLoggerType, Logger>();
            NLoggerParamDic = new Dictionary<EnumLoggerType, NLoggerParam>();
            NLoggerRuleDic = new Dictionary<EnumLoggerType, NLoggerRuleConfiger>();
            NLoggerHeaderDic = new Dictionary<EnumLoggerType, String>();
            _RegisteredLoggerList = new List<EnumLoggerType>();
        }
        public void AddResource(EnumLoggerType loggerType, NLoggerParam loggerParam, NLoggerRuleConfiger nLoggerRuleConfiger)
        {
            NLoggerRuleDic.Add(loggerType, nLoggerRuleConfiger);
            NLoggerParamDic.Add(loggerType, loggerParam);
            _RegisteredLoggerList.Add(loggerType);
        }
        public void ConfigLoggerRule()
        {
            foreach (var loggerRule in NLoggerRuleDic)
            {
                EnumLoggerType loggerType = loggerRule.Key;
                NLoggerRuleConfiger loggerRuleConfiger = loggerRule.Value;


                String loggerName = loggerType.ToString();
                NLoggerParam param;
                
                if(NLoggerParamDic.TryGetValue(loggerType, out param))
                {

                    loggerRuleConfiger.Config(loggerName, _Config, param);
                }
            }

            //==> Set Configuration, it will influence globally
            LogManager.Configuration = _Config;

            foreach (EnumLoggerType loggerType in _RegisteredLoggerList)
            {
                Logger logger = LogManager.GetLogger(loggerType.ToString());
                NLoggerDic.Add(loggerType, logger);
            }
        }
    }
}