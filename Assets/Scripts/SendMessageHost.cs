using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Linq;

public class SendMessageHost : MonoBehaviour
{
    public CreateServer createS;
    public Text message;

    private byte[] data;
    public void Send()
    {

        Thread sendthread = new Thread(SendM);
        string messageString = "-Host: " + message.text;

        data = new byte[256];
        data = Encoding.ASCII.GetBytes(messageString);


        createS.recData = messageString;
        createS.recTrue = true;
        createS.messageSent = true;

        sendthread.Start();
    }

    void SendM()
    {
        createS.recSocket.SendTo(data, data.Length, SocketFlags.None, createS.ipepClient);
    }
}