using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Aladdin.HASP;
using System.Net.NetworkInformation;
using System.Xml;
using System.Security.Cryptography;

namespace Sentinel
{
    public class Sentinel
    {
        private static int ResultHaspStatus()
        {
            int retVal = 0xFF;
            try
            {
                HaspFeature feature = HaspFeature.FromFeature(1000);

                string vendorCode =
                "XJ69p9rEb9GDh8FU992mcRUd+QyQ5EevwxMm8k+JvSvTTg78gfRJu6cyn8OnXhFjdESZYjEiJ8q4iEre" +
                "LNmu8e7GS43MTVSVxgJ1KYPLi0CztJnPzep5c8Ol5MljjPkZTji1KrgPaGsA1Gbln7ACaJkf5axn2Q4l" +
                "NyiV9pCc2HZvUAqBl21Pg9N1vdl8LDn211Oq4ARfF49SPi2o0qRvnSenpvotNMAt4n7k4Sw7VhGlWdeS" +
                "+y1ZeAfyY/+3ea5anN/f7aoP4A2pOfV6h+y17oyGdeaD9UPyHavS46SQLxMdeLxK1Rt6VJx7NLHEBij1" +
                "znHBFIpTY8KZO70W3LX6nUIZcY5xvFq99CEGAcL4My3zDnZlhfBVdXz0NU9IElBfmQ1qMhiFQsm8cifP" +
                "mvb5uNwdE+5bV+g4ItaPNHcHZheYYtz2h5VU7311lMm+kEACSwreSJrV1CFC2n++AoY2NYLnf9F63h63" +
                "fTYn/EoJSzbItTk/IRe9YD9V+Hk28Rz1wKwF0QfAtq8d9Tny+2n1H97exzmKAjL1hgsMClfHHf3r/g85" +
                "YFi0R6AmlvZbbvbAgorc36BPs09Z5+tpkLi+QR9JSMbX4WJnWoMkCRfPNceRm9jgX1puCmmkNt61KbWo" +
                "5Oe9b7oGkZFI2VKV276hxLA/7J5ByPV/LZ5p4uF7xLmN1KslkDnqNYKZ7SHi91A3SdwDCwb9nM80Mb3/" +
                "Ba8zs6ivAQ8o2OA59b3ASI3WyoffWUEZ8WAe1brfrojRdtjeQj6L1QoaeFQt+3VTSMHllRa7KGmhNPTi" +
                "uTs5gf+ZlAcF4F3Th6ezx9N17sWRWopWfiUnrnMM0NbA2dAk+tpf3J0+wrvf+ekZAc5b9907Cfjigy+y" +
                "hbX390DSV/5L1wEmAfDlTAXOl0NRRZUk8d+jUKRTqXsib1QHSiaERToT7Shyhq0l54yebvma63EK3Vd8" +
                "33tM0ThX96BjMSZ6eianyA==";

                Hasp hasp = new Hasp(feature);                
                HaspStatus status = hasp.Login(vendorCode);
                                
                if (HaspStatus.StatusOk != status)
                {
                    //handle error                    
                    Console.WriteLine("HASP Runtime API handle error reason: " + status.ToString());
                    retVal = 0x00;
                }
                else
                {
                    // HaspStatus.StatusOk 상태라는건, usb 인식 or Sentinel SL Key ID 인식 둘 중 하나 성공
                    // case 1. usb 인식 - Agent, customer
                    // case 2. GenerateUniqueKey(Sentinel SL) 인식 - developer
                    Console.WriteLine("HASP Runtime API handle success");
                    retVal = 0x01;
                }               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        /// <summary>
        /// 0: 프로그램 인증 실패 <br/>
        /// 0 보다 크다: 프로그램 인증 성공 <br/>
        /// 1: emul mode <br/>
        /// 2: sentinel usb 인증 or sentinel SL 인증(usb 꽂고 인증하면 다음에 안꽂아도 사용가능하도록 하는 것) <br/>      
        /// </summary>
        /// <returns></returns>
        public static int VerifyEncryption()
        {
            int retVal = 0x00;
            string retValstr = "";
            try
            {
                // step1: validate emul mode   
                SystemManager.LoadParam();
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    // step2: validate debug mode ( developer - sentinel SL active)
#if DEBUG
                    retVal = 0x01;
                    retValstr += retVal.ToString() + "/";                    
#else
                    // step3: validate sentinel usb doggle
                    int ret = ResultHaspStatus();
                    if (ret == 0x01)
                    {
                        retVal = 0x02;
                        retValstr += retVal.ToString();
                    }
#endif          
                }
                else
                {
                    // real machine
                    retVal = 0x01;
                    retValstr += retVal.ToString();
                }
                Console.WriteLine("VerifyEncryption() : " + retValstr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public static string GetMacAddress()
        {
            var macAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();

            return macAddress;
        }

        public static string ExtractKeyId(string xml)
        {
            string id = "";
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);            
            
            XmlNode xNode = doc.SelectSingleNode("/hasp_info/hasp");
            if(xNode != null)
            {
                id = xNode.Attributes["id"].Value;
            }
            return id;
        }
        /// <summary>
        /// SHA256: 암호화 해싱 알고리즘.
        /// 대상 문자열을 256bit 길이의 key로 변환한다.
        /// </summary>
        /// <param name="macAddress"></param>
        /// <param name="sentinelId"></param>
        /// <returns></returns>
        public static string GenerateSHA256(string macAddress, string sentinelId)
        {            
            string combined = macAddress + sentinelId;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));
                }
                return hashStringBuilder.ToString();
            }
        }   
    }
}
