using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;

public class JoinServerTCP : MonoBehaviour
{
    public Text username;
    public Text ip;
    public Text message;

    public Socket newSocket;
    private int port = 5631;

    private bool connected = false;

    // Sockets stuff
    private byte[] data;

    [HideInInspector] public EndPoint ipepServer;

    public Canvas canvasJoin;
    public Canvas textCanvas;

    [HideInInspector] public Thread sendthread;

    public void Join()
    {
        string ipString = ip.text;
        string usernameString = "User has Joined: " + username.text;

         data = new byte[256];

        // Creating TCP Socket
        newSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Stream, ProtocolType.Tcp);

        // Server Endpoint
        ipepServer = new IPEndPoint(IPAddress.Parse(ipString), port);

        data = Encoding.ASCII.GetBytes(usernameString);

        Thread connectwait = new Thread(Connect);
        connectwait.Start();


        sendthread = new Thread(Send);

        message.text = usernameString;

        sendthread.Start();

        canvasJoin.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;
    }

    void Send()
    {
        while (!connected)
        {

        }
        newSocket.Send(data, data.Length, SocketFlags.None);
    }


    void Connect()
    {
        newSocket.Connect(ipepServer);
        connected = true;
    }
}