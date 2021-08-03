using UnityEngine;
using System.Collections;

public class Controls : MonoBehaviour {

	private int status; //command type is passed to other classes using this variable

	public GameObject MainCam;
	public Renderer SelectionBox;
	private bool showGUI=false; //chief object in a selection gets to show its gui
	private bool selected=false; //is this object in selection
	public MasterSelector masterselector;
	public Texture2D targetcursor;
	public Terrain terrian;

	public int priority;
	public string objectName;
	public int objectTag; //determines which set of commands to run
	//Row 1 = 0-4, Row 2 = 5-9, etc, control if buttons are enabled and textures are on them
	public bool[] enabledButtons= new bool[15];
	public string[] buttonTitles = new string[15];
	public Texture[] buttonTextures = new Texture[15];

	//Black rect around gui
	private Rect GUIbackdrop;
	private Rect GUIbackdropright; //right side of the screen box
	private int masterCursorSize; //all cursors should scale from the master size
	private GUIStyle statusStyle = new GUIStyle();
	private GUIStyle mouseOverStyle = new GUIStyle();
	public Texture2D mouseOverBG; //pure black background for mouse over text
	//button parameters
	private float offwidth; //horizontal spacing between buttons
	private float offheight; //vertical spacing between buttons
	private float buttonwidth;
	private float buttonheight;
	private float corner; //top left of menu box
	private bool isHovering;
	private string displayHoverText;
	private int layerMask = Constants.IGNORE_RAYCAST_LAYERMASK;
	
	private bool targeting;

	private GameObject rangeIndicator; //range indication circle for links and such
	private bool alwaysOnIndicator; //allows certain units to always display the range indicator
	private ResourceManager manager;
	private bool healthOnlyDisplay; //allows the gui to display just the health of enemies


	
	void Start () {
		status = -1; //-1 is always the default error status
		//setup button params
		offwidth = Screen.width/(Constants.BUTTON_WIDTH_DIVISOR*8); //horizontal spacing between buttons
		offheight = Screen.height/(Constants.BUTTON_HEIGHT_DIVISOR*8); //vertical spacing between buttons
		buttonwidth = Screen.width/Constants.BUTTON_WIDTH_DIVISOR;
		buttonheight = buttonwidth; //Screen.height/Constants.BUTTON_HEIGHT_DIVISOR;
		isHovering = false;
		displayHoverText = "";
		corner=Screen.height-4*(buttonheight); //top left of menu box
		MainCam = Camera.main.gameObject;
		masterCursorSize = Screen.width / Constants.CURSOR_SIZE_DIVISOR;

		statusStyle.fontSize = Screen.width / (int)(Constants.FONT_SIZE_DIVISOR / .75);
		statusStyle.normal.textColor = Color.cyan;

		mouseOverStyle.fontSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2; //mouse over font should be small
		mouseOverStyle.normal.textColor = Color.cyan;
		mouseOverStyle.normal.background = mouseOverBG;
		mouseOverStyle.wordWrap = true; //mouseover text should word wrap to fit its box
		
		terrian = Terrain.activeTerrain;

		if (masterselector == null) {
			masterselector=MainCam.GetComponent<MasterSelector>();
		}

		masterselector.AddObject (gameObject); //register with the list of selectable objects

		//determine if the unit is an enemy and should remain uncontrollable
		healthOnlyDisplay = false;
		if (GetComponent<UnitLife> () != null) {
			if(GetComponent<UnitLife>().getOwner() != 1){
				healthOnlyDisplay = true;
			}
		}

		targeting = false;


		if (rangeIndicator == null) {
			rangeIndicator = GameObject.FindGameObjectWithTag("ControlsRangeIndicator");
		}
		if (manager == null) {
			manager = GameObject.FindGameObjectWithTag ("ResourceManager").GetComponent<ResourceManager> ();
		}
		StartCoroutine (updateRangeIndicator ());
		if (rangeIndicator != null) {
			rangeIndicator.GetComponent<Animator> ().speed = .2f;
		}
	}

	private void OnGUI(){

		if (showGUI && selected && !healthOnlyDisplay) {
			//Draw large background box
			GUIbackdrop = new Rect (0f, (Screen.height - Screen.height / 4) - Screen.height / 45, 
			                        Screen.width / 5f, (Screen.height / 4) + Screen.height / 45);
			GUIbackdropright = new Rect (Screen.width - Screen.width / 5f, (Screen.height - Screen.height / 4) - Screen.height / 45, 
			                        Screen.width / 5f, (Screen.height / 4) + Screen.height / 45);
			//		GUI.color = new Color (0, 0, 0, 1f);
			GUI.Box (GUIbackdrop, objectName);
			GUI.Box (GUIbackdropright, "");
			
			GUI.color = new Color (200, 200, 200, 1f);

			//Buttons depend on which object is chosen
			switch (objectTag) {
			case 5:
				GUIGenericTurret ();
				break;
			case 15: //transmission line
				GUIGenericGenerator ();
				break;
			case 16: //substation
				GUIGenericGenerator ();
				break;
			case 17: //capacitor
				GUIGenericGenerator ();
				break;
			case 18: //battery
				GUIGenericGenerator ();
				break;
			case 19: //wireless charger
				GUIGenericGenerator ();
				break;
			case 20: //shield
				GUIShield ();
				break;
			case 25: //generic power
				GUIGenericGenerator ();
				break;
			case 26: //wind
				GUIGenericGenerator ();
				break;
			case 27: //solar
				GUIGenericGenerator ();
				break;
			case 28: //geo
				GUIGeothermal ();
				break;
			case 32: //nuclear
				GUINuclear ();
				break;
			case 40: //metal mine
				GUIGenericResourceProducer ();
				break;
			case 41: //metal refine
				GUIGenericResourceProducer ();
				break;
			case 42: //ammo supplier
				GUIAmmoSupplier ();
				break;
			case 50:
				GUIDeliveryDrone ();
				break;
			case 95: //drone base
				GUIConstructor ();
				break;
			case 99: //nuclearlauncher
				GUINuclearLauncher ();
				break;
			case 100:
				GUIBase ();
				break;
			case 120: 
				GUICube ();
				break;
			case 125:
				GUIGenericDrone ();
				break;
			case 105: //orbital launcher
				GUIOrbitalLauncher ();
				break;
			default:
				break;
			}

		} else if (showGUI && selected && healthOnlyDisplay) {
			//Draw large background box
			GUIbackdrop = new Rect (0f, (Screen.height - Screen.height / 4) - Screen.height / 45, 
			                        Screen.width / 5f, (Screen.height / 4) + Screen.height / 45);
			GUIbackdropright = new Rect (Screen.width - Screen.width / 5f, (Screen.height - Screen.height / 4) - Screen.height / 45, 
			                             Screen.width / 5f, (Screen.height / 4) + Screen.height / 45);
			//		GUI.color = new Color (0, 0, 0, 1f);
			GUI.Box (GUIbackdrop, objectName);
			GUI.Box (GUIbackdropright, "");
			
			GUI.color = new Color (200, 200, 200, 1f);
			GUIHealthOnly();
		}
	}

	void Update () {

		if (selected && !healthOnlyDisplay) {
			switch (objectTag) {
			case 5: //gatling gun turret
				RunAsGenericTurret ();
				break;
			case 15: //transmission line
				RunAsGenericGenerator ();
				break;
			case 16: //substation
				RunAsGenericGenerator ();
				break;
			case 17: //capacitor
				RunAsGenericGenerator ();
				break;
			case 18: //battery
				RunAsGenericGenerator ();
				break;
			case 19: //wireless charger
				RunAsGenericGenerator ();
				break;
			case 20: //shield
				RunAsShield ();
				break;
			case 25:
				RunAsGenericGenerator ();
				break;
			case 26: //wind
				RunAsGenericGenerator ();
				break;
			case 27: //solar
				RunAsGenericGenerator ();
				break;
			case 28: //geo
				RunAsGenericGenerator ();
				break;
			case 32: //nuclear
				RunAsNuclear ();
				break;
			case 40: //metal mine
				RunAsGenericMine ();
				break;
			case 41: //metal refine
				RunAsGenericRefinery();
				break;
			case 42: //ammo supplier
				RunAsAmmoSupplier();
				break;
			case 50:
				RunAsDeliveryDrone ();
				break;
			case 95: //drone bay
				RunAsConstructor ();
				break;
			case 99: //nuclearlauncher
				RunAsNuclearLauncher ();
				break;
			case 100:
				RunAsBase ();
				break;
			case 120:
				RunAsCube ();
				break;
			case 125:
				RunAsGenericDrone();
				break;
			case 105:
				RunAsOrbitalLauncher ();
				break;
			default:
				break;
			}
			if (!targeting) {
				if(rangeIndicator!=null){ 
					if(rangeIndicator.GetComponent<Renderer>().enabled && !alwaysOnIndicator && selected){
						rangeIndicator.GetComponent<Renderer>().enabled = false; //disable range indicator if not targeting
					}
				}
				if ( Input.GetKeyDown ("a") ) {
					if(objectTag == 105 || objectTag == 20 || objectTag == 42){
						//Handle these in "runas" methods
					}else{
						status = 1;
						masterselector.Suspend ();
						targeting = true;
					}
				}else if( Input.GetKeyDown ("s") ) {
					if(objectTag == 105 || objectTag == 20 || objectTag == 42){
						//Handle these in "runas" methods
					}else{
						status = 2;
						masterselector.Suspend ();
						targeting = true;
					}
				}else if( Input.GetKeyDown ("d") ) {
					if(objectTag == 105 || objectTag == 20 || objectTag == 42){
						//Handle these in "runas" methods
					}else{
						status = 3;
						masterselector.Suspend ();
						targeting = true;
					}
				}
			}
		}
	
	}

