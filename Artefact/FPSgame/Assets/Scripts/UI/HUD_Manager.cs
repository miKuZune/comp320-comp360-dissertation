using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour {

    public static HUD_Manager instance;                                                     // Creates this class as a singleton.

    public GameObject HUD;                                                                  // Store the prefab of the Heads Up Display.
    Text healthText;                                                                        // Store the text object in the HUD.

    GameObject inGameHUD;                                                                   // Store a reference to the HUD that is in the game scene.

    // Use this for initialization
    void Start()
    {
        if (instance == null) { instance = this; }                                          // Check if there is already an instance of this class. If not make this the instance of the class.
        else { Destroy(this.gameObject); }                                                  // If there is already an instance of this class delete this gameobject.

        inGameHUD = Instantiate(HUD, Vector3.zero, Quaternion.identity);                    // Create the HUD in the game scene.

        healthText = inGameHUD.transform.Find("HealthPanel").GetComponentInChildren<Text>();    // Get and store the text to display the health to.
        
        UpdateHealth();
	}

    public void UpdateHealth()
    {
        Health h = GetComponent<Health>();                                                  // Get the Player's health component.
        healthText.text = "Health : " + h.currHealth + "/" + h.maxHealth;                   // Set the health text to the curr health and max health stored.
    }
}
