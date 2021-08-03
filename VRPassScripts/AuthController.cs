using UnityEngine;
using System.Collections;

public class AuthController : MonoBehaviour {

	public string password;
	public string RecordedPassword;
	public string[] buttonText= new string[9];
	public GameObject projectile;
	public GameObject heldprojectile;
	public GameObject character;
	public Texture2D crosshair;
	public GameObject MatchMessage;
	public GameObject AuthenticationObject;
	public GameObject[] LookTargets = new GameObject[10];
	public Vector3[] MoveTargets = new Vector3[10];


	private bool hasball=true; //is the ball available for an action
	private bool recordPassword=false; //recording password entry
	private bool testing=false; //testing a recorded password
	private int LookIndex=0; //index of the looktargets array, marks which target is next
	private int MoveIndex=0; //index of the movetargets array, marks which target is next
	private float tolerance = 5f; //acceptable distance from move targets
	

	void Start () {

	}

	void Update () {

		if (Input.GetMouseButtonDown (0)) {
			if((recordPassword || testing) && hasball){
			//	object control should handle this
			}
			StartCoroutine(ThrowBallandRespawn(1.5f));
		}

		if(Input.GetKeyDown("z")){
			if((recordPassword || testing) && hasball){
				password=password+"z";
			}
			StartCoroutine(TossBallUpandDown());
		}

		if(Input.GetKeyDown ("x")){
			if((recordPassword || testing) && hasball){
				password=password+"x";
			}
			StartCoroutine (PassBallBetweenHands());
		}

		if(Input.GetKeyDown ("c")){
			if(recordPassword || testing){
				StartCoroutine (LookPoint());
			}
		}

		if(Input.GetKeyDown ("v")){
			if(recordPassword || testing){
				StartCoroutine (MovePoint());
			}
		}

		if (Input.GetKeyDown ("1")) {
			recordPassword=true;
			password="";
			LookIndex=0;
			MoveIndex=0;
		}

		if (Input.GetKeyDown ("2")) {
			recordPassword=false;
			RecordedPassword = password;
		//	Debug.Log("save");
		}
		if (Input.GetKeyDown ("3")) {
			testing=true;
			password="";
			LookIndex=0;
			MoveIndex=0;
			//	Debug.Log("save");
		}

		if (Input.GetKeyDown ("4")) {
			if(password.Equals(RecordedPassword)){
				testing=false;
				password="";
				LookIndex=0;
				MoveIndex=0;
				Instantiate(MatchMessage, character.transform.position + character.transform.forward * 5,
				            Quaternion.Euler(-90f, 0, 0));
			}
		}

		if (Input.GetKeyDown ("5")) {
			AuthenticationObject.GetComponent<MasterAuthObjectControl>().Scramble();
		}
	
	}

	void OnGUI(){

		GUI.DrawTexture(new Rect(Screen.width/2, Screen.height/2 , 
		                 Screen.width/64, Screen.width/64), crosshair);


		GUI.Box(new Rect(0f, Screen.height-Screen.height/4-Screen.height/32, 
		                 Screen.width/4, Screen.height/4+Screen.height/32)," Toolbox");

		Rect GUIbutton;
		float offwidth = Screen.width/256; //horizontal spacing between buttons
		float offheight = Screen.height/128; //vertical spacing between buttons
		float buttonwidth = Screen.width/16;
		float buttonheight = Screen.height/16;
		float corner=Screen.height-4*(buttonheight); //top left of menu box

		GUI.color = new Color (200, 200, 200, 1f);
		//Generate button array
		for(int i=1; i<4; i++){
			for(int j=1; j<4; j++){
				int buttonnumber=(j-1) + (i-1)*3;
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonText[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
					if(buttonnumber==0){
						password="";
						recordPassword=true;
					}else if(buttonnumber==1){
						recordPassword=false;
						RecordedPassword = password;
					}else if(buttonnumber==3){
						testing=true;
						password="";
					}else if(buttonnumber==4){
						if(password.Equals(RecordedPassword)){
							testing=false;
							password="";
							Instantiate(MatchMessage, character.transform.position + character.transform.forward * 5,
							            Quaternion.Euler(-90f, 0, 0));
						}
					}else if(buttonnumber==6){
						AuthenticationObject.GetComponent<MasterAuthObjectControl>().Scramble();
					}

					}// end button code
						
				} //end inner for loop
			} //end for loops

	}

