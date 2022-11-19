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

    private bool joined = false;

    // Declare Threads
    [HideInInspector] public Thread sendthread;
    [HideInInspector] public Thread recthread;

    //Tank and spawn
    public GameObject tankPrefab;
    public GameObject spawn;
<<<<<<< Updated upstream

    [HideInInspector] public GameObject joinTank;
=======
<<<<<<< Updated upstream
    public GameObject enemySpawn;
=======
    public GameObject enemySpawn; //host tank spawn

    [HideInInspector] public GameObject joinTank;
    [HideInInspector] public GameObject hostTankCopy;

    // vars for serialization

    MemoryStream mem = new MemoryStream();
    private byte[] data;
    private string json;
>>>>>>> Stashed changes
>>>>>>> Stashed changes

    class tankClass
    {
        public float hp;
        public Vector3 pos;
        public Quaternion rot;
    }

    private tankClass myTankClass;
    private tankClass hostTankClass;

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
        
    }

    public void Join()
    {
        data = new byte[256];
        hostTankClass = new tankClass();
        myTankClass = new tankClass();

        string ipString = ip.text;
        string usernameString = "User has Joined: " + username.text;

        // Creating UDP Socket
        newSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);

        // Server Endpoint
        ipepServer = new IPEndPoint(IPAddress.Parse(ipString), port);

        messageSent = true;

        // start the threads
        sendthread = new Thread(Send);
        sendthread.Start();

        recthread = new Thread(Rec);
        recthread.Start();

        message.text = "Server joined";
        canvasJoin.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;

<<<<<<< Updated upstream
        // We instantiate the tank
        joinTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
=======
<<<<<<< Updated upstream
        // Create the tank that will be controled by the client
        GameObject joinTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
>>>>>>> Stashed changes
            transform.rotation);

        SpriteRenderer sprite = joinTank.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;
<<<<<<< Updated upstream
=======
        tankInstances.Add(joinTank);

        // Create the tank that will be controled by the host
        GameObject hostTank = Instantiate(tankPrefab, enemySpawn.transform.position,
            transform.rotation);

        tankInstances.Add(hostTank);
=======
        // We instantiate the tank (our tank)
        joinTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);

        tankInstances.Add(joinTank);

        SpriteRenderer sprite = joinTank.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;

        // We instantiate the tank for the host
        hostTankCopy  = (GameObject)Instantiate(tankPrefab, enemySpawn.transform.position,
            transform.rotation);

        tankInstances.Add(joinTank);
>>>>>>> Stashed changes
>>>>>>> Stashed changes
    }

    void Update()
    {
<<<<<<< Updated upstream
        // Each frame we actualize the content of tankClass
        myTankClass.pos.x = joinTank.transform.position.x;
        myTankClass.pos.y = joinTank.transform.position.y;
        myTankClass.rot = joinTank.GetComponentInChildren<Transform>().Find("Cannon").rotation;
        myTankClass.hp = joinTank.GetComponent<TankControls>().GetHP();
=======
<<<<<<< Updated upstream
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

        
=======
        if (tankInstances.Count > 0)
        {
            // Each frame we actualize the content of tankClass
            myTankClass.pos.x = joinTank.transform.position.x;
            myTankClass.pos.y = joinTank.transform.position.y;
            myTankClass.rot = joinTank.GetComponentInChildren<Transform>().Find("Cannon").rotation;
            myTankClass.hp = joinTank.GetComponent<TankControls>().GetHP();
        }

        // if json is not null, we update the position of the host tank copy
        if (json != null)
        {
            hostTankClass = JsonUtility.FromJson<tankClass>(json);

            Debug.Log(hostTankClass.pos.ToString());
            Vector3 newPos = new Vector3(hostTankClass.pos.x, hostTankClass.pos.y, hostTankClass.pos.z);
            tankInstances[1].transform.position = newPos;
        }
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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

    void Rec()
    {
        while (true)
        {
            // We recieve data, then deserialize it
            newSocket.ReceiveFrom(data, ref ipepServer);
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            json = reader.ReadString();

            Debug.Log(json);

            if (recData != null)
            {
                recTrue = true;
            }
        }
    }

    private void OnApplicationQuit()
    {
        newSocket.Close();
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