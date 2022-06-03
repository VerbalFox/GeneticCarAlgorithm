using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    Rigidbody2D rb;
    float speed = 10f;
    float reverseSpeed = -.6f;
    private float rotationSpeed = .2f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // function to add horizontal force to the car based on the input
    public void Move(float force)
    {
        if (force > 0)
        {
            rb.AddForce(transform.up * speed * force);
        }
        else if (force < 0)
        {
            rb.AddForce(transform.up * reverseSpeed * force);
        }
    }

    // function to add rotation force to the car based on the input
    public void Rotate(float force)
    {
        rb.AddTorque(force  * rotationSpeed);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            SendMessageUpwards("OnCarCollision");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GetComponentInParent<AgentCar>().CheckIfHitNextCheckpoint(collision.gameObject);
    }
}
