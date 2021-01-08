using System.Collections;
using System.Collections.Generic;
using Mathd_Lib;
using System.Text.RegularExpressions;
using Communication;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;

namespace CommonMethods
{
    public static class CommunicationOps
    {
        public static bool IP_AddressIsValid(string IP_toCheck)
        {
            string ipv4_REGEX = "(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])";
            return Regex.IsMatch(IP_toCheck, ipv4_REGEX);
        }

        public static ComProtocol Str_2_ComProtocol(string stringEnum)
        {
            (ComProtocol,bool) res = ObjectsHandling.Generic_Str_2_Enum<ComProtocol>(stringEnum);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return ComProtocol.TCPIP_Sender; // Default value to return
        }
        public static ComConectionType Str_2_ComConnectionType(string stringEnum)
        {
            (ComConectionType,bool) res = ObjectsHandling.Generic_Str_2_FlagedEnum<ComConectionType>(stringEnum);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return ComConectionType.dataVisualization; // Default value to return
        }
        public static ComDataFieldsIn Str_2_ComDataFieldsIn(string stringEnum)
        {
            (ComDataFieldsIn,bool) res = ObjectsHandling.Generic_Str_2_FlagedEnum<ComDataFieldsIn>(stringEnum);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return ComDataFieldsIn.None; // Default value to return
        }
        public static ComDataFieldsOut Str_2_ComDataFieldsOut(string stringEnum)
        {
            (ComDataFieldsOut,bool) res = ObjectsHandling.Generic_Str_2_FlagedEnum<ComDataFieldsOut>(stringEnum);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return ComDataFieldsOut.None; // Default value to return
        }

        public static ComDataFieldsOut MS_Dropdown_2_ComDataFieldOut(List<stringBoolStruct> msDrop_options)
        {
            ComDataFieldsOut outDataFields = ComDataFieldsOut.None;
            foreach(stringBoolStruct pair in msDrop_options) {
                if(pair.optionIsSelected) {
                    if(outDataFields == ComDataFieldsOut.None)
                        outDataFields = Str_2_ComDataFieldsOut(pair.optionString);
                    else
                        outDataFields |= Str_2_ComDataFieldsOut(pair.optionString);
                }
            }
            return outDataFields;
        }
        public static ComDataFieldsIn MS_Dropdown_2_ComDataFieldIn(List<stringBoolStruct> msDrop_options)
        {
            ComDataFieldsIn outDataFields = ComDataFieldsIn.None;
            foreach(stringBoolStruct pair in msDrop_options) {
                if(pair.optionIsSelected) {
                    if(outDataFields == ComDataFieldsIn.None)
                        outDataFields = Str_2_ComDataFieldsIn(pair.optionString);
                    else
                        outDataFields |= Str_2_ComDataFieldsIn(pair.optionString);
                }
            }
            return outDataFields;
        }
    }
}