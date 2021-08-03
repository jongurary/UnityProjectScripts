using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//TODO this whole script is shit, rewrite it.
//TODO give individual units autonomy when their main target dies

public class AISpawner : MonoBehaviour {

	public MasterSelector selector; //used to query for allied units
	public int killCount; //How many units have been killed this mission

	[Tooltip("Locations where ground units can be spawned")]
	public GameObject[] groundSpawnLocations;
	public GameObject[] airSpawnLocations;

	[Tooltip("Spawnable units in ascending order of strength (weakest is first)")]
	public GameObject[] spawnables;
	[Tooltip("Spawnable air units in ascending order of strength (weakest is first)")]
	public GameObject[] spawnablesAir;
	[Tooltip("All the units that comprise an attack wave. Once ordered together, these units should be commanded by thier individual AI module.")]
	public List<GameObject> attackWave = new List<GameObject>();

	[Tooltip("Time before the first wave spawns")]
	public float graceDelay;

	public int initialSpawnSeed;
	public int initialDifficultySeed;

	public float difficultyIncreaseTimer; //lower is faster
	public int difficultyIncreaseRate; //higher is more difficulty increased

	private int currentSpawnSeed;
	private int currentDifficultySeed;
	private int maxSpawnSeed = 1000;
	private int maxDifficultySeed = 1000;

	private int tier2SeedVal = 400; //Roll required to spawn the unit at index 1
	private int tier3SeedVal = 800; //Roll required to spawn the unit at index 2

	[Tooltip("Max time between waves, the delay is rolled randomly with this as the maximum time delay")]
	public float regularSpawnInterval; //Default time that passes between enemy spawns
	public int regularSpawnUpperbound = 20; //default upper bound for unit spawning mechanics, can spawn 1.25x this #

	private float randReduction; //randomly generated timer reduction for attack wave spawn, based on difficulty seed

	private float spawnOffset = 25; //units are spawned randomly between 0 and this distance from spawn location along x/z

	[Tooltip("True if air units can be spawned, false if not")]
	public bool canSpawnAir; 

	void Start () {
		currentSpawnSeed = initialSpawnSeed;
		currentDifficultySeed = initialDifficultySeed;
		if (selector == null) {
			GameObject MainCam = Camera.main.gameObject;
			selector=MainCam.GetComponent<MasterSelector>();
		}

		StartCoroutine (spawnAttackWave());
		StartCoroutine (raiseDifficulty ());
	}

	void Update () {
		
	}

	IEnumerator raiseDifficulty(){
		yield return new WaitForSeconds(graceDelay);
		while (true) {
			yield return new WaitForSeconds(difficultyIncreaseTimer);
			yield return new WaitForSeconds(.1f); //to avoid system crashes during testing, remove later
			if(currentSpawnSeed + difficultyIncreaseRate < maxSpawnSeed){
				currentSpawnSeed += difficultyIncreaseRate;
			}
			if(currentDifficultySeed + difficultyIncreaseRate < maxDifficultySeed){
				currentDifficultySeed += difficultyIncreaseRate;
			}
		}
	}

