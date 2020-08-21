using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using Matlab_Communication;

public class SpaceshipController
{
    public Spaceship ship {get; private set;}
    public MatlabComChannel<TCPServer> ordersComReceiver
    {
        get {
            return ordersComReceiver;
        }
        private set {
            ordersComReceiver=value;
            ordersComReceiver.channelObj.onDataReceivedEvent.AddListener(On_New_Control_Orders_Received);
        }
    }
    public string lastReceivedOrders {get; private set;}
    //================
    //================
    public SpaceshipController(Spaceship _ship)
    {
        ship = _ship;
        ordersComReceiver = null;
        lastReceivedOrders = null;
    }
    
    public SpaceshipController(Spaceship _ship, MatlabComChannel<TCPServer> _ordersComReceiver)
    {
        ship = _ship;
        ordersComReceiver = _ordersComReceiver;
        ordersComReceiver.channelObj.onDataReceivedEvent.AddListener(delegate{
            On_New_Control_Orders_Received(ordersComReceiver.channelObj.lastReceivedData);
        });
        lastReceivedOrders = null;
    }
    //================
    //================
    private void On_New_Control_Orders_Received(string receivedData)
    {
        Debug.Log("New orders have been received from the TCP/IP receiver");
        Debug.LogFormat("ReceivedData : {0}", receivedData);
        MainNozzle_Control nozzleControl = ship.transform.Find("FreeFloating").GetComponent<MainNozzle_Control>();
        float[] arg1 = {0f,0f};
        nozzleControl.engineIsActive = true;
        nozzleControl.FireEngine(arg1, float.Parse(receivedData));
        nozzleControl.engineIsActive = false;
    }
}