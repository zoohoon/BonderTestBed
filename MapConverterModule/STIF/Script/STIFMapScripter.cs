using LogModule;
using ProberErrorCode;
using ProberInterfaces.ResultMap.Script;
using System.Collections.Generic;
using System;

namespace MapConverterModule.STIF
{

    public class STIFMapScripter : MapScripterBase
    {
        public double Version { get; set; }
        public void SetVersion(double version)
        {
            if (version != 1.1 && version != 1.2 && version != 1.3)
            {
                LoggerManager.Error($"[STIFMapScripter], SetVersion() : Input version is wrong. Version = {version}");
            }
            else
            {
                this.Version = version;
            }
        }

        public STIFMapScripter()
        {
            this.ScriptType = typeof(STIFScript);
            this.ScriptMethodAttributeType = typeof(STIFMapScriptVersionAttribute);
        }
        public double GetVersion()
        {
            return Version;
        }

        //public override EventCodeEnum MakeScript()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        MethodInfo[] TheMethods = this.GetType().GetMethods();

        //        for (int i = 0; i < TheMethods.GetLength(0); i++)
        //        {
        //            MethodInfo mi = TheMethods[i];

        //            System.Attribute attr = mi.GetCustomAttribute(ScriptMethodAttributeType);

        //            if (attr != null)
        //            {
        //                dynamic theStereotype = Convert.ChangeType(attr, ScriptMethodAttributeType);

        //                if (theStereotype.Version == this.Version)
        //                {
        //                    object[] parameters = new object[] { null, ScriptType };

        //                    object ret = mi.Invoke(this, parameters);

        //                    retval = (EventCodeEnum)ret;

        //                    if (retval == EventCodeEnum.NONE)
        //                    {
        //                        Script = parameters[0] as IMapScript;
        //                    }
        //                }
        //            }
        //        }

        //        if (Script != null)
        //        {
        //            retval = EventCodeEnum.NONE;
        //        }
        //        else
        //        {
        //            LoggerManager.Error($"[STIFMapScripter], MakeScript() : Faild.");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private void SetCommonScriptData(out IMapScript script)
        {
            script = null;

            try
            {
                STIFScript stif_script = new STIFScript();

                if (stif_script.STIFComponentOrder == null)
                {
                    stif_script.STIFComponentOrder = new List<STIFCOMPONENTTYPE>();
                }
                else
                {
                    stif_script.STIFComponentOrder.Clear();
                }

                stif_script.STIFComponentOrder.Add(STIFCOMPONENTTYPE.SIGNATURE);
                stif_script.STIFComponentOrder.Add(STIFCOMPONENTTYPE.HEADER);
                stif_script.STIFComponentOrder.Add(STIFCOMPONENTTYPE.MAP);
                stif_script.STIFComponentOrder.Add(STIFCOMPONENTTYPE.FOOTER);

                if (stif_script.HeaderParameters == null)
                {
                    stif_script.HeaderParameters = new List<MapScriptElement>();
                }
                else
                {
                    stif_script.HeaderParameters.Clear();
                }

                if (stif_script.MapParameters == null)
                {
                    stif_script.MapParameters = new List<MapScriptElement>();
                }
                else
                {
                    stif_script.MapParameters.Clear();
                }

                if (stif_script.FooterParameters == null)
                {
                    stif_script.FooterParameters = new List<MapScriptElement>();
                }
                else
                {
                    stif_script.FooterParameters.Clear();
                }

                script = stif_script;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        [STIFMapScriptVersionAttribute(version: 1.1)]
        public EventCodeEnum ScriptV1_1(out IMapScript script, Type attributetype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            script = null;

            try
            {
                SetCommonScriptData(out script);

                if (attributetype != null)
                {
                    dynamic stif_script = Convert.ChangeType(script, attributetype);

                    #region 타사 STIF 파일의 헤더 포맷에 맞춰서.. 2021-08-02

                    // Header
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "LOT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "WAFER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRODUCT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "READER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTEP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTEP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "FLAT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XREF");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YREF");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XFRST");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YFRST");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRQUAD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "COQUAD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DIAM");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DATE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TIME");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OPERATOR");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "SETUP FILE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBE CARD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTRP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTRP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "RPSEL");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "NULBC");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "GOODS");

                    stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMXDIM");
                    stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMYDIM");

                    stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "EDATE");
                    stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "ETIME");

