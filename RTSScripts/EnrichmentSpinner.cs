using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnrichmentSpinner : MonoBehaviour {

	//TODO tie spin to refinery output for centrifuge?
	public GameObject spinner;
	public PowerControl pow;
	private Animator animController;

	private int maxSpeed = 20;
	private float updateSpinIncrement=1f;
	private int minWattagetoEngage =500; //the lowest wattage that will start making the spinner go faster

	void Start () {
		animController = spinner.GetComponent<Animator> ();
		animController.speed = 1;
		pow = GetComponent<PowerControl> ();
		StartCoroutine (updateSpin ());
		
	}

	void Update () {
		
	}

	IEnumerator updateSpin(){
		while (true) {
			yield return new WaitForSeconds(updateSpinIncrement);
			if( pow.getCurrentWattHours() > minWattagetoEngage ) {
			//	Debug.Log ("has power");
				if(animController.speed < maxSpeed){
			//		Debug.Log ("can spin up");
				//	animController.SetFloat("Speed", 2f);
					animController.speed = animController.speed + .5f;
			//		Debug.Log (animController.speed);
				}
			}else{
				if(animController.speed > 1){
					animController.speed = animController.speed - .5f;
				}
			}
		}

	}

	}
