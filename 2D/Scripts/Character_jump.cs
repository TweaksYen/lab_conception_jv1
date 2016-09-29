using UnityEngine;
using System.Collections;

public class Character_jump : MonoBehaviour {
	// **** jump characteristics ****
	[SerializeField] public float jumpForceX = 400f;	// Amount of force added when the player jumps.	
	[SerializeField] public float jumpForceY = 500f;	// Amount of force added when the player jumps.	
	[SerializeField] public float jumpTime = .7f;		// duration of the normal jump
	[SerializeField] public float wallJumpTime = .6f;	// duration of the walljump
	[Range(0, 1)]
	[SerializeField] public float airControl = .75f;			// Amount of maxSpeed applied to in air movement. 1 = 100%
	bool jumping = false;

	// **** multiple jumps ****
	[SerializeField] int jumps_limit = 2;
	int jumps_left = 2;

	// **** walljumps ****
	Transform wallCheck;
	[SerializeField] public bool walled;
	float walledRadius = .05f;							// Radius of the overlap circle to determine if walled

	private PlatformerCharacter2D character;


	void Awake() {
		character = GetComponent<PlatformerCharacter2D>();
		wallCheck = transform.Find ("WallCheck");
	}

	void FixedUpdate() {
		//boolean for walljumps
		walled = Physics2D.OverlapCircle(wallCheck.position, walledRadius, character.whatIsGround);
	}
	
	// Update is called once per frame
	void Update () {
		if (character.grounded && !jumping) {
			jumps_left = jumps_limit; //reset jump limit
		}
	}

	public void jump() {
		StartCoroutine (JumpRoutine ());
	}

	public bool canJump() {
		return (jumps_left > 0 || walled);
	}


	IEnumerator JumpRoutine()
	{
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

		jumping = true;

		if (walled && !character.grounded) { // *** walljump ***
			GetComponent<Rigidbody2D> ().AddForce(new Vector2 (jumpForceX * (character.facingRight ? -1 : 1), jumpForceY));
			character.Flip ();
			character.move_allowed = false;

			float timer = 0;
			while (timer < wallJumpTime)
			{
				timer += Time.deltaTime;
				yield return null;
			}

			character.move_allowed = true;
		} else { // *** normal jump ***
			jumps_left--;
			character.move_allowed = true;

			//calcul de la hauteur de saut (non fonctionnel
			float n = (jumpTime / Time.fixedDeltaTime) - 1;
			float r = -20*(Time.fixedDeltaTime / jumpTime);
			float u0 = 20 * (1 - (Time.fixedDeltaTime / jumpTime));
			float max_force = jumpForceY + (n+1)*(2*u0 + n*r)/2;
			float hauteur_saut = (max_force * jumpTime * jumpTime) / GetComponent<Rigidbody2D> ().mass;
			Debug.Log ("a priori : " + hauteur_saut);

			float timer = 0;
			while (CrossPlatformInput.GetButton("Jump") && timer < jumpTime)
			{
				float proportionCompleted = timer / jumpTime;
				Vector2 thisFrameJumpVector;

				if (timer == 0)
					thisFrameJumpVector = new Vector2 (0f, jumpForceY);
				else
					thisFrameJumpVector = Vector2.Lerp(new Vector2 (0f, 20f), Vector2.zero, proportionCompleted);
				max_force += thisFrameJumpVector.y;

				GetComponent<Rigidbody2D> ().AddForce(thisFrameJumpVector);
				timer += Time.fixedDeltaTime;
				yield return null;
			}
		}

		jumping = false;
	}
}
