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

    //for the blink
    private bool blink;
    private bool canBlink = true;
    public GameObject blinkerObject;
    private Vector3 blinkPos;
    private float blinkTimer;

	void Awake()
	{
		character = GetComponent<PlatformerCharacter2D>();
        blinkTimer = character.blinkDelay;
	}

    void Update ()
    {
        // Read the jump input in Update so button presses aren't missed.
#if CROSS_PLATFORM_INPUT
        if (CrossPlatformInput.GetButtonDown("Jump")) jump = true;
#else
		if (Input.GetButtonDown("Jump")) jump = true;
#endif
        // Read the jump input in Update so button presses aren't missed.
#if CROSS_PLATFORM_INPUT
        if (CrossPlatformInput.GetButtonDown("Blink")) blink = true;
#else
		if (Input.GetButtonDown("Blink")) blink = true;
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

	    jump = false;


        blinkPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)); //x and y in the screen
        blinkPos = new Vector3(blinkPos.x, blinkPos.y, 0f); // z to be visible

        canBlink =
            ((transform.position - blinkPos).magnitude < character.blinkRange)         // blinker is an active object only if close enough to the player
            && !Physics2D.OverlapCircle(blinkPos - new Vector3(0f, .6f), character.groundedRadius, character.whatIsGround);  //avoid teleporting into the ground  
        // /!\ HARDCODED VALUE OF CHARACTER HEIGHT FROM MID TO FEET -0.6f on y 

        blinkTimer += Time.deltaTime;   //increments timer for the blink
        if(blinkTimer < character.blinkDelay)   //while blink is not ready, the character will flash (appear/disappear)
        {
            if(Time.fixedTime % .5 < .2)
            {
                character.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                character.GetComponent<Renderer>().enabled = true;
            }
        }

        if (blinkTimer > character.blinkDelay) character.GetComponent<Renderer>().enabled = true;   // just to make sure character is fully visible when blink is ready

        if (blink && blinkTimer >= character.blinkDelay)   // player has to wait for the blink to be ready
        {
            StartCoroutine(BlinkRoutine());
        }
        blink = false;
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
            
            GetComponent<Rigidbody2D>().AddForce(thisFrameJumpVector);
            //temporaire : pour tests de calcul de hauteur de saut !
            timer += Time.fixedDeltaTime;
            yield return null;
        }

        jumping = false;
        //Increment number of jumps done for multi-jump, if it's not a wall jump
        numJump = (character.walled ? numJump : numJump + 1);
    }

    IEnumerator BlinkRoutine()
    {

        GameObject blinker = (GameObject)Instantiate(blinkerObject, new Vector3(0, 0, 0), Quaternion.identity);

        while (CrossPlatformInput.GetButton("Blink"))
        {
            
            blinker.transform.position = blinkPos;  // position where the ghost will appear
            blinker.SetActive(canBlink);  
            blinker.transform.localScale = transform.localScale;   //so that the blinker ghost is same dimensions as the player

            yield return null;
        }

        if (canBlink)
        {
            character.transform.position = blinkPos;    //teleports
            if (!character.keepSpeed) character.GetComponent<Rigidbody2D>().velocity = Vector2.zero;    // keeps speed or not when blinking
            blinkTimer = 0f;    // resets the timer so the player has to wait before blinking again
        }

        Destroy(blinker);
    }
}
