using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : MonoBehaviour, I_Gun {
    // Local Variable declartion.
    public int maxAmmo, currentAmmo, damage;
    public float reloadTime, fireRate, accuracy;
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
    public AssaultRifle(int maxAmmo, int damage, float reloadTime, float fireRate, float accuracy, string name)
    {
        this.maxAmmo = maxAmmo;
        this.currentAmmo = maxAmmo;
        this.damage = damage;
        this.reloadTime = reloadTime;
        this.fireRate = fireRate;
        this.accuracy = accuracy;
        this.G_Name = name;
    }

    public void Shoot(Vector3 startPoint, Vector3 direction)
    {
        if (currentAmmo <= 0) { return; }
        RaycastHit hit;
        if(Physics.Raycast(startPoint, direction, out hit, Mathf.Infinity))
        {
            if(hit.transform.tag == "Enemy")
            {
                DealDamage(hit.transform.gameObject);
            }

            currentAmmo--;
            HUD_Manager.instance.UpdateAmmo();
            Gun_Manager.instance.ActivateVFX();
        }
    }

    public void DealDamage(GameObject enemy)
    {
        enemy.GetComponent<Health>().DealDmg(damage);
        
    }
}
