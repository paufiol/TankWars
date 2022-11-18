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

    public Socket newSocket;
    private int port = 5631;

    [HideInInspector] public string hostIP;

    // Sockets stuff

    [HideInInspector] public string recData;
    [HideInInspector] public bool recTrue = false;
    [HideInInspector] public bool messageSent = false;

    [HideInInspector] public EndPoint ipepClient;

    [HideInInspector] public Thread recthread;

    byte[] data;
    string json;

    class tankUpdater
    {
        public float hp;
        public Vector2 pos;
        public Quaternion rot;
    }
    tankUpdater enemyTank;

    //Tank and spawn

    public GameObject tankPrefab;
    public GameObject spawn;
    public GameObject enemySpawn;

    private List<GameObject> tankInstances = new List<GameObject>();

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void Create()
    {
        data = new byte[256];
        enemyTank = new tankUpdater();

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

        GameObject hostTank = Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);
       
        
        //hostTankControls.isPlayer = false;

        //TankControls isPlayer=hostTank.GetComponent<TankControls>();
        //isPlayer.DisableTank();

        tankInstances.Add(hostTank);

        Debug.Log(tankInstances.Count);

        GameObject clientTank = (GameObject)Instantiate(tankPrefab, enemySpawn.transform.position, transform.rotation);
        SpriteRenderer sprite = clientTank.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;

        tankInstances.Add(clientTank);

    }

    void Update()
    {
        //Disable 2nd tank controls
        if(tankInstances.Count>0)
        {
            tankInstances[1].GetComponent<TankControls>().isEnabled = false;
        }


        if (json != null)
        {
            enemyTank = JsonUtility.FromJson<tankUpdater>(json);
            
            //Debug.Log(enemyTank.pos.ToString());
            //Debug.Log("X:" + clientTank.transform.position.x + "Y:" + clientTank.transform.position.y + "Z:" + clientTank.transform.position.z);
            tankInstances[1].transform.position.Set(enemyTank.pos.x, enemyTank.pos.y, 0);
        }
    }

    void Rec()
    {
        while (true)
        {
            Debug.Log("1");
            newSocket.ReceiveFrom(data, ref ipepClient);
            Debug.Log("2");
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            json = reader.ReadString();

            //Debug.Log(json);

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