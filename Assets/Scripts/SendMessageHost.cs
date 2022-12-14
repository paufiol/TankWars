using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SendMessageHost : MonoBehaviour
{
    public Text message; // input text
    public Text chat; // text that will appear on screen

    public CreateServer host;

    public void Send()
    {
        if (message.text.Length > 0)
        {
            Debug.Log("im sending a message");
            //Write in your own local chat
            string temp = "\n- Host (me): " + message.text;
            chat.text += temp;

            //Send to client
            host.inputMessage = message.text;
        }
    }
}
