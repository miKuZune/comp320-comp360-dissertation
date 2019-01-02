using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : I_Behaviour
{

    // THIS WAS A FILLER SCRIPT USED IN EARLY TESTING. NO LONGER BEING USED.

    EnemyAI_Controller owner;

    public void End()
    {
        
    }

    public void Execute()
    {
        Debug.Log("idling");
    }

    public void Start(EnemyAI_Controller newOwner)
    {
        owner = newOwner;
    }
}
