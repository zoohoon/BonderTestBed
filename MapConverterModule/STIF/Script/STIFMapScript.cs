using LogModule;
using ProberInterfaces.ResultMap.Script;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace MapConverterModule.STIF
{
    public class STIFScript : IMapScript
    {
        public STIFScript()
        {
            if (HighlightBrush == null)
            {
                HighlightBrush = Brushes.Magenta;
            }

            if (DefaultBrush == null)
            {
                DefaultBrush = Brushes.Gray;
            }
        }

        public SolidColorBrush HighlightBrush {get;set;}
        public SolidColorBrush DefaultBrush { get; set; }

        public List<STIFCOMPONENTTYPE> STIFComponentOrder { get; set; }
        public List<MapScriptElement> HeaderParameters { get; set; }
        public List<MapScriptElement> MapParameters { get; set; }
        public List<MapScriptElement> FooterParameters { get; set; }

        public void Add(STIFCOMPONENTTYPE type, string text, bool IsHighlight = false)
        {
            MapScriptElement tmpElement = new MapScriptElement();

            if(IsHighlight == true)
            {
                tmpElement.Background = HighlightBrush;
            }
            else
            {
                tmpElement.Background = DefaultBrush;
            }

            tmpElement.Text = text;

            try
            {
                switch (type)
                {
                    case STIFCOMPONENTTYPE.SIGNATURE:
                        // NOTHING
                        break;
                    case STIFCOMPONENTTYPE.HEADER:
                        HeaderParameters.Add(tmpElement);
                        break;
                    case STIFCOMPONENTTYPE.MAP:
                        MapParameters.Add(tmpElement);
                        break;
                    case STIFCOMPONENTTYPE.FOOTER:
                        FooterParameters.Add(tmpElement);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
