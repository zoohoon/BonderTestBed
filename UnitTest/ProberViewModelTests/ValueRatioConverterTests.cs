using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoakingSettingView.Tests
{
    [TestClass()]
    public class ValueRatioConverterTests
    {
        [DataTestMethod]
        [DataRow(0, 0, 0)]
        [DataRow(-1, 50, -50)]
        [DataRow(10.1,0.5 , 5.05)]
        [DataRow(10.1, "50%", 5.05)]
        public void ConvertTest(double value, object ratio, double expected)
        {
            ValueRatioConverter c = new ValueRatioConverter();
            var ret = c.Convert(value, typeof(double), ratio, System.Globalization.CultureInfo.CurrentCulture);
            Assert.AreEqual(expected, ret);
        }
    }
}