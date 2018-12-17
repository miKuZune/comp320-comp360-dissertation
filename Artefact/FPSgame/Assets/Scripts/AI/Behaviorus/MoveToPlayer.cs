using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToPlayer : I_Behaviour
{

    EnemyAI_Controller owner;                                           // Store the instance of the controller that this script belongs to.
    NavMeshAgent NMA;                                                   // The NavMeshAgent that is associated with the owner script. Allow the behaviour to control the movement of the AI.
    Animator anim;                                                      // The Animator associated with the owner. Allows access to the animations being played to tie them into the AI's behaviour.
    GameObject player;                                                  // The player's gameobject.
    

    public void End()
    {
        anim.SetFloat("movement", 0);                                   // Stop playing the running animation.
        NMA.isStopped = true;                                           // Stop the AI from moving.
    }

    public void Execute()
    {
        NMA.SetDestination(player.transform.position);                  // Constantly gets the AI to move toward the player's location.
    }

    public void Start(EnemyAI_Controller newOwner)
    {
        // Get references to neccessary Objects.
        owner = newOwner;
        NMA = owner.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        anim = owner.GetComponent<Animator>();

        anim.SetFloat("movement", owner.moveSpeed);                 // Play the running animation.
        NMA.isStopped = false;                                      // Allow the AI to move.
    }

    
}
