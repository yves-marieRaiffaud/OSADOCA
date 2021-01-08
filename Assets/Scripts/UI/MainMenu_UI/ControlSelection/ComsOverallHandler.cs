using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ComOps = CommonMethods.CommunicationOps;
using ObjHand = CommonMethods.ObjectsHandling;
using MathOps = CommonMethods.MathsOps;
using Communication;
using System;
using Universe;
using Mathd_Lib;

public class ComsOverallHandler : MonoBehaviour
{
    public const int maxNBConnections=8; // Max connections that can be created
    public List<ComChannelInterface> channels;
    public List<bool> channels_Cor_AreRunning; // List of bool to indicate if the channels, with the same mapping as 'channels', have their coroutine running

    UniverseRunner universe;

    [HideInInspector] public MainPanelIsSetUp panelIsFullySetUp;

    public const string SENDER_DELIMITER = ";";
    // Dictionary that is filled in a method in the 'ComsOverallHandler' script
    public Dictionary<Enum, object> comDataVariables = new Dictionary<Enum, object>() { };
    public Dictionary<Enum, Type> comDataTypes = new Dictionary<Enum, Type>() { };

    void Awake()
    {
        DontDestroyOnLoad(this);

        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();

        channels = new List<ComChannelInterface>();
        channels_Cor_AreRunning = new List<bool>();
    }
    // UI METHOD TO ADD A NEW CONNECTION TO THE 'channels' List<ComChannelInterface>
    public int Add_ComObject(ComChannelParams comChannelParams)
    {
        // Method that creates a new com objects with the speciied ComChannelParams and returns the int index of the object in the 'channels' List<ComChannel>
        switch(comChannelParams.protocol)
        {
            case ComProtocol.UDP_Sender:
                channels.Add(new ComChannel<UDPSender>(comChannelParams));
                break;
            case ComProtocol.UDP_Receiver:
                channels.Add(new ComChannel<UDPReceiver>(comChannelParams));
                break;
            case ComProtocol.TCPIP_Sender: case ComProtocol.TCPIP_Receiver:
                channels.Add(new ComChannel<TCPServer>(comChannelParams));
                break;
        }
        // By default, when creating an object, we are not activating its corresponding coroutine to send/receive data
        channels_Cor_AreRunning.Add(false);
        return channels.Count-1;
    }
    public void Remove_ComObject_At_Index(int removedIndex)
    {
        // Removing comObjects and its corresponding bool in every List of the ComsOverallHandler
        channels.RemoveAt(removedIndex);
        channels_Cor_AreRunning.RemoveAt(removedIndex);
    }

    bool Try_Get_UniverseRunner()
    {
        // Returns the boolean to indicate if the 'universe' variable has been initiliazed with the UniverseRunner component of the active scene
        GameObject uniGO = GameObject.Find("UniverseRunner");
        if(uniGO != null) {
            universe = uniGO.GetComponent<UniverseRunner>();
            return true;
        }
        return false;
    }
    bool Check_ComDataVariables_Init()
    {
        // Returns the boolean to indicate if the 'comDataVariables' Dictionary has been initiliazed and filled with variables 
        if(comDataVariables.Count > 0)
            return true;
        else
            return false;
    }

