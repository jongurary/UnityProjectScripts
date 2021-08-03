using UnityEngine;
using System.Collections;

public class TVControls : MonoBehaviour {

	public GameObject playerAvatar;
	private PlayerLookingAt lookDetector;

	public Material[] Channels = new Material[4];
	public GameObject TVScreen;

	private int currentChannel;
	private bool active;
	private bool onOff; //true = on, false =off

	public Sprite onSprite;
	public Sprite offSprite;
	public Sprite upSprite;
	public Sprite DownSprite;

	// Use this for initialization
	void Start () {
		active = false;
		if (playerAvatar == null) {
			playerAvatar = GameObject.Find("OVRPlayerController");
		}
		//Connect to the master lookdetector
		lookDetector = playerAvatar.GetComponentInChildren<PlayerLookingAt>();

		TVScreen.GetComponent<Renderer> ().material = Channels [currentChannel];
		currentChannel = 0; //start at Channel 1
		onOff = false; //start off
	}
	
	// Update is called once per frame
	void Update () {
	
		//turns on or off the TV
		if (active && (Input.GetKeyDown (KeyCode.Alpha1) || Input.GetButtonDown ("Fire3"))) {
		//	Debug.Log(onOff);
			if(!onOff){
				onOff=true;
				TVScreen.SetActive(true);		
				lookDetector.SetLeftImage(offSprite);
				((MovieTexture)TVScreen.GetComponent<Renderer>().material.mainTexture).loop=true;
				((MovieTexture)TVScreen.GetComponent<Renderer>().material.mainTexture).Play();
			}else{
				onOff=false;
				TVScreen.SetActive(false);		
				lookDetector.SetLeftImage(onSprite);
			}
		}

		if (active && (Input.GetKeyDown (KeyCode.Alpha2) || Input.GetButtonDown ("Fire4"))) {
			if(currentChannel<3){
				currentChannel++;
			}else{
				currentChannel=0;
			}
			TVScreen.GetComponent<Renderer> ().material = Channels [currentChannel];
			((MovieTexture)TVScreen.GetComponent<Renderer>().material.mainTexture).loop=true;
			((MovieTexture)TVScreen.GetComponent<Renderer>().material.mainTexture).Play();
		//	Debug.Log(currentChannel);
		}

		if (active && (Input.GetKeyDown (KeyCode.Alpha3) || Input.GetButtonDown ("Fire2"))) {
			if(currentChannel>0){
				currentChannel--;
			}else{
				currentChannel=3;
			}
			TVScreen.GetComponent<Renderer> ().material = Channels [currentChannel];
			((MovieTexture)TVScreen.GetComponent<Renderer>().material.mainTexture).loop=true;
			((MovieTexture)TVScreen.GetComponent<Renderer>().material.mainTexture).Play();
		//	Debug.Log(currentChannel);
		}

	}

	public void setActive(){
		lookDetector.SetLeftImage(onSprite);
		lookDetector.SetTopImage(upSprite);
		lookDetector.SetRightImage(DownSprite);
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
