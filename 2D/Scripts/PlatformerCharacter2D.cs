using UnityEngine;
using System.Collections;

public class PlatformerCharacter2D : MonoBehaviour 
{
	// **** general ****
	public bool facingRight = true;								// For determining which way the player is currently facing.
	public bool move_allowed = true;							// allowed to move in the x axis
	[SerializeField] float maxSpeed = 10f;						// The fastest the player can travel in the x axis.
	[Range(0, 1)][SerializeField] float crouchSpeed = .25f;		// Amount of maxSpeed applied to crouching movement. 1 = 100%

	// **** other ****
	[SerializeField] public LayerMask whatIsGround;				// A mask determining what is ground to the character
	Transform groundCheck;										// A position marking where to check if the player is grounded.
	float groundedRadius = .1f;									// Radius of the overlap circle to determine if grounded
	public bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;										// A position marking where to check for ceilings
	float ceilingRadius = .01f;									// Radius of the overlap circle to determine if the player can stand up
	Animator anim;												// Reference to the player's animator component.

	private Character_blink blinker;							// Instance of the character blink class (used to teleport)
	private Character_jump jumper;								// Instance of the character jump class

    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		anim = GetComponent<Animator>();
		blinker = GetComponent<Character_blink>();
		jumper = GetComponent<Character_jump> ();
	}


	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

		// Set the vertical animation
		anim.SetFloat("vSpeed", GetComponent<Rigidbody2D>().velocity.y);
	}


	// Main function that controls the movement, the crounch state, the jump system and the blink system
	public void Move(float move, bool crouch, bool jumpButtonPressed, bool blinkButtonPressed)
	{
		/***** Crounch handling *****/
		if(!crouch && anim.GetBool("Crouch")) // If crouching, check to see if the character can stand up
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if( Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
				crouch = true;
		}
		anim.SetBool("Crouch", crouch);

		/***** x-Deplacement system *****/
		if (move_allowed) {
			//if grounded and crounched, limit speed to crounchSpeed. If in air, limit the speed to airControl
			if (grounded) {
				move = (crouch ? move * crouchSpeed : move);
			} else {
				move = move * jumper.airControl;
			}

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat ("Speed", Mathf.Abs (move));

			if (move != 0){
				// Move the character
				GetComponent<Rigidbody2D> ().velocity = new Vector2 (move * maxSpeed, GetComponent<Rigidbody2D> ().velocity.y);

				//flip the character if needed
				if (move > 0 && !facingRight)
					Flip ();
				else if (move < 0 && facingRight)
					Flip ();
			}
		}

		/***** jump handling *****/
		if (jumpButtonPressed && jumper.canJump()) {
			anim.SetBool("Ground", false);
			jumper.jump();
        }

		/***** blink handling *****/
		if (blinkButtonPressed && blinker.can_blink()) {
			blinker.blink();
		}
	}

	
	public void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;

		//when the player flips, its blink's ghost must flip too
		blinker.flip_ghost (theScale);
	}
		
}
