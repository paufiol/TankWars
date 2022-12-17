using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SendMessageHost : MonoBehaviour
{
    public Text message; // input text
    public Text chat; // text that will appear on screen


    public CreateServer host;

    // Start is called before the first frame update
    //void Start()
    //{
    //    if (message.text.Length > 0)
    //    {
    //        Debug.Log("AYUDAME PORFAVOR SACAME DE AQUI");
    //        //Write in your own local chat
    //        chat.text += "/n" + message.text;

    //        //Send to client
    //        host.inputMessage = message.text;
    //    }
    //}

    public void Send()
    {
        if(message.text.Length > 0)
        {
            Debug.Log("AYUDAME PORFAVOR SACAME DE AQUI");
            //Write in your own local chat
            string temp = "\n- Host (me): " + message.text;
            chat.text += temp;
            temp = "";

            //Send to client
            host.inputMessage = message.text;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
