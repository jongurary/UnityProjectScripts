using UnityEngine;
using System.Collections;

public class GunSpinner : MonoBehaviour {

	public GameObject toSpin;
	public float spinSpeed;
	public float spinRate;

	private UnitAttackArea targetControl;
	
	void Start () {
		targetControl = GetComponent<UnitAttackArea> ();
		StartCoroutine(Spin(spinSpeed));
	}

	IEnumerator Spin(float waitTime) {
		while (true) {
			yield return new WaitForSeconds (waitTime);
			if(targetControl.hasTarget){
				toSpin.transform.Rotate(new Vector3(spinRate, 0f, 0f));
			}
		}
	}
}
