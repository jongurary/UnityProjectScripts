using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCamera : MonoBehaviour {

	public int xMin;
	public int xMax;
	public int zMin;
	public int zMax;
	public int yMin;
	public int yMax;

	[Tooltip("Speed of panning camera left/right/up/down.")]
	public float scrollSpeed;
	[Tooltip("Speed of zooming in/out, should be set significantly faster than scrollSpeed in most instances.")]
	public float zoomSpeed;
	[Tooltip("Speed of rotation, should be set very low in most instances.")]
	public float rotationSpeed;

	private float PanPercCut = .01f; // percent width of the screen used to pan. For example, .02 will use the last 2% of the screen to pan
	private CursorLockMode wantedMode;
	private bool SecretGUIOn = false;
	private bool edgeSnap; //if true, camera jumps to the other side of the map when panned to the extreme of one side. If false, just stops.
	private float FovValue = 60f;

	void Start () {
		Cursor.lockState = CursorLockMode.Confined;
	}

	void Update () {

		//Right-left camera pan
		if (Input.GetKey ("right") || (Input.mousePosition.x >= Screen.width * (1-PanPercCut) && Input.mousePosition.x <= Screen.width) ) {
			transform.position += Vector3.right * Time.deltaTime * scrollSpeed;
			if(transform.position.x  > xMax){
				if(edgeSnap){ //if edgesnap engaged, jump to other side of the map
					transform.position = new Vector3( xMin, transform.position.y, transform.position.z );
				}else{ //otherwise move back
					transform.position += Vector3.left * Time.deltaTime * scrollSpeed * 2f;
				}
			}
			
		} else if (Input.GetKey ("left") || (Input.mousePosition.x <= Screen.width * (PanPercCut) && Input.mousePosition.x >= 0) ) {
			transform.localPosition += Vector3.left * Time.deltaTime * scrollSpeed;
			if(transform.position.x  < xMin){
				if(edgeSnap){ //if edgesnap engaged, jump to other side of the map
					transform.position = new Vector3( xMax, transform.position.y, transform.position.z );
				}else{ //otherwise move back
					transform.position += Vector3.right * Time.deltaTime * scrollSpeed * 2f;
				}
			}
		}

		//Up-down camera pan
		//TODO restrict access to the top control bar area in windowed  mode
		if (Input.GetKey ("up") || (Input.mousePosition.y >= Screen.height * (1-PanPercCut) && Input.mousePosition.y <= Screen.height) ) {
			transform.localPosition += Vector3.forward * Time.deltaTime * scrollSpeed;
			if(transform.position.z  > zMax){
				if(edgeSnap){ //if edgesnap engaged, jump to other side of the map
					transform.position = new Vector3( transform.position.x, transform.position.y, zMin );
				}else{ //otherwise move back
					transform.position += Vector3.back * Time.deltaTime * scrollSpeed * 2f;
				}
			}
			
		} else if (Input.GetKey ("down") || (Input.mousePosition.y <= Screen.height * (PanPercCut) && Input.mousePosition.y >= 0) ) {
			transform.localPosition += Vector3.back * Time.deltaTime * scrollSpeed;
			if(transform.position.z  < zMin){
				if(edgeSnap){ //if edgesnap engaged, jump to other side of the map
					transform.position = new Vector3( transform.position.x, transform.position.y, zMax );
				}else{ //otherwise move back
					transform.position += Vector3.forward * Time.deltaTime * scrollSpeed * 2f;
				}
			}
		}

		//Zoom in (move camera inwards)
		if (Input.GetAxis ("Mouse ScrollWheel") > 0 && transform.position.y > yMin) {
			transform.localPosition += Vector3.down * Time.deltaTime * zoomSpeed;
		}

		//Zoom out (move camera outwards)
		if (Input.GetAxis ("Mouse ScrollWheel") < 0 && transform.position.y < yMax) {
			transform.localPosition += Vector3.up * Time.deltaTime * zoomSpeed;
		}

		//Rotate Camera left/right
		if (Input.GetKey ("left ctrl") && Input.GetMouseButton (1)) {
			//Note: Variable names are pretty paradoxical here
			float xRotation = rotationSpeed * Input.GetAxis ("Mouse Y");
			transform.Rotate (xRotation, 0, 0);
		//	Debug.Log (transform.eulerAngles.x);
			//Clamp rotations
			if(transform.eulerAngles.x <= 40){
				transform.eulerAngles = new Vector3(40f, 0f, 0f);
			}
			if(transform.eulerAngles.x >= 80){
				transform.eulerAngles = new Vector3(80f, 0f, 0f);
			}
		}
		if (Input.GetKey ("left ctrl") && Input.GetKey ("q")) {
			transform.Rotate(new Vector3(0f, 0f, rotationSpeed));
			if(transform.eulerAngles.z >= 45 && transform.eulerAngles.z<=315){
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 45f);
			}
		}
		if (Input.GetKey ("left ctrl") && Input.GetKey ("e")) {
			transform.Rotate(new Vector3(0f, 0f, -rotationSpeed));
			if(transform.eulerAngles.z <= 315 && transform.eulerAngles.z>=45){
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 315f);
			}
		}

		//Engages the secret camera settings menu
		if (Input.GetKeyDown (KeyCode.Escape)) {
			SecretGUIOn = !SecretGUIOn ;
		}
			
	}
	

	void OnGUI()
	{
		//Secret GUI menu enabled via escape key, controls cursor constraint and speed of various pan/zooms
		if (SecretGUIOn) {
			bool isHovering = false;
			GUILayout.BeginArea(new Rect(10, 10, 200, 500));
			//Handles constraining the cursor to the screen and releasing it
			GUILayout.BeginVertical ();
			switch (Cursor.lockState) {
			case CursorLockMode.None:
				if (GUILayout.Button ("Confine Cursor to Screen")){
					wantedMode = CursorLockMode.Confined;}
				break;
			case CursorLockMode.Confined:
				if (GUILayout.Button ("Release Cursor")){
					wantedMode = CursorLockMode.None;}
				break;
			}
			GUILayout.EndVertical ();
			Cursor.lockState = wantedMode;


			//Handles increased or decreasing scoll speed
			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal();
			GUILayout.Box("Scroll Speed");
				float newScrollSpeed = GUILayout.HorizontalSlider(scrollSpeed, 10F, 250F);
				scrollSpeed = newScrollSpeed;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical ();

			//Handles increased or decreasing zoom
			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal();
			GUILayout.Box("Zoom Speed");
				float newZoomSpeed = GUILayout.HorizontalSlider(zoomSpeed, 50F, 350F);
				zoomSpeed = newZoomSpeed;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical ();

			//Handles edge snap or simple stop
			GUILayout.BeginVertical ();
			string extraText;
			if(edgeSnap){
				extraText="On";
			}else{
				extraText="Off";
			}
			if (GUILayout.Button ("Edge Snap " + extraText )){
				edgeSnap = !edgeSnap;
			}
			GUILayout.EndVertical ();


			//Handles FoV adjustments
			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal();
				GUILayout.Box("FoV");
				FovValue = GUILayout.HorizontalSlider(FovValue, 40.0F, 90.0F);
				GetComponent<Camera>().fieldOfView = FovValue;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical ();

			GUILayout.EndArea();
		}
	}
}
