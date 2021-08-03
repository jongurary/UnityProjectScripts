using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefine : MonoBehaviour {

	private PowerControl pow;
	private ResourceControl res;

	[Tooltip("Source resource consumed per tick, e.g. \"Steel\"")]
	public int[] sourcePerTick; //source resource pulled from the ground each tick
	[Tooltip("How many input resources are required to make a single output resource")]
	public int[] inputPerOutput; //how many input resources are required to make each output resource
	[Tooltip("Bonus multiplier for output production, does not effect input consumption")]
	public int[] multiplier; //final bonus multiplier for production
	public int[] powerPerSecond; //power drained per second of ammo production
	public int ammoDistribPowerCost; //power cost to ship any kind of ammo
	
	public List<GameObject> connections = new List<GameObject>(); //connected users with an ammo controller serviced by this hub
	
	public GameObject particleAnim; //whatever particle animation this object has
	public GameObject giveAnim; //particle system played when ammo is given to a unit.
	[Tooltip("Ammo of each type given per unit per tick, corresponding to the output resource in the resource controller")]
	public int[] ammoGivenPerTick; //ammo of each type given per tick, corresponding to the output resource in the resource controller.

	
	void Start () {
		pow = GetComponent<PowerControl> ();
		res = GetComponent<ResourceControl> ();
		if (particleAnim != null) {
			ParticleSystem.EmissionModule smokeanim = particleAnim.GetComponent<ParticleSystem> ().emission;
			smokeanim.rateOverTime = 0;
		}
		StartCoroutine(Refine());
		StartCoroutine (DistributeAmmo ());
	}
	
	public void issueCommand(int status, GameObject obj){
		if (obj != gameObject) { //ignore anything targeting self
			if (status == 1) {	
				//do nothing
			} else if (status == 2) {
				//do nothing
			}
		}
	}

	/// <summary>
	/// Distributes held ammo (NOTE: stored in the RESOURCE CONTROL module, this unit does NOT have an ammo control module)
	/// to the ammo control module of connected units.
	/// </summary>
	IEnumerator DistributeAmmo() {
		while (true) {
			yield return new WaitForSeconds(Constants.AMMO_REFINE_TICK_RATE);
			//gives ammo to each object in connections in the order they were added
			//Note: may not be able to service all objects in the connections list, later objects may not have enough ammo
			foreach(GameObject obj in connections){
				if(obj!=null){
					AmmoControl amCont = obj.GetComponent<AmmoControl>();
					if( amCont!=null ){
						string type = amCont.getType();
						int index = res.getIndexofOutputType(type);
						if(index>-1){
							if( !res.isEmpty(ammoGivenPerTick[index], index) && !pow.isEmpty(ammoDistribPowerCost)  && !amCont.isFull(ammoGivenPerTick[index]) ){
								res.drain(ammoGivenPerTick[index], index);
								pow.drain(ammoDistribPowerCost);
								spawnAnimation(obj);
								amCont.charge(ammoGivenPerTick[index]);
							//	Debug.Log ("gave ammo to " + obj);
							}
						}
					}
				}
			}

		}
		yield break;
	}

	//Note: Potentially maintain a list of animations like linkage. This may be more efficient than instantiating new objects repeatedly.
	private void spawnAnimation(GameObject target){

		//Used to generate the particle emitter parent object
		GameObject ammoLink = Instantiate (giveAnim, transform.position, transform.rotation, transform);
		ammoLink.name = "AmmoLink_" + gameObject.name + "_" + target.name;
		ammoLink.transform.rotation = Quaternion.LookRotation(-transform.position + target.transform.position);
		//!Endpoint must be the first (or only) child of the particle system! (could fix by looping through all children, but MUH EFFICIENCY)
		//this sets the endpoint of the particle effect
		ammoLink.transform.GetChild(0).transform.position=target.transform.position;
		StartCoroutine (updateAnimationPosition (target, ammoLink));
	}

	//Note: this is probably fairly processing intensive.
	IEnumerator updateAnimationPosition( GameObject target, GameObject link){
		while (true) {
			yield return new WaitForSeconds(.1f);
			if(link==null || target ==null){
				yield break;
			}else{
				link.transform.rotation = Quaternion.LookRotation(-transform.position + target.transform.position);
				link.transform.GetChild(0).transform.position=target.transform.position;
			}
		}
	}



	/// <summary>
	/// Refines resources into different kinds of ammunition
	/// </summary>
	IEnumerator Refine() {
		int index;
		string inputType;
		bool smokeOn;

		while (true) {
			yield return new WaitForSeconds (Constants.AMMO_REFINE_TICK_RATE);
			smokeOn = false; //smoke off by default

			for (int i=0; i<res.getInputCount(); i++) { //run through the inputs

				inputType = res.getInputType (i);
				switch (inputType) {
				case "Steel":
					//			Debug.Log ("Converting Steel to basic ammo");
					break;
				case "Fuel":
					//			Debug.Log ("Converting Fuel to explosives");
					break;
				default:
					break;
				}

				//if the system has power, the input isn't empty, and the output isn't full
				//NOTE: can be jammed such that only the first production is generated, due to insufficient power.
				if (!pow.isEmpty (powerPerSecond[i]) &&
				    !res.isEmptyInput (sourcePerTick[i], i) && res.isNotFull ( (sourcePerTick[i] / inputPerOutput[i]) * multiplier[i], i) 
			   ) {
					pow.drain (powerPerSecond[i]);
					res.drainInput (sourcePerTick[i], i);
					res.fill (( (sourcePerTick[i] / inputPerOutput[i]) * multiplier[i] ), i);
					smokeOn = true; //engage smoke when refining		
				}			
			} //end for loop

			if(particleAnim!=null){
				ParticleSystem.EmissionModule smokeanim = particleAnim.GetComponent<ParticleSystem> ().emission;
				if (smokeOn) {
					smokeanim.rateOverTime = 5;
				} else {
					smokeanim.rateOverTime = 0;
				}
			}
		}
	}
	
}
