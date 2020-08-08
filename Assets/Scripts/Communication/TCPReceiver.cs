using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TCPReceiver : MonoBehaviour
{
    public string IP = "127.0.0.1";
    public int port;
    private TcpListener server;
    private Thread _serverThread;
    private IReceiverObserver _Observer;
    private TcpClient client;

    void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize objects.
    /// </summary>
    public void Initialize()
    {
        _serverThread = new Thread(new ThreadStart(ReceiveData));
        _serverThread.IsBackground = true;
        _serverThread.Start();
    }

    public void SetObserver(IReceiverObserver observer)
    {
        _Observer = observer;
    }

    /// <summary>
    /// Receive data with pooling.
    /// </summary>
    private void ReceiveData()
    {
        try
        {
            server = new TcpListener(IPAddress.Parse(IP), port);
            server.Start();

            Byte[] bytes = new Byte[256];
            String data = null;

            while(true)
            {
                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");
                data = null;
                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;
                // Loop to receive all the data sent by the client.
                while((i = stream.Read(bytes, 0, bytes.Length))!=0)
                {
                    double[] values = new double[bytes.Length / 8];
                    Buffer.BlockCopy(bytes, 0, values, 0, values.Length * 8);
                    if (_Observer != null)
                        _Observer.OnDataReceived(values);
                    Debug.Log(">>>>");

                    // Translate data bytes to a ASCII string.
                    //data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    //Console.WriteLine("Received: {0}", data);

                    // Process the data sent by the client.
                    //data = data.ToUpper();

                    /*byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", data);*/
                }
            }
        }
        catch(SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
    }

    /// <summary>
    /// Deinitialize everything on quiting the application.Or you might get error in restart.
    /// </summary>
    private void OnApplicationQuit()
    {
        try
        {
            _serverThread.Abort();
            _serverThread = null;
            client.Close();
            server.Stop();
        }
        catch (Exception err)
        {
            Debug.Log("<color=red>" + err.Message + "</color>");
        }
    }
}