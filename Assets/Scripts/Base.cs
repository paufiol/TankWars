using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Base : MonoBehaviour
{
    public GameObject plane;
    float rotationTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //plane.transform.Rotate(Vector3(0.0,1.0,0.0));
        rotationTime = 180f;
        plane.transform.Rotate(Vector3.down * (rotationTime * Time.deltaTime));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Loader());
        }

    }


    public string url = "https://docs.unity3d.com/uploads/Main/ShadowIntro.png";

    IEnumerator Loader()
    {
        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            GetComponent<Renderer>().material.mainTexture = tex;
        }
    }

}
