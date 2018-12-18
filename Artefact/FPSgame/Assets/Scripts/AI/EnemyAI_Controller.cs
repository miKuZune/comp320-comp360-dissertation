using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_Controller : MonoBehaviour {

    public float moveSpeed;

    public float minTimeBetweenShot;
    public float maxTimeBetweenShot;

    public float minReEvaluationTime = 1.5f;
    public float maxReEvaluationTimer = 5;
    float reEvaluationTimer;

    public Vector3 gunBloom;

    GameObject player;

    // AI behaviour evaluation variables
    float healthToFleePercent = 20;
    float shootAtPlayerHealthPercent = 20;
    float moveToCoverDistMin = 2.5f;
    float moveToCoverDistMax = 20;
    float minPlayerDistToShoot = 5;
    float maxPlayerDistToShoot = 25;

    // List of all posible behaviours.
    I_Behaviour Move_ToPlayer;
    I_Behaviour ShootAtPlayer;
    I_Behaviour moveToCover;
    I_Behaviour flee;

    I_Behaviour idle;
    
    I_Behaviour currentBehaviour;                                   // Store the current behaviour that should be exhibited.

    float timer;



    void ChangeBehaviour(I_Behaviour newBehaviour)
    {
        if (currentBehaviour != null) { currentBehaviour.End(); }

        currentBehaviour = newBehaviour;
        currentBehaviour.Start(this);
        Debug.Log(currentBehaviour);
    }

	// Use this for initialization
	void Start () {

        // Create behaviour objects.
        Move_ToPlayer = new MoveToPlayer();
        ShootAtPlayer = new ShootAtPlayer();
        moveToCover = new MoveToCover();
        flee = new Flee();

        idle = new Idle();

        GetComponent<NavMeshAgent>().speed = moveSpeed;                                     // Set Unit's per second the AI can travel.
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(currentBehaviour != null) { currentBehaviour.Execute(); }                        // Run the code of the Execute function of the currently loaded behaviour.

        if (reEvaluationTimer < 0)
        {
            ReEvaluateBehaviour();

            System.Random rand = new System.Random();
            int min = (int)minReEvaluationTime;
            int max = (int)maxReEvaluationTimer;
            reEvaluationTimer = rand.Next(min, max);

        }

        reEvaluationTimer -= Time.deltaTime;
	}

    void ReEvaluateBehaviour()
    {

        if (!EvaluateAI_Health()) { return; }
        if (!EvaluatePlayerHealth()) { return; }
        if (!EvaluateDistToCover()) { return; }
        if (!EvaluateDistToPlayer()) { return; }

        Debug.Log("void zone in controller vairables - deafulting to move to player");
        ChangeBehaviour(Move_ToPlayer);


    }

    bool EvaluateAI_Health()
    {
        int h = GetAI_HealthPercent();

        if(h < healthToFleePercent)
        {
            ChangeBehaviour(flee);
            return false;
        }

        return true;
    }

    bool EvaluatePlayerHealth()
    {
        int h = GetPlayer_HealthPercent();

        if(h < shootAtPlayerHealthPercent)
        {
            ChangeBehaviour(ShootAtPlayer);
            return false;
        }

        return true;
    }

    bool EvaluateDistToCover()
    {
        float distance = CalculateDistanceToCover();
        if(distance < moveToCoverDistMax && distance > moveToCoverDistMin)
        {
            ChangeBehaviour(moveToCover);
            return false;
        }
        else if(distance < moveToCoverDistMin)
        {
            ChangeBehaviour(ShootAtPlayer);
            return false;
        }

        return true;
    }

    bool EvaluateDistToPlayer()
    {
        float distance = CalculateDistanceToPlayer();

        if(distance < maxPlayerDistToShoot && distance > minPlayerDistToShoot)
        {
            ChangeBehaviour(ShootAtPlayer);
            return false;
        }else if(distance < minPlayerDistToShoot)
        {
            ChangeBehaviour(moveToCover);
            return false;
        }
        else if(distance > maxPlayerDistToShoot)
        {
            ChangeBehaviour(Move_ToPlayer);
            return false;
        }


        return true;
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

    float CalculateDistanceToPlayer()
    {
        if (player == null) { player = GameObject.FindGameObjectWithTag("Player"); }            // Checks if this script has a reference to the player gameobject. If not finds the reference.

        float distance = Vector3.Distance(transform.position, player.transform.position);       // Use Vector3's distance function to get the distance beetween the two objects.

        return distance;
    }

    float CalculateDistanceToCover()
    {
        float distance = Mathf.Infinity;

        Debug.Log("IMPLEMENT COVER");

        return distance;
    }

    int GetAI_HealthPercent()
    {
        int currhealth = GetComponent<Health>().currHealth;                                     // Access the AI's instance of the Health script and get the current health variable.
        int maxHealth = GetComponent<Health>().maxHealth;

        float healthPercent = ((float)currhealth / (float)maxHealth) * 100 ;

        return (int)healthPercent;
    }

    int GetPlayer_HealthPercent()
    {
        if (player == null) { player = GameObject.FindGameObjectWithTag("Player"); }        // Checks if this script has a reference to the player gameobject. If not finds the reference.
        int CurrHealth = player.GetComponent<Health>().currHealth;                              // Access the player's instance of Health and get the current health variable.
        int maxHealth = player.GetComponent<Health>().maxHealth;

        float healthPercent = ((float)CurrHealth / (float)maxHealth) * 100;

        return (int)healthPercent;
    }
}