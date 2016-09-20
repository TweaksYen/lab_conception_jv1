using UnityEngine;

public class PlatformerCharacter2D : MonoBehaviour 
{
	bool facingRight = true;							// For determining which way the player is currently facing.

	[SerializeField] float maxSpeed = 10f;				// The fastest the player can travel in the x axis.
	[SerializeField] public float jumpForce = 400f;			// Amount of force added when the player jumps.	
    [SerializeField] public float wallJumpForce = 10f;
    [SerializeField] public float jumpTime = .0001f;              // Maximum time the player can push the jumpButton to jump higher.	
    bool jumping = false;

    [Range(0, 1)]
	[SerializeField] float crouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%

    [Range(0, 3)]
    [SerializeField] float airControl = 1.0f;			// Whether or not a player can steer while jumping;
	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character
	
	Transform groundCheck;								// A position marking where to check if the player is grounded.
	float groundedRadius = .2f;							// Radius of the overlap circle to determine if grounded
	public bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;								// A position marking where to check for ceilings
	float ceilingRadius = .01f;                         // Radius of the overlap circle to determine if the player can stand up

    Transform rightWallCheck;
    Transform leftWallCheck;
    float wallRadius = .05f;
    public bool rightWalled = false;
    public bool leftWalled = false;

    Animator anim;										// Reference to the player's animator component.

    //Maximum number of jump the player can do before touching the ground again
    [SerializeField] public int maxJumps = 2;



    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
        rightWallCheck = transform.Find("RightWallCheck");
        leftWallCheck = transform.Find("LeftWallCheck");
        anim = GetComponent<Animator>();
	}


	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

        // Determine if the player is hugging a wall, left or right
        rightWalled = Physics2D.OverlapCircle(rightWallCheck.position, wallRadius, whatIsGround);
        leftWalled = Physics2D.OverlapCircle(leftWallCheck.position, wallRadius, whatIsGround);
        // Turn both these variables false if they are both true, in case walls are too close
        if (rightWalled == leftWalled == true)
        {
            leftWalled = false;
            rightWalled = false;
        }

        // Set the vertical animation
        anim.SetFloat("vSpeed", GetComponent<Rigidbody2D>().velocity.y);
	}


	public void Move(float move, bool crouch, bool jump)
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



        // Change the speed if airborn, according to the airControl multiplier
        move = (!grounded ? move * airControl : move);

        // Reduce the speed if crouching by the crouchSpeed multiplier
        move = (crouch&&grounded ? move * crouchSpeed : move);

		// The Speed animator parameter is set to the absolute value of the horizontal input.
		anim.SetFloat("Speed", Mathf.Abs(move));

		// Move the character
		GetComponent<Rigidbody2D>().velocity = new Vector2(move * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
			
		// If the input is moving the player right and the player is facing left...
		if(move > 0 && !facingRight)
			// ... flip the player.
			Flip();
		// Otherwise if the input is moving the player left and the player is facing right...
		else if(move < 0 && facingRight)
			// ... flip the player.
			Flip();

        // If the player should jump...
        if (grounded && jump) {
            // Animate jumping
            anim.SetBool("Ground", false);

            //StartCoroutine(JumpRoutine(jump));
            //old jump control
            //GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));
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


}
