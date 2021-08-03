using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarPower : MonoBehaviour {

	public float outputFactor; //scaling factor for power output
	public GameObject sun;
	public Animator anim; //animator that open/closes the panels
	private bool isOpen;

	public int baseOutput; //watt output at peak solar power
	private DayNightCycle daynight;
	private PowerControl pow;
	private float checkIncrement = 1f; //how often to scan for time of day
	private int astralZenith = 90; //peak sunlight
	private float powerFactor; //production factor based on time of day
	
	void Start () {
		if (sun == null) {
			sun=GameObject.Find("Sun");
		}
		StartCoroutine(ChangeOutput());
		daynight = sun.GetComponent<DayNightCycle> ();
		pow = GetComponent<PowerControl> ();
		powerFactor = 0f;
		isOpen = false;
	}

	IEnumerator ChangeOutput (){
		int time = 1;

		while (true) {
			yield return new WaitForSeconds (checkIncrement);
			if(daynight.getIsDay()){
				if( anim.GetBool("canOpen") && !isOpen ){
					anim.SetTrigger("Open");
					isOpen = true;
				}
				if(isOpen){
					time=daynight.getTimeofDay();
					if(time > 1 && time < astralZenith){ //only this portion will normally execute
						powerFactor = 1f * ( (float) time / (float) astralZenith);
					}else if (time > astralZenith && time < 180){ //THIS SHOULDNT HAPPEN DUE TO HOW TIME/ROTATION IS ADJUSTED IN DAYNIGHTCYCLE
						powerFactor = 1f * (180f- (float) time) / (float) astralZenith;
					}
					pow.setWatts( (int) (baseOutput * powerFactor * outputFactor));
				//	Debug.Log (powerFactor);
				}else{
					pow.setWatts(0); //turn off the solar reactor
				}
			}else{
				if(isOpen){
					anim.SetTrigger("Close");
					isOpen = false;
				}
				pow.setWatts(0); //turn off the solar reactor
			}
		}
	}

	public void ForceShut(){
		if (isOpen) {
			anim.SetTrigger ("Close");
			isOpen = false;
		}
		anim.SetBool ("canOpen", false);
		return;
	}

	public void allowOpen(){
		anim.SetBool ("canOpen", true);
		return;
	}
		

}
