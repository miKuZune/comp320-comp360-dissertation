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

    [HideInInspector]
    public GameObject closestCoverObj;

    // List of all posible behaviours.
    I_Behaviour Move_ToPlayer;
    I_Behaviour ShootAtPlayer;
    I_Behaviour moveToCover;
    I_Behaviour flee;

    BehaviourScore Move_ToPlayer_BS;
    BehaviourScore ShootAtPlayer_BS;
    BehaviourScore MoveToCover_BS;
    BehaviourScore Flee_BS;

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
        Move_ToPlayer_BS = new BehaviourScore(Move_ToPlayer);

        ShootAtPlayer = new ShootAtPlayer();
        ShootAtPlayer_BS = new BehaviourScore(ShootAtPlayer);

        moveToCover = new MoveToCover();
        MoveToCover_BS = new BehaviourScore(moveToCover);

        flee = new Flee();
        Flee_BS = new BehaviourScore(flee);

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

        CalcMoveToPlayerScore();

        reEvaluationTimer -= Time.deltaTime;
	}

    void ReEvaluateBehaviour()
    {
        Move_ToPlayer_BS.score = CalcMoveToPlayerScore();
        MoveToCover_BS.score = CalcMoveToCoverScore();
        ShootAtPlayer_BS.score = CalcShootScore();
        Flee_BS.score = CalcFleeScore();

        List<BehaviourScore> BScores = new List<BehaviourScore>{Move_ToPlayer_BS, MoveToCover_BS, ShootAtPlayer_BS, Flee_BS};

        BehaviourScore highestScorer = null;

        foreach(BehaviourScore behaviourS in BScores)
        {
            if (highestScorer == null) { highestScorer = behaviourS; }
            else if(behaviourS.score > highestScorer.score){highestScorer = behaviourS;}
        }

        Debug.Log(highestScorer.behaviour);

        if(currentBehaviour != highestScorer.behaviour)
        {
            ChangeBehaviour(highestScorer.behaviour);
        }
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
                score = 125;
            }
            else
            {
                score = (int)hit.distance;
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

        if (nearestDist < 50 && nearestDist > 5) { score = 50; }

        return score;
    }

    int CalcShootScore()
    {
        int score = 0;

        if (player == null) { player = GameObject.FindGameObjectWithTag("Player"); }

        Health playerH = player.GetComponent<Health>();

        float healthPercent = (float)playerH.currHealth / (float)playerH.maxHealth;

        if (healthPercent < 0.2) { score = 110; }
        else
        {
            float distToPlayer = Vector3.Distance(this.transform.position, player.transform.position);
            score = 100 - (int)distToPlayer;
        }


        return score;
    }

    int CalcFleeScore()
    {
        int score = 0;

        Health h = GetComponent<Health>();

        float healthPercent = (float)h.currHealth / (float)h.maxHealth;

        if (healthPercent < 0.2) { score = 150; }


        return score;
    }

    public void OnDeath()
    {
        float AI_PlayerDist = Vector3.Distance(transform.position, player.transform.position);
        string killGunName = player.GetComponent<Gun_Manager>().currentGun.Gun_Name;
        Debug.Log(killGunName);

    }

    public void OnDamage()
    {
        
    }


}

class BehaviourScore
{
    public I_Behaviour behaviour;
    public int score;

    public BehaviourScore(I_Behaviour behaviour)
    {
        this.behaviour = behaviour;
    }
}