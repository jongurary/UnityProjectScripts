using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceElevator : MonoBehaviour {

	[Tooltip("Time per resource transfer.")]
	public float tickTime;
	[Tooltip("Power consumed by the space elevator per tick during elevator operation.")]
	public int wattsPerTick; 
	public int steelPerTick;
	public int uraniumPerTick;
	public int exoticsPerTick;
	public int fuelPerTick;

	[Tooltip("Power laser beamed into space.")]
	public int wattsPerTickLaser;

	[Tooltip("Is a space elevator constructed? True if constructed")]
	public bool hasSpaceElevator;
	public bool hasSpaceLaser;
	public bool spaceElevatorPaused;
	public bool spaceLaserPaused;
	public GameObject spaceElevator; //space elevator game object
	public GameObject spaceLift; //space lift attached to elevator
	public GameObject spaceLaser; //space laser sending energy into space
	
	void Start () {
		hasSpaceElevator = false;
		spaceElevatorPaused = false;
		hasSpaceLaser = false;
		spaceLaserPaused = false;
	}

	void Update () {
		
	}


	/// <summary>
	/// Builds a new space elevator, if one doesn't already exist
	/// </summary>
	/// <returns><c>true</c>, if space elevator was built, <c>false</c> otherwise.</returns>
	public bool buildSpaceElevator(){
		if (!hasSpaceElevator && !hasSpaceLaser) {
			spaceElevator.SetActive(true);
			spaceLift.SetActive(true);
			StartCoroutine(raiseElevator());
			StartCoroutine (runElevator ());
			hasSpaceElevator=true;
			return true;
		} else {
			return false; //couldn't build an elevator because one already exists
		}
	}


	public bool buildSpaceLaser(){
		if (!hasSpaceElevator && !hasSpaceLaser) {
			spaceLaser.SetActive(true);
			StartCoroutine( runLaser() );
			hasSpaceLaser=true;
			return true;
		} else {
			return false; //couldn't build an elevator because one already exists
		}
	}

	IEnumerator raiseElevator(){
		while (spaceElevator.transform.localPosition.y < 23 ) {
			yield return new WaitForSeconds(.02f);
			spaceElevator.transform.localPosition = 
				new Vector3(spaceElevator.transform.localPosition.x, spaceElevator.transform.localPosition.y + .1f, spaceElevator.transform.localPosition.z);

			if (spaceLift.transform.localPosition.y < 0.75) {
				spaceLift.transform.localPosition = 
					new Vector3(spaceLift.transform.localPosition.x, spaceLift.transform.localPosition.y + .02f, spaceLift.transform.localPosition.z);
			}
		}
		yield break;
	}

	//send resources into space
	IEnumerator runLaser(){
		ResourceManager resManager = GameObject.FindGameObjectWithTag ("ResourceManager").GetComponent<ResourceManager> ();
		PowerControl pow = GetComponent<PowerControl> ();

		while (true) {
			yield return new WaitForSeconds(tickTime);
			if( spaceLaser && !spaceLaserPaused ){
				if( !pow.isEmpty(wattsPerTickLaser) ){
					pow.drain(wattsPerTickLaser);
					resManager.sendEnergytoOrbit(wattsPerTickLaser);
					var emission = spaceLaser.GetComponent<ParticleSystem>().emission;
					emission.rateOverTime = 1;
				}else{
					var emission = spaceLaser.GetComponent<ParticleSystem>().emission;
					emission.rateOverTime = 0;
				}
			}
		}
	}

	//sends resources up and down the elevator
	IEnumerator runElevator(){
		ResourceManager resManager = GameObject.FindGameObjectWithTag ("ResourceManager").GetComponent<ResourceManager> ();
		ResourceControl resControl = GetComponent<ResourceControl> ();
		PowerControl pow = GetComponent<PowerControl> ();
		int toSend, maxToSend;

		while (true) {
			yield return new WaitForSeconds(tickTime);
			if( hasSpaceElevator && !spaceElevatorPaused ){
			if( !pow.isEmpty(wattsPerTick) ){
				for (int k=0; k<resControl.getInputCount(); k++) {
					if (resControl.getInputType (k) == "Steel") {
						maxToSend = steelPerTick;
					} else if (resControl.getInputType (k) == "Enriched Uranium") {
						maxToSend = uraniumPerTick;
					} else if (resControl.getInputType (k) == "Exotics") {
						maxToSend = exoticsPerTick;
					}else if (resControl.getInputType (k) == "Fuel") {
						maxToSend = fuelPerTick;
					}else {
						maxToSend = 10;
					}

					if (resControl.getCurrentInputResource (k) > maxToSend) {
						toSend = maxToSend;
					} else {
						toSend = resControl.getCurrentInputResource (k);
					}
					StartCoroutine (sendLiftUp ());
					pow.drain(wattsPerTick);
					resControl.drainInput (toSend, k);
					resManager.sendResourceWithTag (resControl.getInputType (k), toSend);
				}
			}
			} //end has elevator
		}
	}

	//sends the lift animation up and down
	IEnumerator sendLiftUp(){
	//	Debug.Log ("lifting");
		while (spaceLift.transform.localPosition.y < 35f) {
			yield return new WaitForSeconds(.01f);
			spaceLift.transform.localPosition = 
				new Vector3(spaceLift.transform.localPosition.x, spaceLift.transform.localPosition.y + .15f, spaceLift.transform.localPosition.z);
		}
		while (spaceLift.transform.localPosition.y > 0.8) {
			yield return new WaitForSeconds(.02f);
			spaceLift.transform.localPosition = 
				new Vector3(spaceLift.transform.localPosition.x, spaceLift.transform.localPosition.y - .1f, spaceLift.transform.localPosition.z);
		}
		yield break;
	}

}
