using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Linq;

public class CreateServerTCP : MonoBehaviour
{

    public Text message;
    public Canvas canvas;
    public Canvas textCanvas;

    public Socket newSocket;
    public Socket client;
    private int port = 5631;

   [HideInInspector] public string hostIP;

    // Sockets stuff
    private byte[] data = { };
    private string recData;
    private bool recTrue = false;


    private bool acceptted = false;
    private int recv;

    private EndPoint ipepClient;

    [HideInInspector] public Thread recthread;

    public void Create()
    {
        data = new byte[256];
        // Creating UDP Socket
        newSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Stream, ProtocolType.Tcp);
        // Server Endpoint
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        ipepClient = (EndPoint)ipep;

        // Binding Socket
        newSocket.Bind(ipep);
        newSocket.Listen(10);

        Thread acceptwait = new Thread(Accept);
        acceptwait.Start();

        recthread = new Thread(Rec);
        recthread.Start();

        message.text = " Server created with IP: " + GetLocalIPv4();
        canvas.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;
    }

    void Accept()
    {
        client = newSocket.Accept();
        acceptted = true;
    }

    void Update()
    {
        if (recTrue)
        {
            message.text = message.text.Replace("\0", "");
            message.text += "\n" + recData;
            Array.Clear(data, 0, data.Length);
            recTrue = false;
        }
    }

    void Rec()
    {
        while (!acceptted)
        {

        }
        while (acceptted)
        {
            client.Receive(data);
            recData = Encoding.ASCII.GetString(data);

            recTrue = true;
        }
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.Last(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}
