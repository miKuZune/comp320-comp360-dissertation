using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Data;
using Mono.Data.Sqlite;

public class MenuManager : MonoBehaviour {

    Image loadingImg;

    public void Start()
    {
        loadingImg = GameObject.Find("LoadingImg").GetComponent<Image>();
        loadingImg.enabled = false;
    }

    // Loads the scene which belongs to the name given.
    public void LoadScene(string sceneName)
    {
        // Randomly choose if the ML should be enabled or not.
        System.Random rand = new System.Random((int)(Time.time * 1000));

        int enableML = rand.Next(0, 2);
        PlayerPrefs.SetInt("enableML", enableML);
        loadingImg.enabled = true;

        // Set the finished rounds - here rounds are the session each individual has played. 
        PlayerPrefs.SetInt("FinishedRounds", 0);
        Debug.Log(PlayerPrefs.GetInt("FinishedRounds"));

        // Get the SessionID for the A/B tests.
        string connString = "URI=file:" + Application.dataPath + "/DB_Official.db";
        IDbConnection dbConn = new SqliteConnection(connString);
        dbConn.Open();

        IDbCommand comm = dbConn.CreateCommand();
        string Query = "SELECT * FROM MLSessionData";
        comm.CommandText = Query;
        IDataReader reader = comm.ExecuteReader();

        int sessionCount = 0;
        while(reader.Read())
        {
            sessionCount++;
        }

        sessionCount += 1;

        Debug.Log(sessionCount);
        PlayerPrefs.SetInt("NewSessionID", sessionCount);

        dbConn.Close();

        SceneManager.LoadScene(sceneName);
    }
    public void NextSession()
    {
        loadingImg.enabled = true;

        int enableML = 0;
        if (PlayerPrefs.GetInt("enableML") == 0) { enableML = 1;}
        
        PlayerPrefs.SetInt("enableML", enableML);

        SceneManager.LoadScene("SampleScene");
    }
    // Closes the game.
    public void QuitGame()
    {
        Application.Quit();
    }
}
