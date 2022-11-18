using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankControls : MonoBehaviour
{
    private float maxHealth = 100;
    public float currentHealth;

    [SerializeField] private HealthBar healthBar;

    [HideInInspector] public bool isEnabled;
    private void Start()
    {
        isEnabled = true;
        currentHealth = maxHealth;
        healthBar.UpdateHealthBar(maxHealth, currentHealth);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isEnabled)
        {
            float inputX = Input.GetAxis("Horizontal");

            Vector2 movement = new Vector2(1f * inputX, 0);
            movement *= Time.deltaTime;
            transform.Translate(movement);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            currentHealth -= collision.gameObject.GetComponent<BulletLogic>().damage;
            healthBar.UpdateHealthBar(maxHealth, currentHealth);
        }
    }

    public float GetHP()
    {
        return currentHealth;
    }
}
