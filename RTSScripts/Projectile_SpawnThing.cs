using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns a gameobject on collision
/// </summary>
public class Projectile_SpawnThing : MonoBehaviour {

	public GameObject[] toSpawn; //whatever needs to be spawned on collision
	[Header("x-axis, y-axis, z-axis")]
	[Tooltip("Use randomly generated offsets for spawns. Objects will be spawned within random offset of position.")]
	public bool[] useRandomOffsets = new bool[3]; //if true, spawn objects with some random offset on x, y, and z axises respectively.
	public float randOffsetRange; //if useRandomOffset is true, this is the range on each axis to roll randoms
	[Header("If used, Unit must be the first targetType.")]
	[Header("If no targetType is desired, set targetType 1 to Any.")]
	public string targetType1;
	public string targetType2;
	public string targetType3;

	void OnCollisionEnter(Collision collision) {
	//	Debug.Log ("Boop" + collision.gameObject.tag);
		bool canCollide = false;
		if (targetType1 == "Any") { //if using the any tag, can collide with anything
			canCollide = true;
		} else { //has specific objects that are allowed to be collided with
			if (targetType1 != null && targetType1 != "") {
				if (collision.gameObject.CompareTag (targetType1)) {
					if (targetType1 == "Unit") {
						if (collision.gameObject.GetComponent<UnitLife> ().getOwner () != GetComponent<UnitLife> ().getOwner ()) {
							canCollide = true;
						}
					}
				}
			}
			if (targetType2 != null && targetType2 != "") {
				if (collision.gameObject.CompareTag (targetType2)) {
					canCollide = true;
				}
			}
			if (targetType3 != null && targetType3 != "") {
				if (collision.gameObject.CompareTag (targetType3)) {
					canCollide = true;
				}
			}
		}

		if (canCollide) {
					float xVector = 0f, yVector = 0f, zVector = 0f;
					if (useRandomOffsets [0] == true) {
						xVector = Random.Range (-randOffsetRange, randOffsetRange);
					} 
					if (useRandomOffsets [1] == true) {
						yVector = Random.Range (-randOffsetRange, randOffsetRange);
					} 
					if (useRandomOffsets [2] == true) {
						zVector = Random.Range (-randOffsetRange, randOffsetRange);
					}

					foreach (GameObject obj in toSpawn) {
						Instantiate (obj, transform.position + new Vector3 (xVector, yVector, zVector), Quaternion.identity);
					}

					Destroy (gameObject);
		}
	}

	//Allows objects to use a trigger collider instead
	void OnTriggerEnter(Collider col) {
	//	Debug.Log ("Moop" + col.gameObject.tag);
		bool canCollide = false;
		if (targetType1 == "Any") {
			canCollide = true;
		} else {
			if (targetType1 != null && targetType1 != "") {
				if (col.gameObject.CompareTag (targetType1) && !col.gameObject.CompareTag ("Shield")) {
					canCollide = true;
				}
			}
			if (targetType2 != null && targetType2 != "") {
				if (col.gameObject.CompareTag (targetType2) && !col.gameObject.CompareTag ("Shield")) {
					canCollide = true;
				}
			}
			if (targetType3 != null && targetType3 != "") {
				if (col.gameObject.CompareTag (targetType3) && !col.gameObject.CompareTag ("Shield")) {
					canCollide = true;
				}
			}
		}

		//ignore shield trigger colliders, let the shield handle these collision events
		if (canCollide) {
				float xVector = 0f, yVector = 0f, zVector = 0f;
				if (useRandomOffsets [0] == true) {
					xVector = Random.Range (-randOffsetRange, randOffsetRange);
				} 
				if (useRandomOffsets [1] == true) {
					yVector = Random.Range (-randOffsetRange, randOffsetRange);
				} 
				if (useRandomOffsets [2] == true) {
					zVector = Random.Range (-randOffsetRange, randOffsetRange);
				}
		
				foreach (GameObject obj in toSpawn) {
					Instantiate (obj, transform.position + new Vector3 (xVector, yVector, zVector), Quaternion.identity);
				}
		
				Destroy (gameObject);
	}
	}

}
