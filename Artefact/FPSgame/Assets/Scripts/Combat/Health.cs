using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public int maxHealth;
    public int currHealth { get; set; }

    I_CharManager charManager;                                  // Store a reference to the character manager. 
                                                                // Stores an interface giving acess to certain methods 
                                                                // but different code depending on the specific nature of the manager.
    // Use this for initialization
    void Start ()
    {
        currHealth = maxHealth;
        charManager = GetComponent<I_CharManager>();
        Debug.Log(currHealth);
	}
	
    public void DealDmg(int dmg)
    {
        currHealth -= dmg;

        CheckForDeath();
        charManager.OnDmg();
    }

    void CheckForDeath()
    {
        if(currHealth <= 0)
        {
            currHealth = 0;
            charManager.OnDeath();
        }
    }
}
