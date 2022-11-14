using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;

public class JoinServer : MonoBehaviour
{
    public Text message;

    public Text username;
    public Text ip;
    public Canvas canvasJoin;
    public Canvas textCanvas;

    public Socket newSocket;
    private int port = 5631;

    // Sockets stuff
    private byte[] data;

    [HideInInspector] public EndPoint ipepServer;

    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = false;

    [HideInInspector] public bool messageSent = false;


    private bool joined = false;

    [HideInInspector] public Thread sendthread;

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void Join()
    {
        string ipString = ip.text;
        string usernameString = "User has Joined: " + username.text;

        // Creating UDP Socket
        newSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);

        // Server Endpoint
        ipepServer = new IPEndPoint(IPAddress.Parse(ipString), port);

        data = new byte[256];
        data = Encoding.ASCII.GetBytes(usernameString);

        messageSent = true;

        sendthread = new Thread(Send);

        sendthread.Start();
        message.text = "Server joined";
        canvasJoin.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;
    }

    void Update()
    {
        if (recTrue)
        {
            message.text = message.text.Replace("\0", "");
            message.text += "\n" + recData;


            //Clear
            Array.Clear(data, 0, data.Length);
            recTrue = false;
        }
    }

    void Send()
    {
        while (true)
        {
            if (messageSent)
            {
                newSocket.SendTo(data, data.Length, SocketFlags.None, ipepServer);
                messageSent = false;
            }

            newSocket.ReceiveFrom(data, ref ipepServer);
            recData = Encoding.ASCII.GetString(data);

            if (recData != null)
            {
                recTrue = true;
            }

        }
    }
}