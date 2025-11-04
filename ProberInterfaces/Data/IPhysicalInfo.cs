namespace ProberInterfaces
{
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;

    public interface IPhysicalInfo
    {
        //Element<string> CassetteID { get; set; }
        //Element<CategoryStatsBase> CatStatistics { get; set; }
        WaferCoordinate LowLeftCorner { get; set; }
        Element<double> DieSizeY { get; set; }
        Element<double> DieSizeX { get; set; }
        Element<double> FramedNotchAngle { get; set; }
        Element<int> MapCountX { get; set; }
        Element<int> MapCountY { get; set; }
  
        //Element<DateTime> LoadTime { get; set; }
        Element<MapHorDirectionEnum> MapDirX { get; set; }
        Element<MapVertDirectionEnum> MapDirY { get; set; }
        Element<double> NotchAngle { get; set; }
        Element<double> UnLoadingAngle { get; set; }
        Element<string> NotchType { get; set; }
        Element<double> NotchAngleOffset { get; set; }
        Element<NotchDriectionEnum> NotchDirection { get; set; }
        ElemMachineIndex OrgM { get; set; }
        ElemUserIndex OrgU { get; set; }
        ElemUserIndex RefU { get; set; }
        
        ElemMachineIndex CenM { get; set; }
        ElemUserIndex CenU { get; set; }

        ObservableCollection<ElemUserIndex> TargetUs { get; set; }
        CatCoordinates CenDieOffset { get; set; }
        //Element<int> SlotIndex { get; set; }
        Element<double> Thickness { get; set; }
        Element<double> ThicknessTolerance { get; set; }
        //Element<DateTime> UnloadTime { get; set; }

        //Element<string> WaferID { get; set; }
        Element<double> WaferMargin_um { get; set; }
        Element<double> WaferSize_um { get; set; }
        Element<double> WaferSize_Offset_um { get; set; }
        EnumWaferSize WaferSizeEnum { get; set; }
        Element<EnumPadType> PadType { get; set; }
        Element<double> BumpPadHeight { get; set; }
        Element<WaferSubstrateTypeEnum> WaferSubstrateType { get; set; }
        Element<OCRTypeEnum> OCRType { get; set; }
        Element<OCRDirectionEnum> OCRDirection { get; set; }
        Element<OCRModeEnum> OCRMode { get; set; }
        Element<MachineIndex> TeachDieMIndex { get; set; }
        Element<double> OCRAngle { get; set; }

    }
}
