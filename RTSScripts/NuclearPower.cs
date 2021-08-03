using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuclearPower : MonoBehaviour {

	public int wattsPerUranium; //watts generated per uranium ore consumed each second
	public int uraniumPerTick;
	public float outputFactor; //scaling factor for power output

	public GameObject[] spawnOnDestroy; //meltdown spawns
	public GameObject meltDownAnimator;
	ParticleSystem.EmissionModule emitter; //emmission component of meltDownAnimator

	private ResourceControl res;
	private PowerControl pow;
	private int meltdownChance =5; //percent chance to meltdown on overheat, from 0-100
	private int meltdownConstantChance = 1; //percent chance to overheat on a power cycle from 0-1000 (divided by 10)
	public bool isMeltingDown; //if true, in meltdown.
	private bool savedMeltDown; //true if a meltdown has just been averted
	
	void Start () {
		if (res == null) { res = GetComponent<ResourceControl>(); }
		if (pow == null) { pow=GetComponent<PowerControl>(); }
		isMeltingDown = false;
		savedMeltDown = false;
		emitter = meltDownAnimator.GetComponent<ParticleSystem>().emission;
		StartCoroutine (updatePower ());
	}

	void Update () {
		
	}

	/// <summary>
	/// Increaes uranium consumption by 1 per second
	/// </summary>
	public void overheatReactor(){
		uraniumPerTick += 1;
		outputFactor += .05f;
		meltdownConstantChance += 1;
		pow.maxWattHours += (int)(uraniumPerTick * wattsPerUranium * 1.5f);
		int rand = Random.Range (0, 100);
		if (rand < meltdownChance) {
			isMeltingDown = true;
			StartCoroutine (meltdown ());
			emitter.rateOverTime = 3;
		}
	}

	/// <summary>
	/// Vents the reactor, stopping a potential meltdown, but purging all power from the reactor
	/// </summary>
	public void ventReactor(){
		if (isMeltingDown) {
			pow.drain(pow.getCurrentWattHours());
			savedMeltDown=true;
			isMeltingDown = false;
			emitter.rateOverTime = 1;
		}
	}

	IEnumerator meltdown(){
		yield return new WaitForSeconds (60f);
		if (isMeltingDown) {
			if (spawnOnDestroy.Length > 0) {
				foreach (GameObject obj in spawnOnDestroy) {
					Instantiate (obj, transform.position, Quaternion.identity);
				}
			}
			Destroy(gameObject);
		}
		yield break;
	}

	/// <summary>
	/// Updates the power and drains uranium.
	/// </summary>
	IEnumerator updatePower(){
		while (true) {
			yield return new WaitForSeconds(1f);
			if(savedMeltDown){
				yield return new WaitForSeconds(10f);
				emitter.rateOverTime = 0;
				savedMeltDown = false;
			}
			pow.setWatts( (int)(wattsPerUranium * uraniumPerTick * outputFactor) ); //update the power generation

			//Note: Enriched uranium should be the first input slot, so this check is not neccessary
			int index = res.getIndexofInputType("Enriched Uranium");
			if( res.getCurrentInputResource(index) > uraniumPerTick //if has uranium
			    && !pow.isFull( (int)(wattsPerUranium * uraniumPerTick * outputFactor) ) ){ //and not full on power
				res.drainInput( uraniumPerTick, index ); //drain uranium

				int rand = Random.Range (0, 1000);
				if (rand < meltdownConstantChance) {
					isMeltingDown = true;
					StartCoroutine (meltdown ());
					emitter.rateOverTime = 3;
				}
			}else{
				pow.setWatts(0);
			}

		}
	}
}
