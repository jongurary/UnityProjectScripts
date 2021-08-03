using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AoECaclulator : MonoBehaviour {
	public List<GameObject> inAoe = new List<GameObject>();

	void OnTriggerEnter(Collider other){
		if (other.tag != "AirUnit" && other.tag != "Terrian" && other.tag != "Missile") {
			inAoe.Add (other.transform.root.gameObject);
		//	print (other.transform.root.gameObject);
		}
	}

	void OnTriggerExit(Collider other){
		if (other.tag != "AirUnit" && other.tag != "Terrian" && other.tag != "Missile") {
			inAoe.Remove (other.transform.root.gameObject);
		//	print (other.transform.root.gameObject);
		}
	}
}
