using System.Collections.Generic;

namespace ProberInterfaces
{
    //public interface IMetroDialogViewModel
    //{
    //}

    public interface IPnpAdvanceSetupView 
    {
        //EventCodeEnum CloseWindow();
    }
    public interface IPnpAdvanceSetupViewModel
    {
        List<byte[]> GetParameters();
        void SetParameters(List<byte[]> data = null);

        void Init();
    }

    public interface IElemMinMaxAdvanceSetupViewModel
    {      
    }
}
