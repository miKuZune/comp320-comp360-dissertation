using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour, I_Gun {

    // Local Variable declartion.
    public int maxAmmo, currentAmmo, damage;
    public float reloadTime, fireRate, accuracy, dmgFallOffRate, headShotMultiplier;
    public string G_Name;
    // Dynamic variables
    float calculatedDmg;

    // I_Gun gets and sets. Convert the interface properties to usable variables.
    public int MaxAmmo
    {
        get { return maxAmmo; }
        set { maxAmmo = value; }
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
    public Shotgun(int maxAmmo, int damage, float reloadTime, float fireRate, float accuracy, float damageFallOffRate, float headShotMultiplier, string name)
    {
        this.maxAmmo = maxAmmo;
        this.currentAmmo = maxAmmo;
        this.damage = damage;
        this.reloadTime = reloadTime;
        this.fireRate = fireRate;
        this.accuracy = accuracy;
        this.dmgFallOffRate = damageFallOffRate;
        this.G_Name = name;
        this.headShotMultiplier = headShotMultiplier;
    }

    // Handle how the weapon shoots.
    public void Shoot(Vector3 startPoint, Vector3 direction)
    {
        if (currentAmmo <= 0) { return; }
        // Handle if the shot has hit an enemy.
        RaycastHit hit;
        if (Physics.Raycast(startPoint, direction, out hit, Mathf.Infinity))
        {
            // Decrease the damage based on the distance between the enemy and the shot.
            float dist = Vector3.Distance(startPoint, hit.point);

            calculatedDmg = CalculateDamage(dist);
            if (hit.transform.tag == "Enemy")
            {
                DealDamage(hit.transform.gameObject, 1);
                
            }else if(hit.transform.tag == "Head")
            {
                GameObject AI_GO = hit.transform.GetComponent<Head>().AI_main;
                DealDamage(AI_GO, headShotMultiplier);
            }
            // Decrease ammo.
            currentAmmo--;
            HUD_Manager.instance.UpdateAmmo();
            Gun_Manager.instance.ActivateVFX();
        }
    }

    public void DealDamage(GameObject enemy, float multiplier)
    {
        enemy.GetComponent<Health>().DealDmg((int)(calculatedDmg * multiplier));
        HUD_Manager.instance.EnableHitMarker();
    }
    float CalculateDamage(float dist)
    {
        float dmgtoDeal = damage - ((dist * dist) * dmgFallOffRate);                        // Calculate the damage using a negative power for fall off damage.
            
        if(dmgtoDeal < 1){ dmgtoDeal = 1; }                                                 // Limit the minimum damage to be 1.

        return dmgtoDeal;
    }
}
