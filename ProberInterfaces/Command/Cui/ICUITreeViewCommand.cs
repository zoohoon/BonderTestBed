using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberInterfaces.Command.Cui
{
    using StringUtil;
    public class CUITreeViewCommandParam : ProbeCommandParameter
    {
        private char _MarkUpOpenChar = '[';
        private char _MarkUpCloseChar = ']';
        private char _MarkUpSplitChar = '=';
        public String UIGUID { get; set; }
        public String Text { get; set; }
        public CUITreeViewCommandParam()
        {

        }
        public CUITreeViewCommandParam(String uiguid, String text)
        {
            try
            {
            UIGUID = uiguid;
            Text = text;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public String WriteLogCmdArgument()
        {
            String guidMarkup = _MarkUpOpenChar + $"{nameof(UIGUID)}{_MarkUpSplitChar}{UIGUID}" + _MarkUpCloseChar;
            String treeViewitemMarkup = _MarkUpOpenChar + $"{nameof(Text)}{_MarkUpSplitChar}{Text}" + _MarkUpCloseChar;
            return guidMarkup + treeViewitemMarkup;
        }
        public bool ParseLogCmdArgument(String log)
        {
            List<String> markupList = StringTool.GetAllMarkup(log, _MarkUpOpenChar, _MarkUpCloseChar);

            bool isParseOk = true;
            try
            {
            foreach (String markup in markupList)
            {
                String markupContent = StringTool.GetMarkupContent(markup);
                String[] propInfo = markupContent.Split(_MarkUpSplitChar);
                if (propInfo.Count() != 2)
                    isParseOk = false;

                String propName = propInfo[0].Trim();
                String propValue = propInfo[1].Trim();

                if (propName == nameof(UIGUID))
                    UIGUID = propValue;
                else if (propName == nameof(Text))
                    Text = propValue;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return isParseOk;
        }
    }

    public interface ICUITreeViewCommand : IProbeCommand
    {
    }
}
