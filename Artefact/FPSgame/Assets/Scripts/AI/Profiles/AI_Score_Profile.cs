using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AI_Profile")]
public class AI_Score_Profile : ScriptableObject
{
    [Header("Move to player")]
    public int RunToPlayerScore;
    public int CantSeePlayerScore;
    [Header("Move to cover")]
    public int InProximityToCoverScore;
    [Header("Fire at player")]
    public float percentageHealthToFireAtPlayer;
    public int p_healthLessThanpercentScore;
    public float maxDistanceToShootAtPlayer;
    public int ShootAtPlayerScore;
    [Header("Flee")]
    public float percentageHealthToFleeAt;
    public int fleeScore;
	
}
