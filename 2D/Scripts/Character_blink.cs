using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Character_blink : MonoBehaviour {
	// **** blinks ****
	[SerializeField] public bool can_blink = true;
	[SerializeField] float blink_radius = 7f;
	[SerializeField] float blink_delay = 5f;
	public bool blink_keep_velocity = true;

	// **** blink icon ****
	public GameObject prefab_blink;

	private PlatformerCharacter2D character;

	
	// Update is called once per frame
	void Awake () {
		character = GetComponent<PlatformerCharacter2D>();
	}

	public void blink() {
		StartCoroutine (BlinkRoutine ());
	}

	public void flip_ghost (Vector3 scale) {
		prefab_blink.transform.localScale = scale;
	}


	IEnumerator BlinkRoutine()
	{
		bool is_in_range = true;
		Vector2 mousePosition = transform.position;
		GameObject blink = (GameObject)Instantiate (prefab_blink, new Vector3 (0, 0, 0), Quaternion.identity);
		Transform groundCheck = blink.transform.Find ("groundCheck");

		while (Input.GetKey(KeyCode.LeftShift))
		{
			mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			blink.transform.position = mousePosition;
			blink.transform.localScale = transform.localScale;

			if ((Vector2.Distance (mousePosition, transform.position) < blink_radius) && (!Physics2D.OverlapCircle(groundCheck.position, .3f, character.whatIsGround))) {
				is_in_range = true;
				blink.SetActive(true);
			} else {
				is_in_range = false;
				blink.SetActive(false);
			}
			yield return null;
		}

		Destroy (blink);

		if (is_in_range) {
			//teleport
			transform.position = mousePosition;
			if (!blink_keep_velocity) {
				GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
			}

			can_blink = false;
			float timer = 0;
			while (timer < blink_delay)
			{
				if (Time.fixedTime % .2 < .1) {
					GetComponent<SpriteRenderer> ().color = new Color (1f, 1f, 1f, .5f);
				} else {
					GetComponent<SpriteRenderer>().color = new Color (1f, 1f, 1f, 1f);
				}
				timer += Time.deltaTime;
				yield return null;
			}
			GetComponent<SpriteRenderer>().color = new Color (1f, 1f, 1f, 1f);
			can_blink = true;
		}
	}
}
