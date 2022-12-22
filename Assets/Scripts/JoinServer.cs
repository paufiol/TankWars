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

public class JoinServer : MonoBehaviour
{
    public Text message; // The chat
    [HideInInspector] public string inputMessage = "0"; // Here we store the string that the user enters
    private string helperString; // What this string does is the following: when the host sends a chat message, the client first prints it on screen,
                                 // then equals the message to this string. It will only print it on chat if the received string is different than this one.
    [HideInInspector] public bool sendClientInfo = false;

    int frameCount = 0; // What UDP does to a code

    // UI
    public Text username;
    public Text ip;
    public Canvas canvasJoin;
    public Canvas textCanvas;
    public Text winOrLose;

    // Sockets stuff

    public Socket sendSocket;
    public Socket recSocket;
    private int port = 5631;
    private int port2 = 5655;

    [HideInInspector] public EndPoint ipepServer;
    [HideInInspector] public EndPoint ipepServer2;

    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = true;

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
    public class Package
    {
        public float hp;
        public Vector3 pos;
        public Quaternion cannonRot;
        public Vector3 cannonPos;
        public List<AimControls.BulletInfo> bulletData = new List<AimControls.BulletInfo>();

        public string message; //Here we store messages sent
        public bool isRestarting = false; //Used for game loop communication
    }

    // create the needed classes
    public Package packageReceived;
    public Package packageToSend;

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

    private int bulletAmount = 0;

    private bool restartInitiated;

    void Start()
    {
        restartInitiated = false;
        textCanvas.GetComponent<Canvas>().enabled = false;
        sendClientInfo = true;
    }

    public void Join()
    {
        data = new byte[1024];
        packageReceived = new Package();
        packageToSend = new Package();

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

        message.text += "Server joined";

        canvasJoin.GetComponent<Canvas>().enabled = false;
        textCanvas.GetComponent<Canvas>().enabled = true;

        // Create the tank that will be controled by the client
        GameObject joinTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);
        SpriteRenderer sprite = joinTank.GetComponent<SpriteRenderer>();
        SpriteRenderer sprite2 = joinTank.GetComponentInChildren<SpriteRenderer>();
        sprite.color = Color.red;
        sprite2.color = Color.red;
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
            UpdatePackage();

            if (jsonHost != null)
            {
                UpdateWorldState();
            }

            if (packageReceived.isRestarting)
            {
                tankInstances[0].transform.position = spawn.transform.position;
                tankInstances[0].GetComponent<TankControls>().SetHP(100);
                tankInstances[0].GetComponentInChildren<AimControls>().bulletData.Clear();
                packageToSend.isRestarting = true;
            }
            else
            {
                packageToSend.isRestarting = false;
            }

            //Win/Lose condition
            if (tankInstances[1].GetComponentInChildren<TankControls>().GetHP() <= 0)
            {
                winOrLose.text = "YOU WIN";
                tankInstances[0].GetComponent<TankControls>().isEnabled = false;
            }
            else if (tankInstances[0].GetComponentInChildren<TankControls>().GetHP() <= 0)
            {
                winOrLose.text = "YOU LOSE";
                tankInstances[0].GetComponent<TankControls>().isEnabled = false;
            }
            else
            {
                tankInstances[0].GetComponent<TankControls>().isEnabled = true;
                winOrLose.text = "";
            }

            
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
            SerializeJson(packageToSend);
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

    void UpdatePackage()
    {
        // Each frame we update the content of myTankClass
        packageToSend.pos.x = tankInstances[0].transform.position.x;
        packageToSend.pos.y = tankInstances[0].transform.position.y;
        packageToSend.cannonRot = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").rotation;
        packageToSend.cannonPos = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").position;
        packageToSend.hp = tankInstances[0].GetComponent<TankControls>().GetHP();

        frameCount++;
        if (frameCount < 5) // The first package sent is only for the IP, and sometimes the second does not arrive. UDP kinda bad
        {
            string temp = username.text + " has joined!";
            packageToSend.message = temp;
            //sendClientInfo = false;
            Debug.Log(packageToSend.message);
            Debug.Log(temp);
        }
        else if (frameCount > 5)
        {
            packageToSend.message = inputMessage;

        }

        //Update list of bullets
        if (tankInstances[0].GetComponentInChildren<AimControls>().shotUpdateNeeded)
        {

            packageToSend.bulletData = tankInstances[0].GetComponentInChildren<AimControls>().bulletData;
            tankInstances[0].GetComponentInChildren<AimControls>().shotUpdateNeeded = false;
        }

    }

    void UpdateWorldState()
    {
        packageReceived = JsonUtility.FromJson<Package>(jsonHost);

        //Debug.Log(enemyTank.pos.ToString());
        Vector3 newPos = new Vector3(packageReceived.pos.x, packageReceived.pos.y, packageReceived.pos.z);
        tankInstances[1].transform.position = newPos;
        tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").rotation = packageReceived.cannonRot;
        tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").position = packageReceived.cannonPos;
        tankInstances[1].GetComponentInChildren<TankControls>().SetHP(packageReceived.hp);

        //Send message to the chat Canvas if it isn't empty
        if (packageReceived.message != string.Empty && packageReceived.message.ToString() != helperString)
        {
            message.text += "\n- Host: " + packageReceived.message.ToString();
            helperString = packageReceived.message;

        }

        //Instantiate enemy bullets
        if (packageReceived.bulletData.Count > 0 && packageReceived.bulletData[packageReceived.bulletData.Count - 1] != null)
        {
            if (bulletAmount < packageReceived.bulletData.Count)
            {
                Instantiate(bulletPrefab, packageReceived.bulletData[packageReceived.bulletData.Count - 1].position, packageReceived.bulletData[packageReceived.bulletData.Count - 1].rotation);
            }
        }
        bulletAmount = packageReceived.bulletData.Count;

        if (packageReceived.message.ToString() != "")
        {
            Debug.Log(packageReceived.message.ToString());
        }
    }
    void SerializeJson(Package a)
    {
        mem = new MemoryStream();
        string json = JsonUtility.ToJson(a);
        BinaryWriter writer = new BinaryWriter(mem);
        writer.Write(json);
    }
    private void OnApplicationQuit()
    {
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