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



    //Tank and spawn

    public GameObject tankPrefab;
    public GameObject spawn;

    void Start()
    {
        textCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void Create()
    {
        data = new byte[256];


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
        GameObject hostTank = (GameObject)Instantiate(tankPrefab, spawn.transform.position,
            transform.rotation);
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
            string json = reader.ReadString();
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