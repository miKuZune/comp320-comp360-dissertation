using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public int damage = 2;
    public float speed = 3.5f;

    Vector3 velocity;
    Vector3 target;

    public void SetVelocity(Vector3 newVel)
    {
        velocity = newVel;
        velocity.y = 0;
    }

    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
    }
	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        transform.position = newPos;

        float distToTarget = Vector3.Distance(transform.position, target);
        if (distToTarget < 0.05f) { Destroy(this.gameObject); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy") { return; }                               // Stop this code from executing if the collided object is an enemy as they are created by enemies.
        if(other.tag == "Player")
        {
            other.GetComponent<Health>().DealDmg(damage);
        }

        Destroy(this.gameObject);
    }
}
