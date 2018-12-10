using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour {

    public static HUD_Manager instance;                                                     // Creates this class as a singleton.

    public GameObject HUD;                                                                  // Store the prefab of the Heads Up Display.
    Text healthText;                                                                        // Store a reference to the health text component.
    Text ammoText;                                                                          // Store the text component for ammo.

    Scrollbar reloadBar;                                                                    // Store a reference to the scroll bar used to represent the progress of the reload.

    GameObject inGameHUD;                                                                   // Store a reference to the HUD that is in the game scene.

    void Awake()
    {
        if (instance == null) { instance = this; }                                          // Check if there is already an instance of this class. If not make this the instance of the class.
        else { Destroy(this.gameObject); }                                                  // If there is already an instance of this class delete this gameobject.
    }

    // Use this for initialization
    void Start()
    {
        inGameHUD = Instantiate(HUD, Vector3.zero, Quaternion.identity);                    // Create the HUD in the game scene.

        healthText = inGameHUD.transform.Find("HealthPanel").GetComponentInChildren<Text>();    // Get and store the text to display the health to.
        ammoText = inGameHUD.transform.Find("AmmoPanel").GetComponentInChildren<Text>();        // Get and store the text to dispaly the ammo to.

        reloadBar = inGameHUD.transform.Find("ReloadTime").GetComponent<Scrollbar>();

        DisableReload();

        UpdateHealth();
	}

    public void UpdateHealth()
    {
        Health h = GetComponent<Health>();                                                  // Get the Player's health component.
        healthText.text = "Health : " + h.currHealth + "/" + h.maxHealth;                   // Set the health text to the curr health and max health stored.
    }

    public void UpdateAmmo()
    {
        if (ammoText == null) { return; }
        ammoText.text = "Ammo: " + Gun_Manager.instance.currentGun.CurrentAmmo + "/" + Gun_Manager.instance.currentGun.MaxAmmo;
    }

    public void UpdateAmmo(int currAmmo, int maxAmmo)
    {
        if (ammoText == null) { return; }
        ammoText.text = "Ammo: " + currAmmo + "/" + maxAmmo;
    }


    public void EnableReload()
    {
        reloadBar.gameObject.SetActive(true);
    }

    public void DisableReload()
    {
        reloadBar.gameObject.SetActive(false);
    }

    public void UpdateReloadTime(float reloadTime, float currProgress)
    {
        float score = currProgress / reloadTime;
        reloadBar.size = score;
    }
}
