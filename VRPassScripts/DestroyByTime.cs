using UnityEngine;
using System.Collections;

public class DestroyByTime : MonoBehaviour {

	public float waittime;
	// Use this for initialization
	void Start () {
		Destroy (gameObject, waittime);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
