using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindPower : MonoBehaviour {
	
	public float outputFactor; //scaling factor for power output
	public GameObject wind;
	public GameObject spinner; // the blades that spin based on wind speed
	public GameObject rotator; //the midsection object that rotates towards the wind
	private float rotSpeed = .01f; //speed at which the midsection rotates towards the wind
	
	public int baseOutput; //watt output at peak windiness
	private float windStrength;

	//exceeding peak values can still generate more power, that is a power factor of greater than 1
	private float peakWindStrength = (float) Constants.GENERIC_MAX_WIND_STRENGTH; //considered the max wind strength
	private float peakHeight = 10f; //considered the max height

	private PowerControl pow;
	private float checkIncrement = 1f; //how often to scan for wind conditions
	private float powerFactor; //production factor based on height and wind condition
	
	void Start () {
		if (wind == null) {
			wind=GameObject.Find("Wind");
		}
		pow = GetComponent<PowerControl> ();
		powerFactor = 0f;
		StartCoroutine(ChangeOutput());
		StartCoroutine(Spin());
	}
	
	IEnumerator ChangeOutput (){	
		while (true) {
			yield return new WaitForSeconds (checkIncrement);
			//measure the wind's current power
			windStrength = wind.GetComponent<WindZone>().windMain;
			//adjust the power factor relative to wind strength, plus height
			if(gameObject.transform.position.y>0){ //above "sea level"
				powerFactor = 1f * ( windStrength / peakWindStrength + gameObject.transform.position.y / peakHeight);
			}else { //below sea level, no benefit from height at all
				powerFactor = 1f * ( windStrength / peakWindStrength );
			}
			pow.setWatts( (int) (baseOutput * powerFactor * outputFactor));
		}	
	}

	IEnumerator Spin() {
		while (true) {
			yield return new WaitForSeconds (.01f);
			//TODO replace spin with simple animation using varying speed
			//spin relative to the power factor, multiplied by the "base" spin speed of 4 units
			float spinRate = 4f * powerFactor;
			spinner.transform.Rotate(new Vector3(0f, 0f, spinRate));
			//move the midsection towards the wind
			rotator.transform.rotation = Quaternion.Slerp (rotator.transform.rotation, wind.transform.rotation, rotSpeed);
		}
	}
	
}
