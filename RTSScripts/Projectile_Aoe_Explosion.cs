using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; //TODO possibly remove dependence on linq, if a better solution is found
//An alternative is to check if objects are distinct before adding them

public class Projectile_Aoe_Explosion : MonoBehaviour {

	public GameObject Kaboom; //explosion effect
	public float range;
	public int damage;
	public int directDamage; //damage done directly to a hit target, in ADDITION to aoe
	public string attackType1;
	public string attackType2;
//	public float delayDamageTime; //time to wait before applying the damage effect

	private int layerMask = Constants.IGNORE_RAYCAST_LAYERMASK;

	void OnCollisionEnter(Collision collision) {
	//	Debug.Log("col");
		GameObject hit;
		hit = collision.gameObject;
		
		if (hit.tag == attackType1 || hit.tag == attackType2) {
			if(hit.GetComponent<UnitLife>() != null) {
				hit.GetComponent<UnitLife>().Damage(directDamage);
			}
		}

		Instantiate (Kaboom, transform.position, Quaternion.identity);

		StartCoroutine(DoDamage(transform.position));
	}

	IEnumerator DoDamage(Vector3 impactLoc) {

		//adding a delay doesn't really work due to the delayed destroy
//		yield return new WaitForSeconds (delayDamageTime);
		if (damage > 0) { //helps efficiency

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
		}
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
