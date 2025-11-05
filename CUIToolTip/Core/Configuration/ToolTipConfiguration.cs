using System;
using System.Windows.Controls.Primitives;

namespace CUIToolTip.Core.Configuration
{
    public class ToolTipConfiguration
    {
        #region Configuration Default values

        private static readonly TimeSpan DefaultDisplayDuration = TimeSpan.FromSeconds(2);

        /// <summary>
        /// The default notifications window Width
        /// </summary>
        private const int DefaultWidth = 350;

        /// <summary>
        /// The default notifications window Height
        /// </summary>
        private const int DefaultHeight = 100;

        /// <summary>
        /// The default template of notification window
        /// </summary>
        private const string DefaultTemplateName = "tooltip_SimpleTemplate";

        #endregion

        #region ctor

        public ToolTipConfiguration(TimeSpan displayDuration, int? width, int? height, string templateName, PlacementMode? tooltipPlacementMode)
        {
            try
            {
                DisplayDuration = displayDuration;
                Width = width.HasValue ? width : DefaultWidth;
                Height = height.HasValue ? height : DefaultHeight;
                TemplateName = !string.IsNullOrEmpty(templateName) ? templateName : DefaultTemplateName;
                PlacementMode = tooltipPlacementMode ?? PlacementMode.Mouse;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        #endregion

        #region public Properties

        public static ToolTipConfiguration DefaultConfiguration
        {
            get
            {
                return new ToolTipConfiguration(
                    DefaultDisplayDuration,
                    DefaultWidth,
                    DefaultHeight,
                    DefaultTemplateName,
                    PlacementMode.Mouse);
            }
        }

        public TimeSpan DisplayDuration { get; private set; }

        public int? Width { get; set; }
        public int? Height { get; set; }

        public string TemplateName { get; private set; }

        public PlacementMode PlacementMode { get; set; }
        #endregion
    }
}
