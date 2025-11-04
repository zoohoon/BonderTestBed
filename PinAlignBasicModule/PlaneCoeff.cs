using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinHighAlignModule
{
    public class PlaneCoeff
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
        public PlaneCoeff()
        {

        }
        public PlaneCoeff(float a, float b, float c, float d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }
        public float GetHeight(float x, float y)
        {
            float height = 0;
            try
            {
                height = -(A * x + B * y + D) / C;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"PlaneCoeff(): Exception occurred. Err = {err.Message}");
            }
            return height;
        }
        public static PlaneCoeff GetMeanPlane(List<PlaneCoeff> planes)
        {
            PlaneCoeff repPlane = new PlaneCoeff();

            try
            {
                double mA = 0;
                double mB = 0;
                double mC = 0;
                double mD = 0;

                if (planes != null)
                {
                    int planeCount = planes.Count;
                    if (planeCount > 1)
                    {
                        foreach (var plane in planes)
                        {
                            mA += plane.A;
                            mB += plane.B;
                            mC += plane.C;
                            mD += plane.D;
                        }
                    }
                    repPlane.A = (float)(mA / planeCount);
                    repPlane.B = (float)(mB / planeCount);
                    repPlane.C = (float)(mC / planeCount);
                    repPlane.D = (float)(mD / planeCount);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetMeanPlane(): Error occurred. Err = {err.Message}");
            }
            return repPlane;
        }
    }
}
