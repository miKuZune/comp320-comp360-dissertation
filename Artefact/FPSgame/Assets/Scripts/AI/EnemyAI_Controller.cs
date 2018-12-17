using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_Controller : MonoBehaviour {

    public float moveSpeed;

    public float minTimeBetweenShot;
    public float maxTimeBetweenShot;

    public Vector3 gunBloom;

    // List of all posible behaviours.
    I_Behaviour Move_ToPlayer;
    I_Behaviour ShootAtPlayer;

    I_Behaviour idle;
    
    I_Behaviour currentBehaviour;                                   // Store the current behaviour that should be exhibited.

    float timer;



    void ChangeBehaviour(I_Behaviour newBehaviour)
    {
        if (currentBehaviour != null) { currentBehaviour.End(); }

        currentBehaviour = newBehaviour;
        currentBehaviour.Start(this);
    }

	// Use this for initialization
	void Start () {

        // Create behaviour objects.
        Move_ToPlayer = new MoveToPlayer();
        ShootAtPlayer = new ShootAtPlayer();
        idle = new Idle();

        ChangeBehaviour(ShootAtPlayer);

        GetComponent<NavMeshAgent>().speed = moveSpeed;
	}
	
	// Update is called once per frame
	void Update ()
    {
        

        currentBehaviour.Execute();
	}

    public void Shoot()
    {
        GameObject bullet;            
        
        bullet =  Instantiate((GameObject)Resources.Load("Bullet"), transform);             // Load the bullet prefab from the resources folder and create a new instance of it within the game world.
        bullet.transform.parent = null;

        Bullet bull = bullet.GetComponent<Bullet>();                                        // Store a reference to the new gameobjects bullet script.

        

        Vector3 target = GameObject.FindGameObjectWithTag("Player").transform.position;      // Get the player's position.
        System.Random rand = new System.Random();

        // Add some randomness to the shot.
        target.x += rand.Next((int)-gunBloom.x, (int)gunBloom.x);
        target.y += rand.Next((int)-gunBloom.y, (int)gunBloom.y);
        target.z += rand.Next((int)-gunBloom.z, (int)gunBloom.z);


        bull.SetTarget(target);                                                              // Set the target of the new bullet.

    }
}
