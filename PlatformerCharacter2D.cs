using UnityEngine;

public class PlatformerCharacter2D : MonoBehaviour 
{
	public bool facingRight = true;							// For determining which way the player is currently facing.

	[SerializeField] float maxSpeed = 10f;              // The fastest the player can travel in the x axis.

    [Range(400, 600)]
    [SerializeField] public float initialJumpForce = 500f;          // Amount of force added when the player jumps.	
    [Range(10, 25)]
    [SerializeField] public float jumpForce = 15f;			// Amount of force added when the player jumps.	
    [Range(50, 150)]
    [SerializeField] public float wallJumpForce = 80f;          // Amount of force in the opposite direction added when the player jumps against a wall
    [Range(0, 3)]
    [SerializeField] public float jumpTime = .05f;              // Maximum time the player can push the jumpButton to jump higher.	

    [Range(0, 1)]
	[SerializeField] float crouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%

    [Range(0, 3)]
    [SerializeField] float airControl = 0.8f;			// Whether or not a player can steer while jumping;
	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character
	
	Transform groundCheck;								// A position marking where to check if the player is grounded.
	float groundedRadius = .2f;							// Radius of the overlap circle to determine if grounded
	public bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;								// A position marking where to check for ceilings
	float ceilingRadius = .01f;                         // Radius of the overlap circle to determine if the player can stand up

    Transform wallCheck;                                //A position marking where to check if the player is hugging a wall
    float wallRadius = .05f;
    public bool walled = false;

    Transform jumpHeight;                               //A position showing player's full jumping height
    [SerializeField] bool seeJumpHeight = true;

    Animator anim;										// Reference to the player's animator component.

    //Maximum number of jump the player can do before touching the ground again
    [SerializeField] public int maxJumps = 2;



    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
        wallCheck = transform.Find("WallCheck");
        jumpHeight = transform.Find("JumpHeight");
        anim = GetComponent<Animator>();
	}


	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

        // Determine if the player is hugging a wall
        walled = Physics2D.OverlapCircle(wallCheck.position, wallRadius, whatIsGround);

        // Set the vertical animation
        anim.SetFloat("vSpeed", GetComponent<Rigidbody2D>().velocity.y);

        // Set position of the jump height marker, and if it's visible or not
        jumpHeight.GetComponent<SpriteRenderer>().enabled = seeJumpHeight;
        jumpHeight.position = transform.position + new Vector3(0f, 1f + 5f);   //1f is roughly the size of the character
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

   

	
	public void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}


}
