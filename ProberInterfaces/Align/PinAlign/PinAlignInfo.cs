using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.PinAlign
{
    using ProberErrorCode;
    using ProberInterfaces.Error;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;

    public class PinAlignInfo : IDeviceParameterizable, IParam
    {
        public string FilePath { get; } = "";

        public string FileName { get; } = "PinAlignParam.xml";

        [XmlIgnore]
        public Object OwnerModule { get; set; }
        [XmlIgnore]
        public PinAlignResultes AlignResult { get; set; }
        [XmlIgnore]
        public ObservableCollection<PinAlignDisplayData> DisplayData { get; set; }


        public PinAlignParameters PinAlignParam { get; set; }

        public PinAlignInfo()
        {
            try
            {
            AlignResult = new PinAlignResultes();
            DisplayData = new ObservableCollection<PinAlignDisplayData>();
            PinAlignParam = new PinAlignParameters();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public ErrorCodeEnum SetDefaultParam()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

            //PinAlignParam.PinAlignSettignParam = new ObservableCollection<PinAlignSettignParameter>();
            PinAlignParam.PinFocusParam = new PinFocusParameter();
            PinAlignParam.PinLowFocusParam = new FocusParameter();

            //PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter());
            //PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter());
            //PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter());
            //PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter());
            //PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter());
            //PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter());
            //PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter());

            PinAlignParam.PinLowAlignParam = new PinLowAlignParameter();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;

        }
    }
}
