using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalDrop : MonoBehaviour {

	public GameObject fireParticle;
	public GameObject dustParticle;
	public GameObject smokeParticle;
	public GameObject impactParticle;

	private bool landed;

	void OnCollisionEnter(Collision collision) {
		
		if ( collision.transform.root.CompareTag ("Terrian") || collision.transform.root.CompareTag ("ResourceDeposit") ) {
			if(!landed){
				StartCoroutine(DisableFlame());
			}
			landed=true;
			//TODO terrian collisions unrelated to landing should cause massive damage
		}

	}

	IEnumerator DisableFlame() {
		fireParticle.SetActive (false);

		ParticleSystem impact = impactParticle.GetComponent<ParticleSystem> ();
		impact.Emit (20);
		ParticleSystem dust = dustParticle.GetComponent<ParticleSystem> ();
		dust.Emit (30);
		ParticleSystem smoke = smokeParticle.GetComponent<ParticleSystem> ();
		smoke.Emit (10);

		int initialDustRate = (int) dust.emission.rateOverTime.constant / 10;
		int initialSmokeRate = (int) smoke.emission.rateOverTime.constant / 10;

		var dustEmitter = dust.emission;
		var smokeEmitter = smoke.emission;

		for (int i=0; i<10; i++) {
			yield return new WaitForSeconds(.15f);
			if(dust.emission.rateOverTime.constant>0){
				dustEmitter.rateOverTime = dust.emission.rateOverTime.constant - initialDustRate; }

			if(smoke.emission.rateOverTime.constant>0){
				smokeEmitter.rateOverTime = smoke.emission.rateOverTime.constant - initialSmokeRate; }
		}
		yield return new WaitForSeconds(2f);
		GetComponent<Rigidbody>().isKinematic = true;
		yield return null;
	}

	//triggers the animation disable after a certain time in the event of a failed terrian collision
	IEnumerator EmergencyDisable() {
		yield return new WaitForSeconds (10f);
		if (!landed) {
			StartCoroutine(DisableFlame());
		}
		yield return null;
	}
	
	void Start () {
		landed = false;
		StartCoroutine(EmergencyDisable());
	}

	public bool isLanded(){ return landed; }

}
