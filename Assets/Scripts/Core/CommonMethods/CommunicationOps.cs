using System.Collections;
using System.Collections.Generic;
using Mathd_Lib;
using System.Text.RegularExpressions;

namespace CommonMethods
{
    public static class CommunicationOps
    {
        public static bool IP_AddressIsValid(string IP_toCheck)
        {
            string ipv4_REGEX = "(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])";
            return Regex.IsMatch(IP_toCheck, ipv4_REGEX);
        }
    }
}