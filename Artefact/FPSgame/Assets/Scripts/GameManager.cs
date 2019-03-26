﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;                                                     // Used to make a singleton instance of this object.

    public GameObject[] CoverObjs { get; set; }                                             // Stores a list of all Gameobjects that can be used as cover in the game.
    [HideInInspector]    
    public GameObject[] SpawnLocations;                                                     // Stores a list of all possible spawn locations that the Enemy AI can spawn from.

    GameObject EnemyPrefab;                                                                 // Stores the prefab containing the enemy AI.
    GameObject player;                                                                      // Stores a refernce to the player.

    [Header("Round settings")]
    // Design stuff for rounds.
    [SerializeField]                    // Enables a field in the inspector without making it open to all other scripts.
    int startEnemyNum = 1;                                                                  // Stores the base number of enemies that will spawn in round 0.
    [SerializeField]
    int enemyIncreasePerRound = 2;                                                          // Adds to the amount of enemies the player has to fight each round.
    [SerializeField]
    int maxActiveEnemies = 12;                                                              // Stores the maximum number of enemies that can be active at any one time.
    // Round variables.
    [SerializeField]
    int roundNum = 0;                                                                       // Stores the current round number.
    int enemiesInRound;                                                                     // Stores the total number of enemies the player needs to kill to get to the next round.
    int currActiveEnemies;                                                                  // Stores the current number of enemies in the scene.
    int enemiesKilledInRound;                                                               // Stores the enemies that have been killed so far in the round.


    [Header("Spawner settings")]
    [SerializeField]            
    float minDistToEnableSpawn = 8.5f;                                                      // Stores the minimum distance between a spawn point and the player to allow the spawn point to spawn enemies.
    [SerializeField]
    int randSpawnOffsetMin;
    [SerializeField]
    int randSpawnOffsetMax;

    [HideInInspector]
    public float timeSinceStart = 0;

    void Awake()
    {
        // Ensures there is only one instance of this script.
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

    public void GetModel()
    {
        StepwiseRegression SR = new StepwiseRegression();
        SR.GetModel();
    }

    // Use this for initialization
    void Start ()
    {
        ToggleMouse();
        // Gets references to the necessary objects in the scene.
        CoverObjs = GameObject.FindGameObjectsWithTag("Cover");
        SpawnLocations = GameObject.FindGameObjectsWithTag("SpawnLocation");
        EnemyPrefab = (GameObject)Resources.Load("Enemy");
        player = GameObject.FindGameObjectWithTag("Player");
        // Starts the first round.
        NextRound();

        timeSinceStart = 57;
        
    }

    public void ToggleMouse()
    {
        Cursor.visible = !Cursor.visible;                                                               // Makes the cursor visible/invisble to the player.

        if (Cursor.lockState == CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.None; }      // Stops the cursor from moving/ enables the cursor to move again.
        else { Cursor.lockState = CursorLockMode.Locked; }
    }

    // Start the next round of the
    void NextRound()
    {
        roundNum++;                                                                                     // Increase the round number.
        enemiesInRound = startEnemyNum + (enemyIncreasePerRound * roundNum);                            // Calculate the number of enemies in the new round.
        // Reset values that need to be reset.
        enemiesKilledInRound = 0;
        currActiveEnemies = 0;
        // Tell the player they are now in a new round.
        HUD_Manager.instance.UpdateRoundText(roundNum);
        DatabaseManager.instance.currSessionData.endRound++;
    }
    // Check if a new enemy needs to be created in the scene.
    bool NeedToSpawnEnemy()
    {
        if (currActiveEnemies + enemiesKilledInRound < enemiesInRound && currActiveEnemies < maxActiveEnemies) { return true; }

        return false;
    }
    // Spawn a new enemy.
    void SpawnEnemy()
    {
        Instantiate(EnemyPrefab, ChooseSpawnLocation(), Quaternion.identity);                   // Creates the enemy. Gets the starting location from another method.
        currActiveEnemies++;
    }
    // Gets the starting location of an enemy.
    Vector3 ChooseSpawnLocation()
    {
        List<GameObject> potentialSpawns = new List<GameObject>();                              // Used to store the list of potential spots to spawn the enemy.

        foreach(GameObject location in SpawnLocations)                                          // Goes through all spawn points in the scene.
        {
            float dist = Vector3.Distance(player.transform.position, location.transform.position);      // Gets distance between spawn point and player.
            if(dist > minDistToEnableSpawn)                                                             // Checks if the distance is high enough that the player will not likely see the spawn.
            {
                potentialSpawns.Add(location);                                                          // Adds it to the list of potential spawn points.
            }
        }

        // Pick a random number between 0 and the length of the list of potential spawn points.
        System.Random rand = new System.Random((int)(Time.time * 1000));
        int randNum = rand.Next(0, potentialSpawns.Count);

        // Add extra randomisation to the spawn points so enemies do not spawn ontop of each other.
        Vector3 extraRandomisation = potentialSpawns[randNum].transform.position;
        extraRandomisation.x += rand.Next(randSpawnOffsetMin, randSpawnOffsetMax);
        extraRandomisation.z += rand.Next(randSpawnOffsetMin, randSpawnOffsetMax);

        // Return the vec3 position of the spawn point randomly chosen, with some extra randomisation added.
        return extraRandomisation;
    }
    // Handle the changing of numbers when an enemy is killed.
    public void IncrementEnemiesKilled()
    {
        enemiesKilledInRound++;                                 // Ensures that the right number of enemies will be in each round.
        currActiveEnemies--;                                    // Decreased so that a new enemy can be spawned if necessary.
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Check if more enemies need to be created.
        if(NeedToSpawnEnemy()){SpawnEnemy();}
        // Check if all enemies in the round have been killed.
        if (enemiesKilledInRound == enemiesInRound) { NextRound(); }

        // Count how long the player has been in game.
        timeSinceStart += Time.deltaTime;
	}

    

    
}