	IEnumerator updateRangeIndicator(){
		while (true) {
			yield return new WaitForSeconds(.2f);
			if(targeting ||
			   	(alwaysOnIndicator && selected) ){ //certain units have an always-on range indicator
				if(rangeIndicator!=null){
					if(!rangeIndicator.GetComponent<Renderer>().enabled){
						rangeIndicator.GetComponent<Renderer>().enabled = true;
						rangeIndicator.transform.position = transform.position + new Vector3(0f, 1f, 0f); //for circle location determination
					}
				}
				
				float rangeSize=0f;
				//TODO possible inefficiency, determine unit type using builder or unit id
				//SLIGHT BUG: The very first selection of an ammo supplier does not trigger the range indicator
				if(GetComponent<AmmoRefine>()!=null){ //having an AmmoRefine component means a child must have the trigger
					rangeSize = GetComponentInChildren<AmmoRefineTrigger>().range * 2f;
				}
				else if(GetComponent<Linkage>()!=null){
					rangeSize = GetComponent<Linkage>().linkRange * 2f;
				}

				if(rangeIndicator!=null){
					rangeIndicator.transform.localScale = new Vector3( rangeSize, rangeSize, rangeSize );
				}
			}
		}
	}

	#region RunAs operations
	void RunAsGenericTurret(){
		//right click action
		if (Input.GetMouseButtonDown (1)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
				//does nothing
				StartCoroutine(FreeMasterSelector());
			}
		}
		
