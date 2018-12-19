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
        if (NMA == null) { NMA = owner.GetComponent<NavMeshAgent>(); }
        if (targetCover != null) { NMA.destination = targetCover.transform.position; }
        
    }

    public void Start(EnemyAI_Controller newOwner)
    {
        owner = newOwner;
        NMA = owner.GetComponent<NavMeshAgent>();

        targetCover = owner.closestCoverObj;
    }

}
