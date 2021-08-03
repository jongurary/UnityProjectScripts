using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO indicator when object is coming down
public class Builder : MonoBehaviour {

	//Black rect around gui
	private Rect GUIbackdrop;
	private Rect GUIbackdropright; //right side of the screen box
	private int masterCursorSize; //all cursors should scale from the master size
	private GUIStyle statusStyle = new GUIStyle();
	private GUIStyle mouseOverStyle = new GUIStyle();
	[Tooltip("Background for mouseover text")]
	public Texture2D mouseOverBG; //pure black background for mouse over text
	[Tooltip("Highlight texture for which tab is currently selected")]
	public Texture2D highlightTexture; 
	[Tooltip("Background texture for build cooldown")]
	public Texture2D buildLockTexture; 
	private GameObject rangeIndicator; //a display circle for various contruction ranges
	private GameObject buildIndicator; //display circle for construction size
	//button parameters
	private float offwidth; //horizontal spacing between buttons
	private float offheight; //vertical spacing between buttons
	private float buttonwidth;
	private float buttonheight;
	private float corner; //top left of menu box

	public GameObject MainCam;
	public MasterSelector masterselector;

	//0-4 are resereved to switch tabs
	//5-15 are buttons, multiply by ten for remaining "tabs"
	public int tabNumber; //button multiplier
	public bool[] enabledButtons= new bool[65];
	public string[] buttonTitles = new string[65];
	public Texture[] buttonTextures = new Texture[65];
	//public Texture2D targetcursor; //obsolete, uses ground targeting via buildindicator
	
	private bool targeting;
	public GameObject[] buildable = new GameObject[60]; //list of all constructable buildings

	private GameObject toBuild; //object pending construction
	//TODO scale buildSize to camera height
	private float buildSize; //size of the planned construction, in physics largest sphere radius
	private float spawnHeight = 100f; //y offset to spawn orbitally dropped constructions
	
	[Tooltip("Lockout time before another unit can be built.")]
	public float lockOutTime;
	private Rect lockGUI;
	private float lockGUIWidth;
	private Color lockColor; //lock color shifts over time
	private float currentLockTime = 0f; //current lockout time remaining
	private string lockString;
	private bool unlocked; //is the builder locked?

	private Vector3 hitPoint; //for drawing the gizmo, delete later
	private int layerMask = Constants.IGNORE_RAYCAST_LAYERMASK;
	private ResourceManager manager;

	/**
	 * Used by event managers to track built objects. Set to the prefab of the last built structure, and is reset to null only by event managers.
	 * Null by default.
	 */
	public GameObject dirtyFlag = null;

	/**
	 * Used by event managers to track built objects. Set to the specific instance of the last built structure, and is reset to null only by even managers. 
	 * Null be default.
	 */
	public GameObject dirtyLastBuilt = null;

	//TODO Ignore builds in the GUI

	// Use this for initialization
	void Start () {

		//setup button params
		offwidth = Screen.width/(Constants.BUTTON_WIDTH_DIVISOR*8); //horizontal spacing between buttons
		offheight = Screen.height/(Constants.BUTTON_HEIGHT_DIVISOR*8); //vertical spacing between buttons
		buttonwidth = Screen.width/Constants.BUTTON_WIDTH_DIVISOR;
		buttonheight = buttonwidth; // Screen.height/Constants.BUTTON_HEIGHT_DIVISOR;
		corner=Screen.height-4*(buttonheight); //top left of menu box
		masterCursorSize = Screen.width / Constants.CURSOR_SIZE_DIVISOR;
		tabNumber = 0;

		statusStyle.fontSize = Screen.width / Constants.FONT_SIZE_DIVISOR;
		statusStyle.normal.textColor = Color.blue;

		mouseOverStyle.fontSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2; //mouse over font should be small
		mouseOverStyle.normal.textColor = Color.cyan;
		mouseOverStyle.normal.background = mouseOverBG;
		mouseOverStyle.wordWrap = true; //mouseover text should word wrap to fit its box

		unlocked = true;
		
		MainCam = Camera.main.gameObject;
		if (masterselector == null) {
			masterselector=MainCam.GetComponent<MasterSelector>();
		}

		if (rangeIndicator == null) {
			rangeIndicator = GameObject.FindGameObjectWithTag("RangeIndicator");
		}

		if (buildIndicator == null) {
			buildIndicator = GameObject.FindGameObjectWithTag("BuildIndicator");
		}

		if (manager == null) {
			manager = GameObject.FindGameObjectWithTag ("ResourceManager").GetComponent<ResourceManager> ();
		}

		StartCoroutine (updateRangeIndicator ());
		rangeIndicator.GetComponent<Animator> ().speed = .2f;
		buildIndicator.GetComponent<Animator> ().speed = .1f;

		lockGUIWidth = Screen.width/10f;
		lockGUI = new Rect (Screen.width/5f/2-lockGUIWidth/2, (Screen.height-Screen.height/4)-Screen.height/20, 
		                    lockGUIWidth, Screen.height/40);
		lockString = "<<Fabricator Ready>>";
		lockColor = new Color (0, .5f, 0);
	}

