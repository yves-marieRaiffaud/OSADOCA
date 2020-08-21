using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Mathd_Lib;
using Fncs = UsefulFunctions;

namespace Matlab_Communication
{
    public class MatlabComHandler : MonoBehaviour
    {
        [Header("UniverseRunner Instance")]
        public UniverseRunner universe;
        //=============
        [Header("UDP Data Visualization Sender")]
        public bool enableDataVisSenderChannel;
        public float senderFrequencyDataVis; // in s
        [NonSerialized] public MatlabComChannel<UDPSender> dataVisSenderChannel;
        //=============
        [Header("TCP/IP Simulation Env Sender")]
        public bool enableSimEnvTCPServer;
        public float senderFrequencySimEnv; // in s
        private IEnumerator simEnvCoroutine;
        private bool simEnvCoroutineIsRunning;
        [NonSerialized] public MatlabComChannel<TCPServer> simEnvTCPServer;
        //=============
        [Header("TCP/IP Control Algo Orders Receiver")]
        public bool enableControlAlgoTCPServer;
        public float receiverFrequencyControlAlgo; // in s
        [NonSerialized] public MatlabComChannel<TCPServer> controlAlgoTCPServer;
        //=============
        [Header("UI Button 'Matlab Connection Init'")]
        public Button matlabCoInit_btn;
        //=============
        void Awake()
        {
            DontDestroyOnLoad(this);
            if(matlabCoInit_btn != null)
                matlabCoInit_btn.onClick.AddListener(MatlabStartSequence);
        }

        private string GatherData_simEnv()
        {
            // Returns one string that contains the simulation envrionment data that is sent to Matlab
            //========================================================================================
            //###############.......DATA STRUCTURE.......##############
            // value separator = ' ; '
            // Ship acceleration (in m/s^2)
            // Ship velocity (in m/s)
            // Ship world position in the scene (in unity units)
            // 
            //###############.......++++++++++++++.......##############
            string data = "";
            //====
            Vector3d acc = universe.activeSpaceship.orbitedBodyRelativeAcc;
            Vector3d speed = universe.activeSpaceship.orbitedBodyRelativeVel;
            Vector3d worldPos = new Vector3d(universe.activeSpaceship.transform.position);
            data = Format_Vector3d(acc, 5) + " ; " + Format_Vector3d(speed, 5) + " ; " + Format_Vector3d(worldPos, 5);
            Debug.Log(data);
            return data;
        }

        private string Format_Vector3d(Vector3d inputVec, int nbDigits)
        {
            return Fncs.DoubleToSignificantDigits(inputVec.x, nbDigits) + " ; " + Fncs.DoubleToSignificantDigits(inputVec.y, nbDigits) + " ; " + Fncs.DoubleToSignificantDigits(inputVec.z, nbDigits);
        }

        private IEnumerator SimulationEnv_Sending_Coroutine()
        {
            while (true)
            {
                string data = GatherData_simEnv();
                simEnvTCPServer.channelObj.Send(data);
                yield return new WaitForSeconds(senderFrequencySimEnv);
            }
        }

        void Start()
        {
            simEnvCoroutineIsRunning = false;
            //============
            if(enableDataVisSenderChannel)
            {
                MatlabConectionType coType = MatlabConectionType.dataVisualization;
                MatlabSendReceiveType srType = MatlabSendReceiveType.classExplicit;
                ComChannelParams dataParams = new ComChannelParams("169.254.183.22", 25010, coType, srType, 25010);
                dataVisSenderChannel = new MatlabComChannel<UDPSender>(dataParams);
            }
            //======
            if(enableSimEnvTCPServer)
            {
                simEnvCoroutine = SimulationEnv_Sending_Coroutine();
                MatlabConectionType coType = MatlabConectionType.simulationEnv;
                MatlabSendReceiveType srType = MatlabSendReceiveType.sendOnly;
                ComChannelParams dataParams = new ComChannelParams("169.254.183.22", 25011, coType, srType, 25011);
                simEnvTCPServer = new MatlabComChannel<TCPServer>(dataParams);
                if(simEnvTCPServer != null) {
                    StartCoroutine(simEnvCoroutine);
                    simEnvCoroutineIsRunning = true;
                }
            }
            //======
            if(enableControlAlgoTCPServer)
            {
                MatlabConectionType coType = MatlabConectionType.shipControlOrders;
                MatlabSendReceiveType srType = MatlabSendReceiveType.receiveOnly;
                ComChannelParams dataParams = new ComChannelParams("169.254.183.22", 25012, coType, srType, 25012);
                controlAlgoTCPServer = new MatlabComChannel<TCPServer>(dataParams);
            }
        }

        private void MatlabStartSequence()
        {
            if(simEnvTCPServer != null)
            {
                //simEnvTCPServer.
            }
        }

        void OnApplicationQuit()
        {
            StopAllCoroutines();
            //==================
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