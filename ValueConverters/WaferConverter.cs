using System;

namespace ValueConverters
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using LoaderParameters;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.SequenceRunner;
    using ProberInterfaces.CardChange;

    public abstract class ModuleInfoToIconConverter : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        protected String GetLoadImageSource(ModuleTypeEnum moduleType)
        {
            string imagePath = String.Empty;

            try
            {
                switch (moduleType)
                {
                    case ModuleTypeEnum.CST:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/8InchCST.png";
                        break;
                    case ModuleTypeEnum.ARM:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/ARM1_W.png";
                        break;
                    case ModuleTypeEnum.PA:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/PreAligner_W.png";
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_W.png";
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_W.png";
                        break;
                    case ModuleTypeEnum.CHUCK:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Chuck_W.png";
                        break;
                    default:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Minus.png";
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return imagePath;
        }
        protected String GetUnloadImageSource(ModuleTypeEnum moduleType)
        {
            string imagePath = String.Empty;

            try
            {
                switch (moduleType)
                {
                    case ModuleTypeEnum.CST:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/8InchCST.png";
                        break;
                    case ModuleTypeEnum.ARM:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/ARM1_NoWafer.png";
                        break;
                    case ModuleTypeEnum.PA:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/PreAligner_NoWafer.png";
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_NoWafer.png";
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_NoWafer.png";
                        break;
                    case ModuleTypeEnum.CHUCK:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Chuck_NoWafer.png";
                        break;
                    default:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Minus.png";
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return imagePath;
        }
    }

    public class CassetteModuleInfoToCassetteIconConverter : ModuleInfoToIconConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String imageSource = String.Empty;

            try
            {
                CassetteModuleInfo cassetteModuleInfo = value as CassetteModuleInfo;
                if (cassetteModuleInfo == null)
                    return null;

                if (cassetteModuleInfo.ScanState == CassetteScanStateEnum.READ)
                    imageSource = GetLoadImageSource(cassetteModuleInfo.ID.ModuleType);
                else
                    imageSource = GetUnloadImageSource(cassetteModuleInfo.ID.ModuleType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return imageSource;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HolderModuleInfoToWaferHandleIconConverter : ModuleInfoToIconConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String imageSource = String.Empty;

            try
            {
                HolderModuleInfo holderModuleInfo = value as HolderModuleInfo;
                if (holderModuleInfo == null)
                    return null;


                if (holderModuleInfo.WaferStatus == EnumSubsStatus.EXIST || holderModuleInfo.WaferStatus == EnumSubsStatus.UNKNOWN)
                    imageSource = GetLoadImageSource(holderModuleInfo.ID.ModuleType);
                else
                    imageSource = GetUnloadImageSource(holderModuleInfo.ID.ModuleType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return imageSource;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class CommandButtonEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEnable = false;
            try
            {
                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum moduleState = (ModuleStateEnum)value;

                    isEnable =
                        moduleState == ModuleStateEnum.IDLE ||
                        moduleState == ModuleStateEnum.PAUSED ||
                        moduleState == ModuleStateEnum.ERROR ||
                        moduleState == ModuleStateEnum.RECOVERY;
                    //isEnable = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return isEnable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class EnumGPLoaderWaferStatusTextColorConverter : IMultiValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Cyanbrush = new SolidColorBrush(Colors.Cyan);
        static SolidColorBrush LightSeaGreenbrush = new SolidColorBrush(Colors.LightSeaGreen);

        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                if (values[0] is EnumSubsStatus == false)
                {
                    retval = Redbrush;
                    return retval;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                EnumWaferState param = (EnumWaferState)values[1];

                switch (val)
                {
                    case EnumSubsStatus.EXIST:
                        {
                            if (param == EnumWaferState.PROCESSED)
                            {
                                return Cyanbrush;
                            }
                            else if (param == EnumWaferState.SKIPPED)
                            {
                                return Yellowbrush;
                            }
                            else if (param == EnumWaferState.TESTED)
                            {
                                return LightSeaGreenbrush;
                            }
                            else
                            {
                                return LimeGreenbrush;
                            }
                        }
                    case EnumSubsStatus.UNKNOWN:
                        return Redbrush;
                    case EnumSubsStatus.NOT_EXIST:
                        return DimGraybrush;
                    case EnumSubsStatus.UNDEFINED:
                        return DimGraybrush;
                }

                retval = DimGraybrush;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumGPLoaderWaferStatusandTypeColorConverter : IMultiValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Puplebrush = new SolidColorBrush(Colors.Purple);
        static SolidColorBrush Cyanbrush = new SolidColorBrush(Colors.Cyan);
        static SolidColorBrush LightSeaGreenbrush = new SolidColorBrush(Colors.LightSeaGreen);

        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                if (values[0] is EnumSubsStatus == false)
                {
                    retval = Redbrush;
                    return retval;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                EnumWaferState param = (EnumWaferState)values[1];
                TransferObject WaferObject = (TransferObject)values[2];

                switch (val)
                {
                    case EnumSubsStatus.EXIST:
                        {
                            if (WaferObject != null && WaferObject.WaferType.Value == EnumWaferType.TCW &&
                                param != EnumWaferState.SKIPPED)
                            {
                                return Puplebrush;
                            }
                            else if (param == EnumWaferState.PROCESSED)
                            {
                                return Cyanbrush;
                            }
                            else if (param == EnumWaferState.SKIPPED)
                            {
                                return Yellowbrush;
                            }
                            else if (param == EnumWaferState.TESTED)
                            {
                                return LightSeaGreenbrush;
                            }
                            else
                            {
                                return LimeGreenbrush;
                            }
                        }
                    case EnumSubsStatus.UNKNOWN:
                        return Redbrush;
                    case EnumSubsStatus.NOT_EXIST:
                        return DimGraybrush;
                    case EnumSubsStatus.UNDEFINED:
                        return DimGraybrush;
                }

                retval = DimGraybrush;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class EnumWaferStatusTextColorConverter : IMultiValueConverter
    {
        static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                if (values[0] is EnumSubsStatus == false)
                    return Blackbrush;

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                ModuleTypeEnum param = (ModuleTypeEnum)values[1];

                switch (val)
                {
                    case EnumSubsStatus.EXIST:
                        retval = Blackbrush;
                        //return Blackbrush;
                        break;
                    case EnumSubsStatus.UNKNOWN:
                        retval = Redbrush;
                        //return Redbrush;
                        break;
                    case EnumSubsStatus.NOT_EXIST:
                        {
                            if (param == ModuleTypeEnum.SLOT || param == ModuleTypeEnum.UNDEFINED)
                            {
                                retval = Whitebrush;
                                //return Whitebrush;
                            }
                            else
                            {
                                retval = Blackbrush;
                                //return Blackbrush;
                            }
                        }
                        break;
                    case EnumSubsStatus.UNDEFINED:
                        retval = Whitebrush;
                        //return Whitebrush;
                        break;
                }

                //retval = Blackbrush;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class EnumSlotTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (values[0] is EnumSubsStatus == false)
                    return "null";

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                EnumWaferState param = (EnumWaferState)values[1];

                switch (val)
                {
                    case EnumSubsStatus.EXIST:
                        if (param == EnumWaferState.SKIPPED)
                        {
                            retval = param.ToString();
                            //return param;
                        }
                        else if (param == EnumWaferState.PROCESSED)
                        {
                            retval = param.ToString();
                            //return param;
                        }

                        retval = val.ToString();
                        //return val;
                        break;
                    case EnumSubsStatus.UNKNOWN:
                        retval = val.ToString();
                        //return val;
                        break;
                    case EnumSubsStatus.NOT_EXIST:
                        retval = val.ToString();
                        //return val;
                        break;
                    case EnumSubsStatus.UNDEFINED:
                        retval = val.ToString();
                        //return val;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumSlotTextConverter1 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (values[0] is EnumSubsStatus == false)
                    return "null";

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                EnumWaferState param = (EnumWaferState)values[1];
                TransferObject Obj = (TransferObject)values[2];

                switch (val)
                {
                    case EnumSubsStatus.EXIST:

                        if (param == EnumWaferState.SKIPPED)
                        {
                            retval = param.ToString();
                            //return param;
                        }
                        else if (Obj != null)
                        {
                            string processedStr = "";
                            if (param == EnumWaferState.PROCESSED || param == EnumWaferState.TESTED)
                            {
                                if (Obj.ProcessCellIndex >= 10)
                                {
                                    processedStr = $" / C{Obj.ProcessCellIndex}";
                                }
                                else if (Obj.ProcessCellIndex >= 1)
                                {
                                    processedStr = $" / C0{Obj.ProcessCellIndex}";
                                }
                            }

                            if (Obj.CurrHolder == Obj.OriginHolder)
                            {
                                if (Obj.DoPMIFlag)
                                {
                                    return param + "(PMI)" + processedStr;
                                }
                                else
                                {
                                    return param + processedStr;
                                }
                            }
                            else if (Obj.CurrHolder.ModuleType == ModuleTypeEnum.SLOT && Obj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                return param + processedStr;
                            }
                            else if (Obj.CurrHolder.ModuleType == ModuleTypeEnum.SLOT && Obj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                return param + processedStr;
                            }
                            else
                            {
                                if (Obj.DoPMIFlag)
                                {
                                    return Obj.CurrHolder.ToString() + "(PMI)" + processedStr;
                                }
                                else
                                {
                                    return Obj.CurrHolder.ToString() + processedStr;
                                }
                            }
                        }
                        return val;
                    case EnumSubsStatus.UNKNOWN:
                        return val;
                    case EnumSubsStatus.NOT_EXIST:
                        return val;
                    case EnumSubsStatus.UNDEFINED:
                        return val;
                }

                return val;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class EnumSlotTextConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = null;
            try
            {
                if (values[0] is EnumSubsStatus == false)
                    return "null";

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                EnumWaferState param = (EnumWaferState)values[1];
                TransferObject Obj = (TransferObject)values[2];
                switch (val)
                {
                    case EnumSubsStatus.EXIST:
                        if (param == EnumWaferState.SKIPPED)
                        {
                            return param;
                        }
                        else if (Obj != null)
                        {
                            string processedStr = null;
                            if (Obj.OCR.Value == "")
                            {
                                processedStr = "";
                            }
                            else
                            {
                                processedStr = "| " + Obj.OCR.Value + " ";
                            }
                            if (param == EnumWaferState.PROCESSED || param == EnumWaferState.TESTED)
                            {
                                if (Obj.ProcessCellIndex >= 10)
                                {
                                    processedStr += $" / C{Obj.ProcessCellIndex}";
                                }
                                else if (Obj.ProcessCellIndex >= 1)
                                {
                                    processedStr += $" / C0{Obj.ProcessCellIndex}";
                                }
                            }
                            string msg = null;
                            if (Obj.CurrHolder == Obj.OriginHolder)
                            {
                                msg = String.Format("{0,-10}  {1,-10}", param, processedStr);
                                return msg;
                            }
                            else
                            {
                                msg = String.Format("{0,-15}  {1,-10}", Obj.CurrHolder.ToString(), processedStr);
                                return msg;
                            }
                        }

                        retval = val.ToString();
                        //return val;

                        break;
                    case EnumSubsStatus.UNKNOWN:
                        retval = val.ToString();
                        //return val;
                        break;
                    case EnumSubsStatus.NOT_EXIST:
                        retval = val.ToString();
                        //return val;
                        break;
                    case EnumSubsStatus.UNDEFINED:
                        retval = val.ToString();
                        //return val;
                        break;
                }

                //return val;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class EnumWaferStatusTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (!(values[0] is EnumSubsStatus) && !(values[1] is int))
                {
                    return "null";
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                int number = (int)values[1];

                switch (val)
                {
                    case EnumSubsStatus.EXIST:
                        retval = "EXIST #" + number;
                        //return "EXIST #" + number;
                        break;
                    case EnumSubsStatus.UNKNOWN:
                        retval = val.ToString();
                        //return val;
                        break;
                    case EnumSubsStatus.NOT_EXIST:
                        retval = val.ToString();
                        //return val;
                        break;
                    case EnumSubsStatus.UNDEFINED:
                        retval = val.ToString();
                        //return val;
                        break;
                }

                //return val;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumWaferStatusEnableTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (!(values[0] is EnumSubsStatus) && !(values[1] is bool))
                {
                    return "null";
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                bool enable = (bool)values[1];


                if (enable)
                {
                    return val;
                }
                return "DISABLE";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumStageWaferStatusTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (!(values[0] is EnumSubsStatus))
                {
                    retval = "null";
                    return retval;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];

                TransferObject wafer = null;

                if (values[2] is TransferObject)
                {
                    wafer = (TransferObject)values[2];
                }

                switch (val)
                {
                    case EnumSubsStatus.EXIST:

                        if (wafer != null)
                        {
                            string WAFERID = null;

                            if (wafer.WaferType.Value == EnumWaferType.STANDARD)
                            {
                                if (wafer.OCR?.Value != null)
                                {
                                    WAFERID = wafer.OCR.Value.Replace("_", "__");
                                    retval = "WAFER : " + WAFERID;

                                }

                            }
                            else if (wafer.WaferType.Value == EnumWaferType.POLISH)
                            {
                                if (string.IsNullOrEmpty(wafer.OCR?.Value) == false)
                                {
                                    var ocrid = wafer.OCR.Value.Replace("_", "__");
                                    var type = wafer.PolishWaferInfo?.DefineName?.Value?.Replace("_", "__");

                                    WAFERID = ocrid + " (" + type + ")";
                                }
                                else
                                {
                                    var type = wafer.PolishWaferInfo?.DefineName?.Value?.Replace("_", "__");

                                    WAFERID = "(" + type + ")";
                                }

                                retval = "WAFER : " + WAFERID;
                            }
                            else
                            {
                                retval = "WAFER : " + val;
                            }
                        }
                        else
                        {
                            retval = "WAFER : " + val;
                        }
                        break;
                    case EnumSubsStatus.UNKNOWN:
                    case EnumSubsStatus.NOT_EXIST:
                    case EnumSubsStatus.UNDEFINED:
                    case EnumSubsStatus.HIDDEN:
                        return "WAFER : " + val;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class Stage_OD_TextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                //if (!(values[0] is EnumSubsStatus) && !(values[1] is int))
                //{
                //    return "null";
                //}

                if (!(values[0] is string))
                {
                    retval = "";
                    return retval;
                }
                if (!(values[1] is string))
                {
                    retval = "";
                    return retval;
                }

                string od = (string)values[0];
                string clearlance = (string)values[1];
                retval = $"OD: {od} , Clearance: {clearlance} ";
                //int number = (int)values[1];



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class EnumStageCardStatusTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                //if (!(values[0] is EnumSubsStatus) && !(values[1] is int))
                //{
                //    retval = "null";
                //    return retval;
                //}

                if (!(values[0] is EnumSubsStatus))
                {
                    retval = "null";
                    return retval;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                //int number = (int)values[1];

                TransferObject card = null;

                if (values[2] is TransferObject)
                {
                    card = (TransferObject)values[2];
                }

                switch (val)
                {
                    case EnumSubsStatus.EXIST:

                        if (card != null && card.ProbeCardID.Value != null)
                        {
                            if ((card.ProbeCardID.Value == string.Empty || card.ProbeCardID.Value == null)
                                && card.WaferState == EnumWaferState.READY)
                            {
                                retval = "C : " + "READY";
                            }
                            else if ((card.ProbeCardID.Value == string.Empty || card.ProbeCardID.Value == null) && card.WaferState == EnumWaferState.PROCESSED)
                            {
                                retval = "HOLDER";
                            }
                            else
                            {
                                string CardID = card.ProbeCardID.Value.Replace("_", "__");
                                retval = "C : " + CardID;//#" + number;
                            }

                        }
                        else
                        {
                            retval = "C : " + val;
                        }
                        break;

                    case EnumSubsStatus.UNKNOWN:
                    case EnumSubsStatus.NOT_EXIST:
                    case EnumSubsStatus.UNDEFINED:
                    case EnumSubsStatus.HIDDEN:
                        retval = val.ToString();
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumStageCardStatusTextConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (!(values[0] is EnumSubsStatus))
                {
                    retval = "null";
                    return retval;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];

                TransferObject card = null;

                if (values[1] is TransferObject)
                {
                    card = (TransferObject)values[1];
                }

                switch (val)
                {
                    case EnumSubsStatus.EXIST:

                        if (card != null)
                        {
                            if (card.ProbeCardID.Value == null || card.ProbeCardID.Value == "")
                            {
                                retval = val.ToString();
                            }
                            else
                            {
                                retval = card.ProbeCardID.Value;//#" + number;
                            }
                        }
                        else
                        {
                            retval = val.ToString();
                        }

                        break;

                    case EnumSubsStatus.UNKNOWN:
                    case EnumSubsStatus.NOT_EXIST:
                    case EnumSubsStatus.UNDEFINED:
                    case EnumSubsStatus.HIDDEN:
                        retval = val.ToString();
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// MD Load Main 화면에서 사용되는 converter
    /// </summary>
    public class EnumStageWaferStatusTextConverter1 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (!(values[0] is EnumSubsStatus))
                {
                    retval = "null";
                    return retval;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                TransferObject wafer = null;
                if (values[2] is TransferObject)
                {
                    wafer = (TransferObject)values[2];
                }

                switch (val)
                {
                    case EnumSubsStatus.EXIST:

                        if (wafer != null)
                        {
                            if (wafer.PolishWaferInfo != null)
                            {
                                retval = wafer.PolishWaferInfo.DefineName.Value;
                            }
                            else
                            {
                                retval = wafer.OCR.Value;
                            }
                        }
                        else
                        {
                            retval = val.ToString();
                        }
                        break;
                    case EnumSubsStatus.UNKNOWN:
                    case EnumSubsStatus.NOT_EXIST:
                    case EnumSubsStatus.UNDEFINED:
                    case EnumSubsStatus.HIDDEN:
                        retval = val.ToString();
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumStageCardStatusTextConverter1 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                //if (!(values[0] is EnumSubsStatus) && !(values[1] is int))
                //{
                //    retval = "null";
                //    return retval;
                //}

                if (!(values[0] is EnumSubsStatus))
                {
                    retval = "null";
                    return retval;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                //int number = (int)values[1];

                TransferObject card = null;

                if (values[2] is TransferObject)
                {
                    card = (TransferObject)values[2];
                }

                switch (val)
                {
                    case EnumSubsStatus.EXIST:

                        if (card != null)
                        {
                            retval = card.ProbeCardID.Value;//#" + number;
                        }
                        else
                        {
                            retval = val.ToString();
                        }

                        break;

                    case EnumSubsStatus.UNKNOWN:
                    case EnumSubsStatus.NOT_EXIST:
                    case EnumSubsStatus.UNDEFINED:
                    case EnumSubsStatus.HIDDEN:
                        retval = val.ToString();
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumStageCardStateBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            var brush = Brushes.Gray;

            try
            {
                //if (!(values[0] is EnumSubsStatus) && !(values[1] is int))
                //{
                //    retval = "null";
                //    return retval;
                //}

                if (!(values[0] is EnumSubsStatus) || !(values[1] is EnumWaferState))
                {
                    brush = Brushes.Gray;
                    return brush;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                if (!(val == EnumSubsStatus.EXIST))
                {
                    return Brushes.Gray;
                }
                if (val == EnumSubsStatus.CARRIER)
                {
                    return Brushes.MediumPurple;
                }
                EnumWaferState val1 = (EnumWaferState)values[1];

                switch (val1)
                {
                    case EnumWaferState.PROCESSED:
                        brush = Brushes.Cyan;
                        break;
                    case EnumWaferState.READY:
                        brush = Brushes.LimeGreen;
                        break;
                    case EnumWaferState.UNPROCESSED:
                        brush = Brushes.Yellow;
                        break;
                    default:
                        brush = Brushes.Red;
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumCardBufferStateBrushConverter : IMultiValueConverter
    {
        static SolidColorBrush Green = new SolidColorBrush(Colors.Green);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Blue = new SolidColorBrush(Colors.Blue);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Cyanbrush = new SolidColorBrush(Colors.Cyan);
        static SolidColorBrush Purplebrush = new SolidColorBrush(Colors.MediumPurple);

        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = DimGraybrush;

            try
            {

                if (!(values[0] is EnumSubsStatus) || !(values[1] is CardPRESENCEStateEnum))
                {
                    brush = DimGraybrush;
                    return brush;
                }

                EnumSubsStatus status = (EnumSubsStatus)values[0];
                switch (status)
                {
                    case EnumSubsStatus.UNKNOWN:
                        brush = DarkSlateGraybrush;
                        break;
                    case EnumSubsStatus.UNDEFINED:
                        brush = DimGraybrush;
                        break;
                    case EnumSubsStatus.NOT_EXIST:
                        brush = DimGraybrush;
                        break;
                    case EnumSubsStatus.EXIST:
                        brush = LimeGreenbrush;
                        break;
                    case EnumSubsStatus.CARRIER:
                        brush = Purplebrush;
                        break;
                    default:
                        brush = DimGraybrush;
                        break;
                }


                CardPRESENCEStateEnum cardPRESENCEState = (CardPRESENCEStateEnum)values[1];
                if (status == EnumSubsStatus.EXIST)
                {
                    if (cardPRESENCEState == ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_ATTACH)
                    {
                        brush = LimeGreenbrush;
                    }
                    else if (cardPRESENCEState == ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_DETACH)
                    {
                        brush = Yellowbrush;
                    }
                    else
                    {
                        brush = Redbrush;
                    }
                }
                else
                {
                    if (cardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                    {
                        brush = Redbrush;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ScanStateValueConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is CassetteScanStateEnum)
                {
                    CassetteScanStateEnum status = (CassetteScanStateEnum)value;
                    switch (status)
                    {
                        case CassetteScanStateEnum.NONE:
                            return DimGraybrush;
                        case CassetteScanStateEnum.READ:
                            return LimeGreenbrush;
                        case CassetteScanStateEnum.READING:
                            return Yellowbrush;
                        case CassetteScanStateEnum.ILLEGAL:
                            return Redbrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return DarkSlateGraybrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SubsStatusToValueConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);
        static SolidColorBrush Purplebrush = new SolidColorBrush(Colors.MediumPurple);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is EnumSubsStatus)
                {
                    EnumSubsStatus status = (EnumSubsStatus)value;
                    switch (status)
                    {
                        case EnumSubsStatus.UNKNOWN:
                            return Redbrush;
                        case EnumSubsStatus.UNDEFINED:
                            return DimGraybrush;
                        case EnumSubsStatus.NOT_EXIST:
                            return DimGraybrush;
                        case EnumSubsStatus.EXIST:
                            return LimeGreenbrush;
                        case EnumSubsStatus.CARRIER:
                            return Purplebrush;
                        default:
                            return DimGraybrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return DarkSlateGraybrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardBufferTrayStatusToValueConverter : IMultiValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);
        static SolidColorBrush Purplebrush = new SolidColorBrush(Colors.MediumPurple);
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (!(values[0] is EnumSubsStatus) || !(values[1] is bool))
                {
                    return Brushes.Gray;
                }

                EnumSubsStatus value1 = (EnumSubsStatus)values[0];
                switch (value1)
                {
                    case EnumSubsStatus.UNKNOWN:
                        return Redbrush;
                    case EnumSubsStatus.UNDEFINED:
                        return DimGraybrush;
                    case EnumSubsStatus.NOT_EXIST:
                        return DimGraybrush;
                    case EnumSubsStatus.EXIST:
                        bool value2 = (bool)values[1];
                        if (value2 == true)
                        {
                            return LimeGreenbrush;
                        }
                        return Yellowbrush;
                    case EnumSubsStatus.CARRIER:
                        return Purplebrush;
                    default:
                        return DimGraybrush;
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return DarkSlateGraybrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class CardBufferTrayStatusTostringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (!(values[0] is EnumSubsStatus) || !(values[1] is bool))
                {
                    return values[0];
                }

                EnumSubsStatus value1 = (EnumSubsStatus)values[0];
                switch (value1)
                {
                    case EnumSubsStatus.UNKNOWN:
                        return EnumSubsStatus.UNKNOWN.ToString();
                    case EnumSubsStatus.UNDEFINED:
                        return EnumSubsStatus.UNDEFINED.ToString();
                    case EnumSubsStatus.NOT_EXIST:
                        return EnumSubsStatus.NOT_EXIST.ToString();
                    case EnumSubsStatus.EXIST:
                        bool value2 = (bool)values[1];
                        if (value2 == true)
                        {
                            return EnumSubsStatus.EXIST.ToString();
                        }
                        return "Holder";
                    case EnumSubsStatus.CARRIER:
                        return EnumSubsStatus.CARRIER.ToString();
                    default:
                        return values[0];
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class AlignStateStrToValueConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Orangebrush = new SolidColorBrush(Colors.Orange);


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string)
                {
                    string str = value as string;
                    if (str.Equals("DONE"))
                    {
                        return LimeGreenbrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Orangebrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleStateToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                if (values[1] is int && values[2] is int)
                {
                    if (System.Convert.ToInt32(values[1]) == System.Convert.ToInt32(values[2]) + 1)
                    {
                        strPos = false;
                    }
                    else
                    {
                        if (values[0] is SequenceBehaviorStateEnum)
                        {
                            SequenceBehaviorStateEnum val = (SequenceBehaviorStateEnum)values[0];

                            if (val == SequenceBehaviorStateEnum.IDLE)
                            {
                                strPos = false;
                            }
                            else if (val == SequenceBehaviorStateEnum.PAUSED)
                            {
                                strPos = false;
                            }
                            else if (val == SequenceBehaviorStateEnum.RUNNING)
                            {
                                strPos = true;
                            }
                            else
                            {
                                strPos = false;
                            }
                        }
                        else
                        {
                            strPos = false;
                        }
                    }
                }
                else
                {
                    strPos = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"ModuleStateToBoolConverter(): Error occurred. Err = {err.Message}");
                return false;
            }
            return strPos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ModuleStateToBoolConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                if (values[0] is SequenceBehaviorStateEnum)
                {
                    SequenceBehaviorStateEnum val = (SequenceBehaviorStateEnum)values[0];

                    if (val == SequenceBehaviorStateEnum.IDLE)
                    {
                        strPos = false;
                    }
                    else if (val == SequenceBehaviorStateEnum.PAUSED)
                    {
                        strPos = true;
                    }
                    else if (val == SequenceBehaviorStateEnum.ERROR)
                    {
                        strPos = true;
                    }
                    else
                    {
                        strPos = false;
                    }
                }
                else
                {
                    strPos = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ModuleStateToBoolConverter2(): Error occurred. Err = {err.Message}");
                return false;
            }
            return strPos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class ModuleStateToBoolConverter3 : IMultiValueConverter
    {
        //Manual버튼 돌고있는 behavior가 Running인경우 Disable, Idle이나 다른 상태라면 Enable
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                if (values[0] is SequenceBehaviorStateEnum && values[1] is SequenceBehaviorStateEnum)
                {
                    SequenceBehaviorStateEnum val1 = (SequenceBehaviorStateEnum)values[0];
                    SequenceBehaviorStateEnum val2 = (SequenceBehaviorStateEnum)values[1];

                    if (val1 != SequenceBehaviorStateEnum.RUNNING && val2 != SequenceBehaviorStateEnum.RUNNING)
                    {
                        strPos = true;
                    }
                    else
                    {
                        strPos = false;
                    }
                }
                else
                {
                    strPos = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ModuleStateToBoolConverter3(): Error occurred. Err = {err.Message}");
                return false;
            }
            return strPos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    //public class CardStatusToValueConverter2 : IValueConverter
    //{
    //    static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
    //    static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
    //    static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
    //    static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
    //    static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
    //    static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        try
    //        {
    //            if (value is CardHolderStatus)
    //            {
    //                CardHolderStatus status = (CardHolderStatus)value;
    //                switch (status.Status)
    //                {
    //                    case EnumSubsStatus.UNKNOWN:
    //                        return DarkSlateGraybrush;
    //                        break;
    //                    case EnumSubsStatus.UNDEFINED:
    //                        return DarkSlateGraybrush;
    //                        break;
    //                    case EnumSubsStatus.NOT_EXIST:
    //                        return DimGraybrush;
    //                        break;
    //                    case EnumSubsStatus.EXIST:
    //                        return LimeGreenbrush;
    //                        break;
    //                    default:
    //                        return DimGraybrush;
    //                        break;
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return DarkSlateGraybrush;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class SubstatusEnableColorConverter : IMultiValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            var brush = Brushes.Gray;

            try
            {
                if (values.Length >= 1)
                {
                    if (values[0] is ProberInterfaces.PreAligner.EnumPAStatus) //PA
                    {
                        ProberInterfaces.PreAligner.EnumPAStatus enumPAStatus = (ProberInterfaces.PreAligner.EnumPAStatus)values[0];
                        if (enumPAStatus == ProberInterfaces.PreAligner.EnumPAStatus.Error)
                        {
                            return Brushes.Red;
                        }
                        else
                        {
                            return Brushes.Gray;
                        }
                    }
                    else //EnumSubsStatus stage 등
                    {
                        if (values[0] is EnumSubsStatus && values.Length > 1)
                        {
                            bool enable = (bool)values[1];
                            if (!enable)
                            {
                                return Brushes.Red;
                            }
                            EnumSubsStatus val = (EnumSubsStatus)values[0];
                            switch (val)
                            {
                                case EnumSubsStatus.UNKNOWN:
                                    return Redbrush;
                                case EnumSubsStatus.UNDEFINED:
                                    return DarkSlateGraybrush;
                                case EnumSubsStatus.NOT_EXIST:
                                    return DimGraybrush;
                                case EnumSubsStatus.EXIST:
                                    return LimeGreenbrush;
                                default:
                                    return DimGraybrush;
                            }
                        }
                        else
                        {
                            return Brushes.Gray;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class WaferStatusToValueConverter : IMultiValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);
        static SolidColorBrush Purplebrush = new SolidColorBrush(Colors.MediumPurple);
        static SolidColorBrush Cyanbrush = new SolidColorBrush(Colors.Cyan);
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool iswaferholdhandler = false;
                if (values.Length > 1 && (bool)values[1] == false)
                    return DimGraybrush;

                if (values.Length >= 3)//GOP, 베르누이 헨들러 장비일 때
                {
                    if ((bool)values[2] == true) 
                    {
                        iswaferholdhandler = true;
                    }
                }

                if (values[0] is EnumSubsStatus)
                {
                    EnumSubsStatus status = (EnumSubsStatus)values[0];
                    switch (status)
                    {
                        case EnumSubsStatus.UNKNOWN:
                            return Redbrush;
                        case EnumSubsStatus.UNDEFINED:
                            return DimGraybrush;
                        case EnumSubsStatus.NOT_EXIST:
                            return DimGraybrush;
                        case EnumSubsStatus.EXIST:
                            if (iswaferholdhandler == true)
                            {
                                return Cyanbrush;
                            }
                            return LimeGreenbrush;
                        case EnumSubsStatus.CARRIER:
                            return Purplebrush;
                        default:
                            return DimGraybrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return DarkSlateGraybrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw null;
        }
    }
    public class CardStatusToValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            var brush = Brushes.Gray;

            try
            {
                if (values.Length > 2 && (bool)values[2] == false)
                {
                    return Brushes.Gray;
                }

                if (!(values[0] is EnumSubsStatus) || !(values[1] is EnumWaferState))
                {
                    return Brushes.Gray;
                }

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                if (!(val == EnumSubsStatus.EXIST))
                {
                    return Brushes.Gray;
                }
                EnumWaferState val1 = (EnumWaferState)values[1];

                switch (val1)
                {
                    case EnumWaferState.PROCESSED:
                        brush = Brushes.Cyan;
                        break;
                    case EnumWaferState.READY:
                        brush = Brushes.LimeGreen;
                        break;
                    case EnumWaferState.UNPROCESSED:
                        brush = Brushes.Yellow;
                        break;
                    default:
                        brush = Brushes.Red;
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class TimeVisibiltyConvert : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool enable = (bool)value;
            if (enable)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridWidthConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        //todo
                    }
                    else
                    {
                        if ((string)value == "FAIL")
                        {
                            return 40;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class WaferRecoveryVisibiltyConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    if ((string)value == "FAIL")
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Hidden;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class FoupLotStateValueConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Gray;
            try
            {


                if (value is LotStateEnum)
                {
                    LotStateEnum state = (LotStateEnum)value;
                    switch (state)
                    {
                        case LotStateEnum.Idle:
                            brush = Brushes.Orange;
                            break;
                        case LotStateEnum.Done:
                            brush = Brushes.LimeGreen;
                            break;
                        case LotStateEnum.Cancel:
                            brush = Brushes.Magenta;
                            break;
                        case LotStateEnum.Running:
                            brush = Brushes.LimeGreen;
                            break;
                        case LotStateEnum.Error:
                            brush = Brushes.Red;
                            break;
                        case LotStateEnum.End:
                            brush = Brushes.LimeGreen;
                            break;
                        case LotStateEnum.Suspend:
                            brush = Brushes.Purple;
                            break;
                        case LotStateEnum.Pause:
                            brush = Brushes.Purple;
                            break;
                        default:
                            //THROUGH OUT
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StageStateToValueConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Gray;
            try
            {


                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    switch (state)
                    {
                        case ModuleStateEnum.ERROR:
                            brush = Brushes.Red;
                            break;
                        case ModuleStateEnum.IDLE:
                            brush = Brushes.Orange;
                            break;
                        case ModuleStateEnum.RUNNING:
                            brush = Brushes.LimeGreen;
                            break;
                        case ModuleStateEnum.UNDEFINED:
                            brush = Brushes.Gray;
                            break;
                        case ModuleStateEnum.PAUSING:
                            brush = Brushes.Gray;
                            break;
                        case ModuleStateEnum.PAUSED:
                            brush = Brushes.Violet;
                            break;
                        case ModuleStateEnum.ABORT:
                            brush = Brushes.RosyBrown;
                            break;
                        default:
                            //THROUGH OUT
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StageErrorStateToVisibilityConvertet : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility enable = Visibility.Hidden;
            try
            {
                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    switch (state)
                    {
                        case ModuleStateEnum.ERROR:
                            enable = Visibility.Visible;
                            break;
                        case ModuleStateEnum.PAUSED:
                            enable = Visibility.Visible;
                            break;
                        default:
                            enable = Visibility.Hidden;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return enable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }



    public class StageIndexTextConvertet : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String str = String.Empty;
            try
            {
                if (value is int)
                {
                    int idx = (int)value;
                    switch (idx)
                    {
                        case 1:
                            str = "1(A)";
                            break;
                        case 2:
                            str = "2(B)";
                            break;
                        case 3:
                            str = "3(C)";
                            break;
                        case 4:
                            str = "4(D)";
                            break;
                        case 5:
                            str = "5(E)";
                            break;
                        case 6:
                            str = "6(F)";
                            break;
                        case 7:
                            str = "7(G)";
                            break;
                        case 8:
                            str = "8(H)";
                            break;
                        case 9:
                            str = "9(I)";
                            break;
                        case 10:
                            str = "10(J)";
                            break;
                        case 11:
                            str = "11(K)";
                            break;
                        case 12:
                            str = "12(L)";
                            break;
                        default:
                            str = idx + "";
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ForcedDoneStateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String str = String.Empty;
            try
            {
                if (value is EnumModuleForcedState)
                {
                    EnumModuleForcedState state = (EnumModuleForcedState)value;
                    switch (state)
                    {
                        case EnumModuleForcedState.ForcedDone:
                            str = " (DEMO)";
                            break;

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class LoaderStateEnableConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool enable = false;
            try
            {


                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    switch (state)
                    {
                        case ModuleStateEnum.ERROR:
                            enable = true;
                            break;
                        default:
                            enable = false;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return enable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class LoaderMultiEnableConverter : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
        {
            bool enable = false;

            try
            {
                if (values[0] != DependencyProperty.UnsetValue &&
                   values[1] != DependencyProperty.UnsetValue)
                {
                    if (values[0] is ModuleStateEnum)
                    {
                        ModuleStateEnum state = (ModuleStateEnum)values[0];
                        switch (state)
                        {
                            case ModuleStateEnum.ERROR:
                                enable = true;
                                return enable;
                            default:
                                enable = false;
                                break;
                        }
                    }
                    if (values[1] is ModuleStateEnum)
                    {
                        ModuleStateEnum state = (ModuleStateEnum)values[1];
                        switch (state)
                        {
                            case ModuleStateEnum.ERROR:
                                enable = true;
                                return enable;
                            default:
                                enable = false;
                                break;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return enable;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class StageStateToValueConverter1 : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Gray;
            try
            {


                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    switch (state)
                    {
                        case ModuleStateEnum.ERROR:
                            brush = Brushes.Red;
                            break;
                        case ModuleStateEnum.IDLE:
                            brush = Brushes.Yellow;
                            break;
                        case ModuleStateEnum.RUNNING:
                            brush = Brushes.Green;
                            break;
                        case ModuleStateEnum.UNDEFINED:
                            brush = Brushes.Gray;
                            break;
                        case ModuleStateEnum.PAUSING:
                            brush = Brushes.Gray;
                            break;
                        case ModuleStateEnum.PAUSED:
                            brush = Brushes.Orange;
                            break;
                        case ModuleStateEnum.ABORT:
                            brush = Brushes.Red;
                            break;
                        default:
                            //THROUGH OUT
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StageStateToValueTextConverter : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = "";
            try
            {
                if (values[0] is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)values[0];
                    switch (state)
                    {
                        case ModuleStateEnum.PAUSING:
                            str = "PAUSED";

                            break;
                        case ModuleStateEnum.PAUSED:

                        default:
                            str = state.ToString();
                            if (state == ModuleStateEnum.PAUSED)
                            {
                                if (values[1] != null)
                                {
                                    if (values[1].Equals("") == false)
                                    {
                                        str += $":[{values[1].ToString()}]";
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StageStateToValueTextConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = "";
            try
            {


                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    switch (state)
                    {
                        case ModuleStateEnum.PAUSING:
                            str = "PAUSED";
                            break;
                        case ModuleStateEnum.PAUSED:

                        default:
                            str = state.ToString();
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StageLockToValueTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = "";
            try
            {
                if (values[0] is GPCellModeEnum && values[1] is StageLockMode)
                {
                    GPCellModeEnum mode = (GPCellModeEnum)values[0];
                    StageLockMode lockMode = (StageLockMode)values[1];
                    bool bReconnecting = (bool)values[2];
                    if (bReconnecting)
                    {
                        str = "RECONNECTING";
                    }
                    else
                    {
                        switch (mode)
                        {
                            case GPCellModeEnum.DISCONNECT:
                                str = "DISCONNECT";
                                break;
                            case GPCellModeEnum.MAINTENANCE:
                                str = "MAINTENANCE";
                                if (lockMode == StageLockMode.LOCK)
                                {
                                    str = "MAINTENANCE [LOCK]";
                                }
                                break;
                            case GPCellModeEnum.ONLINE:
                                str = "ONLINE";
                                if (lockMode == StageLockMode.LOCK)
                                {
                                    str = "ONLINE [LOCK]";
                                }
                                break;
                            case GPCellModeEnum.OFFLINE:
                                str = "OFFLINE";
                                if (lockMode == StageLockMode.LOCK)
                                {
                                    str = "OFFLINE";
                                }
                                break;

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class StageStateToValueTextConverter : IValueConverter
    //{


    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string str = "";
    //        try
    //        {


    //            if (value is ModuleStateEnum)
    //            {
    //                ModuleStateEnum state = (ModuleStateEnum)value;
    //                switch (state)
    //                {
    //                    case ModuleStateEnum.PAUSING:
    //                        str = "PAUSED";
    //                        break;
    //                    case ModuleStateEnum.PAUSED:

    //                    default:
    //                        str = state.ToString();
    //                        break;
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return str;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class StateToValueTextConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = "";
            try
            {


                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    str = state.ToString();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //public class FoupLotStateValueConverter : IValueConverter
    //{
    //    static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
    //    static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
    //    static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
    //    static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
    //    static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
    //    static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        Brush brush = Brushes.Gray;
    //        try
    //        {


    //            if (value is LotStateEnum)
    //            {
    //                LotStateEnum state = (LotStateEnum)value;
    //                switch (state)
    //                {
    //                    case LotStateEnum.Idle:
    //                        brush = Brushes.Orange;
    //                        break;
    //                    case LotStateEnum.Done:
    //                        brush = Brushes.LimeGreen;
    //                        break;
    //                    case LotStateEnum.Cancel:
    //                        brush = Brushes.Purple;
    //                        break;
    //                    case LotStateEnum.Running:
    //                        brush = Brushes.LimeGreen;
    //                        break;
    //                    case LotStateEnum.Error:
    //                        brush = Brushes.Red;
    //                        break;
    //                    case LotStateEnum.End:
    //                        brush = Brushes.LimeGreen;
    //                        break;
    //                    case LotStateEnum.Pause:
    //                        brush = Brushes.Purple;
    //                        break;
    //                    default:
    //                        //THROUGH OUT
    //                        break;
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return brush;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class EnumWaferStatusEnableTextConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType,
    //          object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        try
    //        {
    //            if (!(values[0] is EnumSubsStatus) && !(values[1] is bool))
    //            {
    //                return "null";
    //            }

    //            EnumSubsStatus val = (EnumSubsStatus)values[0];
    //            bool enable = (bool)values[1];


    //            if (enable)
    //            {
    //                return val;
    //            }
    //            else
    //            {
    //                return "DISABLE";
    //            }

    //            return val;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //            throw;
    //        }
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes,
    //              object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}


    public class EnumSlotTextConverter3 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (values[0] is EnumSubsStatus == false)
                {
                    retval = "null";
                }
                else
                {
                    EnumSubsStatus val = (EnumSubsStatus)values[0];
                    EnumWaferState param = (EnumWaferState)values[1];
                    TransferObject Obj = (TransferObject)values[2];

                    switch (val)
                    {
                        case EnumSubsStatus.EXIST:

                            if (param == EnumWaferState.SKIPPED)
                            {
                                retval = param.ToString();
                            }
                            else
                            {
                                if (Obj != null)
                                {
                                    string msg = string.Empty;

                                    if (param == EnumWaferState.PROCESSED || param == EnumWaferState.TESTED)
                                    {
                                        if (Obj.ProcessCellIndex >= 10)
                                        {
                                            msg = param + $" (C{Obj.ProcessCellIndex})";
                                        }
                                        else if (Obj.ProcessCellIndex >= 1)
                                        {
                                            msg = param + $" (C0{Obj.ProcessCellIndex})";
                                        }
                                    }
                                    else
                                    {
                                        if (Obj.CurrHolder == Obj.OriginHolder)
                                        {
                                            msg = param.ToString();
                                        }
                                        else
                                        {
                                            msg = Obj.CurrHolder.ToString();
                                        }
                                    }

                                    retval = msg;
                                }
                            }
                            break;
                        case EnumSubsStatus.UNKNOWN:
                        case EnumSubsStatus.NOT_EXIST:
                        case EnumSubsStatus.UNDEFINED:
                            retval = val.ToString().Replace("_", " ");
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumWaferStatusToColorConverter : IMultiValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values[0] is EnumSubsStatus == false)
                    return Transparentbrush;

                EnumSubsStatus val = (EnumSubsStatus)values[0];
                ModuleTypeEnum param = (ModuleTypeEnum)values[1];


                switch (val)
                {
                    case EnumSubsStatus.EXIST:
                        return LimeGreenbrush;
                    case EnumSubsStatus.UNKNOWN:
                        return Redbrush;
                    case EnumSubsStatus.NOT_EXIST:
                        {
                            if (param == ModuleTypeEnum.SLOT || param == ModuleTypeEnum.UNDEFINED)
                            {
                                return Transparentbrush;
                            }
                            else
                            {
                                return Yellowbrush;
                            }
                        }
                    case EnumSubsStatus.UNDEFINED:
                        return Transparentbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return Transparentbrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumWaferStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                EnumSubsStatus val = (EnumSubsStatus)values[0];
                ModuleTypeEnum param = (ModuleTypeEnum)values[1];

                //string str = null;

                if (val == EnumSubsStatus.EXIST)
                {
                    if (param == ModuleTypeEnum.SLOT || param == ModuleTypeEnum.UNDEFINED)
                    {
                        retval = val.ToString();
                    }
                    else
                    {
                        retval = param.ToString();
                    }
                }
                else
                {
                    if (param == ModuleTypeEnum.SLOT || param == ModuleTypeEnum.UNDEFINED)
                    {
                        retval = val.ToString();
                    }
                    else
                    {
                        retval = param.ToString();
                    }
                }

                //return str;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }



    public class ManualZUPEnableValueConverter : IValueConverter
    {
        static SolidColorBrush YellowGreenbrush = new SolidColorBrush(Colors.YellowGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is ManualZUPEnableEnum)
                {
                    ManualZUPEnableEnum enable = (ManualZUPEnableEnum)value;
                    switch (enable)
                    {
                        case ManualZUPEnableEnum.Enable:
                            return YellowGreenbrush;
                        case ManualZUPEnableEnum.Disable:
                            return DimGraybrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return DimGraybrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ManualZUPStateValueConverter : IValueConverter
    {
        static SolidColorBrush YellowGreenbrush = new SolidColorBrush(Colors.GreenYellow);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.LightYellow);
        static SolidColorBrush Orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush Dimgraybrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is ManualZUPStateEnum)
                {
                    ManualZUPStateEnum enable = (ManualZUPStateEnum)value;
                    switch (enable)
                    {
                        case ManualZUPStateEnum.Z_UP:
                            return YellowGreenbrush;
                        case ManualZUPStateEnum.Z_DOWN:
                            return Orangebrush;
                        case ManualZUPStateEnum.NONE:
                            return Orangebrush;
                        case ManualZUPStateEnum.UNDEFINED:
                            return Dimgraybrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Orangebrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CassetteIDValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = "";
            try
            {
                if (value is string)
                {
                    if (value != null || value.ToString() != "")
                    {
                        ret = "[" + value.ToString().Replace("_", "__") + "]";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StageSVLabelColorConverter : IValueConverter
    {
        static SolidColorBrush NormalStateBrush = new SolidColorBrush(Colors.MediumPurple);
        static SolidColorBrush AbnormalStateBrush = new SolidColorBrush(Colors.Red);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retVal = NormalStateBrush;
            try
            {
                if (value is String)
                {
                    string tempState = value.ToString();
                    if (String.IsNullOrEmpty(tempState) == false)
                    {
                        if (tempState.Equals(EnumTemperatureState.PauseDiffTemp.ToString()))
                        {
                            retVal = AbnormalStateBrush;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
