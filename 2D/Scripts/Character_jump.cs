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

	Transform jumpHeightIndicator;
	float indicatorHeight;
	[SerializeField] bool seeJumpHeight = true;
	float oldMarker = 0f;

	private PlatformerCharacter2D character;


	void Awake() {
		character = GetComponent<PlatformerCharacter2D>();
		wallCheck = transform.Find ("WallCheck");

		// calculate the maximum height from the character
		jumpHeightIndicator = GameObject.Find ("jumpHeightIndicator").transform;
		indicatorHeight = 0;
		float velocity = 0f;
		float new_height = 0f;
		float timer = 0f;
		float g = Mathf.Abs (Physics2D.gravity.y) * GetComponent<Rigidbody2D> ().gravityScale;
		while (timer < jumpTime) {
			float proportionCompleted = timer / jumpTime;
			float thisFrameForce;

			if (timer == 0)
				thisFrameForce = jumpForceY;
			else
				thisFrameForce = Mathf.Lerp (20f, 0f, proportionCompleted);

			//ajout de la vitesse engendrée par la force à cette itération, appliquée pendant 0.02s
			velocity = velocity + (thisFrameForce * Time.fixedDeltaTime / GetComponent<Rigidbody2D> ().mass);

			//ajout de l'effet de la gravité pendant 0.02s
			velocity = -g * Time.fixedDeltaTime + velocity;

			//calcul de la nouvelle hauteur en fonction de la vélocité courante et de la gravité
			new_height = (-(1 / 2) * g * Time.fixedDeltaTime * Time.fixedDeltaTime + velocity * Time.fixedDeltaTime);
			//conservation de la hauteur la plus haute
			indicatorHeight = Mathf.Max (indicatorHeight, indicatorHeight + new_height);

			timer += Time.fixedDeltaTime;
		}
		//si le personnage est encore en montée, il faut trouver la hauteur max atteinte :
		if (velocity > 0) {
			Debug.Log (velocity);
			indicatorHeight = indicatorHeight + (velocity * velocity / (2 * g));
		}

		//mise en place du marqueur de position (transform.localScale.y*(0.6f) correspond au haut de la tete du robot)
		jumpHeightIndicator.GetComponent<SpriteRenderer> ().enabled = seeJumpHeight;
	}

	void FixedUpdate() {
		//boolean for walljumps
		walled = Physics2D.OverlapCircle(wallCheck.position, walledRadius, character.whatIsGround);

		if (oldMarker == 0f) {
			jumpHeightIndicator.position = new Vector3 (transform.position.x, transform.position.y + 1f + indicatorHeight, 0f);
		}
		else
			jumpHeightIndicator.position = new Vector3 (transform.position.x, oldMarker, 0f);
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

	public void lockMarker() {
		oldMarker = transform.position.y + 1f + indicatorHeight;
	}
	public void unlockMarker() {
		oldMarker = 0f;
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

			float begin_height = transform.position.y;
			float cur_height = 0f;
			float timer = 0;
			while (CrossPlatformInput.GetButton("Jump") && timer < jumpTime)
			{
				float proportionCompleted = timer / jumpTime;
				Vector2 thisFrameJumpVector;

				if (timer == 0)
					thisFrameJumpVector = new Vector2 (0f, jumpForceY);
				else
					thisFrameJumpVector = Vector2.Lerp(new Vector2 (0f, 20f), Vector2.zero, proportionCompleted);

				GetComponent<Rigidbody2D> ().AddForce(thisFrameJumpVector);

				if (transform.position.y - begin_height < cur_height) { //on a atteint le point max du saut
					unlockMarker();
				} else {
					cur_height = transform.position.y - begin_height;
				}
				timer += Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate(); //Pour que les quantums de force soient appliqués toutes les 0.02s (facilite le calcul de hauteur maximum)
			}
		}

		unlockMarker();
		jumping = false;
	}
}
