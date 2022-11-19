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
using System.Linq;

public class CreateServer : MonoBehaviour
{
    //Chat and messages
    public Text message;
    public Canvas canvas;
    public Canvas textCanvas;

    // Socket
    [HideInInspector] public Socket newSocket;
    private int port = 5631;

    [HideInInspector] public string hostIP;

    // More sockets stuff

    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = false;
    [HideInInspector] public bool messageSent = false;

    [HideInInspector] public EndPoint ipepClient;

    // Declare Threads
    [HideInInspector] public Thread recthread;
    [HideInInspector] public Thread sendthread;

    // vars for serialization

    MemoryStream mem = new MemoryStream();
    private byte[] data;
    private string json;

    // We create the class where we will store all the data of the tank
    class tankClass
    {
        public float hp;
        public Vector3 pos;
        public Quaternion rot;
    }

    // The class that will store the values of the client tank
    private tankClass enemyTankClass;
    private tankClass myTankClass;

    //Tank and spawn
    public GameObject tankPrefab;
    public GameObject spawn;
    public GameObject enemySpawn; //client tank spawn

    [HideInInspector] public GameObject hostTank;
    [HideInInspector] public GameObject clientTankCopy;

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void Create()
    {
        data = new byte[256];
        enemyTankClass = new tankClass();
        myTankClass = new tankClass();

        // Creating UDP Socket
        newSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);
        // Server Endpoint
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        ipepClient = (EndPoint)ipep;

        // Binding Socket
        newSocket.Bind(ipep);

        recthread = new Thread(Rec);
        recthread.Start();

        sendthread = new Thread(Send);
        sendthread.Start();

        message.text = " Server created with IP: " + GetLocalIPv4();
        canvas.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;


        // Create the tank that will be controled by the host (us)
        hostTank = Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);

        tankInstances.Add(hostTank);

<<<<<<< Updated upstream
        //hostTankControls.isPlayer = false;

        //TankControls isPlayer=hostTank.GetComponent<TankControls>();
        //isPlayer.DisableTank();

        //Debug.Log(tankInstances.Count);

=======
<<<<<<< Updated upstream
>>>>>>> Stashed changes
        // Create the tank that will be controled by the client
        GameObject clientTank = (GameObject)Instantiate(tankPrefab, enemySpawn.transform.position, transform.rotation);
        SpriteRenderer sprite = clientTank.GetComponent<SpriteRenderer>();
=======
        //hostTankControls.isPlayer = false;

        //TankControls isPlayer=hostTank.GetComponent<TankControls>();
        //isPlayer.DisableTank();

        //Debug.Log(tankInstances.Count);

        // Create the tank that will be controled by the client (not us)
        clientTankCopy = (GameObject)Instantiate(tankPrefab, enemySpawn.transform.position, transform.rotation);
        SpriteRenderer sprite = clientTankCopy.GetComponent<SpriteRenderer>();
>>>>>>> Stashed changes
        sprite.color = Color.red;

        tankInstances.Add(clientTankCopy);
    }

    void Update()
    {
<<<<<<< Updated upstream
        //Disable 2nd tank controls
        if(tankInstances.Count>0)
=======
<<<<<<< Updated upstream
        //Make sure there is at least one tank
=======
        // we update the class with our information
        if (tankInstances.Count > 0)
        {
            myTankClass.pos.x = tankInstances[0].transform.position.x;
            myTankClass.pos.y = tankInstances[0].transform.position.y;
            myTankClass.rot = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").rotation;
            myTankClass.hp = tankInstances[0].GetComponent<TankControls>().GetHP();
        }

        //Disable 2nd tank controls
>>>>>>> Stashed changes
        if (tankInstances.Count>0)
>>>>>>> Stashed changes
        {
            tankInstances[1].GetComponent<TankControls>().isEnabled = false;
        }

        // if json is not null, we update the position of the client tank copy
        if (json != null)
        {
            enemyTankClass = JsonUtility.FromJson<tankClass>(json);
            
            Debug.Log(enemyTankClass.pos.ToString());
            Vector3 newPos = new Vector3(enemyTankClass.pos.x, enemyTankClass.pos.y, enemyTankClass.pos.z);
            tankInstances[1].transform.position = newPos;
        }
    }

    void Send()
    {
        while (true)
        {
            // Serialize and send the data inside tankClass
            SerializeJson(myTankClass);
            Debug.Log(mem.GetBuffer().Length.ToString());
            newSocket.SendTo(mem.GetBuffer(), mem.GetBuffer().Length, SocketFlags.None, ipepClient);
            Debug.Log("Message sent: " + myTankClass.hp.ToString() + "POS " + myTankClass.pos.x.ToString() + " " + myTankClass.pos.y.ToString() + "Turret Rot:" + myTankClass.rot.ToString());
        }
    }
    void Rec()
    {
        while (true)
        {
            // We recieve data, then deserialize it
            newSocket.ReceiveFrom(data, ref ipepClient);
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

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.Last(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
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