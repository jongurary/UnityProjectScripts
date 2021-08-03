using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterSelector : MonoBehaviour {

	public Texture marqueeGraphics;
	public Texture2D cursorTexture;
	private Vector2 marqueeOrigin;
	private Vector2 marqueeSize;
	public Rect marqueeRect;
	private Rect backupRect;

	static private List<GameObject> Selectable = new List < GameObject > ();
	static private List<GameObject> Selected= new List < GameObject > ();

	public float SCANTIME; //how often to scan which units are under marquee, for lag throttle
	private bool scanlock;
	private bool suspended; // suspends all selection during targeting gestures
	GameObject priorityobject=null;
	
	void Start () {
		scanlock = false;
		suspended = false;
		Cursor.SetCursor (cursorTexture, new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.Auto);
	}

	private void OnGUI()
	{
		//Draw selector rectangle
		marqueeRect = new Rect(marqueeOrigin.x, marqueeOrigin.y, marqueeSize.x, marqueeSize.y);
		GUI.color = new Color(0, 0, 0, .3f);
		GUI.DrawTexture(marqueeRect, marqueeGraphics);
	}

	void Update () {
	//	Debug.Log (suspended);
		if(!suspended){
		bool inGUI = false;
		Rect GUIArea = new Rect (0f, (Screen.height-Screen.height/4)-15f, Screen.width/4, (Screen.height/4)+15f);
		//Ignore clicks in control area, if it's up
		if(Selected.Count>0){
			//this area should match the one in "Controls"
			if(GUIArea.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))){
				inGUI=true;
			}else{
				inGUI=false;
			}
		}

		if(!inGUI){
		//mouse released, find selected objects then remove gui selector
		if (Input.GetMouseButtonUp (0)) { //RELEASE

			//Don't bother unit scanning on a tiny marquee (e.g. a sloppy click)
			if (Mathf.Abs(marqueeRect.size.x) > 10 && Mathf.Abs(marqueeRect.size.y) > 10) {
				//lock prevents rescanning under lag if user spam clicks
				if(!scanlock){
					StartCoroutine(ScanForUnits(SCANTIME));
					scanlock=true;
				}else{ //clean up marquee in event of scanlock
					marqueeRect.width = 0;
					marqueeRect.height = 0;
					marqueeSize = Vector2.zero;
				}
				//Note: SelectUnits is invoked at the end of ScanForUnits
			}else{ //a short click sends a ray for single unit selections
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			//ignore shields (layer 14)
			int layerMask = Constants.IGNORE_RAYCAST_LAYERMASK;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){ //NOTE max distance can be set to <infinity, possibly with less perf impact?
				Debug.DrawLine(ray.origin, hit.point);

				GameObject unithit = hit.transform.root.gameObject;
				if( (unithit.tag == "Building" || unithit.tag== "Unit") ){
					//		Debug.Log("click: " + unithit);
					//		Selected.Clear (); //Should already be clear, this is a sanity check
							Selected.Add(unithit);
								//clear old gui object, set clicked object to new one
							if(priorityobject!=null){ //sanity checks in case something is already selected
								priorityobject.GetComponent<Controls>().Deprioritize();
								priorityobject.GetComponent<Controls>().Deselect();
								}
							priorityobject=unithit;
							priorityobject.GetComponent<Controls>().Prioritize();
							priorityobject.GetComponent<Controls>().Select();
					} //end if unit or building
				} //end raycast
				//Inform units they have been selected and build the GUI
				if(Selected.Count>0){
					StartCoroutine(SelectUnits());
					}
			}
		} else if (Input.GetMouseButtonDown (0)) { //CLICK
			//on left click, begin drawing the selection square
			float _invertedY = Screen.height - Input.mousePosition.y;
			marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);
			//Clean whatever was selected before in prep for new selections
			StartCoroutine(PurgeSelections());
		}
		
		if (Input.GetMouseButton (0)) { //CLICKHELD
			//on left click held, extend size of selection square
			float invertY = Screen.height - Input.mousePosition.y;
			marqueeSize = new Vector2 (Input.mousePosition.x - marqueeOrigin.x, (marqueeOrigin.y - invertY) * -1);

			if (marqueeRect.width < 0 && marqueeRect.height < 0){
				backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y - Mathf.Abs(marqueeRect.height), Mathf.Abs(marqueeRect.width), Mathf.Abs(marqueeRect.height));
			}else if (marqueeRect.width < 0){
				backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y, Mathf.Abs(marqueeRect.width), marqueeRect.height);
			}else if (marqueeRect.height < 0){
				backupRect = new Rect(marqueeRect.x, marqueeRect.y - Mathf.Abs(marqueeRect.height), marqueeRect.width, Mathf.Abs(marqueeRect.height));
			}else{ //if we don't need to draw a backrect, purge the old one.
				backupRect.width=0; backupRect.height=0;
			}
		}
			}} //end if suspended, if inGUI
	}

	//Converts 3d object positions into gui points and looks for which ones are in the marquee
	IEnumerator ScanForUnits (float scantime){
			yield return new WaitForSeconds(scantime);

			foreach (GameObject obj in Selectable) {
			//Emergency null check
			if(obj==null){
				//Selectable.Remove(obj); //don't do this, it causes an enumeration error
			}else{

			Vector3 objposition = Camera.main.WorldToScreenPoint(obj.transform.position);
			Vector2 position2d = new Vector2(objposition.x, Screen.height - objposition.y);

				if (marqueeRect.Contains(position2d) || backupRect.Contains(position2d)){
					Selected.Add(obj);
	//				Debug.Log("marqueeadd: " +obj + " at " + position2d);
				}
			}
			}
			scanlock=false;

		marqueeRect.width = 0;
		marqueeRect.height = 0;
		marqueeSize = Vector2.zero;
	//	Debug.Log ("Scanned " + Selected.Count + " Units");
		//Inform units they have been selected and build the GUI
		if(Selected.Count>0){
			StartCoroutine(SelectUnits());
		}

		yield return null;
	}

	//get all the units in the selection list, display their gui
	IEnumerator SelectUnits (){
		int maxpriority =0;
		foreach (GameObject obj in Selected) {
			Controls controls = obj.GetComponent<Controls>();
			controls.Select(); //informs the unit it's been selected
//			Debug.Log ("Selected " + obj);
			//finds which unit is top priority
			if(controls.priority>maxpriority){
				maxpriority=controls.priority;
				priorityobject= obj;
			}
		}
		priorityobject.GetComponent<Controls>().Prioritize(); //highest priority unit signaled
		//	Debug.Log (priorityobject);
		yield return null;
	}

	//informs objects they have been deselected and clears Selected array
	IEnumerator PurgeSelections (){
	//	Debug.Log ("Cleaned Selections");
		//No object is selected yet clear gui by clearing priority object
		if (priorityobject != null) {
			priorityobject.GetComponent<Controls> ().Deprioritize ();
			priorityobject = null;
		}

		//Deselect all objects
		foreach (GameObject obj in Selected) {
			if(obj!=null){
				Controls controls = obj.GetComponent<Controls>();
				controls.Deselect();
			}
		}

		//clear array
		Selected.Clear ();
		yield return null;
	}

	//if we get stuck in targeting mode somehow
	IEnumerator EmergencyUnsuspend (){
		yield return new WaitForSeconds(10f);
		if (Selected.Count == 0 && suspended) {
			suspended=false;
		}
		yield return null;
	}

	//Unsuspends after the left click is released
	IEnumerator Unsuspend(){
		yield return new WaitUntil ( () => !Input.GetMouseButton (0) == true );
		suspended = false;
		yield return null;
	}

	public void AddObject(GameObject obj){
		Selectable.Add (obj);
	}

	public void RemoveObject(GameObject obj){
		Selectable.Remove (obj);
		Selected.Remove (obj);
	}

	public void Suspend(){
		suspended = true;
		StartCoroutine (EmergencyUnsuspend ());
	}

	public void Resume(){
		StartCoroutine (Unsuspend ());
	}

	public bool hasSelection(){
		if (Selected.Count > 0) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// The number of selectable objects currently in the game
	/// </summary>
	public int getSelectableSize(){
		return Selectable.Count;
	}

	/// <summary>
	/// Return the selectable object at a given index. Checks if index is out of range. If it is, return the first object in selectable arbitrarily
	/// </summary>
	public GameObject getSelectableByIndex(int index){
		if (index < Selectable.Count && index > -1) {
			return Selectable [index];
		} else {
			return Selectable [0];
		}
	}

}
