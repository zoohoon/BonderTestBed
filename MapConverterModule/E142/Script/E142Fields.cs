using MapConverterModule.E142.Format;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MapConverterModule.E142.Script
{
    public interface IE142Field : IEnumerable
    {
        string Identifier { get; set; }
        Type AssignType { get; set; }
    }

    public interface IHasChild
    {
        List<ChildInfo> childInfos { get; set; }
    }

    public class ChildInfo
    {
        public object ChildEnum { get; set; }
        public List<IE142Field> Fields { get; set; }
        //public List<Type> AssignTypes { get; set; }
        public Type TargetArrayType { get; set; }

        public object GetChildEnum()
        {
            return ChildEnum;
        }

        public ChildInfo()
        {
            this.Fields = new List<IE142Field>();
            //this.AssignTypes = new List<Type>();
        }
    }

    public class LayoutField : IE142Field, IHasChild
    {
        public string Identifier { get; set; }
        public LayoutFieldType[] layoutFieldTypes { get; set; }
        public Type AssignType { get; set; }

        public List<ChildInfo> childInfos { get; set; }

        public IEnumerator GetEnumerator()
        {
            return layoutFieldTypes.GetEnumerator();
        }

        public LayoutField(string identifier)
        {
            this.Identifier = identifier;
            this.childInfos = new List<ChildInfo>();
        }
    }

    public class ChildLayoutField : IE142Field
    {
        public ChildLayoutFieldType[] childLayoutFieldTypes { get; set; }
        public string Identifier { get; set; }
        public Type AssignType { get; set; }

        public IEnumerator GetEnumerator()
        {
            return childLayoutFieldTypes.GetEnumerator();
        }

        public ChildLayoutField(string identifier)
        {
            this.Identifier = identifier;
        }
    }

    public class SubstrateField : IE142Field
    {
        public string Identifier { get; set; }

        public SubstrateFieldType[] substrateFields { get; set; }
        public Type AssignType { get; set; }


        public IEnumerator GetEnumerator()
        {
            return substrateFields.GetEnumerator();
        }
        public SubstrateField(string identifier)
        {
            this.Identifier = identifier;
        }
    }

    public class SubstrateMapField : IE142Field, IHasChild
    {
        public string Identifier { get; set; }
        public SubstrateMapFieldType[] substrateMapFields { get; set; }
        public Type AssignType { get; set; }

        public List<ChildInfo> childInfos { get; set; }
        public SubstrateMapField(string identifier)
        {
            this.Identifier = identifier;
            this.childInfos = new List<ChildInfo>();
        }

        public IEnumerator GetEnumerator()
        {
            return substrateMapFields.GetEnumerator();
        }
    }

    public class OverlayField : IE142Field, IHasChild
    {
        public OverlayFieldType[] overlayFieldTypes { get; set; }
        public List<ChildInfo> childInfos { get; set; }
        public Type AssignType { get; set; }

        public string Identifier { get; set; }

        public OverlayField(string identifier)
        {
            this.Identifier = identifier;
            this.childInfos = new List<ChildInfo>();
        }

        public IEnumerator GetEnumerator()
        {
            return overlayFieldTypes.GetEnumerator();
        }
    }

    public class ReferenceDeviceField : IE142Field
    {
        public ReferenceDeviceFieldType[] referenceDeviceFieldTypes { get; set; }
        public Type AssignType { get; set; }
        public string Identifier { get; set; }

        public IEnumerator GetEnumerator()
        {
            return referenceDeviceFieldTypes.GetEnumerator();
        }

        public ReferenceDeviceField(string identifier)
        {
            this.Identifier = identifier;
        }
    }

    public class BinCodeMapField : IE142Field, IHasChild
    {
        public BinCodeMapFieldType[] binCodeMapFieldTypes { get; set; }
        public Type AssignType { get; set; }

        public string Identifier { get; set; }
        public List<ChildInfo> childInfos { get; set; }

        public BinCodeMapField(string identifier)
        {
            this.Identifier = identifier;

            // TODO : 설정되어있는 BIN 파라미터 데이터를 기반으로 제작 됨.
            this.childInfos = new List<ChildInfo>();
        }

        public IEnumerator GetEnumerator()
        {
            return binCodeMapFieldTypes.GetEnumerator();
        }
    }

    public class BinDefinitionField : IE142Field
    {
        public BinDefinitionFieldType[] binDefinitionFieldTypes { get; set; }
        public Type AssignType { get; set; }

        public string Identifier { get; set; }

        public IEnumerator GetEnumerator()
        {
            return binDefinitionFieldTypes.GetEnumerator();
        }

        public BinDefinitionField(string identifier)
        {
            this.Identifier = identifier;
        }
    }

    public class BinCodeField : IE142Field
    {
        public Type AssignType { get; set; }
        public string Identifier { get; set; }
        public BinCodeFieldType[] binCodeFieldTypes { get; set; }

        public IEnumerator GetEnumerator()
        {
            return null;
        }

        public BinCodeField(string identifier)
        {
            this.Identifier = identifier;
        }
    }

    // TODO : DeviceIdMap to DiceMap
    // <sme:DiceMap Comment="Generated for experimenting">
    /* <sme:Die 
    X="124" => mMapCoordX
    Y="116" => mMapCoordY
    BinCode="1" => mBINMap
    ProbeCardTestSite="1" => mDutMap
    Inked="false" => X
    XWaferCenterDistance="30738" => X
    YWaferCenterDistance="87145" => X
    Overdrive="75" => X
    TestStartTime="2019-09-03T09:03:42+01:00" => X
    FailMarkInspection="false" => X
    NeedleMarkInspection="false" => X
    NeedleCleaning="false" => X
    NeedleAlign="false" => X
    >
    pass => mStatusMap
    </sme:Die>
    */

    public class DeviceIdMapField : IE142Field, IHasChild
    {
        public DeviceIdMapFieldType[] deviceMapFieldTypes { get; set; }
        public Type AssignType { get; set; }
        public string Identifier { get; set; }
        public List<ChildInfo> childInfos { get; set; }

        public DeviceIdMapField(string identifier)
        {
            this.Identifier = identifier;

            this.childInfos = new List<ChildInfo>();
        }

        public IEnumerator GetEnumerator()
        {
            return deviceMapFieldTypes.GetEnumerator();
        }
    }

    public class DeviceIdIdField : IE142Field
    {
        public DeviceIdFieldType[] deviceIdFieldTypes { get; set; }
        public Type AssignType { get; set; }
        public string Identifier { get; set; }
        public DeviceIdIdField(string identifier)
        {
            this.Identifier = identifier;
        }

        public IEnumerator GetEnumerator()
        {
            return deviceIdFieldTypes.GetEnumerator();
        }
    }
}
