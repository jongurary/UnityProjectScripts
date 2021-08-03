using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour {


	//length of day and night, in arbitary duration units
	public bool simple; //true for simple/day night based in intensity, false for dynamic day/night from rotation of sun
	[Tooltip("Set to true to disable changing of day/night cycle. Sun will be locked at current time of day.")]
	public bool changeDisabled;
	public int dayLength;
	public int nightLength;
	private float inverseFactor = 100f; //numerator on which lengths are divded to get the speed factor of sun rotation

	//Strength of the sun's light, in generic float units
	public float dayIntensity;
	public float nightIntensity;

	private Light sunlight; //the sun's regal glow
	private bool isDay; //true in day, false at night
	private float transitionTimeIncrement; //Increment time, of the transition between day and night
	private float transitionIncrement; //strength of the change in sun insenity per increment
	private int daysElapsed; //counts the days that have gone by
	private int timeofDay; //time of day is just the angle of the sun's rotation, because time is arbitrary, man;
	
	
	void Start () {

		daysElapsed = 0;
		sunlight = GetComponent<Light> ();
		isDay = true;
		transitionTimeIncrement = .2f; //probably want to keep this high to avoid redrawing light maps excessively
		transitionIncrement = .007f; //want to keep this low to avoid "choppy" looking shadow changes
		StartCoroutine(ChangeTimeofDay());
		timeofDay = 1;
		
	}

	void Update () {
		
	}

	//Makes the sun come up and down at set intervals
	//Note: day/night status is set immediately at the start of the transition between day and night
	IEnumerator ChangeTimeofDay (){
		while (true) {
			if(!changeDisabled){
				if(!simple){
					yield return new WaitForSeconds (transitionTimeIncrement);
					float speedFactor=1f; //based on day or night duration and current time
					int rot = (int) transform.rotation.eulerAngles.x;
					if(rot<0){ //fix negative euler angles with advanced mathematics.
						rot=rot+360;
					}

					if(rot > 180 && rot < 360){ //it's night
						isDay = false;
						speedFactor = (float) inverseFactor/nightLength;
					}else{ //it's day
						speedFactor = (float) inverseFactor/dayLength;
						isDay=true;
					}
						timeofDay = rot;
			//			Debug.Log(rot);
						sunlight.transform.Rotate(Vector3.right * transitionIncrement * speedFactor);

				}else{ //use simple day/night code instead

					 //Simple Day/night code based on intensity
					if (isDay) {
						yield return new WaitForSeconds (dayLength);
						isDay = false;
						//facilitates gradual change of lightning based on increment settings
						while(sunlight.intensity > nightIntensity){
							yield return new WaitForSeconds (transitionTimeIncrement);
							sunlight.intensity -= transitionIncrement;
								if(timeofDay>0){
									timeofDay--;
								}
						}
					} else {
						yield return new WaitForSeconds (nightLength);
						isDay = true;
						while(sunlight.intensity < dayIntensity){
							yield return new WaitForSeconds (transitionTimeIncrement);
							sunlight.intensity += transitionIncrement;
								timeofDay++;
						}
						daysElapsed++; //a new day dawns, increment elapsed days
					}

				}
			}
		}

		yield return null;
	}

	public bool getIsDay(){ return isDay; }
	public int getTimeofDay(){ return timeofDay; }
}
