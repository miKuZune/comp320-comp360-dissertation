using System.Collections;
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
    public double AR_wep_pref = 0;
    [HideInInspector]
    public double Shotgun_wep_pref = 0;
    [HideInInspector]
    public double Sniper_wep_pref = 0;

    
    [HideInInspector]
    public float timeSinceStart = 0;

    [Header("Machine Learning settings")]
    public bool mapWeaponPrefsAsPercentage = true;

    public bool Use_ML_toChangeAI_Profiles = true;

    public StepwiseRegression stepwiseRegression;

    [Header("AI profiles")]
    public AI_Score_Profile AR;
    public AI_Score_Profile Shotgun;
    public AI_Score_Profile Sniper;
    public AI_Score_Profile AR_Shotgun;
    public AI_Score_Profile AR_Sniper;
    public AI_Score_Profile Shotgun_Sniper;
    public AI_Score_Profile Default;

    void Awake()
    {
        // Ensures there is only one instance of this script.
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
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

        // Check if the ML should be enabled or not.
        int enableML = PlayerPrefs.GetInt("enableML");
        if (enableML == 0) { Use_ML_toChangeAI_Profiles = false; }
        else { Use_ML_toChangeAI_Profiles = true; }

        // Starts the first round.
        NextRound();

        timeSinceStart = 57;

        GetModel();

        stepwiseRegression.MapPreferencesAsPercentage(mapWeaponPrefsAsPercentage);
    }

    public void GetModel()
    {
        // Create the stepwise regression object and get the model.
        stepwiseRegression = new StepwiseRegression();
        stepwiseRegression.GetModel();
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
        GameObject newEnemy = Instantiate(EnemyPrefab, ChooseSpawnLocation(), Quaternion.identity);                   // Creates the enemy. Gets the starting location from another method.
        ChooseAIProfile();
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

    public void ChooseAIProfile()
    {
        if (!Use_ML_toChangeAI_Profiles) {return; }

        // Calculate the differences.
        double AR_Shotgun_diff = AR_wep_pref - Shotgun_wep_pref;
        double AR_Sniper_Diff = AR_wep_pref - Sniper_wep_pref;
        double Shotgun_Sniper_Diff = Shotgun_wep_pref - Sniper_wep_pref;
        // Ensure all differences are positive.
        AR_Shotgun_diff = System.Math.Sqrt(AR_Shotgun_diff * AR_Shotgun_diff);
        AR_Sniper_Diff = System.Math.Sqrt(AR_Sniper_Diff * AR_Sniper_Diff);
        Shotgun_Sniper_Diff = System.Math.Sqrt(Shotgun_Sniper_Diff * Shotgun_Sniper_Diff);

        // Look for AR profile
        double meanDelta = (AR_Shotgun_diff + AR_Sniper_Diff + Shotgun_Sniper_Diff) / 3;

        if (meanDelta > 0.35f)               // Check if the difference is signifigant.
        {
            if(Sniper_wep_pref > (AR_wep_pref + Shotgun_wep_pref) / 2)
            {
                SetAIsProfile(Sniper);
            }
            else if(Shotgun_wep_pref > (AR_wep_pref + Sniper_wep_pref))
            {
                SetAIsProfile(Shotgun);
            }
            else if(AR_wep_pref > (Shotgun_wep_pref + Sniper_wep_pref) / 2)
            {
                SetAIsProfile(AR);
            }
        }
        else
        {
            if(Shotgun_Sniper_Diff > (AR_Shotgun_diff + AR_Sniper_Diff) / 2)
            {
                SetAIsProfile(Shotgun_Sniper);
            }
            else if(AR_Sniper_Diff > (AR_Shotgun_diff + Shotgun_Sniper_Diff) / 2)
            {
                SetAIsProfile(AR_Sniper);
            }
            else if(AR_Shotgun_diff > (AR_Sniper_Diff + Shotgun_Sniper_Diff) / 2)
            {
                SetAIsProfile(AR_Shotgun);
            }
        }
    }

    void SetAIsProfile(AI_Score_Profile profile)
    {
        EnemyAI_Controller[] enemies = FindObjectsOfType<EnemyAI_Controller>();

        for(int i = 0; i < enemies.Length; i++)
        {
            enemies[i].currProfile = profile;
        }
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
