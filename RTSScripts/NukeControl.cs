using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls ground-targeting missiles that climb into the sky then fall back down again
/// </summary>
public class NukeControl : MonoBehaviour {

	public Vector3 target;
	public string name;

	private float targetHeight = Constants.MISSILE_MAX_HEIGHT; //height to reach before beginning path towards target
	public bool initiateLaunch;
	private bool hasHeight; //has the missile reached the height cieling?
	private bool facingTarget; //has the missile rotated towards the target successfully
	private bool isArmed; //arm the missle during descent phase

	void Start () {
		initiateLaunch = false;
		hasHeight = false;
		facingTarget = false;
		isArmed = false;
		GetComponent<Rigidbody> ().Sleep();
		StartCoroutine (RunMission ());
	}
	

	void Update () {
		
	}

	IEnumerator RunMission(){
		float forceToAdd = 10000f;

		while (true) {
			yield return new WaitForSeconds(.1f); //TODO consider using fixedupdate

			if(initiateLaunch){
				GetComponent<Rigidbody> ().WakeUp();

				//check states
				if (!hasHeight && transform.position.y > targetHeight) {
					hasHeight = true;
					forceToAdd = 10000f;
				}
				Quaternion lookTo = Quaternion.LookRotation (-transform.position + target);
				if(!facingTarget && Quaternion.Angle(transform.rotation, lookTo) < 5f){
					facingTarget = true;
				}
				if(facingTarget && hasHeight){
					isArmed = true;
					GetComponent<Collider>().enabled = true;
				}else{ //for now, this is neccessary because the unitbuilder re-enables colliders
					GetComponent<Collider>().enabled = false;
				}
				

				//conduct mission phases
				//phase 1: climb to the heavens
				if (!hasHeight) { 
					forceToAdd += 3000f;
				//	Debug.Log ("added force to missile: " + forceToAdd);
					GetComponent<Rigidbody> ().AddForce (new Vector3 (0f, forceToAdd, 0f));
				}else { //height is reached, rotate to target and begin descent
				//phase 2: rotate to target

					if(!facingTarget){ //TODO maybe get rid of this since it's off camera anyways
						transform.rotation = Quaternion.Slerp (transform.rotation, lookTo, .1f);
					}else{ //target facing is reached, being descent

				//phase 3: final descent
						if(facingTarget){
							transform.LookAt(target);
							GetComponent<Rigidbody>().AddRelativeForce(0, 0, forceToAdd);
							//GetComponent<Rigidbody>().AddForce((target - transform.position).normalized * forceToAdd);
							forceToAdd += 5000f;
						}
					}
				} //end if height reached
			} //end if can launch 
		} //end while true
	}
}
