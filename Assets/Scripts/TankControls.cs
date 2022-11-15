using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankControls : MonoBehaviour
{
    
    //public float minAngle = 90f;
    //public float maxAngle = 220f;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        float inputX = Input.GetAxis("Horizontal");

        Vector2 movement = new Vector2(1f * inputX, 0);
        movement *= Time.deltaTime;
        transform.Translate(movement);

    }
}
