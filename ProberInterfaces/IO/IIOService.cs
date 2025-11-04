using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ProberErrorCode;


namespace ProberInterfaces
{
    public interface IIOService : IFactoryModule, IModule, IHasSysParameterizable
    {
        ObservableCollection<InputChannel> Inputs { get; set; }
        ObservableCollection<OutputChannel> Outputs { get; set; }
        ObservableCollection<AnalogInputChannel> AnalogInputs { get; set; }
        int DeInitIO();
        int InitIOService();
        EventCodeEnum InitModule(int ctrlNum, string devConfigParam, string ecatIOConfigParam);
        
        IORet WriteBit(IOPortDescripter<bool> io, bool value);
        IORet ReadBit(IOPortDescripter<bool> io, out bool value);
        //IORet ReadBit(int channel, int port, out bool value, bool reverse = false);
        //int WaitForIO(int channel, int port, bool level, long timeout = 0);
        int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0);
        int MonitorForIO(IOPortDescripter<bool> io, bool level, long maintainTime = 0, long timeout = 0, bool writelog = true);
        void BitOutputEnable(IOPortDescripter<bool> ioport);
        void BitOutputDisable(IOPortDescripter<bool> ioport);
        ICommand BitOutputEnableCommand { get; }
        ICommand BitOutputDisableCommand { get; }
        List<IIOBase> IOList { get; }
    }
}
