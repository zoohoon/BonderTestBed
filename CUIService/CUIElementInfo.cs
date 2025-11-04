using ProberInterfaces;
using System;

namespace CUIServices
{
    [Serializable]
    public class CUIElementInfo
    {
        public Guid CUIGUID;
        public int MaskingLevel;
        public Guid TargetViewGUID;
        public bool Visibility;
        public bool Lockable;

        public string key;
        public string Description;

        //public SimpleTooltipInfo tooltipinfo;
        public IToolTipInfoBase tooltipinfo;
    }
}
