using UnityEngine;
using System.Collections;

public class WaterFaucet : MonoBehaviour {

	public GameObject playerAvatar;
	private PlayerLookingAt lookDetector;
	
	public GameObject water;
	public ParticleSystem flowParticle;

	public Sprite raiseSprite;
	public Sprite lowerSprite;

	public float maxLevel; //max height of the "water"
	public float minLevel; //min height of the "water"
	
	private bool active;
	private bool engaged;

	//control the rate at which the water is turned up
	private float timer; //timer that counts until fillrate
	private float fillrate = .1f; //rate at which flows change
	private float raiserate; //rate the "water" raises at
	private float minraiserate =-.002f;
	private float maxraiserate =.002f;
	
	void Start () {
		active = false;
		if (playerAvatar == null) {
			playerAvatar = GameObject.Find("OVRPlayerController");
		}
		//Connect to the master lookdetector
		lookDetector = playerAvatar.GetComponentInChildren<PlayerLookingAt>();
		StartCoroutine ("FillSink");
		raiserate = -.0002f;
		engaged = true;
	}
	
	void Update () {
		timer = timer + Time.deltaTime;

		//increase water flow
		if (active && (Input.GetKey (KeyCode.Alpha1) || Input.GetButton ("Fire3"))) {
			if(flowParticle.emissionRate<55 && timer>fillrate){
				engaged = true;
				flowParticle.emissionRate=flowParticle.emissionRate+3;
				timer=0;
				if(raiserate<maxraiserate){
					raiserate=raiserate+.0002f;
				}
			}
		}
		if (active && (Input.GetKeyUp (KeyCode.Alpha1) || Input.GetButtonUp ("Fire3"))) {
			lookDetector.writeFile("Increased flow", transform.gameObject.name + " " + transform.GetInstanceID().ToString(), 
			                       transform.position.ToString(), transform.rotation.eulerAngles.ToString(), 
			                       " to level " + flowParticle.emissionRate.ToString() );
		}
		
		//decrease water flow
		if (active && (Input.GetKey(KeyCode.Alpha2) || Input.GetButton ("Fire4"))) {
			if(flowParticle.emissionRate>0 && timer>fillrate ){
				engaged = true;
				flowParticle.emissionRate=flowParticle.emissionRate-3;
				timer=0;
				if(raiserate>minraiserate){
					raiserate=raiserate-.001f;
				}
			}
		}
		if (active && (Input.GetKeyUp (KeyCode.Alpha2) || Input.GetButtonUp ("Fire4"))) {
			lookDetector.writeFile("Decreased flow", transform.gameObject.name + " " + transform.GetInstanceID().ToString(), 
			                       transform.position.ToString(), transform.rotation.eulerAngles.ToString(), 
			                       " to level " + flowParticle.emissionRate.ToString() );
		}
		
		//Placeholder for extra button
		if (active && (Input.GetKey(KeyCode.Alpha3) || Input.GetButton ("Fire2"))) {
		}	
	}

	IEnumerator FillSink(){
		while (true) {
			if(engaged){
				water.transform.localPosition = water.transform.localPosition + new Vector3 (0f, raiserate, 0f);
			}

			if (water.transform.localPosition.y < minLevel) {
				engaged=false;
				water.transform.localPosition=
					new Vector3(water.transform.localPosition.x, minLevel+.01f,water.transform.localPosition.z);
			}else if(water.transform.localPosition.y > maxLevel){
				engaged=false;
				water.transform.localPosition=
					new Vector3(water.transform.localPosition.x, maxLevel-.01f,water.transform.localPosition.z);
			}

			yield return new WaitForSeconds(.1f);
		}
//		yield break;
	}

	public void setActive(){
		lookDetector.SetLeftImage(raiseSprite);
		lookDetector.SetTopImage(lowerSprite);
		active = true;
		//		Debug.Log ("active");
		return; }
	
	public void setInactive(){
		lookDetector.ClearLeftImage();
		lookDetector.ClearTopImage();
		active=false;
		//		Debug.Log ("inactive");
		return; }
}
