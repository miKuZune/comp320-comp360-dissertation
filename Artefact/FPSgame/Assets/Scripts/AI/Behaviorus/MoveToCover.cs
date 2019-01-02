using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToCover : I_Behaviour
{
    EnemyAI_Controller owner;

    GameObject targetCover;

    NavMeshAgent NMA;


    public void End()
    {
        
    }

    public void Execute()
    {
        if (NMA == null) { NMA = owner.GetComponent<NavMeshAgent>(); }                              // Ensure their is a reference to the NavMeshAgent.
        if (targetCover != null) { NMA.destination = targetCover.transform.position; }              // Set the destination of the AI.
    }

    public void Start(EnemyAI_Controller newOwner)
    {
        // Get Necessary componenets.
        owner = newOwner;
        NMA = owner.GetComponent<NavMeshAgent>();

        targetCover = owner.closestCoverObj;
    }

}
