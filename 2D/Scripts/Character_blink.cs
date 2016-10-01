using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Character_blink : MonoBehaviour {
	// **** blinks ****
	bool canBlink = true;											// can the character teleport
	[SerializeField] float blink_radius = 7f;						// max radius outside of which the character can't blink
	[SerializeField] float blink_delay = 5f;						// delay before a new blink is possible
	[SerializeField] bool blink_keep_velocity = true;				// does the character keeps its speed when it teleports

	// **** blink icon ****
	public GameObject prefab_blink;									// the prefab corresponding to the blink-ghost

	private PlatformerCharacter2D character;						// Instance of the character main class

	
	// Update is called once per frame
	void Awake () {
		// Setting up reference
		character = GetComponent<PlatformerCharacter2D>();
	}

	//returns if the player can teleport
	public bool can_blink() {
		return canBlink;
	}

	//initiate a blink
	public void blink() {
		StartCoroutine (BlinkRoutine ());
	}

	//flip the blink-ghost sprite
	public void flip_ghost (Vector3 scale) {
		prefab_blink.transform.localScale = scale;
	}

	/* 
	 * This routine make the player perform a blink
	 * It has 2 phases :
	 * 1/ display the blink-ghost under the mouse
	 * 2/ teleport after the release of the key and set a cooldown until the player can do a blink again (during which the character is "flashing" (clignotement))
	 */
	IEnumerator BlinkRoutine()
	{
		bool is_in_range = true; // is the mouse whithin the blink radius
		Vector2 mousePosition = transform.position; //mouse position

		// creation of the blink-ghost
		GameObject blink = (GameObject)Instantiate (prefab_blink, new Vector3 (0, 0, 0), Quaternion.identity);
		// reference to the groundCheck of the ghost (used to prevent from blinking in the floor)
		Transform groundCheck = blink.transform.Find ("groundCheck");

		// 1st phase : ghost display
		while (Input.GetKey(KeyCode.LeftShift))
		{
			mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition); // get the real mouse position
			blink.transform.position = mousePosition; // set the ghost to follow the mouse
			blink.transform.localScale = transform.localScale; // set the ghost's localScale

			//check wether the ghost must be displayed : only if the mouse is in the blink range and if the ghost isn't "in" a piece of floor or wall
			if ((Vector2.Distance (mousePosition, transform.position) < blink_radius) && (!Physics2D.OverlapCircle(groundCheck.position, .3f, character.whatIsGround))) {
				is_in_range = true;
				blink.SetActive(true);
			} else {
				is_in_range = false;
				blink.SetActive(false);
			}
			yield return null;
		}

		// destroy the blink-ghost
		Destroy (blink);

		// 2nd phase : teleport and cooldown
		if (is_in_range) {
			//teleport the character to the current mouse position
			transform.position = mousePosition;
			//set speed to 0 if needed
			if (!blink_keep_velocity) {
				GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
			}

			canBlink = false; //prevent from blinking again
			float timer = 0;
			while (timer < blink_delay)
			{
				// set the alpha value of the character to 50% so it is a little transparent and then back to 100% visible, repeatedly
				if (Time.fixedTime % .2 < .1) {
					GetComponent<SpriteRenderer> ().color = new Color (1f, 1f, 1f, .5f);
				} else {
					GetComponent<SpriteRenderer>().color = new Color (1f, 1f, 1f, 1f);
				}
				timer += Time.deltaTime;
				yield return null;
			}
			//in case it stopped on a 50% visible state, set back the alpha to 100%
			GetComponent<SpriteRenderer>().color = new Color (1f, 1f, 1f, 1f);
			canBlink = true; // blinking is available
		}
	}
}
