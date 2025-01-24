using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushBackAndForth : MonoBehaviour
{
    public float pushForce = 5f; // Force to apply for the push
    public float interval = 1f;   // Time interval between pushes

    private Rigidbody rb;
    private bool pushingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(Push());
    }

    IEnumerator Push()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (pushingRight)
            {
                rb.AddForce(Vector3.right * pushForce, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(Vector3.left * pushForce, ForceMode.Impulse);
            }

            pushingRight = !pushingRight;
        }
    }
}