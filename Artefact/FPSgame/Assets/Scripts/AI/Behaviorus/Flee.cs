using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Flee : I_Behaviour
{
    EnemyAI_Controller owner;

    NavMeshAgent NMA;

    Animator anim;

    const float fleeSphereRadius = 20;                      // Used to get a random point around the AI for it to move to.

    float timer;
    const float timerResetValue = 10;                       // Timer used to get a new random location to move to.

    public void End()
    {
        
    }

    public void Execute()
    {

        if(timer < 0)
        {
            EvaluateFleePosition();
            timer = timerResetValue;
        }

        timer -= Time.deltaTime;
    }

    public void Start(EnemyAI_Controller newOwner)
    {
        // Get Neccessary componenets.
        owner = newOwner;
        NMA = owner.GetComponent<NavMeshAgent>();
        anim = owner.GetComponent<Animator>();
        anim.SetTrigger("flee");
        // Get a position to flee to.
        EvaluateFleePosition();
    }

    // Get the AI to move to a new position.
    void EvaluateFleePosition()
    {
        NMA.destination = GetPointToRunTo();
    }
    // Choose the new position for the AI to move to.
    Vector3 GetPointToRunTo()
    {
        Vector3 point = Random.insideUnitSphere * fleeSphereRadius;                     // Gets a random point within a radius.

        point += owner.gameObject.transform.position;                                   // Applies the random point in a radius to the AI's current position.

        // Check if the random position is possible to move to.
        NavMeshHit hit;
        NavMesh.SamplePosition(point, out hit, fleeSphereRadius, 1);                   
        point = hit.position;                            

        return point;
    }
}
