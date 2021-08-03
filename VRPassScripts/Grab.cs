using UnityEngine;
using System.Collections;

public class Grab : MonoBehaviour {

	public GameObject rootParent;
	public GameObject playerAvatar;
	public GameObject HCube;
	public GameObject VCube;
	private PlayerLookingAt lookDetector;
	private OVRPlayerController playerController;
	private GameObject eyeAnchor;

	private bool active;
	private bool held;
	private bool spinning;
	private Rigidbody rb;

//	public Sprite grabSprite;
	public Sprite throwSprite;
	public Sprite spinSprite;
	public Sprite dropSprite;

	private float pushForce = 5000f;

	// Use this for initialization
	void Start () {
		active = false;
		held = false;
		spinning = false;

		//Try to correct the mistakes of lazy programmers
		if (rootParent == null) {
			rootParent=transform.parent.gameObject; }

		if (playerAvatar == null) {
			playerAvatar = GameObject.Find("OVRPlayerController"); }

		//Connect to the master lookdetector and the rigidbody of the parent
		lookDetector = playerAvatar.GetComponentInChildren<PlayerLookingAt>();
		rb = rootParent.GetComponent<Rigidbody>();
//		rb.isKinematic=false;
//		rb.detectCollisions=false;

		//Connect to the script on the character that moves around
		playerController = playerAvatar.GetComponent<OVRPlayerController> ();

		//Connect to the eye anchor area to bind child obejcts in front of the player
		eyeAnchor = lookDetector.centerAnchor;

		//Positions H and V cubes in the correct locations
		HCube.transform.localPosition = new Vector3 (-2.5f, -1.8f, 0f);
		VCube.transform.localPosition = new Vector3 (0, -1.8f, 2.5f);

	}
	
	// Update is called once per frame
	void Update () {

		//pick up object
		if(active && !held && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetButtonDown("Fire3")) ){
			lookDetector.writeFile("Picked up", rootParent.gameObject.name + " " + rootParent.GetInstanceID().ToString(), 
			          rootParent.transform.position.ToString(), rootParent.transform.rotation.eulerAngles.ToString(), 
			          " Player Facing: " + eyeAnchor.transform.rotation.eulerAngles);
			//transform the object in front of the player, bind it as a child
			rootParent.transform.position=eyeAnchor.transform.position + 
				eyeAnchor.transform.forward*4 + new Vector3(0f,-1f,0f);
			rootParent.transform.parent=eyeAnchor.transform;
	//		rootParent.transform.rotation=eyeAnchor.transform.rotation;
			//Tell the look detector to stop detecting things
			lookDetector.SetHolding();
			held = true;
			//tell detector to update the sprites accordingly
			lookDetector.ClearLeftImage();
			lookDetector.SetTopImage(throwSprite);
			lookDetector.SetExtraImage1(spinSprite);
			lookDetector.SetLeftImage(dropSprite);
			//make sure phsyics are off
			rb.isKinematic=true;
			rb.detectCollisions=false;

			Rigidbody[] allRBs = rootParent.GetComponentsInChildren<Rigidbody>();
			for(int r=0; r<allRBs.Length; r++){
				allRBs[r].isKinematic = true;
			}
		} else
		//drop object
		if (held && (Input.GetKeyDown (KeyCode.Alpha1) || Input.GetButtonDown ("Fire3"))) {
			lookDetector.writeFile("Dropped", rootParent.gameObject.name + " " + rootParent.GetInstanceID().ToString(), 
			          rootParent.transform.position.ToString(), rootParent.transform.rotation.eulerAngles.ToString(), 
			          " Player Facing: " + eyeAnchor.transform.rotation.eulerAngles);
			//disconnect from player
			rootParent.transform.parent=null;
			//turn on physics for the object
			rb.isKinematic=false;
			rb.detectCollisions=true;
			Rigidbody[] allRBs = rootParent.GetComponentsInChildren<Rigidbody>();
			for(int r=0; r<allRBs.Length; r++){
				allRBs[r].transform.parent=null;
				allRBs[r].isKinematic = false;
			}
			//apply force forward
			rootParent.GetComponent<Rigidbody>().AddForce(
				eyeAnchor.transform.forward*100);
			//update booleans
			held=false;
			setInactive();
			lookDetector.FreeHolding();
			//turn physics back off
			StartCoroutine ("TurnOffPhysicsAfterTime",10f);
			//update sprites accordingly
			lookDetector.ClearTopImage();
			lookDetector.ClearLeftImage();
			lookDetector.ClearExtraImage1();
			lookDetector.ClearOldObject();
			if(HCube!=null && VCube!=null){
				HCube.SetActive(false);
				VCube.SetActive(false);
			}


		}

