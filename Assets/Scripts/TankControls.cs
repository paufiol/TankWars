using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankControls : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");

        Vector2 movement = new Vector2(1f * inputX, 0);
        movement *= Time.deltaTime;
        transform.Translate(movement);
    }
}
