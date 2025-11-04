using LogModule;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;


// 함수로 보면....

// Wafer Alignment 관련 데이터가 있어야 계산이 가능 할텐데..?
// 
// (1) Index Size X
// (2) Index Size Y
// (3) Gutter Size X
// (4) Gutter Size Y
// (5) TSCOFFSET을 적용한 Device Size X
// (6) TSCOFFSET을 적용한 Device Size Y
// (7) PadOrg(High Mag)
// (7) HighMagPat1Offset


//    만들어야 할 함수가....

//    뭘까??

//    생각을해보면...

//    1. 인덱스 X,Y를 알 때, 해당 DIe로 이동 할 수 있어야하고, 위치들을 알 수 있어야 함. 위치라하면... (각 모서리들, 특정 패드, 특정 패턴)

//    2. 패드 번호를 알 때, 해당 패드의 센터로 이동 할 수 있어야 함.(현재 보고 있는 Die 내부에서, 또는 특정 Die(인덱스를 알 때)
//    3. 핀 번호를 알 때, 해당 핀의 센터로 이동 할 수 있어야 함.(현재 보고 있는 Dut 내부에서, 또는 특정 Dut(인덱스를 알 때))

namespace CoordinateSystem
{
    public class DieViewPointNONE : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointNONE(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }

    public class DieViewPointCENTER : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointCENTER(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }

    public class DieViewPointLOWPAT1 : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointLOWPAT1(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }

    public class DieViewPointLOWPAT2 : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointLOWPAT2(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }
    public class DieViewPointHIGHPAT1 : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointHIGHPAT1(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }
    public class DieViewPointHIGHPAT2 : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointHIGHPAT2(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }
    public class DieViewPointUPPERLEFT : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointUPPERLEFT(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }
    public class DieViewPointUPPERRIGHT : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointUPPERRIGHT(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }
    public class DieViewPointLOWERLEFT : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointLOWERLEFT(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }
    public class DieViewPointLOWERRIGHT : IDieViewPoint
    {
        private DeviceParams _DeviceParam;
        public DeviceParams DeviceParam { get; set; }

        public DieViewPointLOWERRIGHT(DeviceParams deviceparam)
        {
            this._DeviceParam = deviceparam;
        }

        public CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex)
        {
            CatCoordinates Result;
            try
            {

                Result = relPos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
    }
}
