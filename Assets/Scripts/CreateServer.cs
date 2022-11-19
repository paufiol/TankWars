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
    public Text message;
    public Canvas canvas;
    public Canvas textCanvas;

    [HideInInspector] public Socket newSocket;
    private int port = 5631;

    [HideInInspector] public string hostIP;

    // Sockets stuff

    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = false;
    [HideInInspector] public bool messageSent = false;

    [HideInInspector] public EndPoint ipepClient;

    [HideInInspector] public Thread recthread;

    private byte[] data;
    private string json;

    // We create the class where we will store all the data of the tank
    class tankClass
    {
        public float hp;
        public Vector3 pos;
        public Quaternion rot;
    }

    private tankClass enemyTank;

    //Tank and spawn
    public GameObject tankPrefab;
    public GameObject spawn;
    public GameObject enemySpawn;

    // Create a list where we will store the tanks
    private List<GameObject> tankInstances = new List<GameObject>();

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void Create()
    {
        data = new byte[256];
        enemyTank = new tankClass();

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
        }

        if (json != null)
        {
            enemyTank = JsonUtility.FromJson<tankClass>(json);
            
            Debug.Log(enemyTank.pos.ToString());
            Vector3 newPos = new Vector3(enemyTank.pos.x, enemyTank.pos.y, enemyTank.pos.z);
            tankInstances[1].transform.position = newPos;
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
}