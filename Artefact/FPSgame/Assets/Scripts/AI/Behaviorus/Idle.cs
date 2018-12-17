using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : I_Behaviour
{
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
