using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; //TODO possibly remove dependence on linq, if a better solution is found
//An alternative is to check if objects are distinct before adding them

public class AoeDamage : MonoBehaviour {
	
	public float range;
	public int damage;
	public int increments; //how many times to deal damage
	public float incrementTime; //time between damage ticks
	public float delayTime; //delay initial damage by this time
	public string attackType1;
	public string attackType2;
	public string attackType3;
//	public float delayDamageTime; //time to wait before applying the damage effect

	void Start() {
		StartCoroutine(DoDamage(transform.position));
	}

	IEnumerator DoDamage(Vector3 impactLoc) {

		yield return new WaitForSeconds (delayTime);

		int incrementsElapsed = 0;

		while(incrementsElapsed < increments) {

			List<GameObject> hitCollidersUnique = new List<GameObject> (); //unique objects to be damaged
			
			Collider[] hitColliders = Physics.OverlapSphere (impactLoc, range);
			
			foreach (Collider col in hitColliders) {
				hitCollidersUnique.Add (col.transform.root.gameObject);
			}
			
			hitCollidersUnique = hitCollidersUnique.Distinct ().ToList (); //depends on linq, removes duplicate colliders in the same object

			foreach (GameObject obj in hitCollidersUnique) {
				if (obj.tag == attackType1 || obj.tag == attackType2 || obj.tag == attackType3) {
					if (obj.GetComponent<UnitLife> () != null) {
							obj.GetComponent<UnitLife> ().Damage (damage);
					}
				}
			}

			yield return new WaitForSeconds (incrementTime);
			incrementsElapsed++;
			hitCollidersUnique.Clear ();

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
