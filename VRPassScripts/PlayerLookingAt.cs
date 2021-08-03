using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class PlayerLookingAt : MonoBehaviour {
	
	Transform cameraTransform = null;
	public float length; //allowed distance from object
	public Text textbox; //Textbox displaying object name (debug only?)
	//zones in the command card
	public Image leftImage;
	public Image rightImage;
	public Image topImage;
	public Image bottomImage;
	public Image extraImage1;

	private int timeStamp;

	public GameObject centerAnchor;

	//images for the command card
	public Sprite transparent;
	public Sprite grabSprite;

	//physics variables
	private RaycastHit hit;
	private Vector3 rayDirection;
	private Vector3 rayStart;

	//Storage for object's name and position
	//Note: Only one object can be interacted with at once!
	private GameObject InteractableObject;
	private Vector3 ObjectPosition;
	private Vector3 PlayerPosition; //your own position
	//Selector symbol over the object's head
	private UpdateSymbol symb;
	//Various interaction codes
	private Grab grab;
	private InteractDoor door;
	private InteractDoorZ doorz;
	private Slide slide;
	private SlideZ slidez;
	private LightControl lightControl;
	private IgniteBurner flameControl;
	private WaterFaucet waterControl;
	private TVControls tvControl;
	private ToiletControl toiletControl;

	private bool holding; //is something being interacted with

	void Start() {
		holding = false; //holding nothing at the start
		timeStamp =  (int)System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		cameraTransform = GameObject.FindWithTag("MainCamera").transform;
		StartCoroutine ("testHit"); //detects object being looked at
	}

	//Always runing co-routine to test which object is being looked at
	IEnumerator testHit() {
		while (true) {
		if(!holding){
			rayDirection = cameraTransform.TransformDirection (Vector3.forward);
			rayStart = cameraTransform.position + rayDirection;	
			Debug.DrawRay (rayStart, rayDirection * length, Color.green);

			//if physics hit directly in front of center camera
			if (Physics.Raycast (rayStart, rayDirection, out hit, length)) {
				//if object is interactable and not already currently selected
				if(hit.collider.tag=="Interactable" && 
				   InteractableObject != hit.collider.gameObject){
					//update the symbol above the old object to deselect
					if(InteractableObject!=null){
						//Deselects the old object
						ClearOldObject();
//						Debug.Log ("Cleared old Object");
					}
					//completes all tasks associated with picking an object
					PickObject();

				}
			}
		} //end if holding

			yield return new WaitForSeconds(.1f);
		} //end while true
	//	yield return null;
	}

	//Deselect the old object, cleans its UI elements, send deactiviate commands
	public void ClearOldObject(){

		symb = InteractableObject.GetComponentInChildren<UpdateSymbol>();
		grab = InteractableObject.GetComponentInChildren<Grab>();
		door = InteractableObject.GetComponentInChildren<InteractDoor>();
		doorz = InteractableObject.GetComponentInChildren<InteractDoorZ>();
		slide = InteractableObject.GetComponentInChildren<Slide>();
		slidez = InteractableObject.GetComponentInChildren<SlideZ>();
		lightControl = InteractableObject.GetComponentInChildren<LightControl>();
		flameControl = InteractableObject.GetComponentInChildren<IgniteBurner>();
		waterControl = InteractableObject.GetComponentInChildren<WaterFaucet>();
		tvControl = InteractableObject.GetComponentInChildren<TVControls>();
		toiletControl = InteractableObject.GetComponentInChildren<ToiletControl>();
		
		if (symb != null) {
			symb.DeSelect ();
			InteractableObject=null;
			ObjectPosition=Vector3.zero;
			textbox.text="No Target";
		}
		if(grab!=null){
			grab.setInactive();
			ClearLeftImage();
		}
		if (door != null) {
			door.setInactive();
		}
		if (doorz != null) {
			doorz.setInactive();
		}
		if (slide != null) {
			slide.setInactive();
		}
		if (slidez != null) {
			slidez.setInactive();
		}
		if (lightControl != null) {
			lightControl.setInactive();
		}
		if (flameControl != null) {
			flameControl.setInactive();
		}
		if (waterControl != null) {
			waterControl.setInactive();
		}
		if (tvControl != null) {
			tvControl.setInactive();
		}
		if (toiletControl != null) {
			toiletControl.setInactive();
		}
	}

	//Selects objects, updates symbols and UI, sends commands
	public void PickObject(){

		//Select the object, save its position
		//		Debug.Log (hit.collider.gameObject);
		textbox.text=hit.collider.gameObject.name;
		InteractableObject=hit.collider.gameObject;
		ObjectPosition=InteractableObject.transform.position;
		
		//update the symbol above the object
		symb = InteractableObject.GetComponentInChildren<UpdateSymbol>();
		if (symb != null) {
			symb.Select ();
		}
		//update the command list, send command signals.
		grab = InteractableObject.GetComponentInChildren<Grab>();
		if(grab!=null){
			SetLeftImage(grabSprite); //TODO move this to grab class
			grab.setActive();
		}

		door = InteractableObject.GetComponentInChildren<InteractDoor>();
		if (door != null) {
			door.setActive();
		}

		doorz = InteractableObject.GetComponentInChildren<InteractDoorZ>();
		if (doorz != null) {
			doorz.setActive();
		}

		slide = InteractableObject.GetComponentInChildren<Slide>();
		if (slide != null) {
			slide.setActive();
		}

		slidez = InteractableObject.GetComponentInChildren<SlideZ>();
		if (slidez != null) {
			slidez.setActive();
		}

		lightControl = InteractableObject.GetComponentInChildren<LightControl>();
		if (lightControl != null) {
			lightControl.setActive();
		}

		flameControl = InteractableObject.GetComponentInChildren<IgniteBurner>();
		if (flameControl != null) {
			flameControl.setActive();
		}

		waterControl = InteractableObject.GetComponentInChildren<WaterFaucet>();
		if (waterControl != null) {
			waterControl.setActive();
		}

		tvControl = InteractableObject.GetComponentInChildren<TVControls>();
		if (tvControl != null) {
			tvControl.setActive();
		}

		toiletControl = InteractableObject.GetComponentInChildren<ToiletControl>();
		if (toiletControl != null) {
			toiletControl.setActive();
		}


		StartCoroutine ("OutofRange"); //clears object when range is exceeded
		//		Debug.Log (ObjectPosition);
	}

	IEnumerator OutofRange(){
		GameObject original = InteractableObject;
		//escape if the interactable object changes and terminate.
		while (true && InteractableObject==original) {
			PlayerPosition=gameObject.transform.position;
				if(!holding && Vector3.Distance(ObjectPosition,PlayerPosition)>length+5f){
					ClearOldObject();
				}
			yield return new WaitForSeconds(.9f);
		}
		yield break;
	}

	//allows other methods to tell this method when a grab is initiated
	public void SetHolding(){ holding = true; }
	public void FreeHolding(){ holding = false;}

	public void SetLeftImage(Sprite theSprite){
		leftImage.sprite=theSprite;}
	public void ClearLeftImage(){
		leftImage.sprite=transparent;}

	public void SetRightImage(Sprite theSprite){
		rightImage.sprite=theSprite;}
	public void ClearRightImage(){
		rightImage.sprite=transparent;}

	public void SetTopImage(Sprite theSprite){
		topImage.sprite=theSprite;}
	public void ClearTopImage(){
		topImage.sprite=transparent;}

	public void SetBottomImage(Sprite theSprite){
		bottomImage.sprite=theSprite;}
	public void ClearBottomImage(){
		bottomImage.sprite=transparent;}

	public void SetExtraImage1(Sprite theSprite){
		extraImage1.sprite=theSprite;}
	public void ClearExtraImage1(){
		extraImage1.sprite=transparent;}

	public void writeFile(string actionName, string objectName, string location, string rotation, string special){
		string Filename = "Record" +  timeStamp.ToString() + ".txt";
		using (StreamWriter sw = new StreamWriter(Filename, true)) 
		{
			// Add some text to the file.
			sw.Write(actionName);
			sw.Write(" ");
			sw.Write(objectName);
			sw.Write(" at: ");
			sw.Write (location);
			sw.Write(" rotation: ");
			sw.Write (rotation);
			sw.Write(" ");
			sw.Write (special);
			sw.Write(" time=");
			sw.Write ( (int)System.DateTime.Now.TimeOfDay.TotalMilliseconds-timeStamp );
			sw.WriteLine("");
		}
	}
}
