using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Credit: https://www.youtube.com/watch?v=1Ip4atvjwps&ab_channel=AM-APPS%26GAMES
public class bouncyBullet : MonoBehaviour
{
    [SerializeField] private Rigidbody bulletRB;
    [SerializeField] private int numOfBounces = 7;
    [SerializeField] private int damage = 5;


    private Vector3 lastVelocity;
    private float curSpeed;
    private Vector3 direction;
    private int curBounces = 0;

    private HealthManager health;

    void Start() 
    {
        GameObject healthManagerObject = GameObject.Find("healthManager");
        health = healthManagerObject.GetComponent<HealthManager>();

    }
    void LateUpdate() {
        lastVelocity = bulletRB.velocity;
    }

    private void OnCollisionEnter(Collision collision) {
        if (curBounces >= numOfBounces) Destroy(gameObject);
        curSpeed = lastVelocity.magnitude;
        direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

        bulletRB.velocity = direction * Mathf.Max(curSpeed, 0);
        curBounces += 1;

    
        // Debug.Log("damage gameobject: " + other.name);

        NetworkMoveEnemy component = collision.gameObject.GetComponent<NetworkMoveEnemy>();
        if (component) 
        {
            component.TakeDamage(damage);
            GameObject.FindWithTag("Score").GetComponent<ScoreTracker>().addScore(1);
        } 
        else if (collision.gameObject.tag == "Player") 
        {
            Debug.Log("PLAYER DAMAGED");
            health.playerTakeDamage(1);
        }
        
    

    }
}
