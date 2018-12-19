using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Flee : I_Behaviour
{
    EnemyAI_Controller owner;

    NavMeshAgent NMA;

    Animator anim;

    const float fleeSphereRadius = 20;

    float timer;
    const float timerResetValue = 10;

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
        owner = newOwner;
        NMA = owner.GetComponent<NavMeshAgent>();
        anim = owner.GetComponent<Animator>();
        anim.SetTrigger("flee");
        EvaluateFleePosition();
    }

    void EvaluateFleePosition()
    {
        NMA.destination = GetPointToRunTo();
    }

    Vector3 GetPointToRunTo()
    {
        Vector3 point = Random.insideUnitSphere * fleeSphereRadius;

        point += owner.gameObject.transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(point, out hit, fleeSphereRadius, 1);
        point = hit.position;

        return point;
    }
}
