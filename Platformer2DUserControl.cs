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

        if(jump && !jumping && (numJump<character.maxJumps || character.walled))
        {
            jumping = true;
            StartCoroutine(JumpRoutine());
        }


        /*
         * if (jump && !jumping && (character.leftWalled || character.rightWalled))
        {
            jumping = true;
            StartCoroutine(WallJumpRoutine());
        }*/

        // Reset the jump input once it has been used.
	    jump = false;
	}

    
    IEnumerator JumpRoutine()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        float timer = 0;

        //initial jump
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, character.initialJumpForce));

        if (character.walled)
        {
            //changer la façon de faire sauter le joueur contre le mur
            character.Flip();
            GetComponent<Rigidbody2D>().velocity = new Vector2((character.facingRight ? character.wallJumpForce : -1 * character.wallJumpForce), 0f);
        }

        while (CrossPlatformInput.GetButton("Jump") && timer < character.jumpTime)
        {
            
            //Calculate how far through the jump we are as a percentage
            //apply the full jump force on the first frame, then apply less force
            //each consecutive frame
            float proportionCompleted = timer / character.jumpTime;
            Vector2 thisFrameJumpVector = Vector2.Lerp(new Vector2(0f, character.jumpForce), Vector2.zero, proportionCompleted);
           

            //Vector2 thisFrameJumpVector = new Vector2(0f, character.jumpForce);
            GetComponent<Rigidbody2D>().AddForce(thisFrameJumpVector);
            timer += Time.deltaTime;
            yield return null;
        }

        jumping = false;
        //Increment number of jumps done for multi-jump, if it's not a wall jump
        numJump = (character.walled ? numJump : numJump + 1);
    }

}
