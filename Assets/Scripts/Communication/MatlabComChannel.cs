using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Fncs = UsefulFunctions;
using System;

namespace Matlab_Communication
{
    public enum MatlabConectionType { dataVisualization, simulationEnv, shipControlOrders };
    public class MatlabComChannel<T>
    {
        public MatlabConectionType connectionType;

        // T is either the 'UDPReceiver', 'UDPSender', 'TCPReceiver' or 'TCPSender' object
        public T _channelObj;
        public T channelObj
        {
            get {
                return _channelObj;
            }
            set {
                _channelObj=value;
            }
        }

        public string ip_address;
        public int port;

        public MatlabComChannel(MatlabConectionType _coType, string _ip="", int _port=-1)
        {
            connectionType = _coType;
            ip_address = _ip;
            port = _port;

            if(!_ip.Equals("") && _port != -1)
                InitChannelObj();
        }

        public void InitChannelObj()
        {
            if(typeof(T) == typeof(UDPSender))
                channelObj = (T)Convert.ChangeType(new UDPSender(ip_address, port), typeof(T));

            else if(typeof(T) == typeof(UDPReceiver))
                channelObj = (T)Convert.ChangeType(new UDPReceiver(ip_address, port), typeof(T));

            else if(typeof(T) == typeof(TCPSender))
                channelObj = (T)Convert.ChangeType(new TCPSender(ip_address, port), typeof(T));

            else if(typeof(T) == typeof(TCPReceiver))
                channelObj = (T)Convert.ChangeType(new TCPReceiver(ip_address, port), typeof(T));
        }
    }
}