		//throw object
		if (held && (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetButtonDown("Fire4")) ) {
			lookDetector.writeFile("Threw", rootParent.gameObject.name+ " " + rootParent.GetInstanceID().ToString(), 
			          rootParent.transform.position.ToString(), rootParent.transform.rotation.eulerAngles.ToString(), 
			          " Player Facing: " + eyeAnchor.transform.rotation.eulerAngles);
			//disconnect from player
			rootParent.transform.parent=null;
			//turn on physics for the object
			rb.isKinematic=false;
			rb.detectCollisions=true;
			Rigidbody[] allRBs = rootParent.GetComponentsInChildren<Rigidbody>();
			for(int r=0; r<allRBs.Length; r++){
				allRBs[r].transform.parent=null;
				allRBs[r].isKinematic = false;
				allRBs[r].AddForce(
					eyeAnchor.transform.forward*(pushForce/2));
			}
			//apply force forward
			rootParent.GetComponent<Rigidbody>().AddForce(
				eyeAnchor.transform.forward*pushForce);
			//update booleans
			held=false;
			setInactive();
			lookDetector.FreeHolding();
			//turn physics back off
			StartCoroutine ("TurnOffPhysicsAfterTime",10f);
			//update sprites accordingly
			lookDetector.ClearTopImage();
			lookDetector.ClearLeftImage();
			lookDetector.ClearExtraImage1();
			lookDetector.ClearOldObject();
			if(HCube!=null && VCube!=null){
				HCube.SetActive(false);
				VCube.SetActive(false);
			}
		}

		//spin objects when left trigger held, using right analog stick
		if (held && Input.GetAxis ("LeftStick") > 0) {
			spinning = true;
			float spinrate = 1.4f;
			//turn off movement when rotating objects
			playerController.enabled = false;
			if(HCube!=null && VCube!=null){ //engages spin indicator "cubes"
				HCube.SetActive(true);
				VCube.SetActive(true);
			}

			rootParent.transform.Rotate (new Vector3 (
				Input.GetAxis ("HStick") * spinrate, 0f, Input.GetAxis ("VStick") * spinrate));
		} else if(held && Input.GetAxis ("LeftStick") < .5){ //engages movement and clears spin indicators
			if (spinning){
				lookDetector.writeFile("Rotated", rootParent.gameObject.name+ " " + rootParent.GetInstanceID().ToString(), 
				            rootParent.transform.position.ToString(), rootParent.transform.rotation.eulerAngles.ToString(), 
				          " Player Facing: " + eyeAnchor.transform.rotation.eulerAngles);
			}
			spinning = false;
			playerController.enabled = true;
			if(HCube!=null && VCube!=null){
				HCube.SetActive(false);
				VCube.SetActive(false);
			}
		}

	
	}

	//TODO disable this if person re-picks up the object
	//turns off rigidbody physics for an object after some time
	IEnumerator TurnOffPhysicsAfterTime(float waitTime){
		yield return new WaitForSeconds(waitTime);
	//	rb.isKinematic=true;
	//	rb.detectCollisions=false;
		yield return null;
	}

	public void setActive(){
		active = true;
//		lookDetector.SetLeftImage(grabSprite);
//		Debug.Log ("active");
		return;
	}

	public void setInactive(){
		active=false;
		lookDetector.ClearLeftImage();
		playerController.enabled = true;
//		Debug.Log ("inactive");
		return;
	}
}
