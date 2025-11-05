using MapConverterModule.E142.Attribute;

namespace MapConverterModule.E142.Format
{
    public enum E142COMPONENTTYPE
    {
        LAYOUT,
        SUBSTRATE,
        SUBSTRATEMAP
    }

    public enum LogicalCoordinatesFieldType
    {
        X,
        Y,
    }

    public enum PhysicalCoordinatesFieldType
    {
        X,
        Y,
        UNITS
    }

    public enum LayoutFieldType
    {
        DIMENSION,
        LOWERLEFT,
        DEVICESIZE,
        STEPSIZE,
        Z,
        TOPIMAGE,
        BOTTOMIMAGE,
        PRODUCTID,
        [E142FieldAttribute(true)]
        CHILDLAYOUTS,
        LAYOUTID,
        DEFAULTUNITS,
        TOPLEVEL,
    }

    public enum ChildLayoutFieldType
    {
        LAYOUTID,
    }

    public enum SubstrateFieldType
    {
        LOTID,
        ALIASIDS,
        CARRIERTYPE,
        CARRIERID,
        SLOTNUMBER,
        SUBSTRATENUMBER,
        GOODDEVICES,
        SUPPLIERNAME,
        STATUS,
        SUBSTRATETYPE1,
        SUBSTRATEID,
    }

    public enum SubstrateMapFieldType
    {
        [E142FieldAttribute(true)]
        OVERLAY,
        SUBSTRATETYPE,
        SUBSTRATEID,
        LAYOUTSPECIFIER,
        SUBSTRATESIDE,
        ORIENTATION,
        ORIGINLOCATION,
        AXISDIRECTION,
    }

    public enum OverlayFieldType
    {
        [E142FieldAttribute(true)]
        REFERENCEDEVICES,
        [E142FieldAttribute(true)]
        ITEMS,
        //[E142FieldAttribute(true)]
        //BINCODEMAP,
        //[E142FieldAttribute(true)]
        //DEVICEIDMAP,
        TRANSFERMAP,
        MAPNAME,
        MAPVERSION,
    }

    public enum ReferenceDeviceFieldType
    {
        COORDINATES,
        POSITION,
        NAME,
    }

    public enum BinCodeMapFieldType
    {
        [E142FieldAttribute(true, true)]
        BINDEFINITIONS,
        [E142FieldAttribute(true, true)]
        BINCODE,
        BINTYPE,
        NULLBIN,
        MAPTYPE,
        MAPTYPESPECIFIED,
    }

    public enum BinDefinitionFieldType
    {
        BINCODE,
        BINCOUNT,
        BINQUALITY,
        BINDESCRIPTION,
        PICK
    }

    public enum BinCodeFieldType
    {
        X,
        Y,
        Number,
        Value,
    }

    public enum DeviceIdMapFieldType
    {
        [E142FieldAttribute(true, true)]
        DEVICEID
    }

    public enum DeviceIdFieldType
    {
        X,
        Y,
        Value,
    }

    public enum DiceFieldType
    {
        X,
        Y,
        BinCode,
        ProbeCardTestSite,
        Inked,
        XWaferCenterDistance,
        YWaferCenterDistance,
        Overdrive,
        TestStartTime,
        FailMarkInspection,
        NeedleMarkInspection,
        NeedleCleaning,
        NeedleAlign,
        Value,
    }
}
