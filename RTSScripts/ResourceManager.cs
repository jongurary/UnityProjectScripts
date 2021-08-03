using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

	public int[] storedResource;
	public string[] storedResourceTag;

	/**
	 * Set to true when the resource manager has recently received a rocket containing any resources (not zero).
	 * Set to false by event managers that use this flag.
	 */
	public bool dirtyRocketSent=false;

	private int totalEnergyProduced; //total energy produced by the economy
	private int totalEnergyStored; //total energy in storage in the economy
	public List<PowerControl> powerControllers = new List<PowerControl>(); //all power controllers in the scene
	private int totalEnergySentToOrbit; //total energy transmitted back to orbit
	//TODO energy consumed (per second)

	public Texture specialTexture; //slot for "special" campaign only resources
	public Texture steelTexture;
	public Texture fuelTexture;
	public Texture exoticsTexture;
	public Texture uraniumTexture;
	public Texture energyStoredTexture;
	public Texture energyProducedTexture;
	public Texture energySpaceTexture;

	public Texture2D mouseOverBG;

	[Tooltip("Resource icons for cost tooltips, in the following order: special, steel, exotics, fuel, uranium, power")]
	public Texture[] resourceIcons = new Texture[6];

	private GUIStyle topBarStyle = new GUIStyle();
	private GUIStyle mouseOverStyle = new GUIStyle();

	void Start () {

		topBarStyle.fontSize = Screen.width / (int)(Constants.FONT_SIZE_DIVISOR / .35);
		topBarStyle.normal.textColor = Color.cyan;
		topBarStyle.alignment = TextAnchor.MiddleLeft;

		mouseOverStyle.fontSize = Screen.width / (int)(Constants.FONT_SIZE_DIVISOR / .45); //mouse over font should be small
		mouseOverStyle.normal.textColor = Color.cyan;
		mouseOverStyle.normal.background = mouseOverBG; //TODO change background color maybe
		mouseOverStyle.wordWrap = true; //mouseover text should word wrap to fit its box

		totalEnergyProduced = 0;
		totalEnergyStored = 0;
		totalEnergySentToOrbit = 0;

		StartCoroutine (updateStoredPower ());
	}

	void Update () {
		
	}

	IEnumerator updateStoredPower(){
		while (true) {
			yield return new WaitForSeconds(2f); //update stored power every few seconds, or higher if this doesn't lag too badly.
			totalEnergyStored=0; //reset stored energy to zero
			totalEnergyProduced=0; //reset producted energy to zero
			//calculate stored energy
			foreach(PowerControl pow in powerControllers){
				if(pow!=null){
					incrementEnergyStored( pow.getCurrentWattHours() );
					incrementEnergyProduced( pow.getWatts() );
				}
			}
		}
	}

	/// <summary>
	/// Returns true if there is a sufficient quantity of all listed resources in orbit (Special, steel, fuel, exotics, uranium, orbital energy)
	/// </summary>
	/// <returns><c>true</c>, if afford was caned, <c>false</c> otherwise.</returns>
	/// <param name="special">Special.</param>
	/// <param name="steel">Steel.</param>
	/// <param name="fuel">Fuel.</param>
	/// <param name="exotics">Exotics.</param>
	/// <param name="uranium">Uranium.</param>
	/// <param name="power">Orbital Energy.</param>
	public bool canAfford(int special, int steel, int fuel, int exotics, int uranium, int power){
		if (storedResource [0] >= special && storedResource [1] >= steel && storedResource [2] >= fuel && 
		    storedResource [3] >= exotics && storedResource [4] >= uranium && totalEnergySentToOrbit >= power) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Deducts the proper amount of all resources from orbital supply (special, steel, fuel, exotics, uranium, orbital energy)
	/// Forces resources into negatives if there isn't enough, so verify quantity with canAfford first
	/// </summary>
	/// <returns><c>true</c>, if afford was caned, <c>false</c> otherwise.</returns>
	/// <param name="special">Special.</param>
	/// <param name="steel">Steel.</param>
	/// <param name="fuel">Fuel.</param>
	/// <param name="exotics">Exotics.</param>
	/// <param name="uranium">Uranium.</param>
	/// <param name="power">Orbital Energy.</param>
	public void consumeResources(int special, int steel, int fuel, int exotics, int uranium, int power){
		storedResource [0] -= special; 
		storedResource [1] -= steel; 
		storedResource [2] -= fuel; 
		storedResource [3] -= exotics; 
		storedResource [4] -= uranium; 
		totalEnergySentToOrbit -= power;
	}

	/// <summary>
	/// Update the energy production of the total economy by the provided amount. Accepts negatives.
	/// </summary>
	/// <param name="toUpdate">Amount to update global production.</param>
	public void incrementEnergyProduced( int toUpdate ){
		totalEnergyProduced += toUpdate;
	}

	/// <summary>
	/// Update the energy stored in the total economy by the provided amount. Accepts negatives.
	/// </summary>
	/// <param name="toUpdate">Amount to update global storage.</param>
	public void incrementEnergyStored( int toUpdate ){
		totalEnergyStored += toUpdate;
	}

	/// <summary>
	/// Add a power controller to the list of all power controllers in the scene
	/// </summary>
	/// <param name="obj">Object.</param>
	public void addPowerController( PowerControl pow){
		powerControllers.Add (pow);
	}

	/// <summary>
	/// Remove a power controller from the list of all power controllers in the scene
	/// </summary>
	/// <param name="obj">Object.</param>
	public void removePowerController( PowerControl pow){
		powerControllers.Remove(pow);
	}

	/// <summary>
	/// Returns the amount of the resource matching the string description, for example "Steel"
	/// Tags are case-sensitive. Returns -1 if tag is not found.
	/// </summary>
	/// <returns>Amount of resource with tag, -1 if not found.</returns>
	/// <param name="tag">Tag.</param>
	public int getResourceWithTag(string tag){
		for (int i=0; i<storedResourceTag.Length; i++) {
			if(storedResourceTag[i] == tag){
				return storedResource[i];
			}
		}
		return -1;
	}

	/// <summary>
	/// Sends the amount of the resource matching the string description into orbit, for example "Steel"
	/// Tags are case-sensitive. Returns false if tag is not found.
	/// </summary>
	/// <returns>Amount of resource with tag, true for success, false if tag not found.</returns>
	/// <param name="tag">Tag.</param>
	public bool sendResourceWithTag(string tag, int amount){
	//	Debug.Log ("sending " + tag + " " + amount);
		for (int i=0; i<storedResourceTag.Length; i++) {
			if(storedResourceTag[i] == tag){
				storedResource[i] += amount;
				if(amount>0){
					dirtyRocketSent = true;
				}
				return true;
			}
		}
		return false;
	}
	
	/// <summary>
	/// Test if there is enough of the resource "tag" available to deduct "amount"
	/// Returns false if tag not found, or resource would be negative if amount was deducted
	/// Returns true if resource is valid, and enough is available
	/// </summary>
	/// <returns>True if enough resource is available.</returns>
	/// <param name="tag">Tag.</param>
	/// <param name="amount">Amount (should be positive).</param>
	public bool hasResourcebyTag(string tag, int amount){
		int currentResource = getResourceWithTag (tag);
		if (currentResource == -1 || currentResource < amount) {
			return false;
		} else if (currentResource > amount) {
			return true;
		}
		return false; //shouldn't happen
	}

	/// <summary>
	/// Transmit a certain amount of energy into orbit, generally removing it from the economy
	/// </summary>
	/// <param name="amount">Amount.</param>
	public void sendEnergytoOrbit(int amount){
		totalEnergySentToOrbit += amount;
		return;
	}

	/// <summary>
	/// Consume a certain amount of energy in orbit, if available. Orbital energy can go negative.
	/// </summary>
	/// <param name="amount">Amount.</param>
	public void consumeEnergyinOrbit(int amount){
		totalEnergySentToOrbit -= amount;
		return;
	}

	/// <summary>
	/// Test if a certain amount of energy is in orbit. Return true if there is enough energy in orbit
	/// </summary>
	/// <returns><c>true</c>, if enough energy in orbit, <c>false</c> otherwise.</returns>
	/// <param name="amount">Amount.</param>
	public bool hasEnergyinOrbit(int amount){
		if (totalEnergySentToOrbit > amount) {
			return true;
		} else {
			return false;
		}
	}

	void OnGUI(){

		
		//Draw Status screen. Note: on the y axis, 0 is top of screen

		float textSize = Screen.width / 40f;
		float barWidth = Screen.width - Screen.width / 2.5f; //determines origin of bar and overall width
		float widthPadding = Screen.width / 200f;
		float offsetFromTop = Screen.height / 150;
		float barHeight = Screen.height / 40;
		float netoffSet = 0f;
		bool isHovering = false;
		string displayHoverText = "";

		GUI.depth = 1;
		Rect Special = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (Special, specialTexture);
		
		netoffSet += barHeight + widthPadding;
		
		Rect specialText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (specialText, getResourceWithTag("Special").ToString(), topBarStyle);
		
		netoffSet += textSize + widthPadding;
		//End Special

		Rect steel = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (steel, steelTexture);

		netoffSet += barHeight + widthPadding;
		 
		Rect steelText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (steelText, getResourceWithTag("Steel").ToString(), topBarStyle);

		netoffSet += textSize + widthPadding;
		//End Steel

		Rect fuel = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (fuel, fuelTexture);
		
		netoffSet += barHeight + widthPadding;
		
		Rect fuelText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (fuelText, getResourceWithTag("Fuel").ToString(), topBarStyle);
		
		netoffSet += textSize + widthPadding;
		//End Fuel

		Rect exotics = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (exotics, exoticsTexture);
		
		netoffSet += barHeight + widthPadding;
		
		Rect exoticsText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (exoticsText, getResourceWithTag("Exotics").ToString(), topBarStyle);
		
		netoffSet += textSize + widthPadding;
		//End Exotics

		Rect uranium = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (uranium, uraniumTexture);

		netoffSet += barHeight + widthPadding;

		Rect uraniumText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (uraniumText, getResourceWithTag("Enriched Uranium").ToString(), topBarStyle);

		netoffSet += textSize + widthPadding;
		//End Uranium
		
		Rect energy = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (energy, energyStoredTexture);
		
		netoffSet += barHeight + widthPadding;
		
		Rect energyText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (energyText, totalEnergyStored.ToString(), topBarStyle);

		netoffSet += textSize + widthPadding;
		
		Rect energyProduced = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (energyProduced, energyProducedTexture);
		
		netoffSet += barHeight + widthPadding;
		//End Energy
		
		Rect energyProducedText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (energyProducedText, totalEnergyProduced.ToString(), topBarStyle);

		netoffSet += textSize + widthPadding;
		
		Rect energySpace = new Rect (barWidth + netoffSet, offsetFromTop, barHeight, barHeight );
		GUI.DrawTexture (energySpace, energySpaceTexture);
		
		netoffSet += barHeight + widthPadding;
		//End energy produced
		
		Rect energySpaceText = new Rect (barWidth + netoffSet, offsetFromTop, textSize, barHeight);
		GUI.TextField (energySpaceText, totalEnergySentToOrbit.ToString(), topBarStyle);
		
		
		//Note: instead of Event, Input.mouseposition can be used, however the y axis is inverted, do Screen.height - pos.y first
		if (Special.Contains (Event.current.mousePosition) || specialText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
			//			Debug.Log ("hovering");
			displayHoverText = "Special resource held in orbit.";
		}else if (steel.Contains (Event.current.mousePosition) || steelText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
//			Debug.Log ("hovering");
			displayHoverText = "Steel held in orbit.";
		} else if (fuel.Contains (Event.current.mousePosition) || fuelText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
			//			Debug.Log ("hovering");
			displayHoverText = "Fuel held in orbit.";
		} else if (exotics.Contains (Event.current.mousePosition) || exoticsText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
			//			Debug.Log ("hovering");
			displayHoverText = "Exotics held in orbit.";
		} else if (uranium.Contains (Event.current.mousePosition) || uraniumText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
			displayHoverText = "Uranium held in orbit.";
		} else if (energy.Contains (Event.current.mousePosition) || energyText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
			displayHoverText = "Total energy stored along all units.";
		}else if (energyProduced.Contains (Event.current.mousePosition) || energyProducedText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
			displayHoverText = "Total energy produced along all units.";
		}else if (energySpace.Contains (Event.current.mousePosition) || energySpaceText.Contains (Event.current.mousePosition)) { 
			isHovering = true;
			displayHoverText = "Energy available in orbit.";
		}

	//Draw hover dialog, if applicable
	if (isHovering) {
		//TODO scale size to amount of text
		//offset placement by height of the rect so the cursor is the bottom-left of the rectangle
				Rect GUITextbackdrop = new Rect (Event.current.mousePosition.x -Screen.width/8f, Event.current.mousePosition.y, 
				                                 Screen.width/8f, barHeight );
		GUI.depth = 3; //hover text display has a high priority
		GUI.Box (GUITextbackdrop, displayHoverText, mouseOverStyle);
	}

	}
	
}
