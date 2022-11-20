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

    // Create a list where we will store the tank's bullets (EVERYTHING COMMENTED COUSE IT DOESNT WORK)
    //public List<Transform> bulletInstances = new List<Transform>();

    // Update is called once per frame
    void Update()
    {
        if(GetComponentInParent<TankControls>().isEnabled)
        {
            if (Input.GetKey(KeyCode.A))
            {
                transform.RotateAround(tank.transform.position, Vector3.forward, 40f * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
                transform.RotateAround(tank.transform.position, -Vector3.forward, 40f * Time.deltaTime);

            if (Input.GetKey(KeyCode.Space) && Time.time > nextShot)
            {
                ProjectileShoot();
            }
        }
        //ManageBulletList();
    }

    private void ProjectileShoot()
    {
        GameObject projectileGO = (GameObject)Instantiate(bulletPrefab, barrelEnd.transform.position,
            transform.rotation);

        
        //Add new bullets to list
        //bulletInstances.Add(projectileGO.transform);

        nextShot = Time.time + cooldown;
    }

    //private void ManageBulletList()
    //{
    //    //Clean/Manage bullet list
    //    for (int i = bulletInstances.Count - 1; i >= 0; i--)
    //    {
    //        if (bulletInstances[i] != null)
    //        {
    //            //Do stuff
    //        }
    //        else
    //        {
    //            bulletInstances[i] = bulletInstances[bulletInstances.Count - 1];
    //            bulletInstances.RemoveAt(bulletInstances.Count - 1);
    //        }
    //    }
    //}
}
