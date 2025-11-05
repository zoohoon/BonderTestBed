using System;
using System.Linq;

namespace ProberInterfaces.Command.Cui
{
    using StringUtil;
    public class CUIButtonCommandParam : ProbeCommandParameter
    {
        private char _MarkUpOpenChar = '[';
        private char _MarkUpCloseChar = ']';
        private char _MarkUpSplitChar = '=';
        public String UIGUID { get; set; }
        public CUIButtonCommandParam()
        {

        }

        public CUIButtonCommandParam(String uiguid)
        {
            UIGUID = uiguid;
        }

        public String WriteLogCmdArgument()
        {
            return _MarkUpOpenChar + $"{nameof(UIGUID)}{_MarkUpSplitChar}{UIGUID}" + _MarkUpCloseChar;
        }
        public bool ParseLogCmdArgument(String log)
        {
            String markup = StringTool.GetMarkup(log, _MarkUpOpenChar, _MarkUpCloseChar);
            String markupContent = StringTool.GetMarkupContent(markup);
            String[] propInfo = markupContent.Split(_MarkUpSplitChar);
            if (propInfo.Count() != 2)
                return false;

            String propName = propInfo[0].Trim();
            String propValue = propInfo[1].Trim();

            UIGUID = propValue;
            return true;
        }
    }

    public interface ICUIButtonCommand : IProbeCommand
    {
    }
}
