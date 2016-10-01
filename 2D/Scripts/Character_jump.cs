using UnityEngine;
using System.Collections;

public class Character_jump : MonoBehaviour {
	// **** jump characteristics ****
	[SerializeField] public float jumpForceX = 400f;				// Amount of force added in the x-axis when the player jumps 
	[SerializeField] public float jumpForceY = 500f;				// Amount of force added in the y-axis when the player jumps (initial force)	
	[SerializeField] public float jumpForceAddedY = 20;				// Amount of force added in the y-axis when the player jumps (further forces)
	[SerializeField] public float jumpTime = .7f;					// Duration of the normal jump
	[SerializeField] public float wallJumpTime = .6f;				// Duration of the walljump
	[Range(0, 1)][SerializeField] public float airControl = .75f;	// Amount of maxSpeed applied to in air movement. 1 = 100%
	bool jumping = false;											// is the player currently applied forces to lift him up

	// **** multiple jumps ****
	[SerializeField] int jumps_limit = 2;							// Maximum consecutive jumps allowed
	int jumps_left = 2;												// Current number of jumps left

	// **** walljumps ****
	Transform wallCheck;											// A position marking where to check if the player is against a wall.
	float walledRadius = .05f;										// Radius of the overlap circle to determine if touching a wall
	bool walled;													// is the player touching a wall

	public GameObject jumpHeightIndicator;							// The object that indicates the maximum height of a jump
	float indicatorHeight;											// The actual height of the highest jump (calculated only once at the begining)
	[SerializeField] bool seeJumpHeight = true;						// Must the indicator be displayed
	float oldMarker = 0f;											// Fixed height of the indicator during a jump

	private PlatformerCharacter2D character;


	void Awake() {
		character = GetComponent<PlatformerCharacter2D> ();
		wallCheck = transform.Find ("WallCheck");

		//compute the max jump height
		computeMaxHeight ();

		//disable display of the indicator if needed
		jumpHeightIndicator.GetComponent<SpriteRenderer> ().enabled = seeJumpHeight;
	}

	/* 
	 * This function sets "indicatorHeight" to the highest distance the player can jump in one jump
	 */
	private void computeMaxHeight() {
		indicatorHeight = 0f;
		float velocity = 0f;
		float new_height = 0f;
		float g = Mathf.Abs (Physics2D.gravity.y) * GetComponent<Rigidbody2D> ().gravityScale; //gravity absolute value

		float timer = 0f;
		while (timer < jumpTime) {
			float proportionCompleted = timer / jumpTime;
			float thisFrameForce;

			if (timer == 0)
				thisFrameForce = jumpForceY;
			else
				thisFrameForce = Mathf.Lerp (jumpForceAddedY, 0f, proportionCompleted);

			//ajout de la vitesse engendrée par la force à cette itération, appliquée pendant 0.02s
			velocity = velocity + (thisFrameForce * Time.fixedDeltaTime / GetComponent<Rigidbody2D> ().mass); //v = F*t/m

			//ajout de l'effet de la gravité pendant 0.02s
			velocity = -g * Time.fixedDeltaTime + velocity; //principe fondamental de la dynamique : v = -gt + v0

			//calcul de la nouvelle hauteur en fonction de la vélocité courante et de la gravité
			new_height = (-(1 / 2) * g * Time.fixedDeltaTime * Time.fixedDeltaTime + velocity * Time.fixedDeltaTime); //principe fondamental de la dynamique : h = -1/2gt² + v0t
			//conservation de la hauteur la plus haute
			indicatorHeight = Mathf.Max (indicatorHeight, indicatorHeight + new_height);

			timer += Time.fixedDeltaTime;
		}
		//si le personnage est encore en train de monter, il faut trouver la hauteur max atteinte après la dernière force appliquée :
		if (velocity > 0) {
			Debug.Log (velocity);
			indicatorHeight = indicatorHeight + (velocity * velocity / (2 * g)); // utilisation de la conservation de l'énergie mécanique : 1/2(mv²) = mgh
		}
	}

