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
            Vector3d acc = universe.activeSpaceship.orbitedBodyRelativeAcc;
            Vector3d speed = universe.activeSpaceship.orbitedBodyRelativeVel;
            Vector3d speedIncr = universe.activeSpaceship.orbitedBodyRelativeVelIncr;
            Vector3d worldPos = new Vector3d(universe.activeSpaceship.transform.position);
            Quaterniond deltaRotation = universe.activeSpaceship.deltaRotation;
            //=====
            List<string> stringsToAssembleList = new List<string>();
            stringsToAssembleList.Add(Format_Vector3d(acc, 5));
            stringsToAssembleList.Add(Format_Vector3d(speed, 5));
            stringsToAssembleList.Add(Format_Vector3d(speedIncr, 5));
            stringsToAssembleList.Add(Format_Vector3d(worldPos, 5));
            stringsToAssembleList.Add(Format_Quaterniond(deltaRotation, 5));
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
                    txt += item + " ; ";
                else
                    txt += item;
            }
            return txt;
        }

        private string Format_Vector3d(Vector3d inputVec, int nbDigits)
        {
            return Fncs.DoubleToSignificantDigits(inputVec.x, nbDigits) + " ; " + Fncs.DoubleToSignificantDigits(inputVec.y, nbDigits) + " ; " + Fncs.DoubleToSignificantDigits(inputVec.z, nbDigits);
        }

        private string Format_Quaterniond(Quaterniond inputVec, int nbDigits)
        {
            return Fncs.DoubleToSignificantDigits(inputVec.X, nbDigits) + " ; " + Fncs.DoubleToSignificantDigits(inputVec.Y, nbDigits) + " ; " + Fncs.DoubleToSignificantDigits(inputVec.Z, nbDigits) + " ; " + Fncs.DoubleToSignificantDigits(inputVec.W, nbDigits);
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