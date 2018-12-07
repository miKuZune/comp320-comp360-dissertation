using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Manager : MonoBehaviour {

    public static Gun_Manager instance;                                 // Create an easy reference to the script.

    I_Gun[] guns;                                                       // Create an array to store the guns in.
    public I_Gun currentGun;                                            // Store the current gun.

    int currentGunID;

    List<GameObject> gunObjs = new List<GameObject>();
    Camera cam;

    float timeSinceLastShot;

    [Header("AssaultRifleStats")]
    public int AR_MaxAmmo;
    public int AR_Damage;
    public float AR_ReloadTimer;
    public float AR_FireRate;
    public float AR_Accuracy;

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
                           , new AssaultRifle(AR_MaxAmmo, AR_Damage, AR_ReloadTimer, AR_FireRate, AR_Accuracy)};

        foreach(Transform child in transform)
        {
            if(child.tag == "Gun")
            {
                gunObjs.Add(child.gameObject);
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
        currentGunID = newGunID;

        HUD_Manager.instance.UpdateAmmo(currentGun.CurrentAmmo, currentGun.MaxAmmo);
    }
	
	// Update is called once per frame
	void Update ()
    {
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

        timeSinceLastShot += Time.deltaTime;
	}
}