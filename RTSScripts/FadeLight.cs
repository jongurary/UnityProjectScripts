using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeLight : MonoBehaviour {
	public float fadeTime;
	public GameObject toFade;
	private Light lightToFade;

	void Start () {
		lightToFade = toFade.GetComponent<Light> ();
		StartCoroutine (FadeOverTime (fadeTime));
	}

	void Update () {
		
	}

	IEnumerator FadeOverTime( float waitTime){

		while (lightToFade.intensity > .05f) {
			yield return new WaitForSeconds (waitTime);
			lightToFade.intensity = lightToFade.intensity - .05f;
		}
	}
}
