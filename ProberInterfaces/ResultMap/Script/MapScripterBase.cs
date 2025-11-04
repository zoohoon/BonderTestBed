using LogModule;
using ProberErrorCode;
using System;
using System.Reflection;

namespace ProberInterfaces.ResultMap.Script
{
    public interface IMapScript
    { }

    public interface IMapScripter
    {
        IMapScript Script { get; set; }
        Type ScriptType { get; set; }
        Type ScriptMethodAttributeType { get; set; }
    }

    public abstract class MapScripterBase : IMapScripter
    {
        public IMapScript Script { get; set; }

        public Type ScriptType { get; set; }
        public Type ScriptMethodAttributeType { get; set; }

        //public abstract EventCodeEnum MakeScript();
        public EventCodeEnum MakeScript()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                MethodInfo[] TheMethods = this.GetType().GetMethods();

                for (int i = 0; i < TheMethods.GetLength(0); i++)
                {
                    MethodInfo mi = TheMethods[i];

                    System.Attribute attr = mi.GetCustomAttribute(ScriptMethodAttributeType);

                    if (attr != null)
                    {
                        dynamic theStereotype = Convert.ChangeType(attr, ScriptMethodAttributeType);

                        //if (theStereotype.Version == this.Version)
                        //{
                        object[] parameters = new object[] { null, ScriptType };

                        object ret = mi.Invoke(this, parameters);

                        retval = (EventCodeEnum)ret;

                        if (retval == EventCodeEnum.NONE)
                        {
                            Script = parameters[0] as IMapScript;
                            break;
                        }
                        //}
                    }
                }

                if (Script != null)
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"[MapScripterBase], MakeScript() : Faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
