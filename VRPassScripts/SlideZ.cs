using UnityEngine;
using System.Collections;

public class SlideZ : MonoBehaviour {

	public GameObject rootParent;
	public GameObject playerAvatar;
	private PlayerLookingAt lookDetector;
	
	private bool active;
	
	public Sprite openSprite;
	public Sprite closeSprite;
	
	public float openPosition;
	public float closedPosition;
	
	private float moveunit =.005f;
	
	//Note: Only slides along the z-axis
	
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
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 moveVector = new Vector3 (0f, 0f, moveunit);
		
		//close sliding drawer
		if (active && (Input.GetKey(KeyCode.Alpha2) || Input.GetButton("Fire4")) ) {

		if (openPosition > closedPosition){
			if (rootParent.transform.localPosition.z > (moveunit + closedPosition)) {
				rootParent.transform.localPosition = rootParent.transform.localPosition - moveVector;
			}
		}else{
			if (rootParent.transform.localPosition.z < (moveunit + closedPosition)) {
				rootParent.transform.localPosition = rootParent.transform.localPosition + moveVector;
			}
			}
		}

		if (active && (Input.GetKeyUp (KeyCode.Alpha2) || Input.GetButtonUp ("Fire4"))) {
			lookDetector.writeFile("Closed", rootParent.gameObject.name + " " + rootParent.GetInstanceID().ToString(), 
			                       rootParent.transform.position.ToString(), rootParent.transform.rotation.eulerAngles.ToString(), 
			                       " to position " + rootParent.transform.localPosition);
		}
		
		//open sliding drawer
		if (active && (Input.GetKey(KeyCode.Alpha1) || Input.GetButton("Fire3")) ) {

			if (openPosition > closedPosition) {
				if (rootParent.transform.localPosition.z < (moveunit + openPosition)) {
					rootParent.transform.localPosition = rootParent.transform.localPosition + moveVector;
				}
			}else{
				if (rootParent.transform.localPosition.z > (moveunit + openPosition)) {
					rootParent.transform.localPosition = rootParent.transform.localPosition - moveVector;
				}
			}
		}

		if (active && (Input.GetKeyUp (KeyCode.Alpha1) || Input.GetButtonUp ("Fire3"))) {
			lookDetector.writeFile("Opened", rootParent.gameObject.name + " " + rootParent.GetInstanceID().ToString(), 
			                       rootParent.transform.position.ToString(), rootParent.transform.rotation.eulerAngles.ToString(), 
			                       " to position " + rootParent.transform.localPosition);
		}
		
	}
	
	public void setActive(){
		lookDetector.SetLeftImage(openSprite);
		lookDetector.SetTopImage(closeSprite);
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
