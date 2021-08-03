using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterControl : MonoBehaviour {
	//TODO ignore targets that are too far off on the y-axis (i.e. grounded)

	public ParticleSystem ThrusterLeft;
	public ParticleSystem ThrusterRight;
	public GameObject missile;
	public GameObject MissileRight;
	public GameObject MissileLeft;
	public GameObject OwnerBase;
	public GameObject modelTexture; //the actual visible part of the fighter, not the navmesh that's at ground level
	public float firingRange; //distance to release the missiles
	public float pursuitRange; //pursuit range of to aquire attack targets
	public float patrolRange; //range away from owner the unit will patrol

	private string attackType = "AirUnit"; //type is hardcoded for fighters 
	private Vector3 attackOffset = new Vector3 (10f, 0f, 10f); //fighters shouldn't fly directly at their target. this is the offset
	private int owner;


	// TODO set all to private
	public GameObject target;
	public bool hasTarget; //this unit only moves to the target until it delivers it's missile load
//	private bool canPatrol; //has no target, is prepared to patrol to a random target
	public bool hasPatrol; // has a random patrol target set
	private Vector3 randTarget; //randomly generated patrol target
	public bool attackCD; //is the attack ON cooldown?
	public bool doneBuilding; //set when the unit is finished with construction
	
	void Start () {
		if (OwnerBase != null) {
			randTarget = OwnerBase.transform.position;
		} else {
			randTarget = transform.position;
		}
		owner = transform.root.gameObject.GetComponent<UnitLife> ().getOwner ();
	//	InitialBuild ();

	}

	/// <summary>
	/// Raises the texture model from 0,0,0 to the sky, then initializes patrol
	/// </summary>
	public void InitialRaise(){
		StartCoroutine (raise ());
	}

	IEnumerator raise(){
		while (modelTexture.transform.localPosition.y < 10) {
			modelTexture.transform.localPosition = modelTexture.transform.localPosition + new Vector3(0f, .05f, 0f);
			yield return new WaitForSeconds (.04f);
		}
		doneBuilding = true;
		StartCoroutine (seekTargets ());
		yield break;
	}
	

	void Update () {
		//Fires missiles when approaching a target
		if (target != null && hasTarget) {
			if( Vector3.Distance (target.transform.position, transform.position) < firingRange){
				if(!attackCD){ //attack is off cooldown
					hasTarget=false;
					attackCD=true;
					StartCoroutine(resetAttackCD());
					GameObject MissileLaunched;
					//TODO move missile texture to actual spawn location
					MissileLaunched = Instantiate(missile, 
		                              new Vector3(MissileLeft.transform.position.x, 0f, MissileLeft.transform.position.z), 
		                              Quaternion.identity);
					MissileLaunched.GetComponent<SeekTarget>().setTarget(target);
					MissileLaunched = Instantiate(missile, 
		                              new Vector3(MissileRight.transform.position.x, 0f, MissileRight.transform.position.z), 
		                              Quaternion.identity);
					MissileLaunched.GetComponent<SeekTarget>().setTarget(target);
				}
			}
		} 

		if (hasTarget && target!=null && attackCD) { //combat patrol when waiting for cooldown
			if(Vector3.Distance (randTarget, transform.position) < 17f){
				//		Debug.Log("Finished patrol ");
				hasPatrol=false;
			}
		}
		//patrols when no target
		if (!hasTarget) {
			if(Vector3.Distance (randTarget, transform.position) < 17f){
		//		Debug.Log("Finished patrol ");
				hasPatrol=false;
			}
		}

	}

	public void setTarget(GameObject obj){ 
		target = obj; 
		hasTarget = true;
		gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (target.transform.position + attackOffset);
	}

	IEnumerator resetAttackCD() {
		yield return new WaitForSeconds(2.9f);
		attackCD = false;
		yield break;
	}

	IEnumerator seekTargets() {

		while (true) {
		//	Debug.Log ("Scanning for target");

			yield return new WaitForSeconds(1f);

			if(doneBuilding){

			if(!hasTarget && !attackCD){ //no target and attack is off cooldown

				//scan for targets
				Collider[] hitColliders = Physics.OverlapSphere (transform.position, pursuitRange);
				float minDistance=float.MaxValue;
				int closest = -1;
				float distance;
				target = null;

				//choose nearest target
				//TODO fighters should possibly just pick a target at random, not bother scanning for closeness
				int i = 0;
				while (i < hitColliders.Length) {
					if ( hitColliders [i].transform.root != transform && //not self or children
						(hitColliders [i].gameObject.tag == attackType ) && 
					    hitColliders [i].gameObject != transform.gameObject) {
							UnitLife life = hitColliders[i].gameObject.GetComponent<UnitLife>();
							if(life != null){ //if the target has a life module
								if(life.getOwner() != owner){ //if the owner is not the same as this target
									distance = (transform.position - hitColliders[i].gameObject.transform.position).sqrMagnitude;
									if(distance<minDistance){
										minDistance=distance;
										closest=i;
									}
								}
							}
					}
					i++;
				}

				if(closest>=0){ target = hitColliders [closest].gameObject; } //if a closest was found
			}

			//if a target was found by the search
			if(target!=null){ hasTarget = true;  
			}else{ hasTarget = false; }

			if(hasTarget && !attackCD){ //if a target was found and attack is off cooldown, path towards it
				gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (target.transform.position + attackOffset);
				hasPatrol=false; //neccessary to start combat patrols while a target is set
		//		Debug.Log ("set target destination to " + target.transform.position);
			}else if(OwnerBase!=null && !hasTarget && !hasPatrol ){ //if a target was not found, and the owner object is set, path to a random patrol destination
				hasPatrol = true;
				float randX = Random.Range( OwnerBase.transform.position.x - patrolRange, OwnerBase.transform.position.x + patrolRange );
				float randZ = Random.Range( OwnerBase.transform.position.z - patrolRange, OwnerBase.transform.position.z + patrolRange );
				randTarget = new Vector3(randX, transform.position.y, randZ);
				gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (randTarget);
		//		Debug.Log ("set random patrol destination to " + randTarget);
			}else if(OwnerBase!=null && target!=null && attackCD && !hasPatrol){ //if has target and attack on CD, patrol around the target
				hasPatrol = true;
				float randX = Random.Range( target.transform.position.x - patrolRange, target.transform.position.x + patrolRange );
				float randZ = Random.Range( target.transform.position.z - patrolRange, target.transform.position.z + patrolRange );
				randTarget = new Vector3(randX, transform.position.y, randZ);
				gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (randTarget);
		//		Debug.Log ("set combat patrol destination to " + randTarget);
			}
		//	Debug.Log ("has target: " + hasTarget + " can patrol " + canPatrol + " has patrol "  + hasPatrol);
			
			} //end if done building
		}
	}
	
}
