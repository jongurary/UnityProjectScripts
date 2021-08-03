using UnityEngine;
using System.Collections;

/// <summary>
/// Handles attacking for target-facing turrets with continuous energy weapons that ignore accuracy
/// These weapons do not create a "projectile" and deal damage directly instead
/// /// </summary>
public class UnitAttackLaser : MonoBehaviour {

	//TODO Replace with "Unit Attack Area" phyics calculation style as it is more efficient

	public float attackSpeed; //lower is faster
	public float turretSpeed; //high is faster (don't judge the consistency)
	public GameObject laserAnimation; //the laser stream animation object
	private GameObject laserStream; //holds the current laser stream animation
	public GameObject fireArea; //where the animation comes out of
	public float attackRange;
	public string attackType1; //tag the unit can attack, for example "AirUnit"
	public string attackType2;
	public GameObject turret;
	public GameObject attackParticle; //a particle system that's created on projectile spawn
	public float projectileSpeed;
	public int energyCost; //energy cost per attack, if the unit consumes energy
	public int ammoCost; //ammo cost per attack, if the unit consumes ammo
	public int damage; //damage dealt by this unit, since it attacks directly and has no projectile
	public Renderer lowPower; //low power indication symbol
	public Renderer lowAmmo; //low ammo indication symbol
	private bool hasLowPower; //is the object unable to fire due to low power?
	private bool hasLowAmmo; //is the object unable to fire due to low ammo?
	
	private bool hasTarget; //does the unit have a living target?
	private bool turretAligned; //is the turret lined up with the target?
	private GameObject target;
	private float rangeSpeed =.25f; //how often we scan for target
	private bool initiateAttack; //this is true if the turret is aligned with target
	private GameObject spawnedProjectile;
	private PulseParticle Pulser; //controls particle systems around projectile spawn
	private int owner; //owning player id, inherited from unitLife

	// Use this for initialization
	void Start () {
		StartCoroutine(Attack(attackSpeed));
		StartCoroutine(TestRange(rangeSpeed));
		StartCoroutine (blinkLowPower());
		StartCoroutine (blinkLowAmmo());
		hasTarget = false;
		if (attackParticle != null) {
			Pulser = attackParticle.GetComponent<PulseParticle> ();
		}
		owner = transform.root.gameObject.GetComponent<UnitLife> ().getOwner ();
	}

	void Update () {

		//Note, this turrent position updator is responsible for most of the overhead in this script

		//rotate the turret to face target
		if (hasTarget && turret!=null && target!=null && !hasLowPower) {
			Vector3 targetPostition = new Vector3 (this.target.transform.position.x, 
		                                      this.target.transform.position.y, 
		                                      this.target.transform.position.z);
			Quaternion lookTo = Quaternion.LookRotation (turret.transform.position - targetPostition);
			float rotSpeed= Time.deltaTime * 8 * (turretSpeed / 1);

			if(Quaternion.Angle(turret.transform.rotation, lookTo)>2f){
				if(Quaternion.Angle(turret.transform.rotation, lookTo)>4f){
					//if the angle difference becomes too great, stop shooting
					initiateAttack=false;
				}
				turret.transform.rotation = Quaternion.Slerp (turret.transform.rotation, lookTo, rotSpeed);
			}else{ //turret is on target and ready to begin shooting.
				//TODO Check if 2 degree causes any accuracy issues, can reduce overhead
				initiateAttack=true;
			}

			//aligns laser beam with target if the target is ready to strike
			if(laserStream!=null && initiateAttack){
				//using Slerp for the beam makes it less jagged when swapping targets and reduces lingering laser effect from previous target slightly
				laserStream.transform.rotation = Quaternion.Slerp (
					laserStream.transform.rotation, Quaternion.LookRotation(-fireArea.transform.position + target.transform.position), rotSpeed * 5 ) ;

				//!Endpoint must be the first (or only) child of the particle system! (could fix by looping through all children, but MUH EFFICIENCY)
				//this sets the endpoint of the particle effect
				laserStream.transform.GetChild(0).transform.position=target.transform.position;
			}
		
		}
	}

