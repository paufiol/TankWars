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
using System.Text.RegularExpressions;

public class CreateServer : MonoBehaviour
{
    // UI
    public Text message;
    public Canvas canvas;
    public Canvas textCanvas;
    public Text winOrLose;

    [HideInInspector] public string inputMessage; // Here we store the string that the user enters
    private string helperString; // What this string does is the following: when the host sends a chat message, the host first prints it on screen,
                                 // then equals the message to this string. (if this != inputMessage)
    private bool printClientInfo = false;
    private string clientInfo;

    // Sockets stuff

    [HideInInspector] public Socket recSocket;
    [HideInInspector] public Socket sendSocket;
    private int port = 5631;
    private int port2 = 5655;

    [HideInInspector] public string hostIP;
    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = false;
    [HideInInspector] public bool messageSent = false;

    [HideInInspector] public EndPoint ipepClient;
    [HideInInspector] public EndPoint ipepClient2;

    // Threads
    [HideInInspector] public Thread recthread;
    [HideInInspector] public Thread sendthread;

    // Packets
    MemoryStream mem = new MemoryStream();
    private byte[] data;
    private string json;
    private string jsonClient;
    private bool isFirstMessage = true;

    public string clientIP;

    

    // We create the class where we will store all the data of the tank and anything else that we want to send
    class Package
    {
        // Tank info
        public float hp;
        public Vector3 pos;
        public Quaternion cannonRot;
        public Vector3 cannonPos;
        public List<AimControls.BulletInfo> bulletData=new List<AimControls.BulletInfo>();

        // Other
        public string message; // Here we store messages that we want to send
    }


    private Package packageToSend;
    private Package packageToReceive; // Technically its 'received' and not 'to receive'

    //Tank and spawn
    public GameObject tankPrefab;
    public GameObject spawn;
    public GameObject enemySpawn;
    public GameObject bulletPrefab;

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

    private int bulletAmount = 0;

    // For the restart
    private bool restartEnabled;
    private bool hasRestarted;

    void Start()
    {
        restartEnabled = false;
        hasRestarted = false;
        textCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void Create()
    {
        data = new byte[1024];
        packageToReceive = new Package();
        packageToSend = new Package();

        // Creating UDP Sockets
        recSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);

        sendSocket = new Socket(AddressFamily.InterNetwork,
                              SocketType.Dgram, ProtocolType.Udp);
        // Server Endpoint
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        ipepClient = (EndPoint)ipep;

        // Binding Socket
        recSocket.Bind(ipep);

        recthread = new Thread(Rec);
        recthread.Start();

        sendthread = new Thread(Send);
        sendthread.Start();

        message.text = " Server created with IP: " + GetLocalIPv4();
        canvas.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;


        // Create the tank that will be controled by the host
        GameObject hostTank = Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);

        tankInstances.Add(hostTank);

        // Create the tank that will be controled by the client
        GameObject clientTank = (GameObject)Instantiate(tankPrefab, enemySpawn.transform.position, transform.rotation);
        SpriteRenderer sprite = clientTank.GetComponent<SpriteRenderer>();
        SpriteRenderer sprite2 = clientTank.GetComponentInChildren<SpriteRenderer>();
        sprite.color = Color.red;
        sprite2.color = Color.red;

