using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Mathd_Lib;
using Universe;
using MathsOps = CommonMethods.MathsOps;

namespace Communication
{
    public class ComsCoroutine : MonoBehaviour
    {
        public const string SENDER_DELIMITER = ";";

        [Header("UniverseRunner Instance")]
        public UniverseRunner universe;
        //=============
        [Header("UDP Data Visualization Sender")]
        public bool enableDataVisSenderChannel;
        public float senderFrequencyDataVis; // in s
        [NonSerialized] public ComChannel<UDPSender> dataVisSenderChannel;
        //=============
        [Header("TCP/IP Simulation Env Sender")]
        public bool enableSimEnvTCPServer;
        public float senderFrequencySimEnv; // in s
        private IEnumerator simEnvCoroutine;
        private IEnumerator shipDeltaRotCoroutine;
        private bool simEnvCoroutineIsRunning;
        [NonSerialized] public ComChannel<TCPServer> simEnvTCPServer;
        //=============
        [Header("TCP/IP Control Algo Orders Receiver")]
        public bool enableControlAlgoTCPServer;
        public float receiverFrequencyControlAlgo; // in s
        [NonSerialized] public ComChannel<TCPServer> controlAlgoTCPServer;
        //=============
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private string GatherData_simEnv()
        {
            // Returns one string that contains the simulation envrionment data that is sent to Matlab
            //========================================================================================
            //###############.......DATA STRUCTURE.......##############
            // value separator = ' ; '
            // Ship acceleration (in m/s^2)
            // Ship velocity (in m/s)
            // Ship velocity increment (in m/s)
            // Ship world position in the scene (in unity units)
            // Ship Delta Rotation Quaterniond
            //###############.......++++++++++++++.......##############
            string data = "";
            //====
            Vector3d acc = universe.activeSC.relativeAcc;
            Vector3d speed = universe.activeSC.relativeVel;
            //Vector3d speedIncr = universe.activeSC.orbitedBodyRelativeVelIncr;
            Vector3d worldPos = new Vector3d(universe.activeSC.transform.position);
            //Quaterniond deltaRotation = universe.activeSpaceship.deltaRotation;
            //=====
            List<string> stringsToAssembleList = new List<string>();
            stringsToAssembleList.Add(Format_Vector3d(acc, 5));
            stringsToAssembleList.Add(Format_Vector3d(speed, 5));
            //stringsToAssembleList.Add(Format_Vector3d(speedIncr, 5));
            stringsToAssembleList.Add(Format_Vector3d(worldPos, 5));
            //stringsToAssembleList.Add(Format_Quaterniond(deltaRotation, 5));
            //=====
            data = AssembleStringList(stringsToAssembleList);
            return data;
        }
        private string AssembleStringList(List<string> stringsToAssembleList)
        {
            string txt = "";
            int counter = 0;
            int listCount = stringsToAssembleList.Count;
            foreach(string item in stringsToAssembleList)
            {
                counter++;
                if(counter != listCount)
                    txt += item + SENDER_DELIMITER;
                else
                    txt += item;
            }
            return txt;
        }
        private string Format_Vector3d(Vector3d inputVec, int nbDigits)
        {
            return MathsOps.DoubleToSignificantDigits(inputVec.x, nbDigits) + SENDER_DELIMITER + MathsOps.DoubleToSignificantDigits(inputVec.y, nbDigits) + SENDER_DELIMITER + MathsOps.DoubleToSignificantDigits(inputVec.z, nbDigits);
        }
        private string Format_Quaterniond(Quaterniond inputVec, int nbDigits)
        {
            return MathsOps.DoubleToSignificantDigits(inputVec.X, nbDigits) + SENDER_DELIMITER + MathsOps.DoubleToSignificantDigits(inputVec.Y, nbDigits) + SENDER_DELIMITER + MathsOps.DoubleToSignificantDigits(inputVec.Z, nbDigits) + SENDER_DELIMITER + MathsOps.DoubleToSignificantDigits(inputVec.W, nbDigits);
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
            if(universe == null)
                return;
            simEnvCoroutineIsRunning = false;
            //============
            if(enableDataVisSenderChannel)
            {
                ComProtocol protocol= ComProtocol.UDP_Sender;
                ComConectionType coType = ComConectionType.dataVisualization;
                ComSendReceiveType srType = ComSendReceiveType.classExplicit;
                ComChannelParams dataParams = new ComChannelParams("169.254.183.22", 25010, protocol, coType, srType, 25010);
                dataVisSenderChannel = new ComChannel<UDPSender>(dataParams);
            }
            //======
            if(enableSimEnvTCPServer)
            {
                simEnvCoroutine = SimulationEnv_Sending_Coroutine();
                //shipDeltaRotCoroutine = universe.activeSC.DeltaRotation_Coroutine(senderFrequencySimEnv);
                ComProtocol protocol= ComProtocol.TCPIP_Sender;
                ComConectionType coType = ComConectionType.simulationEnv;
                ComSendReceiveType srType = ComSendReceiveType.sendOnly;
                ComChannelParams dataParams = new ComChannelParams("169.254.183.22", 25011, protocol, coType, srType, 25011);
                simEnvTCPServer = new ComChannel<TCPServer>(dataParams);
                if(simEnvTCPServer != null) {
                    StartCoroutine(simEnvCoroutine);
                    //StartCoroutine(shipDeltaRotCoroutine);
                    simEnvCoroutineIsRunning = true;
                }
            }
            //======
            if(enableControlAlgoTCPServer)
            {
                ComProtocol protocol= ComProtocol.TCPIP_Receiver;
                ComConectionType coType = ComConectionType.shipControlOrders;
                ComSendReceiveType srType = ComSendReceiveType.receiveOnly;
                ComChannelParams dataParams = new ComChannelParams("169.254.183.22", 25012, protocol, coType, srType, 25012);
                controlAlgoTCPServer = new ComChannel<TCPServer>(dataParams);
            }
        }

        void OnApplicationQuit()
        {
            StopAllCoroutines();
            if(shipDeltaRotCoroutine != null)
                StopCoroutine(shipDeltaRotCoroutine);
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