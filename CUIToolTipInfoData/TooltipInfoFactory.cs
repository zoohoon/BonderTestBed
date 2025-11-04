using CUIToolTipInfoData.Model;
using ProberInterfaces;

namespace CUIToolTipInfoData
{
    public static class TooltipInfoFactory
    {
        public static IToolTipInfoBase GetTooltipInfo(ToolTipInfoType tooltipInfoType, string key, string description)
        {
            IToolTipInfoBase retBase = null;

            switch (tooltipInfoType)
            {
                case ToolTipInfoType.SIMPLE:
                    retBase = new SimpleTooltipInfo(key, description);
                    break;
                case ToolTipInfoType.IMAGE:
                    retBase = new ImageTooltipInfo(key, description);
                    break;
                case ToolTipInfoType.GIF:
                    retBase = new GifTooltipInfo(key, description);
                    break;
                case ToolTipInfoType.NONE:
                default:
                    break;
            }

            return retBase;
        }
    }
}
