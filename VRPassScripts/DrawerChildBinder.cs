using UnityEngine;
using System.Collections;

//Binds interactable objects as children when they collide with this object

public class DrawerChildBinder : MonoBehaviour {

	void Start(){

	}

	void OnCollisionEnter(Collision col){
	//	Debug.Log (col.gameObject.name);
		if (col.gameObject.tag == "Interactable") {
			col.gameObject.transform.SetParent(transform, true);

		}
	}

	void OnTriggerEnter(Collider col){
	//	Debug.Log (col.gameObject.name);
		if (col.gameObject.tag == "Interactable") {
			col.gameObject.transform.SetParent(transform, true);
			
		}
	}
}
