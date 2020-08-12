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
        lastReceivedOrders = null;
    }
    //================
    //================
    private void On_New_Control_Orders_Received(byte[] receivedData)
    {
        Debug.Log("New orders have been received from the TCP/IP receiver");
        Debug.Log("ReceivedData : " + string.Join(System.Environment.NewLine, receivedData));
    }
}