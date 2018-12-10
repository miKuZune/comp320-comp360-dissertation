using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour, I_Gun {

    // Local Variable declartion.
    public int maxAmmo, currentAmmo, damage;
    public float reloadTime, fireRate, accuracy;
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

    // Constructor
    public Shotgun(int maxAmmo, int damage, float reloadTime, float fireRate, float accuracy)
    {
        this.maxAmmo = maxAmmo;
        this.currentAmmo = maxAmmo;
        this.damage = damage;
        this.reloadTime = reloadTime;
        this.fireRate = fireRate;
        this.accuracy = accuracy;
    }

    public void Shoot(Vector3 startPoint, Vector3 direction)
    {
        if (currentAmmo <= 0) { return; }
        RaycastHit hit;
        if (Physics.Raycast(startPoint, direction, out hit, Mathf.Infinity))
        {
            if (hit.transform.tag == "Enemy")
            {
                DealDamage(hit.transform.gameObject);
            }

            currentAmmo--;
            HUD_Manager.instance.UpdateAmmo();
        }
    }

    public void DealDamage(GameObject enemy)
    {
        enemy.GetComponent<Health>().DealDmg(damage);

    }
}
