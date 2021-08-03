using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour {

	public GameObject dest;

	
	void Start () {

	
	}

	void OnTriggerEnter(Collider other) {
		other.transform.position = dest.transform.position;
		other.transform.rotation = dest.transform.rotation;
	}

	void Update () {
	
	}
}
