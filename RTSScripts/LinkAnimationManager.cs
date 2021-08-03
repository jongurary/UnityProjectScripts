using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkAnimationManager : MonoBehaviour {

	//TODO change link renderers to pink when selected
	private GameObject wire; //the wire object that corresponds to this link
	private GameObject origin;
	private GameObject target;
	private int wattage; //current bandwidth of this link
	private ParticleSystem flowPart;
	private float distance; //length of this link
	private float maxDistance; //the link range of the intiating unit, used for distance calculations
	private float loss; //loss, expressed as a percentage, over the connection

	public bool usesEfficiency; //does this unit lose transmission power over distance
	public float inefficiencyFactor; //(0 -1) multiplies by inefficiency to mitigate it (lower is better)

	private float maxInefficiency = Constants.MAX_INEFFICIENCY;

	void Start () {
		flowPart = GetComponent<ParticleSystem> ();
		StartCoroutine(UpdateAnimations());

		//calculate the loss of this link based on distance
		if(usesEfficiency){
			loss = maxInefficiency * (distance/maxDistance)  * inefficiencyFactor;
		}else{
			loss=0;
		}
	}

	IEnumerator UpdateAnimations() {
		while (true) {
			var flowEmitter = flowPart.emission;
			float particles = Mathf.Clamp(Constants.MAX_LINKANIMATION_PARTICLES * ((float) wattage / (float) Constants.MAX_LINKANIMATION_ANIMATEDWATTS),
			            0f, Constants.MAX_LINKANIMATION_PARTICLES);
			flowEmitter.rateOverTime = (int) particles;
			yield return new WaitForSeconds(.7f);
		}
	}

	/// <summary>
	/// Sets the wire component that corresponds to this link animation
	/// </summary>
	/// <param name="wir">Wir.</param>
	public void setWire(GameObject wir){ wire = wir; }

	public GameObject getWire(){ return wire; }

	public void setOrigin(GameObject org){ origin = org; }

	public void setTarget(GameObject tar){ target = tar; }

	public GameObject getOrigin(){ return origin; }
	
	public GameObject getTarget(){ return target; }

	public void setWatts(int wat){ wattage = wat; }
	
	public int getWatts(){ return wattage; }

	public float getDistance(){ return distance; }

	public void setDistance(float dis){ distance = dis; }

	public float getMaxDistance(){ return maxDistance; }
	
	public void setMaxDistance(float dis){ maxDistance = dis; }

	public float getLoss(){ return loss; }

	/// <summary>
	/// Destroys the particle animation and wire components
	/// </summary>
	public void destroySelf() { Destroy (wire, .1f); Destroy(gameObject, .1f); }

}
