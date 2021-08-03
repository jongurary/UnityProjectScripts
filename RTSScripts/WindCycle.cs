using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindCycle : MonoBehaviour {

	public WindZone wind;

	public int maxIntensity;
	public int minIntensity;

	public GameObject dustStorm; //gameobject that holds the dust storm particle system
	[Tooltip("If true, sisables spawning of dust storms. If storm is already present, storm will not end")]
	public bool stormsDisabled;

	private float transitionTimeIncrement = Constants.WIND_UPDATE_INTERVAL; //How often the wind changes speed/direction
	private float dustStormSpawnChance = Constants.DUST_STORM_SPAWN_CHANCE;
	private float dustStormEndChance = Constants.DUST_STORM_END_CHANCE;
	
	
	void Start () {

		if (wind == null) {
			wind = GetComponent<WindZone> ();
		}
		StartCoroutine(ChangeWind());
		
	}

	void Update () {
	

	}

/// <summary>
/// Periodically changes the strength and direction of the wind
/// </summary>
	IEnumerator ChangeWind (){
		bool isDustStorm = false;
		ParticleSystem dustPart = dustStorm.GetComponent<ParticleSystem> ();
		var dustEmitter = dustPart.emission;

		while (true) {
			//randomly generate new wind strength and direction
			yield return new WaitForSeconds (transitionTimeIncrement);
			wind.windMain = (float) Random.Range(minIntensity, maxIntensity);
			transform.rotation = Quaternion.Euler(0f, (float) Random.Range(0, 360), 0f);

			if(!stormsDisabled){
				//randomly generate dust storms, inform all solar panels to close
				float rand = Random.Range(0f,100f);
				if( rand < dustStormSpawnChance && !isDustStorm ){
	//				Debug.Log ("Storm started!!");
					SolarPower[] sols = FindObjectsOfType<SolarPower>();
					foreach( SolarPower sol in sols){
						sol.ForceShut();
					}
					dustEmitter.rateOverTime = 25;
					dustPart.Emit(500);
					isDustStorm = true;
				}else if(isDustStorm){ //randomly end dust storms, allow panels to open
					if( rand < dustStormEndChance ){
	//					Debug.Log ("Storm ended!!");
						SolarPower[] sols = FindObjectsOfType<SolarPower>();
						foreach( SolarPower sol in sols){
							sol.allowOpen();
						}
						dustEmitter.rateOverTime = 0;
						isDustStorm=false;
					}
				}
			}

			//TODO random chance to spawn lightning storms
		}

		yield return null;
	}

}