	IEnumerator ThrowBallandRespawn (float waittime){
		if(hasball){
			hasball=false;
			heldprojectile.GetComponent<Renderer> ().enabled = false;
			yield return new WaitForSeconds(waittime/4);

			Vector3 offset = new Vector3(0f, 1f, 0f);
			GameObject proj =  (GameObject) Instantiate(projectile, character.transform.position+offset, Quaternion.identity);
			Rigidbody projectilebody;
			projectilebody = proj.GetComponent<Rigidbody> ();
			projectilebody.AddForce(character.transform.forward * 1000);

			yield return new WaitForSeconds(waittime);
			heldprojectile.GetComponent<Renderer> ().enabled = true;
			hasball=true;
		}
		yield return null;

	}

	IEnumerator TossBallUpandDown (){
		int count = 0;
		if (hasball) {
			hasball=false;
			while (count<20) { 
				yield return new WaitForSeconds (.01f);
				heldprojectile.transform.Translate (Vector3.up / 10);
				count++;
			}
			count = 0;

			while (count<20) { 
				yield return new WaitForSeconds (.01f);
				heldprojectile.transform.Translate (Vector3.down / 10);
				count++;
			}
			hasball=true;
		}
		yield return null;
	}

	IEnumerator PassBallBetweenHands (){
		int count = 0;
		if (hasball) {
			hasball=false;
			while (count<20) { 
				yield return new WaitForSeconds (.01f);
				if(count<10){
					heldprojectile.transform.Translate (Vector3.up / 20);
				}else {
					heldprojectile.transform.Translate (Vector3.down / 20);
				}
				heldprojectile.transform.Translate (Vector3.left / 10);
				count++;
			}
			count = 0;
			
			while (count<20) { 
				yield return new WaitForSeconds (.01f);
				if(count<10){
					heldprojectile.transform.Translate (Vector3.up / 20);
				}else {
					heldprojectile.transform.Translate (Vector3.down / 20);
				}
				heldprojectile.transform.Translate (Vector3.right / 10);
				count++;
			}
			hasball=true;
		}
		yield return null;
	}

	IEnumerator LookPoint (){
	//	Ray ray = Camera.main.ViewportPointToRay (new Vector3(0.5f,0.5f,0f)); //same thing, more voodoo
		Ray ray = Camera.main.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2, 0f));

		RaycastHit hit;
		if (Physics.Raycast (ray, out hit)) {
		//	Debug.DrawLine(ray.origin, ray.direction * 1000, Color.red, 500f, false);

			if(recordPassword){ //if we are recording password, add this object to array
			if(LookIndex<9){ //max size of the lookindex array
				LookTargets[LookIndex]=hit.collider.gameObject;
				LookIndex++;
				password = password + "c";
			}}

			if(testing){ //if we are testing the password, check against the array
			if(LookIndex<9){ //max size of the lookindex array
					if(hit.collider.gameObject == LookTargets[LookIndex]){
						LookIndex++;
						password = password + "c";
					}
			}}
		}
		yield return null;
	}

	IEnumerator MovePoint (){

		Vector3 charPosition = character.transform.position;

			if(recordPassword){ //if we are recording password, add this object to array
				if(MoveIndex<9){ //max size of the lookindex array
					MoveTargets[MoveIndex]=charPosition;
					MoveIndex++;
					password = password + "v";
				}}
			
			if(testing){ //if we are testing the password, check against the array
				if(MoveIndex<9){ //max size of the lookindex array
				if(charPosition == MoveTargets[MoveIndex]){
						MoveIndex++;
						password = password + "v";
					}
				}}
		yield return null;
	}

	public bool isrecord(){
		if (recordPassword) {
			return true;
		}
		return false;
	}

	public bool istesting(){
		if (testing) {
			return true;
		}
		return false;
	}

}
