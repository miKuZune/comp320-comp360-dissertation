using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAtPlayer : I_Behaviour
{
    EnemyAI_Controller owner;
    GameObject player;
    GameObject ownerObj;
    Animator anim;

    float minShootCooldown;
    float maxShootCooldown;
    float timeToShoot;

    public void End()
    {
        
    }

    public void Execute()
    {

        if (timeToShoot < 0) { Shoot(); }
        
        // Make the AI look at the player.
        Vector3 lookAtPos = player.transform.position;
        lookAtPos.y = ownerObj.transform.position.y;
        ownerObj.transform.LookAt(lookAtPos);

        // Handle the timer
        timeToShoot -= Time.deltaTime;
    }

    public void Start(EnemyAI_Controller newOwner)
    {
        owner = newOwner;
        player = GameObject.Find("Player");
        ownerObj = owner.gameObject;
        anim = owner.GetComponent<Animator>();

        minShootCooldown = owner.minTimeBetweenShot;
        maxShootCooldown = owner.maxTimeBetweenShot;
        BeginShootTimer();
    }

    void Shoot()
    {
        Debug.Log("Bang");
        anim.SetTrigger("shoot");
        BeginShootTimer();
    }

    void BeginShootTimer()
    {
        // Get a random time between shots.
        System.Random rand = new System.Random();
        int min = (int)(minShootCooldown * 100);
        int max = (int)(maxShootCooldown * 100);
        timeToShoot = (rand.Next(min, max)) / 100;
        

    }
}
