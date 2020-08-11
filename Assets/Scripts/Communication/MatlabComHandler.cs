using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Matlab_Communication
{
    public class MatlabComHandler : MonoBehaviour
    {
        public MatlabComChannel<UDPSender> dataVisSenderChannel;

        public MatlabComChannel<TCPServer> simEnvTCPServer;
        public MatlabComChannel<TCPSender> simEnvSenderChannel;

        public MatlabComChannel<TCPServer> controlAlgoTCPServer;
        public MatlabComChannel<TCPReceiver> controlAlgoReceiverChannel;
        //=============
        //=============
        void Awake()
        {
            DontDestroyOnLoad(this);
            //======
            dataVisSenderChannel = new MatlabComChannel<UDPSender>(MatlabConectionType.dataVisualization, 25010, 25010, "127.0.0.1");

            simEnvTCPServer = new MatlabComChannel<TCPServer>(MatlabConectionType.simulationEnv, 25011);
            simEnvSenderChannel = new MatlabComChannel<TCPSender>(MatlabConectionType.simulationEnv, 25011, 25011, "127.0.0.1");

            controlAlgoTCPServer = new MatlabComChannel<TCPServer>(MatlabConectionType.simulationEnv, 25012);
            controlAlgoReceiverChannel = new MatlabComChannel<TCPReceiver>(MatlabConectionType.shipControlOrders, 25012, 25012, "127.0.0.1");
        }

        void OnApplicationQuit()
        {
            // Close every sender/receiver connections
            dataVisSenderChannel.channelObj.Terminate();
            simEnvSenderChannel.channelObj.Terminate();
            controlAlgoReceiverChannel.channelObj.Terminate();
            //===============
            // Terminate TCP servers
            simEnvTCPServer.channelObj.StopServer();
            controlAlgoTCPServer.channelObj.StopServer();
        }
    }
}