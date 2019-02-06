using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : MonoBehaviour, I_Gun {
    // Local Variable declartion.
    public int maxAmmo, currentAmmo, damage;
    public float reloadTime, fireRate, accuracy, headShotMultiplier;
    public string G_Name;
    // I_Gun gets and sets. Convert the interface properties to usable variables.
    public int MaxAmmo
    {
        get{ return maxAmmo; }
        set{ maxAmmo = value; }
    }
    public int CurrentAmmo
    {
        get { return currentAmmo; }
        set { currentAmmo = value; }
    }
    public float ReloadTime
    {
        get { return reloadTime; }
        set { reloadTime = value; }
    }
    public float FireRate
    {
        get { return fireRate; }
        set { fireRate = value; }
    }
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }
    public float Accuracy
    {
        get { return accuracy; }
        set { accuracy = value; }
    }
    public string Gun_Name
    {
        get { return G_Name; }
        set { G_Name = value; }
    }


    // Constructor
    public AssaultRifle(int maxAmmo, int damage, float reloadTime, float fireRate, float accuracy, float headShotMultiplier, string name)
    {
        this.maxAmmo = maxAmmo;
        this.currentAmmo = maxAmmo;
        this.damage = damage;
        this.reloadTime = reloadTime;
        this.fireRate = fireRate;
        this.accuracy = accuracy;
        this.G_Name = name;
        this.headShotMultiplier = headShotMultiplier;
    }

    // Handles how the weapon should be shot. 
    public void Shoot(Vector3 startPoint, Vector3 direction)
    {
        if (currentAmmo <= 0) { return; }
        // Handle if the shot has hit the enemies.
        RaycastHit hit;
        if(Physics.Raycast(startPoint, direction, out hit, Mathf.Infinity))
        {
            // Handle if it hit the Enemies.
            if (hit.transform.tag == "Enemy")
            {
                DealDamage(hit.transform.gameObject, 1);
                DatabaseManager.instance.currSessionData.body_shots++;
                DatabaseManager.instance.currSessionData.AR_body_shots++;
            }
            else if (hit.transform.tag == "Head")
            {
                GameObject AI_GO = hit.transform.GetComponent<Head>().AI_main;
                DealDamage(AI_GO, headShotMultiplier);
                DatabaseManager.instance.currSessionData.head_shots++;
                DatabaseManager.instance.currSessionData.AR_head_shots++;
            }
            else {
                DatabaseManager.instance.currSessionData.missed_shots++;
                DatabaseManager.instance.currSessionData.AR_missed_shots++;
            }
        }
        // Update the amount of ammo.
        currentAmmo--;
        HUD_Manager.instance.UpdateAmmo();
        Gun_Manager.instance.ActivateVFX();
    }

    public void DealDamage(GameObject enemy, float multiplier)
    {
        enemy.GetComponent<Health>().DealDmg((int)(damage * multiplier));
        HUD_Manager.instance.EnableHitMarker();
    }
}
