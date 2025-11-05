using LogModule;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.Communication.BarcodeReader;
using ProberInterfaces.RFID;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO.Ports;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Data;
using ProberInterfaces.Foup;

namespace LoaderParameterSettingView
{
    /// <summary>
    /// LoaderParameterSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderParameterSettingView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("AE2B4076-1C65-87E1-7DA5-64BA7B1D4CCA");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public LoaderParameterSettingView()
        {
            InitializeComponent();
        }
    }

    public class SerialPortList : ObservableCollection<string>
    {
        public SerialPortList()
        {
            for(int i=1; i<=10; i++)
            {
                Add("COM" + i);
            }
        }
    }

    [Serializable, DataContract]
    public class RFIDParamConverter
    {
        [DataMember]
        public Element<int> Index;
        [DataMember]
        public bool IsAttatch;
        [DataMember]
        public EnumCommmunicationType CommType;
        [DataMember]
        public StopBits StopBits;
        [DataMember]
        public string SerialPort;
    }
    public class RFIDObjectChangeConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object[] retVal = null;
            var fineCommandParameter = new RFIDParamConverter();
            try
            {
                if (values != null)
                {
                    fineCommandParameter.Index = (Element<int>)values[1];
                    if(values[0] is bool)
                    {
                        fineCommandParameter.IsAttatch = (bool)values[0];
                    }
                    else if(values[0] is EnumCommmunicationType)
                    {
                        fineCommandParameter.CommType = (EnumCommmunicationType)values[0];
                    }
                    else if(values[0] is StopBits)
                    {
                        fineCommandParameter.StopBits = (StopBits)values[0];
                    }
                    else if(values[0] is string) //TO DO: combobox 설정에서 string type 중복 되는 경우 수정 필요
                    {
                        fineCommandParameter.SerialPort = (string)values[0];
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return fineCommandParameter;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
 
    public class RFIDProtocolTypeToIsEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                EnumRFIDProtocolType protocolType = (EnumRFIDProtocolType)values[0];
                Element<int> foupIdx = (Element<int>)values[1];
                if (protocolType != null && foupIdx != null)
                {
                    if(protocolType == EnumRFIDProtocolType.MULTIPLE)
                    {
                        if(foupIdx.Value > 0)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable, DataContract]
    public class BCDParamConverter
    {
        [DataMember]
        public Element<int> Index;
        [DataMember]
        public bool IsAttatch;
        [DataMember]
        public EnumCommmunicationType CommType;
    }
    public class BCDObjectChangeConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object[] retVal = null;
            var fineCommandParameter = new BCDParamConverter();
            try
            {
                if (values != null)
                {
                    fineCommandParameter.Index = (Element<int>)values[1];
                    if (values[0] is bool)
                    {
                        fineCommandParameter.IsAttatch = (bool)values[0];
                    }
                    else if (values[0] is EnumCommmunicationType)
                    {
                        fineCommandParameter.CommType = (EnumCommmunicationType)values[0];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return fineCommandParameter;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SubstrateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = "";
            try
            {
                var substrateSize = values[0]?.ToString();

                if (substrateSize == null || !(values[1] is CassetteTypeEnum casstteType))
                    return null;

                ret = (casstteType == CassetteTypeEnum.FOUP_13) ? $"{substrateSize}__{casstteType}" : substrateSize;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