	private void OnGUI(){
		
		if (!masterselector.hasSelection()) {
			//Draw large background box
			GUIbackdrop = new Rect (0f, (Screen.height-Screen.height/4)-Screen.height/45, 
			                        Screen.width/5f, (Screen.height/4)+Screen.height/45);
			GUIbackdropright = new Rect (Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
			                             Screen.width/5f, (Screen.height/4)+Screen.height/45);
			string tabName;
			switch(tabNumber){
			case 0:
				tabName="Power Generation";
				break;
			case 1:
				tabName="Support Infrastructure";
				break;
			case 2:
				tabName="Defenses";
				break;
			case 3:
				tabName="Assault Units";
				break;
			case 4:
				tabName="Resource Infrastructure";
				break;
			default:
				tabName="";
				break;
			}

			//Builder lock GUI
			GUI.color = lockColor; //updated by lock Coroutine
			lockGUI = new Rect (Screen.width/5f/2-lockGUIWidth/2, (Screen.height-Screen.height/4)-Screen.height/20, 
			                    lockGUIWidth, Screen.height/40);
			GUI.DrawTexture (lockGUI, buildLockTexture);
			GUI.color = Color.white;
			GUI.Box (lockGUI, lockString);

			//Corner boxes
			GUI.color = new Color (1f, 1f, 1f, 1f);
			GUI.Box (GUIbackdrop, tabName);
			GUI.Box (GUIbackdropright, "");

			//All buttons and hover text
			builderGUI();
			
		}
	}

