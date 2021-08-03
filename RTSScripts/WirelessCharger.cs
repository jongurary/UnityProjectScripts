using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//NOTE: if physics setting "queries hit triggers" is not disabled, this has to be on an ignoreraycast layer

public class WirelessCharger : MonoBehaviour {

	private List<GameObject> toLinkOnDrop = new List<GameObject>();

	void Start () {
		StartCoroutine (addOnDrop ());
		
	}

	void Update () {
		
	}

	/// <summary>
	/// Adds links as valid objects enter range
	/// </summary>
	void OnTriggerEnter(Collider other){
		if (other.gameObject.CompareTag ("Unit") || other.gameObject.CompareTag ("Building")) {
		//	Debug.Log (other.gameObject);
			if(other.transform.root.gameObject.GetComponentInParent<Linkage>()!=null){

				//if not finished with orbital drop, wait for drop to finish then attempt all links
				if(!GetComponentInParent<OrbitalDrop>().isLanded()){
					toLinkOnDrop.Add(other.transform.root.gameObject);
				}else{
				//	Debug.Log (other.gameObject + " linked");
					GetComponentInParent<Linkage>().forceOutLink(other.transform.root.gameObject);
				}
			}
		}
	}

	/// <summary>
	/// Breaks links as valid objects exit range
	/// </summary>
	void OnTriggerExit(Collider other){

		if (other.gameObject.CompareTag ("Unit") || other.gameObject.CompareTag ("Building")) {
			if(other.transform.root.gameObject.GetComponentInParent<Linkage>()!=null){
				GetComponentInParent<Linkage>().forceBreakLink(other.transform.root.gameObject);
			}
		}
	}

	/// <summary>
	/// Add links after orbital drop is finished then terminates
	/// </summary>
	IEnumerator addOnDrop(){
		if (! GetComponentInParent<OrbitalDrop>().isLanded()) {
			yield return new WaitUntil( () => GetComponentInParent<OrbitalDrop>().isLanded() == true );
		}
		foreach( GameObject obj in toLinkOnDrop){
			if(obj!=null){
				GetComponentInParent<Linkage>().forceOutLink(obj);
			}
		}
		yield break;
	}
	
}
