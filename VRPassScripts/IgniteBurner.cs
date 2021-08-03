using UnityEngine;
using System.Collections;

public class IgniteBurner : MonoBehaviour {
	
	public GameObject playerAvatar;
	private PlayerLookingAt lookDetector;

	public GameObject flame;
	public ParticleSystem flameParticle;

	public Sprite igniteSprite;
	public Sprite raiseSprite;
	public Sprite lowerSprite;

	private bool active;
	
	void Start () {
		active = false;
		if (playerAvatar == null) {
			playerAvatar = GameObject.Find("OVRPlayerController");
		}
		//Connect to the master lookdetector
		lookDetector = playerAvatar.GetComponentInChildren<PlayerLookingAt>();
	}

	void Update () {

		//ignite fire when pressed
		if (active && (Input.GetKeyDown (KeyCode.Alpha1) || Input.GetButtonDown ("Fire3"))) {
			flame.SetActive(true);
			lookDetector.writeFile("Ignited Burner", transform.gameObject.name + " " + GetInstanceID().ToString(), 
			                       transform.position.ToString(), transform.rotation.eulerAngles.ToString(),
			                       " at level " + flameParticle.emissionRate.ToString() );
//			Debug.Log("Engaged");
		}

		//increase the emission of flames
		if (active && (Input.GetKey(KeyCode.Alpha2) || Input.GetButton ("Fire4"))) {
			if(flameParticle.emissionRate<300){
				flameParticle.emissionRate=flameParticle.emissionRate+1;
			}
//			Debug.Log("fire turning up");
		}
		if (active && (Input.GetKeyUp (KeyCode.Alpha2) || Input.GetButtonUp ("Fire4"))) {
			lookDetector.writeFile("Increased Burner", transform.gameObject.name + " " + GetInstanceID().ToString(), 
			                       transform.position.ToString(), transform.rotation.eulerAngles.ToString(),
			                       " to level " + flameParticle.emissionRate.ToString() );
		}

		//reduce flames, put out if low enough
		if (active && (Input.GetKey(KeyCode.Alpha3) || Input.GetButton ("Fire2"))) {
			if(flameParticle.emissionRate>30){
				flameParticle.emissionRate=flameParticle.emissionRate-1;
//				Debug.Log("fire turning down");
			}else{
				flame.SetActive(false);
//				Debug.Log("fire turning off");
			}
		}
		if (active && (Input.GetKeyUp (KeyCode.Alpha3) || Input.GetButtonUp ("Fire2"))) {
			lookDetector.writeFile("Decreased Burner", transform.gameObject.name + " " + GetInstanceID().ToString(), 
			                       transform.position.ToString(), transform.rotation.eulerAngles.ToString(), 
			                       " to level " + flameParticle.emissionRate.ToString() );
		}
	
	}

	public void setActive(){
		lookDetector.SetLeftImage(igniteSprite);
		lookDetector.SetTopImage(raiseSprite);
		lookDetector.SetRightImage(lowerSprite);
		active = true;
		//		Debug.Log ("active");
		return; }
	
	public void setInactive(){
		lookDetector.ClearLeftImage();
		lookDetector.ClearTopImage();
		lookDetector.ClearRightImage();
		active=false;
		//		Debug.Log ("inactive");
		return; }
}
