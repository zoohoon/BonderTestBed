using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DutViewer.ViewModel
{
    using ProberInterfaces;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using SharpDXRender.RenderObjectPack;

    public class PinAlignResultDutViewerViewModel : DutViewerViewModel
    {
        private PinAlignResultes _PinAlignResult;
        private Dictionary<PINALIGNRESULT, String> _PinColorMapping;
        public PinAlignResultDutViewerViewModel(Size scopeSize, PinAlignResultes pinAlignResult)
            : base(scopeSize)
        {
            try
            {
                _PinAlignResult = pinAlignResult;
                _PinColorMapping = new Dictionary<PINALIGNRESULT, string>();
                _PinColorMapping.Add(PINALIGNRESULT.PIN_NOT_PERFORMED, "Gray");
                _PinColorMapping.Add(PINALIGNRESULT.PIN_PASSED, "Green");
                _PinColorMapping.Add(PINALIGNRESULT.PIN_FORCED_PASS, "Green");
                _PinColorMapping.Add(PINALIGNRESULT.PIN_FOCUS_FAILED, "Red");
                _PinColorMapping.Add(PINALIGNRESULT.PIN_BLOB_FAILED, "Yellow");
                _PinColorMapping.Add(PINALIGNRESULT.PIN_OVER_TOLERANCE, "Cyan");
                _PinColorMapping.Add(PINALIGNRESULT.PIN_SKIP, "Purple");

                Dictionary<RenderObject, IPinData> pinDataDic = MainRenderLayer.PinDataDic;

                foreach (EachPinResult pinResult in _PinAlignResult.EachPinResultes)
                {
                    int pinNum = pinResult.PinNum;
                    var find = pinDataDic.FirstOrDefault(item => item.Value.PinNum.Value == pinNum);
                    if (find.Equals(default(KeyValuePair<RenderObject, IPinData>)))
                        continue;

                    RenderObject renderObject = find.Key;
                    renderObject.Color = _PinColorMapping[pinResult.PinResult];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
