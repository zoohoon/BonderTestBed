using LogModule;
using System;

namespace ProberInterfaces
{
    public static class EvaluationPrivilege
    {
        public static Func<bool> Evaluate(int maskinglevel, string strauthlevel)
        {
            try
            {
                Func<bool> f = delegate () { return EvaluateExcutable(maskinglevel, strauthlevel); };
                return f;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static bool EvaluateExcutable(int maskinglevel, string strauthlevel)
        {
            bool excutable;
            try
            {
                int authlevel;

                if (int.TryParse(strauthlevel, out authlevel))
                {
                    if (maskinglevel >= authlevel)
                    {
                        excutable = true;
                    }
                    else
                    {
                        excutable = false;
                    }

                }
                else
                {
                    excutable = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return excutable;
        }
    }
}
