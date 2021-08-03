using UnityEngine;
using System.Collections;

public class ToiletControl : MonoBehaviour {

	public GameObject playerAvatar;
	private PlayerLookingAt lookDetector;
	
	public GameObject water;
	public ParticleSystem flowParticle1;
	public ParticleSystem flowParticle2;
	public GameObject lid;

	public Sprite flushSprite;
	public Sprite openSprite;
	public Sprite closeSprite;

	public float maxLevel; //max height of the "water"
	public float minLevel; //min height of the "water"
	public float closed;
	public float open;
	
	private bool active;
	private bool engaged;
	private float tolerance = .3f;

	//control the rate at which the water is turned up
	private float timer; //timer that counts until fillrate
//	private float fillrate = .1f; //rate at which flows change
	private float raiserate; //rate the "water" raises at
//	private float minraiserate =-.002f;
	private float maxraiserate =.001f;
	
	void Start () {
		active = false;
		if (playerAvatar == null) {
			playerAvatar = GameObject.Find("OVRPlayerController");
		}
		//Connect to the master lookdetector
		lookDetector = playerAvatar.GetComponentInChildren<PlayerLookingAt>();
		StartCoroutine ("FillSink");
		raiserate = .0002f;
		engaged = true;
	}
	
	void Update () {
		timer = timer + Time.deltaTime;

		//flush
		if (active && (Input.GetKeyDown (KeyCode.Alpha1) || Input.GetButtonDown ("Fire3"))) {
			raiserate = -.006f;
			engaged=true;
			flowParticle1.emissionRate = 20;
			flowParticle2.emissionRate = 20;
			StartCoroutine ("TurnOffFlush");
	//		Debug.Log(raiserate);
			lookDetector.writeFile("Flushed", transform.gameObject.name + " " + transform.GetInstanceID().ToString(), 
			                       transform.position.ToString(), transform.rotation.eulerAngles.ToString(), 
			                       "" );
		}
		
		//seat down
		if (active && (Input.GetKey(KeyCode.Alpha2) || Input.GetButton ("Fire4"))) {
			float currenRotation = lid.transform.localRotation.eulerAngles.x;
	//		Debug.Log(currenRotation);
			if(Mathf.Abs(currenRotation - closed) > tolerance){
				if(open<closed){
					if(currenRotation>closed){
						lid.transform.Rotate(new Vector3(-.4f, 0f, 0f));
					} else if(currenRotation < closed){
						lid.transform.Rotate(new Vector3(.4f, 0f, 0f));
					}
				} else { 
					if(currenRotation<closed){
						lid.transform.Rotate(new Vector3(.4f, 0f, 0f));
					} else {
						lid.transform.Rotate(new Vector3(-.4f, 0f, 0f));
					}
				}
			} // end if within tolerance
		}
		if (active && (Input.GetKeyUp (KeyCode.Alpha2) || Input.GetButtonUp ("Fire4"))) {
			lookDetector.writeFile("Lowered seat", lid.transform.gameObject.name + " " + lid.transform.GetInstanceID().ToString(), 
			                       lid.transform.position.ToString(), lid.transform.rotation.eulerAngles.ToString(), 
			                       " to level " + lid.transform.localRotation.eulerAngles.ToString() );
		}
		
		//seat up
		if (active && (Input.GetKey(KeyCode.Alpha3) || Input.GetButton ("Fire2"))) {

			float currenRotation = lid.transform.localRotation.eulerAngles.x;
	//		Debug.Log(currenRotation);
			if(Mathf.Abs(currenRotation - open) > tolerance){
				if(open<closed){
					if(currenRotation>open){
						lid.transform.Rotate(new Vector3(-.4f, 0f, 0f));
					} else if(currenRotation < open){
						lid.transform.Rotate(new Vector3(.4f, 0f, 0f));
					}
				} else { 
					if(currenRotation<open){
						lid.transform.Rotate(new Vector3(.4f, 0f, 0f));
					} else {
						lid.transform.Rotate(new Vector3(-.4f, 0f, 0f));
					}
				}
			} // end if within tolerance
		}
		if (active && (Input.GetKeyUp (KeyCode.Alpha3) || Input.GetButtonUp ("Fire2"))) {
			lookDetector.writeFile("Raised seat", lid.transform.gameObject.name + " " + lid.transform.GetInstanceID().ToString(), 
			                       lid.transform.position.ToString(), lid.transform.rotation.eulerAngles.ToString(), 
			                       " to level " + lid.transform.localRotation.eulerAngles.ToString() );
		}
	}

	IEnumerator FillSink(){
		while (true) {
			if(engaged){
				water.transform.localPosition = water.transform.localPosition + new Vector3 (0f, raiserate, 0f);
			}
			if(raiserate<maxraiserate){
				raiserate=raiserate+.0002f;
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

	IEnumerator TurnOffFlush(){
		yield return new WaitForSeconds(4f);
		flowParticle1.emissionRate = 0;
		flowParticle2.emissionRate = 0;
		yield break;
	}

	public void setActive(){
		lookDetector.SetLeftImage(flushSprite);
		lookDetector.SetTopImage(openSprite);
		lookDetector.SetRightImage(closeSprite);
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
