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
        private T _channelObj;
        public T channelObj
        {
            get {
                return _channelObj;
            }
            private set {
                _channelObj=value;
            }
        }

        public string IP;
        public int port;

        public string defaultIP;
        public int defaultPort;

        public MatlabComChannel(MatlabConectionType _coType, int _port=-1, int _defaultPort=5000, string _ip="", string _defaultIP="127.0.0.1")
        {
            connectionType = _coType;
            IP = _ip;
            port = _port;
            defaultIP = _defaultIP;
            defaultPort = _defaultPort;

            if((!Fncs.IP_AddressIsValid(IP) && port > -1) || typeof(T) == typeof(TCPServer))
                InitChannelObj();
        }

        public void InitChannelObj()
        {
            if(typeof(T) == typeof(UDPSender))
                channelObj = (T)Convert.ChangeType(new UDPSender(IP, port), typeof(T));

            else if(typeof(T) == typeof(UDPReceiver))
                channelObj = (T)Convert.ChangeType(new UDPReceiver(IP, port), typeof(T));

            else if(typeof(T) == typeof(TCPSender))
                channelObj = (T)Convert.ChangeType(new TCPSender(IP, port), typeof(T));

            else if(typeof(T) == typeof(TCPReceiver))
                channelObj = (T)Convert.ChangeType(new TCPReceiver(IP, port), typeof(T));

            else if(typeof(T) == typeof(TCPServer))
                channelObj = (T)Convert.ChangeType(new TCPServer(port), typeof(T));
        }
    }
}