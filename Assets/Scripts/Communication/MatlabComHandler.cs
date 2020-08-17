using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Matlab_Communication
{
    public class MatlabComHandler : MonoBehaviour
    {
        [Header("UDP Data Visualization Sender")]
        [NonSerialized] public MatlabComChannel<UDPSender> dataVisSenderChannel;
        public bool enableDataVisSenderChannel;
        public float senderFrequencyDataVis; // in s
        //=============
        [Header("TCP/IP Simulation Env Sender")]
        public MatlabComChannel<TCPServer> simEnvTCPServer;
        public bool enableSimEnvTCPServer;
        public float senderFrequencySimEnv; // in s
        //=============
        [Header("TCP/IP Control Algo Orders Receiver")]
        public MatlabComChannel<TCPServer> controlAlgoTCPServer;
        public bool enableControlAlgoTCPServer;
        public float receiverFrequencyControlAlgo; // in s
        //=============
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            if(enableDataVisSenderChannel)
            {
                MatlabConectionType coType = MatlabConectionType.dataVisualization;
                MatlabSendReceiveType srType = MatlabSendReceiveType.classExplicit;
                ComChannelParams dataParams = new ComChannelParams("127.0.0.1", 25010, coType, srType, 25010);
                dataVisSenderChannel = new MatlabComChannel<UDPSender>(dataParams);
            }
            //======
            if(enableSimEnvTCPServer)
            {
                MatlabConectionType coType = MatlabConectionType.simulationEnv;
                MatlabSendReceiveType srType = MatlabSendReceiveType.sendOnly;
                ComChannelParams dataParams = new ComChannelParams("127.0.0.1", 25011, coType, srType, 25011);
                simEnvTCPServer = new MatlabComChannel<TCPServer>(dataParams);
            }
            //======
            if(enableControlAlgoTCPServer)
            {
                MatlabConectionType coType = MatlabConectionType.shipControlOrders;
                MatlabSendReceiveType srType = MatlabSendReceiveType.receiveOnly;
                ComChannelParams dataParams = new ComChannelParams("127.0.0.1", 25012, coType, srType, 25012);
                controlAlgoTCPServer = new MatlabComChannel<TCPServer>(dataParams);
            }
        }

        void OnApplicationQuit()
        {
            // Close UDP Sender Connection
            if(dataVisSenderChannel != null) {
                if(dataVisSenderChannel.channelObj != null)
                    dataVisSenderChannel.channelObj.Terminate();
            }
            //===============
            // Terminate TCP servers
            if(simEnvTCPServer != null) {
                if(simEnvTCPServer.channelObj != null)
                    simEnvTCPServer.channelObj.StopServer();
            }
            //======
            if(controlAlgoTCPServer != null) {
                if(controlAlgoTCPServer.channelObj != null)
                    controlAlgoTCPServer.channelObj.StopServer();
            }
        }
    }
}