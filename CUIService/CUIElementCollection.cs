using CUIToolTipInfoData;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LogModule;

namespace CUIServices
{
    [Serializable]
    public class CUIElementCollection : ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore, ParamIgnore]
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

        public const string magicGUID = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        static int key_Limit_length = 50;

        public List<CUIElementInfo> Infos;

        public int lcid;

        public CUIElementInfo MakeCUIElementInfo(Guid cuiguid, int maskinglevel, Guid targetviewguid,
                                                bool Visibility = false, bool lockable = true, string key = null,
                                                string description = null, ToolTipInfoType tootipInfoType = ToolTipInfoType.NONE)
        {
            CUIElementInfo RetVal = new CUIElementInfo();

            try
            {
                RetVal.CUIGUID = cuiguid;
                RetVal.MaskingLevel = maskinglevel;
                RetVal.TargetViewGUID = targetviewguid;
                RetVal.Lockable = lockable;

                if (key != null)
                {
                    if (key.Length > key_Limit_length)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        RetVal.key = key;
                    }
                }

                RetVal.Description = description;

                IToolTipInfoBase toolTipInfo = TooltipInfoFactory.GetTooltipInfo(tootipInfoType, key, description);

                RetVal.tooltipinfo = toolTipInfo;
            }
            catch (Exception err)
            {
                throw err;
            }

            return RetVal;
        }

        public string FilePath { get; } = "";

        public string FileName { get; } = "CUIInfo.json";
        public EventCodeEnum SetEmulParam()
        {
            // TODO : Temporary code 
            //return MakeDefaultParamSetFor_BSCI();

            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            //CUIElementInfo makedInfo = null;

            try
            {
                //string magicGUID = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";
                // Language

                // English
                lcid = 1033;

                if (Infos == null)
                {
                    Infos = new List<CUIElementInfo>();
                }

                //Infos.Add(MakeCUIElementInfo(new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
                //                             0,
                //                             new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                //                             ));

                #region OperatorView

                Infos.Add(MakeCUIElementInfo(new Guid("1b5ef581-fac0-4bea-a73b-3b4dc88c05ac"),
                                             0,
                                             new Guid("0922A4D5-D811-425D-A68F-6CE5A210102B"),
                                             false, true,
                                             ""));


                Infos.Add(MakeCUIElementInfo(new Guid("31ad3cc0-e934-477e-a51c-18bc2df52ce6"),
                                             0,
                                             new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"),
                                             false, true,
                                             ""));

                Infos.Add(MakeCUIElementInfo(new Guid("f88475b7-c1bb-4c4a-996c-64617f506e78"),
                                             0,
                                             new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"),
                                             false, true,
                                             ""));

                Infos.Add(MakeCUIElementInfo(new Guid("c43edfed-f815-4b29-ab04-d68f110a56db"),
                                             0,
                                             new Guid("5B755687-AE34-4BBF-8112-71738B9F91D8"),
                                             false, true,
                                             ""));


                #endregion

                #region LotScreen

                // LotStartButton
                Infos.Add(MakeCUIElementInfo(new Guid("ba1fc6ab-088c-4a2d-a51a-ccc9123ba079"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_START", "LOT_START_DESCRIPTION", ToolTipInfoType.SIMPLE));

                // LotEndButton
                Infos.Add(MakeCUIElementInfo(new Guid("d293677c-09e4-45df-9e78-a5dba2daa1ae"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOTSCREEN_LOTEND_BUTTON"));

                // LotSetupButton
                Infos.Add(MakeCUIElementInfo(new Guid("ff73b7f2-3f38-4375-8b1d-137d62d89336"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_SETUP", "LOT_SETUP_DESCRIPTION", ToolTipInfoType.IMAGE));

                // LotInfo_Yield Button
                Infos.Add(MakeCUIElementInfo(new Guid("fcc03d05-202d-41c0-8f0a-4ee63961a4b2"),
                                             0,
                                             new Guid("b0722a2f-bc56-4e74-88e7-63fbf4ec7d63"),
                                             false, false,
                                             "LOT_INFO_YIELD", "LOT_INFO_YIELD_DESCRIPTION"));

                // LotInfo_Recipe Button
                Infos.Add(MakeCUIElementInfo(new Guid("c820c484-7b89-4cd0-b677-2d506f08ae7d"),
                                             0,
                                             new Guid("7c94444f-d655-407b-9a7e-0b938505eb99"),
                                             false, false,
                                             "LOT_INFO_RECIPE", "LOT_INFO_YIELD_DESCRIPTION"));

                // OCR MANUAL INPUT Button
                Infos.Add(MakeCUIElementInfo(new Guid("c3aa3a94-8179-466a-a9e3-36f16f4e03fb"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "OCR_MANUAL_INPUT", "OCR_MANUAL_INPUT_DESCRIPTION"));

                // LotInfo_Device Setting Button
                Infos.Add(MakeCUIElementInfo(new Guid("1057874e-b364-4ac1-9845-77ae011bc512"),
                                             0,
                                             new Guid("436db5ea-e49e-4fca-a1a8-e3be24639e31"),
                                             false, false,
                                             "LOT_INFO_DEVICE_SETTING", "LOT_INFO_YIELD_DESCRIPTION"));

                // Lot Fast Rewind
                Infos.Add(MakeCUIElementInfo(new Guid("b1aa95e2-8943-412c-8b46-e754db6cba4d"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_FAST_REWIND", "LOT_FAST_REWIND_DESCRIPTION"));

                // Lot Normal Rewind
                Infos.Add(MakeCUIElementInfo(new Guid("59a307d1-8e6b-442e-8bc8-68ceeb93b66c"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_NORMAL_REWIND", "LOT_NORMAL_REWIND_DESCRIPTION"));

                // Lot Normal Forward
                Infos.Add(MakeCUIElementInfo(new Guid("dfe6543f-f852-4c43-ba92-1902af1412cb"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_NORMAL_FORWARD", "LOT_NORMAL_FORWARD_DESCRIPTION"));

                // Lot Fast Forward
                Infos.Add(MakeCUIElementInfo(new Guid("761d35d9-780d-440e-8661-188cc3799525"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_FAST_FORWARD", "LOT_FAST_FORWARD_DESCRIPTION"));

                // Lot Restart
                Infos.Add(MakeCUIElementInfo(new Guid("a0744ecd-c106-40eb-abda-e6bc52a40516"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_RESTAT", "LOT_RESTART_DESCRIPTION"));

                // App Selector
                Infos.Add(MakeCUIElementInfo(new Guid("59217259-A2D7-4F9E-AD58-182FA46F4EB5"),
                                             0,
                                             new Guid(magicGUID),
                                             false, false,
                                             "LOT_APP_SECTOR", "LOT_APP_SECTOR_DESCRIPTION"));

                #endregion

                #region TopBar-Main

                // HomeScreen
                Infos.Add(MakeCUIElementInfo(new Guid("3776199a-0c2c-49c1-8f6c-5c2a5a9fa873"),
                                            999,
                                             new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"),
                                             false, true,
                                             "LOTSCREEN_MAINSCREEN"));

                // BackScreen
                Infos.Add(MakeCUIElementInfo(new Guid("5e80ff23-fbc8-4a56-9c31-a5d060397807"),
                                             999,
                                             new Guid(magicGUID),
                                             false, true,
                                             "BACK_SCREEN"));

                Infos.Add(MakeCUIElementInfo(new Guid("5e80ff23-fbc8-4a56-9c31-a5d060397802"),
                                             999,
                                             new Guid(magicGUID),
                                             false, true,
                                             "BELL_PROLOG_VIEW"));

                Infos.Add(MakeCUIElementInfo(new Guid("5e80ff23-fbc8-4a56-9c31-a5d060397803"),
                                             999,
                                             new Guid(magicGUID),
                                             false, true,
                                             "PROLOG_VIEW"));

                #endregion

                #region Loader MainView
                Infos.Add(MakeCUIElementInfo(new Guid("EF1F38E3-AF27-41CD-8EEA-99C06C30B0EC"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "", "ONLINE_btn"));
                Infos.Add(MakeCUIElementInfo(new Guid("BD90F7E0-E3B4-408E-8B47-C903021E3DA6"),
                                            0,
                                            new Guid(magicGUID),
                                            false, true,
                                            "", "OFFLINE_btn"));
                Infos.Add(MakeCUIElementInfo(new Guid("37A463EC-7191-4C76-B3A0-9357B29FEB34"),
                                            0,
                                            new Guid(magicGUID),
                                            false, true,
                                            "", "MAINTENANCE_btn"));
                Infos.Add(MakeCUIElementInfo(new Guid("80132709-BE3D-49FE-AAC0-449787C8DB1C"),
                                            0,
                                            new Guid(magicGUID),
                                            false, true,
                                            "", "Tester_Connection"));
                Infos.Add(MakeCUIElementInfo(new Guid("7BEF55D9-A1A2-472E-BF95-ED215960DE19"),
                                            0,
                                            new Guid(magicGUID),
                                            false, true,
                                            "", "Cell_Teach_Pin"));



                #endregion

                #region Loader Menu
                Infos.Add(MakeCUIElementInfo(new Guid("552BA89C-EDD6-4973-8193-B8614F85D128"),
                             1,
                             new Guid(magicGUID),
                             false, true,
                             "", "Loader_Menu_Operation"));
                Infos.Add(MakeCUIElementInfo(new Guid("E6F43425-0E60-423C-B686-22967B805314"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_OCR"));
                Infos.Add(MakeCUIElementInfo(new Guid("99351430-B1EA-4AFC-9849-BD62B810DFD8"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_File_Transfer"));
                Infos.Add(MakeCUIElementInfo(new Guid("BFE8EF22-C2C7-4A04-9E71-EB17A73FE684"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Wafer"));
                Infos.Add(MakeCUIElementInfo(new Guid("A939D780-7575-40AA-9F3C-CCD45366050C"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Probe_Card"));
                Infos.Add(MakeCUIElementInfo(new Guid("BC670597-E190-4D8F-AA42-FE2C1569DFD0"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Probe_Mark"));
                Infos.Add(MakeCUIElementInfo(new Guid("713C9C9C-452C-42C8-8FE8-C59F2FFA3AA8"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Card_Cleaning"));
                Infos.Add(MakeCUIElementInfo(new Guid("FBE51D5E-5C6A-4E3E-9C92-E6666FB121F1"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Result_Data"));
                Infos.Add(MakeCUIElementInfo(new Guid("05B2801C-8042-49AC-8C1F-80CDE89A0DD8"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Lot"));
                Infos.Add(MakeCUIElementInfo(new Guid("933CC99F-F98A-48F0-A42A-EB179DC0A864"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Temperature"));
                Infos.Add(MakeCUIElementInfo(new Guid("37831686-08A1-434D-91C8-DF4AF21AC7C0"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Device_Change"));
                Infos.Add(MakeCUIElementInfo(new Guid("8771333E-751C-4621-AFCC-0CF59A20037E"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Verify_parameter"));
                Infos.Add(MakeCUIElementInfo(new Guid("BA588D26-F6FC-4474-B931-7833DA2381F3"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Manual_Contact"));
                Infos.Add(MakeCUIElementInfo(new Guid("DC405C34-63A5-4D72-92B0-9A8CAAE1AD61"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Multi_Manual_Contact"));
                Infos.Add(MakeCUIElementInfo(new Guid("E070B319-D520-497B-BF51-6649C5AD44C3"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Wafer_Inspection"));
                Infos.Add(MakeCUIElementInfo(new Guid("3F625AFB-4FCF-48C5-A7E6-59F42C1542C5"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_TempCalibration"));
                Infos.Add(MakeCUIElementInfo(new Guid("2C529398-CE41-4A8A-847A-83B17C5975AB"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_CCSettingOp"));
                Infos.Add(MakeCUIElementInfo(new Guid("38D8E95F-1756-4BB4-947B-31E6C2BB30E9"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_CCObservationOP"));
                Infos.Add(MakeCUIElementInfo(new Guid("139E753B-D9B4-4E7D-AC28-C2209F1C0E31"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Tester Interface"));
                Infos.Add(MakeCUIElementInfo(new Guid("7B19A1F0-41AB-4E89-B394-1CD55C980525"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Manual Cleaning"));
                Infos.Add(MakeCUIElementInfo(new Guid("B8C5C694-09F7-4086-A3F6-4EEEB2D2CAF0"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Manual Soaking"));
                Infos.Add(MakeCUIElementInfo(new Guid("D84A404D-636F-493E-BE3F-068552908486"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_TouchSensorSetup"));
                Infos.Add(MakeCUIElementInfo(new Guid("5BD7F6E3-467C-4E28-835F-F99009E666C6"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Account"));
                Infos.Add(MakeCUIElementInfo(new Guid("F624D7F8-8808-4D5C-9420-301DFE77BB8D"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_PMIViewer"));
                Infos.Add(MakeCUIElementInfo(new Guid("A0C0760E-FEA5-4E44-AD73-1275BF0DBB75"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_File_Explorer"));
                Infos.Add(MakeCUIElementInfo(new Guid("0683DC02-3DAF-41A8-9615-A484821AECF7"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_E84"));
                Infos.Add(MakeCUIElementInfo(new Guid("1E096D6A-9AB9-45FD-AE54-CAD50249A0C7"),
                           1,
                           new Guid(magicGUID),
                           false, true,
                           "", "Loader_Menu_Option"));
                Infos.Add(MakeCUIElementInfo(new Guid("48042693-EF01-4C52-922F-8F9DB0210496"),
                           1,
                           new Guid(magicGUID),
                           false, true,
                           "", "Loader_Menu_Option"));
                Infos.Add(MakeCUIElementInfo(new Guid("DBD07301-5CAE-487F-95C1-358751E9424B"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Foup_Recovery"));
                Infos.Add(MakeCUIElementInfo(new Guid("DAE70FCF-E179-474B-BF52-8031064B0328"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_ENV. Monitoring"));
                Infos.Add(MakeCUIElementInfo(new Guid("005F54F1-020F-4341-92FF-4DBC957412BF"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_System_Parameter"));
                Infos.Add(MakeCUIElementInfo(new Guid("793663BD-CCDB-46FD-8F66-123D84C11F99"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Maintenance"));
                Infos.Add(MakeCUIElementInfo(new Guid("2D33A64D-0DD3-46F9-A62F-D6F4558260DD"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_System"));
                Infos.Add(MakeCUIElementInfo(new Guid("39A347A8-F079-4ACB-8C33-515831641AFB"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Networking"));
                Infos.Add(MakeCUIElementInfo(new Guid("58E0678D-5419-40DC-8E12-865334DC02D7"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_ResultData"));
                Infos.Add(MakeCUIElementInfo(new Guid("CE87F52D-24DC-43C6-A167-F3A423DD44F0"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Diagnosis"));
                Infos.Add(MakeCUIElementInfo(new Guid("33EF938F-E44A-448B-990B-626C1159EDB6"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Setup"));
                Infos.Add(MakeCUIElementInfo(new Guid("70C48178-B7C8-4B65-921D-443DF6184709"),
                            1,
                            new Guid(magicGUID),
                            false, true,
                            "", "Loader_Menu_Chiller"));


                #endregion

                #region TopBar-MainMenu

                // (1) OPERATION

                // 1-1 (PAGE SWITCHING)
                Infos.Add(MakeCUIElementInfo(new Guid("c2fc1bc0-47ef-4245-9be3-d3d8b1c1e993"),
                             int.MaxValue,
                             new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"),
                             false, true,
                             "MENU_OPERATION_MANUALPROBE"));

                // 1-2 (PAGE SWITCHING)
                Infos.Add(MakeCUIElementInfo(new Guid("07e8063c-c82e-4205-a48a-a50ebc75d283"),
                             int.MaxValue,
                             new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"),
                             false, true,
                             "MENU_OPERATION_WAFERINSPECTION"));

                // 1-3 (PAGE SWITCHING)
                Infos.Add(MakeCUIElementInfo(new Guid("6aab9edd-a6f5-4af3-b28c-aca2f5e8c912"),
                             int.MaxValue,
                             new Guid("4F42078C-05FE-B4B7-70ED-0602C9DF269B"),
                             false, true,
                             "MENU_OPERATION_WAFERHANDLING"));

                // 1-4 (PAGE SWITCHING)
                Infos.Add(MakeCUIElementInfo(new Guid("A3A63ECA-8554-E143-B29C-785AF67A618E"),
                             int.MaxValue,
                             new Guid("0922A4D5-D811-425D-A68F-6CE5A210102B"),
                             false, true,
                             "MENU_OPERATION_PROBECARD_CHANGE"));

                Infos.Add(MakeCUIElementInfo(new Guid("6AC7461F-C60E-44E6-AC8B-B12BEEE69C75"),
                             -1,
                             new Guid("5C897E13-2D0B-4A12-8F6E-7389E5A75482"),
                             false, true,
                             "MENU_TESTHEADDOCKUNDOCKCHANGE_MENUITEM"));

                Infos.Add(MakeCUIElementInfo(new Guid("48AE13F2-CF6D-4363-9824-8A83E4A6D1AC"),
                             int.MaxValue,
                             new Guid("82816653-AE8A-462E-9F9A-CAE770926A37"),
                             false, true,
                             "MENU_NCPADCHANGE_MENUITEM"));

                // 1-5
                Infos.Add(MakeCUIElementInfo(new Guid("870725f0-45c4-421c-9518-4124e4b46b66"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_OPERATION_CLEANSHEET_CHANGE"));

                // 1-6
                Infos.Add(MakeCUIElementInfo(new Guid("ea92c38f-412f-42e7-95f9-a10d87e0fe86"),
                             int.MaxValue,
                             new Guid("4e9f3ab5-d0b8-47c0-8ce6-cbcb56803e98"),
                             false, true,
                             "MENU_OPERATION_CLEANPAD_CLEANING"));

                // 1-7
                Infos.Add(MakeCUIElementInfo(new Guid("4a314616-85e4-470a-98d2-0794ba46b786"),
                             int.MaxValue,
                             new Guid("D1BED9F8-163C-8D35-170B-A6339D9EF22C"),
                             false, true,
                             "MENU_OPERATION_POLISHWAFER_CLEANING"));

                // 1-8
                Infos.Add(MakeCUIElementInfo(new Guid("c5401dca-cc6e-47cc-ae5b-8e2cb49f0d29"),
                             int.MaxValue,
                             new Guid("51562D6C-D283-85E5-D743-350E2F0C8ABD"),
                             false, true,
                             "MENU_OPERATION_MOTIONJOG"));

                // 1-9
                Infos.Add(MakeCUIElementInfo(new Guid("6e5a46fe-99a0-477c-a4ba-9ee3f6d27cce"),
                             int.MaxValue,
                             new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53"),
                             false, true,
                             "MENU_OPERATION_CASSETTEPORT"));
                // 1-10
                Infos.Add(MakeCUIElementInfo(new Guid("984EA871-A66D-48A9-AA32-5E9E82DEA84B"),
                             int.MaxValue,
                             new Guid("8B2993EA-7358-43CD-91BC-BAD430C0A9F4"),
                             false, true,
                             "MENU_OPERATION_MANUALSOAKING"));

                // (2) RECIPE

                // 2-1
                Infos.Add(MakeCUIElementInfo(new Guid("62768cfc-5554-4cc7-9440-beb88cf110d8"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_RECIPE_CREATE"));

                // 2-2
                Infos.Add(MakeCUIElementInfo(new Guid("299e36db-7e1b-4a8c-8700-c7edcd9bcf90"),
                             int.MaxValue,
                             new Guid("C059599B-BEDF-2137-859B-47C15E433E4D"),
                             false, true,
                             "MENU_RECIPE_SETTING_CHANGE"));

                // 2-3
                Infos.Add(MakeCUIElementInfo(new Guid("11113CA7-3C74-49E1-8F7C-B72019B7AA35"),
                             int.MaxValue,
                             new Guid("956bb44f-4b89-42b3-b21a-69f896a840fe"),
                             false, true,
                             "MENU_RECIPE_DEVICE_CHANGE"));

                // 2-4
                Infos.Add(MakeCUIElementInfo(new Guid("cdff09fe-36de-41c7-b940-45d1ea608444"),
                             int.MaxValue,
                             new Guid("5B755687-AE34-4BBF-8112-71738B9F91D8"),
                             false, true,
                             "MENU_RECIPE_UPLOAD_DOWNLOAD"));


                // 2-5
                Infos.Add(MakeCUIElementInfo(new Guid("150464d0-f0d1-49ac-b619-ed8fafbc1425"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_RECIPE_MANAGEMENT_COPY_REMOVE"));

                // 2-6
                Infos.Add(MakeCUIElementInfo(new Guid("fddd5efa-8b26-4c0c-a98f-311ba91ae67c"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_RECIPE_CHANGE_DEVICE"));

                // 2-7
                Infos.Add(MakeCUIElementInfo(new Guid("baad2700-15f2-4e45-b12d-56e97b90e2f2"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_RECIPE_CREATE_NEW_DEVICE"));

                // 2-8
                Infos.Add(MakeCUIElementInfo(new Guid("0be242de-db1b-4ab0-a8dc-023180f8bfba"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_RECIPE_SAVE_AS"));

                // 2-9
                Infos.Add(MakeCUIElementInfo(new Guid("6bdbd496-3c8a-4888-b3a6-5d0fbdd65dbb"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_RECIPE_DEVICE_DELETE"));

                // 2-10
                Infos.Add(MakeCUIElementInfo(new Guid("864AB820-A21C-42F6-BB3D-D510AD256BA5"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_RECIPE_DEVICE_VIEWER"));

                // (3) USER

                // 3-1
                Infos.Add(MakeCUIElementInfo(new Guid("c50a7c18-e386-4dab-81ad-d656944b61f6"),
                             int.MaxValue,
                             new Guid("28a11f12-8918-47fe-8161-3652f2efef29"),
                             false, true,
                             "MENU_USER_LOGIN_LOGOUT"));

                //// 3-2
                //makedInfo = MakeCUIElementInfo(new Guid("f2ef8427-cd5f-4467-a328-c0d1f6741ef3"),
                //             0,
                //             new Guid(magicGUID),
                //             false, true,
                //             "MENU_USER_ACCOUNT_CREATE");
                //Infos.Add(makedInfo);

                //// 3-3
                //makedInfo = MakeCUIElementInfo(new Guid("d5c07a11-65bd-4739-92c1-40ecac5518b6"),
                //             0,
                //             new Guid(magicGUID),
                //             false, true,
                //             "MENU_USER_ACCOUNT_SETTING");
                //Infos.Add(makedInfo);

                // (4) UTILITY

                // 4-1 : PMI Viewer
                Infos.Add(MakeCUIElementInfo(new Guid("f2bd33d8-2c9d-45b2-a5b0-1fd43ff486a5"),
                             int.MaxValue,
                             new Guid("90cc9901-72d7-451e-94a4-daf3aa6931ea"),
                             false, true,
                             "MENU_UTILITY_PMIVIEWER"));

                // 4-2
                Infos.Add(MakeCUIElementInfo(new Guid("6ad513cd-0991-4f1e-8d5e-1ef9ab89e344"),
                             int.MaxValue,
                             new Guid("e0d23fd0-73a3-4055-a60a-89308b37808b"),
                             false, true,
                             "MENU_UTILITY_SNAPSHOT"));

                // 4-3
                Infos.Add(MakeCUIElementInfo(new Guid("60b0deec-0c8e-4698-828e-041d4c747b50"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_UTILITY_SNAPSHOT"));

                // 4-4
                Infos.Add(MakeCUIElementInfo(new Guid("2d24423e-b34d-4633-9415-e8176f1aabb3"),
                             int.MaxValue,
                             new Guid("E063E8F5-EE65-8FB5-0CA3-F3FBD7B51B78"),
                             false, false,
                             "MENU_UTILITY_TASKMANAGEMENT"));


                //// 4-3
                //makedInfo = MakeCUIElementInfo(new Guid("163BC097-C7DA-47DD-A7BC-7104F3E1702F"),
                //             0,
                //             new Guid("7C4D2394-B6A6-455C-8EE0-92CD86CC4789"),
                //             false, false,
                //             "MENU_UTILITY_THREADLOCKEXPLORER");
                //Infos.Add(makedInfo);

                // 4-4
                //Infos.Add(MakeCUIElementInfo(new Guid("B4CB69A2-5433-49F1-8515-F3A164AC335F"),
                //    -1,
                //    new Guid(magicGUID),
                //    false, true,
                //    "TEST_SETUP_SETTING_PARAM"));

                // (5) SYSTEM

                // 5-1
                Infos.Add(MakeCUIElementInfo(new Guid("a8159a60-25c8-4a84-8b29-c61baf9680df"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_SYSTEM_MACHINE_INITIALIZATION"));

                // 5-2
                Infos.Add(MakeCUIElementInfo(new Guid("9b194aa1-bbe3-481a-b91d-0461ffd889f1"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_SYSTEM_LOADER_INITIALIZATION"));

                // 5-3
                Infos.Add(MakeCUIElementInfo(new Guid("61777098-82f7-4e7c-a9ee-a40e46051367"),
                             int.MaxValue,
                             new Guid("A43A956D-B46E-61B6-5A61-31766111750D"),
                             false, false,
                             "MENU_SYSTEM_SETTING_CHANGE"));

                // 5-4
                Infos.Add(MakeCUIElementInfo(new Guid("039176e9-b73c-463c-aca3-0a5dfb5fc102"),
                             -1,
                             new Guid(magicGUID),
                             false, false,
                             "MENU_CLEAR_PARAMETER_TABLE"));

                // 5-5
                Infos.Add(MakeCUIElementInfo(new Guid("c50a7c18-e386-4dab-81ad-d656944b61f6"),
                             int.MaxValue,
                             new Guid("A43A956D-B46E-61B6-5A61-31766111750D"),
                             false, true,
                             "MENU_LOGOUT"));

                // 5-5
                Infos.Add(MakeCUIElementInfo(new Guid("7ac354a9-ad86-43ae-94cf-00e55e460507"),
                             int.MaxValue,
                             new Guid(magicGUID),
                             false, true,
                             "MENU_SYSTEM_QUIT"));

                #endregion

                #region // RecipeSeutup Dynamic Buttons Info

                ////ElmoSetup
                //makedInfo = MakeCUIElementInfo(new Guid("4a08d377-e45c-404c-8bb9-545718e4f1a6"),
                //                             0,
                //                             new Guid("B1B4E681-ADEA-18EF-0886-EDF8D3510861")
                //                             );
                //Infos.Add(makedInfo);

                ////RecipeEditor
                //makedInfo = MakeCUIElementInfo(new Guid("29d4ce8a-ef01-44a2-9201-8559b16360c0"),
                //                             0,
                //                             new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE")
                //                             );
                //Infos.Add(makedInfo);

                //// WIzard Main
                //makedInfo = MakeCUIElementInfo(new Guid("3DAD60EB-5F3C-E39E-BBA5-8118896F97EC"),
                //                             0,
                //                             new Guid("2DFA38AC-D0C0-6046-7C6C-B6AFEB295828")
                //             );
                //Infos.Add(makedInfo);

                ////Wizard
                //makedInfo = MakeCUIElementInfo(new Guid("30DE939A-D478-10EE-342E-0B51B7E109F1"),
                //                            0,
                //                            new Guid("770BEFDB-6D8F-054C-DFBD-BF3E77591032")
                //                            );
                //Infos.Add(makedInfo);

                ////OCRSetting
                //makedInfo = MakeCUIElementInfo(new Guid("18ab3321-fd07-4aa5-93fb-bb9f5b99c983"),
                //                             0,
                //                             new Guid("5142BD1C-E64B-51F5-29CE-50620BED445A")
                //                             );
                //Infos.Add(makedInfo);

                ////OCRErrorSetting
                //makedInfo = MakeCUIElementInfo(new Guid("a930bb63-1112-474a-8e7c-116b1cbf7cad"),
                //                           0,
                //                           new Guid("b2b83d71-fc2f-40ff-8748-f10dc67ecd5e")
                //                           );
                //Infos.Add(makedInfo);

                //// Vision Test
                //makedInfo = MakeCUIElementInfo(new Guid("5BD3100B-FC12-3F75-15BF-8B66C93D3EDF"),
                //                             0,
                //                             new Guid("367FF140-F606-B65B-7C17-1A743AD06ED6")
                //                             );
                //Infos.Add(makedInfo);

                //// Monitoring
                //makedInfo = MakeCUIElementInfo(new Guid("be19af14-580c-4641-b724-babe2bdb295f"),
                //                             0,
                //                             new Guid("5d9d5473-d933-4151-9d1b-93e001e0d589")
                //                             );
                //Infos.Add(makedInfo);

                //// Path Maker
                //makedInfo = MakeCUIElementInfo(new Guid("812a4f02-10dd-4f52-adf9-df260ae236a1"),
                //                           0,
                //                           new Guid("717bad3b-49a1-46d5-b0ec-dfabee758708")
                //                           );

                //Infos.Add(makedInfo);

                //LoaderSetup
                Infos.Add(MakeCUIElementInfo(new Guid("2D11FE59-7C4D-08C2-A2B9-D0CBC8732E44"),
                                             0,
                                             new Guid("31211D6F-B8A1-16C6-04EA-1656F17C4C54")
                                             ));

                #endregion

                #region Login

                Infos.Add(MakeCUIElementInfo(new Guid("f235990d-35a9-4c90-89bd-5af1946a6e71"),
                                             0,
                                             new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"),
                                             false, true,
                                             "LOGINSCREEN_SUBMIT_BUTTON"));

                #endregion

                #region FoupPage

                Infos.Add(MakeCUIElementInfo(new Guid("b302fd1f-19ba-4ef7-9045-a990a5beee1a"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "FOUP_LOAD", "FOUP_LOAD_DESCRIPTION", ToolTipInfoType.GIF));

                #endregion

                #region FoupRecoveryPage

                Infos.Add(MakeCUIElementInfo(new Guid("1de06aa7-ba15-493b-a6e2-f540d6199170"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "FOUP_RECOVERY_FASTBACKWARD", "FOUP_RECOVERY_FASTBACKWARD_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("e2945b43-bdf4-453a-90ba-2a3ca8a14e8a"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "FOUP_RECOVERY_PREVIOUS", "FOUP_RECOVERY_PREVIOUS_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("917783a8-13a7-4a81-aca6-28c3e4877063"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "FOUP_RECOVERY_REVERSE", "FOUP_RECOVERY_REVERSE_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("067fffd3-c754-42d4-a11e-f0610163d146"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "FOUP_RECOVERY_NEXT", "FOUP_RECOVERY_NEXT_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("61f0e52b-4c26-401a-9c2f-82b1ea1d6c1e"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "FOUP_RECOVERY_FASTFORWARD", "FOUP_RECOVERY_FASTFORWARD_DESCRIPTION", ToolTipInfoType.GIF));

                #endregion

                #region => SettingPage - SubView

                #region ==> Device Setting
                //Create Wafer Map 
                Infos.Add(MakeCUIElementInfo(new Guid("5E1F25BD-DCAD-2F39-CFAA-3062B356AE82"),
                           0,
                           new Guid("5CCAD8EF-1255-F3CE-5118-C245943F1993")
                           ));

                // Wafer 
                Infos.Add(MakeCUIElementInfo(new Guid("a05a34bf-e63f-41ee-9819-285274faef1a"),
                           10,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));
                // Pad 
                Infos.Add(MakeCUIElementInfo(new Guid("21092926-C512-A80F-BFA6-EF25137B51A8"),
                           0,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));

                // Probing Seq
                Infos.Add(MakeCUIElementInfo(new Guid("98bc50c7-7db5-42a9-b9ab-61983a4fc97e"),
                           0,
                           new Guid("EC8FB998-222F-1E88-2C18-6DF6A742B3E9")
                           ));

                // PMI
                Infos.Add(MakeCUIElementInfo(new Guid("103482a3-ad4c-4e9f-88fe-a4df5877924b"),
                           0,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));

                // Mark
                Infos.Add(MakeCUIElementInfo(new Guid("66ae4fca-caf5-42b9-a4ba-bd22d026e65a"),
                           0,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));

                // OCR - Cognex
                Infos.Add(MakeCUIElementInfo(new Guid("bf753e7a-639e-44dc-b50d-037995d4f5d3"),
                           0,
                           new Guid("8aa6dac9-5c54-43e3-8802-0a5dd69e25c8")
                           ));

                // OCR - Semics
                Infos.Add(MakeCUIElementInfo(new Guid("f6873f0c-5bcd-48be-898b-6c03d86d1d1c"),
                           0,
                           new Guid("5142BD1C-E64B-51F5-29CE-50620BED445A")
                           ));

                // Pin
                Infos.Add(MakeCUIElementInfo(new Guid("29cc6805-9418-4fb5-813c-fa1101820b3c"),
                           0,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));

                // Dut Editor
                Infos.Add(MakeCUIElementInfo(new Guid("cb860bcb-0120-438d-8b9c-ab605bf8f95b"),
                           0,
                           new Guid("78744426-ef1d-4624-a961-4a756669a9b7")
                           ));

                // Needle Clean Pad Setup
                Infos.Add(MakeCUIElementInfo(new Guid("8d2cd7a9-caf5-4bb0-89f5-3b6642d26fb6"),
                           0,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));

                // Needle Clean Pad Sequence Setup
                Infos.Add(MakeCUIElementInfo(new Guid("2d37a087-fad5-4c3a-ba72-30759c929a57"),
                           0,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));

                // Needle Clean Pad Setup
                Infos.Add(MakeCUIElementInfo(new Guid("26168D6E-408A-8D81-28BF-4A77F4A3599E"),
                           0,
                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                           ));

                // Manual PMI Page
                Infos.Add(MakeCUIElementInfo(new Guid("DC94686C-5209-4AAE-9ECE-F2627F2D5FB1"),
                           0,
                           new Guid("F1862D58-2C59-4FD3-8439-2C673605E2CE")
                           ));

                // Device UpDown
                Infos.Add(MakeCUIElementInfo(new Guid("939ed729-f25e-42f1-a228-f0a8a0aa3586"),
                           0,
                           new Guid("5B755687-AE34-4BBF-8112-71738B9F91D8")
                           ));

                //Polish Wafer
                Infos.Add(MakeCUIElementInfo(new Guid("2CBCCFE6-9305-1EE7-D2A5-16CB67DDB926"),
                           0,
                           new Guid("398086FA-3531-5841-45C4-A371C317A39C")
                           ));
               

                // GP Card Align
                if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    Infos.Add(MakeCUIElementInfo(new Guid("15c2a43e-a0cd-4c12-b030-845d52e4973d"),
                                                 10,
                                                 new Guid("b094fbf9-35a0-43ab-9311-def5a717a9f7")//<-- View
                                                 ));
                }
                else
                {
                    Infos.Add(MakeCUIElementInfo(new Guid("15c2a43e-a0cd-4c12-b030-845d52e4973d"),
                               10,
                               new Guid("b7104207-1f96-4669-b027-03061794d5a5")//<-- View
                               ));
                }
                #endregion

                #region ==> System Setting

                #region ===> System

                // Alarms
                Infos.Add(MakeCUIElementInfo(new Guid("cdd988da-d8b9-4af3-ad2c-f15d943e15ad"),
                           0,
                           new Guid("65ffcd05-b8f8-4e2b-81e5-25af0409fb75")
                           ));

                // Gem
                Infos.Add(MakeCUIElementInfo(new Guid("BBBB0268-46FE-49F7-81D4-564198965106"),
                           0,
                           new Guid(magicGUID)
                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("5D828F9A-ACC3-4A82-A090-FC4E6A447C58"),
                           0,
                           new Guid(magicGUID)
                           ));

                #endregion


                #region ===> Diagnosis

                // Chuck Tilting
                Infos.Add(MakeCUIElementInfo(new Guid("aba97069-068a-406f-8f7c-0098618401b4"),
                           0,
                           new Guid("209216bf-209e-45ea-b35e-6fdfe2748876")
                           ));

                // Digital Twin
                Infos.Add(MakeCUIElementInfo(new Guid("505694ae-e2cd-4f39-bfdb-448861d8740c"),
                           0,
                           new Guid("f4a0569c-b2a3-4190-9b4c-b4a44f060773")
                           ));

                // Machine Setting - Stage
                Infos.Add(MakeCUIElementInfo(new Guid("11f39b70-571a-4ef1-9b6a-da47b18e3ab7"),
                           0,
                           new Guid("367FF140-F606-B65B-7C17-1A743AD06ED6")
                           ));

                #endregion

                // Machine Setting - Loader
                Infos.Add(MakeCUIElementInfo(new Guid("9ed68f49-c49f-4836-9e07-91dc26e9af77"),
                           0,
                           new Guid("367FF140-F606-B65B-7C17-1A743AD06ED6")
                           ));

                // Vision Mapping
                Infos.Add(MakeCUIElementInfo(new Guid("7ecef13e-99b8-4095-81c9-7fea6daf61f0"),
                           0,
                           new Guid("8412aec0-e1c1-4a4e-aa73-2b0fef470909")
                           ));

                // IO
                Infos.Add(MakeCUIElementInfo(new Guid("2a5f2c63-4390-425e-9faf-998c2a16cf00"),
                           0,
                           new Guid("10CA2446-3785-44C1-AB07-E292036B82EA")
                           ));

                // Chuck Planarity
                Infos.Add(MakeCUIElementInfo(new Guid("a8af6b20-9e34-462c-abbc-0530bc07a232"),
                           0,
                           new Guid("d3b97c85-2bee-4fd8-834f-bb4c3401752a")
                           ));

                // Accuracy Check
                Infos.Add(MakeCUIElementInfo(new Guid("819625fe-f940-49ca-851b-f442d29e64ec"),
                           0,
                           new Guid("7ecc6dde-63e4-4bd8-a4a9-4e982afb831a")
                           ));

                // FOUP
                Infos.Add(MakeCUIElementInfo(new Guid("a902502c-460c-4d0a-bc1f-74ebcaf73302"),
                           0,
                           new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53")
                           ));

                // Masking
                Infos.Add(MakeCUIElementInfo(new Guid("1f83d195-cb33-43d3-891d-fa5ea2a49649"),
                           0,
                           new Guid("cff8b896-1ad0-4c13-b51a-859f352385f4")
                           ));

                // Probing Seq
                Infos.Add(MakeCUIElementInfo(new Guid("d55a3241-c917-4dc5-b902-f83571200b1d"),
                           0,
                           new Guid("31211D6F-B8A1-16C6-04EA-1656F17C4C54")
                           ));

                // TemperatureSetup SetValue Button
                Infos.Add(MakeCUIElementInfo(new Guid("FEF726E0-E3D3-422C-AC37-A292D85A6F7E"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "TEMPERATURESETUP_SETVALUE_BUTTON"));

                // Account AddUserLevel Button
                Infos.Add(MakeCUIElementInfo(new Guid("7849ADB5-FB9E-46F9-962D-0AA1FAA2E4A4"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "ACCOUNT_ADDUSERLEVEL_BUTTON"));

                // Account EnableModify Button
                Infos.Add(MakeCUIElementInfo(new Guid("FEF726E0-E3D3-422C-AC37-A292D85A6F7E"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "ACCOUNT_ENABLEMODIFY_BUTTON"));

                // Account Modify Button
                Infos.Add(MakeCUIElementInfo(new Guid("FA2E59BA-F908-4DF3-879C-D1877D045DF3"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "ACCOUNT_MODIFY_BUTTON"));

                // Account DeleteASelectedAccount Button
                Infos.Add(MakeCUIElementInfo(new Guid("D79DECAD-2AA1-4B8B-B3E9-AD041DBDD547"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "ACCOUNT_DELETE_ASELECTEDACCOUNT_BUTTON"));

                // Account DisableModify Button
                Infos.Add(MakeCUIElementInfo(new Guid("A664CA00-7B61-4D61-AADC-1CF22AB9512F"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "ACCOUNT_DISABLEMODIFY_BUTTON"));

                // GpibSetting ReInitialize Button
                Infos.Add(MakeCUIElementInfo(new Guid("C48A1BEE-FCA7-4151-A3F8-24B5854B3143"),
                             0,
                             new Guid(magicGUID),
                             false, true,
                             "GpibSetting_ReInitialize_BUTTON"));


                //// OCR Setup Button
                //Infos.Add(MakeCUIElementInfo(new Guid("BBD68340-A079-4EBE-BE5F-6F3767E3A828"),
                //             0,
                //             new Guid(magicGUID),
                //             false, true,
                //             "OCR_SETUP_BUTTON"));
                // OCR Setup Button
                Infos.Add(MakeCUIElementInfo(new Guid("BBD68340-A079-4EBE-BE5F-6F3767E3A828"),
                             0,
                             new Guid("8aa6dac9-5c54-43e3-8802-0a5dd69e25c8"),
                             false, true,
                             "OCR_SETUP_BUTTON"));


                #endregion

                #endregion

                #region ==> DisplayPort

                // Displayport Dialog
                Infos.Add(MakeCUIElementInfo(new Guid("8E1A4A49-B8A3-4A3C-96AF-915CFBB0F8AD"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "DISPLAYPORT_DIALOG_DISPLAYPORT"));

                // Extra Camera Dialog
                Infos.Add(MakeCUIElementInfo(new Guid("24EEEAE9-620A-4D6D-9363-06EE35B6B360"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "EXTRA_CAMERA_DIALOG_DISPLAYPORT"));

                // Inspection Control
                Infos.Add(MakeCUIElementInfo(new Guid("63802E31-70BF-4814-9F2F-295F69B876CD"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "INSPECTION_CONTROL_DISPLAYPORT"));

                // Inspection View
                Infos.Add(MakeCUIElementInfo(new Guid("43F955CB-C43C-47D3-9B12-0FEE840640ED"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "INSPECTION_VIEW_DISPLAYPORT"));

                // Camera Display Port
                Infos.Add(MakeCUIElementInfo(new Guid("5A6E07DB-F26E-4E4D-86C2-7BE9AE38E5C9"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "CAMERA_DISPLAYPORT"));

                // LOT's DisplayPort
                Infos.Add(MakeCUIElementInfo(new Guid("C4E51FA2-3384-40CF-9790-3B6F2BA4A817"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "LOT_DISPLAYPORT"));

                // LOT's Loader DisplayPort
                Infos.Add(MakeCUIElementInfo(new Guid("369E0EC1-1A75-404B-825D-15F03B634B6D"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "LOT_LOADER_DISPLAYPORT"));

                // Account Check's DisplayPort
                Infos.Add(MakeCUIElementInfo(new Guid("b582cfa0-2ed6-4a70-b44a-02b4c874fcc5"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "ACCOUNTCHECK_DISPLAYPORT"));

                // MappingView
                Infos.Add(MakeCUIElementInfo(new Guid("A934F8F7-6F50-44C3-B501-9F01B33B8E58"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "MAPPINGVIEW_DISPLAYPORT"));

                // OCR Settingbase
                Infos.Add(MakeCUIElementInfo(new Guid("1A62DBDB-96AD-49E4-B23B-74EFE35A7561"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OCR_SETTINGBASE_DISPLAYPORT"));

                // PMI Manual Result DisplayPort
                Infos.Add(MakeCUIElementInfo(new Guid("B557D2A5-AA68-419A-8B67-14BFC5B6AB75"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "PMI_MANUALRESULT_VIEW_DISPLAYPORT"));

                // PNP Setup DisplayPort
                Infos.Add(MakeCUIElementInfo(new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "PNP_SETUP_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("6A505783-6588-BBAC-5B86-EA4F72C13A9B"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "WAFERMAP_MAKER_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("9ACC1870-87E2-4A6E-9F3B-5CF8D55C09D0"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "StageSetting_DISPLAYPORT"));
                #endregion

                #region ==> OperatorView

                // OPERATOR CONTINUOUS LOT CB
                Infos.Add(MakeCUIElementInfo(new Guid("293BDA62-28D5-4468-B02D-667EF60D898C"),
                    -1,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_CONTINUOUS_LOT_CB"));

                // OPERATOR STOP AFTER LOAD CST CB
                Infos.Add(MakeCUIElementInfo(new Guid("244E364B-9CF8-407E-BD85-13C34AB9EF20"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_STOP_AFTER_LOAD_CST_CB"));

                // OPERATOR STOP AFTER WAFER LOAD CB
                Infos.Add(MakeCUIElementInfo(new Guid("F0C7CE52-FB6F-473B-B9D9-7D587A87861E"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_STOP_AFTER_WAFER_LOAD_CB"));

                // OPERATOR EVERY STOP BEFORE PROBE CB
                Infos.Add(MakeCUIElementInfo(new Guid("55DA3EE4-3FE3-41B6-94CA-2C01C0D34D18"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_EVERY_STOP_BEFORE_PROBE_CB"));

                // OPERATOR EVERY STOP AFTER PROBE CB
                Infos.Add(MakeCUIElementInfo(new Guid("86205474-2993-423D-9664-EFEE68702D7D"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_EVERY_STOP_AFTER_PROBE_CB"));

                // OPERATOR STOP BEFORE PROBE CB
                Infos.Add(MakeCUIElementInfo(new Guid("15A50273-0224-487C-91F1-3CC48CA243DE"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_STOP_BEFORE_PROBE_CB"));

                // OPERATOR STOP AFTER PROBE CB
                Infos.Add(MakeCUIElementInfo(new Guid("07093718-7CEB-43A7-B9CE-865FF7D76E00"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_STOP_AFTER_PROBE_CB"));

                // OPERATOR STOP BEFORE PROBE BTN
                Infos.Add(MakeCUIElementInfo(new Guid("2C64274F-AB85-4410-8942-A00FFF86C92E"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_STOP_BEFORE_PROBE_BTN"));

                // OPERATOR STOP AFTER PROBE BTN
                Infos.Add(MakeCUIElementInfo(new Guid("8B99441C-C4FA-4B16-B2A3-3618806F44BD"),
                    0,
                    new Guid(magicGUID),
                    false, true,
                    "OPERATOR_STOP_AFTER_PROBE_BTN"));
                #endregion

                #region LightJog
                Infos.Add(MakeCUIElementInfo(new Guid("C399DE62-C35E-8CA5-67B2-926140E66910"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true, "LIGHT_JOG"));
                #endregion

                #region MotionJog
                Infos.Add(MakeCUIElementInfo(new Guid("968F1EDF-CA30-FDF3-7861-D481E0CAC0D6"),
                          0,
                          new Guid(magicGUID),
                          false, true, "MOTION_JOG"));
                #endregion

                #region PNP 

                //NaviControl
                Infos.Add(MakeCUIElementInfo(new Guid("734F1145-C26B-AE62-0495-25F36C71FF82"),
                          0,
                          new Guid(magicGUID),
                          false, true, "NAVI_CONTROL"));

                //PnpButtonJog
                Infos.Add(MakeCUIElementInfo(new Guid("3455C399-9C8F-E9BC-0394-58FDFB04CDB1"),
                          0,
                          new Guid(magicGUID),
                          false, true, "PNP_BUTTON"));

                //PnpButtonJog
                Infos.Add(MakeCUIElementInfo(new Guid("F4910448-97F9-EB9C-5701-6BEFE25AF372"),
                          0,
                          new Guid(magicGUID),
                          false, true, "PNP_PRE_NEXT_BUTTON"));

                //PnpButtonJog
                Infos.Add(MakeCUIElementInfo(new Guid("EB991C5E-610B-79A1-5BC9-87B05ABDBA2A"),
                          0,
                          new Guid(magicGUID),
                          false, true, "PNP_CIRCULAR_BUTTON"));


                #endregion

                #region WaferMapViwer
                Infos.Add(MakeCUIElementInfo(new Guid("D1F3E640-2AD0-8085-F5D1-512D41DBAE71"),
                          0,
                          new Guid(magicGUID),
                          false, true, "MAP_VIEWER"));
                #endregion

                #region DutViwer

                #endregion

                #region Inspection
                // SET FROM BTN
                Infos.Add(MakeCUIElementInfo(new Guid("439A82B1-158C-4D69-B17D-AE30B5398F1C"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_SET_FROM_BTN"));
                // APPLY BTN
                Infos.Add(MakeCUIElementInfo(new Guid("51F0E013-3165-4FD5-845E-2A6255EDE74F"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_APPLY_BTN"));
                // CLEAR BTN
                Infos.Add(MakeCUIElementInfo(new Guid("0D05470F-C71C-4913-8305-3F07A8B92769"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_CLEAR_BTN"));
                // PREVIOUS DUT BTN
                Infos.Add(MakeCUIElementInfo(new Guid("84A5F5D4-2148-4475-B619-F812ACE31F2D"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_PRE_DUT_BTN"));
                // NEXT DUT BTN
                Infos.Add(MakeCUIElementInfo(new Guid("AD6CCB4A-D499-48ED-ADF5-E434F2332F97"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_NEXT_DUT_BTN"));
                // PREVIOUS PAD BTN
                Infos.Add(MakeCUIElementInfo(new Guid("FF7970B8-8FEB-47F6-BA0F-D8C6FA29FEAC"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_PRE_PAD_BTN"));
                // NEXT PAD BTN
                Infos.Add(MakeCUIElementInfo(new Guid("B5BC9FA9-FAB2-4158-A9C4-8FFD636DA793"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_NEXT_PAD_BTN"));
                //// SET MANUAL INDEX X TEXT
                //Infos.Add(MakeCUIElementInfo(new Guid("964A7014-193A-47C4-A2FA-1737A73E19BC"),
                //                             0,
                //                             new Guid(magicGUID),
                //                             false, true,
                //                             "INSPECTION_SET_MANUAL_INDEX_X_TEXT"));
                //// SET MANUAL INDEX Y TEXT
                //Infos.Add(MakeCUIElementInfo(new Guid("6211901D-0E1A-4355-A2FA-3380FF180083"),
                //                             0,
                //                             new Guid(magicGUID),
                //                             false, true,
                //                             "INSPECTION_SET_MANUAL_INDEX_Y_TEXT"));
                // SET INDEX BTN
                Infos.Add(MakeCUIElementInfo(new Guid("4CE55F55-A38A-4937-B43C-FAA1CCD506E5"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_SET_INDEX_BTN"));
                // SET MANUAL / AUTO INDEX TOGGLE BTN
                Infos.Add(MakeCUIElementInfo(new Guid("D9903B07-4317-4B5B-ABD6-B1FC9EB02C8E"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_MANUAL_AUTO_INDEX_TOGGLE_BTN"));
                // DUT / PIN CAM VIEW TOGGLE BTN
                Infos.Add(MakeCUIElementInfo(new Guid("18114BE3-A2D8-48E9-93B6-317C66CD12BF"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "INSPECTION_DUT_PIN_CAM_TOGGLE_BTN"));
                #endregion

                #region PolishWafer
                //Step - Centering/Focusing
                Infos.Add(MakeCUIElementInfo(new Guid("75C98D02-9854-7723-C912-1D89EE60A24F"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_STEP_CENTERING_FOCUSING"));
                //Step - Cleaning
                Infos.Add(MakeCUIElementInfo(new Guid("71C18D46-06E8-06DA-E367-54FFA17E9DBF"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_STEP_CLEANING"));
                //Step - Wafer
                Infos.Add(MakeCUIElementInfo(new Guid("2FDAAE5A-3D48-867C-B561-FDCE592AE346"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_STEP_WAFER_SETTING"));
                //Step - Manual
                Infos.Add(MakeCUIElementInfo(new Guid("3CB80428-23B8-922B-FBDA-F9F3B04DF9B0"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_STEP_MANUAL_SETTING"));
                //Load
                Infos.Add(MakeCUIElementInfo(new Guid("C999E5B3-CC43-8380-25F6-2514B55BE8B6"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_LOAD"));
                //Unload
                Infos.Add(MakeCUIElementInfo(new Guid("A2D99045-F7AD-B7B2-70CE-26EF888074DC"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_UNLOAD"));
                //Focusing
                Infos.Add(MakeCUIElementInfo(new Guid("5C678C55-4DD2-56FC-ADAF-F9523121DD4D"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_FOCUSING"));
                //Cleaning
                Infos.Add(MakeCUIElementInfo(new Guid("E231DDD4-DF2C-24D1-C9FC-E73CFB6BA7C3"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true,
                                          "POLISH_WAFER_CLEANING"));
                #endregion

                //Cleaning
                Infos.Add(MakeCUIElementInfo(new Guid("764051a7-75fe-48a9-b685-d335e0f978d8"),
                                          0,
                                          new Guid("4a828af3-fd28-4d05-974b-3e526f781454"),
                                          false, true,
                                          "POLISH_WAFER_SOURCE_SETUP"));

                // Semics OCR Setup Button
                Infos.Add(MakeCUIElementInfo(new Guid("603ede06-438b-415f-97f1-bbbcc21db1b0"),
                                          0,
                                          new Guid("5142BD1C-E64B-51F5-29CE-50620BED445A"),
                                          false, true,
                                          "SEMICS_OCR_SETUP"));


                #region Status Soaking
                Infos.Add(MakeCUIElementInfo(new Guid("1C8A8C0B-67E2-4D55-BD37-9C54A7179F47"),
                          0,
                          new Guid(magicGUID),
                          false, true,
                          "STATUS_SOAKING_SETUP"));
                #endregion
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"[CUIElementCollection - SetDefaultParam()] [Error = {err}]");
            }

            return RetVal;
        }

        private EventCodeEnum MakeDefaultParamSetFor_BSCI()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                lcid = 1033;

                if (Infos == null)
                {
                    Infos = new List<CUIElementInfo>();
                }

                Infos.Add(MakeCUIElementInfo(new Guid("1b5ef581-fac0-4bea-a73b-3b4dc88c05ac"),
                                              0,
                                              new Guid("0922A4D5-D811-425D-A68F-6CE5A210102B"),
                                              false, true,
                                              ""));

                Infos.Add(MakeCUIElementInfo(new Guid("31ad3cc0-e934-477e-a51c-18bc2df52ce6"),
                                                            0,
                                                            new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"),
                                                            false, true,
                                                            ""));

                Infos.Add(MakeCUIElementInfo(new Guid("f88475b7-c1bb-4c4a-996c-64617f506e78"),
                                                             0,
                                                             new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"),
                                                             false, true,
                                                             ""));

                Infos.Add(MakeCUIElementInfo(new Guid("c43edfed-f815-4b29-ab04-d68f110a56db"),
                                                             0,
                                                             new Guid("5B755687-AE34-4BBF-8112-71738B9F91D8"),
                                                             false, true,
                                                             ""));

                Infos.Add(MakeCUIElementInfo(new Guid("ba1fc6ab-088c-4a2d-a51a-ccc9123ba079"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOT_START", "LOT_START_DESCRIPTION", ToolTipInfoType.SIMPLE));

                Infos.Add(MakeCUIElementInfo(new Guid("d293677c-09e4-45df-9e78-a5dba2daa1ae"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOTSCREEN_LOTEND_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("ff73b7f2-3f38-4375-8b1d-137d62d89336"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOT_SETUP", "LOT_SETUP_DESCRIPTION", ToolTipInfoType.IMAGE));

                Infos.Add(MakeCUIElementInfo(new Guid("fcc03d05-202d-41c0-8f0a-4ee63961a4b2"),
                                                             0,
                                                             new Guid("b0722a2f-bc56-4e74-88e7-63fbf4ec7d63"),
                                                             false, false,
                                                             "LOT_INFO_YIELD", "LOT_INFO_YIELD_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("c820c484-7b89-4cd0-b677-2d506f08ae7d"),
                                                             0,
                                                             new Guid("7c94444f-d655-407b-9a7e-0b938505eb99"),
                                                             false, false,
                                                             "LOT_INFO_RECIPE", "LOT_INFO_YIELD_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("c3aa3a94-8179-466a-a9e3-36f16f4e03fb"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "OCR_MANUAL_INPUT", "OCR_MANUAL_INPUT_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("1057874e-b364-4ac1-9845-77ae011bc512"),
                                                             0,
                                                             new Guid("436db5ea-e49e-4fca-a1a8-e3be24639e31"),
                                                             false, false,
                                                             "LOT_INFO_DEVICE_SETTING", "LOT_INFO_YIELD_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("b1aa95e2-8943-412c-8b46-e754db6cba4d"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "LOT_FAST_REWIND", "LOT_FAST_REWIND_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("59a307d1-8e6b-442e-8bc8-68ceeb93b66c"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOT_NORMAL_REWIND", "LOT_NORMAL_REWIND_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("dfe6543f-f852-4c43-ba92-1902af1412cb"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOT_NORMAL_FORWARD", "LOT_NORMAL_FORWARD_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("761d35d9-780d-440e-8661-188cc3799525"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOT_FAST_FORWARD", "LOT_FAST_FORWARD_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("a0744ecd-c106-40eb-abda-e6bc52a40516"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOT_RESTAT", "LOT_RESTART_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("59217259-A2D7-4F9E-AD58-182FA46F4EB5"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, false,
                                                             "LOT_APP_SECTOR", "LOT_APP_SECTOR_DESCRIPTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("3776199a-0c2c-49c1-8f6c-5c2a5a9fa873"),
                                                            999,
                                                             new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"),
                                                             false, true,
                                                             "LOTSCREEN_MAINSCREEN"));

                Infos.Add(MakeCUIElementInfo(new Guid("5e80ff23-fbc8-4a56-9c31-a5d060397807"),
                                                             999,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "BACK_SCREEN"));

                Infos.Add(MakeCUIElementInfo(new Guid("5e80ff23-fbc8-4a56-9c31-a5d060397802"),
                                                             5,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "BELL_PROLOG_VIEW"));

                Infos.Add(MakeCUIElementInfo(new Guid("5e80ff23-fbc8-4a56-9c31-a5d060397803"),
                                                             999,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "PROLOG_VIEW"));

                Infos.Add(MakeCUIElementInfo(new Guid("c2fc1bc0-47ef-4245-9be3-d3d8b1c1e993"),
                                             int.MaxValue,
                                             new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"),
                                             false, true,
                                             "MENU_OPERATION_MANUALPROBE"));

                Infos.Add(MakeCUIElementInfo(new Guid("07e8063c-c82e-4205-a48a-a50ebc75d283"),
                                             int.MaxValue,
                                             new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"),
                                             false, true,
                                             "MENU_OPERATION_WAFERINSPECTION"));

                Infos.Add(MakeCUIElementInfo(new Guid("6aab9edd-a6f5-4af3-b28c-aca2f5e8c912"),
                                             int.MaxValue,
                                             new Guid("4F42078C-05FE-B4B7-70ED-0602C9DF269B"),
                                             false, true,
                                             "MENU_OPERATION_WAFERHANDLING"));

                Infos.Add(MakeCUIElementInfo(new Guid("A3A63ECA-8554-E143-B29C-785AF67A618E"),
                                             int.MaxValue,
                                             new Guid("0922A4D5-D811-425D-A68F-6CE5A210102B"),
                                             false, true,
                                             "MENU_OPERATION_PROBECARD_CHANGE"));

                Infos.Add(MakeCUIElementInfo(new Guid("6AC7461F-C60E-44E6-AC8B-B12BEEE69C75"),
                                             -1,
                                             new Guid("5C897E13-2D0B-4A12-8F6E-7389E5A75482"),
                                             false, true,
                                             "MENU_TESTHEADDOCKUNDOCKCHANGE_MENUITEM"));

                Infos.Add(MakeCUIElementInfo(new Guid("48AE13F2-CF6D-4363-9824-8A83E4A6D1AC"),
                                             int.MaxValue,
                                             new Guid("82816653-AE8A-462E-9F9A-CAE770926A37"),
                                             false, true,
                                             "MENU_NCPADCHANGE_MENUITEM"));

                Infos.Add(MakeCUIElementInfo(new Guid("870725f0-45c4-421c-9518-4124e4b46b66"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_OPERATION_CLEANSHEET_CHANGE"));

                Infos.Add(MakeCUIElementInfo(new Guid("ea92c38f-412f-42e7-95f9-a10d87e0fe86"),
                                             int.MaxValue,
                                             new Guid("4e9f3ab5-d0b8-47c0-8ce6-cbcb56803e98"),
                                             false, true,
                                             "MENU_OPERATION_CLEANPAD_CLEANING"));

                Infos.Add(MakeCUIElementInfo(new Guid("4a314616-85e4-470a-98d2-0794ba46b786"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_OPERATION_POLISHWAFER_CLEANING"));

                Infos.Add(MakeCUIElementInfo(new Guid("f2bd33d8-2c9d-45b2-a5b0-1fd43ff486a5"),
                                             int.MaxValue,
                                             new Guid("90cc9901-72d7-451e-94a4-daf3aa6931ea"),
                                             false, true,
                                             "MENU_OPERATION_PMIVIEWER"));

                Infos.Add(MakeCUIElementInfo(new Guid("c5401dca-cc6e-47cc-ae5b-8e2cb49f0d29"),
                                             int.MaxValue,
                                             new Guid("51562D6C-D283-85E5-D743-350E2F0C8ABD"),
                                             false, true,
                                             "MENU_OPERATION_MOTIONJOG"));

                Infos.Add(MakeCUIElementInfo(new Guid("6e5a46fe-99a0-477c-a4ba-9ee3f6d27cce"),
                                             int.MaxValue,
                                             new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53"),
                                             false, true,
                                             "MENU_OPERATION_CASSETTEPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("62768cfc-5554-4cc7-9440-beb88cf110d8"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_RECIPE_CREATE"));

                Infos.Add(MakeCUIElementInfo(new Guid("299e36db-7e1b-4a8c-8700-c7edcd9bcf90"),
                                             int.MaxValue,
                                             new Guid("C059599B-BEDF-2137-859B-47C15E433E4D"),
                                             false, true,
                                             "MENU_RECIPE_SETTING_CHANGE"));

                Infos.Add(MakeCUIElementInfo(new Guid("11113CA7-3C74-49E1-8F7C-B72019B7AA35"),
                                             int.MaxValue,
                                             new Guid("956bb44f-4b89-42b3-b21a-69f896a840fe"),
                                             false, true,
                                             "MENU_RECIPE_DEVICE_CHANGE"));

                Infos.Add(MakeCUIElementInfo(new Guid("cdff09fe-36de-41c7-b940-45d1ea608444"),
                                             int.MaxValue,
                                             new Guid("5B755687-AE34-4BBF-8112-71738B9F91D8"),
                                             false, true,
                                             "MENU_RECIPE_UPLOAD_DOWNLOAD"));

                Infos.Add(MakeCUIElementInfo(new Guid("150464d0-f0d1-49ac-b619-ed8fafbc1425"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_RECIPE_MANAGEMENT_COPY_REMOVE"));

                Infos.Add(MakeCUIElementInfo(new Guid("c50a7c18-e386-4dab-81ad-d656944b61f6"),
                                             int.MaxValue,
                                             new Guid("28a11f12-8918-47fe-8161-3652f2efef29"),
                                             false, true,
                                             "MENU_USER_LOGIN_LOGOUT"));

                Infos.Add(MakeCUIElementInfo(new Guid("60b0deec-0c8e-4698-828e-041d4c747b50"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_UTILITY_SNAPSHOT"));

                Infos.Add(MakeCUIElementInfo(new Guid("2d24423e-b34d-4633-9415-e8176f1aabb3"),
                                             int.MaxValue,
                                             new Guid("E063E8F5-EE65-8FB5-0CA3-F3FBD7B51B78"),
                                             false, false,
                                             "MENU_UTILITY_TASKMANAGEMENT"));

                Infos.Add(MakeCUIElementInfo(new Guid("a8159a60-25c8-4a84-8b29-c61baf9680df"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_SYSTEM_MACHINE_INITIALIZATION"));

                Infos.Add(MakeCUIElementInfo(new Guid("9b194aa1-bbe3-481a-b91d-0461ffd889f1"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_SYSTEM_LOADER_INITIALIZATION"));

                Infos.Add(MakeCUIElementInfo(new Guid("61777098-82f7-4e7c-a9ee-a40e46051367"),
                                             int.MaxValue,
                                             new Guid("A43A956D-B46E-61B6-5A61-31766111750D"),
                                             false, false,
                                             "MENU_SYSTEM_SETTING_CHANGE"));

                Infos.Add(MakeCUIElementInfo(new Guid("039176e9-b73c-463c-aca3-0a5dfb5fc102"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, false,
                                             "MENU_CLEAR_PARAMETER_TABLE"));

                Infos.Add(MakeCUIElementInfo(new Guid("c50a7c18-e386-4dab-81ad-d656944b61f6"),
                                             int.MaxValue,
                                             new Guid("A43A956D-B46E-61B6-5A61-31766111750D"),
                                             false, true,
                                             "MENU_LOGOUT"));

                Infos.Add(MakeCUIElementInfo(new Guid("7ac354a9-ad86-43ae-94cf-00e55e460507"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_SYSTEM_QUIT"));

                Infos.Add(MakeCUIElementInfo(new Guid("2D11FE59-7C4D-08C2-A2B9-D0CBC8732E44"),
                                                             0,
                                                             new Guid("31211D6F-B8A1-16C6-04EA-1656F17C4C54")
                                                             ));

                Infos.Add(MakeCUIElementInfo(new Guid("f235990d-35a9-4c90-89bd-5af1946a6e71"),
                                                             0,
                                                             new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"),
                                                             false, true,
                                                             "LOGINSCREEN_SUBMIT_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("b302fd1f-19ba-4ef7-9045-a990a5beee1a"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "FOUP_LOAD", "FOUP_LOAD_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("1de06aa7-ba15-493b-a6e2-f540d6199170"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "FOUP_RECOVERY_FASTBACKWARD", "FOUP_RECOVERY_FASTBACKWARD_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("e2945b43-bdf4-453a-90ba-2a3ca8a14e8a"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "FOUP_RECOVERY_PREVIOUS", "FOUP_RECOVERY_PREVIOUS_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("917783a8-13a7-4a81-aca6-28c3e4877063"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "FOUP_RECOVERY_REVERSE", "FOUP_RECOVERY_REVERSE_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("067fffd3-c754-42d4-a11e-f0610163d146"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "FOUP_RECOVERY_NEXT", "FOUP_RECOVERY_NEXT_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("61f0e52b-4c26-401a-9c2f-82b1ea1d6c1e"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "FOUP_RECOVERY_FASTFORWARD", "FOUP_RECOVERY_FASTFORWARD_DESCRIPTION", ToolTipInfoType.GIF));

                Infos.Add(MakeCUIElementInfo(new Guid("5E1F25BD-DCAD-2F39-CFAA-3062B356AE82"),
                                           0,
                                           new Guid("5CCAD8EF-1255-F3CE-5118-C245943F1993")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("a05a34bf-e63f-41ee-9819-285274faef1a"),
                                           10,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("21092926-C512-A80F-BFA6-EF25137B51A8"),
                                           0,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("98bc50c7-7db5-42a9-b9ab-61983a4fc97e"),
                                           0,
                                           new Guid("EC8FB998-222F-1E88-2C18-6DF6A742B3E9")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("103482a3-ad4c-4e9f-88fe-a4df5877924b"),
                                           0,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("66ae4fca-caf5-42b9-a4ba-bd22d026e65a"),
                                           0,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("bf753e7a-639e-44dc-b50d-037995d4f5d3"),
                                           0,
                                           new Guid("8aa6dac9-5c54-43e3-8802-0a5dd69e25c8")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("29cc6805-9418-4fb5-813c-fa1101820b3c"),
                                           0,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("cb860bcb-0120-438d-8b9c-ab605bf8f95b"),
                                           0,
                                           new Guid("78744426-ef1d-4624-a961-4a756669a9b7")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("8d2cd7a9-caf5-4bb0-89f5-3b6642d26fb6"),
                                           0,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("2d37a087-fad5-4c3a-ba72-30759c929a57"),
                                           0,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("26168D6E-408A-8D81-28BF-4A77F4A3599E"),
                                           0,
                                           new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("DC94686C-5209-4AAE-9ECE-F2627F2D5FB1"),
                                           0,
                                           new Guid("F1862D58-2C59-4FD3-8439-2C673605E2CE")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("939ed729-f25e-42f1-a228-f0a8a0aa3586"),
                                           0,
                                           new Guid("5B755687-AE34-4BBF-8112-71738B9F91D8")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("cdd988da-d8b9-4af3-ad2c-f15d943e15ad"),
                                           0,
                                           new Guid("65ffcd05-b8f8-4e2b-81e5-25af0409fb75")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("aba97069-068a-406f-8f7c-0098618401b4"),
                                           0,
                                           new Guid("209216bf-209e-45ea-b35e-6fdfe2748876")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("11f39b70-571a-4ef1-9b6a-da47b18e3ab7"),
                                           0,
                                           new Guid("367FF140-F606-B65B-7C17-1A743AD06ED6")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("9ed68f49-c49f-4836-9e07-91dc26e9af77"),
                                           0,
                                           new Guid("367FF140-F606-B65B-7C17-1A743AD06ED6")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("7ecef13e-99b8-4095-81c9-7fea6daf61f0"),
                                           0,
                                           new Guid("8412aec0-e1c1-4a4e-aa73-2b0fef470909")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("2a5f2c63-4390-425e-9faf-998c2a16cf00"),
                                           0,
                                           new Guid("10CA2446-3785-44C1-AB07-E292036B82EA")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("a902502c-460c-4d0a-bc1f-74ebcaf73302"),
                                           0,
                                           new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("1f83d195-cb33-43d3-891d-fa5ea2a49649"),
                                           0,
                                           new Guid("cff8b896-1ad0-4c13-b51a-859f352385f4")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("d55a3241-c917-4dc5-b902-f83571200b1d"),
                                           0,
                                           new Guid("31211D6F-B8A1-16C6-04EA-1656F17C4C54")
                                           ));

                Infos.Add(MakeCUIElementInfo(new Guid("FEF726E0-E3D3-422C-AC37-A292D85A6F7E"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "TEMPERATURESETUP_SETVALUE_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("7849ADB5-FB9E-46F9-962D-0AA1FAA2E4A4"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "ACCOUNT_ADDUSERLEVEL_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("FEF726E0-E3D3-422C-AC37-A292D85A6F7E"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "ACCOUNT_ENABLEMODIFY_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("FA2E59BA-F908-4DF3-879C-D1877D045DF3"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "ACCOUNT_MODIFY_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("D79DECAD-2AA1-4B8B-B3E9-AD041DBDD547"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "ACCOUNT_DELETE_ASELECTEDACCOUNT_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("A664CA00-7B61-4D61-AADC-1CF22AB9512F"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "ACCOUNT_DISABLEMODIFY_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("C48A1BEE-FCA7-4151-A3F8-24B5854B3143"),
                                             0,
                                             new Guid(magicGUID),
                                             false, true,
                                             "GpibSetting_ReInitialize_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("BBD68340-A079-4EBE-BE5F-6F3767E3A828"),
                                             0,
                                             new Guid("8aa6dac9-5c54-43e3-8802-0a5dd69e25c8"),
                                             false, true,
                                             "OCR_SETUP_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("8E1A4A49-B8A3-4A3C-96AF-915CFBB0F8AD"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "DISPLAYPORT_DIALOG_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("24EEEAE9-620A-4D6D-9363-06EE35B6B360"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "EXTRA_CAMERA_DIALOG_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("63802E31-70BF-4814-9F2F-295F69B876CD"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "INSPECTION_CONTROL_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("43F955CB-C43C-47D3-9B12-0FEE840640ED"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "INSPECTION_VIEW_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("5A6E07DB-F26E-4E4D-86C2-7BE9AE38E5C9"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "CAMERA_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("C4E51FA2-3384-40CF-9790-3B6F2BA4A817"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "LOT_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("369E0EC1-1A75-404B-825D-15F03B634B6D"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "LOT_LOADER_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("A934F8F7-6F50-44C3-B501-9F01B33B8E58"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "MAPPINGVIEW_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("1A62DBDB-96AD-49E4-B23B-74EFE35A7561"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OCR_SETTINGBASE_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("B557D2A5-AA68-419A-8B67-14BFC5B6AB75"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "PMI_MANUALRESULT_VIEW_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "PNP_SETUP_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("6A505783-6588-BBAC-5B86-EA4F72C13A9B"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "WAFERMAP_MAKER_DISPLAYPORT"));

                Infos.Add(MakeCUIElementInfo(new Guid("293BDA62-28D5-4468-B02D-667EF60D898C"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_CONTINUOUS_LOT_CB"));

                Infos.Add(MakeCUIElementInfo(new Guid("244E364B-9CF8-407E-BD85-13C34AB9EF20"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_STOP_AFTER_LOAD_CST_CB"));

                Infos.Add(MakeCUIElementInfo(new Guid("F0C7CE52-FB6F-473B-B9D9-7D587A87861E"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_STOP_AFTER_WAFER_LOAD_CB"));

                Infos.Add(MakeCUIElementInfo(new Guid("55DA3EE4-3FE3-41B6-94CA-2C01C0D34D18"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_EVERY_STOP_BEFORE_PROBE_CB"));

                Infos.Add(MakeCUIElementInfo(new Guid("86205474-2993-423D-9664-EFEE68702D7D"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_EVERY_STOP_AFTER_PROBE_CB"));

                Infos.Add(MakeCUIElementInfo(new Guid("15A50273-0224-487C-91F1-3CC48CA243DE"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_STOP_BEFORE_PROBE_CB"));

                Infos.Add(MakeCUIElementInfo(new Guid("07093718-7CEB-43A7-B9CE-865FF7D76E00"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_STOP_AFTER_PROBE_CB"));

                Infos.Add(MakeCUIElementInfo(new Guid("2C64274F-AB85-4410-8942-A00FFF86C92E"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_STOP_BEFORE_PROBE_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("8B99441C-C4FA-4B16-B2A3-3618806F44BD"),
                                    0,
                                    new Guid(magicGUID),
                                    false, true,
                                    "OPERATOR_STOP_AFTER_PROBE_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("C399DE62-C35E-8CA5-67B2-926140E66910"),
                                                          0,
                                                          new Guid(magicGUID),
                                                          false, true, "LIGHT_JOG"));

                Infos.Add(MakeCUIElementInfo(new Guid("968F1EDF-CA30-FDF3-7861-D481E0CAC0D6"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true, "MOTION_JOG"));

                Infos.Add(MakeCUIElementInfo(new Guid("734F1145-C26B-AE62-0495-25F36C71FF82"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true, "NAVI_CONTROL"));

                Infos.Add(MakeCUIElementInfo(new Guid("3455C399-9C8F-E9BC-0394-58FDFB04CDB1"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true, "PNP_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("F4910448-97F9-EB9C-5701-6BEFE25AF372"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true, "PNP_PRE_NEXT_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("EB991C5E-610B-79A1-5BC9-87B05ABDBA2A"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true, "PNP_CIRCULAR_BUTTON"));

                Infos.Add(MakeCUIElementInfo(new Guid("D1F3E640-2AD0-8085-F5D1-512D41DBAE71"),
                                          0,
                                          new Guid(magicGUID),
                                          false, true, "MAP_VIEWER"));

                Infos.Add(MakeCUIElementInfo(new Guid("439A82B1-158C-4D69-B17D-AE30B5398F1C"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_SET_FROM_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("51F0E013-3165-4FD5-845E-2A6255EDE74F"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_APPLY_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("0D05470F-C71C-4913-8305-3F07A8B92769"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_CLEAR_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("84A5F5D4-2148-4475-B619-F812ACE31F2D"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_PRE_DUT_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("AD6CCB4A-D499-48ED-ADF5-E434F2332F97"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_NEXT_DUT_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("FF7970B8-8FEB-47F6-BA0F-D8C6FA29FEAC"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_PRE_PAD_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("B5BC9FA9-FAB2-4158-A9C4-8FFD636DA793"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_NEXT_PAD_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("4CE55F55-A38A-4937-B43C-FAA1CCD506E5"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_SET_INDEX_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("D9903B07-4317-4B5B-ABD6-B1FC9EB02C8E"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_MANUAL_AUTO_INDEX_TOGGLE_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("18114BE3-A2D8-48E9-93B6-317C66CD12BF"),
                                                             0,
                                                             new Guid(magicGUID),
                                                             false, true,
                                                             "INSPECTION_DUT_PIN_CAM_TOGGLE_BTN"));

                Infos.Add(MakeCUIElementInfo(new Guid("864AB820-A21C-42F6-BB3D-D510AD256BA5"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_RECIPE_DEVICE_VIEWER"));

                Infos.Add(MakeCUIElementInfo(new Guid("fddd5efa-8b26-4c0c-a98f-311ba91ae67c"),
                                             int.MaxValue,
                                             new Guid(magicGUID),
                                             false, true,
                                             "MENU_RECIPE_CHANGE_DEVICE"));

                Infos.Add(MakeCUIElementInfo(new Guid("a8af6b20-9e34-462c-abbc-0530bc07a232"),
                                           0,
                                           new Guid("d3b97c85-2bee-4fd8-834f-bb4c3401752a")
                                           ));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retval;
        }
    }
}