	private void builderGUI(){

		string displayHoverText=""; //text to display in the popup mouseover window
		bool isHovering = false;
		int hoverBuildable=0; //button that's currently being hovered over

		Rect GUIbuttonTab; //the upper tab area, which switches the role of the remaining 10 buttons

		//Used to highlight which tab is currently selected
		Rect GUIHighlight = new Rect ((buttonwidth * (tabNumber)) + (offwidth*(tabNumber+1)), corner+(buttonheight* (0))+(offheight*(0)), 
		                              buttonwidth, buttonheight);
		GUI.DrawTexture (GUIHighlight, highlightTexture);

		for(int i=1; i<2; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber]){
					//location of the button, used for determining click and mouseover locations
					GUIbuttonTab = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbuttonTab, buttonTextures[buttonnumber])){
						GUI.depth = 1; //buttons have the lowest possible display priority on the gui
						//button click action code begins here
						switch(buttonnumber){
						case 0:
							tabNumber=0;
							break;
						case 1:
							tabNumber=1;
							break;
						case 2:
							tabNumber=2;
							break;
						case 3:
							tabNumber=3;
							break;
						case 4:
							tabNumber=4;
							break;
						default:
							break;
						}
					} //end click processing code
					//mouse over display code begins here
					if(GUIbuttonTab.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						isHovering=false; //assume nothing is being hovered over
						switch(buttonnumber){
						case 0:
							displayHoverText = "Power Generation Structures \n\nProduce electrical power.";
							isHovering=true;
							break;
						case 1:
							displayHoverText = "Support Infrastructure \n\nTransmit, distribute, and store electrical power. Defensive support structures, such as shields.";
							isHovering=true;
							break;
						case 2:
							displayHoverText = "Defenses \n\nStationary defense turrets.";
							isHovering=true;
							break;
						case 3:
							displayHoverText = "Assault Units \n\nBuildings that house and maintain mobile attack units or long-range.";
							isHovering=true;
							break;
						case 4:
							displayHoverText = "Resource Infrastructure \n\nResource gathering and processing facilities. Send resources into orbit to fuel construction, or process them into ammunition.";
							isHovering=true;
							break;
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
				
				}}} //end button array


		Rect GUIbutton;
		for(int i=2; i<4; i++){
			for(int j=1; j<6; j++){
				int buttonnumber=(j-1) + (i-1)*5;
				if(enabledButtons[buttonnumber + (tabNumber*10) ]){
					GUIbutton = new Rect ((buttonwidth * (j-1)) + (offwidth*j), corner+(buttonheight* (i-1))+(offheight*(i-1)), 
					                      buttonwidth, buttonheight);
					if(GUI.Button (GUIbutton, buttonTextures[buttonnumber + (tabNumber*10)])){
						GUI.depth = 1; //buttons have the lowest possible display priority on the gui
						//Button on click code here
						switch(buttonnumber+ (tabNumber*10)){
						case Constants.radioscopicGenerator:
							targeting=true;
							toBuild=buildable[Constants.radioscopicGenerator];
							break;
						case 6:
							targeting=true;
							toBuild=buildable[Constants.solarGenerator];
							break;
						case 7:
							targeting=true;
							toBuild=buildable[Constants.windGenerator];
							break;
						case 8:
							targeting=true;
							toBuild=buildable[Constants.geoGenerator];
							break;
						case Constants.nuclearReactor:
							targeting=true;
							toBuild=buildable[Constants.nuclearReactor];
							break;
						case Constants.transmissionLine: 
							targeting=true;
							toBuild=buildable[Constants.transmissionLine];
							break;
						case Constants.substation: 
							targeting=true;
							toBuild=buildable[Constants.substation];
							break;
						case Constants.shieldGenerator: //belongs further down
							targeting=true;
							toBuild=buildable[Constants.shieldGenerator];
							break;
						case Constants.capacitor: 
							targeting=true;
							toBuild=buildable[Constants.capacitor];
							break;
						case Constants.battery: 
							targeting=true;
							toBuild=buildable[Constants.battery];
							break;
						case Constants.wireless: 
							targeting=true;
							toBuild=buildable[Constants.wireless];
							break;
						case 25: 
							targeting=true;
							toBuild=buildable[Constants.gatlingTurret];
							break;
						case 26: 
							targeting=true;
							toBuild=buildable[Constants.flakTurret];
							break;
						case Constants.cannonTurret: 
							targeting=true;
							toBuild=buildable[Constants.cannonTurret];
							break;
						case 30: 
							targeting=true;
							toBuild=buildable[Constants.laserTurret];
							break;
						case 31: 
							targeting=true;
							toBuild=buildable[Constants.plasmaTurret];
							break;
						case 35: 
							targeting=true;
							toBuild=buildable[Constants.droneBay];
							break;
						case 39: 
							targeting=true;
							toBuild=buildable[Constants.missileLauncher];
							break;
						case 40: 
							targeting=true;
							toBuild=buildable[Constants.airbase];
							break;
						case 45: 
							targeting=true;
							toBuild=buildable[Constants.metalMine];
							break;
						case 46: 
							targeting=true;
							toBuild=buildable[Constants.metalRefine];
							break;
						case Constants.uraniumEnricher: 
							targeting=true;
							toBuild=buildable[Constants.uraniumEnricher];
							break;
						case Constants.fuelSynth: 
							targeting=true;
							toBuild=buildable[Constants.fuelSynth];
							break;
						case Constants.orbitalLauncher: 
							targeting=true;
							toBuild=buildable[Constants.orbitalLauncher];
							break;
						case Constants.ammoSupplier: 
							targeting=true;
							toBuild=buildable[Constants.ammoSupplier];
							break;
						default:
							break;
						}

						if(toBuild!=null){
							//build size is generated using physics.overlapsphere, so just pick the largest 2d dimension for "size"
							if(toBuild.transform.localScale.x > toBuild.transform.localScale.z){
								buildSize = toBuild.transform.localScale.x;
							}else{
								buildSize = toBuild.transform.localScale.z;
							}
						}
						masterselector.Suspend();
						
					} //end if button click code
					//mouse over display code begins here
					if(GUIbutton.Contains(Event.current.mousePosition)){ 
						//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
						isHovering=false; //assume nothing is being hovered over
						hoverBuildable = buttonnumber + (tabNumber*10);
						switch(buttonnumber+ (tabNumber*10)){
						case Constants.radioscopicGenerator:
							displayHoverText = 
								"Radioscopic Generator \n\nThe RTG uses radioactive isotopes to consistently generate a tiny amount of power. Highly uneconomical, useful only as a last resort, or to power remote areas.";
							isHovering=true;
							break;
						case 6:
							displayHoverText = 
								"Solar Panel \n\nHarvests the power of sunlight and rainbows to produce moderate electricity. Peak production at mid-day, no production at night or during dust storms.";
							isHovering=true;
							break;
						case 7:
							displayHoverText = 
								"Wind Mill \n\nHarvests the natural woosh woosh of air to produce a small amount of electricity. Produces more power on higher ground.";
							isHovering=true;
							break;
						case 8:
							displayHoverText = 
								"Geothermal Plant \n\nSucks free power out of safe, warm lava. Requires geothermal node. Produces constant power always and never runs out.";
							isHovering=true;
							break;
						case Constants.nuclearReactor:
							displayHoverText = 
								"Nuclear Reactor \n\nThe power of fission generates massive amounts of energy, but requires enriched uranium. Has a chance to catastrophically meltdown.";
							isHovering=true;
							break;
						case Constants.transmissionLine: 
							displayHoverText = 
								"Transmission Tower \n\nMoves electricity from place to place over great distances. Power losses to inefficiency increase over distance.";
							isHovering=true;
							break;
						case Constants.substation: 
							displayHoverText = 
								"Substation \n\nConnects multiple electrical units to the grid. Connects up to five inputs and outputs.";
							isHovering=true;
							break;
						case Constants.shieldGenerator: //belongs lower down
							displayHoverText = 
								"Shield Generator \n\nProvides a dome of shielding that has a chance to absorb most incoming projectiles. Consumes large amounts of power. Be cautious placing attack units near the edge of your own shield.";
							isHovering=true;
							break;
						case Constants.capacitor: 
							displayHoverText = 
								"Capacitor \n\nStores a moderate amount of electrical power. Can absorb and distribute power faster than any other structure. Supports 2 inputs and 2 outputs. Useful to facilitate quick, powerful bursts of energy on demand.";
							isHovering=true;
							break;
						case Constants.battery: 
							displayHoverText = 
								"Battery \n\nStores a large amount of electrical power. Supports 2 inputs and 1 output. Store excess power to bridge the gap during periods of low power production.";
							isHovering=true;
							break;
						case Constants.wireless: 
							displayHoverText = 
								"Wireless Charger \n\nAutomatically gives power to anything in range. High losses to inefficiency, especially near max range, and max distributed power is relatively low. Useful to recharge multiple low-demand consumers, like drones.";
							isHovering=true;
							break;
						case 25: 
							displayHoverText = 
								"Gatling Turret \n\nKinetic single-target turret. Requires ammunition (steel). Low accuracy, weak against fast targets.";
							isHovering=true;
							break;
						case 26: 
							displayHoverText = 
								"Flak Turret \n\nKinetic anti-air turret. Only attacks air. Requires ammunition (steel). Massive damage to slow-moving large targets, weak against fast targets.";
							isHovering=true;
							break;
						case Constants.cannonTurret: 
							displayHoverText = 
								"Cannon Turret \n\nKinetic high damage area-of-effect turret. Shoots and turns relatively slowly. Requires ammunition (explosive). Massive damage to clustered targets, weak against individual and fast targets.";
							isHovering=true;
							break;
						case 30: 
							displayHoverText = 
								"Laser Turret \n\nEnergy-based single-target turret. Requires no ammo, but consumes significant amounts of power. Never misses.";
							isHovering=true;
							break;
						case 31: 
							displayHoverText = 
								"Plasma Turret \n\nEnergy-based area-of-effect turret. Requires no ammo, but consumes massive amounts of power. Slow moving projectile with a delayed explosion, weak against fast targets.";
							isHovering=true;
							break;
						case 35: 
							displayHoverText = 
								"Drone Bay \n\nConstructs land units.";
							isHovering=true;
							break;
						case 39: 
							displayHoverText = 
								"Missile Launcher \n\nConstructs missiles and nuclear missiles, delivering massive explosive power anywhere in the world.";
							isHovering=true;
							break;
						case 40: 
							displayHoverText = 
								"Air Base \n\nHouses a single unit to rain death on far away lands, or defend the skies from invaders.";
							isHovering=true;
							break;
						case 45: 
							displayHoverText = 
								"Metal Mine \n\nExtracts Iron and Exotic metals from the earth, producing ore. Metal is useless until refined. Also can extract hydrocarbon fuel, which requires no further refining.";
							isHovering=true;
							break;
						case 46: 
							displayHoverText = 
								"Metal Refinery \n\nRefines Iron and Exotic ore into usable materials. A refinery must be supplied by a mine.";
							isHovering=true;
							break;
						case Constants.uraniumEnricher: 
							displayHoverText = 
								"Uranium Enricher \n\nCentrifuges spin useless glowing uranium ore into stable fission material. Behold the atomic age. Requires supply from a uranium mine.";
							isHovering=true;
							break;
						case Constants.fuelSynth: 
							displayHoverText = 
								"Fuel Synthesizer \n\nGenerates hydrocarbon fuel from atmospheric gases by expending large amounts of energy. No input resource required.";
							isHovering=true;
							break;
						case Constants.orbitalLauncher: 
							displayHoverText = 
								"Orbital Launcher \n\nEssential structure. Sends finished materials and energy back into space, allowing for construction of additional structures.";
							isHovering=true;
							break;
						case Constants.ammoSupplier: 
							displayHoverText = 
								"Ammo Supplier \n\nEssential structure. Converts finished materials into all varities of ammunitition and automatically sends it to units in range. Requires power and finished materials such as Steel.";
							isHovering=true;
							break;
						default:
							displayHoverText="";
							isHovering=false;
							break;
						}
					}
				
				}}} //end button array

		//Draw hover dialog, if applicable
		if (isHovering) {
			//TODO scale size to amount of text
			//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
			Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y - Screen.height/6, 
			                                 Screen.width/8f, Screen.height/6 );

			GUI.depth = 3; //hover text display has a high priority
			GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
			//Display unit costs
			if(buildable[hoverBuildable]!=null){
				UnitCost unitCost = buildable[hoverBuildable].GetComponent<UnitCost>();
				if(unitCost!=null){

					float textureSize = Screen.width / Constants.FONT_SIZE_DIVISOR / 2;
						//Screen.height / 64f; //size of res icon
				float textSize = textureSize * 2f; //size of text cost
				float widthPadding = Screen.width / 250f;
				float barHeight = Screen.height / 64f;
				float netoffSet = 0f;
				for(int i=0; i<5; i++){
					if(unitCost.resourceCost[i]>0){
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
						GUI.TextField (ResTipText, unitCost.resourceCost[i].ToString(),toolTipStyle);
						netoffSet += textSize + widthPadding;

					}
			}
			}
			}
		}


		if (targeting) { //targeting construction
			float relativeXScale = toBuild.transform.localScale.x * masterCursorSize / 4;
			float relativeZScale = toBuild.transform.localScale.z * masterCursorSize / 4;

			//obsolete
			/*
			GUI.DrawTexture (new Rect (Input.mousePosition.x - relativeXScale / 2,
			                         Screen.height - Input.mousePosition.y - relativeZScale / 2, 
			                         relativeXScale, relativeZScale), targetcursor); 
			                         */
		} else {
			rangeIndicator.SetActive(false);
			buildIndicator.SetActive(false);
		}

		//Draw Right side menu
		Rect origin = new Rect(Screen.width - Screen.width/5f, (Screen.height-Screen.height/4)-Screen.height/45, 
		Screen.width/5f, (Screen.height/4)+Screen.height/45);

		GUI.TextField(origin, ""
		              ,statusStyle);

	}

	void RunAsBuilder(){
		
		//when not targeting, clicks should do nothing, this is just a sanity check and probably not neccessary
		if (Input.GetMouseButtonDown (1)) {
			targeting=false;
			StartCoroutine(FreeMasterSelector());
		}
		
		if (targeting) { 
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
					GameObject unithit = hit.transform.root.gameObject; //what the raycast has struck, typically terrian
					bool canBuild=true;

						if(unithit.CompareTag("Terrian") //TODO mines should NOT be on terrian? Or change indicator somehow?
					   || (toBuild == buildable[Constants.metalMine] && unithit.CompareTag("ResourceDeposit")) 
					   || (toBuild == buildable[Constants.geoGenerator] && unithit.CompareTag("ResourceDeposit")) ){
							Collider[] proximityCheck = Physics.OverlapSphere (hit.point, buildSize, layerMask);
							hitPoint = hit.point; //for painting the gizmo

						//if the object is a resource mine, check if the hit is a valid resource node
						if(toBuild == buildable[Constants.metalMine]){
							canBuild=false;
							//TODO add other metal types
							if(unithit.CompareTag("ResourceDeposit")){
								if(unithit.GetComponent<ResourceNode>().getType() == "Iron"
								   || unithit.GetComponent<ResourceNode>().getType() == "Uranium"
								   || unithit.GetComponent<ResourceNode>().getType() == "Exotic Metal"
								   || unithit.GetComponent<ResourceNode>().getType() == "Hydrocarbon"){
									canBuild = true;
								}else{
									canBuild = false;
								}
							}
						}

						//if the object is a geothermal power supply, check if the hit is a valid resource node
						if(toBuild == buildable[Constants.geoGenerator]){
							canBuild=false;
							//TODO add other metal types
							if(unithit.CompareTag("ResourceDeposit")){
								if(unithit.GetComponent<ResourceNode>().getType() == "Geothermal"){
									canBuild = true;
								}
							}
						}
						
						
						//check if an existing building blocks construction
							foreach(Collider col in proximityCheck){
								if(col.transform.root.gameObject.CompareTag("Building")){
									canBuild = false;
								}
							} //end checking all the colliders

						if(canBuild && unlocked){
							if(toBuild.GetComponent<UnitCost>()!=null){
								UnitCost cost = toBuild.GetComponent<UnitCost>();
								//if has enough resources
								if(manager.canAfford(cost.resourceCost[0], // special cost
								                     cost.resourceCost[1], // steel
								                     cost.resourceCost[2], // fuel
								                     cost.resourceCost[3], // exotics
								                     cost.resourceCost[4], // uranium
								                     cost.orbitalPowerCost // power
								                     )){
									//spend resources 
									manager.consumeResources(cost.resourceCost[0], // special cost
									                         cost.resourceCost[1], // steel
									                         cost.resourceCost[2], // fuel
									                         cost.resourceCost[3], // exotics
									                         cost.resourceCost[4], // uranium
									                         cost.orbitalPowerCost);
									StartCoroutine(LockBuild());
									dirtyLastBuilt = Instantiate (toBuild, hitPoint + new Vector3(0f, spawnHeight, 0f), Quaternion.identity);
								}else{
									//TODO can't afford error message
								}
							}else{ //unit has no cost method, build it for free
								StartCoroutine(LockBuild());
								dirtyLastBuilt = Instantiate (toBuild, hitPoint + new Vector3(0f, spawnHeight, 0f), Quaternion.identity);
							}
						} else if(!unlocked){
							//TODO build cooldown isn't expired error message
						}
							
						}
				}
				targeting=false;
				StartCoroutine(FreeMasterSelector());
			}
		}
		
	}

	//Prevents selecting something as you click to make an attack or whatever
	//should sanity check this to make sure nobody sneaks a suspend in just before resume
	IEnumerator FreeMasterSelector (){
		yield return new WaitForSeconds (.1f);
		masterselector.Resume ();
		yield return null;
	}

	/**
	 * Locks the builder until the lockout timer elapses, and updates all related GUI animations
	 */
	IEnumerator LockBuild (){
		unlocked = false;
		dirtyFlag = toBuild;
		currentLockTime = lockOutTime;
		lockGUIWidth = Screen.width/8f;

		while (currentLockTime-.1f>0) {
			yield return new WaitForSeconds (.1f);
			currentLockTime-=.1f;
			if(lockGUIWidth>0){
				lockGUIWidth = (currentLockTime / lockOutTime) * Screen.width/16f + Screen.width/10f;
			}
			lockColor = new Color((currentLockTime / lockOutTime) * 1f, .5f, 0);
			lockString = "Fabricator Cooling: " + (int) ((currentLockTime / lockOutTime) * 100);
		}
		lockString = "<<Fabricator Ready>>";
		unlocked = true;
		yield return null;
	}

	IEnumerator updateRangeIndicator(){
		while (true) {
			yield return new WaitForSeconds(.05f); //NOTE: possibly inefficient, reduce to make choppier updating of target area
			if(targeting){
				rangeIndicator.SetActive (true);
				buildIndicator.SetActive (true);
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
					GameObject unithit = hit.transform.root.gameObject;




					if(unithit.CompareTag("Terrian") || (toBuild == buildable[Constants.metalMine] && unithit.CompareTag("ResourceDeposit")) ){
						rangeIndicator.transform.position = hit.point + new Vector3(0f,1f,0f); //for circle location determination
						buildIndicator.transform.position = hit.point + new Vector3(0f,1f,0f);
					}

					float rangeSize=0f;
					//TODO possibly ineffienct, use tag-based checking (using builder ID tags) instead
					//TODO add shield range indicator
					if(toBuild.GetComponent<UnitAttack>()!=null){ 
						rangeSize = toBuild.GetComponent<UnitAttack>().attackRange * 2f;
					} else if(toBuild.GetComponent<UnitAttackLaser>()!=null){
						rangeSize = toBuild.GetComponent<UnitAttackLaser>().attackRange * 2f;
					}else if(toBuild.GetComponent<AmmoRefine>()!=null){ //having an AmmoRefine component means a child must have the trigger
						rangeSize = toBuild.GetComponentInChildren<AmmoRefineTrigger>().range * 2f;
					}else if(toBuild.GetComponent<Linkage>()!=null){
						rangeSize = toBuild.GetComponent<Linkage>().linkRange * 2f;
					}
					rangeIndicator.transform.localScale = new Vector3( rangeSize, rangeSize, rangeSize);

					//BUG Medium priority, build indicator is not always representative of size
					buildIndicator.transform.localScale = new Vector3( buildSize * 1.5f, buildSize * 1.5f, buildSize * 1.5f);
				}
			}
		}
	}


	//Draws a sphere in the editor for visualization
	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (hitPoint, buildSize);
	}

	void Update () {

		RunAsBuilder ();

		if (!masterselector.hasSelection ()) {
			//Tab swapper
			if (Input.GetKeyDown ("q")) {
				tabNumber=0;
			} else if (Input.GetKeyDown ("w")) {
				tabNumber=1;
			} else if (Input.GetKeyDown ("e")) {
				tabNumber=2;
			}else if (Input.GetKeyDown ("r")) {
				tabNumber=3;
			}else if (Input.GetKeyDown ("t")) {
				tabNumber=4;
			}

			if (!targeting) { 
				if (tabNumber == 0) { //generator build tab
					if (Input.GetKeyDown ("a") && enabledButtons[Constants.radioscopicGenerator]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.radioscopicGenerator];
					} else if (Input.GetKeyDown ("s") && enabledButtons[Constants.solarGenerator]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.solarGenerator];
					} else if (Input.GetKeyDown ("d") && enabledButtons[Constants.windGenerator]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.windGenerator];
					} else if (Input.GetKeyDown ("f") && enabledButtons[Constants.geoGenerator]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.geoGenerator];
					} 
					//other chars...

					else if (Input.GetKeyDown ("c") && enabledButtons[Constants.nuclearReactor]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.nuclearReactor];
					}
					//more chars
				} else if (tabNumber == 1) { //support infrastructure build tab
					if (Input.GetKeyDown ("a") && enabledButtons[Constants.transmissionLine]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.transmissionLine];
					}else if (Input.GetKeyDown ("s") && enabledButtons[Constants.substation]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.substation];
					}else if (Input.GetKeyDown ("d") && enabledButtons[Constants.capacitor]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.capacitor];
					}else if (Input.GetKeyDown ("f") && enabledButtons[Constants.battery]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.battery];
					}else if (Input.GetKeyDown ("g") && enabledButtons[Constants.wireless]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.wireless];
					}
						
						///other chars.....
					else if (Input.GetKeyDown ("z") && enabledButtons[Constants.shieldGenerator]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.shieldGenerator];
					}
				} else if (tabNumber == 2) { //defensive build tab
					if (Input.GetKeyDown ("a") && enabledButtons[Constants.gatlingTurret]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.gatlingTurret];
					} else if (Input.GetKeyDown ("s") && enabledButtons[Constants.flakTurret]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.flakTurret];
					}else if (Input.GetKeyDown ("d") && enabledButtons[Constants.cannonTurret]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.cannonTurret];
					}

					else if (Input.GetKeyDown ("z") && enabledButtons[Constants.laserTurret]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.laserTurret];
					} else if (Input.GetKeyDown ("x") && enabledButtons[Constants.plasmaTurret]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.plasmaTurret];
					}
				}else if (tabNumber == 3) { //offensive build tab
					if (Input.GetKeyDown ("a") && enabledButtons[Constants.droneBay]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.droneBay];
					}else if (Input.GetKeyDown ("g") && enabledButtons[Constants.missileLauncher]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.missileLauncher];
					}else if (Input.GetKeyDown ("z") && enabledButtons[Constants.airbase]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.airbase];
					}
				}else if (tabNumber == 4) { //resource build tab
					if (Input.GetKeyDown ("a") && enabledButtons[Constants.metalMine]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.metalMine];
					} else 	if (Input.GetKeyDown ("s") && enabledButtons[Constants.metalRefine]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.metalRefine];
					} else if (Input.GetKeyDown ("d") && enabledButtons[Constants.uraniumEnricher]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.uraniumEnricher];
					}else if (Input.GetKeyDown ("f") && enabledButtons[Constants.fuelSynth]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.fuelSynth];
					}

					else if (Input.GetKeyDown ("z") && enabledButtons[Constants.orbitalLauncher]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.orbitalLauncher];
					}else if (Input.GetKeyDown ("x") && enabledButtons[Constants.ammoSupplier]) {
						targeting = true;
						masterselector.Suspend ();
						toBuild = buildable [Constants.ammoSupplier];
					}
				}//more build tabs....
			} 

			if (toBuild != null) {
				//build size is generated using physics.overlapsphere, so just pick the largest 2d dimension for "size"
				if (toBuild.transform.localScale.x > toBuild.transform.localScale.z) {
					buildSize = toBuild.transform.localScale.x;
				} else {
					buildSize = toBuild.transform.localScale.z;
				}
			}
		}
	}

	/**
	 * Get the current time remaining before next build is permitted
	 */
	public float getLockOutTime(){ return currentLockTime;}

}
