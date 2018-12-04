using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Char_Manager : MonoBehaviour , I_CharManager{
    Player_Movement PM;

    public void OnDeath()
    {
        // Do Game over stuff.
        PM.SetInActive();
    }

    public void OnDmg()
    {
        GetComponent<HUD_Manager>().UpdateHealth();
    }

    // Use this for initialization
    void Start ()
    {
        PM = GetComponent<Player_Movement>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.E))
        {
            GetComponent<Health>().DealDmg(3);
        }
	}
}
