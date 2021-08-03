using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceRefine : MonoBehaviour {

	private PowerControl pow;
	private ResourceControl res;

	[Tooltip("Input resource consumed per tick. Can be zero")]
	public int[] sourcePerSecond; //source resource pulled from the ground each tick
	[Tooltip("Input resource required per output resource generated. If sourcePerSecond is zero, then this is the resource generated per tick overall")]
	public int[] inputPerOutput; //how many input resources are required to make each output resource
	public int[] multiplier; //final bonus multiplier for production
	public int[] powerPerSecond; //power drained per second of resource production
	
	public List<GameObject> deliveryTrucks = new List<GameObject>(); //all delivery trucks owned by this facility
	public GameObject truckObject; //spawned delivery truck object
	public GameObject truckSpawn; //truck spawn location
	
	public GameObject smokeStack; //the pillar of beautiful carbon dust
	private bool building; //is the truck being built?

	
	void Start () {
		pow = GetComponent<PowerControl> ();
		res = GetComponent<ResourceControl> ();
		if (smokeStack != null) {
			var smokeanim = smokeStack.GetComponent<ParticleSystem> ().emission;
			smokeanim.rateOverTime = 0;
		}
		building = false;
		StartCoroutine(Refine());
	}
	
	public void issueCommand(int status, GameObject obj){
		if (obj != gameObject) { //ignore anything targeting self
			if (status == 1) {	
				StartCoroutine(buildTruck(obj, status-1));
			} else if (status == 2) {
				StartCoroutine(buildTruck(obj, status-1));
			}else if (status == 3) {
				StartCoroutine(buildTruck(obj, status-1));
			}
		}
	}

	/// <summary>
	/// Check if the status command given can be issued to this particular unit
	/// </summary>
	/// <param name="status">Status.</param>
	public bool doesStatusExist(int status){
		int outputSize = res.getOutputCount ();
		if (status > outputSize) {
			return false;
		} else {
			return true;
		}
	}
	
	/// <summary>
	/// Constructs a delivery truck and begins shipping resources to the target
	/// Requires target and specified index of the outputType for this node
	/// </summary>
	IEnumerator buildTruck(GameObject target, int typeIndex){

		if (!building) { //TODO add some kind of check if the location is blocked.
			string type = res.getOutputType(typeIndex);
			bool hasResource=false;
			
			ResourceControl tarRes= target.GetComponent<ResourceControl>(); //test if the target resource controller supports this resource.
			if (tarRes.getIndexofInputType (type) != -1) { //if this type is in the input list of target
				hasResource=true;
			}else if (tarRes.getIndexofOutputType (type) != -1) { // if this type is in the output list of target
				hasResource=true;
			}else{
				hasResource = false; //doesn't have this resource in its controller
			}
			
			if(hasResource){
				building = true;
				GameObject truck = Instantiate (truckObject, truckSpawn.transform.position, Quaternion.identity);
				ResourceCarrier carrier = truck.GetComponent<ResourceCarrier> ();
				carrier.setOwner (gameObject);
				carrier.TYPE=type;
				carrier.setTarget (target);
				yield return new WaitForSeconds (1f); //wait a bit to release construction
				building = false;
			}else{
				//TODO building error, target lacks the resource 
			}
		} //TODO building error, something already under construction
		yield break;
	}

	IEnumerator Refine() {
		int index;
		string inputType;
		bool smokeOn;

		smokeOn = false;
		while (true) {
			yield return new WaitForSeconds (1f);

			for (int i=0; i<res.getInputCount(); i++) { //run through the inputs

				inputType = res.getInputType (i);
				switch (inputType) {
				case "Iron Ore":
					//			Debug.Log ("Converting Iron Ore to Steel");
					break;
				case "Exotic Ore":
					//			Debug.Log ("Converting Iron Ore to Exotics");
					break;
				case "Uranium Ore":
					//			Debug.Log ("Converting Uranium Ore to Enriched Uranium");
					break;
				case "Atmosphere":
					//			Debug.Log ("Generating fuel");
					break;
				default:
					break;
				}


				if(sourcePerSecond[i] > 0){ //if this structure consumes input resources
					//if the system has power, the input isn't empty, and the output isn't full
					if (!pow.isEmpty (powerPerSecond[i]) &&
					    !res.isEmptyInput (sourcePerSecond[i], i) && res.isNotFull ( (sourcePerSecond[i] / inputPerOutput[i]) * multiplier[i], i) 
				   ) {
						pow.drain (powerPerSecond[i]);
						res.fill (( (sourcePerSecond[i] / inputPerOutput[i]) * multiplier[i] ), i);
						res.drainInput (sourcePerSecond[i], i);
						smokeOn = true;				
					}	
				}else{ //this structure does not consume input resources, it generates output with no input required
					//if the system has power, and the output isn't full
					if (!pow.isEmpty (powerPerSecond[i]) &&
					    res.isNotFull ( inputPerOutput[i] * multiplier[i], i) ) {
						pow.drain (powerPerSecond[i]);
						res.fill ( inputPerOutput[i] * multiplier[i] , i);
						smokeOn = true;				
					}
				}
			} //end for loop

			if(smokeStack!=null){
				ParticleSystem.EmissionModule smokeanim = smokeStack.GetComponent<ParticleSystem> ().emission;
				if (smokeOn) {
					smokeanim.rateOverTime = 10;
				} else {
					smokeanim.rateOverTime = 0;
				}
			}
		}
	}
	
}
