using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    // • Our character's walk speed, self-explanatory. 
    public float walkSpeed = 6.0f;  

    // • Our character's run speed, self-explanatory.
    public float runSpeed = 11.0f;  

    // If true, diagonal speed (when strafing + moving forward or back) can't exceed normal move speed; otherwise it's about 1.4 times faster
    // • If true, uses inputModifyFactor to limit character movement if moving diagonally so that it's not faster than normal.
    public bool limitDiagonalSpeed = true;  

    // If checked, the run key toggles between running and walking. Otherwise player runs if the key is held down and walks otherwise
    // There must be a button set up in the Input Manager called "Run"
    // • If true, then the run key will have toggle function instead of hold function.
    public bool toggleRun = false;  

    // • Our character's jump speed, self-explanatory.
    public float jumpSpeed = 8.0f;  
                                   // • The gravity variable for adding negative y-axis movement.
    public float gravity = 20.0f;  

    // Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector
    // • Distance in units that the player can fall before the FallingDamageAlert function is executed.
    public float fallingDamageThreshold = 10.0f;  

    // If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down
    // • If true, then the player will slide down on slopes that have the degree of at least the slope limit set on the character.
    public bool slideWhenOverSlopeLimit = false;  

    // If checked and the player is on an object tagged "Slide", he will slide down it regardless of the slope limit
    // • If true, then the player will slide on any objects that have the tag "Slide", regardless of the slope limit.
    public bool slideOnTaggedObjects = false;  

    // • The speed at which the player will slide down objects.
    public float slideSpeed = 12.0f;  

    // If checked, then the player can change direction while in the air
    // • If true, then the player will be able to alter their movement while not on the ground.
    public bool airControl = false;  

    // • The amount of reduced control the player has in the air.
    public float airControlFactor = 0.5f;  

    // Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast
    // • The amount of vertical movement that happens in reaction to walking down slopes.
    public float antiBumpFactor = .75f;  

    // Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
    // • The amount of frames that the player must be grounded for before they can jump again. Acts to restrict "bunny hopping".
    public int antiBunnyHopFactor = 1;  

    // • If true, cursor is locked to screen, otherwise false.
    public bool locked = true;

    // • The direction of movement determined by input. (Set to zero (0,0,0) )
    private Vector3 moveDirection = Vector3.zero;  

    // • Set to true if player is on the ground, otherwise false.
    private bool grounded = false;  

    // • Defines the player's CharacterController object.
    private CharacterController controller;  

    // • The physical properties (transform includes position, rotation & scale) of the player.
    private Transform player;  

    // • The speed of the player.
    private float speed;  

    // • Gets back information from a a raycast, which creates a ray from the origin,
    // • in a specified direction of length maxDistance, against all colliders in the scene.
    private RaycastHit hit;  

    // • The point at which to start counting downward movement for determining fall distance.
    private float fallStartLevel;  

    // • Set to true if the player is falling, otherwise false.
    private bool falling;  

    // • The degree that a surface must have before the player will start sliding down it.
    private float slideLimit;  

    // • The maximum distance that the raycast hit will travel for determining collision.
    private float rayDistance;  

    // • The point at which the player is colliding with the ground.
    private Vector3 contactPoint;  

    // • Set to true when the player has control of the character, otherwise false.
    private bool playerControl = false;  

    // • The same as antiBunnyHopFactor, but private & assigned in the start function.
    // • Set in the start function & kept private so the player can't modify the value.
    private int jumpTimer;  

    // • The start function, called in the first frame only.
    void Start()
    {  

        lockCursor(true);
        // • Locks the cursor in the screen and makes it invisible.
        //Cursor.lockState = CursorLockMode.Locked;  

        // • Sets the controller to the CharacterController attached to the current gameObject (the player capsule).
        controller = GetComponent<CharacterController>();  

        // • Sets the transform object to the player variable, easier to read.
        player = transform;  

        // • Sets the potential speed of the player to the walk speed.
        speed = walkSpeed;  

        // • Sets the distance of the raycast to half the character's height,
        // • multiplied by the radius of the character.
        rayDistance = controller.height * .5f + controller.radius;  

        // • Sets the slide limit equal to the default slope limit of the character
        slideLimit = controller.slopeLimit - .1f;  

        // • Sets the jumpTimer equal to the antiBunnyHopFactor.
        // •  Makes the jumpTimer private so the player can't edit it.
        jumpTimer = antiBunnyHopFactor;  
    }

    void lockCursor(bool var_locked)
    {
        locked = var_locked;
        Cursor.visible = !var_locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    // • Updates player movement every fixed framerate frame.
    void FixedUpdate()
    {  

        // • Gets and assigns input from player, movement is left or right.
        float inputX = Input.GetAxis("Horizontal");  

        // • Gets and assigns input from player, movement is forward or backward.
        float inputY = Input.GetAxis("Vertical");  

        // If both horizontal and vertical are used simultaneously, limit speed (if allowed), so the total doesn't exceed normal move speed
        // • Calculate inputModifyFactor based on whether we're receiving
        // • 2 inputs from player and limit speed is active.
        // • If active, set it to .7071, if not, then set to 1.0
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed) ? .7071f : 1.0f;  

        // • Checks if the player is on the ground.
        if (grounded)
        {  

            // • Makes sure the player isn't sliding
            // • before we check to see if they should be.
            bool sliding = false;  

            // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
            // because that interferes with step climbing amongst other annoyances
            // • player.position = origin of Raycast,
            // • -Vector3.up = direction of Raycast (negative of up is down)
            // • hit = output of the Raycast,
            // • rayDistance = max distance that the raycast will check.
            // • Checks for colliders under the player and stores info in "hit".
            if (Physics.Raycast(player.position, -Vector3.up, out hit, rayDistance))
            {  

                // • If the angle between the normal of the surface (perpendicular from the surface)
                // • and the world vector up is greater than the character's angle slide limit,
                // • then sliding = true.
                if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)  

                    // • Set sliding to true, making the player slide.
                    sliding = true;  
            }
            // However, just raycasting straight down from the center can fail when on steep slopes
            // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
            // • Since raycasting from the origin of the controller can fail on steep slopes,
            // • send another raycast from 1 unit above the contact point.
            else
            {
                Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out hit);
                if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                    sliding = true;
            }

            // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
            // • If the player is falling
            if (falling)  
            {
                // • Set the falling to false, we'll hit the ground soon enough.
                falling = false;   

                // • If the player's vertical height is less than the difference from the 
                // • falling start position and the damage threshold from falling
                if (player.position.y < fallStartLevel - fallingDamageThreshold)  

                    // • Execute the falling damage alert method, and pass in the total
                    // • vertical distance traveled while falling.
                    FallingDamageAlert(fallStartLevel - player.position.y);  
            }

            // If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down
            // • If the run button isn't toggled
            if (!toggleRun)  

                // • If player is pressing the run button, speed is equal to run speed
                // • otherwise, speed is equal to walk speed.
                speed = Input.GetButton("Run") ? runSpeed : walkSpeed;  

            // If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
            // • If sliding is enabled & the player is sliding,
            // • or if sliding on tagged is enabled, and the player hits a tagged collider.
            if ((sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && hit.collider.tag == "Slide"))  
            {

                // • Assigns the normal (ray perpendicular from the surface) to hitNormal.
                Vector3 hitNormal = hit.normal;  

                // • Set the move vector to normal of the surface, pointing towards it.
                moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);  

                // • Normalize the moveDirection vector, making it 
                // • perpendicular to the hitNormal, & parallel to the sloped surface.
                Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);  

                // • Multiply the move direction vector by the slide speed
                // • to make the player slide down the slope.
                moveDirection *= slideSpeed;  

                // • Disallow the player to move while sliding.
                playerControl = false;  
            }

            // Otherwise recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
            // • If the player is not sliding
            else  
            {

                // • Set the move vector to the direction dictated by the inputs of the player,
                // • multiplied by the input modify factor. 
                // • Set the y direction of the move vector to negative anti bump factor
                // • to make sure that the player doesn't bounce when walking down slopes.
                moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);  

                // • Set the move vector to the transformation of the player in the
                // • direction of the vector, multiplied by the speed,
                // • in order to make the player move in that direction.
                moveDirection = player.TransformDirection(moveDirection) * speed;  

                // • Set the player control to true, since player is in control.
                playerControl = true;  
            }

            // Jump! But only if the jump button has been released and player has been grounded for a given number of frames
            // • If the player is not pressing the jump button
            if (!Input.GetButton("Jump"))  
                // • Increase the jump timer.
                jumpTimer++;  

            // • If player is jumping & jumptimer is greater than the bunny hop limiter
            else if (jumpTimer >= antiBunnyHopFactor)  
            {
                // • Jump
                moveDirection.y = jumpSpeed;  

                // • Reset the jump timer.
                jumpTimer = 0;  
            }
        }

        // • If the player is not on the ground
        else  
        {
            // If we stepped over a cliff or something, set the height at which we started falling
            // • If falling is not true
            if (!falling)  
            {

                // • Set falling to true, because we're in the air.
                falling = true;  

                // • Record the start fall level at the player's position.
                fallStartLevel = player.position.y;  
            }

            // If air control is allowed, check movement but don't touch the y component
            // • If movement in air is enabled, & player has control.
            if (airControl && playerControl)  
            {
                // • 

                moveDirection.x = inputX * speed * inputModifyFactor;
                moveDirection.z = inputY * speed * inputModifyFactor;
                moveDirection = player.TransformDirection(moveDirection);
            }
        }

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller, and set grounded true or false depending on whether we're standing on something
        grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }

    void Update()
    {
        // If the run button is set to toggle, then switch between walk/run speed. (We use Update for this...
        // FixedUpdate is a poor place to use GetButtonDown, since it doesn't necessarily run every frame and can miss the event)
        if (toggleRun && grounded && Input.GetButtonDown("Run"))
            speed = (speed == walkSpeed ? runSpeed : walkSpeed);


        if (Input.GetKeyDown("escape") && locked)
        {
            lockCursor(!locked);
        }

        if (Input.GetButtonDown("Fire1") && !locked)
        {
            lockCursor(!locked);
        }
    }

    // Store point that we're in contact with for use in FixedUpdate if needed
    // • OnControllerColliderHit is called when the CharacterController hits a collider while performing a move.
    // • Pass in parameter hit, so that we can record and store the point of contact.
    void OnControllerColliderHit(ControllerColliderHit hit)  
    { 

        // • Store the contact point found when calling OnControllerColliderHit to contactPoint,
        // • so we can use it in FixedUpdate if needed.
        contactPoint = hit.point;  
    }

    // If falling damage occured, this is the place to do something about it. You can make the player
    // have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
    void FallingDamageAlert(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
    }
}