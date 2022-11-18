using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
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

    MemoryStream mem = new MemoryStream();

    private bool joined = false;

    [HideInInspector] public Thread sendthread;

    //Tank and spawn
    public GameObject tankPrefab;
    public GameObject spawn;

    class testClass
    {
        public int hp = 10;
        public Vector2 pos = new Vector2(1, 2);
    }

    testClass tank1 = new testClass();

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;

        tank1.hp = 8;
        tank1.pos.x = 0;
        tank1.pos.y = 0;
        
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

        data = new byte[1024];
        data = Encoding.ASCII.GetBytes(usernameString);

        messageSent = true;

        sendthread = new Thread(Send);

        sendthread.Start();
        message.text = "Server joined";
        canvasJoin.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;

        GameObject joinTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);
        SpriteRenderer sprite = joinTank.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;
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
            //if (messageSent)
            //{
            //    newSocket.SendTo(data, data.Length, SocketFlags.None, ipepServer);
            //    messageSent = false;
            //}

            //newSocket.ReceiveFrom(data, ref ipepServer);
            //recData = Encoding.ASCII.GetString(data);

            //if (recData != null)
            //{
            //    recTrue = true;
            //}

            SerializeJson(tank1);
            Debug.Log(mem.GetBuffer().Length.ToString());
            newSocket.SendTo(mem.GetBuffer(), mem.GetBuffer().Length, SocketFlags.None, ipepServer);
            Debug.Log("Message sent: " + tank1.hp.ToString() + " " + tank1.pos.y.ToString() + " " + tank1.pos.x.ToString());

  

        }
    }

    void SerializeJson(testClass a)
    {

        mem = new MemoryStream();
        string json = JsonUtility.ToJson(a);
        BinaryWriter writer = new BinaryWriter(mem);
        writer.Write(json);
        Debug.Log("Serialized");
    }
}