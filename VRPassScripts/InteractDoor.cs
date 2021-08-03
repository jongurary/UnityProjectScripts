using UnityEngine;
using System.Collections;

public class InteractDoor: MonoBehaviour {
	
	public GameObject rootParent;
	public GameObject playerAvatar;
	private PlayerLookingAt lookDetector;
	
	private bool active;
	
	public Sprite openSprite;
	public Sprite closeSprite;

	public float openRotation;
	public float closedRotation;
	
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

		//TODO allow rotation beyond 360 degree point?
		float tolerance = .3f;

		//close door
		if (active && (Input.GetKey(KeyCode.Alpha2) || Input.GetButton("Fire4")) ) {
			float currenRotation = rootParent.transform.localRotation.eulerAngles.y;

			//Rotates depending on which direction is closed and open
			if(Mathf.Abs(currenRotation - closedRotation) > tolerance){
			if (openRotation < closedRotation) {
				if (currenRotation < closedRotation) {
					rootParent.transform.Rotate (new Vector3 (0f, .4f, 0f));
				} else {
					rootParent.transform.Rotate (new Vector3 (0f, -.4f, 0f));
				}
			} else {
				if (currenRotation > closedRotation) {
					rootParent.transform.Rotate (new Vector3 (0f, -.4f, 0f));
				} else {
					rootParent.transform.Rotate (new Vector3 (0f, .4f, 0f));
				}
			}
			} //end if within tolerance
		}
		if (active && (Input.GetKeyUp (KeyCode.Alpha2) || Input.GetButtonUp ("Fire4"))) {
			lookDetector.writeFile("Closed", rootParent.gameObject.name + " " + rootParent.GetInstanceID().ToString(), 
			                       rootParent.transform.position.ToString(), rootParent.transform.rotation.eulerAngles.ToString(), 
			                       " to position " + rootParent.transform.localPosition);
		}
		
		//open door
		if (active && (Input.GetKey(KeyCode.Alpha1) || Input.GetButton("Fire3")) ) {
			float currenRotation = rootParent.transform.localRotation.eulerAngles.y;

			if(Mathf.Abs(currenRotation - openRotation) > tolerance){
			if(openRotation<closedRotation){
				if(currenRotation>openRotation){
					rootParent.transform.Rotate(new Vector3(0f, -.4f, 0f));
				} else if(currenRotation < openRotation){
					rootParent.transform.Rotate(new Vector3(0f, .4f, 0f));
				}
			} else { 
				if(currenRotation<openRotation){
					rootParent.transform.Rotate(new Vector3(0f, .4f, 0f));
				} else {
					rootParent.transform.Rotate(new Vector3(0f, -.4f, 0f));
				}
			}
			} // end if within tolerance
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
