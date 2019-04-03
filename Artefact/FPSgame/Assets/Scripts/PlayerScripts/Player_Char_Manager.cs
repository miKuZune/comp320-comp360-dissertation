using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Char_Manager : MonoBehaviour , I_CharManager{
    Player_Movement PM;

    public void OnDeath()
    {
        // Do Game over stuff.
        PM.SetInactive();                           // Stop the player from moving.
        Gun_Manager.instance.SetInactive();         // Stop the player from shooting.
        GameManager.instance.ToggleMouse();         // Show the mouse.

        int currSessionsFinished = PlayerPrefs.GetInt("FinishedRounds") + 1;
        
        PlayerPrefs.SetInt("FinishedRounds", currSessionsFinished);
        Instantiate(Resources.Load("DeathScreen"), Vector3.zero, Quaternion.identity);          // Bring up the death screen.



        if (PlayerPrefs.GetInt("FinishedRounds") >= 2)
        {
            GameObject.Find("NextSession").SetActive(false);
        }
        else { GameObject.Find("ReturnToMainMenu").SetActive(false); }

        DatabaseManager.instance.InsertAllData();
        
    }

    public void OnDmg()
    {
        GetComponent<HUD_Manager>().UpdateHealth();     // Change the HUD to represent the player's health.
    }

    // Use this for initialization
    void Start ()
    {
        PM = GetComponent<Player_Movement>();           // Get a reference to the player's Player Movement script.
	}
}
