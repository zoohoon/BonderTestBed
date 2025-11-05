using LogModule;
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Serial
{
    public static class SerialMaker
    {
        public static string MakeSerialString()
        {
            string retStr = null;
            try
            {
                string HDDSerialNum = GetHDDSerialNumber();

                if (HDDSerialNum != null)
                    retStr = ByteToSerial(MakeSerialByte(HDDSerialNum));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retStr;
        }

        public static string ByteToSerial(byte[] strByte)
        {
            string retStr = null;
            try
            {

                if (strByte != null)
                {
                    foreach (var v in strByte)
                    {
                        int tmpByte = Convert.ToInt16(v);
                        retStr += (char)(Convert.ToInt16('A') + (tmpByte % 26));
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retStr;
        }

        public static byte[] MakeSerialByte(String Str)
        {
            byte[] retVal = null;
            try
            {

                if (Str != null)
                    retVal = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(Str));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private static string GetHDDSerialNumber(string driver = "C")
        {
            if (driver == "" || driver == null)
                driver = "C";

            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"" + driver + ":\"");
            disk.Get();

            return disk["VolumeSerialNumber"].ToString();
        }
    }
}
