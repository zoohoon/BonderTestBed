using System;

namespace ProberInterfaces
{
    using ProberInterfaces.PMI;
    using LogModule;
    /// <summary>
    /// Data Converter입니다.
    /// Converter는 Prober Value => GEM Value의 방향입니다.
    /// Converter는 Gem Value => Prober Value의 방향입니다.
    /// </summary>
    public interface IDataConverter
    {
        object OptionStr { get; set; }
        object Convert(object value);
        object ConvertBack(object value);
    }

    [Serializable]
    public class DataMultiplesConverter : IDataConverter
    {
        public object OptionStr { get; set; }

        public object Convert(object value)
        {
            double convValue = 0;
            double multipleData = 1;
            bool isPossibleConv = double.TryParse(value.ToString(), out convValue);

            if (isPossibleConv)
            {
                if (OptionStr != null)
                {
                    isPossibleConv = double.TryParse(OptionStr.ToString(), out multipleData);

                    if (!isPossibleConv)
                        multipleData = 1;
                }

                convValue = convValue * multipleData;
            }
            return convValue;
        }

        public object ConvertBack(object value)
        {
            double convValue = 0;
            double multipleData = 1;
            bool isPossibleConv = double.TryParse(value.ToString(), out convValue);

            if (isPossibleConv)
            {
                if (OptionStr != null)
                {
                    isPossibleConv = double.TryParse(OptionStr.ToString(), out multipleData);

                    if (!isPossibleConv)
                        multipleData = 1;
                }

                convValue = convValue / multipleData;
            }
            return convValue;
        }
    }

    [Serializable]
    public class BooleanTypeConverter
    {
        public object OptionStr { get; set; }

        public object Convert(object value)
        {
            int convValue = 0;
            try
            {
                bool valueConv = (bool)value;

                if (valueConv)
                {
                    if (valueConv == false)
                    {
                        convValue = 0;
                    }
                    else
                    {
                        convValue = 1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return convValue;
        }

        public object ConvertBack(object value)
        {
            return 0;
        }
    }

    [Serializable]
    public class PMIEnumConverter
    {
        public object OptionStr { get; set; }

        public object Convert(object value)
        {
            int convValue = 0;
            try
            {
                if(value is OP_MODE)
                {
                    OP_MODE valueConv = (OP_MODE)value;
                    if (valueConv == OP_MODE.Disable)
                    {
                        convValue = 0;
                    }
                    else
                    {
                        convValue = 1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
           
            return convValue;
        }

        public object ConvertBack(object value)
        {
            return 0;
        }
    }
}
