using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Unity.Netcode;

public class NetworkMoveEnemy : NetworkBehaviour
{
    public bool active = false;
    public float speed = 0.5f;
    public int health = 10;

     private HealthManager playerHealth;
    // Start is called before the first frame update
    void Start()
    {
        GameObject healthManagerObject = GameObject.Find("healthManager");
        playerHealth = healthManagerObject.GetComponent<HealthManager>();

    }


    // Update is called once per frame
    void Update()
    {
        if(IsServer) {
            // Move the enemy forward
            if (gameObject.layer == LayerMask.NameToLayer("SpawnerHost")) {
                transform.position += transform.forward * -2f * Time.deltaTime;
            } else if (gameObject.layer == LayerMask.NameToLayer("SpawnerGuest")) {
                transform.position += transform.forward * 2f * Time.deltaTime;
            }
        }
    }


     void FixedUpdate()
    {
            checkAlive();
        
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

    private void checkAlive(){
        if (health <= 0){
            MusicManager.AudioManager.goopMusic();
            Destroy(gameObject);
        }
        if (Math.Abs(transform.position.z) <= 5) {
            //Debug.Log("Enemy reached the player" + playerHealth);
            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn(true); 
            Destroy(gameObject); // Destroy the GameObject
            //take dmg
            playerHealth.playerTakeDamage(1);
        }
    }
}