    void Init_ComDataVariables_Dict()
    {
        bool res = false;
        if(universe == null)
            res = Try_Get_UniverseRunner();
        else
            res = true;
        if(!res) {
            Debug.LogError("Error while initializing the 'comDataVariables' Dictionary: UniverseRunner Component could not be found.");
            return;
        }
        //---------------------
        comDataVariables.Add(ComDataFieldsOut.shipAcc      , universe.activeSC.relativeAcc);
        comDataTypes.Add(ComDataFieldsOut.shipAcc, typeof(Vector3d));

        comDataVariables.Add(ComDataFieldsOut.shipDeltaRot , universe.activeSC.deltaRotation);
        comDataTypes.Add(ComDataFieldsOut.shipDeltaRot, typeof(Quaterniond));

        comDataVariables.Add(ComDataFieldsOut.shipPos      , universe.activeSC.realPosition);
        comDataTypes.Add(ComDataFieldsOut.shipPos, typeof(Vector3d));

        comDataVariables.Add(ComDataFieldsOut.shipVel      , universe.activeSC.relativeVel);
        comDataTypes.Add(ComDataFieldsOut.shipVel, typeof(Vector3d));

        comDataVariables.Add(ComDataFieldsOut.shipVelIncr  , universe.activeSC.deltaRelativeVel);
        comDataTypes.Add(ComDataFieldsOut.shipVelIncr, typeof(Vector3d));
    }

    public IEnumerator SimEnv_Coroutine()
    {
        // COMMON METHODS TO INCLUDE IN EVERY COROUTINE FOR SENDING DATA
        //--------------------------------------------------
        //--------------------------------------------------
        bool res = false;
        if(universe == null)
            res = Try_Get_UniverseRunner();
        else
            res = true;
        if(!res) {
            Debug.LogError("Error while initializing the 'SimEnv_Coroutine': UniverseRunner Component could not be found.");
            yield break;
        }

        bool dictIsReady = Check_ComDataVariables_Init();
        if(!dictIsReady)
            Init_ComDataVariables_Dict();
        //------------------------------
        while(true)
        {
            List<string> strToAssemble = new List<string>();
            foreach(Enum enumObj in ObjHand.GetEnumFlags<ComDataFieldsOut>((ComDataFieldsOut)channels[1].comParams.dataFields))
                strToAssemble.Add(Format_DataComVariable(comDataTypes[enumObj], comDataVariables[enumObj]));

            string dataToSend = AssembleStringList(strToAssemble);
            channels[0].channelObj_generic.Send(dataToSend);

            yield return new WaitForSeconds(1f);
        }
    }
    string Format_DataComVariable(Type variableType, object variable)
    {
        string formatStr = "";
        switch(variableType)
        {
            case Type vec3d when vec3d == typeof(Vector3d):
                formatStr = Format_Vector3d((Vector3d) variable, 5);
                break;
            case Type quatd when quatd == typeof(Quaterniond):
                formatStr = Format_Quaterniond((Quaterniond) variable, 5);
                break;
        }
        return formatStr;
    }
    private string Format_Vector3d(Vector3d inputVec, int nbDigits)
    {
        return MathOps.DoubleToSignificantDigits(inputVec.x, nbDigits) + SENDER_DELIMITER + MathOps.DoubleToSignificantDigits(inputVec.y, nbDigits) + SENDER_DELIMITER + MathOps.DoubleToSignificantDigits(inputVec.z, nbDigits);
    }
    private string Format_Quaterniond(Quaterniond inputVec, int nbDigits)
    {
        return MathOps.DoubleToSignificantDigits(inputVec.X, nbDigits) + SENDER_DELIMITER + MathOps.DoubleToSignificantDigits(inputVec.Y, nbDigits) + SENDER_DELIMITER + MathOps.DoubleToSignificantDigits(inputVec.Z, nbDigits) + SENDER_DELIMITER + MathOps.DoubleToSignificantDigits(inputVec.W, nbDigits);
    }
    string AssembleStringList(List<string> stringsToAssembleList)
    {
        string txt = "";
        int counter = 0;
        int listCount = stringsToAssembleList.Count;
        foreach(string item in stringsToAssembleList) {
            counter++;
            if(counter != listCount)
                txt += item + SENDER_DELIMITER;
            else
                txt += item;
        }
        return txt;
    }

    public void StartChannel(int channelIdx)
    {
        if(channelIdx < channels.Count && channels[channelIdx] != null)
        {
            // Authorized to start the channel
            channels_Cor_AreRunning[channelIdx] = true; // Setting to true
            // StartCoroutine(...);
        }
    }
}