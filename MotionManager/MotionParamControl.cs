using LogModule;
using ProberInterfaces;
using System;

namespace ProbeMotion
{
    public class MotionParamControl
    {

#region // Properties
        public int iDynamicAccControl { get; set; }

#endregion

        // Constructor
        public MotionParamControl()
        {

        }

        public void CalcMotionAccParam(int ax, double dist, double gAcc, out double acc)
        {
            try
            {
            double tmpAcc;

            if (dist < 0)
            {
                dist = dist * -1;
            }

            if ((ax == (int)EnumAxisConstants.X) || (ax == (int)EnumAxisConstants.Y))
            {
                if (dist > 5000)
                {
                    acc = gAcc;
                }
                else
                {
                    if (iDynamicAccControl == 0)
                    {
                        acc = gAcc;
                    }
                    else
                    {
                        if (dist < 200)
                        {
                            tmpAcc = ((gAcc / 2) - ((gAcc / 2) * (1000 - dist) / 1000) / 2) / 2;
                        }
                        else if ((dist >= 200) && (dist < 1000))
                        {
                            tmpAcc = (gAcc / 2) - ((gAcc / 2) * ((1000 - dist) / 1000)) / 2;
                        }
                        else if ((dist >= 1000) && (dist <= 5000))
                        {
                            tmpAcc = gAcc - ((gAcc * (5000 - dist) / 5000) / 2);
                        }
                        else
                        {
                            tmpAcc = gAcc;
                        }

                        acc = tmpAcc;
                    }
                }
            }
            else
            {
                if (dist < 1500)
                {
                    acc = gAcc * 0.25;
                }
                else
                {
                    // Input Normal speed, acceleration and Jerk valve
                    acc = gAcc;
                }
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public void CalcMotionParam(int ax, double dist, double gSpd, double gAcc, double gJrk, out double vel, out double acc, out double jerk)
        {
            try
            {
            vel = 0;
            acc = 0;
            jerk = 0;

            if (dist < 0)
            {
                dist = dist * -1;
            }

            //double coeff;
            if ((ax == (int)EnumAxisConstants.X) || (ax == (int)EnumAxisConstants.Y))
            {
                // Dynamic acceleration control
                //	Min acc = 500,000
                //	Angle = 277.7 (=> Acc is 3000,000 when dist is 9mm 

                double tmpAcc;

                if (dist > 5000)
                {
                    vel = gSpd;
                    acc = gAcc;
                    jerk = gJrk;
                }
                else
                {
                    if (iDynamicAccControl == 0)
                    {
                        vel = gSpd;
                        acc = gAcc;
                        jerk = gJrk;
                    }
                    else
                    {
                        if (dist < 200)
                        {
                            tmpAcc = ((gAcc / 2) - ((gAcc / 2) * (1000 - dist) / 1000) / 2) / 2;
                        }
                        else if ((dist >= 200) && (dist < 1000))
                        {
                            tmpAcc = (gAcc / 2) - ((gAcc / 2) * ((1000 - dist) / 1000)) / 2;
                        }
                        else if ((dist >= 1000) && (dist <= 5000))
                        {
                            tmpAcc = gAcc - ((gAcc * (5000 - dist) / 5000) / 2);
                        }
                        else
                        {
                            tmpAcc = gAcc;
                        }

                        vel = gSpd;
                        acc = tmpAcc;
                        jerk = gJrk;
                    }
                }

            }
            else
            {
                if (dist < 1500)
                {
                    vel = gSpd;
                    acc = gAcc * 0.25; 
                    jerk = gJrk;
                }
                else
                {
                    // Input Normal speed, acceleration and Jerk valve

                    vel = gSpd; 
                    acc = gAcc;
                    jerk = gJrk;
                }
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
