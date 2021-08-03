using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatchController : MonoBehaviour {

	[Tooltip("Optional, rotates to 100 degrees when the missile is launched")]
	public GameObject hatch; //hatch that pops open when the missile is launched
	[Tooltip("object holding the hatch's mesh renderer")]
	public GameObject hatchRenderer; //object holding the hatch's mesh renderer
	[Tooltip("Material used for the hatch when it has a missile.")]
	public Material armedHatch;
	[Tooltip("Material used for the hatch when it is is empty")]
	public Material disarmedHatch;
	private Quaternion startRotation;

	// Use this for initialization
	void Start () {
		startRotation = hatch.transform.rotation;	
		StartCoroutine (checkHatch ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void openHatch(){
		StartCoroutine (openHatchRoutine ());
	}

	public void closeHatch(){
		StartCoroutine (closeHatchRoutine ());
	}

	IEnumerator checkHatch(){
		if(hatchRenderer==null){
			yield break;
		}

		while (true) {
			yield return new WaitForSeconds(1f);
			if(GetComponent<UnitBuilder>().Slave1 !=null){
				hatchRenderer.GetComponent<Renderer>().material = armedHatch;
			}else{
				hatchRenderer.GetComponent<Renderer>().material = disarmedHatch;
			}
		}
	}

	IEnumerator openHatchRoutine(){
		while ( hatch.transform.localRotation.z < .65f ) {
		//	Debug.Log (transform.localRotation.z);
			yield return new WaitForSeconds(.01f);
			//rotate hatch into launch position
			hatch.transform.Rotate(0f,0f,3f);
		}
		yield return new WaitForSeconds (5f);
		StartCoroutine (closeHatchRoutine ());
		yield break;
	}

	IEnumerator closeHatchRoutine(){
		//close the hatch
		while( hatch.transform.localRotation.z > 0f ){
			yield return new WaitForSeconds(.01f);
			//return to closed position
			hatch.transform.Rotate(0f,0f,-1f);
		}
		yield break;
	}
}
