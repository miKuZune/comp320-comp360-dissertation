using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Manager : MonoBehaviour {

    public static Gun_Manager instance;                                 // Create an easy reference to the script.

    I_Gun[] guns;                                                       // Create an array to store the guns in.
    public I_Gun currentGun;                                            // Store the script of the current gun.

    int currentGunID;                                                   // Store the ID of the current gun, used to get from the list of gun Gameobjects and the list of I_Gun scripts.
    List<ParticleSystem> gunParticles = new List<ParticleSystem>();     // Stores the particle scripts that  are used to represent when the gun is fired.

    List<GameObject> gunObjs = new List<GameObject>();                  // Stores a list of the GameObjects in the scene that represent the guns.
    Camera cam;                                                         // Stores a reference to Unity's camera script that is stored on the camera being used for the player's vision.

    float timeSinceLastShot;                                            // Counts the time since the player last executed the code to fire the gun.

    bool reloading = false;                                                                           
    float reloadTimer;                                                  // Counts the time the player has spent reloading their gun.

    // Stores the values used for each of the guns. 
    [Header("AssaultRifleStats")]
    public int AR_MaxAmmo;
    public int AR_Damage;
    public float AR_ReloadTimer;
    public float AR_FireRate;
    public float AR_Accuracy;
    const string AR_name = "Assult Rifle";

    [Header("Shotgun stats")]
    public int S_MaxAmmo;
    public int S_Damage;
    public float S_ReloadTimer;
    public float S_FireRate;
    public float S_Accuracy;
    public float damageFallOffRate;                                   // Stores a value used to calculate the shotguns damage in relation to the distance between the player and the AI that has been shot.
    const string S_name = "Shotgun";

    [Header("Sniper stats")]
    public int SN_MaxAmmo;
    public int SN_Damage;
    public float SN_ReloadTimer;
    public float SN_FireRate;
    public float SN_Accuracy;
    const string SN_name = "Sniper Rifle";

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

	// Use this for initialization
	void Start ()
    {
        cam = Camera.main;                                                                                                          // Get the Camera script on the camera used for the player's vision.

        guns = new I_Gun[] { new AssaultRifle(AR_MaxAmmo, AR_Damage, AR_ReloadTimer, AR_FireRate, AR_Accuracy, AR_name)                      // Create the gun scripts needed for the player to have all avaliable guns.
                           , new Shotgun(S_MaxAmmo, S_Damage, S_ReloadTimer, S_FireRate, S_Accuracy, damageFallOffRate, S_name)
                           , new Sniper(SN_MaxAmmo, SN_Damage, SN_ReloadTimer, SN_FireRate, SN_Accuracy, SN_name)};

        foreach(Transform child in transform)                                                                                       // Gets the gun Gameobjects and the Particle systems that are used to represent the guns and the firing of said guns.
        {
            if(child.tag == "Gun")
            {
                gunObjs.Add(child.gameObject);
                gunParticles.Add(child.GetComponentInChildren<ParticleSystem>());
                child.gameObject.SetActive(false);                                                                                  
            }
        }

        LoadGun(0);                                                                                                                 // Load the gun stored at ID 0 in the guns array by default.
	}

    // Stops showing all the gun Gameobjects in the scene.
    void HideGuns()
    {
        foreach(GameObject obj in gunObjs)
        {
            obj.SetActive(false);
        }
    }
    // Handle changing to a new gun
    void LoadGun(int newGunID)
    {
        HideGuns();                                                                         // Stop showing all the gun gameobjects.

        currentGun = guns[newGunID];                                                        // Store the I_Gun script of the new gun that is being loaded.
        gunObjs[newGunID].SetActive(true);                                                  // Show the gun gameobject that has been loaded.

        gunParticles[newGunID].gameObject.SetActive(false);                                 // De-activate the particle system of the new gun, As Unity plays the effect when the system is enabled.

        currentGunID = newGunID;                                                            // Store the ID of the now current gun.

        HUD_Manager.instance.UpdateAmmo(currentGun.CurrentAmmo, currentGun.MaxAmmo);        // Update the Heads Up Display to show the ammo in the new gun.
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Handle reloading the gun.
        if (reloading)
        {
            reloadTimer += Time.deltaTime;                                                  // Count how long the player has been reloding for.

            HUD_Manager.instance.UpdateReloadTime(currentGun.ReloadTime, reloadTimer);      // Display to the playey the progress of their reload.

            if(reloadTimer > currentGun.ReloadTime)                                         // Check if the player has been reloading for long enough that the reload has finished.
            {
                currentGun.CurrentAmmo = currentGun.MaxAmmo;                                
                reloadTimer = 0;
                reloading = false;
                HUD_Manager.instance.UpdateAmmo();
                HUD_Manager.instance.DisableReload();
                timeSinceLastShot = currentGun.FireRate;
            }
            return;
        }

        // Handle firing the gun.
        if(Input.GetButton("Fire1") && timeSinceLastShot >= currentGun.FireRate)
        {
            currentGun.Shoot(cam.transform.position, cam.transform.forward);                // Runs the code in the current I_Gun Script. Means each gun can deal with shooting differently.
            
            timeSinceLastShot = 0;
        }
        // Handle changing weapon
        if(Input.GetKeyDown(KeyCode.E))
        {
            currentGunID++;
            if (currentGunID >= guns.Length){ currentGunID = 0; }
            LoadGun(currentGunID);
        }
        // Handle starting to reload.
        if(Input.GetKey(KeyCode.R) && currentGun.CurrentAmmo != currentGun.MaxAmmo)
        {
            reloading = true;
            HUD_Manager.instance.EnableReload();
        }

        timeSinceLastShot += Time.deltaTime;                                                // Add to the time since the player last shot.
	}

    // Play the particle effects for the current gun that is being used.
    public void ActivateVFX()
    {
        gunParticles[currentGunID].gameObject.SetActive(true);
        gunParticles[currentGunID].Play();
    }
}