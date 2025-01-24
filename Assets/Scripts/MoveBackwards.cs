using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class MoveBackward : MonoBehaviour
{
    public float speed = .5f;
    private Rigidbody rb;

    [SerializeField] public GameObject shopCanvas;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Moves the character forward continuously using physics
        rb.MovePosition(transform.position - transform.forward * speed * Time.fixedDeltaTime);

        // open shop
        shopCanvas.SetActive(true);
    }
}
