using LogModule;
using ProberInterfaces.Enum;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapView
{
    public class TextFormatWidthMetrics
    {
        public int Length { get; set; }
        public TextFormat Foramt { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public class TextFormatForBinData
    {
        public List<TextFormatWidthMetrics> Formats { get; set; }
        public float LastZoomLevel { get; set; }
        public BinType LastBinType { get; set; }
        public TextFormatWidthMetrics GetMatchedFormat(int length)
        {
            TextFormatWidthMetrics retval = null;

            try
            {
                retval = Formats.FirstOrDefault(x => x.Length == length);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public TextFormatForBinData()
        {
            Formats = new List<TextFormatWidthMetrics>();
        }
    }
}
