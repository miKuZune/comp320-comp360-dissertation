using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

	// Use this for initialization
	void Start ()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }

        ToggleMouse();
	}

    void ToggleMouse()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.None; }
        else { Cursor.lockState = CursorLockMode.Locked; }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q)) { ToggleMouse(); }
	}
}
