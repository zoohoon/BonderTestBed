using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleFactory;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using Autofac;

namespace RequestCore.ActionPack.TCPIP.Tests
{
    //[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    //public class MyCustomDataRowAttribute : DataRowAttribute
    //{
    //}

    [TestClass()]
    public class SetLotProcessingVerifyTests : IFactoryModule
    {
        private static IContainer Container;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            SystemManager.SysteMode = SystemModeEnum.Multiple;
            Container = ModuleResolver.ConfigureDependencies();

            Extensions_IModule.SetContainer(null, Container);
            Extensions_ICommand.SetStageContainer(null, Container);
        }

        [DataTestMethod]
        
        [DataRow("LPV0")]
        [DataRow("LPV1")]
        
        [DataRow("LPV0#P1")]
        [DataRow("LPV1#P1")]
        
        [DataRow("LPV0#P2")]
        [DataRow("LPV1#P2")]
        
        [DataRow("LPV0#P3")]
        [DataRow("LPV1#P3")]
        public void RunTest(string argument)
        {
            // Send ParamPath
            Container.Resolve<IFileManager>(new NamedParameter("ParamPath", "C:\\ProberSystem\\C01"));

            SetLotProcessingVerify setLotProcessingVerify = new SetLotProcessingVerify();

            setLotProcessingVerify.Argument = argument;
            Assert.AreEqual(EventCodeEnum.NONE, setLotProcessingVerify.Run());
        }
    }
}