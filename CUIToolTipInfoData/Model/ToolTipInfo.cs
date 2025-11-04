using CUIToolTipInfoData.Core;
using Newtonsoft.Json;
using ProberInterfaces;
using System;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;

namespace CUIToolTipInfoData.Model
{
    [Serializable]
    public abstract class ToolTipInfoBase : IToolTipInfoBase
    {
        public ToolTipInfoBase()
        {
            try
            {
                this.PlacementMode = PlacementMode.Mouse;
                this.Duration = 3000;
                this.Width = 300;
                this.Height = 150;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public ToolTipInfoBase(double width, double height)
        {
            try
            {
                this.PlacementMode = PlacementMode.Mouse;
                this.Duration = 3000;
                this.Width = width;
                this.Height = height;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public abstract string TemplateName { get; }

        public string Resource_Title { get; set; }
        public string Resource_Description { get; set; }

        public int Duration { get; set; }
        public PlacementMode PlacementMode { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        [XmlIgnore, JsonIgnore]
        public string Title { get; set; }
        [XmlIgnore, JsonIgnore]
        public string Description { get; set; }
    }

    [Serializable]
    public class SimpleTooltipInfo : ToolTipInfoBase
    {
        public override string TemplateName { get => Constants.Tooltip_SimpleTemplate; }

        public SimpleTooltipInfo() : base()
        {
        }

        public SimpleTooltipInfo(string title_res, string Description_res) : base()
        {
            try
            {
                this.Resource_Title = title_res;
                this.Resource_Description = Description_res;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public SimpleTooltipInfo(string title_res, string Description_res, double width, double height) : base(width, height)
        {
            try
            {
                this.Resource_Title = title_res;
                this.Resource_Description = Description_res;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
    }

    [Serializable]
    public class ImageTooltipInfo : ToolTipInfoBase
    {
        public override string TemplateName { get => Constants.Tooltip_ImgTemplate; }

        public ImageTooltipInfo() : base()
        {

        }

        public ImageTooltipInfo(string title_res, string Description_res) : base()
        {
            try
            {
                this.Resource_Title = title_res;
                this.Resource_Description = Description_res;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public ImageTooltipInfo(string title_res, string Description_res, double width, double height) : base(width, height)
        {
            try
            {
                this.Resource_Title = title_res;
                this.Resource_Description = Description_res;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public string ImgURL { get; set; }
    }

    [Serializable]
    public class GifTooltipInfo : ToolTipInfoBase
    {
        public override string TemplateName { get => Constants.Tooltip_GifTemplate; }

        public GifTooltipInfo() : base()
        {

        }

        public GifTooltipInfo(string title_res, string Description_res) : base()
        {
            try
            {
                this.Resource_Title = title_res;
                this.Resource_Description = Description_res;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public GifTooltipInfo(string title_res, string Description_res, double width, double height) : base(width, height)
        {
            try
            {
                this.Resource_Title = title_res;
                this.Resource_Description = Description_res;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public string ImgURL { get; set; }
    }
}
