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

    public List<BulletInfo> bulletData = new List<BulletInfo>();
    private List<GameObject> bulletList = new List<GameObject>();

    [System.Serializable]
    public class BulletInfo
    {
        public Vector3 position;
        public Quaternion rotation;
    }


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
        ManageBulletList();
    }

    private void ProjectileShoot()
    {
        GameObject projectileGO = (GameObject)Instantiate(bulletPrefab, barrelEnd.transform.position,
            transform.rotation);


        //Add new bullets to list
        BulletInfo bullet = new BulletInfo();
        bullet.position = projectileGO.transform.position;
        bullet.rotation = projectileGO.transform.rotation;

        bulletList.Add(projectileGO);
        bulletData.Add(bullet);

        //Bullet cooldown
        nextShot = Time.time + cooldown;
    }

    private void ManageBulletList()
    {
        //Clean/Manage bullet list
        for (int i = bulletList.Count - 1; i >= 0; i--)
        {
            if (bulletList[i] != null)
            {
                //Do stuff
            }
            else
            {
                bulletList[i] = bulletList[bulletList.Count - 1];
                bulletList.RemoveAt(bulletList.Count - 1);

                bulletData[i] = bulletData[bulletData.Count - 1];
                bulletData.RemoveAt(bulletData.Count - 1); 
            }
        }
    }
}
