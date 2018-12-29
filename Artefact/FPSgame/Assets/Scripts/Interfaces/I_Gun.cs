using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Gun
{
    int MaxAmmo { get; set; }
    int CurrentAmmo { get; set; }
    float ReloadTime { get; set; }
    float FireRate { get; set; }
    int Damage { get; set; }
    float Accuracy { get; set; }
    string Gun_Name { get; set; }

    void Shoot(Vector3 shootFromPoint, Vector3 direction);
    void DealDamage(GameObject enemy);
	
}
