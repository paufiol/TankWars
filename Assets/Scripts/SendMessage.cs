using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Linq;



public class SendMessage : MonoBehaviour
{
    public JoinServer joinS;
    public Text message;

    private byte[] data;
    public void Send()
    {

            Thread sendthread = new Thread(SendM);
            string messageString = "-" + joinS.username.text + ": " + message.text;
            data = new byte[256];
            data = Encoding.ASCII.GetBytes(messageString);

            joinS.recData = messageString;
            joinS.recTrue = true;
            joinS.messageSent = true;

            sendthread.Start();
        
    }
    void SendM()
    {
        joinS.newSocket.SendTo(data, data.Length, SocketFlags.None, joinS.ipepServer);
        Debug.Log("Message sent");
    }


}
