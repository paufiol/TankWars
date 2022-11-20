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
    public Text message;
    public Canvas canvas;
    public Canvas textCanvas;

    [HideInInspector] public Socket newSocket;
    [HideInInspector] public Socket oldSocket;
    private int port = 5631;
    private int port2 = 5655;

    [HideInInspector] public string hostIP;

    // Sockets stuff

    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = false;
    [HideInInspector] public bool messageSent = false;

    [HideInInspector] public EndPoint ipepClient;
    [HideInInspector] public EndPoint ipepClient2;

    [HideInInspector] public Thread recthread;
    [HideInInspector] public Thread sendthread;

    MemoryStream mem = new MemoryStream();
    private byte[] data;
    private string json;
    private string jsonClient;
    private bool isFirstMessage = true;

    public string clientIP;

    // We create the class where we will store all the data of the tank
    class tankClass
    {
        public float hp;
        public Vector3 pos;
        public Quaternion cannonRot;
        public Vector3 cannonPos;
        public List<Transform> bulletInstances;
    }

    private tankClass myTankClass;
    private tankClass enemyTankClass;
    
    //Tank and spawn
    public GameObject tankPrefab;
    public GameObject spawn;
    public GameObject enemySpawn;
    public GameObject bulletPrefab;

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void Create()
    {
        data = new byte[1024];
        enemyTankClass = new tankClass();
        myTankClass = new tankClass();

        // Creating UDP Socket
        newSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);

        oldSocket = new Socket(AddressFamily.InterNetwork,
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


        // Create the tank that will be controled by the host
        GameObject hostTank = Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);

        tankInstances.Add(hostTank);

        // Create the tank that will be controled by the client
        GameObject clientTank = (GameObject)Instantiate(tankPrefab, enemySpawn.transform.position, transform.rotation);
        SpriteRenderer sprite = clientTank.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;

        tankInstances.Add(clientTank);
    }

    void Update()
    {
        //Make sure there is at least one tank
        if (tankInstances.Count>0)
        {
            //Disable 2nd tank controls
            tankInstances[1].GetComponent<TankControls>().isEnabled = false;

            // Each frame we update the content of myTankClass
            myTankClass.pos.x = tankInstances[0].transform.position.x;
            myTankClass.pos.y = tankInstances[0].transform.position.y;
            myTankClass.cannonRot = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").rotation;
            myTankClass.hp = tankInstances[0].GetComponent<TankControls>().GetHP();
            myTankClass.cannonPos = tankInstances[0].GetComponentInChildren<Transform>().Find("Cannon").position;

            //Update list of bullets
            myTankClass.bulletInstances = tankInstances[0].GetComponentInChildren<AimControls>().bulletInstances;
            
            for (int i=0;i < myTankClass.bulletInstances.Count;i++)
            {
                myTankClass.bulletInstances[i] = tankInstances[0].GetComponentInChildren<AimControls>().bulletInstances[i];
            }
        }

        if (jsonClient != null)
        {
            enemyTankClass = JsonUtility.FromJson<tankClass>(jsonClient);



            Debug.Log(enemyTankClass.bulletInstances.Count);
            if (enemyTankClass.bulletInstances.Count != 0)
            {
                Debug.Log(enemyTankClass.bulletInstances[0]);
            }


            //Debug.Log(enemyTank.pos.ToString());
            Vector3 newPos = new Vector3(enemyTankClass.pos.x, enemyTankClass.pos.y, enemyTankClass.pos.z);
            tankInstances[1].transform.position = newPos;
            tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").rotation = enemyTankClass.cannonRot;
            tankInstances[1].GetComponentInChildren<Transform>().Find("Cannon").position = enemyTankClass.cannonPos;
            tankInstances[1].GetComponentInChildren<TankControls>().SetHP(enemyTankClass.hp);
          
           


            //Instantiate enemy bullets
            foreach (Transform projectileGO in enemyTankClass.bulletInstances)
            {
                Debug.Log( enemyTankClass.bulletInstances.Count);
                if (projectileGO != null /*&& bulletAmount>= enemyTankClass.bulletInstances.Count*/)
                {
                    Instantiate(bulletPrefab, projectileGO.transform.position,
            projectileGO.transform.rotation);
                    Debug.Log("It is here");
                }
                
            }

        }
    }
    void Send()
    {
        while (true)
        {
            SerializeJson(myTankClass);
            if (!isFirstMessage) //wait until the message with the client IP is functioning.
            {
                oldSocket.SendTo(mem.GetBuffer(), mem.GetBuffer().Length, SocketFlags.None, ipepClient2);

            }
        }
    }
    void Rec()
    {
        while (true)
        {
            // We recieve data, then deserialize it
            newSocket.ReceiveFrom(data, ref ipepClient);
            
            if (isFirstMessage)
            {
                //First Message is receiving the client IP, and is thus handled differently
                clientIP = Encoding.ASCII.GetString(data);

                Debug.Log("client IP: "+ clientIP);
                clientIP = Regex.Replace(clientIP, "\0", "");

                IPEndPoint ipep2 = new IPEndPoint(IPAddress.Parse(clientIP), port2);
                ipepClient2 = (EndPoint)ipep2;

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

    public string GetLocalIPv4()
    {
        //Does not parse All possible IPs; There might be IP that are not valid (Virtual Networks)
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.Last(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
    void SerializeJson(tankClass a)
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
        if (newSocket != null)
        {
            newSocket.Close();
            newSocket = null;
        }
        if (oldSocket != null)
        {
            oldSocket.Close();
            oldSocket = null;
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