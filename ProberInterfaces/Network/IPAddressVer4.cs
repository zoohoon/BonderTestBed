using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberInterfaces.Network
{
    [Serializable]
    public struct IPAddressVer4
    {
        [NonSerialized]
        private String _Octet1;
        [NonSerialized]
        private String _Octet2;
        [NonSerialized]
        private String _Octet3;
        [NonSerialized]
        private String _Octet4;

        public String IP { get; set; }
        
        public IPAddressVer4(String ip) 
        {
            _Octet1 = "0";
            _Octet2 = "0";
            _Octet3 = "0";
            _Octet4 = "0";
            IP = String.Empty;
        }
        public override string ToString()
        {
            return IP;
        }
        public static bool CheckIpOctetAvailabe(IEnumerable<String> octetList)
        {
            bool isOctetAvailabe = true;
            do
            {
                if (octetList.Count() != 4)
                {
                    isOctetAvailabe = false;
                    break;
                }

                foreach (String octet in octetList)
                {
                    int intOctet = 0;
                    if (int.TryParse(octet, out intOctet) == false)
                    {
                        isOctetAvailabe = false;
                        break;
                    }

                    if (intOctet < 0 || intOctet > 255)
                    {
                        isOctetAvailabe = false;
                        break;
                    }
                }
            } while (false);

            return isOctetAvailabe;
        }
        public static IPAddressVer4 GetData(String ip)
        {
            IPAddressVer4 ipAdd = new IPAddressVer4();

            if(ip != null)
            {
                String[] octetSplit = ip.Split(new char[] { '.' });


                if (CheckIpOctetAvailabe(octetSplit.AsEnumerable()))
                {
                    ipAdd._Octet1 = octetSplit[0];
                    ipAdd._Octet2 = octetSplit[1];
                    ipAdd._Octet3 = octetSplit[2];
                    ipAdd._Octet4 = octetSplit[3];
                }
                else
                {
                    ipAdd._Octet1 = "0";
                    ipAdd._Octet2 = "0";
                    ipAdd._Octet3 = "0";
                    ipAdd._Octet4 = "0";
                }
            }
            else
            {
                ipAdd._Octet1 = "0";
                ipAdd._Octet2 = "0";
                ipAdd._Octet3 = "0";
                ipAdd._Octet4 = "0";
            }

            ipAdd.IP = $"{ipAdd._Octet1}.{ipAdd._Octet2}.{ipAdd._Octet3}.{ipAdd._Octet4}";

            return ipAdd;
        }
    }
}
