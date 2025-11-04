using Autofac;
using FileSystem.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProberInterfaces;
using ProberInterfaces.Param;
using SinglePinAlign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProbeCardObject;

namespace SinglePinAlign.Tests
{
    [TestClass()]
    public class BlobSinglePinAlignTests : TestBaseProberSystem
    {
        [TestMethod()]
        public void SinglePinalignTest()
        {
            Container.Resolve<IFileManager>(new NamedParameter("ParamPath", "C:\\ProberSystem\\CELLParam\\Emul_c01"));

            PinCoordinate NewPinPos;

            int CurDutIndex = 0;
            int CurPinArrayIndex = 0;

            // PinHighAlignModule이 갖고 있는 값과 동일하게 구성하였음.
            var PinAlignDevParameters = this.PinAligner().PinAlignDevParam as PinAlignDevParameters;
            IFocusing PinFocusModel = this.FocusManager().GetFocusingModel(PinAlignDevParameters.FocusingModuleDllInfo);
            IFocusParameter FocusParam = PinAlignDevParameters.FocusParam;
            EnumProberCam camtype = EnumProberCam.PIN_HIGH_CAM;

            // 배경 :
            // VisionGrabberEmul의 데이터 할당을 통해 Grab 이미지를 획득하여 사용한다.
            // StartGrab()에서는 이미지 버퍼가 null이 발생되고 있음.
            // 한번 사용된 이미지의 플래그를 둠으로써 다음 영상을 사용

            // TODO : (SKIP)의 경우..현재 예외 발생 됨.
            // Focusing 동작을 위해서 Motion 예외 처리 필요
            // Focusing 동작을 위한 추가 파라미터 셋 구성 필요

            // Case 1)

            // (1) SinglePinAlignKeyFocusingModule - Focusing_Retry, (SKIP)

            // (2) getOtsuThreshold(), EnumThresholdType.AUTO
            string modulename = "BlobSinglePinAlign";
            string path = @"C:\Logs\Test\B57T\FV(2646742)_Z(-18422.1990275493).bmp";

            this.VisionManager().SetParameter(camtype, modulename, path);

            // (3) FindBlobWithRectangularity()
            modulename = "VisionManager";
            path = @"C:\Logs\Test\B57T\FV(2646742)_Z(-18422.1990275493).bmp";

            this.VisionManager().SetParameter(camtype, modulename, path);

            // (4) TipFocusModule, (SKIP)

            // (5) getOtsuThreshold(), EnumThresholdType.AUTO
            modulename = "BlobSinglePinAlign";
            path = @"C:\Logs\Test\B57T\FV(14499319)_Z(-18712.210645078).bmp";

            this.VisionManager().SetParameter(camtype, modulename, path);

            // (6) TipBlobModule()
            modulename = "VisionManager";
            path = @"C:\Logs\Test\B57T\FV(14499319)_Z(-18712.210645078).bmp";

            this.VisionManager().SetParameter(camtype, modulename, path);

            var result = this.PinAligner().SinglePinAligner.SinglePinalign(out NewPinPos, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex], PinFocusModel, FocusParam);
        }
    }
}