	void FixedUpdate() {
		// boolean for walljumps
		walled = Physics2D.OverlapCircle(wallCheck.position, walledRadius, character.whatIsGround);

		// update the max height indicator position. (transform.position.y + 1f) corresponds to the position of the robot's head
		// if the player is jumping, the indicator must be displayed at a constant height (oldMarker)
		if (oldMarker == 0f)
			jumpHeightIndicator.transform.position = new Vector3 (transform.position.x, transform.position.y + 1f + indicatorHeight, 0f);
		else
			jumpHeightIndicator.transform.position = new Vector3 (transform.position.x, oldMarker, 0f);
	}

	void Update () {
		//reset the jump limit if the player touches the ground
		if (character.grounded && !jumping) {
			jumps_left = jumps_limit; 
		}
	}

	// initiate a jump
	public void jump() {
		lockMarker ();
		StartCoroutine (JumpRoutine ());
	}

	//returns true if the player has the ability to jump
	public bool canJump() {
		return (jumps_left > 0 || walled);
	}

	//lock the position of the max height indicator (this is done during a jump)
	public void lockMarker() {
		oldMarker = transform.position.y + 1f + indicatorHeight;
	}
	//unlock the position of the max height indicator
	public void unlockMarker() {
		oldMarker = 0f;
	}


	/* 
	 * This routine make the player perform a jump
	 * It differenciates a normal jump and a walljump
	 */
	IEnumerator JumpRoutine()
	{
		//reset the velocity of the player
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

		jumping = true; // indicates that the character jumps

		if (walled && !character.grounded) { // *** walljump ***
			// add a force in the x-axis and in the y-axis (smaller amount)
			GetComponent<Rigidbody2D> ().AddForce(new Vector2 (jumpForceX * (character.facingRight ? -1 : 1), jumpForceY));
			//flips the robot
			character.Flip (); 

			// prevent the player from moving during a period of wallJumpTime
			character.move_allowed = false;
			yield return new WaitForSeconds (wallJumpTime);
			character.move_allowed = true;

		} else { // *** normal jump ***
			jumps_left--; //decrement the number of jumps allowed
			character.move_allowed = true; //the player can cancel the "post walljump immobilisation" if he does a normal jump right after the walljump

			float begin_height = transform.position.y; //height of the player before the beginning of the jump
			float cur_height = 0f; //current height of the player, relative to its height at the beginning of the jump
			float timer = 0;
			// add-force-loop while the player has the jump key pressed (and while this lasts less than jumpTime)
			// protocol : 1st force : jumpForceY ; then, at each iteration, a force between jumpForceAddedY and 0 is added
			// the more time passes, the closer to 0 this force will be
			while (CrossPlatformInput.GetButton("Jump") && timer < jumpTime) 
			{
				float proportionCompleted = timer / jumpTime;
				Vector2 thisFrameJumpVector;

				//determine which quantity of force will be used at this iteration
				if (timer == 0)
					thisFrameJumpVector = new Vector2 (0f, jumpForceY);
				else
					thisFrameJumpVector = Vector2.Lerp(new Vector2 (0f, jumpForceAddedY), Vector2.zero, proportionCompleted);

				// add the force to the character
				GetComponent<Rigidbody2D> ().AddForce(thisFrameJumpVector);

				// determine if the max height has been reached. If so, the max height indicator is unlocked
				if (transform.position.y - begin_height < cur_height) {
					unlockMarker();
				} else {
					cur_height = transform.position.y - begin_height;
				}
				timer += Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate(); //Pour que les quantums de force soient appliqués toutes les 0.02s (facilite le calcul de hauteur maximum)
			}
		}

		// unlocks the max height marker, in case it has been locked and not unlocked (this case happens when the player releases the jump button before reaching the max height)
		unlockMarker(); 
		jumping = false; // indicates the character is not jumping any more
	}
}
