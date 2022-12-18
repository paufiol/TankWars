using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class C_SendMessage : MonoBehaviour
{
    public Text message; // input text
    public Text chat; // text that will appear on screen

    public JoinServer client;

    public void C_Send()
    {
        if (message.text.Length > 0)
        {
            //Write in your own local chat
            string temp = "\n- Client (me): " + message.text;
            chat.text += temp;

            //Send to Host
            client.inputMessage = message.text;
        }
    }
}
