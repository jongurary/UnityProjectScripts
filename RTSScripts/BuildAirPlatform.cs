using UnityEngine;
using System.Collections;

public class BuildAirPlatform : MonoBehaviour {

	public Animator buildanimator;
	public int animationlength;

	// Use this for initialization
	void Start () {
		buildanimator=GetComponent<Animator>();
		StartCoroutine(TurnOffAnimator());

	}

	IEnumerator TurnOffAnimator(){
		yield return new WaitForSeconds(animationlength);
		buildanimator.enabled = false;
		yield break;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
