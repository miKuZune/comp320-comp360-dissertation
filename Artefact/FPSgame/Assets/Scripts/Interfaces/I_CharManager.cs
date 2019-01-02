using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_CharManager
{
    void OnDeath();                     // Called when a unit's health value reaches 0 or less.
    void OnDmg();                       // Called anytime a unit's health is lowered.
}
