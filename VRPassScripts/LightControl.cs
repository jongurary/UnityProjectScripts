using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {

	public GameObject rootParent; //component that holds the light object this switch controls
	public GameObject extra1;
	public GameObject extra2;
	public GameObject playerAvatar;
	public GameObject HaloOn;
	public GameObject backupLight;
	public GameObject HaloOff;
	public GameObject SwitchObject;
	private PlayerLookingAt lookDetector;
	private Light rootLight;
	private Light extraLight1;
	private Light extraLight2;
	
	private bool active;
	public bool onoff;
	
	public Sprite onSprite;
	public Sprite offSprite;

	
	// Use this for initialization
	void Start () {
		active = false;
		
		//Try to correct the mistakes of lazy programmers
		if (rootParent == null) {
			rootParent=transform.parent.gameObject;
		}
		if (playerAvatar == null) {
			playerAvatar = GameObject.Find("OVRPlayerController");
		}
		
		//Connect to the master lookdetector
		lookDetector = playerAvatar.GetComponentInChildren<PlayerLookingAt>();
		//connect to light objects
		rootLight = rootParent.GetComponent<Light> ();
		if(extra1!=null)
			extraLight1 = extra1.GetComponent<Light> ();
		if(extra2!=null)
			extraLight2 = extra2.GetComponent<Light> ();
	}

	void Update () {		
		//Turn on/off light
		if (active && (Input.GetKeyDown (KeyCode.Alpha1)|| Input.GetButtonDown ("Fire3"))) {
			if(!onoff){ //if light is off
				onoff=true; //turn light on
				//fancy stuff for light switches only
				if(HaloOff!=null)
					HaloOff.SetActive(false);
				if(HaloOn!=null)
					HaloOn.SetActive(true);
				if(backupLight!=null)
					backupLight.SetActive(false);
				if(SwitchObject!=null)
					SwitchObject.transform.Rotate(new Vector3(0f, 0f, 180f));

				//turn the light on
				rootLight.enabled=true;

				//turn option lights on
				if(extraLight1!=null)
					extraLight1.enabled=true;
				if(extraLight2!=null)
					extraLight2.enabled=true;

				lookDetector.SetLeftImage (offSprite);
			}else{ //light was on, turn it off
				onoff=false; //turn light off
				//fancy stuff for light switches only
				if(HaloOff!=null)
					HaloOff.SetActive(true);
				if(HaloOn!=null)
					HaloOn.SetActive(false);
				if(backupLight!=null)
					backupLight.SetActive(true);
				if(SwitchObject!=null)
					SwitchObject.transform.Rotate(new Vector3(0f, 0f, 180f));

				//turn the light off
				rootLight.enabled=false;

				//turn option lights off
				if(extraLight1!=null)
					extraLight1.enabled=false;
				if(extraLight2!=null)
					extraLight2.enabled=false;

				lookDetector.SetLeftImage (onSprite);
			}
		}
		
		//for future use
		if (active && Input.GetKey (KeyCode.Alpha2)) {
//KEY2
		}	
		
	}

	public void setActive(){
		if (!onoff) { //if light is off, show on sprite
			lookDetector.SetLeftImage (onSprite);
		} else { //if light is on, show off sprite
			lookDetector.SetLeftImage (offSprite);
		}
		active = true;
	//			Debug.Log ("active");
		return; }
	
	public void setInactive(){
		lookDetector.ClearLeftImage();
		active=false;
		//		Debug.Log ("inactive");
		return; }
}
