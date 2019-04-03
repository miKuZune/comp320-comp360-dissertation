using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_Controller : MonoBehaviour {

    public float moveSpeed;                    // Stores the distance per second the AI can travel.

    // Values for the timer to dictate how long the AI takes between shots when in the shooting behaviour.
    public float minTimeBetweenShot;
    public float maxTimeBetweenShot;

    // Values for the timer which allows the AI to re-evaluate and choose a new behaviour.
    [SerializeField]
    float minReEvaluationTime = 1.5f;
    [SerializeField]
    float maxReEvaluationTimer = 5;
    float reEvaluationTimer;

    [SerializeField]
    Vector3 gunBloom;           // Chooses the amount of variation in the direction of the AI's shots.

    GameObject player;          // Stores a reference to the player.

    [HideInInspector]
    public GameObject closestCoverObj;      // Stores the closest gameobject that is tagged as being cover.

    // List of all posible behaviours.
    I_Behaviour Move_ToPlayer;
    I_Behaviour ShootAtPlayer;
    I_Behaviour moveToCover;
    I_Behaviour flee;

    BehaviourScore Move_ToPlayer_BS;
    BehaviourScore ShootAtPlayer_BS;
    BehaviourScore MoveToCover_BS;
    BehaviourScore Flee_BS;

    GameObject bulletOriginPoint;
    
    I_Behaviour currentBehaviour;                                   // Store the current behaviour that should be exhibited.

    float timer;

    public AI_Score_Profile currProfile;

    // Data collection variables.
    bool hasBeenShot = false;
    float timeSinceFirstShot;

    // Changes the current behaviour to a new behaviour.
    void ChangeBehaviour(I_Behaviour newBehaviour)
    {
        if (currentBehaviour != null) { currentBehaviour.End(); }               // Run the Ending code of the behaviour.

        currentBehaviour = newBehaviour;                                        // Store the new behaviour to exhibit.
        currentBehaviour.Start(this);                                           // Run the start code for the new behaviour.
    }

	// Use this for initialization
	void Start () {

        // Create behaviour objects and Behaviour score objects.
        Move_ToPlayer_BS = new BehaviourScore(new MoveToPlayer());

        ShootAtPlayer_BS = new BehaviourScore(new ShootAtPlayer());

        MoveToCover_BS = new BehaviourScore(new MoveToCover());

        Flee_BS = new BehaviourScore(new Flee());

        GetComponent<NavMeshAgent>().speed = moveSpeed;                                     // Set Unit's per second the AI can travel.

        bulletOriginPoint = transform.Find("BulletSpawn").gameObject;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(currentBehaviour != null) { currentBehaviour.Execute(); }                        // Run the code of the Execute function of the currently loaded behaviour.

        if (hasBeenShot) { timeSinceFirstShot += Time.deltaTime; }                          // Counts the time between first being shot and being killed. Used in the events database.

        // See if the AI should re-evaluate their behaviour.
        if (reEvaluationTimer < 0)
        {
            ReEvaluateBehaviour();

            // Get a new random time after which the AI will re-evaluate again.
            System.Random rand = new System.Random();
            int min = (int)minReEvaluationTime;
            int max = (int)maxReEvaluationTimer;
            reEvaluationTimer = rand.Next(min, max);
        }

        reEvaluationTimer -= Time.deltaTime;
	}

    void ReEvaluateBehaviour()
    {
        // Get a score for each behaviour that the AI could be exhibiting.
        Move_ToPlayer_BS.score = CalcMoveToPlayerScore();
        MoveToCover_BS.score = CalcMoveToCoverScore();
        ShootAtPlayer_BS.score = CalcShootScore();
        Flee_BS.score = CalcFleeScore();

        // Store the list of Behaviours and scores.
        List<BehaviourScore> BScores = new List<BehaviourScore>{Move_ToPlayer_BS, MoveToCover_BS, ShootAtPlayer_BS, Flee_BS};

        // Get the highest scoring behaviour.
        BehaviourScore highestScorer = null;

        foreach(BehaviourScore behaviourS in BScores)
        {
            if (highestScorer == null) { highestScorer = behaviourS; }          // Checks if there is not already a set behaviour and sets one.
            else if(behaviourS.score > highestScorer.score){highestScorer = behaviourS;}        // Checks if the score of the current behaviour is higher than the current highest.
        }

        if(currentBehaviour != highestScorer.behaviour){ChangeBehaviour(highestScorer.behaviour);}      // Only change behaviour if the highest scorer is different to the current behaviour.
    }

    public void Shoot()
    {
        GameObject bullet;            
        
        bullet =  Instantiate((GameObject)Resources.Load("Bullet"), bulletOriginPoint.transform.position, Quaternion.identity);             // Load the bullet prefab from the resources folder and create a new instance of it within the game world.
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

    int CalcMoveToPlayerScore()
    {
        int score = 0;

        // Check if the AI can "see" the player.
        if (player == null) { player = GameObject.FindGameObjectWithTag("Player"); }
        Vector3 dir = player.transform.position - transform.position;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, dir, out hit))
        {
            if(hit.transform.tag != "Player")
            {
                score = currProfile.CantSeePlayerScore;
            }
            else
            {
                if (hit.distance > currProfile.maxDistanceToShootAtPlayer) { score = currProfile.RunToPlayerScore; }
            }
        }

        return score;
    }

    int CalcMoveToCoverScore()
    {
        int score = 0;

        // Find the distance to the nearest cover gameobject in the scene.
        float nearestDist = Mathf.Infinity;
        foreach(GameObject cover in GameManager.instance.CoverObjs)                                     // A list of all cover objects is stored in the gamemanager script. 
        {
            float dist = Vector3.Distance(transform.position, cover.transform.position);
            if(dist < nearestDist)
            {
                nearestDist = dist;
                closestCoverObj = cover;
            }
        }

        if (nearestDist < 50 && nearestDist > 5) { score = currProfile.InProximityToCoverScore; }

        return score;
    }

    int CalcShootScore()
    {
        int score = 0;

        // Check the player's health.
        if (player == null) { player = GameObject.FindGameObjectWithTag("Player"); }

        Health playerH = player.GetComponent<Health>();

        float healthPercent = (float)playerH.currHealth / (float)playerH.maxHealth;

        if (healthPercent < currProfile.percentageHealthToFireAtPlayer) { score = currProfile.p_healthLessThanpercentScore; }               // Set score when the player's health is below a certain point.
        else
        {
            float distToPlayer = Vector3.Distance(this.transform.position, player.transform.position);      // Else set the score to a value - the distance between them.
            if (distToPlayer < currProfile.maxDistanceToShootAtPlayer) { score = currProfile.ShootAtPlayerScore; }
        }
        return score;
    }

    int CalcFleeScore()
    {
        int score = 0;

        Health h = GetComponent<Health>();

        float healthPercent = (float)h.currHealth / (float)h.maxHealth;             // Get AI health as a percentage.

        if (healthPercent < currProfile.percentageHealthToFleeAt) { score = currProfile.fleeScore; }// Set score if it is below a threshold.

        return score;
    }

    // Handle the death of an AI.
    public void OnDeath()
    {
        // Get variables to be stored in the database.
        float AI_PlayerDist = Vector3.Distance(transform.position, player.transform.position);
        string killGunName = player.GetComponent<Gun_Manager>().currentGun.Gun_Name;
        // Store the data in the database.
        DatabaseManager.instance.StoreNewEventData(killGunName, AI_PlayerDist, timeSinceFirstShot);
        DatabaseManager.instance.currSessionData.enemiesKilled++;
        GameManager.instance.ChooseAIProfile();
        // Tell the gamemanager that an enemy has been killed so it can handle the rounds.
        GameManager.instance.IncrementEnemiesKilled();
    }

    // Handle taking damage.
    public void OnDamage()
    {
        hasBeenShot = true;
    }
}

class BehaviourScore
{
    public I_Behaviour behaviour;       // Store the behaviour.
    public int score;                   // Store the score when it is re-evaluated.

    // Constructor.
    public BehaviourScore(I_Behaviour behaviour)
    {
        this.behaviour = behaviour;
    }
}