using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    private float speed = 10f;
    [HideInInspector] public float damage = 25;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.right * Time.deltaTime * speed;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
