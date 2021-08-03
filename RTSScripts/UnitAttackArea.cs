using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles attacking for stationary turret type units
/// </summary>
public class UnitAttackArea : MonoBehaviour {
	
	public float attackSpeed;
	public GameObject projectile;
	public GameObject fireArea; //where the projectile comes out of
	public int damage; //autofire weapons like this can do direct damage, if desired
	public string attackType1; //tag the unit can attack, for example "AirUnit"
	public string attackType2;
	public GameObject attackParticle; //a particle system that's created on projectile spawn
	public float projectileSpeed;
	public float spread; //range along x and z axis of random spread
	
	public List<GameObject> inRange = new List<GameObject>();
	public bool hasTarget; //does the unit have a living target?
	private GameObject target;
	private float rangeSpeed =.25f; //how often we look for closest target
	private GameObject spawnedProjectile;
	private PulseParticle Pulser; //controls particle systems around projectile spawn
	private int owner; //owning player id, inherited from unitLife

	// Use this for initialization
	void Start () {
		StartCoroutine(Attack(attackSpeed));
		StartCoroutine(TestRange(rangeSpeed));
		StartCoroutine(CleanDead(7f));
		hasTarget = false;
		if (attackParticle != null) {
			Pulser = attackParticle.GetComponent<PulseParticle> (); }
		owner = transform.root.gameObject.GetComponent<UnitLife> ().getOwner ();
	}

	void Update () {
		if (target == null) {
			hasTarget=false;
		}
	}

	void OnTriggerEnter(Collider other){
		if (other.tag == attackType1 || other.tag == attackType2) {
			inRange.Add (other.transform.root.gameObject);
		//		Debug.Log ("added: "+ other.transform.root.gameObject);
		}
	}
	
	void OnTriggerExit(Collider other){
		if (other.tag == attackType1 || other.tag == attackType2) {
			inRange.Remove (other.transform.root.gameObject);
		//		Debug.Log ("removed: "+other.transform.root.gameObject);

			//Purge the target if it leaves range
			if(other.transform.root.gameObject==target)
				target=null;
		}
	}

	//Attack if all conditions are met
	IEnumerator Attack(float waitTime) {

		while (true) {
			yield return new WaitForSeconds (waitTime);
			if(hasTarget && target!=null)
			{
				if(Pulser!=null){
					//EFFECTS STUFF
				}
				spawnedProjectile = (GameObject)Instantiate (projectile, fireArea.transform.position, transform.rotation);

				spawnedProjectile.GetComponent<Rigidbody>().AddForce(
					(target.transform.position - fireArea.transform.position).normalized * projectileSpeed);
				spawnedProjectile.GetComponent<Rigidbody>().AddForce(
					Random.Range(-spread, spread), 0f, Random.Range (-spread, spread));
			}
		}} //end while true, end method

	//Periodically determines the closest target
	IEnumerator TestRange(float waitTime) {
		float distance;
		float minDistance;

		while (true) {
			yield return new WaitForSeconds (waitTime);
			if(target==null){ //this should be enough to purge dead targets
				hasTarget=false;
			}

			//picks the closest target
			if(!hasTarget){
				minDistance=float.MaxValue;
				target = null;
				//TODO if performance is impacted, cap number of scanned units

				foreach (GameObject obj in inRange) {
					if(obj!=null){
					if ((obj.tag == attackType1 || obj.tag == attackType2) &&
					obj != transform.gameObject) {
						distance= (transform.position - obj.transform.position).sqrMagnitude;
						UnitLife life = obj.gameObject.GetComponent<UnitLife>();
						if(life != null){ //if the target has a life module
							if(life.getOwner() != owner){ //if the owner is not the same as this target
								distance = (transform.position - obj.gameObject.transform.position).sqrMagnitude;
								if(distance<minDistance){
									minDistance=distance;
									target=obj;
								}
							}
						}
					}}
				}
				if(target!=null){
					hasTarget = true;
				//	Debug.Log ("target scan picked: " + target);
				}
		} //end if !hasTarget
		//	Debug.Log(target);
		} //end while true
	} //end method

	//slow cleanup of dead units, very minor performance impact
	IEnumerator CleanDead(float waitTime) {
		while (true) {
			yield return new WaitForSeconds (waitTime);
			inRange.RemoveAll(item => item == null);
			inRange.TrimExcess();
		}
	}
	
}
