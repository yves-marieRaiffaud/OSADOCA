using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Matlab_Communication
{
    public class MatlabComHandler : MonoBehaviour
    {
        public MatlabComChannel<UDPSender> dataVisSenderChannel;
        public MatlabComChannel<TCPServer> simEnvTCPServer;
        public MatlabComChannel<TCPServer> controlAlgoTCPServer;
        //=============
        //=============
        void Awake()
        {
            DontDestroyOnLoad(this);
            //======
            //dataVisSenderChannel = new MatlabComChannel<UDPSender>(MatlabConectionType.dataVisualization, MatlabSendReceiveType.classExplicit, 25010, 25010);
            //simEnvTCPServer = new MatlabComChannel<TCPServer>(MatlabConectionType.simulationEnv, MatlabSendReceiveType.sendOnly, 25011, 25011);
            //controlAlgoTCPServer = new MatlabComChannel<TCPServer>(MatlabConectionType.simulationEnv, MatlabSendReceiveType.receiveOnly, 25012, 25012);
        }

        void OnApplicationQuit()
        {
            // Close UDP Sender Connection
            if(dataVisSenderChannel.channelObj != null)
                dataVisSenderChannel.channelObj.Terminate();
            //===============
            // Terminate TCP servers
            if(simEnvTCPServer.channelObj != null)
                simEnvTCPServer.channelObj.StopServer();

            if(controlAlgoTCPServer.channelObj != null)
                controlAlgoTCPServer.channelObj.StopServer();
        }
    }
}