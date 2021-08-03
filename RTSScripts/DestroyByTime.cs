using UnityEngine;
using System.Collections;

public class DestroyByTime : MonoBehaviour {

	public float time;
	public bool dontKill; //if true, don't kill this unit
	
	void Start () {
		if (!dontKill) {
			Destroy (gameObject, time);
		}
	}

	void Update () {
		if (!dontKill) {
			Destroy (gameObject, time);
		}
	
	}
}
