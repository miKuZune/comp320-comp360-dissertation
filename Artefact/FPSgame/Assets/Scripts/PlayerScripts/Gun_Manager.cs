using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Manager : MonoBehaviour {

    public static Gun_Manager instance;                                 // Create an easy reference to the script.

    I_Gun[] guns;                                                       // Create an array to store the guns in.
    public I_Gun currentGun;                                            // Store the current gun.

    int currentGunID;
    List<ParticleSystem> gunParticles = new List<ParticleSystem>();

    List<GameObject> gunObjs = new List<GameObject>();
    Camera cam;

    float timeSinceLastShot;

    bool reloading = false;
    float reloadTimer;

    [Header("AssaultRifleStats")]
    public int AR_MaxAmmo;
    public int AR_Damage;
    public float AR_ReloadTimer;
    public float AR_FireRate;
    public float AR_Accuracy;

    [Header("Shotgun stats")]
    public int S_MaxAmmo;
    public int S_Damage;
    public float S_ReloadTimer;
    public float S_FireRate;
    public float S_Accuracy;
    public float damageFallOffRate;

    [Header("Sniper stats")]
    public int SN_MaxAmmo;
    public int SN_Damage;
    public float SN_ReloadTimer;
    public float SN_FireRate;
    public float SN_Accuracy;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

	// Use this for initialization
	void Start ()
    {
        cam = Camera.main;

        guns = new I_Gun[] { new AssaultRifle(AR_MaxAmmo, AR_Damage, AR_ReloadTimer, AR_FireRate, AR_Accuracy) 
                           , new Shotgun(S_MaxAmmo, S_Damage, S_ReloadTimer, S_FireRate, S_Accuracy, damageFallOffRate)
                           , new Sniper(SN_MaxAmmo, SN_Damage, SN_ReloadTimer, SN_FireRate, SN_Accuracy)};

        foreach(Transform child in transform)
        {
            if(child.tag == "Gun")
            {
                gunObjs.Add(child.gameObject);
                gunParticles.Add(child.GetComponentInChildren<ParticleSystem>());
                child.gameObject.SetActive(false);
            }
        }

        LoadGun(0);
	}

    void HideGuns()
    {
        foreach(GameObject obj in gunObjs)
        {
            obj.SetActive(false);
        }
    }

    void LoadGun(int newGunID)
    {
        HideGuns();

        currentGun = guns[newGunID];
        gunObjs[newGunID].SetActive(true);

        gunParticles[newGunID].gameObject.SetActive(false);

        currentGunID = newGunID;

        HUD_Manager.instance.UpdateAmmo(currentGun.CurrentAmmo, currentGun.MaxAmmo);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (reloading)
        {
            reloadTimer += Time.deltaTime;

            HUD_Manager.instance.UpdateReloadTime(currentGun.ReloadTime, reloadTimer);

            if(reloadTimer > currentGun.ReloadTime)
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

        if(Input.GetButton("Fire1") && timeSinceLastShot >= currentGun.FireRate)
        {
            currentGun.Shoot(cam.transform.position, cam.transform.forward);
            
            timeSinceLastShot = 0;
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            currentGunID++;
            if (currentGunID >= guns.Length){ currentGunID = 0; }
            LoadGun(currentGunID);
        }

        if(Input.GetKey(KeyCode.R) && currentGun.CurrentAmmo != currentGun.MaxAmmo)
        {
            reloading = true;
            HUD_Manager.instance.EnableReload();
        }

        timeSinceLastShot += Time.deltaTime;
	}

    public void ActivateVFX()
    {
        gunParticles[currentGunID].gameObject.SetActive(true);
        gunParticles[currentGunID].Play();
    }
}