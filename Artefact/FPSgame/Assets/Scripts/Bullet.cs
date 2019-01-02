using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public int damage = 2;                                              // Store the damage each bullet deals to the player.
    public float speed = 3.5f;                                          // Store the speed at which the bullet travels.

    Vector3 target;                                                     // Stores a target location for the bullet to travel to.

    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
    }
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);       // Get a new position for the bullet.
        transform.position = newPos;                                                                    // Move the bullet to the new position.

        float distToTarget = Vector3.Distance(transform.position, target);                              // Find the distance to the bullet's target.
        if (distToTarget < 0.05f) { Destroy(this.gameObject); }                                         // Destroy the bullet if close enough to it's target.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy") { return; }                               // Stop this code from executing if the collided object is an enemy as they are created by enemies.
        if(other.tag == "Player")                                           // Deal damage to the player if it has hit them.
        {
            other.GetComponent<Health>().DealDmg(damage);
        }

        Destroy(this.gameObject);                                           // Destroy the bullet regardless of what object it hits.
    }
}
