using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoControl : MonoBehaviour {

	public int currentAmmo; //amount of ammo currently stored
	public int maxAmmo;
	public string type; //type of ammo used by this unit
	
	void Start () {
		
	}

	void Update () {
		
	}



/// <summary>
/// Pump ammo into the system, returns true if unit is not full, but fills the ammo regardless
/// </summary>
	public bool charge(int toCharge){
		if (currentAmmo <= maxAmmo - toCharge) {
			currentAmmo = currentAmmo + toCharge;
			return true;
		} else {
			currentAmmo = currentAmmo + toCharge;
			return false;
		}
	}

	/// <summary>
	/// Drain ammo from system, returns true if unit can give ammo, but drains ammo regardless
	/// </summary>
	public bool drain(int toCharge){
		currentAmmo = currentAmmo - toCharge; //negative ammo indicates an error, but drains must always be applied
		if (currentAmmo >= toCharge) {
			return true;
		} else {
			return false;
		}
	}
	
	/// <summary>
	/// Test if adding ammo will render this unit over-full, without adding the ammo, returns true if would be full	
	/// </summary>
	/// <returns><c>true</c>, if full, <c>false</c> otherwise.</returns>
	/// <param name="toCharge">To charge.</param>
	public bool isFull(int toCharge){
		if (currentAmmo + toCharge > maxAmmo) {
			return true;
		} else {
			return false;
		}
	}
	
	/// <summary>
	/// Test if a drain will render this unit negative in ammo, without draining the ammo, returns true if would be empty
	/// </summary>
	/// <returns><c>true</c>, if empty, <c>false</c> otherwise.</returns>
	/// <param name="toCharge">To charge.</param>
	public bool isEmpty(int toCharge){
		if (currentAmmo > toCharge) {
			return false;
		} else {
			return true;
		}
	}

	public int getCurrentAmmo(){ return currentAmmo; }
	public int getMaxAmmo(){ return maxAmmo; }
	/// <summary>
	/// Returns the ammo type used by this unit, for example "BasicAmmo"	
	/// </summary>
	public string getType(){ return type; }

}
