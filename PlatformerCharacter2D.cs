using UnityEngine;
using System.Collections;

public class PlatformerCharacter2D : MonoBehaviour 
{
	bool facingRight = true;							// For determining which way the player is currently facing.

	[SerializeField] float maxSpeed = 10f;				// The fastest the player can travel in the x axis.

	[SerializeField] public float jumpForce = 500f;			// Amount of force added when the player jumps.	
	[Range(0, 1)]
	[SerializeField] public float jumpTime = .7f;
	[Range(0, 1)]
	[SerializeField] float airControl = .75f;			// Amount of maxSpeed applied to in air movement. 1 = 100%

	//controles du saut multiple
	[SerializeField] int jumps_limit = 3;
	int nbr_sauts_left = 3;
	bool jumping = false;

	[Range(0, 1)]
	[SerializeField] float crouchSpeed = .25f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%

	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character
	
	Transform groundCheck;								// A position marking where to check if the player is grounded.
	float groundedRadius = .2f;							// Radius of the overlap circle to determine if grounded
	public bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;								// A position marking where to check for ceilings
	float ceilingRadius = .01f;							// Radius of the overlap circle to determine if the player can stand up
	Animator anim;										// Reference to the player's animator component.


    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		anim = GetComponent<Animator>();
	}


	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

		// Set the vertical animation
		anim.SetFloat("vSpeed", GetComponent<Rigidbody2D>().velocity.y);
	}


	public void Move(float move, bool crouch, bool jumpButtonPressed)
	{

		// If crouching, check to see if the character can stand up
		if(!crouch && anim.GetBool("Crouch"))
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if( Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
				crouch = true;
		}

		// Set whether or not the character is crouching in the animator
		anim.SetBool("Crouch", crouch);

		//only control the player if grounded or airControl is turned on
		if (grounded || airControl != 0) {
			if (grounded) {
				// Reduce the speed if crouching by the crouchSpeed multiplier
				move = (crouch ? move * crouchSpeed : move);
			} else {
				move = move * airControl;
			}				

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat ("Speed", Mathf.Abs (move));

			// Move the character
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (move * maxSpeed, GetComponent<Rigidbody2D> ().velocity.y);
			
			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !facingRight)
				// ... flip the player.
				Flip ();
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && facingRight)
				// ... flip the player.
				Flip ();
		}

        // If the player should jump...
		if (jumpButtonPressed && nbr_sauts_left > 0) {
            // Add a vertical force to the player.
            anim.SetBool("Ground", false);
			jumping = true;
			nbr_sauts_left--;
			StartCoroutine (JumpRoutine ());
        }

		if (grounded && !jumping) {
			nbr_sauts_left = jumps_limit;
		}
	}

	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	IEnumerator JumpRoutine()
	{
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		float timer = 0;

		while (CrossPlatformInput.GetButton("Jump") && timer < jumpTime)
		{
			//Calculate how far through the jump we are as a percentage
			//apply the full jump force on the first frame, then apply less force
			//each consecutive frame

			float proportionCompleted = timer / jumpTime;
			Vector2 thisFrameJumpVector;

			if (timer == 0)
				thisFrameJumpVector = new Vector2 (0f, jumpForce);
			else
				thisFrameJumpVector = Vector2.Lerp(new Vector2 (0f, 10f), Vector2.zero, proportionCompleted);

			GetComponent<Rigidbody2D> ().AddForce(thisFrameJumpVector);
			timer += Time.deltaTime;
			yield return null;
		}

		jumping = false;
	}

}
