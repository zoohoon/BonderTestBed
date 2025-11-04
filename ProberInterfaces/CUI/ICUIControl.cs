using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace ProberInterfaces
{
    public interface ICUIControl
    {
        Guid GUID { get; set; }
        //<remark> CuiService통해서 Setting을 하면 Param에 저장이 된다. </remark>
        //<remark> CUIService.SetMaskingLevel(cuiCon.GUID, selectMaskingValue); </remark>
        int MaskingLevel { get; set; }
        bool Lockable { get; set; }
        bool InnerLockable { get; set; }
        List<int> AvoidLockHashCodes { get; set; }
        bool IsReleaseMode { get; set; }
        BindingBase IsEnableBindingBase { get; set; }
    }

    public class MaskingItem
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public string Color { get; set; } = "Red";
        public double Opacity { get; set; } = 0.6;
        public ICUIControl SourceUI { get; set; }
    }
}
