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

    public GameObject joinTank;

    public TankControls controls;

    class tankUpdater
    {
        public float hp;
        public Vector2 pos;
        public Quaternion rot;
    }

    tankUpdater tank1 = new tankUpdater();

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

        data = new byte[1024];
        data = Encoding.ASCII.GetBytes(usernameString);

        messageSent = true;

        sendthread = new Thread(Send);

        sendthread.Start();
        message.text = "Server joined";
        canvasJoin.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;

        joinTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);

        SpriteRenderer sprite = joinTank.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;
    }

    void Update()
    {
        //if (recTrue)
        //{
        //    message.text = message.text.Replace("\0", "");
        //    message.text += "\n" + recData;


        //    //Clear
        //    Array.Clear(data, 0, data.Length);
        //    recTrue = false;
        //}

        tank1.pos.x = joinTank.transform.position.x;
        tank1.pos.y = joinTank.transform.position.y;
        tank1.rot = joinTank.GetComponentInChildren<Transform>().Find("Cannon").rotation;
        tank1.hp = joinTank.GetComponent<TankControls>().GetHP();
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
            Debug.Log("Message sent: " + tank1.hp.ToString() + "POS " + tank1.pos.x.ToString() + " " + tank1.pos.y.ToString() + "Turret Rot:" + tank1.rot.ToString());

  

        }
    }

    void SerializeJson(tankUpdater a)
    {

        mem = new MemoryStream();
        string json = JsonUtility.ToJson(a);
        BinaryWriter writer = new BinaryWriter(mem);
        writer.Write(json);
        Debug.Log("Serialized");
    }
}