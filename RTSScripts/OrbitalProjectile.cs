using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OrbitalProjectile : MonoBehaviour {
	
	public GameObject Kaboom; //explosion effect
	public GameObject structure; //frame is disabled, rather than destroyed on impact, to allow particle effects to persist temporarily.
	public float range;
	public int damage;
	public string attackType1;
	public string attackType2;

	private int layerMask = Constants.IGNORE_RAYCAST_LAYERMASK;

	private bool hasCrashed = false;
	
	void Start () {
		//Objects descending from orbit have a random initial angular velocity
		GetComponent<Rigidbody>().AddTorque(
			Random.Range (-20000000f, 20000000f), 
			Random.Range (-20000000f, 20000000f), 
			Random.Range (-20000000f, 20000000f) );

		GetComponent<Rigidbody>().AddForce(
			Random.Range (-5000000f, 5000000f), 
			0f, 
			Random.Range (-5000000f, 5000000f) );
	}

	void Update () {
		
	}

	void OnCollisionEnter(Collision collision) {
		//Hitting the ground, or anything, causes a significant boom.
		if (!hasCrashed) {
			hasCrashed = true;
			Instantiate (Kaboom, transform.position, Quaternion.identity);
			StartCoroutine (DoDamage (transform.position));
		}
	}

	IEnumerator DoDamage(Vector3 impactLoc) {

		List<GameObject> hitCollidersUnique = new List<GameObject> (); //unique objects to be damaged
	
		Collider[] hitColliders = Physics.OverlapSphere (impactLoc, range, layerMask);
	
		foreach (Collider col in hitColliders) {
			hitCollidersUnique.Add (col.transform.root.gameObject);
		}
	
		hitCollidersUnique = hitCollidersUnique.Distinct ().ToList (); //depends on linq, removes duplicate colliders in the same object
		foreach (GameObject obj in hitCollidersUnique) {
			if (obj.tag == attackType1 || obj.tag == attackType2) {
				if (obj.GetComponent<UnitLife> () != null) {
					obj.GetComponent<UnitLife> ().Damage (damage / 2);
				}
			}
		}
		hitCollidersUnique.Clear ();
	
		hitColliders = Physics.OverlapSphere (transform.position, range / 3, layerMask);
		foreach (Collider col in hitColliders) {
			hitCollidersUnique.Add (col.transform.root.gameObject);
		}
	
		hitCollidersUnique = hitCollidersUnique.Distinct ().ToList (); //depends on linq, removes duplicate colliders in the same object
		//inner radius extra damage
		foreach (GameObject obj in hitCollidersUnique) {
			if (obj.tag == attackType1 || obj.tag == attackType2) {
				if (obj.GetComponent<UnitLife> () != null) {
					obj.GetComponent<UnitLife> ().Damage (damage / 2);
				}
			}
		}

		//destroy the frame, but allow particle children to persist for some time.
		structure.SetActive (false);

		yield return new WaitForSeconds (4f);
		Destroy (gameObject);
		yield break;

	}

	//Draws a sphere in the editor for visualization
	private void OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		//Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
		Gizmos.DrawWireSphere (transform.position, range);
	}
}
