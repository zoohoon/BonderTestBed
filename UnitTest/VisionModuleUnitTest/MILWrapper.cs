using System;
using Matrox.MatroxImagingLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VisionModuleUnitTests
{
    public class MilApp : IDisposable
    {
        private MIL_ID ApplicationIdPtr = MIL.M_NULL;

        public MIL_ID Id { get => ApplicationIdPtr; }

        public MilApp(bool validCheck=true)
        {
            MIL.MappAlloc(MIL.M_DEFAULT, ref ApplicationIdPtr);

            if (!validCheck)
                return;

            MIL_INT LicenseModules = MIL.M_NULL;
            MIL.MappInquire(ApplicationIdPtr, MIL.M_LICENSE_MODULES, ref LicenseModules);
            if ( (LicenseModules & MIL.M_LICENSE_IM) == 0)
            {
                Dispose();
                Assert.Inconclusive("MIL 라이선스가 없어 관련 테스트가 수행되지 않습니다. Unable to test MIL library.");
            }
        }
        public void Dispose()
        {
            if (ApplicationIdPtr != MIL.M_NULL)
            {
                MIL.MappFree(ApplicationIdPtr);
                ApplicationIdPtr = MIL.M_NULL;
            }
        }
        public static implicit operator MIL_ID(MilApp i) => i.Id;
    }

    public class MilSys : IDisposable
    {
        private MIL_ID SystemIdPtr = MIL.M_NULL;

        public MIL_ID Id { get => SystemIdPtr; }

        public MilSys(bool validCheck = true)
        {
            MIL.MsysAlloc(MIL.M_SYSTEM_DEFAULT, MIL.M_NULL, MIL.M_DEFAULT, ref SystemIdPtr);

            if (!validCheck)
                return;

            MIL_ID applicationPtr = MIL.M_NULL;
            MIL.MsysInquire(SystemIdPtr, MIL.M_OWNER_APPLICATION, ref applicationPtr);
            MIL_ID licenseModulePtr = MIL.M_NULL;
            MIL.MappInquire(applicationPtr, MIL.M_LICENSE_MODULES, ref licenseModulePtr);


            if ((licenseModulePtr & MIL.M_LICENSE_MOD) != MIL.M_LICENSE_MOD)
            {
                Dispose();
                Assert.Inconclusive("MIL 라이선스가 없어 관련 테스트가 수행되지 않습니다. Unable to test MIL library.");
            }

        }
        public void Dispose()
        {
            if (SystemIdPtr != MIL.M_NULL)
            {
                MIL.MsysFree(SystemIdPtr);
                SystemIdPtr = MIL.M_NULL;
            }
        }
        public static implicit operator MIL_ID(MilSys i) => i.Id;
    }

    public class Milbuf2d : IDisposable
    {
        private MIL_ID BufIdPtr = MIL.M_NULL;

        public MIL_ID Id { get => BufIdPtr; }

        public Milbuf2d(MIL_ID SystemId, MIL_INT SizeX, MIL_INT SizeY, MIL_INT Type, long Attribute)
        {
            MIL.MbufAlloc2d(SystemId, SizeX, SizeY, Type, Attribute, ref BufIdPtr);
        }
        public void Dispose()
        {
            if (BufIdPtr != MIL.M_NULL)
            {
                MIL.MbufFree(BufIdPtr);
                BufIdPtr = MIL.M_NULL;
            }
        }

        public static implicit operator MIL_ID(Milbuf2d i) => i.Id;
    }

}