        tankInstances.Add(clientTank);
    }

    void Update()
    {
        //Check is the tanks exist
        if (tankInstances.Count>0)
        {
            //Disable 2nd tank controls
            tankInstances[1].GetComponent<TankControls>().isEnabled = false;

            // Each frame we update the content of myTankClass
            UpdatePackage();

            if (jsonClient != null)
            {
                // Each frame we update the world state (everything that is printed on screen)
                UpdateWorldState();

                //Win/Lose condition
                if (tankInstances[1].GetComponentInChildren<TankControls>().GetHP() <= 0)
                {
                    winOrLose.text = "YOU WIN";
                    tankInstances[0].GetComponent<TankControls>().isEnabled = false;
                    restartEnabled = true;
                }
                else if(tankInstances[0].GetComponentInChildren<TankControls>().GetHP() <= 0)
                {
                    winOrLose.text = "YOU LOSE";
                    tankInstances[0].GetComponent<TankControls>().isEnabled = false;
                    restartEnabled = true;
                }
                else
                {
                    winOrLose.text = "";
                }
            }
            if (Input.GetKey(KeyCode.R) && restartEnabled)
            {
                tankInstances[0].transform.position = spawn.transform.position;
                
            }
        }
    }
    void Send()
    {
        while (true)
        {
            SerializeJson(packageToSend);

            if (!isFirstMessage) //wait until the message with the client IP is functioning.
            {
                sendSocket.SendTo(mem.GetBuffer(), mem.GetBuffer().Length, SocketFlags.None, ipepClient2);
            }
        }
    }// Thread
    void Rec() // Thread
    {
        while (true)
        {
            // We recieve data, then deserialize it
            recSocket.ReceiveFrom(data, ref ipepClient);
            
            if (isFirstMessage)
            {
                //First Message is receiving the client IP, and is thus handled differently
                clientIP = Encoding.ASCII.GetString(data);

                Debug.Log("client IP: "+ clientIP);
                clientInfo = "\n Has joined client with IP: " + clientIP; // Should change it to name
                clientIP = Regex.Replace(clientIP, "\0", "");

                IPEndPoint ipep2 = new IPEndPoint(IPAddress.Parse(clientIP), port2);
                ipepClient2 = (EndPoint)ipep2;

                printClientInfo = true;
                isFirstMessage = false;
            }
            else
            {
                MemoryStream stream = new MemoryStream(data);
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(0, SeekOrigin.Begin);
                jsonClient = reader.ReadString();

                //Debug.Log(json);
            }

            if (recData != null)
            {
                recTrue = true;
            }
        }
    }

    void UpdatePackage()
    {
        packageToSend.pos.x = tankInstances[0].transform.position.x;
        packageToSend.pos.y = tankInstances[0].transform.position.y;
        packageToSend.cannonRot = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").rotation;
        packageToSend.hp = tankInstances[0].GetComponent<TankControls>().GetHP();
        packageToSend.cannonPos = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").position;
        packageToSend.message = inputMessage;
        Debug.Log("Updating package");

        //Update list of bullets
        
        packageToSend.bulletData = tankInstances[0].GetComponentInChildren<AimControls>().bulletData;
        
        
        if(tankInstances[0].GetComponentInChildren<AimControls>().bulletData.Count>0)
        { Debug.Log("What I send: " + tankInstances[0].GetComponentInChildren<AimControls>().bulletData[0].position); }
        
    }

    void UpdateWorldState()
    {
        packageToReceive = JsonUtility.FromJson<Package>(jsonClient);

        Vector3 newPos = new Vector3(packageToReceive.pos.x, packageToReceive.pos.y, packageToReceive.pos.z);
        tankInstances[1].transform.position = newPos;
        tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").rotation = packageToReceive.cannonRot;
        tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").position = packageToReceive.cannonPos;
        tankInstances[1].GetComponentInChildren<TankControls>().SetHP(packageToReceive.hp);

        //if(printClientInfo)
        //{
        //    message.text += clientInfo;
        //    printClientInfo = false;
        //}

        //Send message to the chat Canvas if it isn't empty
        if (packageToReceive.message != string.Empty && packageToReceive.message.ToString() != helperString)
        {
            message.text += "\n- Client: " + packageToReceive.message.ToString(); // Does this work?
            helperString = packageToReceive.message;
        }

        //Instantiate enemy bullets
        if (packageToReceive.bulletData.Count > 0 && packageToReceive.bulletData[packageToReceive.bulletData.Count - 1] != null)
        {
            if (bulletAmount < packageToReceive.bulletData.Count)
            {
                Instantiate(bulletPrefab, packageToReceive.bulletData[packageToReceive.bulletData.Count - 1].position, packageToReceive.bulletData[packageToReceive.bulletData.Count - 1].rotation);
            }

        }
        bulletAmount = packageToReceive.bulletData.Count;

    }
    public string GetLocalIPv4()
    {
        //Does not parse All possible IPs; There might be IP that are not valid (Virtual Networks)
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.Last(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
    void SerializeJson(Package a)
    {
        mem = new MemoryStream();
        json = JsonUtility.ToJson(a);
        BinaryWriter writer = new BinaryWriter(mem);
        writer.Write(json);
        //Debug.Log("Serialized");
    }

    private void OnApplicationQuit()
    {
        //Socket & Thread CleanUp
        if (recSocket != null)
        {
            recSocket.Close();
            recSocket = null;
        }
        if (sendSocket != null)
        {
            sendSocket.Close();
            sendSocket = null;
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
}