	//Attack if a target is detected in range and all conditions are met
	IEnumerator Attack(float waitTime) {
		bool canAttackEnergy = false; //can the unit attack for energy purposes
		bool canAttackAmmo = false; //can the unit attack for ammo purposes
		PowerControl pow = GetComponent<PowerControl>();
		AmmoControl amo = GetComponent<AmmoControl>();

		while (true) {
			yield return new WaitForSeconds (waitTime);

			//Objects that use energy must first check their energy resource
			if(pow==null){
				canAttackEnergy=true;
			}else{
				if(!pow.isEmpty(energyCost)){
					canAttackEnergy=true;
					hasLowPower=false;
				}else{
					canAttackEnergy=false;
					hasLowPower=true;
				}
			}

			//Objects that use ammo must check their ammo resource
			if(amo==null){
				canAttackAmmo=true;
			}else{
				if(!amo.isEmpty(ammoCost)){
					canAttackAmmo=true;
					hasLowAmmo=false;
				}else{
					canAttackAmmo=false;
					hasLowAmmo=true;
				}
			}
			
			//if all attack criteria are met
			if(hasTarget && initiateAttack && target!=null && canAttackEnergy && canAttackAmmo)
			{
				//first drain power and ammo, if possible
				if(pow!=null){
					pow.drain (energyCost);
				}
				if(amo!=null){
					amo.drain (ammoCost);
				}
				
				//TODO minimum distance check

				//Pulses the barrel flares
					if(Pulser!=null){
						Pulser.pulse1(10);
						Pulser.pulse3(1);
						Pulser.engage2(true);
					}

				if( target.GetComponent<UnitLife>()!=null ){ //sanity check for objects with malfunctioning life system
					target.GetComponent<UnitLife>().Damage(damage);
				}else if (target.transform.root.gameObject.GetComponent<UnitLife>()!=null){
					target.transform.root.gameObject.GetComponent<UnitLife>().Damage(damage);
				}

				//Used to generate the particle emitter parent object
				if(laserStream==null){
					laserStream = Instantiate (laserAnimation, fireArea.transform.position, transform.rotation, fireArea.transform);
				}
				var flowEmitter = laserStream.GetComponent<ParticleSystem>().emission;
				flowEmitter.rateOverTime=8;

				laserStream.name = "Laser_" + gameObject.name + "_" + target.name;
				//TODO damage animation at endpoint?

			}else{
				//disable the laser and barrel flare effects
				if(laserStream!=null){
					var flowEmitter = laserStream.GetComponent<ParticleSystem>().emission;
					flowEmitter.rateOverTime=0;
				}
				if(Pulser!=null){
					Pulser.engage2(false);
				}
			}

		}} //end while true, end method

	/// <summary>
	/// Overrides the target with a provided obj, if it's in range. Returns true if in range, false otherwise
	/// </summary>
	/// <returns><c>true</c>, if target was overridden, <c>false</c> otherwise.</returns>
	/// <param name="obj">Object.</param>
	public bool overrideTarget(GameObject obj){
		Collider[] hitColliders = Physics.OverlapSphere (transform.position, attackRange);
		
		//TODO if performance is impacted, cap number of scanned units
		int i = 0;
		while (i < hitColliders.Length) {
			if ((hitColliders [i].gameObject.tag == attackType1 || hitColliders [i].gameObject.tag == attackType2) &&
			    hitColliders [i].gameObject != transform.gameObject) {
				if(hitColliders[i].gameObject==obj){
					target=obj;
					return true;
				}
			}
			i++;
		}
		return false;
	}

	//Periodically check for a target in range, also purges dead targets
	IEnumerator TestRange(float waitTime) {
		float distance;
		float minDistance;
		int closest=0;

		while (true) {
			yield return new WaitForSeconds (waitTime);
			if(target==null){ //this should be enough to purge dead targets
				hasTarget=false;
			}
			//cancels target if out of range
			if(hasTarget){
				distance= (transform.position - target.transform.position).sqrMagnitude;
					if(distance>attackRange*attackRange+5f){ //add a small buffer distance
				//	Debug.Log(distance);
				//	Debug.Log ("Purged Target");
					target=null;
					hasTarget=false;
				}
			}
			//Note, only rescans if no target, meaning performance impact is worsened outside combat
			else if(!hasTarget){
			Collider[] hitColliders = Physics.OverlapSphere (transform.position, attackRange);
				minDistance=float.MaxValue;
				closest = -1;
				target = null;

				//TODO if performance is impacted, cap number of scanned units
				int i = 0;
				while (i < hitColliders.Length) {
					if ((hitColliders [i].gameObject.tag == attackType1 || hitColliders [i].gameObject.tag == attackType2) &&
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
				if(closest>=0){
					target=hitColliders [closest].gameObject;
//					Debug.Log(gameObject.ToString() + " aquired target " + target.ToString());
				}
				if(target!=null){
					hasTarget = true;
				//	Debug.Log ("target scan picked: " + target);
				}

		} //end if !hasTarget
		//	Debug.Log(target);
		} //end while true
	} //end method

	/// <summary>
	/// Blinks the low power indicator on and off, or disables it entirely when not low power
	/// </summary>
	IEnumerator blinkLowPower() {
		//if this is not a power using unit, lowPower will be null, terminate this coroutine.
		if (lowPower == null) {
			yield break;
		}

		while (true) {
			yield return new WaitForSecondsRealtime(1f);
			if(hasLowPower){ //has low power, blink indicator
				lowPower.enabled = !lowPower.enabled;
			}else{ //not low power, disable indicator
				lowPower.enabled = false;
			}
		}
	}

	/// <summary>
	/// Blinks the low ammo indicator on and off, or disables it entirely when not low ammo
	/// </summary>
	IEnumerator blinkLowAmmo() {
		//if this is not a power using unit, lowPower will be null, terminate this coroutine.
		if (lowAmmo == null) {
			yield break;
		}
		
		while (true) {
			yield return new WaitForSecondsRealtime(1f);
			if(hasLowAmmo){ //has low power, blink indicator
				lowAmmo.enabled = !lowAmmo.enabled;
			}else{ //not low power, disable indicator
				lowAmmo.enabled = false;
			}
		}
	}

	//Draws a sphere in the editor for visualization
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
			//Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
			Gizmos.DrawWireSphere (transform.position, attackRange);
	}
}
