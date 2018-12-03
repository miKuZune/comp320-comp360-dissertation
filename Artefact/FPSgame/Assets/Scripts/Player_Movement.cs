using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour {

    [Header("Movement")]
    public float walkSpeed = 3.5f;                      // Stores how fast the player can walk. 
    public float runSpeed = 7.5f;                       // Stores how fast the player can run.
    public float jumpPower = 5f;                        // Stores what the player's y velocity will be when they first click the jump button.

    [Header("Gravity")]
    public float gravityPower = 9.807f;                 // Used to pull the player downwards. Set to the average force of earth's gravity m/s^2(4 S.F).
    public float terminalVelocity = 55.43f;             // Used to limit the speed at which the player can fall. Set by default to the terminal velocity m/s^2 (4 S.F) of the average human being.

    [Header("Input modifers")]
    public float sensitivity = 3f;

    // Private varibles
    CharacterController cc;             // Unity's way of creating characters. Used to move the player.
    float yVel;                         // Store's the player's y velocity. Is global so that it is consitent across frames.
    bool jumpThisFrame = false;         // Checks if the player wants to jump this frame. If they do we use this to ignore gravity so that the char controller can stop reading the player as grounded and hard setting the y velocity.
    bool isActive = true;               // Stores if the player is active. Useful for pause screens and just generally stopping the player moving for any reason.
    bool isRunning = false;             // Stores if the player is running or not.
    float currentMoveSpeed;             // Stores the current movespeed the player is moving at. Either walk speed or run speed.

    public void ToggleActive() { isActive = !isActive; }        // Allows for the player to be enabled and disabled at any time from any script.

    // Use this for initialization
    void Start ()
    {
        cc = GetComponent<CharacterController>();           // Get the character controller attached to the player.
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!isActive) { return; }                          // Check if the player is active. Stop from running if not.

        // Handle the player's interactions with player character.
        Movement();                                         // Activate the player's movement code every frame.
        Aim();                                              // Activate the player's aiming code every frame.

        if(Input.GetButtonDown("Jump") && cc.isGrounded){  Jump(); }        // Check if the player wants to jump and can jump. Activate jump code if both conditions are met.

        if (Input.GetButtonDown("Run")) { isRunning = true; }
        if (Input.GetButtonUp("Run")) { isRunning = false; }
    }

    // Handles the players movemnt.
    void Movement()
    {
        // Handle player input.
        float xDir = Input.GetAxis("Horizontal");               // Get's the player's horizontal input. A & D keys on keyboard. Left and right on analog stick.
        float zDir = Input.GetAxis("Vertical");                 // Get the player's vertical input. W & S keys on keyboard. Up and down on analog stick.

        // Handle gravity.
        if(!jumpThisFrame){Gravity();}                          // Checks if the player has not jumped this frame. If so run gravity code.
        else{jumpThisFrame = false;}                            // If they have reset the bool. This then skips the gravity code for one frame.

        if (!isRunning) { currentMoveSpeed = walkSpeed; }
        else { currentMoveSpeed = runSpeed; }

        // Handle the direction of movement.
        Vector3 movementDir = (transform.forward * zDir) + (transform.right * xDir);        // Handles directional movement. Move forward based on player's vertical input and sideways on their horizontal.
        movementDir *= currentMoveSpeed;                                                    // Apply the current move speed of the player.
        movementDir.y = yVel;                                                               // Gets the player's y velocity and uses it.

        // Move the player.
        cc.Move(movementDir * Time.deltaTime);                                  // Move the player in the chosen direction using move speed and the time difference between this frame and the last frame to keep it consistent.
    }
    // Handles the direction aiming of the player.
    void Aim()
    {
        // Get player's inputs.
        float xDir = Input.GetAxis("Aim_X");                                // Gets the player's X input from the mouse or right analog stick X.
        float yDir = -Input.GetAxis("Aim_Y");                               // Gets the player's Y input from the mouse or the right anlaog stick Y.

        //Rotate the player.
        Vector3 rotation = new Vector3(yDir, xDir, 0);                      // Create a vector 3 to rotate the player by.
        rotation *= sensitivity;                                            // Apply the sensitivity.
        transform.Rotate(rotation);                                         // Rotate the player.

        // Hard lock the player's Z rotation.
        Vector3 rot = transform.eulerAngles;                                // Get the player's current rotation.
        rot.z = 0;                                                          // Set the player's z rotations to 0.
        transform.rotation = Quaternion.Euler(rot);                         // Give the player object the new rotation.
    }

    // Handles jumping
    void Jump()
    {
        yVel = jumpPower;                                                   // Set the y velocity to be jump power so that the player will move up by the specified amount.
        jumpThisFrame = true;                                               // Ignore gravity for a frame to stop the y vel hard lock when the player is grounded from affecting the start of the jump.
    }

    // Handle the player's gravity.
    void Gravity()
    {
        if (!cc.isGrounded)
        {
            yVel -= gravityPower * Time.deltaTime;                               // If the player is in the air decrease their y velocity.
            if (yVel < -terminalVelocity) { yVel = -terminalVelocity; }          // Check if the y vel exceeds a certain amount and limit it if it does. Stops the player from falling at ridiculous numbers if they fall too long.
        }     
        else { yVel = -0.05f; }                                                  // If the player is grounded keep the y vel at a low negative constant.
    }

    
}