	IEnumerator spawnAttackWave(){
		yield return new WaitForSeconds(graceDelay);
		while (true) {
			randReduction = ( regularSpawnInterval * (UnityEngine.Random.Range(0, currentSpawnSeed) / (float) maxSpawnSeed) );
			yield return new WaitForSeconds(regularSpawnInterval - randReduction);
			yield return new WaitForSeconds(1f); //to avoid system crashes during testing, remove later
			attackWave.Clear (); //purge the last attack wave from memory
		//	Debug.Log ("engaged");

			int spawnIndex = UnityEngine.Random.Range(0, groundSpawnLocations.Length);
			GameObject spawnLocation = groundSpawnLocations[spawnIndex];

			//determines how many units are spawned
			int upperSpawnBound = (int) (regularSpawnUpperbound * ( (float) currentSpawnSeed / (float) maxSpawnSeed + .25f ));
			int unitsToSpawn = UnityEngine.Random.Range (upperSpawnBound/5, upperSpawnBound);
		//	Debug.Log("spawning: " + unitsToSpawn);

			//Picks a starting target for the attack from the list of selectable allied targets
			int attackTargetIndex = UnityEngine.Random.Range(0, selector.getSelectableSize() );
			GameObject attackTarget = selector.getSelectableByIndex(attackTargetIndex);
			//keep rerolling until a valid target is found
			int rerolls=0;
			while(attackTarget ==null || attackTarget.GetComponent<UnitLife>().getOwner() != 1){
				rerolls++;
				if(rerolls > 100 && attackTarget !=null ){ //breaks if taking too long to find a valid target, settles for whatever, as long as it isn't null.
					break;
				}
				attackTargetIndex = UnityEngine.Random.Range(0, selector.getSelectableSize() );
				attackTarget = selector.getSelectableByIndex(attackTargetIndex);
			}

			//Generate the units and give initial orders
			for(int i=0; i<unitsToSpawn; i++){
				GameObject toSpawn;
				int unitRand = UnityEngine.Random.Range(currentDifficultySeed/4, currentDifficultySeed);
				if(unitRand > tier3SeedVal){
					toSpawn = spawnables[2];
				}else if(unitRand > tier2SeedVal){
					toSpawn = spawnables[1];
				}else{
					toSpawn = spawnables[0];
				}
		//		Debug.Log ("generated " + toSpawn.ToString());
				//TODO navmesh spawn issue potentially related to objects spawning too high/low
				GameObject newSpawn = Instantiate(toSpawn, spawnLocation.transform.position + 
                      new Vector3(UnityEngine.Random.Range(-spawnOffset, spawnOffset), 0f, UnityEngine.Random.Range(-spawnOffset, spawnOffset)), Quaternion.identity);
				attackWave.Add(newSpawn);
				UnityEngine.AI.NavMeshAgent navAgent = newSpawn.GetComponent<UnityEngine.AI.NavMeshAgent> ();
				try{
					bool hasDest = navAgent.SetDestination(attackTarget.transform.position);
					if(!hasDest){
						Destroy(gameObject, 1);
					}
				}catch{
					Debug.Log ("Navagent placed off Navmesh, killing self");
					Destroy(gameObject); //a unit placed off the navmesh should kill itself
				}
			}

			if(canSpawnAir){

				//Air waves should spawn occasionally at higher difficulties, but are relatively unlikely early on.
				int airUpperSpawnBound = (int) (regularSpawnUpperbound * ( (float) currentSpawnSeed / (float) maxSpawnSeed + 0f ));
				int airUnitsToSpawn = UnityEngine.Random.Range (0, airUpperSpawnBound);
				
				int airSpawnIndex = UnityEngine.Random.Range(0, airSpawnLocations.Length);
				GameObject airSpawnLocation = airSpawnLocations[airSpawnIndex];

				//Generate the air units and give initial orders
				for(int i=0; i<airUnitsToSpawn; i++){
					GameObject toSpawn;
					int unitRand = UnityEngine.Random.Range(currentDifficultySeed/4, currentDifficultySeed);
					if(unitRand > 0){ //TODO more difficult units
						toSpawn = spawnablesAir[0];
					}else{ 
						toSpawn = spawnablesAir[0];
					}

					//		Debug.Log ("generated " + toSpawn.ToString());
					GameObject newSpawn = Instantiate(toSpawn, airSpawnLocation.transform.position + 
	                      new Vector3(UnityEngine.Random.Range(-spawnOffset, spawnOffset), 8f, UnityEngine.Random.Range(-spawnOffset, spawnOffset)), Quaternion.identity);
					attackWave.Add(newSpawn);
					UnityEngine.AI.NavMeshAgent navAgent = newSpawn.GetComponent<UnityEngine.AI.NavMeshAgent> ();
					try{
						bool hasDest = navAgent.SetDestination(attackTarget.transform.position);
						if(!hasDest){
							Destroy(gameObject, 1);
						}
					}catch(Exception e){
						Debug.Log ("Navagent placed off Navmesh, killing self");
						Destroy(gameObject); //a unit placed off the navmesh should kill itself
					}
				}
			}

		}
	}
}
