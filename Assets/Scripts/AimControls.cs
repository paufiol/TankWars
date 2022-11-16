using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimControls : MonoBehaviour
{
    public GameObject tank;
    public GameObject barrelEnd;
    public GameObject bulletPrefab;
    private float nextShot = 0.15f;
    private float cooldown=0.5f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(tank.transform.position, Vector3.forward, 40f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
            transform.RotateAround(tank.transform.position, -Vector3.forward, 40f * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space) && Time.time>nextShot)
        {
            ProjectileShoot();
        }
    }

    private void ProjectileShoot()
    {
        GameObject projectileGO = (GameObject)Instantiate(bulletPrefab, barrelEnd.transform.position,
            transform.rotation);
        nextShot = Time.time + cooldown;
    }
}
