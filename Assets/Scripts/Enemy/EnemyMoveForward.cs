using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[RequireComponent(typeof(Rigidbody))]
public class MoveForward : MonoBehaviour
{
    public bool active = false;
    public float speed = 0.5f;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    public int health = 10;
    private HealthManager playerHealth;

    void Start()
    {
        // Attempt to get the MeshRenderer component
        meshRenderer = GetComponent<MeshRenderer>();

        GameObject healthManagerObject = GameObject.Find("healthManager");
        playerHealth = healthManagerObject.GetComponent<HealthManager>();

        meshRenderer.enabled = false; // Optionally disable MeshRenderer on start


        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (active)
        {
            meshRenderer.enabled = true;
            // Moves the character forward continuously using physics
            rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
            checkAlive();
        }
    }

    public void Activate(float speedIn)
    {
        
        speed = speedIn;
        active = true;
        
    }

    public void Deactivate() {
        speed = 0f;
        active = false;
    }

    public void TakeDamage(int x){
        health -= x;
        checkAlive();
    }

    void checkAlive(){
        if (health <= 0){
            MusicManager.AudioManager.goopMusic();
            Destroy(gameObject);
        }
        if (Math.Abs(transform.position.z) <= 5) {
                Destroy(gameObject); // Destroy the GameObject
                //take dmg
                playerHealth.playerTakeDamage(1);
        }
    }
}