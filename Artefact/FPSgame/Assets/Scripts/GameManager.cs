using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public GameObject[] CoverObjs { get; set; }

    public GameObject[] SpawnLocations;

    GameObject EnemyPrefab;
    GameObject player;

    [SerializeField]
    float minDistToEnableSpawn = 8.5f;

    // Design stuff for rounds.
    [SerializeField]
    int startEnemyNum = 1;
    [SerializeField]
    int enemyIncreasePerRound = 2;
    [SerializeField]
    int maxActiveEnemies = 12;
    // Round variables.
    [SerializeField]
    int roundNum = 0;
    int enemiesInRound;
    int currActiveEnemies;
    int enemiesKilledInRound;

	// Use this for initialization
	void Start ()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }

        ToggleMouse();

        CoverObjs = GameObject.FindGameObjectsWithTag("Cover");
        SpawnLocations = GameObject.FindGameObjectsWithTag("SpawnLocation");
        EnemyPrefab = (GameObject)Resources.Load("Enemy");
        player = GameObject.FindGameObjectWithTag("Player");

        NextRound();
    }

    void ToggleMouse()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.None; }
        else { Cursor.lockState = CursorLockMode.Locked; }
    }

    void NextRound()
    {
        roundNum++;
        enemiesInRound = startEnemyNum + (enemyIncreasePerRound * roundNum); Debug.Log(enemiesInRound);
        enemiesKilledInRound = 0;
        currActiveEnemies = 0;

        HUD_Manager.instance.UpdateRoundText(roundNum);
    }

    bool NeedToSpawnEnemy()
    {
        if (currActiveEnemies + enemiesKilledInRound < enemiesInRound && currActiveEnemies < maxActiveEnemies) { return true; }

        return false;
    }

    void SpawnEnemy()
    {
        Instantiate(EnemyPrefab, ChooseSpawnLocation(), Quaternion.identity);
        currActiveEnemies++;
    }

    Vector3 ChooseSpawnLocation()
    {
        List<GameObject> potentialSpawns = new List<GameObject>();

        foreach(GameObject location in SpawnLocations)
        {
            float dist = Vector3.Distance(player.transform.position, location.transform.position);
            if(dist > minDistToEnableSpawn)
            {
                potentialSpawns.Add(location);
            }
        }

        System.Random rand = new System.Random();
        int randNum = rand.Next(0, potentialSpawns.Count);
        
        return potentialSpawns[randNum].transform.position;
    }

    public void IncrementEnemiesKilled()
    {
        enemiesKilledInRound++;
        currActiveEnemies--;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q)) { ToggleMouse(); }

        if(NeedToSpawnEnemy()){SpawnEnemy();}
        if (enemiesKilledInRound == enemiesInRound) { NextRound(); }
	}
}
