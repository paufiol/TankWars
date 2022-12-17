using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SendMessageHost : MonoBehaviour
{
    public Text message;
    public Text chat;


    public CreateServer host;

    // Start is called before the first frame update
    void Start()
    {
        if (message.text.Length > 0)
        {
            Debug.Log("AYUDAME PORFAVOR SACAME DE AQUI");
            //Write in your own local chat
            chat.text += "/n" + message.text;

            //Send to client
            host.myTankClass.message = message.text;
        }
    }

    public void Send()
    {
        if(message.text.Length > 0)
        {
            Debug.Log("AYUDAME PORFAVOR SACAME DE AQUI");
            //Write in your own local chat
            chat.text += "/n" + message.text;

            //Send to client
            host.myTankClass.message = message.text;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
