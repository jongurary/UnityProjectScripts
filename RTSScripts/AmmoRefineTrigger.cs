using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds units to the parent object's ammo refine class
public class AmmoRefineTrigger : MonoBehaviour {
	
	private List<GameObject> toLinkOnDrop = new List<GameObject>();
	private AmmoRefine refine;
	[Tooltip("On start, gets trigger collider radius, used for range indicator display. Set in the prefab to properly display hint during build.")]
	public float range;
	
	void Start () {
		StartCoroutine (addOnDrop ());
		StartCoroutine (purgeDead ());
		refine = GetComponentInParent<AmmoRefine> ();
		range = GetComponent<SphereCollider> ().radius;
		range = range * range; //square value of range properly draws the hint circle (possible bug, but whatever)
		
	}
	
	void Update () {
		
	}
	
	/// <summary>
	/// Adds links as valid objects enter range
	/// </summary>
	void OnTriggerEnter(Collider other){
		if (other.gameObject.CompareTag ("Unit") || other.gameObject.CompareTag ("Building")) {
			//	Debug.Log (other.gameObject);
			if(other.transform.root.gameObject.GetComponentInParent<AmmoControl>()!=null){
				
				//if not finished with orbital drop, wait for drop to finish then attempt all links
				if(!GetComponentInParent<OrbitalDrop>().isLanded()){
					toLinkOnDrop.Add(other.transform.root.gameObject);
				}else{
					//	Debug.Log (other.gameObject + " linked");
					if(!refine.connections.Contains(other.transform.root.gameObject)){
						refine.connections.Add(other.transform.root.gameObject);
					}

				}
			}
		}
	}
	
	/// <summary>
	/// Breaks links as valid objects exit range
	/// </summary>
	void OnTriggerExit(Collider other){
		
		if (other.gameObject.CompareTag ("Unit") || other.gameObject.CompareTag ("Building")) {
			if(other.transform.root.gameObject.GetComponentInParent<AmmoControl>()!=null){
				refine.connections.Remove(other.transform.root.gameObject);
			}
		}
	}

	IEnumerator purgeDead(){
		while (true) {
			yield return new WaitForSeconds(10f); //low priority
			refine.connections.RemoveAll(GameObject => GameObject == null);
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
				if(!refine.connections.Contains(obj)){
					refine.connections.Add(obj);
				}
			}
		}
		yield break;
	}
}
