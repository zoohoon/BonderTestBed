using LogModule;
using MapConverterModule.E142.Attribute;
using MapConverterModule.E142.Format;
using ProberErrorCode;
using ProberInterfaces.ResultMap.Script;
using System;

namespace MapConverterModule.E142.Script
{
    public class E142Scripter : MapScripterBase
    {
        public double Version { get; set; }

        public E142Scripter()
        {
            this.ScriptType = typeof(E142Script);
            this.ScriptMethodAttributeType = typeof(E142MapScriptVersionAttribute);
        }

        [E142MapScriptVersionAttribute(version: 1.1)]
        public EventCodeEnum ScriptV1_1(out IMapScript script, Type attributetype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            script = null;

            try
            {
                if (attributetype != null)
                {
                    script = new E142Script();

                    dynamic e142_script = Convert.ChangeType(script, attributetype);

                    ChildInfo childInfo = null;

                    LayoutField layoutField = null;
                    ChildLayoutField childLayoutField = null;
                    SubstrateField substrateField = null;
                    SubstrateMapField substrateMap = null;
                    OverlayField overlayField = null;
                    ReferenceDeviceField referenceDeviceField = null;
                    BinCodeMapField binCodeMapField = null;
                    BinDefinitionField binDefinitionsField = null;
                    BinCodeField binCodeField = null;
                    DeviceIdMapField deviceMapField = null;
                    DeviceIdIdField deviceIdIdField = null;

                    e142_script.mapDataField = new MapDataField();

                    #region LAYOUTS

                    // WaferLayOut
                    layoutField = new LayoutField("WaferLayout");
                    layoutField.layoutFieldTypes = new LayoutFieldType[]
                    {
                        LayoutFieldType.LAYOUTID, LayoutFieldType.DEFAULTUNITS, LayoutFieldType.TOPLEVEL,
                        LayoutFieldType.DIMENSION,
                        LayoutFieldType.DEVICESIZE,
                        LayoutFieldType.CHILDLAYOUTS, // N개
                    };

                    childLayoutField = new ChildLayoutField("ChildLayout");
                    childLayoutField.AssignType = typeof(ChildLayoutType);
                    childLayoutField.childLayoutFieldTypes = new ChildLayoutFieldType[]
                    {
                        ChildLayoutFieldType.LAYOUTID
                    };

                    childInfo = new ChildInfo();
                    childInfo.ChildEnum = LayoutFieldType.CHILDLAYOUTS;
                    //childInfo.AssignTypes[0] = typeof(ChildLayoutType);
                    childInfo.TargetArrayType = typeof(ChildLayoutType[]);
                    childInfo.Fields.Add(childLayoutField);

                    layoutField.childInfos.Add(childInfo);

                    e142_script.mapDataField.layouts.Add(layoutField);

                    // Devices
                    layoutField = new LayoutField("Devices");
                    layoutField.layoutFieldTypes = new LayoutFieldType[]
                    {
                        LayoutFieldType.LAYOUTID, LayoutFieldType.DEFAULTUNITS,
                        LayoutFieldType.DIMENSION,
                        LayoutFieldType.LOWERLEFT,
                        LayoutFieldType.DEVICESIZE,
                        LayoutFieldType.STEPSIZE,
                        LayoutFieldType.PRODUCTID,
                    };

                    e142_script.mapDataField.layouts.Add(layoutField);

                    #endregion

                    #region SUBSTRATES

                    // TODO : SubstrateType?
                    substrateField = new SubstrateField(SubstrateTypeEnum.Wafer.ToString());

                    substrateField.substrateFields = new SubstrateFieldType[]
                    {
                        SubstrateFieldType.SUBSTRATETYPE1,
                        SubstrateFieldType.SUBSTRATEID,
                        SubstrateFieldType.LOTID,
                        SubstrateFieldType.CARRIERTYPE,
                        SubstrateFieldType.CARRIERID,
                        SubstrateFieldType.SLOTNUMBER,
                        SubstrateFieldType.SUBSTRATENUMBER,
                        SubstrateFieldType.GOODDEVICES,
                        SubstrateFieldType.SUPPLIERNAME,
                        SubstrateFieldType.STATUS
                    };

                    e142_script.mapDataField.substrates.Add(substrateField);
                    #endregion

                    #region SUBSTRATEMAPS

                    substrateMap = new SubstrateMapField(SubstrateTypeEnum.Wafer.ToString());

                    substrateMap.substrateMapFields = new SubstrateMapFieldType[]
                    {
                        SubstrateMapFieldType.SUBSTRATETYPE,
                        SubstrateMapFieldType.SUBSTRATEID,
                        SubstrateMapFieldType.LAYOUTSPECIFIER,
                        SubstrateMapFieldType.SUBSTRATESIDE,
                        SubstrateMapFieldType.ORIENTATION,
                        SubstrateMapFieldType.ORIGINLOCATION,
                        SubstrateMapFieldType.AXISDIRECTION,
                        SubstrateMapFieldType.OVERLAY,
                    };

                    overlayField = new OverlayField("Overlay");
                    overlayField.AssignType = typeof(OverlayType);
                    overlayField.overlayFieldTypes = new OverlayFieldType[]
                    {
                        OverlayFieldType.MAPNAME,
                        OverlayFieldType.MAPVERSION,
                        OverlayFieldType.REFERENCEDEVICES, // N개
                        OverlayFieldType.ITEMS, // N개
                        //OverlayFieldType.BINCODEMAP,
                        //OverlayFieldType.DEVICEIDMAP,
                    };

                    referenceDeviceField = new ReferenceDeviceField("OriginLocation");
                    referenceDeviceField.AssignType = typeof(ReferenceDeviceType);
                    referenceDeviceField.referenceDeviceFieldTypes = new ReferenceDeviceFieldType[]
                    {
                        ReferenceDeviceFieldType.NAME,
                        ReferenceDeviceFieldType.COORDINATES,
                    };

                    childInfo = new ChildInfo();
                    childInfo.ChildEnum = OverlayFieldType.REFERENCEDEVICES;
                    //childInfo.AssignTypes[0] = typeof(ReferenceDeviceType);
                    childInfo.TargetArrayType = typeof(ReferenceDeviceType[]);

                    childInfo.Fields.Add(referenceDeviceField);

                    referenceDeviceField = new ReferenceDeviceField("ReferenceDie");
                    referenceDeviceField.AssignType = typeof(ReferenceDeviceType);
                    referenceDeviceField.referenceDeviceFieldTypes = new ReferenceDeviceFieldType[]
                    {
                        ReferenceDeviceFieldType.NAME,
                        ReferenceDeviceFieldType.COORDINATES,
                        ReferenceDeviceFieldType.POSITION,
                    };

                    childInfo.Fields.Add(referenceDeviceField);

                    referenceDeviceField = new ReferenceDeviceField("CenterDie");
                    referenceDeviceField.AssignType = typeof(ReferenceDeviceType);
                    referenceDeviceField.referenceDeviceFieldTypes = new ReferenceDeviceFieldType[]
                    {
                        ReferenceDeviceFieldType.NAME,
                        ReferenceDeviceFieldType.COORDINATES,
                    };

                    childInfo.Fields.Add(referenceDeviceField);

                    overlayField.childInfos.Add(childInfo);

                    binCodeMapField = new BinCodeMapField("BinCodeMap");
                    binCodeMapField.AssignType = typeof(BinCodeMapType);
                    binCodeMapField.binCodeMapFieldTypes = new BinCodeMapFieldType[]
                    {
                        BinCodeMapFieldType.BINTYPE,
                        BinCodeMapFieldType.NULLBIN,
                        BinCodeMapFieldType.BINDEFINITIONS, // N개
                        BinCodeMapFieldType.BINCODE // N개
                    };

                    childInfo = new ChildInfo();
                    childInfo.ChildEnum = OverlayFieldType.ITEMS;
                    //childInfo.AssignType = typeof(BinCodeMapType);
                    childInfo.TargetArrayType = typeof(object[]);
                    childInfo.Fields.Add(binCodeMapField);

                    // DeviceMap

                    deviceMapField = new DeviceIdMapField("DeviceMap");
                    deviceMapField.AssignType = typeof(DeviceIdMapType);
                    deviceMapField.deviceMapFieldTypes = new DeviceIdMapFieldType[]
                    {
                        DeviceIdMapFieldType.DEVICEID
                    };

                    //childInfo = new ChildInfo();
                    childInfo.ChildEnum = OverlayFieldType.ITEMS;
                    //childInfo.AssignType = typeof(DeviceIdMapType);
                    childInfo.TargetArrayType = typeof(object[]);
                    childInfo.Fields.Add(deviceMapField);

                    overlayField.childInfos.Add(childInfo);

                    #region BinDefinitionFields

                    // N개가 만들어져야 함. (Specific)
                    binDefinitionsField = new BinDefinitionField("");
                    binDefinitionsField.AssignType = typeof(BinDefinitionType);
                    binDefinitionsField.binDefinitionFieldTypes = new BinDefinitionFieldType[]
                    {
                        BinDefinitionFieldType.BINCODE,
                        BinDefinitionFieldType.BINCOUNT,
                        BinDefinitionFieldType.BINQUALITY,
                        BinDefinitionFieldType.BINDESCRIPTION,
                    };

                    childInfo = new ChildInfo();
                    childInfo.ChildEnum = BinCodeMapFieldType.BINDEFINITIONS;
                    //childInfo.AssignType = typeof(BinDefinitionType);
                    childInfo.TargetArrayType = typeof(BinDefinitionType[]);
                    childInfo.Fields.Add(binDefinitionsField);

                    // BinDefinations
                    binCodeMapField.childInfos.Add(childInfo);

                    #endregion

                    #region BinCodeFields

                    // N개가 만들어져야 함. (Specific)
                    binCodeField = new BinCodeField("");
                    binCodeField.AssignType = typeof(BinCodeType);
                    binCodeField.binCodeFieldTypes = new BinCodeFieldType[]
                    {
                        BinCodeFieldType.X,
                        BinCodeFieldType.Y,
                        BinCodeFieldType.Number,
                        BinCodeFieldType.Value,
                    };

                    childInfo = new ChildInfo();
                    childInfo.ChildEnum = BinCodeMapFieldType.BINCODE;
                    //childInfo.AssignType = typeof(BinCodeType);
                    childInfo.TargetArrayType = typeof(BinCodeType[]);
                    childInfo.Fields.Add(binCodeField);

                    // BinCodes
                    binCodeMapField.childInfos.Add(childInfo);

                    #endregion

                    #region DeviceIdField
                    
                    // N개가 만들어져야 함. (Specific)
                    deviceIdIdField = new DeviceIdIdField("");
                    deviceIdIdField.AssignType = typeof(DeviceIdType);
                    deviceIdIdField.deviceIdFieldTypes = new DeviceIdFieldType[]
                    {
                        DeviceIdFieldType.X,
                        DeviceIdFieldType.Y,
                        DeviceIdFieldType.Value,
                    };

                    childInfo = new ChildInfo();
                    childInfo.ChildEnum = DeviceIdMapFieldType.DEVICEID;
                    //childInfo.AssignType = typeof(DeviceIdType);
                    childInfo.TargetArrayType = typeof(DeviceIdType[]);
                    childInfo.Fields.Add(deviceIdIdField);

                    deviceMapField.childInfos.Add(childInfo);
                    
                    #endregion

                    childInfo = new ChildInfo();
                    childInfo.ChildEnum = SubstrateMapFieldType.OVERLAY;
                    //childInfo.AssignType = typeof(OverlayType);
                    childInfo.TargetArrayType = typeof(OverlayType[]);
                    childInfo.Fields.Add(overlayField);

                    substrateMap.childInfos.Add(childInfo);

                    // TODO : DiceMap 대신 DeviceMap으로 구현해놓기.

                    e142_script.mapDataField.substrateMaps.Add(substrateMap);

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
    }
}
