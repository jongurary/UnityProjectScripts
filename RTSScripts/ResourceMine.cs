using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMine : MonoBehaviour {

	private ResourceNode metalNode;
	private PowerControl pow;
	private ResourceControl res;

	public int sourcePerSecond; //source resource pulled from the ground each tick
	public int outputPerSource; //output resource generated per source resource
	public int powerPerSecond; //power drained per second of resource production

	public List<GameObject> deliveryTrucks = new List<GameObject>(); //all delivery trucks owned by this facility
	public GameObject truckObject; //spawned delivery truck object
	public GameObject truckSpawn; //truck spawn location

	public GameObject smokeStack; //the pillar of beautiful carbon dust
	public GameObject laserDrill; //the pillar of light piercing the earth
	public GameObject drillAnimator; //drill's animation control object
	private bool building; //is the truck being built?

	private int outIndex; //output index in the resource manager
	

	void OnCollisionEnter(Collision collision) {
		if ( collision.transform.root.CompareTag ("ResourceDeposit") ) {
			metalNode = collision.transform.gameObject.GetComponent<ResourceNode>();
			int index = res.setNextEmptyInputType ( metalNode.getType() );
	//		res.setInputType( metalNode.getType() );
			string type = res.getInputType(index);
			switch(type){
			case "Iron":
				outIndex = res.setNextEmptyOutputType("Iron Ore");
			//	res.setOutputType("Iron Ore");
				break;
			case "Exotic Metal":
				outIndex = res.setNextEmptyOutputType("Exotic Ore");
				//	res.setOutputType("Iron Ore");
				break;
			case "Uranium":
				outIndex = res.setNextEmptyOutputType("Uranium Ore");
				break;
			case "Hydrocarbon":
				outIndex = res.setNextEmptyOutputType("Fuel");
				break;
			default:
				outIndex = res.setNextEmptyOutputType("None");
			//	res.setOutputType("None");
				break;
			}
			StartCoroutine(Extract());
		}
	}

	void Start () {
		pow = GetComponent<PowerControl> ();
		res = GetComponent<ResourceControl> ();
		disableAnimations ();
		building = false;
	}

	void Update () {
	}

	public void issueCommand(int status, GameObject obj){
		if (obj != gameObject) { //ignore anything targeting self
			if (status == 1) {

				StartCoroutine(buildTruck(obj, 0));
			} else if (status == 2) {
				StartCoroutine(buildTruck(obj, 1));
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

	//Note: only activiated after successful collision with a resource node
	IEnumerator Extract() {
		while (true) {
			yield return new WaitForSeconds (1f);

			//TODO for now, mines only process one resource, this may change in the future
			if( !metalNode.isEmpty(sourcePerSecond) 
			   && ((sourcePerSecond * outputPerSource) + res.getCurrentOutputResource(outIndex) <= res.getMaxOutputResource(outIndex) ) 
			   && !pow.isEmpty(powerPerSecond) ){
				pow.drain(powerPerSecond);
				metalNode.consume(sourcePerSecond);
		//		Debug.Log ("Filled " + res.getOutputType (outIndex) + " at index " + outIndex);
				res.fill( (outputPerSource * sourcePerSecond), outIndex );
				enableAnimations();
			}else{
				disableAnimations();
			}

		}
	}

	/// <summary>
	/// Turn on smoke, laser, and drill animations
	/// </summary>
	private void enableAnimations(){
		var smokeanim = smokeStack.GetComponent<ParticleSystem> ().emission;
		smokeanim.rateOverTime = 10;
		Animator anim = drillAnimator.GetComponent<Animator> ();
	//	anim.Play ("DrillMove");
		anim.SetTrigger ("canStart");
		var drillanim = laserDrill.GetComponent<ParticleSystem> ().emission;
		drillanim.rateOverTime = 3;
		laserDrill.GetComponentInChildren<Light> ().enabled = true;

	
	}

	/// <summary>
	/// Turn off smoke, laser, and drill animations
	/// </summary>
	private void disableAnimations(){
		var smokeanim = smokeStack.GetComponent<ParticleSystem> ().emission;
		smokeanim.rateOverTime = 0;
		Animator anim = drillAnimator.GetComponent<Animator> ();
		anim.SetTrigger ("canExit");
		var drillanim = laserDrill.GetComponent<ParticleSystem> ().emission;
		drillanim.rateOverTime = 0;
		laserDrill.GetComponentInChildren<Light> ().enabled = false;
	}

	public ResourceNode getNode(){ return metalNode; }

}