                    #endregion

                    #region Document

                    ////// Header
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "LOT");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "WAFER");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRODUCT");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "READER");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTEP");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTEP");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "FLAT");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XREF");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YREF");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XFRST");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YFRST");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRQUAD");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "COQUAD");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DIAM");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTRP");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTRP");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "NULBC");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "GOODS");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DATE");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TIME");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "RPSEL");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "SETUP FILE");

                    ////// TODO : 2021-07-23 빼고 가즈아.
                    //////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TEST SYSTEM");
                    //////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TEST PROG");
                    //////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OLIFORMAT");
                    //////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OLIPATH");
                    //////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OPERATOR");

                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBE CARD");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBER");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "MERGEDATE");
                    ////stif_script.Add(STIFCOMPONENTTYPE.HEADER, "MERGETIME");

                    ////stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMXDIM");
                    ////stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMYDIM");

                    ////stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "EDATE");
                    ////stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "ETIME");

                    #endregion

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        [STIFMapScriptVersionAttribute(version: 1.2)]
        public EventCodeEnum ScriptV1_2(out IMapScript script, Type attributetype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            script = null;

            try
            {
                SetCommonScriptData(out script);

                if (attributetype != null)
                {
                    dynamic stif_script = Convert.ChangeType(script, attributetype);

                    // Header
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "LOT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "WAFER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRODUCT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "READER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTEP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTEP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "FLAT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XREF");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YREF");

                    // Added
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XFDI", true);
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YFDI", true);

                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XFRST");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YFRST");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRQUAD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "COQUAD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DIAM");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTRP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTRP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "NULBC");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "GOODS");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DATE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TIME");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "RPSEL");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "SETUP FILE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TEST SYSTEM");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TEST PROG");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OLIFORMAT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OLIPATH");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OPERATOR");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBE CARD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "MERGEDATE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "MERGETIME");

                    stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMXDIM");
                    stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMYDIM");

                    stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "EDATE");
                    stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "ETIME");

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        [STIFMapScriptVersionAttribute(version: 1.3)]
        public EventCodeEnum ScriptV1_3(out IMapScript script, Type attributetype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            script = null;

            try
            {
                SetCommonScriptData(out script);

                if (attributetype != null)
                {
                    dynamic stif_script = Convert.ChangeType(script, attributetype);

                    // Header
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "LOT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "WAFER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRODUCT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "READER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTEP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTEP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "FLAT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XREF");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YREF");

                    // Added, // TODO : NEED TARGn ?
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XBE TARG1", true);
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YBE TARG1", true);
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XBE TARG2", true);
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YBE TARG2", true);
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TARGBC", true);

                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XFRST");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YFRST");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PRQUAD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "COQUAD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DIAM");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "XSTRP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "YSTRP");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "NULBC");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "GOODS");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "DATE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TIME");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "RPSEL");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "SETUP FILE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TEST SYSTEM");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "TEST PROG");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OLIFORMAT");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OLIPATH");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "OPERATOR");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBE CARD");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "PROBER");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "MERGEDATE");
                    stif_script.Add(STIFCOMPONENTTYPE.HEADER, "MERGETIME");

                    // Map
                    stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMXDIM");
                    stif_script.Add(STIFCOMPONENTTYPE.MAP, "WMYDIM");

                    // Footer
                    stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "EDATE");
                    stif_script.Add(STIFCOMPONENTTYPE.FOOTER, "ETIME");

                    retval = EventCodeEnum.NONE;
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
