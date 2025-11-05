using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Windows;

namespace ViewModelModule
{
    [Serializable]
    public class SettingViewParam : IParam, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[SettingViewParam] [Method = Init] [Error = {err}]");
                    retval = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string FilePath { get; } = "";

        public string FileName { get; } = "SettingViewParam.json";

        private List<SettingCategoryInfo> _SystemSettingCategoryInfos = new List<SettingCategoryInfo>();
        public List<SettingCategoryInfo> SystemSettingCategoryInfos
        {
            get { return _SystemSettingCategoryInfos; }
            set
            {
                if (value != _SystemSettingCategoryInfos)
                {
                    _SystemSettingCategoryInfos = value;
                }
            }
        }

        private List<SettingCategoryInfo> _DeviceSettingCategoryInfos = new List<SettingCategoryInfo>();
        public List<SettingCategoryInfo> DeviceSettingCategoryInfos
        {
            get { return _DeviceSettingCategoryInfos; }
            set
            {
                if (value != _DeviceSettingCategoryInfos)
                {
                    _DeviceSettingCategoryInfos = value;
                }
            }
        }


        private string _ParamLabel;
        public string Genealogy
        {
            get { return _ParamLabel; }
            set { _ParamLabel = value; }
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        private enum SettingType
        {
            SYSTEM = 0,
            DEVICE = 1
        }

        public EventCodeEnum DefaultGroupProberLoader()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SettingCategoryInfo addData = null;
                SettingInfo addInfoData = null;


                // Masking 기능은 sub settiog 화면에서는 Main 화면 기반으로 개발 컨셉 잡혀 있기 떄문에 임시(22.02.04) int.MaxValue로 할당함.
                #region ==> System Setting

                #region ===> System Category
                addData = new SettingCategoryInfo();
                addData.Name = "System";
                addData.Icon = "M4,6H20V16H4M20,18A2,2 0 0,0 22,16V6C22,4.89 21.1,4 20,4H4C2.89,4 2,4.89 2,6V16A2,2 0 0,0 4,18H0V20H24V18H20Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Language";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Accuracy";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010002";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Interlocks";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010003";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Alarms";
                addInfoData.ViewGUID = new Guid("0445a899-5d0d-4881-9388-ff215e5b3baf");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010004";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Stage)";
                addInfoData.ViewGUID = new Guid("7bd6a1b7-8d90-4dee-8de7-633bb966de6a");
                addInfoData.CategoryID = "00010005";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Loader)";
                addInfoData.ViewGUID = new Guid("bccda8db-7848-4e6b-af7a-79d796c987ab");
                addInfoData.CategoryID = "00010016";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cameras";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010006";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010007";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sub Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010008";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Loader Arm";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010009";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010010";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Trays";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010011";
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette Port";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010012";
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Mark";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010013";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010014";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Coordinate System";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010015";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Auto Soaking";
                //addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                //addInfoData.CategoryID = "00010020";
                //addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temp.Calibration";
                addInfoData.ViewGUID = new Guid("e905653f-52ff-460a-b7bf-d82f8996c0d8");
                addInfoData.CategoryID = "00010021";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chiller";
                addInfoData.ViewGUID = new Guid("e9a65dcf-90d7-421c-8174-6808d4466bae");
                addInfoData.CategoryID = "00010019";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "DewPoint";
                addInfoData.ViewGUID = new Guid("e9a65dcf-90d7-421c-8174-6808d4466bae");
                addInfoData.CategoryID = "00010020";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing";
                addInfoData.ViewGUID = new Guid("6d270b53-0fe3-40b2-831f-1bd8766ede86");
                addInfoData.CategoryID = "00010017";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "File System";
                addInfoData.ViewGUID = new Guid("8cf01c41-628c-4ea9-bba2-38a6f934e5fc");
                addInfoData.CategoryID = "00010018";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "LOT";
                addInfoData.ViewGUID = new Guid("e9a65dcf-90d7-421c-8174-6808d4466bae");
                addInfoData.CategoryID = "10022";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Soaking";
                addInfoData.ViewGUID = new Guid("BF19307E-AB06-4F09-AC1E-27B348A8F216");
                addInfoData.CategoryID = "00010119";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Change";
                addInfoData.ViewGUID = new Guid("444B1280-FC75-4932-9B63-D055DF8C91C9");
                addInfoData.CategoryID = "00010031";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Safety Option";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010032";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010021";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Align";
                addInfoData.ViewGUID = new Guid("DA742EF8-7AA4-41BE-BA65-81F53166213C");
                addInfoData.CategoryID = "00010033";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);

                #endregion

                #region ===> Devices Cateogy
                addData = new SettingCategoryInfo();
                addData.Name = "Devices";
                addData.Icon = "M19,10H17V8H19M19,13H17V11H19M16,10H14V8H16M16,13H14V11H16M16,17H8V15H16M7,10H5V8H7M7,13H5V11H7M8,11H10V13H8M8,8H10V10H8M11,11H13V13H11M11,8H13V10H11M20,5H4C2.89,5 2,5.89 2,7V17A2,2 0 0,0 4,19H20A2,2 0 0,0 22,17V7C22,5.89 21.1,5 20,5Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Changer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020001";
                addInfoData.MaskingLevel = int.MaxValue;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean";
                addInfoData.ViewGUID = new Guid("89829dc2-e884-4afb-a7d0-ca7576834a18");
                addInfoData.CategoryID = "00020002";
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Touch Sensor Setup";
                addInfoData.ViewGUID = new Guid("9FF1993E-2306-4894-A1A1-271317338E41");
                addInfoData.IsEnabled = true;
                addInfoData.CategoryID = "00020003";
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Brush Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020004";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020005";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chiller";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020006";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020007";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "AGV";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020008";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OHT";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020009";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FFU";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020010";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Controller";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020011";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "RFID Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020012";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Barcode Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020013";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00020014";
                addInfoData.MaskingLevel = int.MaxValue;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Networking Category
                addData = new SettingCategoryInfo();
                addData.Name = "Networking";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Tester communication";
                addInfoData.ViewGUID = new Guid("1ecfc450-fe45-4090-bc22-06da394fe9b1");
                addInfoData.CategoryID = "00030005";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM";
                addInfoData.ViewGUID = new Guid("3579E2AA-BB0D-48DB-8229-BCF66A6044A0");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00030001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB";
                addInfoData.ViewGUID = new Guid("5D647D3B-AFFD-4A4D-9C1F-D0AD8C3D1A75");
                addInfoData.CategoryID = "00030002";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "TCP/IP";
                addInfoData.ViewGUID = new Guid("58e07f0c-0ab5-4cc2-b054-0bccf49700d8");
                addInfoData.CategoryID = "00030003";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Device Up/Down";
                addInfoData.ViewGUID = new Guid("8cd732eb-7cd4-4a0b-bb18-b33b99dafb90");
                addInfoData.CategoryID = "00030004";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> User Accounts Category
                addData = new SettingCategoryInfo();
                addData.Name = "User Accounts";
                addData.Icon = "M7.5,21.5L8.85,20.16L12.66,23.97L12,24C5.71,24 0.56,19.16 0.05,13H1.55C1.91,16.76 4.25,19.94 7.5,21.5M16.5,2.5L15.15,3.84L11.34,0.03L12,0C18.29,0 23.44,4.84 23.95,11H22.45C22.09,7.24 19.75,4.07 16.5,2.5M6,17C6,15 10,13.9 12,13.9C14,13.9 18,15 18,17V18H6V17M15,9A3,3 0 0,1 12,12A3,3 0 0,1 9,9A3,3 0 0,1 12,6A3,3 0 0,1 15,9Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Account";
                addInfoData.ViewGUID = new Guid("67a5e6b4-c986-4323-b05c-9139e703a0f9");
                addInfoData.CategoryID = "00040001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> S/W Updates Category
                addData = new SettingCategoryInfo();
                addData.Name = "S/W Updates";
                addData.Icon = "F1 M 46.8902,10.7149C 50.5335,10.7149 53.4869,13.6315 53.4869,17.2291L 53.4806,17.5169C 56.192,18.326 58.1664,20.8121 58.1664,23.7536C 58.1664,25.8009 57.2101,27.6275 55.7145,28.8217C 54.4478,26.5917 52.1734,25.0271 49.5235,24.7701C 49.364,20.6099 45.3624,16.6518 40.4585,16.6518C 37.7071,16.6518 35.2419,17.7878 33.5784,19.5815L 33.532,18.8783C 33.532,15.9182 35.9686,13.5185 38.9743,13.5185C 39.7573,13.5185 40.5018,13.6814 41.1746,13.9747C 42.3148,12.0261 44.4475,10.7149 46.8902,10.7149 Z M 48.7403,26.382C 52.3009,26.382 55.2372,29.5157 55.6525,33.5592C 57.7595,33.2653 60.2729,32.3053 61.0008,29.4845C 61.0008,29.4845 61.5504,34.2139 55.687,35.1002C 55.6177,36.3113 55.2303,37.45 54.7592,38.4592C 46.3435,37.5685 33.0373,24.5267 24.8972,23.5699C 26.1182,22.156 27.8723,21.2696 29.8214,21.2696C 31.1925,21.2696 32.4671,21.7083 33.5275,22.4606C 34.8655,19.8812 37.431,18.1362 40.376,18.1362C 44.6583,18.1362 48.1381,21.8256 48.2084,26.4055L 48.7403,26.382 Z M 52.7662,41.2321C 51.6353,42.1626 50.2268,42.7088 48.7403,42.7088L 31.3881,42.75L 22.3177,42.75L 22.3177,42.7269C 20.5871,42.5911 18.875,41.7003 17.6737,40.1317C 15.9264,37.8502 15.788,34.8246 17.1279,32.6252C 15.751,31.0935 15.1917,29.1632 15.8333,27.4521C 16.8489,24.7435 20.4926,23.5993 24.0082,24.8665L 24.092,24.7142C 38.6444,28.5259 47.8797,44.0281 60.8185,38.7783C 60.0285,40.0687 57.9506,42.1072 52.7662,41.2321 Z M 18.4227,31.143C 19.839,29.9961 21.6351,29.6537 23.3307,30.0406C 23.2075,29.5022 23.1744,27.5565 23.2362,27.1689C 23.0805,27.0936 17.8649,25.846 17.4348,28.5964C 17.0725,29.5626 17.3066,30.6376 17.9696,31.5516L 18.4227,31.143 Z M 34.8333,45.9167L 41.1666,45.9167L 41.1667,57.3958L 45.9167,52.6458L 45.9167,58.9792L 38,67.2917L 30.0833,58.9792L 30.0833,52.6458L 34.8333,57.3958L 34.8333,45.9167 Z ";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Configure Auto Updater";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Manual Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050002";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Confirm Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050003";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Update History";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050004";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                //#region ===> Log Category
                //addData = new SettingCategoryInfo();
                //addData.Name = "Log";
                //addData.Icon = "M19,3A2,2 0 0,1 21,5V19C21,20.11 20.1,21 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3H19M16.7,9.35C16.92,9.14 16.92,8.79 16.7,8.58L15.42,7.3C15.21,7.08 14.86,7.08 14.65,7.3L13.65,8.3L15.7,10.35L16.7,9.35M7,14.94V17H9.06L15.12,10.94L13.06,8.88L7,14.94Z";

                ////addInfoData = new SettingInfo();
                ////addInfoData.Name = "Locations";
                ////addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                ////addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Backup";
                //addInfoData.ViewGUID = new Guid("ba31e6d7-f7d1-4234-92de-0c373e87d7ee");
                //addInfoData.CategoryID = "00060001";
                //addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Data Management";
                //addInfoData.ViewGUID = new Guid("53201f06-8810-4e46-b2b0-e748dc9d97f1");
                //addInfoData.IsEnabled = false;
                //addInfoData.CategoryID = "00060002";
                //addData.SettingInfos.Add(addInfoData);

                //SystemSettingCategoryInfos.Add(addData);
                //#endregion

                #region ===> Convinience Category
                addData = new SettingCategoryInfo();
                addData.Name = "Convenience";
                addData.Icon = "M21,10.12H14.22L16.96,7.3C14.23,4.6 9.81,4.5 7.08,7.2C4.35,9.91 4.35,14.28 7.08,17C9.81,19.7 14.23,19.7 16.96,17C18.32,15.65 19,14.08 19,12.1H21C21,14.08 20.12,16.65 18.36,18.39C14.85,21.87 9.15,21.87 5.64,18.39C2.14,14.92 2.11,9.28 5.62,5.81C9.13,2.34 14.76,2.34 18.27,5.81L21,3V10.12M12.5,8V12.25L16,14.33L15.28,15.54L11,13V8H12.5Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Auto Soaking";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Screen Saver";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070002";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Masking";
                addInfoData.ViewGUID = new Guid("9f908e5e-db34-4b12-931b-73cc175457f2");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070003";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3M4,9V12C4,14.21 7.58,16 12,16C16.42,16 20,14.21 20,12V9C20,11.21 16.42,13 12,13C7.58,13 4,11.21 4,9M4,14V17C4,19.21 7.58,21 12,21C16.42,21 20,19.21 20,17V14C20,16.21 16.42,18 12,18C7.58,18 4,16.21 4,14Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Create Data Format";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Backup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080002";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Management";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = true;
                addInfoData.CategoryID = "00080003";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Apps Category
                addData = new SettingCategoryInfo();
                addData.Name = "Apps";
                addData.Icon = "M16,20H20V16H16M16,14H20V10H16M10,8H14V4H10M16,8H20V4H16M10,14H14V10H10M4,14H8V10H4M4,20H8V16H4M10,20H14V16H10M4,8H8V4H4V8Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090002";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Cleaning";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090003";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blowing";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090004";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Scan Cassette";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090005";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090006";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Diagnosis
                addData = new SettingCategoryInfo();
                addData.Name = "Diagnosis";
                addData.Icon = "M22.7,19L13.6,9.9C14.5,7.6 14,4.9 12.1,3C10.1,1 7.1,0.6 4.7,1.7L9,6L6,9L1.6,4.7C0.4,7.1 0.9,10.1 2.9,12.1C4.8,14 7.5,14.5 9.8,13.6L18.9,22.7C19.3,23.1 19.9,23.1 20.3,22.7L22.6,20.4C23.1,20 23.1,19.3 22.7,19Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Stage";
                addInfoData.ViewGUID = new Guid("e88db401-f69d-49ee-b4f4-dd7a1816d874");
                addInfoData.CategoryID = "00100001";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Loader";
                addInfoData.ViewGUID = new Guid("647dc97d-99ab-4355-b7ac-1976d322e900");
                addInfoData.CategoryID = "00100002";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Mark";
                addInfoData.ViewGUID = new Guid("297ecf90-74ae-47e3-aba5-d21d01eb095a");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Vision Mapping";
                addInfoData.ViewGUID = new Guid("3cf6c16e-a992-47a7-a923-b77297c5e7b7");
                addInfoData.CategoryID = "00100004";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Statistics";
                addInfoData.ViewGUID = new Guid("5538cad1-5238-43a7-9ed9-c789f191e488");
                addInfoData.CategoryID = "00100005";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("e9a9d64a-4b28-40f1-9f64-1f348dc183ae");
                addInfoData.CategoryID = "00100006";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "ForcedDone";
                addInfoData.ViewGUID = new Guid("589ECFAD-B887-4E38-A9E3-68FECA7E7513");
                addInfoData.MaskingLevel = int.MaxValue;
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Viewer(Bin / XML)";
                //addInfoData.ViewGUID = new Guid("2f255256-a386-4255-8d05-5f21fd03e341");
                //addInfoData.IsEnabled = false;
                //addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck Tilting";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("f78eec9a-874c-4289-b588-3276295b2d67");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck Planarity";
                addInfoData.ViewGUID = new Guid("31c6df0a-ff3c-4d31-b8bd-b7168ac4a7fb");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Test Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("0958E509-2985-42EF-857C-660E2F05789A");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Mapping";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("5E911839-A446-4C6C-BED4-A7EFDA752BFD");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);
                SystemSettingCategoryInfos.Add(addData);

                #endregion

                #endregion

                #region ==> Device Setting

                #region ===> Wafer Category
                addData = new SettingCategoryInfo();
                addData.Name = "Wafer";
                addData.Icon = "M76.127999,72.882892 L95.301018,72.882892 94.721917,74.005702 C90.422562,82.02383 84.014145,88.742046 76.238381,93.418636 L76.127999,93.481998 z M53.127999,72.882892 L70.539002,72.882892 70.539002,96.365863 69.819657,96.676959 C67.59417,97.612224 65.287308,98.392139 62.912629,99.003108 L62.330231,99.145229 62.317387,98.637218 C62.046208,93.287501 58.2715,88.862117 53.24199,87.60979 L53.127999,87.584331 z M29.128001,72.882892 L47.539002,72.882892 47.539002,87.584102 47.424016,87.60979 C42.394501,88.862117 38.619793,93.287501 38.348614,98.637218 L38.335778,99.144984 37.754376,99.003108 C35.040459,98.304851 32.415108,97.385951 29.898589,96.26665 L29.128001,95.904727 z M5.3653045,72.882892 L23.539,72.882892 23.539,92.858653 22.695784,92.327876 C15.43276,87.547237 9.4813457,80.936504 5.4977752,73.151897 z M76.127999,49.882892 L100.66084,49.882892 100.667,50.254642 C100.667,55.980891 99.710763,61.483806 97.949257,66.612407 L97.702729,67.293887 76.127999,67.293887 z M53.127999,49.882892 L70.539002,49.882892 70.539002,67.293887 53.127999,67.293887 z M29.128001,49.882892 L47.539002,49.882892 47.539002,67.293887 29.128001,67.293887 z M0.0060243072,49.882892 L23.539,49.882892 23.539,67.293887 2.9600673,67.293887 2.5102404,65.995189 C0.8812703,61.043216 -5.4765849E-08,55.751846 0,50.254642 z M76.127999,25.882892 L94.380166,25.882892 94.504761,26.102979 C97.411202,31.407243 99.390015,37.292102 100.22292,43.539287 L100.31085,44.293891 76.127999,44.293891 z M53.127999,25.882892 L70.539002,25.882892 70.539002,44.293891 53.127999,44.293891 z M29.128001,25.882892 L47.539002,25.882892 47.539002,44.293891 29.128001,44.293891 z M6.2886501,25.882892 L23.539,25.882892 23.539,44.293891 0.35547207,44.293891 0.42432941,43.68914 C1.2203902,37.578292 3.1125291,31.811818 5.8970166,26.593446 z M23.539,7.6419465 L23.539,20.293891 9.890183,20.293891 10.302688,19.738305 C13.867483,15.069272 18.230597,11.042369 23.1883,7.8613259 z M76.127999,7.0280744 L76.498078,7.2479035 C82.046082,10.630387 86.888238,15.056211 90.751923,20.252753 L90.78164,20.293891 76.127999,20.293891 z M53.127999,0 L54.206181,0.068324186 C59.891251,0.50089046 65.312325,1.877868 70.312867,4.0427162 L70.539002,4.1437613 70.539002,20.293891 53.127999,20.293891 z M47.539002,0 L47.539002,20.293891 29.128001,20.293891 29.128001,4.5951382 29.289086,4.5186154 C34.581582,2.0793973 40.369671,0.53178801 46.46082,0.068324186 z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("A0005054-423A-F131-4B41-29B0091C34C0");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Dut Edit";
                addInfoData.ViewGUID = new Guid("016b67d9-9d63-471a-97af-075abc3a841e");
                addInfoData.CategoryID = "10023001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Setup";
                addInfoData.ViewGUID = new Guid("DE32A33D-AC5D-9A21-CFDA-02A032611C11");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pad Setup";
                addInfoData.ViewGUID = new Guid("56AF1E26-1C9F-1FDD-B38B-EA0C7324311F");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.IsEnabled = false;
                addInfoData.ViewGUID = new Guid("88573974-bdc7-4a80-b399-6fc44852122b");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "COGNEX OCR Setup";
                addInfoData.ViewGUID = new Guid("c1f9e022-9fac-4214-88c9-f3019234c1a4");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "SEMICS OCR Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.IsEnabled = false;
                addInfoData.ViewGUID = new Guid("267933e9-b956-4c26-93ae-f0dd0c78abe1");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing Sequence";
                addInfoData.ViewGUID = new Guid("f83a2fc4-3e01-4b13-ae84-b59acd8e1a26");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Card Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Card";
                addData.Icon = "M12,14C10.89,14 10,13.1 10,12C10,10.89 10.89,10 12,10C13.11,10 14,10.89 14,12A2,2 0 0,1 12,14M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("edf7abaa-b134-40bf-b970-0dbf0f8afa45");
                addInfoData.CategoryID = "10021001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align Setting";
                addInfoData.ViewGUID = new Guid("c55fcfdc-0dca-47a9-a94e-12f4199383ea");
                addInfoData.CategoryID = "10022001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                // TODO ; GP에서 사용 전이라 지워놓음.
                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Pin Align Interval Setup";
                ////addInfoData.ViewGUID = new Guid("24389CE4-FA92-44B7-9D31-FDBFE2CAB2DF");
                //addInfoData.ViewGUID = new Guid("F398D89E-7A38-4CD2-BA9F-291FAED435EB");
                //addInfoData.CategoryID = "10024001";
                //addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Mark Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Mark";
                addData.Icon = "M87.432452,60.289979 C94.05976,60.320748 99.426756,61.539662 99.419971,63.012505 99.413182,64.485337 94.035187,65.654374 87.407875,65.623604 80.780568,65.592835 75.413574,64.373925 75.420361,62.901078 75.425026,61.888501 77.96843,61.019524 81.711151,60.585293 83.412387,60.387917 85.361419,60.280362 87.432452,60.289979 z M53.208527,50.406579 L26.696541,76.674294 114.95738,77.082535 141.46938,50.814824 z M38.844406,44.730065 L168.16104,45.328209 129.31661,83.815041 0,83.216897 z M132.52171,0 L132.47142,0.051912552 138.17574,0.10193323 151.69238,0.10193323 150.15033,12.103282 122.96538,12.103282 122.32124,12.867509 C120.31486,15.226468 118.02977,17.764889 115.5564,20.359826 106.03487,30.349343 97.328761,37.393513 96.110761,36.093422 94.892761,34.793336 101.6241,25.641295 111.14563,15.651773 114.95332,11.656939 118.6306,8.1331335 121.75073,5.4256268 L122.23155,5.0123916 122.23155,0.10193323 131.67326,0.10193323 131.79921,0.046016416 132.25911,0.050050745 z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "PMI Setup";
                addInfoData.ViewGUID = new Guid("bf4d5ae0-9778-45ab-b42d-218bf162d4a6");
                addInfoData.IsEnabled = true;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Contact Setup";
                addInfoData.ViewGUID = new Guid("D13FC548-BCE1-4C4E-B1E3-413A445DB9F0");
                addInfoData.CategoryID = "10031001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Card Cleaning Category
                addData = new SettingCategoryInfo();
                addData.Name = "Card Cleaning";
                addData.Icon = "M61.461502,87.652668 L61.620384,88.305408 C69.46217,118.86195 92.404503,143.34182 121.997,153.27307 L122.923,153.5664 120.63595,154.32059 C91.70215,164.53784 69.341526,188.73314 61.620384,218.81958 L61.461502,219.47232 61.302628,218.81958 C53.58149,188.73314 31.220856,164.53784 2.287056,154.32059 L0,153.5664 0.9260025,153.27307 C30.518517,143.34182 53.460842,118.86195 61.302628,88.305408 z M144.9615,35.000001 L145.04929,35.36069 C149.38239,52.245565 162.05953,65.772608 178.41132,71.260403 L178.923,71.422485 177.65925,71.839237 C161.67144,77.485065 149.31573,90.854862 145.04929,107.47996 L144.9615,107.84067 144.87371,107.47996 C140.60728,90.854862 128.25157,77.485065 112.26375,71.839237 L111,71.422485 111.51168,71.260403 C127.86348,65.772608 140.54061,52.245565 144.87371,35.36069 z M92.961502,0 L93.013102,0.2120018 C95.559962,10.136293 103.01119,18.086981 112.62225,21.3125 L112.923,21.407766 112.18021,21.652716 C102.78308,24.971122 95.520779,32.829386 93.013102,42.600995 L92.961502,42.812998 92.909901,42.600995 C90.402225,32.829386 83.139927,24.971122 73.74279,21.652716 L73,21.407766 73.300747,21.3125 C82.911821,18.086981 90.363043,10.136293 92.909901,0.2120018 z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean Sheet Recipe Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("a4bd4bb5-030d-4644-9c3d-c21921c09eee");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Brush Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Source Setup";
                addInfoData.ViewGUID = new Guid("5b15248a-8999-481b-b2d9-41e4dad41959");
                addInfoData.MaskingLevel = int.MaxValue;
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Interval Setup";
                addInfoData.ViewGUID = new Guid("9C13D639-847B-233D-2D69-69149A78F310");
                addInfoData.CategoryID = "10041000";
                addInfoData.IsEnabled = true;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Sequence Setup";
                addInfoData.ViewGUID = new Guid("ccf581b2-92de-4374-bb38-313deca98fd9");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "SoakingView";
                //addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                //addData.SettingInfos.Add(addInfoData);


                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M17.05,14.22L19.88,17.05L22,14.93V22H14.93L17.05,19.88L14.22,17.05L17.05,14.22M12.33,18H12C7.58,18 4,16.21 4,14V17C4,19.21 7.58,21 12,21C12.39,21 12.77,21 13.14,21L14.22,19.92L12.33,18M17.54,11.89L19.29,13.64C19.73,13.21 20,12.62 20,12V9C20,10.13 19.06,11.16 17.54,11.88V11.89M4,9V12C4,14.21 7.58,16 12,16H12.45L16,12.47C14.7,12.83 13.35,13 12,13C7.58,13 4,11.21 4,9M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Format Setup";
                addInfoData.ViewGUID = new Guid("19b2b401-17e5-4a4e-9c11-9dfc7c74b03c");
                addInfoData.CategoryID = "10051001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Upload Setup";
                //addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                //addInfoData.IsEnabled = false;
                //addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Alarm Category
                addData = new SettingCategoryInfo();
                addData.Name = "Alarm";
                addData.Icon = "M6,6.9L3.87,4.78L5.28,3.37L7.4,5.5L6,6.9M13,1V4H11V1H13M20.13,4.78L18,6.9L16.6,5.5L18.72,3.37L20.13,4.78M4.5,10.5V12.5H1.5V10.5H4.5M19.5,10.5H22.5V12.5H19.5V10.5M6,20H18A2,2 0 0,1 20,22H4A2,2 0 0,1 6,20M12,5A6,6 0 0,1 18,11V19H6V11A6,6 0 0,1 12,5Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Yield Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> LOT Category
                addData = new SettingCategoryInfo();
                addData.Name = "LOT";
                addData.Icon = "M67.083,14.951105 C55.660448,14.951104 46.400642,25.415425 46.40064,38.323817 46.400642,51.232215 55.660448,61.696532 67.083,61.696532 78.50555,61.696532 87.765356,51.232215 87.765356,38.323817 87.765356,25.415425 78.50555,14.951104 67.083,14.951105 z M69.417999,0 L153.25,0 153.25,0.05099991 153.333,0.05099991 153.333,11.808392 153.25,11.808392 153.25,14.350999 131.333,14.350999 131.333,78.051665 109.833,78.051665 109.833,14.350999 93.726305,14.350999 94.636116,15.659371 C98.926348,22.142428 101.467,30.190079 101.467,38.908332 101.467,59.362693 87.482143,76.125762 69.728518,77.652358 L69.332998,77.680686 69.332998,78.051665 17.003998,78.051665 17.003998,78.053664 0.0039978027,78.053664 0.0039978027,77.886664 0,77.886664 0,62.552661 0.0039978027,62.552661 0.0039978027,0.061999127 0.33300018,0.061999127 0.33300018,0.05099991 21.833,0.05099991 21.833,62.552661 39.80481,62.552661 39.529881,62.157294 C35.239651,55.674231 32.698997,47.626582 32.698997,38.908332 32.698997,17.448018 48.093241,0.050998781 67.083,0.05099991 67.676429,0.050998781 68.266348,0.067988057 68.852396,0.10156113 L69.417999,0.14206677 z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Retest Setup";
                addInfoData.IsEnabled = true;
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.ViewGUID = new Guid("71d8a533-6b00-48f1-ae84-da3af07a308e");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Heating Setup";
                addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                addInfoData.CategoryID = "00010020";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FOUP Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Inking Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Stop Event Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "DUT Shift Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sample Test Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Communication Category
                addData = new SettingCategoryInfo();
                addData.Name = "Communication";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Temperature Category
                addData = new SettingCategoryInfo();
                addData.Name = "Temperature";
                addData.Icon = "M16.5,5C18.05,5 19.5,5.47 20.69,6.28L19.53,9.17C18.73,8.44 17.67,8 16.5,8C14,8 12,10 12,12.5C12,15 14,17 16.5,17C17.53,17 18.47,16.66 19.23,16.08L20.37,18.93C19.24,19.61 17.92,20 16.5,20A7.5,7.5 0 0,1 9,12.5A7.5,7.5 0 0,1 16.5,5M6,3A3,3 0 0,1 9,6A3,3 0 0,1 6,9A3,3 0 0,1 3,6A3,3 0 0,1 6,3M6,5A1,1 0 0,0 5,6A1,1 0 0,0 6,7A1,1 0 0,0 7,6A1,1 0 0,0 6,5Z";
                addData.MaskingLevel = int.MaxValue;

                addInfoData = new SettingInfo();
                addInfoData.Name = "Standard Temperature Setup";
                addInfoData.ViewGUID = new Guid("38059571-0235-407E-BE2F-B3CF6073034A");
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Deviation Setup";
                addInfoData.ViewGUID = new Guid("17F02446-D73D-4E02-9685-0EA1F4569E5F");
                addInfoData.CategoryID = "10090001";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Safety Option";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "10090002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Overheating Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                int i = 0;
                int j = 0;

                foreach (var deviceSettingCategoryInfo in DeviceSettingCategoryInfos)
                {
                    deviceSettingCategoryInfo.ID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, ++i, 0);
                    foreach (var deviceInfo in deviceSettingCategoryInfo.SettingInfos)
                    {
                        if (deviceInfo?.CategoryID == null)
                        {
                            deviceInfo.CategoryID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, i, ++j);
                        }
                    }
                    j = 0;
                }

                #endregion

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DefaultGroupProberStage()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SettingCategoryInfo addData = null;
                SettingInfo addInfoData = null;

                #region ==> System Setting

                #region ===> System Category
                addData = new SettingCategoryInfo();
                addData.Name = "System";
                addData.Icon = "M4,6H20V16H4M20,18A2,2 0 0,0 22,16V6C22,4.89 21.1,4 20,4H4C2.89,4 2,4.89 2,6V16A2,2 0 0,0 4,18H0V20H24V18H20Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Language";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Accuracy";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Interlocks";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Alarms";
                addInfoData.ViewGUID = new Guid("0445a899-5d0d-4881-9388-ff215e5b3baf");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Stage)";
                addInfoData.ViewGUID = new Guid("7bd6a1b7-8d90-4dee-8de7-633bb966de6a");
                addInfoData.CategoryID = "00010005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Loader)";
                addInfoData.ViewGUID = new Guid("bccda8db-7848-4e6b-af7a-79d796c987ab");
                addInfoData.CategoryID = "00010016";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cameras";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010007";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sub Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010008";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Loader Arm";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010009";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010010";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Trays";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010011";
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette Port";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010012";
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Mark";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010013";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010014";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Coordinate System";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010015";
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Auto Soaking";
                //addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                //addInfoData.CategoryID = "00010020";
                //addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temp.Calibration";
                addInfoData.ViewGUID = new Guid("e905653f-52ff-460a-b7bf-d82f8996c0d8");
                addInfoData.CategoryID = "00010021";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing";
                addInfoData.ViewGUID = new Guid("6d270b53-0fe3-40b2-831f-1bd8766ede86");
                addInfoData.CategoryID = "00010017";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "File System";
                addInfoData.ViewGUID = new Guid("8cf01c41-628c-4ea9-bba2-38a6f934e5fc");
                addInfoData.CategoryID = "00010018";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "LOT";
                addInfoData.ViewGUID = new Guid("8cf01c41-628c-4ea9-bba2-38a6f934e5fc");
                addInfoData.CategoryID = "00010019";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Soaking";
                addInfoData.ViewGUID = new Guid("BF19307E-AB06-4F09-AC1E-27B348A8F216");
                addInfoData.CategoryID = "00010119";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Change";
                addInfoData.ViewGUID = new Guid("444B1280-FC75-4932-9B63-D055DF8C91C9");
                addInfoData.CategoryID = "00010031";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Safety Option";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010032";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010021";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Devices Cateogy
                addData = new SettingCategoryInfo();
                addData.Name = "Devices";
                addData.Icon = "M19,10H17V8H19M19,13H17V11H19M16,10H14V8H16M16,13H14V11H16M16,17H8V15H16M7,10H5V8H7M7,13H5V11H7M8,11H10V13H8M8,8H10V10H8M11,11H13V13H11M11,8H13V10H11M20,5H4C2.89,5 2,5.89 2,7V17A2,2 0 0,0 4,19H20A2,2 0 0,0 22,17V7C22,5.89 21.1,5 20,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Changer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean Unit";
                addInfoData.ViewGUID = new Guid("89829dc2-e884-4afb-a7d0-ca7576834a18");
                addInfoData.CategoryID = "00020002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Touch Sensor Setup";
                addInfoData.ViewGUID = new Guid("9FF1993E-2306-4894-A1A1-271317338E41");
                addInfoData.CategoryID = "00020003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Brush Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chiller";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020007";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "AGV";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020008";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OHT";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020009";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FFU";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020010";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Controller";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020011";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "RFID Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020012";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Barcode Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020013";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00020014";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Networking Category
                addData = new SettingCategoryInfo();
                addData.Name = "Networking";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Tester communication";
                addInfoData.ViewGUID = new Guid("1ecfc450-fe45-4090-bc22-06da394fe9b1");
                addInfoData.CategoryID = "00030005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM";
                addInfoData.ViewGUID = new Guid("3579E2AA-BB0D-48DB-8229-BCF66A6044A0");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00030001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB";
                addInfoData.ViewGUID = new Guid("5D647D3B-AFFD-4A4D-9C1F-D0AD8C3D1A75");
                addInfoData.CategoryID = "00030002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "TCP/IP";
                addInfoData.ViewGUID = new Guid("58e07f0c-0ab5-4cc2-b054-0bccf49700d8");
                addInfoData.CategoryID = "00030003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Device Up/Down";
                addInfoData.ViewGUID = new Guid("8cd732eb-7cd4-4a0b-bb18-b33b99dafb90");
                addInfoData.CategoryID = "00030004";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> User Accounts Category
                addData = new SettingCategoryInfo();
                addData.Name = "User Accounts";
                addData.Icon = "M7.5,21.5L8.85,20.16L12.66,23.97L12,24C5.71,24 0.56,19.16 0.05,13H1.55C1.91,16.76 4.25,19.94 7.5,21.5M16.5,2.5L15.15,3.84L11.34,0.03L12,0C18.29,0 23.44,4.84 23.95,11H22.45C22.09,7.24 19.75,4.07 16.5,2.5M6,17C6,15 10,13.9 12,13.9C14,13.9 18,15 18,17V18H6V17M15,9A3,3 0 0,1 12,12A3,3 0 0,1 9,9A3,3 0 0,1 12,6A3,3 0 0,1 15,9Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Account";
                addInfoData.ViewGUID = new Guid("67a5e6b4-c986-4323-b05c-9139e703a0f9");
                addInfoData.CategoryID = "00040001";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> S/W Updates Category
                addData = new SettingCategoryInfo();
                addData.Name = "S/W Updates";
                addData.Icon = "F1 M 46.8902,10.7149C 50.5335,10.7149 53.4869,13.6315 53.4869,17.2291L 53.4806,17.5169C 56.192,18.326 58.1664,20.8121 58.1664,23.7536C 58.1664,25.8009 57.2101,27.6275 55.7145,28.8217C 54.4478,26.5917 52.1734,25.0271 49.5235,24.7701C 49.364,20.6099 45.3624,16.6518 40.4585,16.6518C 37.7071,16.6518 35.2419,17.7878 33.5784,19.5815L 33.532,18.8783C 33.532,15.9182 35.9686,13.5185 38.9743,13.5185C 39.7573,13.5185 40.5018,13.6814 41.1746,13.9747C 42.3148,12.0261 44.4475,10.7149 46.8902,10.7149 Z M 48.7403,26.382C 52.3009,26.382 55.2372,29.5157 55.6525,33.5592C 57.7595,33.2653 60.2729,32.3053 61.0008,29.4845C 61.0008,29.4845 61.5504,34.2139 55.687,35.1002C 55.6177,36.3113 55.2303,37.45 54.7592,38.4592C 46.3435,37.5685 33.0373,24.5267 24.8972,23.5699C 26.1182,22.156 27.8723,21.2696 29.8214,21.2696C 31.1925,21.2696 32.4671,21.7083 33.5275,22.4606C 34.8655,19.8812 37.431,18.1362 40.376,18.1362C 44.6583,18.1362 48.1381,21.8256 48.2084,26.4055L 48.7403,26.382 Z M 52.7662,41.2321C 51.6353,42.1626 50.2268,42.7088 48.7403,42.7088L 31.3881,42.75L 22.3177,42.75L 22.3177,42.7269C 20.5871,42.5911 18.875,41.7003 17.6737,40.1317C 15.9264,37.8502 15.788,34.8246 17.1279,32.6252C 15.751,31.0935 15.1917,29.1632 15.8333,27.4521C 16.8489,24.7435 20.4926,23.5993 24.0082,24.8665L 24.092,24.7142C 38.6444,28.5259 47.8797,44.0281 60.8185,38.7783C 60.0285,40.0687 57.9506,42.1072 52.7662,41.2321 Z M 18.4227,31.143C 19.839,29.9961 21.6351,29.6537 23.3307,30.0406C 23.2075,29.5022 23.1744,27.5565 23.2362,27.1689C 23.0805,27.0936 17.8649,25.846 17.4348,28.5964C 17.0725,29.5626 17.3066,30.6376 17.9696,31.5516L 18.4227,31.143 Z M 34.8333,45.9167L 41.1666,45.9167L 41.1667,57.3958L 45.9167,52.6458L 45.9167,58.9792L 38,67.2917L 30.0833,58.9792L 30.0833,52.6458L 34.8333,57.3958L 34.8333,45.9167 Z ";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Configure Auto Updater";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Manual Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Confirm Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Update History";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050004";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                //#region ===> Log Category
                //addData = new SettingCategoryInfo();
                //addData.Name = "Log";
                //addData.Icon = "M19,3A2,2 0 0,1 21,5V19C21,20.11 20.1,21 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3H19M16.7,9.35C16.92,9.14 16.92,8.79 16.7,8.58L15.42,7.3C15.21,7.08 14.86,7.08 14.65,7.3L13.65,8.3L15.7,10.35L16.7,9.35M7,14.94V17H9.06L15.12,10.94L13.06,8.88L7,14.94Z";

                ////addInfoData = new SettingInfo();
                ////addInfoData.Name = "Locations";
                ////addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                ////addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Backup";
                //addInfoData.ViewGUID = new Guid("ba31e6d7-f7d1-4234-92de-0c373e87d7ee");
                //addInfoData.CategoryID = "00060001";
                //addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Data Management";
                //addInfoData.ViewGUID = new Guid("53201f06-8810-4e46-b2b0-e748dc9d97f1");
                //addInfoData.IsEnabled = false;
                //addInfoData.CategoryID = "00060002";
                //addData.SettingInfos.Add(addInfoData);

                //SystemSettingCategoryInfos.Add(addData);
                //#endregion

                #region ===> Convinience Category
                addData = new SettingCategoryInfo();
                addData.Name = "Convenience";
                addData.Icon = "M21,10.12H14.22L16.96,7.3C14.23,4.6 9.81,4.5 7.08,7.2C4.35,9.91 4.35,14.28 7.08,17C9.81,19.7 14.23,19.7 16.96,17C18.32,15.65 19,14.08 19,12.1H21C21,14.08 20.12,16.65 18.36,18.39C14.85,21.87 9.15,21.87 5.64,18.39C2.14,14.92 2.11,9.28 5.62,5.81C9.13,2.34 14.76,2.34 18.27,5.81L21,3V10.12M12.5,8V12.25L16,14.33L15.28,15.54L11,13V8H12.5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Auto Soaking";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Screen Saver";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Masking";
                addInfoData.ViewGUID = new Guid("9f908e5e-db34-4b12-931b-73cc175457f2");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070003";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3M4,9V12C4,14.21 7.58,16 12,16C16.42,16 20,14.21 20,12V9C20,11.21 16.42,13 12,13C7.58,13 4,11.21 4,9M4,14V17C4,19.21 7.58,21 12,21C16.42,21 20,19.21 20,17V14C20,16.21 16.42,18 12,18C7.58,18 4,16.21 4,14Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Create Data Format";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Backup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Management";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = true;
                addInfoData.CategoryID = "00080003";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Apps Category
                addData = new SettingCategoryInfo();
                addData.Name = "Apps";
                addData.Icon = "M16,20H20V16H16M16,14H20V10H16M10,8H14V4H10M16,8H20V4H16M10,14H14V10H10M4,14H8V10H4M4,20H8V16H4M10,20H14V16H10M4,8H8V4H4V8Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Cleaning";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blowing";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Scan Cassette";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090006";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Diagnosis
                addData = new SettingCategoryInfo();
                addData.Name = "Diagnosis";
                addData.Icon = "M22.7,19L13.6,9.9C14.5,7.6 14,4.9 12.1,3C10.1,1 7.1,0.6 4.7,1.7L9,6L6,9L1.6,4.7C0.4,7.1 0.9,10.1 2.9,12.1C4.8,14 7.5,14.5 9.8,13.6L18.9,22.7C19.3,23.1 19.9,23.1 20.3,22.7L22.6,20.4C23.1,20 23.1,19.3 22.7,19Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Stage";
                addInfoData.ViewGUID = new Guid("e88db401-f69d-49ee-b4f4-dd7a1816d874");
                addInfoData.CategoryID = "00100001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Loader";
                addInfoData.ViewGUID = new Guid("647dc97d-99ab-4355-b7ac-1976d322e900");
                addInfoData.CategoryID = "00100002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Mark";
                addInfoData.ViewGUID = new Guid("297ecf90-74ae-47e3-aba5-d21d01eb095a");
                addInfoData.CategoryID = "00100003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Vision Mapping";
                addInfoData.ViewGUID = new Guid("3cf6c16e-a992-47a7-a923-b77297c5e7b7");
                addInfoData.CategoryID = "00100004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Statistics";
                addInfoData.ViewGUID = new Guid("5538cad1-5238-43a7-9ed9-c789f191e488");
                addInfoData.CategoryID = "00100005";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("e9a9d64a-4b28-40f1-9f64-1f348dc183ae");
                addInfoData.CategoryID = "00100006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "ForcedDone";
                addInfoData.ViewGUID = new Guid("589ECFAD-B887-4E38-A9E3-68FECA7E7513");
                addInfoData.MaskingLevel = -1;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck Tilting";
                addInfoData.ViewGUID = new Guid("f78eec9a-874c-4289-b588-3276295b2d67");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck Planarity";
                addInfoData.ViewGUID = new Guid("31c6df0a-ff3c-4d31-b8bd-b7168ac4a7fb");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Test Setup";
                addInfoData.ViewGUID = new Guid("0958E509-2985-42EF-857C-660E2F05789A");
                addInfoData.MaskingLevel = -1;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Mapping";
                addInfoData.ViewGUID = new Guid("5E911839-A446-4C6C-BED4-A7EFDA752BFD");
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);

                #endregion

                #endregion

                #region ==> Device Setting

                #region ===> Wafer Category
                addData = new SettingCategoryInfo();
                addData.Name = "Wafer";
                addData.Icon = "M76.127999,72.882892 L95.301018,72.882892 94.721917,74.005702 C90.422562,82.02383 84.014145,88.742046 76.238381,93.418636 L76.127999,93.481998 z M53.127999,72.882892 L70.539002,72.882892 70.539002,96.365863 69.819657,96.676959 C67.59417,97.612224 65.287308,98.392139 62.912629,99.003108 L62.330231,99.145229 62.317387,98.637218 C62.046208,93.287501 58.2715,88.862117 53.24199,87.60979 L53.127999,87.584331 z M29.128001,72.882892 L47.539002,72.882892 47.539002,87.584102 47.424016,87.60979 C42.394501,88.862117 38.619793,93.287501 38.348614,98.637218 L38.335778,99.144984 37.754376,99.003108 C35.040459,98.304851 32.415108,97.385951 29.898589,96.26665 L29.128001,95.904727 z M5.3653045,72.882892 L23.539,72.882892 23.539,92.858653 22.695784,92.327876 C15.43276,87.547237 9.4813457,80.936504 5.4977752,73.151897 z M76.127999,49.882892 L100.66084,49.882892 100.667,50.254642 C100.667,55.980891 99.710763,61.483806 97.949257,66.612407 L97.702729,67.293887 76.127999,67.293887 z M53.127999,49.882892 L70.539002,49.882892 70.539002,67.293887 53.127999,67.293887 z M29.128001,49.882892 L47.539002,49.882892 47.539002,67.293887 29.128001,67.293887 z M0.0060243072,49.882892 L23.539,49.882892 23.539,67.293887 2.9600673,67.293887 2.5102404,65.995189 C0.8812703,61.043216 -5.4765849E-08,55.751846 0,50.254642 z M76.127999,25.882892 L94.380166,25.882892 94.504761,26.102979 C97.411202,31.407243 99.390015,37.292102 100.22292,43.539287 L100.31085,44.293891 76.127999,44.293891 z M53.127999,25.882892 L70.539002,25.882892 70.539002,44.293891 53.127999,44.293891 z M29.128001,25.882892 L47.539002,25.882892 47.539002,44.293891 29.128001,44.293891 z M6.2886501,25.882892 L23.539,25.882892 23.539,44.293891 0.35547207,44.293891 0.42432941,43.68914 C1.2203902,37.578292 3.1125291,31.811818 5.8970166,26.593446 z M23.539,7.6419465 L23.539,20.293891 9.890183,20.293891 10.302688,19.738305 C13.867483,15.069272 18.230597,11.042369 23.1883,7.8613259 z M76.127999,7.0280744 L76.498078,7.2479035 C82.046082,10.630387 86.888238,15.056211 90.751923,20.252753 L90.78164,20.293891 76.127999,20.293891 z M53.127999,0 L54.206181,0.068324186 C59.891251,0.50089046 65.312325,1.877868 70.312867,4.0427162 L70.539002,4.1437613 70.539002,20.293891 53.127999,20.293891 z M47.539002,0 L47.539002,20.293891 29.128001,20.293891 29.128001,4.5951382 29.289086,4.5186154 C34.581582,2.0793973 40.369671,0.53178801 46.46082,0.068324186 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("A0005054-423A-F131-4B41-29B0091C34C0");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Dut Edit";
                addInfoData.ViewGUID = new Guid("016b67d9-9d63-471a-97af-075abc3a841e");
                addInfoData.CategoryID = "10023001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Setup";
                addInfoData.ViewGUID = new Guid("DE32A33D-AC5D-9A21-CFDA-02A032611C11");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pad Setup";
                addInfoData.ViewGUID = new Guid("56AF1E26-1C9F-1FDD-B38B-EA0C7324311F");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.IsEnabled = false;
                addInfoData.ViewGUID = new Guid("88573974-bdc7-4a80-b399-6fc44852122b");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "COGNEX OCR Setup";
                addInfoData.ViewGUID = new Guid("c1f9e022-9fac-4214-88c9-f3019234c1a4");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "SEMICS OCR Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.IsEnabled = false;
                addInfoData.ViewGUID = new Guid("267933e9-b956-4c26-93ae-f0dd0c78abe1");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing Sequence";
                addInfoData.ViewGUID = new Guid("f83a2fc4-3e01-4b13-ae84-b59acd8e1a26");
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Card Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Card";
                addData.Icon = "M12,14C10.89,14 10,13.1 10,12C10,10.89 10.89,10 12,10C13.11,10 14,10.89 14,12A2,2 0 0,1 12,14M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("edf7abaa-b134-40bf-b970-0dbf0f8afa45");
                addInfoData.CategoryID = "10021001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align Setting";
                addInfoData.ViewGUID = new Guid("c55fcfdc-0dca-47a9-a94e-12f4199383ea");
                addInfoData.CategoryID = "10022001";
                addData.SettingInfos.Add(addInfoData);

                // TODO ; GP에서 사용 전이라 지워놓음.
                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Pin Align Interval Setup";
                ////addInfoData.ViewGUID = new Guid("24389CE4-FA92-44B7-9D31-FDBFE2CAB2DF");
                //addInfoData.ViewGUID = new Guid("F398D89E-7A38-4CD2-BA9F-291FAED435EB");
                //addInfoData.CategoryID = "10024001";
                //addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Mark Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Mark";
                addData.Icon = "M87.432452,60.289979 C94.05976,60.320748 99.426756,61.539662 99.419971,63.012505 99.413182,64.485337 94.035187,65.654374 87.407875,65.623604 80.780568,65.592835 75.413574,64.373925 75.420361,62.901078 75.425026,61.888501 77.96843,61.019524 81.711151,60.585293 83.412387,60.387917 85.361419,60.280362 87.432452,60.289979 z M53.208527,50.406579 L26.696541,76.674294 114.95738,77.082535 141.46938,50.814824 z M38.844406,44.730065 L168.16104,45.328209 129.31661,83.815041 0,83.216897 z M132.52171,0 L132.47142,0.051912552 138.17574,0.10193323 151.69238,0.10193323 150.15033,12.103282 122.96538,12.103282 122.32124,12.867509 C120.31486,15.226468 118.02977,17.764889 115.5564,20.359826 106.03487,30.349343 97.328761,37.393513 96.110761,36.093422 94.892761,34.793336 101.6241,25.641295 111.14563,15.651773 114.95332,11.656939 118.6306,8.1331335 121.75073,5.4256268 L122.23155,5.0123916 122.23155,0.10193323 131.67326,0.10193323 131.79921,0.046016416 132.25911,0.050050745 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "PMI Setup";
                addInfoData.ViewGUID = new Guid("bf4d5ae0-9778-45ab-b42d-218bf162d4a6");
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Contact Setup";
                addInfoData.ViewGUID = new Guid("D13FC548-BCE1-4C4E-B1E3-413A445DB9F0");
                addInfoData.CategoryID = "10031001";
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Card Cleaning Category
                addData = new SettingCategoryInfo();
                addData.Name = "Card Cleaning";
                addData.Icon = "M61.461502,87.652668 L61.620384,88.305408 C69.46217,118.86195 92.404503,143.34182 121.997,153.27307 L122.923,153.5664 120.63595,154.32059 C91.70215,164.53784 69.341526,188.73314 61.620384,218.81958 L61.461502,219.47232 61.302628,218.81958 C53.58149,188.73314 31.220856,164.53784 2.287056,154.32059 L0,153.5664 0.9260025,153.27307 C30.518517,143.34182 53.460842,118.86195 61.302628,88.305408 z M144.9615,35.000001 L145.04929,35.36069 C149.38239,52.245565 162.05953,65.772608 178.41132,71.260403 L178.923,71.422485 177.65925,71.839237 C161.67144,77.485065 149.31573,90.854862 145.04929,107.47996 L144.9615,107.84067 144.87371,107.47996 C140.60728,90.854862 128.25157,77.485065 112.26375,71.839237 L111,71.422485 111.51168,71.260403 C127.86348,65.772608 140.54061,52.245565 144.87371,35.36069 z M92.961502,0 L93.013102,0.2120018 C95.559962,10.136293 103.01119,18.086981 112.62225,21.3125 L112.923,21.407766 112.18021,21.652716 C102.78308,24.971122 95.520779,32.829386 93.013102,42.600995 L92.961502,42.812998 92.909901,42.600995 C90.402225,32.829386 83.139927,24.971122 73.74279,21.652716 L73,21.407766 73.300747,21.3125 C82.911821,18.086981 90.363043,10.136293 92.909901,0.2120018 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean Sheet Recipe Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("a4bd4bb5-030d-4644-9c3d-c21921c09eee");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Brush Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Source Setup";
                addInfoData.ViewGUID = new Guid("5b15248a-8999-481b-b2d9-41e4dad41959");
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Interval Setup";
                addInfoData.ViewGUID = new Guid("9C13D639-847B-233D-2D69-69149A78F310");
                addInfoData.CategoryID = "10041000";
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Sequence Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("ccf581b2-92de-4374-bb38-313deca98fd9");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "SoakingView";
                //addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                //addData.SettingInfos.Add(addInfoData);


                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M17.05,14.22L19.88,17.05L22,14.93V22H14.93L17.05,19.88L14.22,17.05L17.05,14.22M12.33,18H12C7.58,18 4,16.21 4,14V17C4,19.21 7.58,21 12,21C12.39,21 12.77,21 13.14,21L14.22,19.92L12.33,18M17.54,11.89L19.29,13.64C19.73,13.21 20,12.62 20,12V9C20,10.13 19.06,11.16 17.54,11.88V11.89M4,9V12C4,14.21 7.58,16 12,16H12.45L16,12.47C14.7,12.83 13.35,13 12,13C7.58,13 4,11.21 4,9M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Format Setup";
                addInfoData.ViewGUID = new Guid("19b2b401-17e5-4a4e-9c11-9dfc7c74b03c");
                addInfoData.CategoryID = "10051001";
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Upload Setup";
                //addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                //addInfoData.IsEnabled = false;
                //addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Alarm Category
                addData = new SettingCategoryInfo();
                addData.Name = "Alarm";
                addData.Icon = "M6,6.9L3.87,4.78L5.28,3.37L7.4,5.5L6,6.9M13,1V4H11V1H13M20.13,4.78L18,6.9L16.6,5.5L18.72,3.37L20.13,4.78M4.5,10.5V12.5H1.5V10.5H4.5M19.5,10.5H22.5V12.5H19.5V10.5M6,20H18A2,2 0 0,1 20,22H4A2,2 0 0,1 6,20M12,5A6,6 0 0,1 18,11V19H6V11A6,6 0 0,1 12,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Yield Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> LOT Category
                addData = new SettingCategoryInfo();
                addData.Name = "LOT";
                addData.Icon = "M67.083,14.951105 C55.660448,14.951104 46.400642,25.415425 46.40064,38.323817 46.400642,51.232215 55.660448,61.696532 67.083,61.696532 78.50555,61.696532 87.765356,51.232215 87.765356,38.323817 87.765356,25.415425 78.50555,14.951104 67.083,14.951105 z M69.417999,0 L153.25,0 153.25,0.05099991 153.333,0.05099991 153.333,11.808392 153.25,11.808392 153.25,14.350999 131.333,14.350999 131.333,78.051665 109.833,78.051665 109.833,14.350999 93.726305,14.350999 94.636116,15.659371 C98.926348,22.142428 101.467,30.190079 101.467,38.908332 101.467,59.362693 87.482143,76.125762 69.728518,77.652358 L69.332998,77.680686 69.332998,78.051665 17.003998,78.051665 17.003998,78.053664 0.0039978027,78.053664 0.0039978027,77.886664 0,77.886664 0,62.552661 0.0039978027,62.552661 0.0039978027,0.061999127 0.33300018,0.061999127 0.33300018,0.05099991 21.833,0.05099991 21.833,62.552661 39.80481,62.552661 39.529881,62.157294 C35.239651,55.674231 32.698997,47.626582 32.698997,38.908332 32.698997,17.448018 48.093241,0.050998781 67.083,0.05099991 67.676429,0.050998781 68.266348,0.067988057 68.852396,0.10156113 L69.417999,0.14206677 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Retest Setup";
                addInfoData.IsEnabled = true;
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.ViewGUID = new Guid("71d8a533-6b00-48f1-ae84-da3af07a308e");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Heating Setup";
                addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                addInfoData.CategoryID = "00010020";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FOUP Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Inking Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Stop Event Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "DUT Shift Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sample Test Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Communication Category
                addData = new SettingCategoryInfo();
                addData.Name = "Communication";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Temperature Category
                addData = new SettingCategoryInfo();
                addData.Name = "Temperature";
                addData.Icon = "M16.5,5C18.05,5 19.5,5.47 20.69,6.28L19.53,9.17C18.73,8.44 17.67,8 16.5,8C14,8 12,10 12,12.5C12,15 14,17 16.5,17C17.53,17 18.47,16.66 19.23,16.08L20.37,18.93C19.24,19.61 17.92,20 16.5,20A7.5,7.5 0 0,1 9,12.5A7.5,7.5 0 0,1 16.5,5M6,3A3,3 0 0,1 9,6A3,3 0 0,1 6,9A3,3 0 0,1 3,6A3,3 0 0,1 6,3M6,5A1,1 0 0,0 5,6A1,1 0 0,0 6,7A1,1 0 0,0 7,6A1,1 0 0,0 6,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Standard Temperature Setup";
                addInfoData.ViewGUID = new Guid("38059571-0235-407E-BE2F-B3CF6073034A");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Deviation Setup";
                addInfoData.ViewGUID = new Guid("17F02446-D73D-4E02-9685-0EA1F4569E5F");
                addInfoData.CategoryID = "10090001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Safety Option";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "10090002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Overheating Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                int i = 0;
                int j = 0;

                foreach (var deviceSettingCategoryInfo in DeviceSettingCategoryInfos)
                {
                    deviceSettingCategoryInfo.ID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, ++i, 0);
                    foreach (var deviceInfo in deviceSettingCategoryInfo.SettingInfos)
                    {
                        if (deviceInfo?.CategoryID == null)
                        {
                            deviceInfo.CategoryID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, i, ++j);
                        }
                    }
                    j = 0;
                }

                #endregion

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum MakeDefaultParamSetFor_BSCI()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SettingCategoryInfo addData = null;
                SettingInfo addInfoData = null;

                #region ==> System Setting

                #region ===> System Category
                addData = new SettingCategoryInfo();
                addData.Name = "System";
                addData.Icon = "M4,6H20V16H4M20,18A2,2 0 0,0 22,16V6C22,4.89 21.1,4 20,4H4C2.89,4 2,4.89 2,6V16A2,2 0 0,0 4,18H0V20H24V18H20Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Language";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Accuracy";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Interlocks";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Alarms";
                addInfoData.ViewGUID = new Guid("0445a899-5d0d-4881-9388-ff215e5b3baf");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Stage)";
                addInfoData.ViewGUID = new Guid("7bd6a1b7-8d90-4dee-8de7-633bb966de6a");
                addInfoData.CategoryID = "00010005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Loader)";
                addInfoData.ViewGUID = new Guid("bccda8db-7848-4e6b-af7a-79d796c987ab");
                addInfoData.CategoryID = "00010016";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cameras";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010007";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sub Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010008";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Loader Arm";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010009";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010010";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Trays";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010011";
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette Port";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010012";
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Mark";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010013";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010014";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Coordinate System";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010015";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temp.Calibration";
                addInfoData.ViewGUID = new Guid("e905653f-52ff-460a-b7bf-d82f8996c0d8");
                addInfoData.CategoryID = "00010021";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing";
                addInfoData.ViewGUID = new Guid("6d270b53-0fe3-40b2-831f-1bd8766ede86");
                addInfoData.CategoryID = "00010017";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "File System";
                addInfoData.ViewGUID = new Guid("8cf01c41-628c-4ea9-bba2-38a6f934e5fc");
                addInfoData.CategoryID = "00010018";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Soaking";
                addInfoData.ViewGUID = new Guid("BF19307E-AB06-4F09-AC1E-27B348A8F216");
                addInfoData.CategoryID = "00010119";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Change";
                addInfoData.ViewGUID = new Guid("444B1280-FC75-4932-9B63-D055DF8C91C9");
                addInfoData.CategoryID = "00010031";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Devices Cateogy
                addData = new SettingCategoryInfo();
                addData.Name = "Devices";
                addData.Icon = "M19,10H17V8H19M19,13H17V11H19M16,10H14V8H16M16,13H14V11H16M16,17H8V15H16M7,10H5V8H7M7,13H5V11H7M8,11H10V13H8M8,8H10V10H8M11,11H13V13H11M11,8H13V10H11M20,5H4C2.89,5 2,5.89 2,7V17A2,2 0 0,0 4,19H20A2,2 0 0,0 22,17V7C22,5.89 21.1,5 20,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Changer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean Unit";
                addInfoData.ViewGUID = new Guid("89829dc2-e884-4afb-a7d0-ca7576834a18");
                addInfoData.CategoryID = "00020002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Touch Sensor Setup";
                addInfoData.ViewGUID = new Guid("9FF1993E-2306-4894-A1A1-271317338E41");
                addInfoData.CategoryID = "00020003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Brush Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chiller";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020007";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "AGV";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020008";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OHT";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020009";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FFU";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020010";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Controller";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020011";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "RFID Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020012";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Barcode Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020013";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00020014";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Networking Category
                addData = new SettingCategoryInfo();
                addData.Name = "Networking";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM";
                addInfoData.ViewGUID = new Guid("3579E2AA-BB0D-48DB-8229-BCF66A6044A0");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00030001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB";
                addInfoData.ViewGUID = new Guid("5D647D3B-AFFD-4A4D-9C1F-D0AD8C3D1A75");
                addInfoData.CategoryID = "00030002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "TCP/IP";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00030003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Device Up/Down";
                addInfoData.ViewGUID = new Guid("8cd732eb-7cd4-4a0b-bb18-b33b99dafb90");
                addInfoData.CategoryID = "00030004";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> User Accounts Category
                addData = new SettingCategoryInfo();
                addData.Name = "User Accounts";
                addData.Icon = "M7.5,21.5L8.85,20.16L12.66,23.97L12,24C5.71,24 0.56,19.16 0.05,13H1.55C1.91,16.76 4.25,19.94 7.5,21.5M16.5,2.5L15.15,3.84L11.34,0.03L12,0C18.29,0 23.44,4.84 23.95,11H22.45C22.09,7.24 19.75,4.07 16.5,2.5M6,17C6,15 10,13.9 12,13.9C14,13.9 18,15 18,17V18H6V17M15,9A3,3 0 0,1 12,12A3,3 0 0,1 9,9A3,3 0 0,1 12,6A3,3 0 0,1 15,9Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Account";
                addInfoData.ViewGUID = new Guid("67a5e6b4-c986-4323-b05c-9139e703a0f9");
                addInfoData.CategoryID = "00040001";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> S/W Updates Category
                addData = new SettingCategoryInfo();
                addData.Name = "S/W Updates";
                addData.Icon = "F1 M 46.8902,10.7149C 50.5335,10.7149 53.4869,13.6315 53.4869,17.2291L 53.4806,17.5169C 56.192,18.326 58.1664,20.8121 58.1664,23.7536C 58.1664,25.8009 57.2101,27.6275 55.7145,28.8217C 54.4478,26.5917 52.1734,25.0271 49.5235,24.7701C 49.364,20.6099 45.3624,16.6518 40.4585,16.6518C 37.7071,16.6518 35.2419,17.7878 33.5784,19.5815L 33.532,18.8783C 33.532,15.9182 35.9686,13.5185 38.9743,13.5185C 39.7573,13.5185 40.5018,13.6814 41.1746,13.9747C 42.3148,12.0261 44.4475,10.7149 46.8902,10.7149 Z M 48.7403,26.382C 52.3009,26.382 55.2372,29.5157 55.6525,33.5592C 57.7595,33.2653 60.2729,32.3053 61.0008,29.4845C 61.0008,29.4845 61.5504,34.2139 55.687,35.1002C 55.6177,36.3113 55.2303,37.45 54.7592,38.4592C 46.3435,37.5685 33.0373,24.5267 24.8972,23.5699C 26.1182,22.156 27.8723,21.2696 29.8214,21.2696C 31.1925,21.2696 32.4671,21.7083 33.5275,22.4606C 34.8655,19.8812 37.431,18.1362 40.376,18.1362C 44.6583,18.1362 48.1381,21.8256 48.2084,26.4055L 48.7403,26.382 Z M 52.7662,41.2321C 51.6353,42.1626 50.2268,42.7088 48.7403,42.7088L 31.3881,42.75L 22.3177,42.75L 22.3177,42.7269C 20.5871,42.5911 18.875,41.7003 17.6737,40.1317C 15.9264,37.8502 15.788,34.8246 17.1279,32.6252C 15.751,31.0935 15.1917,29.1632 15.8333,27.4521C 16.8489,24.7435 20.4926,23.5993 24.0082,24.8665L 24.092,24.7142C 38.6444,28.5259 47.8797,44.0281 60.8185,38.7783C 60.0285,40.0687 57.9506,42.1072 52.7662,41.2321 Z M 18.4227,31.143C 19.839,29.9961 21.6351,29.6537 23.3307,30.0406C 23.2075,29.5022 23.1744,27.5565 23.2362,27.1689C 23.0805,27.0936 17.8649,25.846 17.4348,28.5964C 17.0725,29.5626 17.3066,30.6376 17.9696,31.5516L 18.4227,31.143 Z M 34.8333,45.9167L 41.1666,45.9167L 41.1667,57.3958L 45.9167,52.6458L 45.9167,58.9792L 38,67.2917L 30.0833,58.9792L 30.0833,52.6458L 34.8333,57.3958L 34.8333,45.9167 Z ";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Configure Auto Updater";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Manual Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Confirm Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Update History";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050004";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                //#region ===> Log Category
                //addData = new SettingCategoryInfo();
                //addData.Name = "Log";
                //addData.Icon = "M19,3A2,2 0 0,1 21,5V19C21,20.11 20.1,21 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3H19M16.7,9.35C16.92,9.14 16.92,8.79 16.7,8.58L15.42,7.3C15.21,7.08 14.86,7.08 14.65,7.3L13.65,8.3L15.7,10.35L16.7,9.35M7,14.94V17H9.06L15.12,10.94L13.06,8.88L7,14.94Z";

                ////addInfoData = new SettingInfo();
                ////addInfoData.Name = "Locations";
                ////addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                ////addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Backup";
                //addInfoData.ViewGUID = new Guid("ba31e6d7-f7d1-4234-92de-0c373e87d7ee");
                //addInfoData.CategoryID = "00060001";
                //addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Data Management";
                //addInfoData.ViewGUID = new Guid("53201f06-8810-4e46-b2b0-e748dc9d97f1");
                //addInfoData.IsEnabled = false;
                //addInfoData.CategoryID = "00060002";
                //addData.SettingInfos.Add(addInfoData);

                //SystemSettingCategoryInfos.Add(addData);
                //#endregion

                #region ===> Convinience Category
                addData = new SettingCategoryInfo();
                addData.Name = "Convenience";
                addData.Icon = "M21,10.12H14.22L16.96,7.3C14.23,4.6 9.81,4.5 7.08,7.2C4.35,9.91 4.35,14.28 7.08,17C9.81,19.7 14.23,19.7 16.96,17C18.32,15.65 19,14.08 19,12.1H21C21,14.08 20.12,16.65 18.36,18.39C14.85,21.87 9.15,21.87 5.64,18.39C2.14,14.92 2.11,9.28 5.62,5.81C9.13,2.34 14.76,2.34 18.27,5.81L21,3V10.12M12.5,8V12.25L16,14.33L15.28,15.54L11,13V8H12.5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Auto Soaking";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Screen Saver";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Masking";
                addInfoData.ViewGUID = new Guid("9f908e5e-db34-4b12-931b-73cc175457f2");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070003";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3M4,9V12C4,14.21 7.58,16 12,16C16.42,16 20,14.21 20,12V9C20,11.21 16.42,13 12,13C7.58,13 4,11.21 4,9M4,14V17C4,19.21 7.58,21 12,21C16.42,21 20,19.21 20,17V14C20,16.21 16.42,18 12,18C7.58,18 4,16.21 4,14Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Create Data Format";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Backup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Management";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080003";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Apps Category
                addData = new SettingCategoryInfo();
                addData.Name = "Apps";
                addData.Icon = "M16,20H20V16H16M16,14H20V10H16M10,8H14V4H10M16,8H20V4H16M10,14H14V10H10M4,14H8V10H4M4,20H8V16H4M10,20H14V16H10M4,8H8V4H4V8Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Cleaning";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blowing";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Scan Cassette";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090006";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Diagnosis
                addData = new SettingCategoryInfo();
                addData.Name = "Diagnosis";
                addData.Icon = "M22.7,19L13.6,9.9C14.5,7.6 14,4.9 12.1,3C10.1,1 7.1,0.6 4.7,1.7L9,6L6,9L1.6,4.7C0.4,7.1 0.9,10.1 2.9,12.1C4.8,14 7.5,14.5 9.8,13.6L18.9,22.7C19.3,23.1 19.9,23.1 20.3,22.7L22.6,20.4C23.1,20 23.1,19.3 22.7,19Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Stage";
                addInfoData.ViewGUID = new Guid("e88db401-f69d-49ee-b4f4-dd7a1816d874");
                addInfoData.CategoryID = "00100001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Loader";
                addInfoData.ViewGUID = new Guid("647dc97d-99ab-4355-b7ac-1976d322e900");
                addInfoData.CategoryID = "00100002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Mark";
                addInfoData.ViewGUID = new Guid("297ecf90-74ae-47e3-aba5-d21d01eb095a");
                addInfoData.CategoryID = "00100003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Vision Mapping";
                addInfoData.ViewGUID = new Guid("3cf6c16e-a992-47a7-a923-b77297c5e7b7");
                addInfoData.CategoryID = "00100004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Statistics";
                addInfoData.ViewGUID = new Guid("5538cad1-5238-43a7-9ed9-c789f191e488");
                addInfoData.CategoryID = "00100005";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("e9a9d64a-4b28-40f1-9f64-1f348dc183ae");
                addInfoData.CategoryID = "00100006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "ForcedDone";
                addInfoData.ViewGUID = new Guid("589ECFAD-B887-4E38-A9E3-68FECA7E7513");
                addInfoData.MaskingLevel = -1;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Chuck Tilting";
                //addInfoData.ViewGUID = new Guid("f78eec9a-874c-4289-b588-3276295b2d67");
                //addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck Planarity";
                addInfoData.ViewGUID = new Guid("31c6df0a-ff3c-4d31-b8bd-b7168ac4a7fb");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Test Setup";
                addInfoData.ViewGUID = new Guid("0958E509-2985-42EF-857C-660E2F05789A");
                addInfoData.MaskingLevel = -1;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Mapping";
                addInfoData.ViewGUID = new Guid("5E911839-A446-4C6C-BED4-A7EFDA752BFD");
                addData.SettingInfos.Add(addInfoData);
                
                SystemSettingCategoryInfos.Add(addData);

                #endregion

                #endregion

                #region ==> Device Setting

                #region ===> Wafer Category
                addData = new SettingCategoryInfo();
                addData.Name = "Wafer";
                addData.Icon = "M76.127999,72.882892 L95.301018,72.882892 94.721917,74.005702 C90.422562,82.02383 84.014145,88.742046 76.238381,93.418636 L76.127999,93.481998 z M53.127999,72.882892 L70.539002,72.882892 70.539002,96.365863 69.819657,96.676959 C67.59417,97.612224 65.287308,98.392139 62.912629,99.003108 L62.330231,99.145229 62.317387,98.637218 C62.046208,93.287501 58.2715,88.862117 53.24199,87.60979 L53.127999,87.584331 z M29.128001,72.882892 L47.539002,72.882892 47.539002,87.584102 47.424016,87.60979 C42.394501,88.862117 38.619793,93.287501 38.348614,98.637218 L38.335778,99.144984 37.754376,99.003108 C35.040459,98.304851 32.415108,97.385951 29.898589,96.26665 L29.128001,95.904727 z M5.3653045,72.882892 L23.539,72.882892 23.539,92.858653 22.695784,92.327876 C15.43276,87.547237 9.4813457,80.936504 5.4977752,73.151897 z M76.127999,49.882892 L100.66084,49.882892 100.667,50.254642 C100.667,55.980891 99.710763,61.483806 97.949257,66.612407 L97.702729,67.293887 76.127999,67.293887 z M53.127999,49.882892 L70.539002,49.882892 70.539002,67.293887 53.127999,67.293887 z M29.128001,49.882892 L47.539002,49.882892 47.539002,67.293887 29.128001,67.293887 z M0.0060243072,49.882892 L23.539,49.882892 23.539,67.293887 2.9600673,67.293887 2.5102404,65.995189 C0.8812703,61.043216 -5.4765849E-08,55.751846 0,50.254642 z M76.127999,25.882892 L94.380166,25.882892 94.504761,26.102979 C97.411202,31.407243 99.390015,37.292102 100.22292,43.539287 L100.31085,44.293891 76.127999,44.293891 z M53.127999,25.882892 L70.539002,25.882892 70.539002,44.293891 53.127999,44.293891 z M29.128001,25.882892 L47.539002,25.882892 47.539002,44.293891 29.128001,44.293891 z M6.2886501,25.882892 L23.539,25.882892 23.539,44.293891 0.35547207,44.293891 0.42432941,43.68914 C1.2203902,37.578292 3.1125291,31.811818 5.8970166,26.593446 z M23.539,7.6419465 L23.539,20.293891 9.890183,20.293891 10.302688,19.738305 C13.867483,15.069272 18.230597,11.042369 23.1883,7.8613259 z M76.127999,7.0280744 L76.498078,7.2479035 C82.046082,10.630387 86.888238,15.056211 90.751923,20.252753 L90.78164,20.293891 76.127999,20.293891 z M53.127999,0 L54.206181,0.068324186 C59.891251,0.50089046 65.312325,1.877868 70.312867,4.0427162 L70.539002,4.1437613 70.539002,20.293891 53.127999,20.293891 z M47.539002,0 L47.539002,20.293891 29.128001,20.293891 29.128001,4.5951382 29.289086,4.5186154 C34.581582,2.0793973 40.369671,0.53178801 46.46082,0.068324186 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("A0005054-423A-F131-4B41-29B0091C34C0");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Dut Edit";
                addInfoData.ViewGUID = new Guid("016b67d9-9d63-471a-97af-075abc3a841e");
                addInfoData.CategoryID = "10023001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Setup";
                addInfoData.ViewGUID = new Guid("DE32A33D-AC5D-9A21-CFDA-02A032611C11");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pad Setup";
                addInfoData.ViewGUID = new Guid("56AF1E26-1C9F-1FDD-B38B-EA0C7324311F");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR Setup";
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.IsEnabled = true;
                addInfoData.ViewGUID = new Guid("88573974-bdc7-4a80-b399-6fc44852122b");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "COGNEX OCR Setup";
                addInfoData.ViewGUID = new Guid("c1f9e022-9fac-4214-88c9-f3019234c1a4");
                addInfoData.IsEnabled = true;
                addInfoData.Visibility = Visibility.Visible;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "SEMICS OCR Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.IsEnabled = false;
                addInfoData.ViewGUID = new Guid("267933e9-b956-4c26-93ae-f0dd0c78abe1");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing Sequence";
                addInfoData.ViewGUID = new Guid("f83a2fc4-3e01-4b13-ae84-b59acd8e1a26");
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Card Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Card";
                addData.Icon = "M12,14C10.89,14 10,13.1 10,12C10,10.89 10.89,10 12,10C13.11,10 14,10.89 14,12A2,2 0 0,1 12,14M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("edf7abaa-b134-40bf-b970-0dbf0f8afa45");
                addInfoData.CategoryID = "10021001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align Setting";
                addInfoData.ViewGUID = new Guid("c55fcfdc-0dca-47a9-a94e-12f4199383ea");
                addInfoData.CategoryID = "10022001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align Interval Setup";
                addInfoData.ViewGUID = new Guid("F398D89E-7A38-4CD2-BA9F-291FAED435EB");
                addInfoData.CategoryID = "10024001";
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Mark Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Mark";
                addData.Icon = "M87.432452,60.289979 C94.05976,60.320748 99.426756,61.539662 99.419971,63.012505 99.413182,64.485337 94.035187,65.654374 87.407875,65.623604 80.780568,65.592835 75.413574,64.373925 75.420361,62.901078 75.425026,61.888501 77.96843,61.019524 81.711151,60.585293 83.412387,60.387917 85.361419,60.280362 87.432452,60.289979 z M53.208527,50.406579 L26.696541,76.674294 114.95738,77.082535 141.46938,50.814824 z M38.844406,44.730065 L168.16104,45.328209 129.31661,83.815041 0,83.216897 z M132.52171,0 L132.47142,0.051912552 138.17574,0.10193323 151.69238,0.10193323 150.15033,12.103282 122.96538,12.103282 122.32124,12.867509 C120.31486,15.226468 118.02977,17.764889 115.5564,20.359826 106.03487,30.349343 97.328761,37.393513 96.110761,36.093422 94.892761,34.793336 101.6241,25.641295 111.14563,15.651773 114.95332,11.656939 118.6306,8.1331335 121.75073,5.4256268 L122.23155,5.0123916 122.23155,0.10193323 131.67326,0.10193323 131.79921,0.046016416 132.25911,0.050050745 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "PMI Setup";
                addInfoData.ViewGUID = new Guid("bf4d5ae0-9778-45ab-b42d-218bf162d4a6");
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Contact Setup";
                addInfoData.ViewGUID = new Guid("D13FC548-BCE1-4C4E-B1E3-413A445DB9F0");
                addInfoData.CategoryID = "10031001";
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Card Cleaning Category
                addData = new SettingCategoryInfo();
                addData.Name = "Card Cleaning";
                addData.Icon = "M61.461502,87.652668 L61.620384,88.305408 C69.46217,118.86195 92.404503,143.34182 121.997,153.27307 L122.923,153.5664 120.63595,154.32059 C91.70215,164.53784 69.341526,188.73314 61.620384,218.81958 L61.461502,219.47232 61.302628,218.81958 C53.58149,188.73314 31.220856,164.53784 2.287056,154.32059 L0,153.5664 0.9260025,153.27307 C30.518517,143.34182 53.460842,118.86195 61.302628,88.305408 z M144.9615,35.000001 L145.04929,35.36069 C149.38239,52.245565 162.05953,65.772608 178.41132,71.260403 L178.923,71.422485 177.65925,71.839237 C161.67144,77.485065 149.31573,90.854862 145.04929,107.47996 L144.9615,107.84067 144.87371,107.47996 C140.60728,90.854862 128.25157,77.485065 112.26375,71.839237 L111,71.422485 111.51168,71.260403 C127.86348,65.772608 140.54061,52.245565 144.87371,35.36069 z M92.961502,0 L93.013102,0.2120018 C95.559962,10.136293 103.01119,18.086981 112.62225,21.3125 L112.923,21.407766 112.18021,21.652716 C102.78308,24.971122 95.520779,32.829386 93.013102,42.600995 L92.961502,42.812998 92.909901,42.600995 C90.402225,32.829386 83.139927,24.971122 73.74279,21.652716 L73,21.407766 73.300747,21.3125 C82.911821,18.086981 90.363043,10.136293 92.909901,0.2120018 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean Sheet Recipe Setup";
                addInfoData.IsEnabled = true;
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.ViewGUID = new Guid("a4bd4bb5-030d-4644-9c3d-c21921c09eee");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Brush Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Source Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("5b15248a-8999-481b-b2d9-41e4dad41959");
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Interval Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("9C13D639-847B-233D-2D69-69149A78F310");
                addInfoData.CategoryID = "10041000";
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Sequence Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("ccf581b2-92de-4374-bb38-313deca98fd9");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "SoakingView";
                //addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                //addData.SettingInfos.Add(addInfoData);


                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M17.05,14.22L19.88,17.05L22,14.93V22H14.93L17.05,19.88L14.22,17.05L17.05,14.22M12.33,18H12C7.58,18 4,16.21 4,14V17C4,19.21 7.58,21 12,21C12.39,21 12.77,21 13.14,21L14.22,19.92L12.33,18M17.54,11.89L19.29,13.64C19.73,13.21 20,12.62 20,12V9C20,10.13 19.06,11.16 17.54,11.88V11.89M4,9V12C4,14.21 7.58,16 12,16H12.45L16,12.47C14.7,12.83 13.35,13 12,13C7.58,13 4,11.21 4,9M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Format Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Upload Setup";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Alarm Category
                addData = new SettingCategoryInfo();
                addData.Name = "Alarm";
                addData.Icon = "M6,6.9L3.87,4.78L5.28,3.37L7.4,5.5L6,6.9M13,1V4H11V1H13M20.13,4.78L18,6.9L16.6,5.5L18.72,3.37L20.13,4.78M4.5,10.5V12.5H1.5V10.5H4.5M19.5,10.5H22.5V12.5H19.5V10.5M6,20H18A2,2 0 0,1 20,22H4A2,2 0 0,1 6,20M12,5A6,6 0 0,1 18,11V19H6V11A6,6 0 0,1 12,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Yield Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> LOT Category
                addData = new SettingCategoryInfo();
                addData.Name = "LOT";
                addData.Icon = "M67.083,14.951105 C55.660448,14.951104 46.400642,25.415425 46.40064,38.323817 46.400642,51.232215 55.660448,61.696532 67.083,61.696532 78.50555,61.696532 87.765356,51.232215 87.765356,38.323817 87.765356,25.415425 78.50555,14.951104 67.083,14.951105 z M69.417999,0 L153.25,0 153.25,0.05099991 153.333,0.05099991 153.333,11.808392 153.25,11.808392 153.25,14.350999 131.333,14.350999 131.333,78.051665 109.833,78.051665 109.833,14.350999 93.726305,14.350999 94.636116,15.659371 C98.926348,22.142428 101.467,30.190079 101.467,38.908332 101.467,59.362693 87.482143,76.125762 69.728518,77.652358 L69.332998,77.680686 69.332998,78.051665 17.003998,78.051665 17.003998,78.053664 0.0039978027,78.053664 0.0039978027,77.886664 0,77.886664 0,62.552661 0.0039978027,62.552661 0.0039978027,0.061999127 0.33300018,0.061999127 0.33300018,0.05099991 21.833,0.05099991 21.833,62.552661 39.80481,62.552661 39.529881,62.157294 C35.239651,55.674231 32.698997,47.626582 32.698997,38.908332 32.698997,17.448018 48.093241,0.050998781 67.083,0.05099991 67.676429,0.050998781 68.266348,0.067988057 68.852396,0.10156113 L69.417999,0.14206677 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Retest Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("71d8a533-6b00-48f1-ae84-da3af07a308e");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Heating Setup";
                addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                addInfoData.CategoryID = "00010020";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FOUP Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Inking Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Stop Event Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "DUT Shift Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sample Test Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Communication Category
                addData = new SettingCategoryInfo();
                addData.Name = "Communication";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Temperature Category
                addData = new SettingCategoryInfo();
                addData.Name = "Temperature";
                addData.Icon = "M16.5,5C18.05,5 19.5,5.47 20.69,6.28L19.53,9.17C18.73,8.44 17.67,8 16.5,8C14,8 12,10 12,12.5C12,15 14,17 16.5,17C17.53,17 18.47,16.66 19.23,16.08L20.37,18.93C19.24,19.61 17.92,20 16.5,20A7.5,7.5 0 0,1 9,12.5A7.5,7.5 0 0,1 16.5,5M6,3A3,3 0 0,1 9,6A3,3 0 0,1 6,9A3,3 0 0,1 3,6A3,3 0 0,1 6,3M6,5A1,1 0 0,0 5,6A1,1 0 0,0 6,7A1,1 0 0,0 7,6A1,1 0 0,0 6,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Standard Temperature Setup";
                addInfoData.ViewGUID = new Guid("38059571-0235-407E-BE2F-B3CF6073034A");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Deviation Setup";
                addInfoData.ViewGUID = new Guid("17F02446-D73D-4E02-9685-0EA1F4569E5F");
                addInfoData.CategoryID = "10090001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Overheating Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                int i = 0;
                int j = 0;

                foreach (var deviceSettingCategoryInfo in DeviceSettingCategoryInfos)
                {
                    deviceSettingCategoryInfo.ID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, ++i, 0);
                    foreach (var deviceInfo in deviceSettingCategoryInfo.SettingInfos)
                    {
                        if (deviceInfo?.CategoryID == null)
                        {
                            deviceInfo.CategoryID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, i, ++j);
                        }
                    }
                    j = 0;
                }

                #endregion

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum MakeDefaultParamSetFor_ALL()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SettingCategoryInfo addData = null;
                SettingInfo addInfoData = null;

                #region ==> System Setting

                #region ===> System Category
                addData = new SettingCategoryInfo();
                addData.Name = "System";
                addData.Icon = "M4,6H20V16H4M20,18A2,2 0 0,0 22,16V6C22,4.89 21.1,4 20,4H4C2.89,4 2,4.89 2,6V16A2,2 0 0,0 4,18H0V20H24V18H20Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Language";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Accuracy";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Interlocks";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Alarms";
                addInfoData.ViewGUID = new Guid("0445a899-5d0d-4881-9388-ff215e5b3baf");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00010004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Stage)";
                addInfoData.ViewGUID = new Guid("7bd6a1b7-8d90-4dee-8de7-633bb966de6a");
                addInfoData.CategoryID = "00010005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Motors(Loader)";
                addInfoData.ViewGUID = new Guid("bccda8db-7848-4e6b-af7a-79d796c987ab");
                addInfoData.CategoryID = "00010016";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cameras";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010007";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sub Chuck";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010008";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Loader Arm";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010009";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010010";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Trays";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010011";
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Cassette Port";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.CategoryID = "00010012";
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Mark";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010013";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010014";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Coordinate System";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00010015";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temp.Calibration";
                addInfoData.ViewGUID = new Guid("e905653f-52ff-460a-b7bf-d82f8996c0d8");
                addInfoData.CategoryID = "00010021";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing";
                addInfoData.ViewGUID = new Guid("6d270b53-0fe3-40b2-831f-1bd8766ede86");
                addInfoData.CategoryID = "00010017";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "File System";
                addInfoData.ViewGUID = new Guid("8cf01c41-628c-4ea9-bba2-38a6f934e5fc");
                addInfoData.CategoryID = "00010018";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Soaking";
                addInfoData.ViewGUID = new Guid("BF19307E-AB06-4F09-AC1E-27B348A8F216");
                addInfoData.CategoryID = "00010119";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Change";
                addInfoData.ViewGUID = new Guid("444B1280-FC75-4932-9B63-D055DF8C91C9");
                addInfoData.CategoryID = "00010031";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);


                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Align";                
                addInfoData.ViewGUID = new Guid("DA742EF8-7AA4-41BE-BA65-81F53166213C");
                addInfoData.CategoryID = "10033";
                addInfoData.MaskingLevel = int.MaxValue;
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Devices Cateogy
                addData = new SettingCategoryInfo();
                addData.Name = "Devices";
                addData.Icon = "M19,10H17V8H19M19,13H17V11H19M16,10H14V8H16M16,13H14V11H16M16,17H8V15H16M7,10H5V8H7M7,13H5V11H7M8,11H10V13H8M8,8H10V10H8M11,11H13V13H11M11,8H13V10H11M20,5H4C2.89,5 2,5.89 2,7V17A2,2 0 0,0 4,19H20A2,2 0 0,0 22,17V7C22,5.89 21.1,5 20,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Card Changer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean Unit";
                addInfoData.ViewGUID = new Guid("89829dc2-e884-4afb-a7d0-ca7576834a18");
                addInfoData.CategoryID = "00020002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Touch Sensor Setup";
                addInfoData.ViewGUID = new Guid("9FF1993E-2306-4894-A1A1-271317338E41");
                addInfoData.CategoryID = "00020003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Brush Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chiller";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Unit";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020007";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "AGV";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020008";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OHT";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020009";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FFU";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020010";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Controller";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020011";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "RFID Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020012";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Barcode Reader";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00020013";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.CategoryID = "00020014";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Networking Category
                addData = new SettingCategoryInfo();
                addData.Name = "Networking";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Tester communication";
                addInfoData.ViewGUID = new Guid("1ecfc450-fe45-4090-bc22-06da394fe9b1");
                addInfoData.CategoryID = "00030005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM";
                addInfoData.ViewGUID = new Guid("3579E2AA-BB0D-48DB-8229-BCF66A6044A0");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00030001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB";
                addInfoData.ViewGUID = new Guid("5D647D3B-AFFD-4A4D-9C1F-D0AD8C3D1A75");
                addInfoData.CategoryID = "00030002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "TCP/IP";
                addInfoData.ViewGUID = new Guid("58e07f0c-0ab5-4cc2-b054-0bccf49700d8");
                addInfoData.IsEnabled = true;
                addInfoData.CategoryID = "00030003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Device Up/Down";
                addInfoData.ViewGUID = new Guid("8cd732eb-7cd4-4a0b-bb18-b33b99dafb90");
                addInfoData.CategoryID = "00030004";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> User Accounts Category
                addData = new SettingCategoryInfo();
                addData.Name = "User Accounts";
                addData.Icon = "M7.5,21.5L8.85,20.16L12.66,23.97L12,24C5.71,24 0.56,19.16 0.05,13H1.55C1.91,16.76 4.25,19.94 7.5,21.5M16.5,2.5L15.15,3.84L11.34,0.03L12,0C18.29,0 23.44,4.84 23.95,11H22.45C22.09,7.24 19.75,4.07 16.5,2.5M6,17C6,15 10,13.9 12,13.9C14,13.9 18,15 18,17V18H6V17M15,9A3,3 0 0,1 12,12A3,3 0 0,1 9,9A3,3 0 0,1 12,6A3,3 0 0,1 15,9Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Account";
                addInfoData.ViewGUID = new Guid("67a5e6b4-c986-4323-b05c-9139e703a0f9");
                addInfoData.CategoryID = "00040001";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> S/W Updates Category
                addData = new SettingCategoryInfo();
                addData.Name = "S/W Updates";
                addData.Icon = "F1 M 46.8902,10.7149C 50.5335,10.7149 53.4869,13.6315 53.4869,17.2291L 53.4806,17.5169C 56.192,18.326 58.1664,20.8121 58.1664,23.7536C 58.1664,25.8009 57.2101,27.6275 55.7145,28.8217C 54.4478,26.5917 52.1734,25.0271 49.5235,24.7701C 49.364,20.6099 45.3624,16.6518 40.4585,16.6518C 37.7071,16.6518 35.2419,17.7878 33.5784,19.5815L 33.532,18.8783C 33.532,15.9182 35.9686,13.5185 38.9743,13.5185C 39.7573,13.5185 40.5018,13.6814 41.1746,13.9747C 42.3148,12.0261 44.4475,10.7149 46.8902,10.7149 Z M 48.7403,26.382C 52.3009,26.382 55.2372,29.5157 55.6525,33.5592C 57.7595,33.2653 60.2729,32.3053 61.0008,29.4845C 61.0008,29.4845 61.5504,34.2139 55.687,35.1002C 55.6177,36.3113 55.2303,37.45 54.7592,38.4592C 46.3435,37.5685 33.0373,24.5267 24.8972,23.5699C 26.1182,22.156 27.8723,21.2696 29.8214,21.2696C 31.1925,21.2696 32.4671,21.7083 33.5275,22.4606C 34.8655,19.8812 37.431,18.1362 40.376,18.1362C 44.6583,18.1362 48.1381,21.8256 48.2084,26.4055L 48.7403,26.382 Z M 52.7662,41.2321C 51.6353,42.1626 50.2268,42.7088 48.7403,42.7088L 31.3881,42.75L 22.3177,42.75L 22.3177,42.7269C 20.5871,42.5911 18.875,41.7003 17.6737,40.1317C 15.9264,37.8502 15.788,34.8246 17.1279,32.6252C 15.751,31.0935 15.1917,29.1632 15.8333,27.4521C 16.8489,24.7435 20.4926,23.5993 24.0082,24.8665L 24.092,24.7142C 38.6444,28.5259 47.8797,44.0281 60.8185,38.7783C 60.0285,40.0687 57.9506,42.1072 52.7662,41.2321 Z M 18.4227,31.143C 19.839,29.9961 21.6351,29.6537 23.3307,30.0406C 23.2075,29.5022 23.1744,27.5565 23.2362,27.1689C 23.0805,27.0936 17.8649,25.846 17.4348,28.5964C 17.0725,29.5626 17.3066,30.6376 17.9696,31.5516L 18.4227,31.143 Z M 34.8333,45.9167L 41.1666,45.9167L 41.1667,57.3958L 45.9167,52.6458L 45.9167,58.9792L 38,67.2917L 30.0833,58.9792L 30.0833,52.6458L 34.8333,57.3958L 34.8333,45.9167 Z ";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Configure Auto Updater";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Manual Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Confirm Updates";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Update History";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00050004";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                //#region ===> Log Category
                //addData = new SettingCategoryInfo();
                //addData.Name = "Log";
                //addData.Icon = "M19,3A2,2 0 0,1 21,5V19C21,20.11 20.1,21 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3H19M16.7,9.35C16.92,9.14 16.92,8.79 16.7,8.58L15.42,7.3C15.21,7.08 14.86,7.08 14.65,7.3L13.65,8.3L15.7,10.35L16.7,9.35M7,14.94V17H9.06L15.12,10.94L13.06,8.88L7,14.94Z";

                ////addInfoData = new SettingInfo();
                ////addInfoData.Name = "Locations";
                ////addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                ////addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Backup";
                //addInfoData.ViewGUID = new Guid("ba31e6d7-f7d1-4234-92de-0c373e87d7ee");
                //addInfoData.CategoryID = "00060001";
                //addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "Data Management";
                //addInfoData.ViewGUID = new Guid("53201f06-8810-4e46-b2b0-e748dc9d97f1");
                //addInfoData.IsEnabled = false;
                //addInfoData.CategoryID = "00060002";
                //addData.SettingInfos.Add(addInfoData);

                //SystemSettingCategoryInfos.Add(addData);
                //#endregion

                #region ===> Convinience Category
                addData = new SettingCategoryInfo();
                addData.Name = "Convenience";
                addData.Icon = "M21,10.12H14.22L16.96,7.3C14.23,4.6 9.81,4.5 7.08,7.2C4.35,9.91 4.35,14.28 7.08,17C9.81,19.7 14.23,19.7 16.96,17C18.32,15.65 19,14.08 19,12.1H21C21,14.08 20.12,16.65 18.36,18.39C14.85,21.87 9.15,21.87 5.64,18.39C2.14,14.92 2.11,9.28 5.62,5.81C9.13,2.34 14.76,2.34 18.27,5.81L21,3V10.12M12.5,8V12.25L16,14.33L15.28,15.54L11,13V8H12.5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Auto Soaking";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Screen Saver";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Masking";
                addInfoData.ViewGUID = new Guid("9f908e5e-db34-4b12-931b-73cc175457f2");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00070003";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3M4,9V12C4,14.21 7.58,16 12,16C16.42,16 20,14.21 20,12V9C20,11.21 16.42,13 12,13C7.58,13 4,11.21 4,9M4,14V17C4,19.21 7.58,21 12,21C16.42,21 20,19.21 20,17V14C20,16.21 16.42,18 12,18C7.58,18 4,16.21 4,14Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Create Data Format";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Backup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00080002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Management";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = true;
                addInfoData.CategoryID = "00080003";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Apps Category
                addData = new SettingCategoryInfo();
                addData.Name = "Apps";
                addData.Icon = "M16,20H20V16H16M16,14H20V10H16M10,8H14V4H10M16,8H20V4H16M10,14H14V10H10M4,14H8V10H4M4,20H8V16H4M10,20H14V16H10M4,8H8V4H4V8Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Wafer Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Cleaning";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blowing";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Scan Cassette";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090005";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.CategoryID = "00090006";
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Diagnosis
                addData = new SettingCategoryInfo();
                addData.Name = "Diagnosis";
                addData.Icon = "M22.7,19L13.6,9.9C14.5,7.6 14,4.9 12.1,3C10.1,1 7.1,0.6 4.7,1.7L9,6L6,9L1.6,4.7C0.4,7.1 0.9,10.1 2.9,12.1C4.8,14 7.5,14.5 9.8,13.6L18.9,22.7C19.3,23.1 19.9,23.1 20.3,22.7L22.6,20.4C23.1,20 23.1,19.3 22.7,19Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Stage";
                addInfoData.ViewGUID = new Guid("e88db401-f69d-49ee-b4f4-dd7a1816d874");
                addInfoData.CategoryID = "00100001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Loader";
                addInfoData.ViewGUID = new Guid("647dc97d-99ab-4355-b7ac-1976d322e900");
                addInfoData.CategoryID = "00100002";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Machine Setting - Mark";
                addInfoData.ViewGUID = new Guid("297ecf90-74ae-47e3-aba5-d21d01eb095a");
                addInfoData.CategoryID = "00100003";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Vision Mapping";
                addInfoData.ViewGUID = new Guid("3cf6c16e-a992-47a7-a923-b77297c5e7b7");
                addInfoData.CategoryID = "00100004";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Statistics";
                addInfoData.ViewGUID = new Guid("5538cad1-5238-43a7-9ed9-c789f191e488");
                addInfoData.CategoryID = "00100005";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "I/O";
                addInfoData.ViewGUID = new Guid("e9a9d64a-4b28-40f1-9f64-1f348dc183ae");
                addInfoData.CategoryID = "00100006";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "ForcedDone";
                addInfoData.ViewGUID = new Guid("589ECFAD-B887-4E38-A9E3-68FECA7E7513");
                addInfoData.MaskingLevel = -1;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck Tilting";
                addInfoData.ViewGUID = new Guid("f78eec9a-874c-4289-b588-3276295b2d67");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Chuck Planarity";
                addInfoData.ViewGUID = new Guid("31c6df0a-ff3c-4d31-b8bd-b7168ac4a7fb");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Accuracy Check";
                addInfoData.ViewGUID = new Guid("6fc69255-2cb4-415c-af6f-2f59b668c94d");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Test Setup";
                addInfoData.ViewGUID = new Guid("0958E509-2985-42EF-857C-660E2F05789A");
                addInfoData.MaskingLevel = -1;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Mapping";
                addInfoData.ViewGUID = new Guid("5E911839-A446-4C6C-BED4-A7EFDA752BFD");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Digital Twin";
                addInfoData.ViewGUID = new Guid("6b96a9a8-19c7-4cd2-b55c-42b75ddc0873");
                addData.SettingInfos.Add(addInfoData);

                SystemSettingCategoryInfos.Add(addData);

                #endregion

                #endregion

                #region ==> Device Setting

                #region ===> Wafer Category
                addData = new SettingCategoryInfo();
                addData.Name = "Wafer";
                addData.Icon = "M76.127999,72.882892 L95.301018,72.882892 94.721917,74.005702 C90.422562,82.02383 84.014145,88.742046 76.238381,93.418636 L76.127999,93.481998 z M53.127999,72.882892 L70.539002,72.882892 70.539002,96.365863 69.819657,96.676959 C67.59417,97.612224 65.287308,98.392139 62.912629,99.003108 L62.330231,99.145229 62.317387,98.637218 C62.046208,93.287501 58.2715,88.862117 53.24199,87.60979 L53.127999,87.584331 z M29.128001,72.882892 L47.539002,72.882892 47.539002,87.584102 47.424016,87.60979 C42.394501,88.862117 38.619793,93.287501 38.348614,98.637218 L38.335778,99.144984 37.754376,99.003108 C35.040459,98.304851 32.415108,97.385951 29.898589,96.26665 L29.128001,95.904727 z M5.3653045,72.882892 L23.539,72.882892 23.539,92.858653 22.695784,92.327876 C15.43276,87.547237 9.4813457,80.936504 5.4977752,73.151897 z M76.127999,49.882892 L100.66084,49.882892 100.667,50.254642 C100.667,55.980891 99.710763,61.483806 97.949257,66.612407 L97.702729,67.293887 76.127999,67.293887 z M53.127999,49.882892 L70.539002,49.882892 70.539002,67.293887 53.127999,67.293887 z M29.128001,49.882892 L47.539002,49.882892 47.539002,67.293887 29.128001,67.293887 z M0.0060243072,49.882892 L23.539,49.882892 23.539,67.293887 2.9600673,67.293887 2.5102404,65.995189 C0.8812703,61.043216 -5.4765849E-08,55.751846 0,50.254642 z M76.127999,25.882892 L94.380166,25.882892 94.504761,26.102979 C97.411202,31.407243 99.390015,37.292102 100.22292,43.539287 L100.31085,44.293891 76.127999,44.293891 z M53.127999,25.882892 L70.539002,25.882892 70.539002,44.293891 53.127999,44.293891 z M29.128001,25.882892 L47.539002,25.882892 47.539002,44.293891 29.128001,44.293891 z M6.2886501,25.882892 L23.539,25.882892 23.539,44.293891 0.35547207,44.293891 0.42432941,43.68914 C1.2203902,37.578292 3.1125291,31.811818 5.8970166,26.593446 z M23.539,7.6419465 L23.539,20.293891 9.890183,20.293891 10.302688,19.738305 C13.867483,15.069272 18.230597,11.042369 23.1883,7.8613259 z M76.127999,7.0280744 L76.498078,7.2479035 C82.046082,10.630387 86.888238,15.056211 90.751923,20.252753 L90.78164,20.293891 76.127999,20.293891 z M53.127999,0 L54.206181,0.068324186 C59.891251,0.50089046 65.312325,1.877868 70.312867,4.0427162 L70.539002,4.1437613 70.539002,20.293891 53.127999,20.293891 z M47.539002,0 L47.539002,20.293891 29.128001,20.293891 29.128001,4.5951382 29.289086,4.5186154 C34.581582,2.0793973 40.369671,0.53178801 46.46082,0.068324186 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("A0005054-423A-F131-4B41-29B0091C34C0");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Dut Edit";
                addInfoData.ViewGUID = new Guid("016b67d9-9d63-471a-97af-075abc3a841e");
                addInfoData.CategoryID = "10023001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Align Setup";
                addInfoData.ViewGUID = new Guid("DE32A33D-AC5D-9A21-CFDA-02A032611C11");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pad Setup";
                addInfoData.ViewGUID = new Guid("56AF1E26-1C9F-1FDD-B38B-EA0C7324311F");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "OCR Setup";
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.IsEnabled = true;
                addInfoData.ViewGUID = new Guid("88573974-bdc7-4a80-b399-6fc44852122b");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "COGNEX OCR Setup";
                addInfoData.ViewGUID = new Guid("c1f9e022-9fac-4214-88c9-f3019234c1a4");
                addInfoData.IsEnabled = true;
                addInfoData.Visibility = Visibility.Visible;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "SEMICS OCR Setup";
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.IsEnabled = false;
                addInfoData.ViewGUID = new Guid("267933e9-b956-4c26-93ae-f0dd0c78abe1");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Probing Sequence";
                addInfoData.ViewGUID = new Guid("f83a2fc4-3e01-4b13-ae84-b59acd8e1a26");
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Card Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Card";
                addData.Icon = "M12,14C10.89,14 10,13.1 10,12C10,10.89 10.89,10 12,10C13.11,10 14,10.89 14,12A2,2 0 0,1 12,14M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Basic Information";
                addInfoData.ViewGUID = new Guid("edf7abaa-b134-40bf-b970-0dbf0f8afa45");
                addInfoData.CategoryID = "10021001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align Setting";
                addInfoData.ViewGUID = new Guid("c55fcfdc-0dca-47a9-a94e-12f4199383ea");
                addInfoData.CategoryID = "10022001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Pin Align Interval Setup";
                addInfoData.ViewGUID = new Guid("F398D89E-7A38-4CD2-BA9F-291FAED435EB");
                addInfoData.CategoryID = "10024001";
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Probe Mark Category
                addData = new SettingCategoryInfo();
                addData.Name = "Probe Mark";
                addData.Icon = "M87.432452,60.289979 C94.05976,60.320748 99.426756,61.539662 99.419971,63.012505 99.413182,64.485337 94.035187,65.654374 87.407875,65.623604 80.780568,65.592835 75.413574,64.373925 75.420361,62.901078 75.425026,61.888501 77.96843,61.019524 81.711151,60.585293 83.412387,60.387917 85.361419,60.280362 87.432452,60.289979 z M53.208527,50.406579 L26.696541,76.674294 114.95738,77.082535 141.46938,50.814824 z M38.844406,44.730065 L168.16104,45.328209 129.31661,83.815041 0,83.216897 z M132.52171,0 L132.47142,0.051912552 138.17574,0.10193323 151.69238,0.10193323 150.15033,12.103282 122.96538,12.103282 122.32124,12.867509 C120.31486,15.226468 118.02977,17.764889 115.5564,20.359826 106.03487,30.349343 97.328761,37.393513 96.110761,36.093422 94.892761,34.793336 101.6241,25.641295 111.14563,15.651773 114.95332,11.656939 118.6306,8.1331335 121.75073,5.4256268 L122.23155,5.0123916 122.23155,0.10193323 131.67326,0.10193323 131.79921,0.046016416 132.25911,0.050050745 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "PMI Setup";
                addInfoData.ViewGUID = new Guid("bf4d5ae0-9778-45ab-b42d-218bf162d4a6");
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Contact Setup";
                addInfoData.ViewGUID = new Guid("D13FC548-BCE1-4C4E-B1E3-413A445DB9F0");
                addInfoData.CategoryID = "10031001";
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Card Cleaning Category
                addData = new SettingCategoryInfo();
                addData.Name = "Card Cleaning";
                addData.Icon = "M61.461502,87.652668 L61.620384,88.305408 C69.46217,118.86195 92.404503,143.34182 121.997,153.27307 L122.923,153.5664 120.63595,154.32059 C91.70215,164.53784 69.341526,188.73314 61.620384,218.81958 L61.461502,219.47232 61.302628,218.81958 C53.58149,188.73314 31.220856,164.53784 2.287056,154.32059 L0,153.5664 0.9260025,153.27307 C30.518517,143.34182 53.460842,118.86195 61.302628,88.305408 z M144.9615,35.000001 L145.04929,35.36069 C149.38239,52.245565 162.05953,65.772608 178.41132,71.260403 L178.923,71.422485 177.65925,71.839237 C161.67144,77.485065 149.31573,90.854862 145.04929,107.47996 L144.9615,107.84067 144.87371,107.47996 C140.60728,90.854862 128.25157,77.485065 112.26375,71.839237 L111,71.422485 111.51168,71.260403 C127.86348,65.772608 140.54061,52.245565 144.87371,35.36069 z M92.961502,0 L93.013102,0.2120018 C95.559962,10.136293 103.01119,18.086981 112.62225,21.3125 L112.923,21.407766 112.18021,21.652716 C102.78308,24.971122 95.520779,32.829386 93.013102,42.600995 L92.961502,42.812998 92.909901,42.600995 C90.402225,32.829386 83.139927,24.971122 73.74279,21.652716 L73,21.407766 73.300747,21.3125 C82.911821,18.086981 90.363043,10.136293 92.909901,0.2120018 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Clean Sheet Recipe Setup";
                addInfoData.IsEnabled = true;
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.ViewGUID = new Guid("a4bd4bb5-030d-4644-9c3d-c21921c09eee");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Needle Brush Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Source Setup";
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.ViewGUID = new Guid("5b15248a-8999-481b-b2d9-41e4dad41959");
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Interval Setup";
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.ViewGUID = new Guid("9C13D639-847B-233D-2D69-69149A78F310");
                addInfoData.CategoryID = "10041000";
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Polish Wafer Sequence Setup";
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addInfoData.ViewGUID = new Guid("ccf581b2-92de-4374-bb38-313deca98fd9");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                //addInfoData = new SettingInfo();
                //addInfoData.Name = "SoakingView";
                //addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                //addData.SettingInfos.Add(addInfoData);


                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Result Data Category
                addData = new SettingCategoryInfo();
                addData.Name = "Result Data";
                addData.Icon = "M17.05,14.22L19.88,17.05L22,14.93V22H14.93L17.05,19.88L14.22,17.05L17.05,14.22M12.33,18H12C7.58,18 4,16.21 4,14V17C4,19.21 7.58,21 12,21C12.39,21 12.77,21 13.14,21L14.22,19.92L12.33,18M17.54,11.89L19.29,13.64C19.73,13.21 20,12.62 20,12V9C20,10.13 19.06,11.16 17.54,11.88V11.89M4,9V12C4,14.21 7.58,16 12,16H12.45L16,12.47C14.7,12.83 13.35,13 12,13C7.58,13 4,11.21 4,9M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Data Format Setup";
                addInfoData.ViewGUID = new Guid("19b2b401-17e5-4a4e-9c11-9dfc7c74b03c");
                addInfoData.CategoryID = "10051001";
                addInfoData.IsEnabled = true;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Upload Setup";
                addInfoData.ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Alarm Category
                addData = new SettingCategoryInfo();
                addData.Name = "Alarm";
                addData.Icon = "M6,6.9L3.87,4.78L5.28,3.37L7.4,5.5L6,6.9M13,1V4H11V1H13M20.13,4.78L18,6.9L16.6,5.5L18.72,3.37L20.13,4.78M4.5,10.5V12.5H1.5V10.5H4.5M19.5,10.5H22.5V12.5H19.5V10.5M6,20H18A2,2 0 0,1 20,22H4A2,2 0 0,1 6,20M12,5A6,6 0 0,1 18,11V19H6V11A6,6 0 0,1 12,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "BIN Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Yield Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Temperature Monitoring";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> LOT Category
                addData = new SettingCategoryInfo();
                addData.Name = "LOT";
                addData.Icon = "M67.083,14.951105 C55.660448,14.951104 46.400642,25.415425 46.40064,38.323817 46.400642,51.232215 55.660448,61.696532 67.083,61.696532 78.50555,61.696532 87.765356,51.232215 87.765356,38.323817 87.765356,25.415425 78.50555,14.951104 67.083,14.951105 z M69.417999,0 L153.25,0 153.25,0.05099991 153.333,0.05099991 153.333,11.808392 153.25,11.808392 153.25,14.350999 131.333,14.350999 131.333,78.051665 109.833,78.051665 109.833,14.350999 93.726305,14.350999 94.636116,15.659371 C98.926348,22.142428 101.467,30.190079 101.467,38.908332 101.467,59.362693 87.482143,76.125762 69.728518,77.652358 L69.332998,77.680686 69.332998,78.051665 17.003998,78.051665 17.003998,78.053664 0.0039978027,78.053664 0.0039978027,77.886664 0,77.886664 0,62.552661 0.0039978027,62.552661 0.0039978027,0.061999127 0.33300018,0.061999127 0.33300018,0.05099991 21.833,0.05099991 21.833,62.552661 39.80481,62.552661 39.529881,62.157294 C35.239651,55.674231 32.698997,47.626582 32.698997,38.908332 32.698997,17.448018 48.093241,0.050998781 67.083,0.05099991 67.676429,0.050998781 68.266348,0.067988057 68.852396,0.10156113 L69.417999,0.14206677 z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Retest Setup";
                addInfoData.IsEnabled = true;
                addInfoData.Visibility = Visibility.Visible;
                addInfoData.ViewGUID = new Guid("71d8a533-6b00-48f1-ae84-da3af07a308e");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Heating Setup";
                addInfoData.ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
                addInfoData.CategoryID = "00010020";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "FOUP Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Inking Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Stop Event Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "DUT Shift Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Sample Test Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Air Blow Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addInfoData.Visibility = Visibility.Hidden;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Communication Category
                addData = new SettingCategoryInfo();
                addData.Name = "Communication";
                addData.Icon = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M3,13V18L3,20H10V18H5V13H3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M14,15H20V19H14V15Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "GPIB Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "GEM Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                #region ===> Temperature Category
                addData = new SettingCategoryInfo();
                addData.Name = "Temperature";
                addData.Icon = "M16.5,5C18.05,5 19.5,5.47 20.69,6.28L19.53,9.17C18.73,8.44 17.67,8 16.5,8C14,8 12,10 12,12.5C12,15 14,17 16.5,17C17.53,17 18.47,16.66 19.23,16.08L20.37,18.93C19.24,19.61 17.92,20 16.5,20A7.5,7.5 0 0,1 9,12.5A7.5,7.5 0 0,1 16.5,5M6,3A3,3 0 0,1 9,6A3,3 0 0,1 6,9A3,3 0 0,1 3,6A3,3 0 0,1 6,3M6,5A1,1 0 0,0 5,6A1,1 0 0,0 6,7A1,1 0 0,0 7,6A1,1 0 0,0 6,5Z";

                addInfoData = new SettingInfo();
                addInfoData.Name = "Standard Temperature Setup";
                addInfoData.ViewGUID = new Guid("38059571-0235-407E-BE2F-B3CF6073034A");
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Deviation Setup";
                addInfoData.ViewGUID = new Guid("17F02446-D73D-4E02-9685-0EA1F4569E5F");
                addInfoData.CategoryID = "10090001";
                addData.SettingInfos.Add(addInfoData);

                addInfoData = new SettingInfo();
                addInfoData.Name = "Overheating Setup";
                addInfoData.ViewGUID = new Guid("00000000-0000-0000-0000-000000000000");
                addInfoData.IsEnabled = false;
                addData.SettingInfos.Add(addInfoData);

                DeviceSettingCategoryInfos.Add(addData);
                #endregion

                int i = 0;
                int j = 0;

                foreach (var deviceSettingCategoryInfo in DeviceSettingCategoryInfos)
                {
                    deviceSettingCategoryInfo.ID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, ++i, 0);
                    foreach (var deviceInfo in deviceSettingCategoryInfo.SettingInfos)
                    {
                        if (deviceInfo?.CategoryID == null)
                        {
                            deviceInfo.CategoryID = string.Format("{0:D1}{1:D3}{2:D4}", (int)SettingType.DEVICE, i, ++j);
                        }
                    }
                    j = 0;
                }

                #endregion

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // STAGE
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    //retval = MakeDefaultParamSetFor_BSCI();

                    retval = MakeDefaultParamSetFor_ALL();

                    //retval = DefaultGroupProberStage();
                }
                // LOADER
                else
                {
                   retval = DefaultGroupProberLoader();
                }

                // If ViewGUID == UnknownGUID
                // 1. IsEnable = false;
                // 2. Visibility = Visibility.Hidden

                Guid UnknownGUID = new Guid("00000000-0000-0000-0000-000000000000");

                bool CheckSuperialCategoryEnable = false;

                // SYSTEM
                foreach (var SystemCategory in SystemSettingCategoryInfos)
                {
                    CheckSuperialCategoryEnable = false;

                    foreach (var subSetting in SystemCategory.SettingInfos)
                    {
                        if(subSetting.ViewGUID == UnknownGUID)
                        {
                            subSetting.IsEnabled = false;
                            subSetting.Visibility = Visibility.Hidden;
                        }

                        if(subSetting.IsEnabled == true)
                        {
                            CheckSuperialCategoryEnable = true;
                            break;
                        }
                    }

                    if(CheckSuperialCategoryEnable == false)
                    {
                        SystemCategory.IsEnabled = false;
                    }
                }

                // DEVICE
                foreach (var DeviceCategory in DeviceSettingCategoryInfos)
                {
                    CheckSuperialCategoryEnable = false;

                    foreach (var subSetting in DeviceCategory.SettingInfos)
                    {
                        if (subSetting.ViewGUID == UnknownGUID)
                        {
                            subSetting.IsEnabled = false;
                            subSetting.Visibility = Visibility.Hidden;
                        }

                        if (subSetting.IsEnabled == true)
                        {
                            CheckSuperialCategoryEnable = true;
                            break;
                        }
                    }

                    if (CheckSuperialCategoryEnable == false)
                    {
                        DeviceCategory.IsEnabled = false;
                    }
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
