using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCarrier : MonoBehaviour {

	public string type; //type is set without a getter/setter and may change over the course of operation

	public int resourcesCarried;
	public GameObject load;
	public GameObject owner; //originating factory or mine
	public GameObject target;

	private UnityEngine.AI.NavMeshAgent navAgent;
	private ResourceControl OwnerRes; //the owner's resource control unit
	public bool hasMission; //false if ready to be assigned on a new movement mission
	public bool isMovingHome; //false if moving towards target, true if moving to owner
	private float stoppingDistance;
	public int resourceCapacity;

	void Start () {
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		navAgent.stoppingDistance = stoppingDistance - 2f; //slightly smaller than truck stopping distance
		isMovingHome = true; //initialize at home
		StartCoroutine ( runRoute () );
		//ignore collisions with owner and target to fix simple issues with pathing
		Physics.IgnoreCollision (owner.GetComponent<Collider> (), GetComponent<Collider> ()); 
		Physics.IgnoreCollision (target.GetComponent<Collider> (), GetComponent<Collider> ()); 
	}

	public void depositResources(int toDeposit){
		resourcesCarried = toDeposit;
		load.SetActive(true);
	}

	/// <summary>
	/// Gives the currently carried resources to the object identified as "target" and returns the amount given
	/// </summary>
	/// <returns>Resources given to target.</returns>
	public int giveResources(){
		int toGive = resourcesCarried;
		resourcesCarried = 0;
		load.SetActive(false);
		ResourceControl res = target.GetComponent<ResourceControl> ();
		int index = res.getIndexofInputType (type);
		int outIndex = res.getIndexofOutputType (type);

		//if this type is in the input list of target, and not full
		if (index != -1) {
			if( res.isNotFullInput( toGive, index) ){
				res.fillInput ( toGive, index );
			}
		// if this type is in the output list of target, and not full
		} else if (outIndex != -1) { 
			if( res.isNotFull( toGive, outIndex ) ){
				res.fill ( toGive, outIndex );
			}
		} else { 
			//ERROR: Truck is somehow giving stuff to a unit that can't support it.
		}
		return toGive;
	}

	/// <summary>
	/// Sends the truck to its target (code 0) or its owner (code 1). Other codes do nothing
	/// </summary>
	/// <param name="status">status code (0=target, 1=owner).</param>
	public void startMission(int status){
		if (status == 0) {
			hasMission = true;
			StartCoroutine (faceThenMove (target));
			isMovingHome = false;
		} else if (status == 1) {
			hasMission = true;
			StartCoroutine (faceThenMove (owner));
			isMovingHome = true;
		} else {
			//Error do nothing
		}
	}

	//TODO sanity check to revive/awaken "orphans"

	IEnumerator faceThenMove(GameObject target){
		//TODO there's probably a less ghetto way to do this.
		Quaternion lookTo = Quaternion.LookRotation (-transform.position + target.transform.position);
		float rotDifference = 10f; //distance from finished rotation
		//TODO consider doing this for static time instead of until proper rotation is met!
		while (rotDifference > 15f) {
	//		Debug.Log ("moving towards " + lookTo + " current distance " + rotDifference);
			rotDifference = Quaternion.Angle (transform.rotation, lookTo);
			yield return new WaitForSeconds(.05f);
			transform.rotation = 
				Quaternion.Slerp(transform.rotation, lookTo, .3f ) ;
		}

		//	Vector3 startPos = gameObject.transform.position;
	//	navAgent.updatePosition = false;
	//	navAgent.SetDestination (target.transform.position);
	//	yield return new WaitForSeconds (3f);
	//	navAgent.Warp (startPos);
	//	navAgent.updatePosition = true;
	//	Debug.Log ("destination locked " + target.transform.position);
		navAgent.SetDestination (target.transform.position);
		yield break;
	}

	IEnumerator runRoute(){
		int status=-1;
		while (true) {
			yield return new WaitForSeconds(2f); // this is not a priority so the scan time can be high

			if(target==null || owner==null){ //check if target and owner still exist
				//TODO commit suicide? Move to nearest recycling facility?
				yield break;
			}

			//if moving to target and within stopping distance of the target
			if( !isMovingHome && Vector3.Distance(transform.position, target.transform.position) < stoppingDistance ) {
				//	Debug.Log ("Moving Home");
					status = 1; //command move to owner
					hasMission=false;
					giveResources();
			//if moving home and within stopping distance of home
			}else if ( isMovingHome && Vector3.Distance(transform.position, owner.transform.position) < stoppingDistance ){
				if(OwnerRes != null){ //if this unit is bound to a resource producing structure
					int typeIndex = OwnerRes.getIndexofOutputType(type); //get index location of the output type in owner
					if( !OwnerRes.isEmpty(resourceCapacity, typeIndex) ){ //if the owner has resource to give
						bool canShip = false;
						ResourceControl res = target.GetComponent<ResourceControl> ();
						int index = res.getIndexofInputType (type);
						int outIndex = res.getIndexofOutputType (type);
						
						//if this type is in the input list of target, and not full
						if (index != -1) {
							if( res.isNotFullInput( resourceCapacity, index) ){
								canShip = true;
							}
							// if this type is in the output list of target, and not full
						} else if (outIndex != -1) { 
							if( res.isNotFull( resourceCapacity, outIndex ) ){
								canShip = true;
							}
						}

							if(canShip){ //go to the target
							//	Debug.Log ("Moving to target");
								status = 0; //command move to target
								hasMission=false;
								OwnerRes.drain( resourceCapacity, typeIndex );
								resourcesCarried = resourceCapacity;
								load.SetActive(true);
							}
				} //end owner has res to give
				} //end owner is not null
			} //end distance check

				if(!hasMission){
					startMission(status);
				}
			}
	}
		
	public void setOwner(GameObject obj){ 
		owner = obj; 

		if (owner.GetComponent<ResourceControl> () != null) {
			OwnerRes = owner.GetComponent<ResourceControl> ();
		//	type = OwnerRes.getOutputType (0); //Now handled by the spawner
		}
		   
		stoppingDistance = (owner.transform.localScale.z * 1.9f) + 4f; //slightly larger than size of factory for truck stopping

		//	Debug.Log (stoppingDistance);
	}

	/// <summary>
	/// Sets the drop-off target and attempts to find the resource type in the target's controller. 
	/// </summary>
	/// <param name="obj">target object.</param>
	public void setTarget(GameObject obj){ target = obj; }

	/// <summary>
	/// Gets or sets the type
	/// </summary>
	/// <value>type</value>
	public string TYPE
	{	get{ return type; }
		set{ type = value;	} }
	
//	public void setType(string str){ type = str; }
//	public string getType(){ return type; }
}
