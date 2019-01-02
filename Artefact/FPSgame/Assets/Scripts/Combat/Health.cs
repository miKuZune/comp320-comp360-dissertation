using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public int maxHealth;                                       // Stores the maximum health of the Unit.
    public int currHealth { get; set; }                         // Stores the current health of the Unit.

    EnemyAI_Controller AI_Controller;                           // Store a referecne to an AI controller if this instance of Health belongs to an AI.

    I_CharManager charManager;                                  // Store a reference to the character manager.
                                                                // Stores an interface giving acess to certain methods 
                                                                // but different code depending on the specific nature of the manager.
    // Use this for initialization
    void Start ()
    {
        currHealth = maxHealth;                                 // Give the Unit their maximum health at their start.
        charManager = GetComponent<I_CharManager>();            // Get a script which follows the I_CharManager framework from the unit.
        AI_Controller = GetComponent<EnemyAI_Controller>();     // Get the AI controller from the Unit.
    }
	
    // Take health away from the Unit.
    public void DealDmg(int dmg)
    {
        // Take the health away.
        currHealth -= dmg;

        CheckForDeath();                                        // Check if the unit has 0 or less health, each time they take damage.
        if (charManager != null) { charManager.OnDmg(); }       // If there is a script that follows the I_CharManager framework, run the code for the OnDmgs method in that script.

        AI_Controller = GetComponent<EnemyAI_Controller>();     // Get the AI_Controller script incase it was not got at the start of this script.
        if (AI_Controller != null) { AI_Controller.OnDamage(); }   
    }

    // Check if the Unit's health is 0 or less.
    void CheckForDeath()
    {
        if(currHealth <= 0)
        {
            // Handle the Unit's death.
            currHealth = 0;
            if (charManager != null) { charManager.OnDeath(); }
            else { Destroy(this.gameObject); }
            
            if (AI_Controller != null) { AI_Controller.OnDeath(); }
        }
    }
}
