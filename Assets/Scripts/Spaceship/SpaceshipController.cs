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
        get; private set;
    }
    public string lastReceivedOrders {get; private set;}
    private MainNozzle_Control nozzleControl;
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
        nozzleControl = ship.transform.Find("FreeFloating").GetComponent<MainNozzle_Control>();
        ordersComReceiver.channelObj.onDataReceivedEvent.AddListener(delegate {
            On_New_Control_Orders_Received(ordersComReceiver.channelObj.lastReceivedData);
        });
        
        lastReceivedOrders = null;
    }
    //================
    //================
    public void On_New_Control_Orders_Received(string receivedData)
    {
        Debug.Log("New orders have been received from the TCP/IP receiver");
        Debug.LogFormat("ReceivedData : {0}", receivedData);
        /*float[] arg1 = {0f,0f};
        nozzleControl.engineIsActive = true;
        float parsedVal;
        UsefulFunctions.ParseStringToFloat(receivedData, out parsedVal);
        // Make sure that the 'FireEngine' method is executed on the main thread, as it is accessing gameObject/Components
        UnityMainThreadDispatcher.Instance().Enqueue(nozzleControl.FireEngine(arg1, parsedVal));*/
        //nozzleControl.engineIsActive = false;
    }
}