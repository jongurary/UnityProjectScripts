using UnityEngine;
using System.Collections;

public class AuthObjectControl : MonoBehaviour {

	public int id;
	public AuthController authcontroller;
	
	void Start () {
	
	}

	void Update () {
	
	}

	void OnTriggerEnter(){
		if(authcontroller.isrecord() || authcontroller.istesting())
			authcontroller.password = authcontroller.password + id.ToString ();
	}
}
