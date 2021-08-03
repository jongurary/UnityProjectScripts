using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinByVelocity : MonoBehaviour {

	public GameObject spinner;
	float spinRate;
	Vector3 lastPosition;
	
	void Start () {
		lastPosition = transform.position;
		StartCoroutine (spin ());
	}

	void Update () {
		
	}

	void FixedUpdate()
	{
		//calculate speed for spin rate
		spinRate = (transform.position - lastPosition).magnitude * 20f;
		lastPosition = transform.position;
		//Debug.Log (spinRate);
	}
	
	IEnumerator spin(){
		while (true) {
			yield return new WaitForSeconds (.01f);

		//	spinRate=4f;
			spinner.transform.Rotate(new Vector3(0f, 0f, -spinRate));
		}
		yield break;
	}
}
