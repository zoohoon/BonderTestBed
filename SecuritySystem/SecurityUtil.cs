using System;
using System.Text;
using System.Security.Cryptography;

namespace SecuritySystem
{
    public class SecurityUtil
    {
        public static string GetHashCode_SHA256(string code)
        {
            SHA256Managed sha256Managed = new SHA256Managed();
            
            return Convert.ToBase64String(sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(code)));
        }
    }
}
