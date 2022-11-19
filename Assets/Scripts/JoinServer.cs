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
    public GameObject enemySpawn;

    class tankClass
    {
        public float hp;
        public Vector2 pos;
        public Quaternion rot;
    }

    private tankClass myTankClass = new tankClass();

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

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

        //data = new byte[1024];
        //data = Encoding.ASCII.GetBytes(usernameString);

        messageSent = true;

        sendthread = new Thread(Send);

        sendthread.Start();
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

            // Each frame we update the content of tankClass
            myTankClass.pos.x = tankInstances[0].transform.position.x;
            myTankClass.pos.y = tankInstances[0].transform.position.y;
            myTankClass.rot = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").rotation;
            myTankClass.hp = tankInstances[0].GetComponent<TankControls>().GetHP();
        }

        
    }

    void Send()
    {
        while (true)
        {
            // Serialize and send the data inside tankClass
            SerializeJson(myTankClass);
            Debug.Log(mem.GetBuffer().Length.ToString());
            newSocket.SendTo(mem.GetBuffer(), mem.GetBuffer().Length, SocketFlags.None, ipepServer);
            Debug.Log("Message sent: " + myTankClass.hp.ToString() + "POS " + myTankClass.pos.x.ToString() + " " + myTankClass.pos.y.ToString() + "Turret Rot:" + myTankClass.rot.ToString());

        }
    }

    void SerializeJson(tankClass a)
    {
        mem = new MemoryStream();
        string json = JsonUtility.ToJson(a);
        BinaryWriter writer = new BinaryWriter(mem);
        writer.Write(json);
        Debug.Log("Serialized");
    }
}