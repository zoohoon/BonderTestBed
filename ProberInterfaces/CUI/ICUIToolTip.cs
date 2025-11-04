using System.Windows.Controls.Primitives;

namespace ProberInterfaces
{
    public enum ToolTipInfoType
    {
        NONE    = 0,
        SIMPLE  = 1,
        IMAGE   = 2,
        GIF     = 3
    }

    public interface IToolTipInfoBase
    {
        PlacementMode   PlacementMode           { get; set; }
        string          Description             { get; set; }
        string          Resource_Description    { get; set; }
        string          Resource_Title          { get; set; }
        string          TemplateName            { get; }
        string          Title                   { get; set; }
        double          Height                  { get; set; }
        double          Width                   { get; set; }
        int             Duration                { get; set; }
    }
}
