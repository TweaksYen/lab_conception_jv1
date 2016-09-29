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
	public Image blink_status;


	
	// Update is called once per frame
	void Update () {
	
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

		while (Input.GetKey(KeyCode.LeftShift))
		{
			mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			blink.transform.position = mousePosition;
			blink.transform.localScale = transform.localScale;

			if (Vector2.Distance (mousePosition, transform.position) < blink_radius) {
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
			//yield return new WaitForSeconds(blink_delay);
			float timer = 0;
			while (timer < blink_delay)
			{
				blink_status.color = Color.Lerp (new Color (1f, 0f, 0f, 1f), new Color (1f, 1f, 1f, 1f), timer / blink_delay);
				timer += Time.deltaTime;
				yield return null;
			}

			can_blink = true;
		}
	}
}
