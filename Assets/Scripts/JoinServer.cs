﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class JoinServer : MonoBehaviour
{
    public Text message;

    public Text username;
    public Text ip;
    public Canvas canvasJoin;
    public Canvas textCanvas;

    public Canvas CanvasHost;
    public Canvas CanvasClient;

    public Socket sendSocket;
    public Socket recSocket;
    private int port = 5631;
    private int port2 = 5655;

    // Sockets stuff

    [HideInInspector] public EndPoint ipepServer;
    [HideInInspector] public EndPoint ipepServer2;

    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = false;

    [HideInInspector] public bool messageSent = false;

    // We declare the threads
    [HideInInspector] public Thread sendthread;
    [HideInInspector] public Thread recthread;

    //Tank and spawn
    public GameObject tankPrefab;
    public GameObject spawn;
    public GameObject enemySpawn;
    public GameObject bulletPrefab;

    MemoryStream mem = new MemoryStream();
    private byte[] data;
    //private string json;
    private string jsonHost;

    // We create the class where we will store all the data of the tank to be turned into the smallest json possible
    public class tankClass
    {
        public float hp;
        public Vector3 pos;
        public Quaternion cannonRot;
        public Vector3 cannonPos;
        public List<AimControls.BulletInfo> bulletData = new List<AimControls.BulletInfo>();

        public string message; //Here we store messages sent
    }

    // create the needed classes
    public tankClass hostTankClass;
    public tankClass myTankClass;

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

    private int bulletAmount = 0;

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
        CanvasHost.GetComponent<Canvas>().enabled = false;
        CanvasClient.GetComponent<Canvas>().enabled = false;
    }

    public void Join()
    {
        data = new byte[1024];
        hostTankClass = new tankClass();
        myTankClass = new tankClass();

        string ipString = ip.text;
        string usernameString = "User has Joined: " + username.text;

        // Creating UDP Sockets
        sendSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);
        recSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);

        // Server Endpoint
        ipepServer = new IPEndPoint(IPAddress.Parse(ipString), port);
        ipepServer2 = new IPEndPoint(IPAddress.Any, port2);

        recSocket.Bind(ipepServer2);

        messageSent = true;

        sendthread = new Thread(Send);
        sendthread.Start();

        recthread = new Thread(Rec);
        recthread.Start();

        message.text = "Server joined";
        canvasJoin.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;

        // Create the tank that will be controled by the client
        GameObject joinTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);
        SpriteRenderer sprite = joinTank.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;
        tankInstances.Add(joinTank);

        // Create the tank that will be controled by the host
        GameObject hostTank = Instantiate(tankPrefab, enemySpawn.transform.position,
            transform.rotation);

        tankInstances.Add(hostTank);
    }

    void Update()
    {
        //Make sure there is at least one tank
        if (tankInstances.Count > 0)
        {
            //Disable 2nd tank controls
            tankInstances[1].GetComponent<TankControls>().isEnabled = false;

            // Each frame we update the content of myTankClass
            myTankClass.pos.x = tankInstances[0].transform.position.x;
            myTankClass.pos.y = tankInstances[0].transform.position.y;
            myTankClass.cannonRot = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").rotation;
            myTankClass.cannonPos = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").position;
            myTankClass.hp = tankInstances[0].GetComponent<TankControls>().GetHP();

            //Update list of bullets
            myTankClass.bulletData = tankInstances[0].GetComponentInChildren<AimControls>().bulletData;

            //for (int i = 0; i < myTankClass.bulletInstances.Count; i++)
            //{
            //    myTankClass.bulletInstances[i] = tankInstances[0].GetComponentInChildren<AimControls>().bulletInstances[i];
            //}

            //VictoryScreen when a tank dies
            if (tankInstances[0].GetComponent<TankControls>().GetHP() <= 0)
            {
                CanvasClient.GetComponent<Canvas>().enabled = true;
            }
            else if (tankInstances[1].GetComponent<TankControls>().GetHP() <= 0)
            {
                CanvasHost.GetComponent<Canvas>().enabled = true;
            }

        }
        if (jsonHost != null)
        {
            hostTankClass = JsonUtility.FromJson<tankClass>(jsonHost);

            //Debug.Log(enemyTank.pos.ToString());
            Vector3 newPos = new Vector3(hostTankClass.pos.x, hostTankClass.pos.y, hostTankClass.pos.z);
            tankInstances[1].transform.position = newPos;
            tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").rotation = hostTankClass.cannonRot;
            tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").position = hostTankClass.cannonPos;
            tankInstances[1].GetComponentInChildren<TankControls>().SetHP(hostTankClass.hp);

            //Send message to the chat Canvas if it isn't empty
            if (hostTankClass.message != string.Empty)
            {
                message.text += "/n" + myTankClass.message;
            }

            //Instantiate enemy bullets
            if (hostTankClass.bulletData.Count > 0 && hostTankClass.bulletData[hostTankClass.bulletData.Count - 1] != null)
            {
                if (bulletAmount < hostTankClass.bulletData.Count)
                {
                    Instantiate(bulletPrefab, hostTankClass.bulletData[hostTankClass.bulletData.Count - 1].position, hostTankClass.bulletData[hostTankClass.bulletData.Count - 1].rotation);
                }
            }
            bulletAmount = hostTankClass.bulletData.Count;
        }
    }

    void Send()
    {
        //Send Client IP to Host
        string myIP = GetLocalIPv4();
        byte[] myIPdata = Encoding.ASCII.GetBytes(myIP);
        sendSocket.SendTo(myIPdata, myIPdata.Length, SocketFlags.None, ipepServer);

        while (true)
        {
            // Serialize and send the data inside tankClass
            SerializeJson(myTankClass);
            sendSocket.SendTo(mem.GetBuffer(), mem.GetBuffer().Length, SocketFlags.None, ipepServer);
            //Debug.Log("Message sent: " + myTankClass.hp.ToString() + "POS " + myTankClass.pos.x.ToString() + " " + myTankClass.pos.y.ToString() + "Turret Rot:" + myTankClass.rot.ToString());

        }
    }

    void Rec()
    {
        while (true)
        {
            // We recieve data, then deserialize it
            recSocket.ReceiveFrom(data, ref ipepServer2);
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            jsonHost = reader.ReadString();

            if (recData != null)
            {
                recTrue = true;
            }
        }
    }

    void SerializeJson(tankClass a)
    {
        mem = new MemoryStream();
        string json = JsonUtility.ToJson(a);
        BinaryWriter writer = new BinaryWriter(mem);
        writer.Write(json);
    }
    private void OnApplicationQuit()
    {
        if (sendSocket != null)
        {
            sendSocket.Close();
            sendSocket = null;
        }
        if (recSocket != null)
        {
            recSocket.Close();
            recSocket = null;
        }
        if (recthread != null)
        {
            recthread.Abort();
            recthread = null;
        }
        if (sendthread != null)
        {
            sendthread.Abort();
            sendthread = null;
        }
    }
    public string GetLocalIPv4()
    {
        //Does not parse All possible IPs; There might be IP that are not valid (Virtual Networks)
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.Last(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}