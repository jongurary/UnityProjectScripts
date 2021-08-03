using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionOneEvents : MonoBehaviour {

	private Rect GUIbackdrop;
	private Rect GUITextZone;
	private Rect GUIImageZone;
	private GUIStyle textStyle = new GUIStyle();
	private bool Scene=false; //is a scene playing

	private string displayText="";
	private Texture2D displayTexture;
	
	public Builder builder;
	public ResourceManager resourceManager;
	public OrbitalDebrisManager orbitalManager;
	public AISpawner spawner;

	public GameObject timerTextGUI;
	private int timerStatus = 0;
	const int TIMER_OFF = 0;
	const int TIMER_ON = 1;
	const int TIMER_FINISHED = 2;

	[Tooltip("Backgrounds for character portriats")]
	public Texture2D[] imageTextures = new Texture2D[50];
	const int EMPTY = 0;
	const int STEWART = 1;
	const int FEMALE_ASSISTANT_PASSIVE = 2;
	const int SCIENTIST = 3;
	const int FEMALE_PILOT = 4;
	const int CAPTAIN = 5;
	const int FEMALE_ASSISTANT_TALKING = 6;
	const int FEMALE_ASSISTANT_FLAT = 7;
	const int MALE_PILOT = 8;

	public GameObject[] indicators = new GameObject[50];
	const int BUILD_ORBITAL = 0;
	const int BUILD_MINE_1 = 1;
	const int BUILD_MINE_2 = 2;
	const int BUILD_SOLAR = 3;
	const int BUILD_SUBSTATION = 4;
	const int BUILD_TURRET = 5;

	private bool[] events = new bool[50];
	const int ORBITAL_BUILT = 0;
	const int MINE_BUILT_1 = 1;
	const int MINE_BUILT_2 = 2;
	const int SOLAR_BUILT = 3;
	const int SUBSTATION_BUILT = 4;
	const int SUBSTATION_POWERED = 5; 
	const int MINES_POWERED = 6; 
	const int SUBSTATION_TWO_IN_CONNECTIONS = 7;
	const int REFINERY_BUILT = 8;
	const int REFINERY_OPERATIONAL = 9;
	const int ORBITAL_HAS_STEEL = 10;
	const int FUEL_BUILT = 11;
	const int FUEL_POWERED = 12;
	const int ORBITAL_SUPPLIED = 13;
	const int ROCKET_LAUNCHED = 14;
	const int TURRET_BUILT_1 = 15;
	const int TURRET_BUILT_2 = 16;
	const int TURRETS_POWERED = 17;
	const int ROLYS_KILLED = 18;
	const int SUPPLIER_BUILT = 19;
	const int SUPPLIER_SUPPLIED = 20;
	const int STEEL_IN_ORBIT = 21;
	const int ORBITAL_DESTROYED = 22;

	private GameObject launcher;
	private GameObject mine1;
	private GameObject mine2;
	private GameObject substation;
	private GameObject refinery; 
	private GameObject fuel;
	private GameObject turret1;
	private GameObject turret2;
	private GameObject supplier;

	void Start () {
		textStyle.fontSize = (int) (Screen.width / Constants.FONT_SIZE_DIVISOR / 1.5);
		textStyle.normal.textColor = Color.cyan;
		//textStyle.normal.background = imageTextures[0];
		textStyle.wordWrap = true;
		textStyle.padding = new RectOffset (10, 10, 10, 10);

		displayTexture = imageTextures [0];
		if (builder == null) {
			builder = GameObject.FindGameObjectWithTag ("Builder").GetComponent<Builder> ();
		}

		if (orbitalManager == null) {
			orbitalManager = GetComponent<OrbitalDebrisManager>();
		}

		StartCoroutine (checkEvents());
		StartCoroutine (Scene_Intro());
	}

	void Update () {
		
	}

	private void OnGUI(){

		//Draw large background box
		GUIbackdrop = new Rect (Screen.width/2 - Screen.width/4, (Screen.height - Screen.height / 5) - Screen.height / 45, 
		                        Screen.width/2, (Screen.height / 5) + Screen.height / 45);
		//GUI.Box (GUIbackdrop, "");

		GUITextZone = new Rect (Screen.width/2 - Screen.width/4 + Screen.width/8, (Screen.height - Screen.height / 4), 
		                        Screen.width/2 - Screen.width/4 + Screen.width/8, (Screen.height / 5) + Screen.height / 45);

		GUIImageZone = new Rect (Screen.width/2 - Screen.width/4 - Screen.width/32, (Screen.height - Screen.height / 4), 
		                         Screen.width/8, Screen.width/8);
		
		GUI.depth = 3;
		GUI.Box (GUITextZone, displayText, textStyle);
		GUI.DrawTexture (GUIImageZone, displayTexture);
			
	}

	/**
	 * Scans periodically for the completion of various campaign related events.
	 */
	IEnumerator checkEvents(){
		while (true) {
			//TODO skip tutorial condition

			yield return new WaitForSeconds (2f);
			//Orbital build
			if(!events [ORBITAL_BUILT]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("OrbitalLauncher")){
						launcher = builder.dirtyLastBuilt;
						events[ORBITAL_BUILT]=true;
					}}}

			//Orbital has been destroyed (DEFEAT)
			if(events [ORBITAL_BUILT] && !events[ORBITAL_DESTROYED]){
				if(launcher==null || (launcher!=null && launcher.GetComponent<UnitLife>().getHealth()<1) ){
					events[ORBITAL_DESTROYED] = true;
					//TODO lose mission
				}}

			//Mine builds
			if(!events[MINE_BUILT_1]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("MetalMine")){
						mine1 = builder.dirtyLastBuilt;
						events[MINE_BUILT_1]=true;
						builder.dirtyFlag = null;
					}}}
			if(events[MINE_BUILT_1] && !events[MINE_BUILT_2]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("MetalMine")){
						mine2 = builder.dirtyLastBuilt;
						events[MINE_BUILT_2]=true;
					}}}

			//Solar build
			if(!events[SOLAR_BUILT]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("SolarGenerator")){
						events[SOLAR_BUILT]=true;
					}}}

			//Substation build
			if(!events[SUBSTATION_BUILT]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("Substation")){
						substation = builder.dirtyLastBuilt;
						events[SUBSTATION_BUILT]=true;
					}}}

			//Substation powered
			if(!events[SUBSTATION_POWERED]){
				if(substation!=null && substation.GetComponent<PowerControl>().getCurrentWattHours() > 1){
					events[SUBSTATION_POWERED] = true;
				}
			}

			//Mines powered
			if(!events[MINES_POWERED]){
				if(mine1!=null && mine1.GetComponent<PowerControl>().getCurrentWattHours() > 1 &&
				   mine2!=null && mine2.GetComponent<PowerControl>().getCurrentWattHours() > 1){
					events[MINES_POWERED] = true;
				}
			}

			//Substation powered at least twice
			if(!events[SUBSTATION_TWO_IN_CONNECTIONS]){
				if(substation!=null && substation.GetComponent<Linkage>().getInLinks() >= 2){
					events[SUBSTATION_TWO_IN_CONNECTIONS] = true;
				}
			}

			//Refinery built
			if(!events[REFINERY_BUILT]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("Refinery")){
						refinery = builder.dirtyLastBuilt;
						events[REFINERY_BUILT]=true;
					}}}
			
			//Refinery operational
			if(!events[REFINERY_OPERATIONAL]){
				if(refinery!=null && refinery.GetComponent<PowerControl>().getCurrentWattHours() > 1 &&
				   !refinery.GetComponent<ResourceControl>().isEmpty(5, 0) ){
						events[REFINERY_OPERATIONAL]=true;
					}}

			//Orbital has steel
			if(!events[ORBITAL_HAS_STEEL]){
				if(launcher!=null && !launcher.GetComponent<ResourceControl>().isEmptyInput(5, 0) ){
					events[ORBITAL_HAS_STEEL]=true;
				}}

			//Fuel synth built
			if(!events[FUEL_BUILT]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("FuelSynth")){
						fuel = builder.dirtyLastBuilt;
						events[FUEL_BUILT]=true;
					}}}

			//Fuel synth powered up
			if(!events[FUEL_POWERED]){
				if(fuel!=null && fuel.GetComponent<PowerControl>().getCurrentWattHours() > 100){
					events[FUEL_POWERED]=true;
				}}

			//Orbital has all supplies
			if(!events[ORBITAL_SUPPLIED]){
				if(launcher!=null && !launcher.GetComponent<ResourceControl>().isEmptyInput(500, 0) &&
				   !launcher.GetComponent<ResourceControl>().isEmptyInput(200, 3) &&
				   launcher.GetComponent<PowerControl>().getCurrentWattHours() > 1){
					events[ORBITAL_SUPPLIED]=true;
				}}

			//Rocket received in orbit
			if(!events[ROCKET_LAUNCHED]){
				if(resourceManager.dirtyRocketSent){
					resourceManager.dirtyRocketSent = false;
					events[ROCKET_LAUNCHED]=true;
				}}


			//Turret builds
			if(!events[TURRET_BUILT_1]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("GatlingTurret")){
						turret1 = builder.dirtyLastBuilt;
						events[TURRET_BUILT_1]=true;
						builder.dirtyFlag = null;
					}}}
			if(events[TURRET_BUILT_1] && !events[TURRET_BUILT_2]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("GatlingTurret")){
						turret2 = builder.dirtyLastBuilt;
						events[TURRET_BUILT_2]=true;
					}}}
			//Turrets powered
			if(!events[TURRETS_POWERED]){
				if(turret1!=null && turret1.GetComponent<PowerControl>().getCurrentWattHours() > 1 &&
				   turret2!=null && turret2.GetComponent<PowerControl>().getCurrentWattHours() > 1){
					events[TURRETS_POWERED] = true;
				}}

			//Roly Polys killed
			if(!events[ROLYS_KILLED]){
				if(spawner!=null && spawner.killCount > 4){
					events[ROLYS_KILLED] = true;
				}}

			//Supplier built
			if(!events[SUPPLIER_BUILT]){
				if(builder.dirtyFlag!=null){
					if(builder.dirtyFlag.name.Equals("Ammo Supplier")){
						supplier = builder.dirtyLastBuilt;
						events[SUPPLIER_BUILT]=true;
					}}}
			//Supplier supplied
			if(!events[SUPPLIER_SUPPLIED]){
				if(supplier!=null && !supplier.GetComponent<ResourceControl>().isEmpty(1, 0) &&
				   supplier.GetComponent<PowerControl>().getCurrentWattHours() > 1){
					events[SUPPLIER_SUPPLIED]=true;
				}}

			//Steel in orbit
			if(!events[STEEL_IN_ORBIT]){
				if(resourceManager!=null && resourceManager.getResourceWithTag("Steel") > 30000){
					events[STEEL_IN_ORBIT]=true;
				}}
			
		}
	}

	IEnumerator Scene_Intro(){
		yield return StartCoroutine(updateText ("This tutorial will teach you the basics of gameplay. " +
			"In most missions, you are timed, and your resources are carried over to the next mission. " +
			"The resources you earn or spend in this mission will not carry on."));

		displayTexture = imageTextures [STEWART];
		yield return StartCoroutine(updateText ("Orbit stable. She's all yours ground team."));
		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		indicators [BUILD_ORBITAL].gameObject.SetActive (true);
		builder.enabledButtons [Constants.orbitalLauncher] = true;
		yield return StartCoroutine(updateText ("Roger that aviation. Dropping orbital platform now.\n\n" +
            "Tutorial: Press \"t\" to access the resource menu, then press \"z\" " +
			"and click a desired location to drop an orbital launcher."));
		yield return new WaitUntil(()=> events [ORBITAL_BUILT]);
		indicators [BUILD_ORBITAL].gameObject.SetActive (false);
		builder.enabledButtons [Constants.orbitalLauncher] = false;

		displayTexture = imageTextures [FEMALE_PILOT];
		yield return StartCoroutine(updateText ("Ground crew, where the fuck is my steel? We'll " +
			"be making drones out of wall panels next jump."));
		builder.enabledButtons [Constants.metalMine] = true;
		indicators [BUILD_MINE_1].gameObject.SetActive (true);
		indicators [BUILD_MINE_2].gameObject.SetActive (true);

		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("You're in luck fighter team. We've got iron. " + 
            "Getting some mines down stat.\n\n" +
            "Tutorial: Press \"t\" to access the resource menu, then press \"a\" " +
            "to drop a mine on an iron node. Mines can only be built on resources."));
		yield return new WaitUntil(()=> events [MINE_BUILT_1] && events[MINE_BUILT_2]);
		indicators [BUILD_MINE_1].gameObject.SetActive (false);
		indicators [BUILD_MINE_2].gameObject.SetActive (false);

		displayTexture = imageTextures [STEWART];
		builder.enabledButtons [Constants.solarGenerator] = true;
		indicators [BUILD_SOLAR].gameObject.SetActive (true);
		yield return StartCoroutine(updateText ("Come on idiots. Mines need power. There's a nice sunny " +
		    "spot right in the middle. Get some solar on it before it gets dark.\n\n" + 
            "Tutorial: Press \"q\" to access the power menu, then press \"s\"  to drop a solar panel. " +
            "Solar panels are relatively cheap, produce little power, and their performance depends on the level of daylight. Most buildings, including " +
            "mines and orbital platforms, require power."));
		yield return new WaitUntil(()=> events [SOLAR_BUILT]);
		indicators [BUILD_SOLAR].gameObject.SetActive (false);
		builder.enabledButtons [Constants.substation] = true;
		indicators [BUILD_SUBSTATION].gameObject.SetActive (true);

		yield return StartCoroutine(updateText ("Oh crap. You built it too far from the mines. Dummy. " +
            "I told you to put it in the sunny spot.\n\n" + 
            "Tutorial: Power plants need to be connected to buildings, but plants only have a single short cable. " +
            "Access the infrastructure tab using \"w\", build a substation in range of your solar using \"s\""));
		yield return new WaitUntil(()=> events [SUBSTATION_BUILT]);
		indicators [BUILD_SUBSTATION].gameObject.SetActive (false);

		builder.enabledButtons [Constants.transmissionLine] = true;
		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("Jeez. What's going on down there. Commander, you should know better than " +
		    "to listen to Private Eric. Let's see if we can salvage this setup. Get that substation hooked up.\n\n" + 
		    "Tutorial: Click on your solar plant, then press \"a\" to create an outbound link. " +
		    "Click on the substation to make a connection. Substations can handle four inbound links and four outbound links. " +
		    "Remember, power flows only in one direction, from source to destination! "));
		yield return new WaitUntil(()=> events [SUBSTATION_POWERED]);

		displayTexture = imageTextures [FEMALE_ASSISTANT_TALKING];
		yield return StartCoroutine(updateText ("Ok, now let's get some power lines hooked up to those mines. Damn. We're " +
            "going to have to deal with some power losses.\n\n" + 
            "Tutorial: Press \"w\", then press \"a\" to build a power line. Connect your substation to the lines, then each line to a mine. " +
            "Lines handle a single link in both directions. Some power is lost depending on travel distance. The less distance you have to transmit " +
            "power, the less power is lost. "));
		yield return new WaitUntil(()=> events [MINES_POWERED]);

		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("A single solar panel isn't going to be enough, we need more power to drive both mines. " +
            "Eventually, we need to power our orbital platform as well. Drop another solar panel and hook it up to the substation. \n\n" + 
            "Tutorial: To see if a building is operating at maximum efficiency, click on it to see the power stored in its internal batteries. " +
            "Buildings running at full power use any surplus to fill their batteries, a building with an empty power storage is probably short on power. "));
		yield return new WaitUntil(()=> events [SUBSTATION_TWO_IN_CONNECTIONS]);

		builder.enabledButtons [Constants.metalRefine] = true;
		displayTexture = imageTextures [FEMALE_ASSISTANT_TALKING];
		yield return StartCoroutine(updateText ("The mines are starting to fill up. Let's get a refinery down and start making steel. " +
            "Captain Erin will rip our heads off if the aliens break warp before we send more steel into orbit. \n\n" + 
            "Tutorial: Press \"t\" then press \"s\" to drop a refinery. These essential structures smelt useless iron and exotic ores into usable alloy. " +
            "Make sure your refinery is powered! "));
		yield return new WaitUntil(()=> events [REFINERY_BUILT]);

		yield return StartCoroutine(updateText ("The mines are filling up fast, let's get that metal rolling out.\n\n" + 
            "Tutorial: Click on a mine, and press \"a\" to build a hover truck. Click on the refinery as its destination. " +
            "Vehicles require energy to produce, but operate for free. A mine will need several transport vehicles to operate at max efficiency. The further " +
            "they have to travel, the more you will need. And again, make sure your refinery is powered! "));
		yield return new WaitUntil(()=> events [REFINERY_OPERATIONAL]);

		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("Ok, nice work. We've got a good head start today, I guess we got lucky. " +
	        "We might even be able to get some steel in orbit before..."));

		displayTexture = imageTextures [CAPTAIN];
		yield return StartCoroutine(updateText ("This is your Commander speaking. Hostiles have broken warp and powered on weapons. " +
            "All fighter crews to deck. Ground crew, prepare for hostile drop activity. Orbit relative angle delta five N seven E. " +
            "We all know the drill people, survive another day."));

		displayTexture = imageTextures [FEMALE_PILOT];
		yield return StartCoroutine(updateText ("Battlestations god damn it, launch the drones, prime the flak cannons. " +
			"Hurry up you lazy louses. We have aliens to kill. And ground crew, if you're listening, I'm going to be killing a lot of aliens today, " +
			"but if I don't get my steel, I might need to take a more personal touch with one of you."));

		displayTexture = imageTextures [FEMALE_ASSISTANT_FLAT];
		yield return StartCoroutine(updateText ("Yikes, we might need to hurry. The swarm looks thin today, Alice might kill them all before we enter warp. " +
	        "Let's start moving some steel to the orbital platform.\n\n" +
	        "Tutorial: Buildings are constructed in the orbital fabricator aboard the ship. Most resources are useless they are launched back into orbit. " +
	        "Many missions will require you to put resources into orbit. Start a hover truck route from your refinery to the orbital platform to begin " +
	        "moving steel aboard. "));
		yield return new WaitUntil(()=> events [ORBITAL_HAS_STEEL]);

		builder.enabledButtons [Constants.fuelSynth] = true;
		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("Okay, now we need a bit of fuel. Scanners didn't pick up any hydrocarbons, but it looks like there's enough " +
			"atmosphere on this planet to make our own.\n\n" +
	        "Tutorial: Some units, like rockets, consume too much power to run on batteries, and must use fuel. On planets with an atmosphere, the fuel " +
	        "synthesizer can convert energy into fuel, however this process requires a lot of power. Press \"t\", then press \"f\" to build a fuel synethsizer. " +
	        "Fuel does not need to be refined."));
		yield return new WaitUntil(()=> events [FUEL_BUILT]);

		yield return StartCoroutine(updateText ("Fuel synths consume a lot of power, make sure we have enough grid capacity.\n\n" +
            "Tutorial: Time to expand your power grid. Remember, most structures can only accept a single inbound and " +
            "outbound link, you will need to chain substations together to share power resources. To destroy an existing link, " +
            "click on a structure, press \"s\", and choose a target to destroy an outbound link. The mission will advance when your fuel synthesizer is powered."));
		yield return new WaitUntil(()=> events [FUEL_POWERED]);

		displayTexture = imageTextures [FEMALE_ASSISTANT_FLAT];
		yield return StartCoroutine(updateText ("That's a beautiful looking grid, Commander. Let's get the launchpad fueled up and start blasting rocks " +
	        "at orbital command. Captain Alice is already sending some of our steel back our way. \n\n" +
	        "Tutorial: Send fuel and steel trucks to the orbital launcher, to fill up its reserves. Also, remember to power your launcher. " +
	        "Launchers can build rockets to send resources into orbit, but these rockets consume steel, fuel, and power. " +
	        "The war in orbit is fierce, and sometimes debris rains down on the battlefield."));
		orbitalManager.Engage (); //start raining debris
		yield return new WaitUntil(()=> events [ORBITAL_SUPPLIED]);

		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("Orbital launcher is all set for fabrication and launch. Just in time too, hostiles are dropping down.\n\n" +
            "Tutorial: Click on your orbital launcher, then press \"z\" to build a rocket. Launchers can only hold one rocket at a time. " +
            "Rockets can ship a limited quantity of resources, hover over the button in the bottom left to check the tooltip for more details. " +
            "When you want to execute a shipment, click on your launcher and press \"a\" to send the rocket into orbit."));
		yield return new WaitUntil(()=> events [ROCKET_LAUNCHED]);

		displayTexture = imageTextures [FEMALE_PILOT];
		yield return StartCoroutine(updateText ("Ground crew, my minions have informed me that my steel has just arrived in our cargo hatch. " +
            "I've decided to take a few moments away from decimating alien drones to say thank you, and to let you know that if we make this " +
            "jump with another 10,000 units I might not decapitate you. Alice out."));

		displayTexture = imageTextures [FEMALE_ASSISTANT_TALKING];
		yield return StartCoroutine(updateText ("I just checked with engineering, I see why Captain Alice is upset. The drone bay is almost empty. " +
            "If we don't get more steel, we might not be able to hold the line in orbit next jump." ));

		displayTexture = imageTextures [STEWART];
		yield return StartCoroutine(updateText ("Hostiles inbound. Relative due north, top of the valley. Pill bugs in mass. "));

		builder.enabledButtons [Constants.gatlingTurret] = true;
		displayTexture = imageTextures [FEMALE_ASSISTANT_FLAT];
		yield return StartCoroutine(updateText ("It's en masse, you idiot, but we really do have pill bugs inbound. Captain Alice is going to have wait " +
            "a little longer, we need to get kinetic turrets online, stat. \n\n" +
            "Tutorial: Enemies will soon advance from the top of the map. Press \"e\", then press \"a\" to build a gatling turret. " +
            "It's best to place defenses away from other buildings; stray fire from kinetic defenses can hurt your own buildings. " +
            "But remember, all buildings require power. Build at least 2 gatling turrets, and run power to them, to continue."));
		indicators [BUILD_TURRET].gameObject.SetActive (true);
		yield return new WaitUntil(()=> events [TURRETS_POWERED]);

		indicators [BUILD_TURRET].gameObject.SetActive (false);
		spawner.gameObject.SetActive (true); //start spawning units
		displayTexture = imageTextures [STEWART];
		yield return StartCoroutine(updateText ("The sweet smell of dead aliens in the morning. Brings a tear to my eye. " +
            "Let's just watch the fireworks for a minute.\n\n" +
            "Tutorial: Kill five Roly-Polys to continue. Roly-Polys are fast-moving, low durability, suicide attack units. " +
            "They are best killed with fast, accurate weapons, such as the laser and gatling turret." ));
		yield return new WaitUntil(()=> events [ROLYS_KILLED]);

		builder.enabledButtons [Constants.ammoSupplier] = true;
		displayTexture = imageTextures [FEMALE_ASSISTANT_FLAT];
		yield return StartCoroutine(updateText ("Alright, alright, enough watching aliens explode. Those guns don't ship with " +
            "infinite ammunition. Back to work Eric, fire up the fabricators.\n\n" +
            "Tutorial: Gatling guns use standard ammunition, made from steel, and drop from orbit with only a small amount of ammunication in storage. " +
            "Press \"t\", then press \"x\" to build an ammo supplier. When connected to steel and power, suppliers will automatically ship ammo " +
            "to all attack units in range. Drop a supplier, and connect it to steel and power resources."));
		yield return new WaitUntil(()=> events [SUPPLIER_SUPPLIED]);

		displayTexture = imageTextures [FEMALE_ASSISTANT_TALKING];
		yield return StartCoroutine(updateText ("That's the spirit. More than enough steel to go around, we can spare some for the aliens. " +
	        "Just make sure there's enough left for Captain Alice. \n\n" +
	        "Tutorial: Remember, in most missions (but not the tutorial), the resources you earn, and spend, are carried over to the next mission. " +
	        "Spend only what you need, or you may have nothing left for the next engagement."));

		displayTexture = imageTextures [FEMALE_PILOT];
		yield return StartCoroutine(updateText ("Ground crew, how dare you turn my precious drone materials into worthless infrastucture? " +
            "Does this look like Gia to you, or is there another reason you want to make your home on this rock? " +
            "I will require 200,000 units of steel as compensation for your wasted materials."));

		displayTexture = imageTextures [FEMALE_ASSISTANT_FLAT];
		yield return StartCoroutine(updateText ("Captain Alice, don't you have drones to command? Maybe you wouldn't lose so many if you took " +
            "more care of them?"));

		displayTexture = imageTextures [FEMALE_PILOT];
		yield return StartCoroutine(updateText ("For your information, my men have already taken control over the orbital field. " +
			"Besides, I'm a trained professional, I can kill aliens with both hands wrapped around your throat, Executor Erin. "));

		displayTexture = imageTextures [FEMALE_ASSISTANT_FLAT];
		yield return StartCoroutine(updateText ("\"Professional?\", you were a cargo pilot two months ago, Alice. You were \"trained\" " +
			"on video games."));

		displayTexture = imageTextures [FEMALE_PILOT];
		yield return StartCoroutine(updateText ("And you were a botanist, Executor, trained on nothing. We all have new jobs now. " +
            "And if I don't get my steel, we don't get drone fighters, and the aliens destroy our ship next jump. You know, the one " +
            "you're sitting in? Big floating thing? Last thing filled with human life in the galaxy? Ring a bell?"));

		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("Ok, ok, I get it. No need to be an asshole about it, Captain. "));

		displayTexture = imageTextures [STEWART];
		yield return StartCoroutine(updateText ("Excuse me ladies, but I just figured out why the drone swarm was so light this jump. " +
			"The aliens just dropped about a thousand Roly Poly fabricators on the surface. You might want to build some more turrets."));

		displayTexture = imageTextures [FEMALE_PILOT];
		yield return StartCoroutine(updateText ("Shit. Ok, drop the nonsense. I need 20,000 units next jump or we're dead. Make it happen. Please."));

		displayTexture = imageTextures [FEMALE_ASSISTANT_PASSIVE];
		yield return StartCoroutine(updateText ("We got this. Let's rocket some steel. \n\n" +
            "Tutorial: Have at least 30,000 steel in orbit. At the end of most missions, maintenance on the ship will consume some of the resources " +
            "that you earn. In this case, Captain Alice's drone construction will cost 20,000 units, leaving you with the remainder. Failing to " +
            "supply the ship with neccessary materials can lead to various negative effects in future missions, potentially costing you the campaign."));
		yield return new WaitUntil(()=> events [STEEL_IN_ORBIT]);

		spawner.regularSpawnUpperbound = 50;
		spawner.difficultyIncreaseTimer = 2f;
		spawner.difficultyIncreaseRate = 10;

		displayTexture = imageTextures [SCIENTIST];
		yield return StartCoroutine(updateText ("Attention crew, this is Sci-bay. Warp coils primed. Initiating " +
            "warp in T-5. Grasp tightly on your atoms. Ensure all quarks are all in the full and upright spin state."));

		displayTexture = imageTextures [CAPTAIN];
		yield return StartCoroutine(updateText ("Warp timer set. Fighter crew, dock drones and prepare for jump. Ground crew, hold the line.\n\n" +
	        "Tutorial: Your orbital launcher must survive until the mission ends."));
		StartCoroutine (timer (300f, "Warp in: ", ""));

		yield return new WaitForSeconds (5f);
		displayTexture = imageTextures [EMPTY];
		yield return StartCoroutine(updateText ("Tutorial: Survive until the timer runs out. Your orbital launcher must survive."));

		yield return new WaitUntil(()=> timerStatus == TIMER_FINISHED);

		//TODO end mission
		yield return StartCoroutine(updateText ("You win (placeholder)"));
		//TODO end mission

		yield break;
	}

	/**
	 * Changes the display text to the given value, at a speed based on the TEXT_DEFAULT_SCROLL_TIME in constants
	 */ 
	IEnumerator updateText(string nextText){
		displayText = "";
		for (int i=0; i<nextText.Length; i++) {
			displayText += nextText[i];
			yield return new WaitForSeconds(Constants.TEXT_DEFAULT_SCROLL_TIME);
		}
		yield return new WaitForSeconds (Constants.TEXT_DEFAULT_END_TIME);
		yield break;
	}

	/**
	 * Sets a timer for some number of seconds, and displays it to the GUI
	 */
	IEnumerator timer(float time, string duringTimer, string afterTimer){
		UnityEngine.UI.Text textBox = timerTextGUI.GetComponent<UnityEngine.UI.Text>();
		timerStatus = TIMER_ON;

		while (time>0) {
			yield return new WaitForSecondsRealtime (1);
			time -= 1;
			if(time<0){
				time = 0;
			}
			int minutes = (int) time / 60;
			int seconds = (int) time % 60;
			textBox.text = duringTimer + minutes + ":" + seconds;
		}
		//terminate
		textBox.text = afterTimer;
		timerStatus = TIMER_FINISHED;
		yield return null;
	}

}
