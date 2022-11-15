using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimControls : MonoBehaviour
{
    public GameObject tank;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(tank.transform.position, Vector3.forward, 40f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
            transform.RotateAround(tank.transform.position, -Vector3.forward, 40f * Time.deltaTime);
    }
}
