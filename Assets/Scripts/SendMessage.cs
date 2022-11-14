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
    public JoinServerTCP joinSTCP;
    public Text message;

    private byte[] data;
    public void Send()
    {
        if (joinS.isUDP)
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
        else
        {
            string messageString = message.text;
            data = new byte[256];
            Debug.Log("You Sent: " + messageString);
            data = Encoding.ASCII.GetBytes(messageString);

            Thread sendthread = new Thread(SendMTCP);
            sendthread.Start();
        }
        
    }
    void SendM()
    {
        joinS.newSocket.SendTo(data, data.Length, SocketFlags.None, joinS.ipepServer);
        Debug.Log("Message sent");
    }

    void SendMTCP()
    {

        joinSTCP.newSocket.Send(data, data.Length, SocketFlags.None);

        Debug.Log("Message sent");
    }

}
