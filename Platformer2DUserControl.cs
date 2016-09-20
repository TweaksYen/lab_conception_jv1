using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlatformerCharacter2D))]
public class Platformer2DUserControl : MonoBehaviour 
{
	private PlatformerCharacter2D character;
    private bool jump;
    private bool jumping = false;

    //number of jump done since last grounded
    private int numJump = 0;


	void Awake()
	{
		character = GetComponent<PlatformerCharacter2D>();
	}

    void Update ()
    {
        // Read the jump input in Update so button presses aren't missed.
#if CROSS_PLATFORM_INPUT
        if (CrossPlatformInput.GetButtonDown("Jump")) jump = true;
#else
		if (Input.GetButtonDown("Jump")) jump = true;
#endif

    }

	void FixedUpdate()
	{
		// Read the inputs.
		bool crouch = Input.GetKey(KeyCode.LeftControl);
		#if CROSS_PLATFORM_INPUT
		float h = CrossPlatformInput.GetAxis("Horizontal");
		#else
		float h = Input.GetAxis("Horizontal");
		#endif

		// Pass all parameters to the character control script.
		character.Move( h, crouch , jump );


        //when character touches the ground, his jump counter resets
        if (character.grounded)
        {
            numJump = 0;
        }

        if(jump && !jumping && numJump<character.maxJumps)
        {
            jumping = true;
            StartCoroutine(JumpRoutine());
        }

        if(jump && !jumping && (character.leftWalled || character.rightWalled))
        {
            jumping = true;
            StartCoroutine(WallJumpRoutine());
        }

        // Reset the jump input once it has been used.
	    jump = false;
	}

    /*
    IEnumerator JumpRoutine()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        float timer = 0;

        while (CrossPlatformInput.GetButton("Jump"))
        {
            /*
            //Calculate how far through the jump we are as a percentage
            //apply the full jump force on the first frame, then apply less force
            //each consecutive frame
            float proportionCompleted = timer / character.jumpTime;
            Vector2 thisFrameJumpVector = Vector2.Lerp(new Vector2(0f, character.jumpForce), Vector2.zero, proportionCompleted);
           

            Vector2 thisFrameJumpVector = new Vector2(0f, character.jumpForce);
            GetComponent<Rigidbody2D>().AddForce(thisFrameJumpVector);
            timer += Time.deltaTime;
            yield return null;
        }

        jumping = false;
    }
    */

    IEnumerator JumpRoutine()
    {

        //Set the gravity to zero and apply the force once
        float startGravity = GetComponent<Rigidbody2D>().gravityScale;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0f, character.jumpForce);
        float timer = 0f;

        while (CrossPlatformInput.GetButton("Jump") && timer < character.jumpTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        //Set gravity back to normal at the end of the jump
        GetComponent<Rigidbody2D>().gravityScale = startGravity;
        jumping = false;

        //Increment number of jumps done for multi-jump
        numJump++;
    }

    IEnumerator WallJumpRoutine()
    {

        //Set the gravity to zero and apply the force once
        float startGravity = GetComponent<Rigidbody2D>().gravityScale;
        GetComponent<Rigidbody2D>().gravityScale = 0;

        // Since the character hugs a wall, he will jump in the opposite direction
        if (character.rightWalled)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(+character.wallJumpForce, character.jumpForce);
        }
        else //il y a probablement une erreur de direction de la force, ou de condition, ou de wallCheck, donc à modifier
             //changer le type de saut pourrait aussi avoir un impact important
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(-character.wallJumpForce, character.jumpForce);
        }

        float timer = 0f;

        while (CrossPlatformInput.GetButton("Jump") && timer < character.jumpTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        //Set gravity back to normal at the end of the jump
        GetComponent<Rigidbody2D>().gravityScale = startGravity;
        jumping = false;

    }

}