		if (targeting) { //targeting attack-move position
			if (Input.GetMouseButtonDown (0)) { //TODO this should order an attack-move vs a move
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
					//does nothing
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				}
			}
		}
	}

	void RunAsCube(){
		//right click to move
			if (Input.GetMouseButtonDown (1)) {
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
					gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (hit.point);
					StartCoroutine(FreeMasterSelector());
				}
			}
		
			if (targeting) { //targeting attack-move position
				if (Input.GetMouseButtonDown (0)) { //TODO this should order an attack-move vs a move
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
						gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (hit.point);
						targeting = false;
						StartCoroutine(FreeMasterSelector());
					}
				}
			}
	}

	void RunAsGenericDrone(){
		//right click to move, or attack unit if in range
		if (Input.GetMouseButtonDown (1)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
				GameObject unithit = hit.transform.root.gameObject;
				//Debug.Log ("hit : " + unithit);
				bool canBuild=true;
				
				if(unithit.CompareTag("Terrian") ){
				//	Debug.Log ("setting path to : " + hit.point);
					gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (hit.point);
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				}
				//if a unit if right-clicked
				else if( unithit.CompareTag("Unit") || unithit.CompareTag("Building") ){
					//if the unit is in range, set it as a target, if not, set as navmesh target
					if( GetComponent<UnitAttack>() != null){
					//	Debug.Log ("attempted to over-write target: " + unithit);
						if(!GetComponent<UnitAttack>().overrideTarget(unithit)){
							gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (hit.point);
						}
					}else if( GetComponent<UnitAttackLaser>() != null){
						if(!GetComponent<UnitAttackLaser>().overrideTarget(unithit)){
							gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (hit.point);
						}
					}
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				}else{
				//	Debug.Log ("hit something odd : " + unithit);
				}
			}
		}
		
		if (targeting) { //targeting attack-move position
			if (Input.GetMouseButtonDown (0)) { //TODO this should order an attack-move vs a move

				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
					gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (hit.point);
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				}
			}

			}

	}

	void RunAsBase(){
			if (targeting) { //targeting a bombing run, await target
				if (Input.GetMouseButtonDown (1)) {
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				} else if (Input.GetMouseButtonDown (0)) {
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
						if(GetComponent<UnitBuilder>().Slave1!=null){ //has a slaved unit
						if(GetComponent<UnitBuilder>().Slave1.GetComponent<BomberControls> () != null) { //sanity check for recent destroy
							GetComponent<UnitBuilder>().Slave1.GetComponent<BomberControls> ().AssignMission (hit.point);
						}
						} //doesn't have a slaved unit, do nothing
						targeting = false;
						StartCoroutine(FreeMasterSelector());
					}
				}
			}
		if (!targeting) {
			if (Input.GetKeyDown ("z")) {
				//Bomber = status 0
				if(checkCostLocallyandDeduct(new int[]{0,500,1000,500,0,1500})){
					GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 0);
				}
			}else if (Input.GetKeyDown ("x")) {
				//Fighter = status 1
				if(checkCostLocallyandDeduct(new int[]{0,500,1000,500,0,1500})){
					GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 1);
				}
			}
		}
	}

	void RunAsConstructor(){ //same as a base, but doesn't slave units after construction
		if (targeting) { //targeting a bombing run, await target
			if (Input.GetMouseButtonDown (1)) {
				targeting = false;
				StartCoroutine(FreeMasterSelector());
			} else if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
					//Does nothing
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				}
			}
		}

		if (!targeting) {
			if (Input.GetKeyDown ("z")) {
				//Drone = status 0
					if(checkCostLocallyandDeduct(new int[]{0,500,500,0,0,500})){
						GetComponent<UnitBuilder>().BuildUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 0);
					}
			}else if (Input.GetKeyDown ("x")) {
				//Tank = status 1
					if(checkCostLocallyandDeduct(new int[]{0,3000,1000,500,0,2000})){
						GetComponent<UnitBuilder>().BuildUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 1);
					}
			}
		}
	}

	void RunAsNuclearLauncher(){ //builds missiles
		if (targeting) { //targeting a missile fire, awaiting target
			if (Input.GetMouseButtonDown (1)) {
				targeting = false;
				StartCoroutine(FreeMasterSelector());
			} else if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
					if(GetComponent<UnitBuilder>().Slave1!=null){ //has a slaved unit
						NukeControl nukeCon = GetComponent<UnitBuilder>().Slave1.GetComponent<NukeControl>();
						if( nukeCon != null ) { //sanity check for recent destroy
							nukeCon.initiateLaunch=true;
							nukeCon.target=hit.point;
						}
						if( GetComponent<HatchController>()!=null ){
							GetComponent<HatchController>().openHatch();
						}
					} //doesn't have a slaved unit, do nothing
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				}
			}
		}
		if (!targeting) {
			if (Input.GetKeyDown ("z")) {
				GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, -8f, 0f), Quaternion.Euler(-90f,0f,0f), 0);
			}else if (Input.GetKeyDown ("x")) {
				GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, -8f, 0f), Quaternion.Euler(-90f,0f,0f), 1);
			}
		}
	}

	void RunAsOrbitalLauncher(){ //builds orbital rockets
		if (targeting) { //targeting a missile fire, awaiting target
			if (Input.GetMouseButtonDown (1)) {
				targeting = false;
				StartCoroutine(FreeMasterSelector());
			} else if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (terrian.GetComponent<Collider> ().Raycast (ray, out hit, Mathf.Infinity)) {
					//do nothing
					targeting = false;
					StartCoroutine(FreeMasterSelector());
				}
			}
		}
		if (!targeting) {
			if ( Input.GetKeyDown ("a") && GetComponent<UnitBuilder>().Slave1 != null ) {
				NukeControl nukeCon = GetComponent<UnitBuilder>().Slave1.GetComponent<NukeControl>();
				if( nukeCon != null  //sanity check for recent destroy
				   && !nukeCon.initiateLaunch ) { 
					nukeCon.initiateLaunch=true;
					nukeCon.target=new Vector3(transform.position.x, 100f, transform.position.z); //point it into space
					GetComponent<UnitBuilder>().Slave1.GetComponent<DestroyByTime>().dontKill = false;
					ResourceManager resManager = GameObject.FindGameObjectWithTag("ResourceManager").GetComponent<ResourceManager>();
					ResourceControl resControl = GetComponent<ResourceControl>();
					for(int k=0; k<resControl.getInputCount(); k++){
						int toSend, maxToSend;
						if(resControl.getInputType(k)=="Steel"){
							maxToSend=4000;
						} else if(resControl.getInputType(k)=="Enriched Uranium"){
							maxToSend=100;
						}else if (resControl.getInputType (k) == "Exotics") {
							maxToSend = 2000;
						}else if (resControl.getInputType (k) == "Fuel") {
							maxToSend = 2000;
						}
						else{
							maxToSend=10;
						}
						if(resControl.getCurrentInputResource(k) > maxToSend){
							toSend = maxToSend;
						}else{
							toSend = resControl.getCurrentInputResource(k);
						}
						resControl.drainInput(toSend, k);
						resManager.sendResourceWithTag(resControl.getInputType(k),toSend);
					}
				}
			}else if(Input.GetKeyDown ("s") && GetComponent<SpaceElevator>().hasSpaceElevator){
				//Pause Elevator
				GetComponent<SpaceElevator>().spaceElevatorPaused = !(GetComponent<SpaceElevator>().spaceElevatorPaused);
			}else if(Input.GetKeyDown ("d") && GetComponent<SpaceElevator>().hasSpaceLaser){
				//Pause Laser
				GetComponent<SpaceElevator>().spaceLaserPaused = !(GetComponent<SpaceElevator>().spaceLaserPaused);
			}else if(Input.GetKeyDown ("z")){
				//Orbital rocket
				if(checkCostLocally(new int[]{0,100,200,0,0,500}) && GetComponent<UnitBuilder>().Slave1 == null){
					checkCostLocallyandDeduct(new int[]{0,100,200,0,0,500});
					GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, -8f, 0f), Quaternion.Euler(-90f,0f,0f), 0);
				}
			}else if(Input.GetKeyDown ("x") && !(GetComponent<SpaceElevator>().hasSpaceElevator) && !(GetComponent<SpaceElevator>().hasSpaceLaser)){
				//Build a space elevator
				if(checkCostLocallyandDeduct(new int[]{0,5000,1000,2000,0,5000})){
					GetComponent<SpaceElevator>().buildSpaceElevator();
				}
			}else if(Input.GetKeyDown ("c") && !(GetComponent<SpaceElevator>().hasSpaceElevator) && !(GetComponent<SpaceElevator>().hasSpaceLaser)){
				//Build a space laser
				if(checkCostLocallyandDeduct(new int[]{0,1000,1000,4000,0,5000})){
					GetComponent<SpaceElevator>().buildSpaceLaser();
				}
			}
		}
	}
	
	void RunAsGenericGenerator(){

		//right click to free, this is building it doesn't move
		if (Input.GetMouseButtonDown (1)) {
			targeting=false;
			StartCoroutine(FreeMasterSelector());
		}
		
		if (targeting) { //click target to establish link
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
					GameObject unithit = hit.transform.root.gameObject;
					if(unithit.GetComponent<Linkage>()!=null){
						GetComponent<Linkage>().issueCommand(status, unithit);
					//	Debug.Log("estalbished link from: " + gameObject + " " + unithit);
						targeting=false;
						StartCoroutine(FreeMasterSelector());
						//TODO target has no available links?
					}else{ //not a linkable object, TODO maybe display an error?
						//	Debug.Log("Not a linkable object!");
						targeting=false;
						StartCoroutine(FreeMasterSelector());
					}
				}
			}
		}
	}

	void RunAsNuclear(){
		
		//right click to free, this is building it doesn't move
		if (Input.GetMouseButtonDown (1)) {
			targeting=false;
			StartCoroutine(FreeMasterSelector());
		}
		
		if (targeting) { //click target to establish link
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
					GameObject unithit = hit.transform.root.gameObject;
					if(unithit.GetComponent<Linkage>()!=null){
						GetComponent<Linkage>().issueCommand(status, unithit);
						//	Debug.Log("estalbished link from: " + gameObject + " " + unithit);
						targeting=false;
						StartCoroutine(FreeMasterSelector());
						//TODO target has no available links?
					}else{ //not a linkable object, TODO maybe display an error?
						//	Debug.Log("Not a linkable object!");
						targeting=false;
						StartCoroutine(FreeMasterSelector());
					}
				}
			}
		}

		if (Input.GetKeyDown ("z")) {
			GetComponent<NuclearPower>().overheatReactor();
				//TODO deduct cost
		}
		if (Input.GetKeyDown ("x") && GetComponent<NuclearPower>().isMeltingDown) {
			GetComponent<NuclearPower>().ventReactor();
		}
	}

	void RunAsShield(){
		
		//right click to free, this is building it doesn't move
		if (Input.GetMouseButtonDown (1)) {
			targeting = false;
			StartCoroutine (FreeMasterSelector ());
		}
		
		if (targeting) { //click target to establish link
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
					GameObject unithit = hit.transform.root.gameObject;
					if (unithit.GetComponent<Linkage> () != null) {
						//Do nothing
						targeting = false;
						StartCoroutine (FreeMasterSelector ());
					} else { 
						//Do nothing
						targeting = false;
						StartCoroutine (FreeMasterSelector ());
					}
				}
			}
		}

		if (Input.GetKeyDown ("a")) {
			if (GetComponentInChildren<ShieldManager> ().boostOverclock ()) { //can only be a certain number of times
				//TODO deduct cost
			}
		} else if (Input.GetKeyDown ("s")) {
			GetComponentInChildren<ShieldManager> ().toggleShield ();
		}
	}

	void RunAsGenericMine(){
		
		//right click to free, this is a building it doesn't move
		if (Input.GetMouseButtonDown (1)) {
			targeting=false;
			StartCoroutine(FreeMasterSelector());
		}
		
		if (targeting) { //establish a truck link to a reciever
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
					GameObject unithit = hit.transform.root.gameObject;
					if(unithit.GetComponent<ResourceControl>()!=null){ //target needs to have a resource controller
						if(GetComponent<ResourceMine>()!=null){
							if(GetComponent<ResourceMine>().doesStatusExist(status)){ //is this status command actually supported by the unit
								if(checkCostLocallyandDeduct(new int[]{0,0,0,0,0,500})){ //can afford/deduct resources
									GetComponent<ResourceMine>().issueCommand(status, unithit);
								}
							}
						}
						//	Debug.Log("established truck link from: " + gameObject + " " + unithit);
						targeting=false;
						StartCoroutine(FreeMasterSelector());
						//TODO target has no available links?
					}else{ //not a linkable object, TODO maybe display an error?
						//	Debug.Log("Not a linkable object!");
						targeting=false;
						StartCoroutine(FreeMasterSelector());
					}
				}
			}
		}
	}

	void RunAsGenericRefinery(){
		
		//right click to free, this is a building it doesn't move
		if (Input.GetMouseButtonDown (1)) {
			targeting=false;
			StartCoroutine(FreeMasterSelector());
		}
		
		if (targeting) { //establish a truck link to a reciever
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
					GameObject unithit = hit.transform.root.gameObject;
					if(unithit.GetComponent<ResourceControl>()!=null){ //target needs to have a resource controller
						if(GetComponent<ResourceRefine>()!=null){
							if(GetComponent<ResourceRefine>().doesStatusExist(status)){ //is this status command actually supported by the unit
								if(checkCostLocallyandDeduct(new int[]{0,0,0,0,0,500})){ //can afford/deduct resources
									GetComponent<ResourceRefine>().issueCommand(status, unithit);
								}
							}
						}
						//TODO send exotics truck
						//	Debug.Log("established truck link from: " + gameObject + " " + unithit);
						targeting=false;
						StartCoroutine(FreeMasterSelector());
					}else{ //not a linkable object, TODO maybe display an error?
						//	Debug.Log("Not a linkable object!");
						targeting=false;
						StartCoroutine(FreeMasterSelector());
					}
				}
			}
		}
	}

	void RunAsAmmoSupplier(){

		alwaysOnIndicator = true;

		//right click to free, this is building it doesn't move
		if (Input.GetMouseButtonDown (1)) {
			targeting=false;
			StartCoroutine(FreeMasterSelector());
		}

	}
	
	void RunAsDeliveryDrone(){
		if (Input.GetMouseButtonDown (1)) {
			//drones don't move by command
		}
		
		if (targeting) {
			if (Input.GetMouseButtonDown (0)) { 
				//drones don't move by command
					targeting = false;
					StartCoroutine(FreeMasterSelector());
			}
		}
	}
	#endregion

	#region GUI operations
	void GUICube(){
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
							targeting=true;
							masterselector.Suspend();
						}
						if(buttonnumber==6){
							targeting=true;
							masterselector.Suspend();
						}
						
					}}}}
		if (targeting) { //targeting an attack, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x, 
			                         Screen.height- Input.mousePosition.y - (masterCursorSize), masterCursorSize, masterCursorSize), buttonTextures[5]);
		}

		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n"	              
		              ,statusStyle);
	}

	void GUIHealthOnly(){
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n"	              
		              ,statusStyle);
	}

	void GUIGenericTurret(){
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE

						
					}}}}
		if (targeting) { //targeting, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x-Screen.width/27,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize), masterCursorSize*2, masterCursorSize*2), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		AmmoControl amo = GetComponent<AmmoControl> ();
		Linkage link = GetComponent<Linkage> ();

		string extraText="";
		if (link != null) {
			extraText += "Charging : " ;
			if(link.getInLinks()>0){
				extraText += "Yes" + "\n";
			}else{
				extraText += "No"  + "\n";
			}
		}
		if (amo != null) {
			extraText += "Stored Ammo: " + amo.getCurrentAmmo ().ToString () + " / " + amo.getMaxAmmo ().ToString () + "\n";
		}
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Stored Power: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" + 
		              extraText
		              
		              ,statusStyle);
	}

	void GUIGenericDrone(){
		string displayHoverText=""; //text to display in the popup mouseover window
		bool isHovering = false;
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						
					} //end button push code
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						isHovering=false; //assume nothing is being hovered over
						switch(buttonnumber){
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
				}}}
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/8, 
			                                 Screen.width/8f, Screen.height/8 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
		}
		if (targeting) { //targeting a link output, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		Linkage link = GetComponent<Linkage> ();
		AmmoControl amo = GetComponent<AmmoControl> ();

		string extraText="";
		if (link != null) {
			extraText += "Charging : " ;
			if(link.getInLinks()>0){
				extraText += "Yes\n";
			}else{
				extraText += "No\n";
			}
		}
		if (amo != null) {
			extraText += "Stored Ammo: " + amo.getCurrentAmmo ().ToString () + " / " + amo.getMaxAmmo ().ToString () + "\n";
		}
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Production: " + pow.getWatts().ToString() + " Watts" + "\n" +
		              "Stored: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" + 
		              extraText
		              
		              ,statusStyle);
	}


	void GUIGenericGenerator(){
		string displayHoverText=""; //text to display in the popup mouseover window
		bool isHovering = false;
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
							status = 1; 
							targeting=true;
							masterselector.Suspend();
						}else if(buttonnumber==6){
							status = 2; 
							targeting=true;
							masterselector.Suspend();
						}
						
					} //end button push code
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						isHovering=false; //assume nothing is being hovered over
						switch(buttonnumber){
						case 5:
							displayHoverText = "Establish Link \n\nTransfers power to the target.";
							isHovering=true;
							break;
						case 6:
							displayHoverText = "Break Link \n\nStops transferring power to the target (must already have existing link).";
							isHovering=true;
							break;
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
				}}}
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/8, 
			                                 Screen.width/8f, Screen.height/8 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
		}
		if (targeting) { //targeting a link output, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}

		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		Linkage link = GetComponent<Linkage> ();

		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Production: " + pow.getWatts().ToString() + " Watts" + "\n" +
		              "Stored: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" + 
		              "Transfer (out): " +  link.getCurrentAmpsOut().ToString() + " / " + link.getMaxOut().ToString() + " Amps" + "\n" + 
		              "Links (out): " + link.getOutLinks().ToString() +  " / " + link.getMaxOutLinks().ToString()  + "\n"

		              ,statusStyle);
	}

	void GUINuclear(){
		string displayHoverText=""; //text to display in the popup mouseover window
		bool isHovering = false;
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(buttonnumber==11 && !(GetComponent<NuclearPower>().isMeltingDown)){
					//do nothing
				}else if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
							status = 1; 
							targeting=true;
							masterselector.Suspend();
						}else if(buttonnumber==6){
							status = 2; 
							targeting=true;
							masterselector.Suspend();
						}else if(buttonnumber==10){
							if(GetComponent<NuclearPower>()!=null){
								GetComponent<NuclearPower>().overheatReactor();
							}
						}else if(buttonnumber==11){
								GetComponent<NuclearPower>().ventReactor();
						}
						
					} //end button push code
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						isHovering=false; //assume nothing is being hovered over
						switch(buttonnumber){
						case 5:
							displayHoverText = "Establish Link \n\nTransfers power to the target.";
							isHovering=true;
							break;
						case 6:
							displayHoverText = "Break Link \n\nStops transferring power to the target (must already have existing link).";
							isHovering=true;
							break;
						case 10:
							displayHoverText = "Overheat Reactor \n\nIncrease uranium consumption per second by 1, and improve power per unit of uranium by 5%. Permenantly increases the risk of a catastrophic meltdown.";
							isHovering=true;
							break;
						case 11:
							displayHoverText = "Vent Reactor \n\nSave the core from going critical! Vent heat from the reactor core, expelling all stored energy and temporarily reducing output to zero.";
							isHovering=true;
							break;
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
				}}}
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/8, 
			                                 Screen.width/8f, Screen.height/8 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
		}
		if (targeting) { //targeting a link output, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		Linkage link = GetComponent<Linkage> ();
		ResourceControl res = GetComponent<ResourceControl> ();
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Production: " + pow.getWatts().ToString() + " Watts" + "\n" +
		              "Stored: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" + 
		              "Transfer (out): " +  link.getCurrentAmpsOut().ToString() + " / " + link.getMaxOut().ToString() + " Amps" + "\n" + 
		              "Links (out): " + link.getOutLinks().ToString() +  " / " + link.getMaxOutLinks().ToString()  + "\n" +
		              "Enriched Uranium: " + res.getCurrentInputResource(res.getIndexofInputType("Enriched Uranium")).ToString() + " / " +
		              res.getMaxInputResource(res.getIndexofInputType("Enriched Uranium")).ToString()
		              
		              ,statusStyle);
	}

	void GUIShield(){
		string displayHoverText=""; //text to display in the popup mouseover window
		bool isHovering = false;

		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
							//insert cost here
							if( GetComponentInChildren<ShieldManager>().boostOverclock() ){ //can only be a certain number of times
								//deduct cost
							}
						}else if(buttonnumber==6){
							GetComponentInChildren<ShieldManager>().toggleShield();
						//	masterselector.Suspend();
						}
						
					}
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						isHovering=false; //assume nothing is being hovered over
						switch(buttonnumber){
						case 5:
							displayHoverText = "Overclock \n\nPermenantly boost shield regeneration, power consumption, and power capacity. Can only be done five times.";
							isHovering=true;
							break;
						case 6:
							displayHoverText = "Toggle On/Off \n\nEngages or disengages the shield.";
							isHovering=true;
							break;
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
				}}}

		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/8, 
			                                 Screen.width/8f, Screen.height/8 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
		}
		if (targeting) { //targeting a link output, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		Linkage link = GetComponent<Linkage> ();
		ShieldManager shield = GetComponentInChildren<ShieldManager> ();
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Stored: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" + 
		              "Shield: " + shield.getShieldCurrentHealth().ToString() + " / " + shield.getShieldMaxHealth().ToString()
		              
		              ,statusStyle);
	}


	void GUIGeothermal(){
		string displayHoverText=""; //text to display in the popup mouseover window
		bool isHovering = false;
		int hoverButton = 0; //stores number of button being hovered over
		int[] resourceCost = new int[6]; //resource cost if a construction is available

		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						GUI.depth = 1;
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
							status = 1; 
							targeting=true;
							masterselector.Suspend();
						}else if(buttonnumber==6){
							status = 2; 
							targeting=true;
							masterselector.Suspend();
						}else if(buttonnumber==10){
							//Note: Pressing the "Z" key is handled in the GeoPower class
							if(checkCostLocallyandDeduct(new int[]{0,0,0,0,0,50000})
							   && GetComponent<GeoPower>().getBoreHoles() < GetComponent<GeoPower>().getMaxBoreHoles() ){
								GetComponent<GeoPower>().drillBoreHole();
							}
						}
						
					}
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						isHovering=false; //assume nothing is being hovered over
						hoverButton = buttonnumber;
						switch(buttonnumber){
						case 5:
							displayHoverText = "Link \n\nTransfer power.";
							isHovering=true;
							break;
						case 6:
							displayHoverText = "UnLink \n\nBreak existing power link.";
							isHovering=true;
							break;
						case 10:
							displayHoverText = "Drill Borehole \n\nSpend a massive amount of stored energy to laser drill a new borehole in the geothermal vent, permanently increasing power production by 500 watts.";
							isHovering=true;
							break;
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
				}}}
		//Draw hover dialog, if applicable
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/6, 
			                                 Screen.width/8f, Screen.height/6 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
			//Display unit costs
			bool hasCost=false;
			switch(hoverButton){
			case 10:
				hasCost=true;
				resourceCost[5]=50000; 	//power
				break;
			default:
				hasCost=false;
				break;
			}
			if(hasCost){
				float textureSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2;
				//Screen.height / 64f; //size of res icon
				float textSize = textureSize * 2f; //size of text cost
				float widthPadding = Screen.width / 250f;
				float barHeight = Screen.height / 64f;
				float netoffSet = 0f;
				for(int i=0; i<6; i++){
					if(resourceCost[i]>0){
						GUI.depth =4;
						GUIStyle toolTipStyle = new GUIStyle();
						toolTipStyle.fontSize = (int)(Screen.width / Constants.FONT_SIZE_DIVISOR / 2.2);
						toolTipStyle.normal.textColor = Color.green;
						Rect GUIResTip = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                           textureSize, textureSize );
						GUI.DrawTexture(GUIResTip, manager.resourceIcons[i]);
						netoffSet += textureSize + widthPadding;
						
						Rect ResTipText = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                            textSize, textureSize);
						GUI.TextField (ResTipText, resourceCost[i].ToString(),toolTipStyle);
						netoffSet += textSize + widthPadding;
						
					}
				}
			}
		}
		if (targeting) { //targeting a link output, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		Linkage link = GetComponent<Linkage> ();
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Production: " + pow.getWatts().ToString() + " Watts" + "\n" +
		              "Stored: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" + 
		              "Boreholes: " + GetComponent<GeoPower>().getBoreHoles() + " / " + GetComponent<GeoPower>().getMaxBoreHoles() + "\n" + 
		              "Transfer (out): " +  link.getCurrentAmpsOut().ToString() + " / " + link.getMaxOut().ToString() + " Amps" + "\n" + 
		              "Links (out): " + link.getOutLinks().ToString() + "\n"
		              
		              ,statusStyle);
	}

	void GUIGenericResourceProducer(){
		isHovering=false; //assume nothing is being hovered over
		int hoverButton = 0; //stores number of button being hovered over
		int[] resourceCost = new int[6]; //resource cost if a construction is available
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
								status = 1; 
								targeting=true;
								masterselector.Suspend();
						}else if(buttonnumber==6){
								status = 3; 
								targeting=true;
								masterselector.Suspend();
						}else if(buttonnumber==7){
								status = 3; 
								targeting=true;
								masterselector.Suspend();
						}
					}
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						hoverButton = buttonnumber;
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						string extraText="";
						switch(buttonnumber){
						case 5:
							extraText="";
							if(GetComponent<ResourceControl>()!=null){
								//NOTE: unstable, can crash if out of bounds. Make SURE the unit has that output index available!
								extraText += GetComponent<ResourceControl>().getOutputType(0);
							}
							displayHoverText = "Ship " + extraText + " \n\nBegin making scheduled deliveries of " + extraText + " to target.";
							isHovering=true;
							break;
						case 6:
							extraText="";
							if(GetComponent<ResourceControl>()!=null){
								//Note: unstable, can crash if out of bounds
								extraText += GetComponent<ResourceControl>().getOutputType(1);
							}
							displayHoverText = "Ship " + extraText + " \n\nBegin making scheduled deliveries of " + extraText + " to target.";
							isHovering=true;
							break;
						case 7:
							extraText="";
							if(GetComponent<ResourceControl>()!=null){
								//Note: unstable, can crash if out of bounds
								extraText += GetComponent<ResourceControl>().getOutputType(2);
							}
							displayHoverText = "Ship " + extraText + " \n\nBegin making scheduled deliveries of " + extraText + " to target.";
							isHovering=true;
							break;
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
					
				}}}

		//Draw hover dialog, if applicable
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/6, 
			                                 Screen.width/8f, Screen.height/8 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
			//Display unit costs
			bool hasCost=false;
			switch(hoverButton){
			case 5:
				hasCost=true;
				resourceCost[1]=0; 	//steel
				resourceCost[2]=0; 	//fuel
				resourceCost[3]=0; 		//exotics
				resourceCost[4]=0; 		//uranium
				resourceCost[5]=500; 		//power
				break;
			case 6:
				hasCost=true;
				resourceCost[1]=0; //steel
				resourceCost[2]=0; //fuel
				resourceCost[3]=0; //exotics
				resourceCost[4]=0; 	//uranium
				resourceCost[5]=500; 	//power
				break;
			case 7:
				hasCost=true;
				resourceCost[1]=0; //steel
				resourceCost[2]=0; //fuel
				resourceCost[3]=0; //exotics
				resourceCost[4]=0; 	//uranium
				resourceCost[5]=500; 	//power
				break;
			default:
				hasCost=false;
				break;
			}
			if(hasCost){
				float textureSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2;
				//Screen.height / 64f; //size of res icon
				float textSize = textureSize * 2f; //size of text cost
				float widthPadding = Screen.width / 250f;
				float barHeight = Screen.height / 64f;
				float netoffSet = 0f;
				for(int i=0; i<6; i++){
					if(resourceCost[i]>0){
						GUI.depth =4;
						GUIStyle toolTipStyle = new GUIStyle();
						toolTipStyle.fontSize = (int)(Screen.width / Constants.FONT_SIZE_DIVISOR / 2.2);
						toolTipStyle.normal.textColor = Color.green;
						Rect GUIResTip = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                           textureSize, textureSize );
						GUI.DrawTexture(GUIResTip, manager.resourceIcons[i]);
						netoffSet += textureSize + widthPadding;
						
						Rect ResTipText = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                            textSize, textureSize);
						GUI.TextField (ResTipText, resourceCost[i].ToString(),toolTipStyle);
						netoffSet += textSize + widthPadding;
						
					}
				}
			}
		}
		
		if (targeting) { //targeting a link output, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		Linkage link = GetComponent<Linkage> ();
		ResourceControl res = GetComponent<ResourceControl> ();

		string resourceText = "";
		if (GetComponent<OrbitalDrop> ().isLanded()) { //engaging UI before landing results in errors
			if (GetComponent<ResourceMine> () != null) { //for mines, show resources in the ground
				if (GetComponent<ResourceMine> ().getNode () != null) {
					ResourceNode node = GetComponent<ResourceMine> ().getNode ();
					resourceText += res.getInputType (0) + " in Node: " + node.getResource () + "\n";
				}
			} else { //for other units, show input resources
				for (int i =0; i< res.getInputCount(); i++) {
					resourceText += "Stored " + res.getInputType (i) + ": " + res.getCurrentInputResource (i).ToString () + " / " + res.getMaxInputResource (i).ToString () + "\n";
				}
			}
			for (int i =0; i< res.getOutputCount(); i++) {
				resourceText += "Stored " + res.getOutputType (i) + ": " + res.getCurrentOutputResource (i).ToString () + " / " + res.getMaxOutputResource (i).ToString () + "\n";
			}
		}

		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Stored Power: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" +
		              resourceText
		              ,statusStyle);
	}

	void GUIAmmoSupplier(){
		//TODO range indicator?
		isHovering=false; //assume nothing is being hovered over
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){

						}	
					}
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						switch(buttonnumber){
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
					
				}}}
		
		//Draw hover dialog, if applicable
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/8, 
			                                 Screen.width/8f, Screen.height/8 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
		}
		
		if (targeting) { //targeting a link output, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2,
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		PowerControl pow = GetComponent<PowerControl> ();
		Linkage link = GetComponent<Linkage> ();
		ResourceControl res = GetComponent<ResourceControl> ();
		
		string resourceText = "";
		if (GetComponent<OrbitalDrop> ().isLanded()) { //engaging UI before landing results in errors
			if (GetComponent<ResourceMine> () != null) { //for mines, show resources in the ground
				if (GetComponent<ResourceMine> ().getNode () != null) {
					ResourceNode node = GetComponent<ResourceMine> ().getNode ();
					resourceText += res.getInputType (0) + " in Node: " + node.getResource () + "\n";
				}
			} else { //for other units, show input resources
				for (int i =0; i< res.getInputCount(); i++) {
					resourceText += "Stored " + res.getInputType (i) + ": " + res.getCurrentInputResource (i).ToString () + " / " + res.getMaxInputResource (i).ToString () + "\n";
				}
			}
			for (int i =0; i< res.getOutputCount(); i++) {
				resourceText += "Stored " + res.getOutputType (i) + ": " + res.getCurrentOutputResource (i).ToString () + " / " + res.getMaxOutputResource (i).ToString () + "\n";
			}
		}
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              "Stored Power: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" +
		              resourceText
		              ,statusStyle);
	}

	void GUIBase(){
		isHovering=false; //assume nothing is being hovered over
		int hoverButton = 0; //stores number of button being hovered over
		int[] resourceCost = new int[6]; //resource cost if a construction is available
		Rect GUIbutton;
		//Generate button array
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					if(buttonnumber==5 && GetComponent<UnitBuilder>().Slave1 ==null){

					}else{
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
							targeting=true;
							masterselector.Suspend();
						}else if(buttonnumber==10){
							//Bomber
							if(checkCostLocallyandDeduct(new int[]{0,500,1000,500,0,1500})){
								GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 0);
							}
						}else if(buttonnumber==11){
							//Fighter
							if(checkCostLocallyandDeduct(new int[]{0,500,1000,500,0,1500})){
								GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 1);
							}
						}

						}
						//mouse over display code begins here
						if(GUIbutton.Contains(Event.current.mousePosition)){ 
							hoverButton = buttonnumber;
							//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
							switch(buttonnumber){
							case 5:
								displayHoverText = "Bombing Run \n\nDrop a considerable amount of freedom on a far away target. Requires docked bomber.";
								isHovering=true;
								break;
							case 10:
								displayHoverText = "Build Bomber \n\nDrops bombs anywhere on the map, inflicting devestating damage to ground targets.";
								isHovering=true;
								break;
							case 11:
								displayHoverText = "Build Fighter \n\nAutomatically patrols around the air base, attacking flying targets in the area.";
								isHovering=true;
								break;
							default:
								displayHoverText="";
								isHovering=false;
								break;
							}
						}

					}}}}

		//Draw hover dialog, if applicable
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/6, 
			                                 Screen.width/8f, Screen.height/6 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
			//Display unit costs
			bool hasCost=false;
			switch(hoverButton){
			case 10:
				hasCost=true;
				resourceCost[1]=500; 	//steel
				resourceCost[2]=1000; 	//fuel
				resourceCost[3]=500; 		//exotics
				resourceCost[4]=0; 		//uranium
				resourceCost[5]=1500; 		//power
				break;
			case 11:
				hasCost=true;
				resourceCost[1]=500; //steel
				resourceCost[2]=1000; //fuel
				resourceCost[3]=500; //exotics
				resourceCost[4]=0; 	//uranium
				resourceCost[5]=1500; 	//power
				break;
			default:
				hasCost=false;
				break;
			}
			if(hasCost){
				float textureSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2;
				//Screen.height / 64f; //size of res icon
				float textSize = textureSize * 2f; //size of text cost
				float widthPadding = Screen.width / 250f;
				float barHeight = Screen.height / 64f;
				float netoffSet = 0f;
				for(int i=0; i<6; i++){
					if(resourceCost[i]>0){
						GUI.depth =4;
						GUIStyle toolTipStyle = new GUIStyle();
						toolTipStyle.fontSize = (int)(Screen.width / Constants.FONT_SIZE_DIVISOR / 2.2);
						toolTipStyle.normal.textColor = Color.green;
						Rect GUIResTip = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                           textureSize, textureSize );
						GUI.DrawTexture(GUIResTip, manager.resourceIcons[i]);
						netoffSet += textureSize + widthPadding;
						
						Rect ResTipText = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                            textSize, textureSize);
						GUI.TextField (ResTipText, resourceCost[i].ToString(),toolTipStyle);
						netoffSet += textSize + widthPadding;
						
					}
				}
			}
		}

		if (targeting) { //targeting an attack, texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x- (masterCursorSize/2), 
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();

		string statusExtra = "";
		if (GetComponent<UnitBuilder> ().Slave1 != null) {
			statusExtra = GetComponent<UnitBuilder>().Slave1.GetComponent<Controls>().objectName + ": " + 
				GetComponent<UnitBuilder>().Slave1.GetComponent<UnitLife>().getHealth().ToString() + " / " +
				GetComponent<UnitBuilder>().Slave1.GetComponent<UnitLife>().getMaxHealth().ToString();
		}

		PowerControl pow = GetComponent<PowerControl> ();
		ResourceControl res = GetComponent<ResourceControl> ();
		
		string resourceText = "";
		for (int i =0; i< res.getInputCount(); i++) {
			resourceText += "Stored " + res.getInputType (i) + ": " + res.getCurrentInputResource (i).ToString () + " / " + res.getMaxInputResource (i).ToString () + "\n";
		}

		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" +
		              "Stored Power: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" +
		              resourceText + statusExtra
		              ,statusStyle);
	}

	void GUIConstructor(){ //same as a base, but units are not slaved after construction
		isHovering=false; //assume nothing is being hovered over
		int hoverButton = 0; //stores number of button being hovered over
		int[] resourceCost = new int[6]; //resource cost if a construction is available
		Rect GUIbutton;
		//Generate button array
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
						GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
						                      buttonwidth, buttonheight);
						
						if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
							//BUTTON SPECIFIC CODE HERE
							if(buttonnumber==5){
								//Gatling Drone
								if(checkCostLocallyandDeduct(new int[]{0,500,500,0,0,500})){
									GetComponent<UnitBuilder>().BuildUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 0);
								}
							}else if(buttonnumber==6){
								//Laser Drone
								if(checkCostLocallyandDeduct(new int[]{0,500,500,500,0,500})){
									GetComponent<UnitBuilder>().BuildUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 1);
								}
							}else if(buttonnumber==10){
								//Tank
								if(checkCostLocallyandDeduct(new int[]{0,3000,1000,500,0,2000})){
									GetComponent<UnitBuilder>().BuildUnit(transform.position, new Vector3(0f, .6f, 0f), transform.rotation, 2);
								}
							}
							
						}
						//mouse over display code begins here
						if(GUIbutton.Contains(Event.current.mousePosition)){ 
							hoverButton = buttonnumber;
							//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
							switch(buttonnumber){
							case 5:
								displayHoverText = "Build Gatling Drone \n\nBuilds a basic portable turret drone. Attacks with a weak gatling gun. Requires ammo.";
								isHovering=true;
								break;
							case 6:
								displayHoverText = "Build Laser Drone \n\nBuilds a basic portable turret drone. Attacks with a weak laser. Requires external recharging to be effective.";
								isHovering=true;
								break;
							case 10:
								displayHoverText = "Build Tank \n\nBuilds a hover tank drone. Moves slowly, turns poorly, attacks with a front-facing gatling gun and high damage cannon.";
								isHovering=true;
								break;
							default:
								displayHoverText="";
								isHovering=false;
								break;
							}
						}
						
					}}}
		
		//Draw hover dialog, if applicable
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/6, 
			                                 Screen.width/8f, Screen.height/6 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
			//Display unit costs
			bool hasCost=false;
			switch(hoverButton){
			case 5: //gatling
				hasCost=true;
				resourceCost[1]=500; 	//steel
				resourceCost[2]=500; 	//fuel
				resourceCost[3]=0; 		//exotics
				resourceCost[4]=0; 		//uranium
				resourceCost[5]=500; 		//power
				break;
			case 6: //laser
				hasCost=true;
				resourceCost[1]=500; //steel
				resourceCost[2]=500; //fuel
				resourceCost[3]=500; //exotics
				resourceCost[4]=0; 	//uranium
				resourceCost[5]=500; 	//power
				break;
			case 10: //tank
				hasCost=true;
				resourceCost[1]=3000; //steel
				resourceCost[2]=1000; //fuel
				resourceCost[3]=500; //exotics
				resourceCost[4]=0; 	//uranium
				resourceCost[5]=2000; 	//power
				break;
			default:
				hasCost=false;
				break;
			}
			if(hasCost){
				float textureSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2;
				//Screen.height / 64f; //size of res icon
				float textSize = textureSize * 2f; //size of text cost
				float widthPadding = Screen.width / 250f;
				float barHeight = Screen.height / 64f;
				float netoffSet = 0f;
				for(int i=0; i<6; i++){
					if(resourceCost[i]>0){
						GUI.depth =4;
						GUIStyle toolTipStyle = new GUIStyle();
						toolTipStyle.fontSize = (int)(Screen.width / Constants.FONT_SIZE_DIVISOR / 2.2);
						toolTipStyle.normal.textColor = Color.green;
						Rect GUIResTip = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                           textureSize, textureSize );
						GUI.DrawTexture(GUIResTip, manager.resourceIcons[i]);
						netoffSet += textureSize + widthPadding;
						
						Rect ResTipText = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                            textSize, textureSize);
						GUI.TextField (ResTipText, resourceCost[i].ToString(),toolTipStyle);
						netoffSet += textSize + widthPadding;
						
					}
				}
			}
		}
		
		if (targeting) { //targeting an attack, texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x- (masterCursorSize/2), 
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}

		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();

		ResourceControl res = GetComponent<ResourceControl> ();
		PowerControl pow = GetComponent<PowerControl> ();

		string resourceText = "";
		for (int i =0; i< res.getInputCount(); i++) {
			resourceText += "Stored " + res.getInputType (i) + ": " + res.getCurrentInputResource (i).ToString () + " / " + res.getMaxInputResource (i).ToString () + "\n";
		}

		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" +
		              "Stored Power: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" +
		              resourceText
		              ,statusStyle);
	}

	void GUINuclearLauncher(){ //constructs missiles, which are stored in the building
		isHovering=false; //assume nothing is being hovered over
		int hoverButton = 0; //stores number of button being hovered over
		int[] resourceCost = new int[6]; //resource cost if a construction is available
		Rect GUIbutton;
		//Generate button array
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					if(buttonnumber==5 && GetComponent<UnitBuilder>().Slave1 ==null){
						//show nothing
					}else{
						GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
						                      buttonwidth, buttonheight);
						
						if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
							//BUTTON SPECIFIC CODE HERE
							if(buttonnumber==5){
								targeting=true;
								masterselector.Suspend();
							}else if(buttonnumber==10){
								//Missile
								if(checkCostLocallyandDeduct(new int[]{0,500,500,0,0,0})){
									GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, -8f, 0f), Quaternion.Euler(-90f,0f,0f), 0);
								}
							}else if(buttonnumber==11){
								//Nuke
								if(checkCostLocallyandDeduct(new int[]{0,1000,1000,0,500,0})){
									GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, -8f, 0f), Quaternion.Euler(-90f,0f,0f), 1);
								}
							}
							
						}
						//mouse over display code begins here
						if(GUIbutton.Contains(Event.current.mousePosition)){ 
							//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
							hoverButton = buttonnumber;
							switch(buttonnumber){
							case 5:
								displayHoverText = "Fire Missile \n\nFire the stored missile at target point.";
								isHovering=true;
								break;
							case 10:
								displayHoverText = "Build Missile \n\nOne-time long range explosive. Deals tremendous damage to the target area upon arrival.";
								isHovering=true;
								break;
							case 11:
								displayHoverText = "Build Nuclear Missile \n\nOne-time long range explosive. Annihilates anything in the blast radius, leaving behind a wide field of flame. Use with extreme caution.";
								isHovering=true;
								break;
							default:
								displayHoverText="";
								isHovering=false;
								break;
							}
						}
						
					}}}}
		
		//Draw hover dialog, if applicable
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/6, 
			                                 Screen.width/8f, Screen.height/6 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
			//Display unit costs
			bool hasCost=false;
			switch(hoverButton){
			case 10:
				hasCost=true;
				resourceCost[1]=500; 	//steel
				resourceCost[2]=500; 	//fuel
				resourceCost[3]=0; 		//exotics
				resourceCost[4]=0; 		//uranium
				resourceCost[5]=0; 		//power
				break;
			case 11:
				hasCost=true;
				resourceCost[1]=1000; //steel
				resourceCost[2]=1000; //fuel
				resourceCost[3]=0; //exotics
				resourceCost[4]=500; 	//uranium
				resourceCost[5]=0; 	//power
				break;
			default:
				hasCost=false;
				break;
			}
			if(hasCost){
				float textureSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2;
				//Screen.height / 64f; //size of res icon
				float textSize = textureSize * 2f; //size of text cost
				float widthPadding = Screen.width / 250f;
				float barHeight = Screen.height / 64f;
				float netoffSet = 0f;
				for(int i=0; i<6; i++){
					if(resourceCost[i]>0){
						GUI.depth =4;
						GUIStyle toolTipStyle = new GUIStyle();
						toolTipStyle.fontSize = (int)(Screen.width / Constants.FONT_SIZE_DIVISOR / 2.2);
						toolTipStyle.normal.textColor = Color.green;
						Rect GUIResTip = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                           textureSize, textureSize );
						GUI.DrawTexture(GUIResTip, manager.resourceIcons[i]);
						netoffSet += textureSize + widthPadding;
						
						Rect ResTipText = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
						                            textSize, textureSize);
						GUI.TextField (ResTipText, resourceCost[i].ToString(),toolTipStyle);
						netoffSet += textSize + widthPadding;
						
					}
				}
			}
		}
		
		if (targeting) { //targeting an attack, texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x- (masterCursorSize/2), 
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();

		ResourceControl res = GetComponent<ResourceControl> ();
		
		string resourceText = "";
				for (int i =0; i< res.getInputCount(); i++) {
					resourceText += "Stored " + res.getInputType (i) + ": " + res.getCurrentInputResource (i).ToString () + " / " + res.getMaxInputResource (i).ToString () + "\n";
				}


		string statusExtra = "";
		if (GetComponent<UnitBuilder> ().Slave1 != null) {
			statusExtra = GetComponent<UnitBuilder>().Slave1.GetComponent<NukeControl>().name;
			if(GetComponent<UnitBuilder>().Slave1.GetComponent<NukeControl>().initiateLaunch){
				statusExtra+=": Launched\n" ;
			}else{
				statusExtra+=": Armed\n" ;
			}
		}
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" +
		              resourceText + statusExtra
		              ,statusStyle);
	}

	void GUIOrbitalLauncher(){ //constructs missiles, which are stored in the building
		isHovering=false; //assume nothing is being hovered over
		int hoverButton = 0; //stores number of button being hovered over
		int[] resourceCost = new int[6]; //resource cost if a construction is available
		Rect GUIbutton;
		//Generate button array
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					if(buttonnumber==5 && GetComponent<UnitBuilder>().Slave1 ==null){
						//show nothing
					}else if( buttonnumber==6 && !(GetComponent<SpaceElevator>().hasSpaceElevator) ){
						//show nothing
					}else if( buttonnumber==7 && !(GetComponent<SpaceElevator>().hasSpaceLaser) ){
						//show nothing
					}else{
						GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
						                      buttonwidth, buttonheight);
						
						if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
							//BUTTON SPECIFIC CODE HERE
							if(buttonnumber==5 && GetComponent<UnitBuilder>().Slave1 != null ){
								//Orbital rockets have a nuke controller for when they fall back planetside
								NukeControl nukeCon = GetComponent<UnitBuilder>().Slave1.GetComponent<NukeControl>();
								if( nukeCon != null //sanity check for recent destroy
								  && !nukeCon.initiateLaunch ) { 
									nukeCon.initiateLaunch=true;
									nukeCon.target=new Vector3(transform.position.x, 100f, transform.position.z); //point it into space
									GetComponent<UnitBuilder>().Slave1.GetComponent<DestroyByTime>().dontKill=false;
									ResourceManager resManager = GameObject.FindGameObjectWithTag("ResourceManager").GetComponent<ResourceManager>();
									ResourceControl resControl = GetComponent<ResourceControl>();
									for(int k=0; k<resControl.getInputCount(); k++){
										int toSend, maxToSend;
										if(resControl.getInputType(k)=="Steel"){
											maxToSend=4000;
										} else if(resControl.getInputType(k)=="Enriched Uranium"){
											maxToSend=100;
										}else if (resControl.getInputType (k) == "Exotics") {
											maxToSend = 2000;
										}else if (resControl.getInputType (k) == "Fuel") {
											maxToSend = 2000;
										}
										else{
											maxToSend=10;
										}
										if(resControl.getCurrentInputResource(k) > maxToSend){
											toSend = maxToSend;
										}else{
											toSend = resControl.getCurrentInputResource(k);
										}
										resControl.drainInput(toSend, k);
										resManager.sendResourceWithTag(resControl.getInputType(k),toSend);
									}
								}
							}else if(buttonnumber==6 && GetComponent<SpaceElevator>().hasSpaceElevator){
								//Pause Elevator
								GetComponent<SpaceElevator>().spaceElevatorPaused = !(GetComponent<SpaceElevator>().spaceElevatorPaused);
							}else if(buttonnumber==7 && GetComponent<SpaceElevator>().hasSpaceLaser){
								//Pause Laser
								GetComponent<SpaceElevator>().spaceLaserPaused = !(GetComponent<SpaceElevator>().spaceLaserPaused);
							}else if(buttonnumber==10){
								//Orbital rocket
								if(checkCostLocally(new int[]{0,100,200,0,0,500}) && GetComponent<UnitBuilder>().Slave1 == null){
									checkCostLocallyandDeduct(new int[]{0,100,200,0,0,500});
									GetComponent<UnitBuilder>().BuildSlaveUnit(transform.position, new Vector3(0f, -8f, 0f), Quaternion.Euler(-90f,0f,0f), 0);
								}
							}else if(buttonnumber==11){
								//Build a space elevator
								if(checkCostLocallyandDeduct(new int[]{0,5000,1000,2000,0,5000})){
									GetComponent<SpaceElevator>().buildSpaceElevator();
								}
							}else if(buttonnumber==12){
								//Build a space laser
								if(checkCostLocallyandDeduct(new int[]{0,1000,1000,4000,0,5000})){
									GetComponent<SpaceElevator>().buildSpaceLaser();
								}
							}
							
						}
						//mouse over display code begins here
						if(GUIbutton.Contains(Event.current.mousePosition)){ 
							hoverButton = buttonnumber;
							//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
							switch(buttonnumber){
							case 5:
								displayHoverText = "Ship to Space \n\nBlast stored resources into orbit.";
								isHovering=true;
								break;
							case 6:
								displayHoverText = "Pause Lift \n\nPause or unpause the space elevator to save power. Current status: ";
								if(GetComponent<SpaceElevator>().spaceElevatorPaused){ displayHoverText += "paused"; }else{ displayHoverText += "unpaused"; }
								isHovering=true;
								break;
							case 7:
								displayHoverText = "Pause Laser \n\nPause or unpause the orbital transmission laser to save power. Current status: ";
								if(GetComponent<SpaceElevator>().spaceLaserPaused){ displayHoverText += "paused"; }else{ displayHoverText += "unpaused"; }
								isHovering=true;
								break;
							case 10:
								displayHoverText = "Build Orbital Rocket \n\nOne-time rocket that launches resources into orbit, where the ship's fabricator can assemble them into new structures. Delivers up to 4000 Steel, 2000 Fuel and Exotics, and 100 Enriched Uranium in each rocket.";
								isHovering=true;
								break;
							case 11:
								displayHoverText = "Build Space Elevator \n\nPermanent thick metal cabling connecting the planet's surface to the ship, automatically transferring held resources into orbit. Requires power to operate. Cannot also build laser.";
								isHovering=true;
								break;
							case 12:
								displayHoverText = "Build Surface to Orbit Laser \n\nAutomatically sends linked power into orbit, allowing the ship to power surface attack batteries. Cannot also build space elevator.";
								isHovering=true;
								break;
							default:
								displayHoverText="";
								isHovering=false;
								break;
							}
						}
						
					}}}}
		
		//Draw hover dialog, if applicable
		if (isHovering) {
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/6, 
			                                 Screen.width/8f, Screen.height/6 );
			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
			//Display unit costs
			bool hasCost=false;
			switch(hoverButton){
			case 10:
				hasCost=true;
				resourceCost[1]=100; 	//steel
				resourceCost[2]=200; 	//fuel
				resourceCost[3]=0; 		//exotics
				resourceCost[5]=500; 	//power
				break;
			case 11:
				hasCost=true;
				resourceCost[1]=5000; //steel
				resourceCost[2]=1000; //fuel
				resourceCost[3]=2000; //exotics
				resourceCost[5]=5000; //power
				break;
			case 12:
				hasCost=true;
				resourceCost[1]=1000; //steel
				resourceCost[2]=1000; //fuel
				resourceCost[3]=4000; //exotics
				resourceCost[5]=5000; //power
				break;
			default:
				hasCost=false;
				break;
			}
				if(hasCost){
					float textureSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2;
					//Screen.height / 64f; //size of res icon
					float textSize = textureSize * 2f; //size of text cost
					float widthPadding = Screen.width / 250f;
					float barHeight = Screen.height / 64f;
					float netoffSet = 0f;
					for(int i=0; i<6; i++){
						if(resourceCost[i]>0){
							GUI.depth =4;
							GUIStyle toolTipStyle = new GUIStyle();
							toolTipStyle.fontSize = (int)(Screen.width / Constants.FONT_SIZE_DIVISOR / 2.2);
							toolTipStyle.normal.textColor = Color.green;
							Rect GUIResTip = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
							                           textureSize, textureSize );
							GUI.DrawTexture(GUIResTip, manager.resourceIcons[i]);
							netoffSet += textureSize + widthPadding;
							
							Rect ResTipText = new Rect (Event.current.mousePosition.x + netoffSet, Event.current.mousePosition.y - Screen.height/6 + textureSize, 
							                            textSize, textureSize);
							GUI.TextField (ResTipText, resourceCost[i].ToString(),toolTipStyle);
							netoffSet += textSize + widthPadding;
							
						}
					}
				}
		}
		
		if (targeting) { //targeting an attack, texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x- (masterCursorSize/2), 
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		
		ResourceControl res = GetComponent<ResourceControl> ();
		
		string resourceText = "";
		for (int i =0; i< res.getInputCount(); i++) {
			resourceText += "Stored " + res.getInputType (i) + ": " + res.getCurrentInputResource (i).ToString () + " / " + res.getMaxInputResource (i).ToString () + "\n";
		}
		
		
		string statusExtra = "";
		if (GetComponent<UnitBuilder> ().Slave1 != null) {
			statusExtra = GetComponent<UnitBuilder>().Slave1.GetComponent<NukeControl>().name;
			if(GetComponent<UnitBuilder>().Slave1.GetComponent<NukeControl>().initiateLaunch){
				statusExtra+=": Launched\n" ;
			}else{
				statusExtra+=": Ready\n" ;
			}
		}
		PowerControl pow = GetComponent<PowerControl> ();
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" +
		              "Stored Power: " + pow.getCurrentWattHours().ToString() + " / " + pow.getMaxWattHours().ToString() + "\n" +
		              resourceText + statusExtra
		              ,statusStyle);
	}
	
	void GUIDeliveryDrone(){
		Rect GUIbutton;
		for(int i=1; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber])){
						//BUTTON SPECIFIC CODE HERE
						if(buttonnumber==5){
							status = 1; 
							targeting=true;
							masterselector.Suspend();
						}
					}
				}}}
		if (targeting) { //targeting an attack, change cursor texture
			GUI.DrawTexture(new Rect(Input.mousePosition.x - masterCursorSize/2, 
			                         Screen.height- Input.mousePosition.y - (masterCursorSize/2), masterCursorSize, masterCursorSize), targetcursor);
		}
		
		//Draw Status screen
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		                       Screen.width/5f, (Screen.height/4)+Screen.height/45);
		UnitLife life = GetComponent<UnitLife> ();
		
		GUI.TextField(origin, "Health: " + life.getHealth().ToString() + " / "  + life.getMaxHealth().ToString() + "\n" + 
		              GetComponent<ResourceCarrier>().TYPE + " Carried: " + GetComponent<ResourceCarrier>().resourcesCarried
		              ,statusStyle);
	}
	#endregion

	#region selection related methods
	public void Select(){
		selected = true;
		SelectionBox.enabled=true;
	//	Debug.Log ("Selection Order Accepted " + gameObject);
	}
	public void Deselect(){
		selected = false;
		targeting = false;
		SelectionBox.enabled=false;
		if(rangeIndicator!=null && rangeIndicator.GetComponent<Renderer>().enabled){
			rangeIndicator.GetComponent<Renderer>().enabled = false; //disable range indicator when deselected (for always-on indicators)
		}
	}
	public void Prioritize(){
		showGUI = true;
	}
	public void Deprioritize(){
		showGUI = false;
	}
	public bool isSelected(){
		return selected;
	}
	public void Remove(){ //invoke before object death
		masterselector.RemoveObject (gameObject);
	}
	public void setHealthOnly(bool status){
		healthOnlyDisplay = status;
	}
	/// <summary>
	/// Checks local costs of special, steel, fuel, exotics, uranium, and local power against the input resource stores of this unit
	/// Returns true if can afford, and deducts the resources immediately. Returns false otherwise and does nothing
	/// </summary>
	/// <returns><c>true</c>, if can afford and deducted locally, <c>false</c> otherwise.</returns>
	/// <param name="cost">Cost.</param>
	public bool checkCostLocallyandDeduct(int[] cost){
		ResourceControl res = GetComponent<ResourceControl> ();
		PowerControl pow = GetComponent<PowerControl> ();
		bool canBuild = true;
		//if has enough resources
		if (res != null) {
			if (res.getIndexofInputType ("Special") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Special")) >= cost [0]) {

				} else {
					//	Debug.Log ("failed due to special");
					canBuild = false;
				}
			}

			if (res.getIndexofInputType ("Steel") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Steel")) >= cost [1]) {
				
				} else {
					//	Debug.Log ("failed due to steel");
					canBuild = false;
				}
			}

			if (res.getIndexofInputType ("Fuel") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Fuel")) >= cost [2]) {
				
				} else {
					//	Debug.Log ("failed due to exotics");
					canBuild = false;
				}
			}

			if (res.getIndexofInputType ("Exotics") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Exotics")) >= cost [3]) {
				
				} else {
					//	Debug.Log ("failed due to fuel");
					canBuild = false;
				}
			}

			if (res.getIndexofInputType ("Enriched Uranium") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Enriched Uranium")) >= cost [4]) {
				
				} else {
					//	Debug.Log ("failed due to uranium");
					canBuild = false;
				}
			}
		}

		if (pow != null) {
			if (!pow.isEmpty (cost [5])) {
			} else {
				//	Debug.Log ("failed due to power");
				canBuild = false;
			}
		}

		if( canBuild ){
			if(res!=null){
				if (res.getIndexofInputType ("Special") != -1) {
					res.drainInput(cost[0], res.getIndexofInputType("Special"));}
				if (res.getIndexofInputType ("Steel") != -1) {
					res.drainInput(cost[1], res.getIndexofInputType("Steel"));}
				if (res.getIndexofInputType ("Fuel") != -1) {
					res.drainInput(cost[2], res.getIndexofInputType("Fuel"));}
				if (res.getIndexofInputType ("Exotics") != -1) {
					res.drainInput(cost[3], res.getIndexofInputType("Exotics"));}
				if (res.getIndexofInputType ("Enriched Uranium") != -1) {
					res.drainInput(cost[4], res.getIndexofInputType("Enriched Uranium"));}
			}
			if(pow!=null){
				pow.drain(cost[5]);
			}

			return true;
		}else{
			return false;
			//TODO can't afford error message
		}
	}

	/// <summary>
	/// Checks local costs of special, steel, fuel, exotics, uranium, and local power against the input resource stores of this unit
	/// Returns true if can afford, but does not deduct any resources. Returns false otherwise and does nothing
	/// </summary>
	/// <returns><c>true</c>, if can afford and deducted locally, <c>false</c> otherwise.</returns>
	/// <param name="cost">Cost.</param>
	public bool checkCostLocally(int[] cost){
		ResourceControl res = GetComponent<ResourceControl> ();
		PowerControl pow = GetComponent<PowerControl> ();
		bool canBuild = true;
		//if has enough resources
		if (res != null) {
			if (res.getIndexofInputType ("Special") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Special")) >= cost [0]) {
					
				} else {
					//	Debug.Log ("failed due to special");
					canBuild = false;
				}
			}
			
			if (res.getIndexofInputType ("Steel") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Steel")) >= cost [1]) {
					
				} else {
					//	Debug.Log ("failed due to steel");
					canBuild = false;
				}
			}
			
			if (res.getIndexofInputType ("Fuel") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Fuel")) >= cost [2]) {
					
				} else {
					//	Debug.Log ("failed due to exotics");
					canBuild = false;
				}
			}
			
			if (res.getIndexofInputType ("Exotics") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Exotics")) >= cost [3]) {
					
				} else {
					//	Debug.Log ("failed due to fuel");
					canBuild = false;
				}
			}
			
			if (res.getIndexofInputType ("Enriched Uranium") != -1) {
				if (res.getCurrentInputResource (res.getIndexofInputType ("Enriched Uranium")) >= cost [4]) {
					
				} else {
					//	Debug.Log ("failed due to uranium");
					canBuild = false;
				}
			}
		}
		
		if (pow != null) {
			if (!pow.isEmpty (cost [5])) {
			} else {
				//	Debug.Log ("failed due to power");
				canBuild = false;
			}
		}
		
		if( canBuild ){
			return true;
		}else{
			return false;
			//TODO can't afford error message
		}
	}
	//Prevents selecting something as you click to make an attack or whatever
	//should sanity check this to make sure nobody sneaks a suspend in just before resume
	IEnumerator FreeMasterSelector (){
		yield return new WaitForSeconds (.1f);
		masterselector.Resume ();
		yield return null;
	}
		#endregion
}
