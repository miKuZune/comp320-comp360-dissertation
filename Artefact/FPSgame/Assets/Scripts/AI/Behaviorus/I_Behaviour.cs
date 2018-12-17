using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Behaviour{

    void Start(EnemyAI_Controller newOwner);
    void Execute();
    void End();
}