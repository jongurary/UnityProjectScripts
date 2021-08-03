using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls cost of unit and structure constructions
/// </summary>
public class UnitCost : MonoBehaviour {

	//Material costs
	[Tooltip("Corresponding amount of each resource required for construction. Tagged in the following order: special, steel, fuel, exotics, uranium")]
	public int[] resourceCost = new int[5];
	[Tooltip("Cost in orbital power")]
	public int orbitalPowerCost;
	[Tooltip("Cost in local power paid by the builder")]
	public int localPowerCost;

	private ResourceManager manager;

	void Start () {
		if (manager == null) {
			manager = GameObject.FindGameObjectWithTag ("ResourceManager").GetComponent<ResourceManager> ();
		}
	}
	
	void Update () {
		
	}

	//Moved to resource manager
	/*
	/// <summary>
	/// Can the builder afford to make this unit or structure. Returns true if can afford, false otherwise
	/// </summary>
	/// <returns><c>true</c>, if can afford, <c>false</c> otherwise.</returns>
	public bool canAfford(){
		for (int i=0; i<resourceCosts.Length; i++) {
			//if resource is not found, or somehow went negative, return false
			if( manager.getResourceWithTag(resourceNames[i])<0 ){
				return false;
			}
			//if manager has less res than the unit costs, we can't afford to build
			if( manager.getResourceWithTag(resourceNames[i]) < resourceCosts[i] ){
				return false;
			}
		}
		if ( energyCost > 0 && GetComponent<PowerControl> () != null ) {
			if( !(GetComponent<PowerControl>().isEmpty(energyCost)) ){
				return false;
			}
		}
		if (orbitalEnergyCost > 0) {
			if( !(manager.hasEnergyinOrbit(orbitalEnergyCost)) ){
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Deducts required resources from the resource manager, and/or deducts local power from owning unit
	/// Note: Should verify canAfford first, otherwise this can force resources into negatives
	/// </summary>
	/// <returns><c>true</c>, if unit was bought, <c>false</c> otherwise.</returns>
	public bool buyUnit(){
		return true;
	}
*/

}
