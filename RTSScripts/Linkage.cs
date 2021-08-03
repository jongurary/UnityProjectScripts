using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linkage : MonoBehaviour {

	//TODO consider converting to floats if performance impact is negligible
	public int maxAmpsOut; //maximum transer rate, in amps, of this unit
	public int maxAmpsIn; //maximum input rate, in amps, to this unit
	public int maxOutLinks; //max targets allowed out
	public int maxInLinks; //max targets allowed in
	public float linkRange; //maximum euclidean distance of link range along the x/y plane
	private float gizmoRange; //draws the range gizmo based on euclidean distance
	private int currentAmpsOut;

	[Tooltip("Controls the link model as well as the link efficiency which is controlled by the attached animator")]
	public GameObject lowLinkModel;
	public GameObject lowLinkObject;
//	public Mesh temp;
//	public Material tempmat;

	//Note: ensure null items are always purged from the list to keep counts accurate
	public List<GameObject> outputs = new List<GameObject>(); //units where this generator is transmitting power only
	public List<GameObject> outputLinks = new List<GameObject>(); //holds the animations between linkages
	public List<int> outputWatts = new List<int> (); //output power of each individual link, in watts
	public List<GameObject> inputs = new List<GameObject>(); //units where this generator is recieving power only
	public List<GameObject> pairings = new List<GameObject>(); //units where this generator is transferring or recieving dynamically

	public List<GameObject> notFullOutputs = new List<GameObject>();

	private int numOut, numIn, numPiarings;
	private float transmissionSpeed = 1f; //Amps are transmitted once per second, this should be universal among all devices
	private PowerControl powercontrol; //the unit's personal power control system

	private int voltage = Constants.VOLTAGE_DEFAULT; 



	void Start () {
		//Initialize counts to size of lists
		numOut = outputs.Count;
		numIn = inputs.Count;
		numPiarings = pairings.Count;

		//begin over-link transmission
		powercontrol = GetComponent<PowerControl> ();
		StartCoroutine(transmit(transmissionSpeed));
		StartCoroutine(purgeDeadLinks());
		//only if the object is a wireless charger do we need to update link positions often, otherwise we can do it slowly
		if (GetComponentInChildren<WirelessCharger> () != null) {
			StartCoroutine (updateLinkPositions (.05f));
		}else{
			StartCoroutine (updateLinkPositions (1f));
		}
	}

	//transmit energy to output links
	IEnumerator transmit(float waitTime) {
		while(true){
			yield return new WaitForSeconds (waitTime);

			notFullOutputs.Clear(); //sanity check
			currentAmpsOut=0; //reset the status display to zero
			int maxTransmissionRate; //maximum rate of transmission to any given output, based on averaging available output

			Linkage targetLink;
			PowerControl targetPower;
			//find how many output targets are not full, or would be full if given a complete full-strength charge cycle
			foreach( GameObject obj in outputs){
	//			targetLink = obj.GetComponent<Linkage>();
				targetPower = obj.GetComponent<PowerControl>();
				int outputIndex=outputs.IndexOf(obj); //index of the object in the main objects list
				LinkAnimationManager linkManager = outputLinks[outputIndex].GetComponent<LinkAnimationManager>();
	//			int maxAmpsIn = targetLink.maxAmpsIn;

				//initalize the link to 0 watts
				linkManager.setWatts(0);
	//			outputWatts[outputIndex]= 0;

				if ( !targetPower.isFull(1) ) { //simply check which outputs can recieve *ANY* charge
					notFullOutputs.Add(obj);
				}

				/* //TODO NEEDLESSLY COMPLEX DOESNT SEEM TO DO ANYTHING, SCHEDULE TO REMOVE
				if( !targetPower.isFull(maxAmpsIn * voltage) ){
					notFullOutputs.Add(obj);
				}else{
					outputLinks[outputIndex].GetComponent<LinkAnimationManager>().setWatts(0);
					outputWatts[outputIndex]= 0;
				}

				if( maxTransmissionRate < maxAmpsIn && !targetPower.isFull(maxTransmissionRate * voltage) && !notFullOutputs.Contains(obj) ){
					notFullOutputs.Add(obj);
				}else{
					outputLinks[outputIndex].GetComponent<LinkAnimationManager>().setWatts(0);
					outputWatts[outputIndex]= 0;
				}
				*/
			}

			if(notFullOutputs.Count > 0){// don't divide by zero

			//max output current is divided evenly among recievers that can recieve power
			maxTransmissionRate = maxAmpsOut/notFullOutputs.Count;

				if(!powercontrol.isEmpty(maxAmpsOut * voltage)){ //if the battery is able to distribute this power load in its entirity

				int remainingBandwidth = maxAmpsOut; //bandwidth remaining for remaining outputs
				int remainingOutputs = notFullOutputs.Count; //outputs remaining to satisfy
				int unUsedBandwidth; //current capacity still available after all outputs are satisfied

				foreach( GameObject obj in notFullOutputs){
					targetLink = obj.GetComponent<Linkage>();
					targetPower = obj.GetComponent<PowerControl>();
					int outputIndex=outputs.IndexOf(obj); //index of the object in the main objects list, rather than the not full list
					LinkAnimationManager linkManager = outputLinks[outputIndex].GetComponent<LinkAnimationManager>();
					float loss = linkManager.getLoss();

					int maxAmpsIn = targetLink.maxAmpsIn;
					int toCharge=0;
						if(maxAmpsIn < maxTransmissionRate){ //if the link input cannot handle available current, ie, its input cannot handle the full load
							int maxPower = maxAmpsIn * voltage; //power that can be transmitted via a limited input link
							if(!targetPower.isFull(maxPower)){ //if link target can handle the output
								toCharge = maxPower;
							//	Debug.Log ("charging unsaturated input link to " + obj + " with " + (1-loss) *toCharge + "watts");
							}else if(!targetPower.isFull (1)){ //grant a fill-up charge to top off the target
								toCharge = targetPower.getMaxWattHours()-targetPower.getCurrentWattHours();
							//	Debug.Log ("fill-up charging unsaturated input link to " + obj + " with " + (1-loss) * toCharge + "watts");
							}
						} else { // if the link input can handle the full available bandwidth
							int maxPowerCapped = maxTransmissionRate * voltage; //a fully saturated input link
							if(!targetPower.isFull(maxPowerCapped)){ //the link can handle the full amount available
								toCharge = maxPowerCapped;
							//	Debug.Log ("charging saturated input link to " + obj + " with " + (1-loss) *toCharge + "watts");
							}else if(!targetPower.isFull (1)){ //grant a fill-up charge to top off the target
								toCharge = targetPower.getMaxWattHours()-targetPower.getCurrentWattHours();
							//	Debug.Log ("fill-up charging saturated input link to " + obj + " with " + (1-loss) * toCharge + "watts");
							}
						}
							outputWatts[outputIndex]= (toCharge); //set wattage of link
							linkManager.setWatts(toCharge);
							targetPower.charge ((int) ((1-loss) *toCharge) ); //give the link the full transmission rate
							powercontrol.drain (toCharge); //discharge this unit
							remainingBandwidth = remainingBandwidth - toCharge/voltage; //remove the amperage lost from remaining available bandwidth

					currentAmpsOut += toCharge/voltage; //add this charge to overall output
					remainingOutputs--; //one less unit to charge
					if(remainingOutputs > 0){ //if we did not just process the last output
						maxTransmissionRate = remainingBandwidth/remainingOutputs; //prepare to distribute remaining charge evenly over remaining units
					}
				} //end foreach not full object

				unUsedBandwidth = remainingBandwidth;

				}else if(!powercontrol.isEmpty(1)){ //not enough power for a full transmit, trickle output
					maxTransmissionRate = powercontrol.getCurrentWattHours()/voltage/notFullOutputs.Count;

					int remainingBandwidth = powercontrol.getCurrentWattHours()/voltage; //bandwidth remaining for remaining outputs (amps)
					int remainingOutputs = notFullOutputs.Count; //outputs remaining to satisfy
					int unUsedBandwidth; //current capacity still available after all outputs are satisfied
					
					foreach( GameObject obj in notFullOutputs){
						targetLink = obj.GetComponent<Linkage>();
						targetPower = obj.GetComponent<PowerControl>();
						int outputIndex=outputs.IndexOf(obj); //index of the object in the main objects list, rather than the not full list
						LinkAnimationManager linkManager = outputLinks[outputIndex].GetComponent<LinkAnimationManager>();
						float loss = linkManager.getLoss();
						
						int maxAmpsIn = targetLink.maxAmpsIn;
						int toCharge=0;

						if(maxAmpsIn < maxTransmissionRate){ //if the link input cannot handle available current, ie, its input cannot handle the full load
							int maxPower = maxAmpsIn * voltage; //power that can be transmitted via a limited input link
							if(!targetPower.isFull(maxPower)){ //if link target can handle the output
								toCharge = maxPower;
								//	Debug.Log ("charging unsaturated input link to " + obj + " with " + (1-loss) *toCharge + "watts");
							}else if(!targetPower.isFull (1)){ //grant a fill-up charge to top off the target
								toCharge = targetPower.getMaxWattHours()-targetPower.getCurrentWattHours();
								//	Debug.Log ("fill-up charging unsaturated input link to " + obj + " with " + (1-loss) * toCharge + "watts");
							}
						} else { // if the link input can handle the full available bandwidth
							int maxPowerCapped = maxTransmissionRate * voltage; //a fully saturated input link
							if(!targetPower.isFull(maxPowerCapped)){ //the link can handle the full amount available
								toCharge = maxPowerCapped;
								//	Debug.Log ("charging saturated input link to " + obj + " with " + (1-loss) *toCharge + "watts");
							}else if(!targetPower.isFull (1)){ //grant a fill-up charge to top off the target
								toCharge = targetPower.getMaxWattHours()-targetPower.getCurrentWattHours();
								//	Debug.Log ("fill-up charging saturated input link to " + obj + " with " + (1-loss) * toCharge + "watts");
							}
						}
						
						outputWatts[outputIndex]= (toCharge); //set wattage of link
						linkManager.setWatts(toCharge);
						targetPower.charge ((int) ((1-loss) *toCharge) ); //give the link the full transmission rate
						powercontrol.drain (toCharge); //discharge this unit
						remainingBandwidth = remainingBandwidth - toCharge/voltage; //remove the amperage lost from remaining available bandwidth

						currentAmpsOut += toCharge/voltage; //add this charge to overall output
						remainingOutputs--; //one less unit to charge
						if(remainingOutputs > 0){ //if we did not just process the last output
							maxTransmissionRate = remainingBandwidth/remainingOutputs; //prepare to distribute remaining charge evenly over remaining units
						}
					} //end foreach not full object
					
					unUsedBandwidth = remainingBandwidth;

				}
				} //end if not zero outputs

	}} //end while true, IEnumerator

	//sorts links by input amperage accepted at target. Always do this after adding an output target.
	private void sortLinks(){
		outputs.Sort(delegate(GameObject a, GameObject b) {
	//		Debug.Log(a.transform.root.GetComponent<Linkage>().maxAmpsIn + " vs " + b.transform.root.GetComponent<Linkage>().maxAmpsIn);
			return (a.GetComponent<Linkage>().maxAmpsIn).CompareTo(b.GetComponent<Linkage>().maxAmpsIn);
		});

		outputLinks.Sort(delegate(GameObject a, GameObject b) {
			return (a.GetComponent<LinkAnimationManager>().getTarget().GetComponent<Linkage>().maxAmpsIn).CompareTo(
				b.GetComponent<LinkAnimationManager>().getTarget().GetComponent<Linkage>().maxAmpsIn);
		});
	}

	/// <summary>
	/// NOT IMPLEMENTED Returns the number of amps delivered by the input link at the given index in the input link list
	/// Returns -1 if the link at index is not valid, for example if the input array is too short
	/// </summary>
	/// <returns>The amps at input index.</returns>
	/// <param name="index">Index.</param>
	public int getAmpsAtInputIndex(int index){
		if (inputs.Count > index) {
			if (inputs [index] != null) {
				//would have to iterate through outputs at link object side, which is quite inefficient
			}
		}
		return -1;
	}

	/// <summary>
	/// Force the generation of a new output link with the target.
	/// Not reccomended in most instances
	/// </summary>
	/// <param name="target">Target.</param>
	public void forceOutLink(GameObject target){
		outLink (target);
	}

	//Adds an object to output linkage
	private void outLink(GameObject target){
		//calculate distance between links in xz coordinations
		float euclidDistance = Vector3.Distance (target.transform.position, transform.position);
			//Mathf.Sqrt( Mathf.Pow((target.transform.position.x - gameObject.transform.position.x),2f)
			//+ Mathf.Pow((target.transform.position.z - gameObject.transform.position.z),2f) );
		gizmoRange = euclidDistance;

		if (euclidDistance < linkRange && gameObject.GetComponent<OrbitalDrop>().isLanded() ) {
			if (outputs.Count < maxOutLinks) {
				if(target.GetComponent<Linkage>().getInLinks() < target.GetComponent<Linkage>().getMaxInLinks()){
				//add to out linkages if not already in links
				if (!outputs.Contains (target)) {
					outputs.Add (target);
					outputWatts.Add (0);
					target.GetComponent<Linkage> ().inLink (gameObject);
					generateLinkAnimation (target, euclidDistance);
					sortLinks ();

					//clear the watt output list
					for (int i=0; i<outputWatts.Count; i++) {
						outputLinks [i].GetComponent<LinkAnimationManager> ().setWatts (0);
						outputWatts [i] = 0;
					}
				} else { //already linked
					// TODO maybe make some error sound (ouput already added)
				}
				}else{ //maxed in links
					//TODO max in link error
				}
			}else{ //maxed out links
				//TODO max out links error
			}
			} else { //out of range
				//TODO add out of range error
			}
	}

	/// <summary>
	/// Destroy a target link and all affiliated components
	/// </summary>
	/// <param name="target">Target.</param>
	private void breakLink(GameObject target){

		if(outputs.Contains(target)){
		//the index of the item in the outputs array also determines location in outputlinks and outputwatts
		int objIndex = outputs.IndexOf (target);
		outputs.Remove(target);

		outputLinks [objIndex].GetComponent<LinkAnimationManager> ().destroySelf ();
		outputLinks.RemoveAt (objIndex);
		outputWatts.RemoveAt (objIndex);
		target.GetComponent<Linkage> ().unInLink (gameObject);
		}
	}

	/// <summary>
	/// Force the breaking of an existing link
	/// Not recommended in most situations
	/// </summary>
	/// <param name="target">Target.</param>
	public void forceBreakLink(GameObject target){
		breakLink (target);
	}

	/// <summary>
	/// Instantiates link animation objects and populates the LinkAnimationManager of the created object
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="distance">Distance.</param>
	private void generateLinkAnimation(GameObject target, float distance){

		//Used to set the "wire" object between the two link objects
		GameObject outAnimation = Instantiate (lowLinkObject, transform.position, transform.rotation);
		outAnimation.name = "WireLink_" + gameObject.name + "_" + target.name;

			Vector3 midpoint = Vector3.Lerp(target.transform.position, transform.position, .5f);
		//	outAnimation.transform.rotation = Quaternion.LookRotation(-transform.position + target.transform.position);
			outAnimation.transform.LookAt(target.transform);
			outAnimation.transform.position = midpoint;
		//sizes the z scale of the object to the Euclidean distance between the two units
			outAnimation.transform.localScale =
				new Vector3(.2f, .2f, Vector3.Distance(target.transform.position, transform.position));
		
			outAnimation.transform.parent=transform; //assign parent status last so that scaling and repositions can be done globally
		outAnimation.transform.LookAt(target.transform);

		//Used to generate the particle emitter parent object
		GameObject lowLink = Instantiate (lowLinkModel, transform.position, transform.rotation, transform);

		lowLink.name = "Link_" + gameObject.name + "_" + target.name;

		LinkAnimationManager linkAnimator = lowLink.GetComponent<LinkAnimationManager> ();
		linkAnimator.setWire (outAnimation);
		linkAnimator.setOrigin(gameObject);
		linkAnimator.setTarget(target);
		linkAnimator.setDistance (distance);
		linkAnimator.setMaxDistance (linkRange);

		lowLink.transform.rotation = Quaternion.LookRotation(-transform.position + target.transform.position);
		//!Endpoint must be the first (or only) child of the particle system! (could fix by looping through all children, but MUH EFFICIENCY)
		//this sets the endpoint of the particle effect
		lowLink.transform.GetChild(0).transform.position=target.transform.position;
		outputLinks.Add (lowLink);
	}

	//Draws a sphere in the editor for visualization
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (gameObject.transform.position, gizmoRange);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere (gameObject.transform.position, linkRange);
	}

	//establishes incoming connection to input linkage
	public void inLink(GameObject origin){
		if (!inputs.Contains (origin)) {
			inputs.Add (origin);
		} else { //already linked
		}
	}

	//breaks incoming connection to input linkage
	public void unInLink(GameObject origin){
		if (inputs.Contains (origin)) {
			inputs.Remove (origin);
		} else {
			Debug.Log ("failed to purge link to : " + origin);
		}
	}

	public void issueCommand(int status, GameObject obj){
		if (obj != gameObject) { //ignore anything targetting self
			if (status == 1) {
				outLink (obj);
			} else if (status == 2) {
				breakLink (obj);
			}
		}
	}

	public int getMaxOut(){ return maxAmpsOut; }
	public int getMaxIn(){ return maxAmpsIn; }
	public int getCurrentAmpsOut(){ return currentAmpsOut; }
	public int getOutLinks(){ return (outputs.Count + pairings.Count); }
	public int getInLinks(){ return (inputs.Count + pairings.Count); }
	public int getMaxInLinks(){return maxInLinks;}
	public int getMaxOutLinks(){return maxOutLinks;}

	//Destroy all links prior to death
	public void OnDestroy(){
		//TODO THIS IS **RARELY** NOT BREAKING THE INPUT LINK OF LINKED OBJECT CORRECTLY
		/* not neccessary, the object is going to die anyways
		foreach (GameObject obj in outputs) {
			//the index of the item in the outputs array also determines location in outputlinks and outputwatts
			int objIndex = outputs.IndexOf (obj);
			outputs.RemoveAt(objIndex);
			
			outputLinks [objIndex].GetComponent<LinkAnimationManager> ().destroySelf ();
			outputLinks.RemoveAt (objIndex);
			outputWatts.RemoveAt (objIndex);
			obj.GetComponent<Linkage> ().unInLink (gameObject);
		}
		*/
		List<GameObject> toBreak = new List<GameObject>(); //avoids enumeration errors due to changing size of input list
		foreach (GameObject obj in inputs) {
			toBreak.Add(obj);
		}

		foreach (GameObject obj in toBreak) {
			if(obj!=null && gameObject != null){
				obj.GetComponent<Linkage> ().breakLink (gameObject); //break all input links by informing the respective outputs
			}
		}

	}

	/// <summary>
	/// Sanity check coroutine periodically checks for dead links
	/// </summary>
	IEnumerator purgeDeadLinks(){
		while (true) {
			yield return new WaitForSeconds(10f); //not time sensitive, can be long interval

			for(int i=0; i<outputs.Count -1; i++){
				if(outputs[i]==null){
					outputWatts.RemoveAt(i);
				}
			}
			inputs.RemoveAll(GameObject => GameObject == null);
			outputs.RemoveAll(GameObject => GameObject == null);
			outputLinks.RemoveAll(GameObject => GameObject == null);
		}
	}

	IEnumerator updateLinkPositions(float interval){
		while (true) {
			yield return new WaitForSeconds(interval); //this is intensive operation, possibly consider lowering this

			List<GameObject> toBreak = new List<GameObject>();
			foreach(GameObject obj in outputLinks){

				//Grab the particle system and adjust its position
				GameObject lowLink = obj;
				LinkAnimationManager linkAnimator = lowLink.GetComponent<LinkAnimationManager> ();
				GameObject target = linkAnimator.getTarget();
				float euclidDistance = Vector3.Distance (target.transform.position, transform.position);

				bool updateAnimation=false;
				if (euclidDistance > linkRange){ //if link outranged
					toBreak.Add(target); //to avoid enumeration errors, store a list of outranged links
				}else{ //link still in range
					updateAnimation=true;
				}

				if(updateAnimation){
					linkAnimator.setDistance (euclidDistance);

					
					lowLink.transform.rotation = Quaternion.LookRotation(-transform.position + target.transform.position);
					//!Endpoint must be the first (or only) child of the particle system! (could fix by looping through all children, but MUH EFFICIENCY)
					//this sets the endpoint of the particle effect
					lowLink.transform.GetChild(0).transform.position=target.transform.position;


					Vector3 midpoint = Vector3.Lerp(target.transform.position, transform.position, .5f);
					GameObject wire = linkAnimator.getWire();
					wire.transform.parent =null;
					//	outAnimation.transform.rotation = Quaternion.LookRotation(-transform.position + target.transform.position);
					wire.transform.LookAt(target.transform);
					wire.transform.position = midpoint;
					//sizes the z scale of the object to the Euclidean distance between the two units
					wire.transform.localScale =
						new Vector3(.2f, .2f, Vector3.Distance(target.transform.position, transform.position));
					
					wire.transform.parent=transform; //assign parent status last so that scaling and repositions can be done globally
					wire.transform.LookAt(target.transform);
				}
			}
			//break links that were found out of range
			foreach (GameObject obj in toBreak) {
				if(obj!=null && gameObject != null){
					breakLink (obj);
				}
			}

		}
	}
}