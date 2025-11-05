using System;

namespace ThetaAlignStandardModule
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.WaferAlignEX;
    using System.Collections.ObjectModel;
    using WAProcBaseParam;

    public static class ConfirmExistManualJumpIndex
    {
        public static int Confirm(JumpIndexStandardParam param, EnumWAProcDirection procdirection)
        {
            int retVal = -1;
            try
            {
                switch (procdirection)
                {
                    case EnumWAProcDirection.HORIZONTAL:
                        if (param.LeftIndex != 0 || param.RightIndex != 0
                             || param.UpperIndex != 0 || param.BottomIndex != 0)
                        {
                            retVal = 1;
                        }
                        break;
                    case EnumWAProcDirection.VERTICAL:
                        if (param.LeftIndex != 0 || param.RightIndex != 0
                             || param.UpperIndex != 0 || param.BottomIndex != 0)
                        {
                            retVal = 1;
                        }
                        break;
                    case EnumWAProcDirection.BIDIRECTIONAL:
                        if (param.LeftIndex != 0 || param.RightIndex != 0
                             || param.UpperIndex != 0 || param.BottomIndex != 0)
                        {
                            retVal = 1;
                        }
                        break;

                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

    public static ObservableCollection<StandardJumpIndexParam> FindJumpIndex(
       JumpIndexStandardParam param, EnumWAProcDirection procdirection, ObservableCollection<StandardJumpIndexParam> jumpindexparam)
    {
        try
        {
            switch (procdirection)
            {
                case EnumWAProcDirection.HORIZONTAL:
                    jumpindexparam.Add(new StandardJumpIndexParam(0, 0));
                    if (param.LeftIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(-(param.LeftIndex), 0));
                    }
                    if (param.RightIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(param.RightIndex, 0));
                    }
                    break;
                case EnumWAProcDirection.VERTICAL:
                    jumpindexparam.Add(new StandardJumpIndexParam(0, 0));

                    if (param.UpperIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(0, param.UpperIndex));
                    }
                    if (param.BottomIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(0, -(param.BottomIndex)));
                    }
                    break;
                case EnumWAProcDirection.BIDIRECTIONAL:
                    jumpindexparam.Add(new StandardJumpIndexParam(0, 0));
                    if (param.LeftIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(-(param.LeftIndex), 0));
                    }
                    if (param.RightIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(param.RightIndex, 0));
                    }
                    if (param.UpperIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(0, param.UpperIndex));
                    }
                    if (param.BottomIndex != 0)
                    {
                        jumpindexparam.Add(new StandardJumpIndexParam(0, -(param.BottomIndex)));
                    }
                    break;

            }
        }
        catch (Exception err)
        {
            LoggerManager.Exception(err);
            throw;
        }


        return jumpindexparam;
    }

}
}
