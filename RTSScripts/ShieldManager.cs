using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldManager : MonoBehaviour {

	public int shieldHealth;
	public int maxShieldHealth;
	public int shieldRegen;
	public int shieldRegenInterval;
	public int powerPerTick;
	public float rebootTime;
	[Tooltip("Change for shield to absorb projectiles from 0 to 100.")]
	public int blockChance;

	public GameObject shieldImpact;

	private float shieldSize; //size of the shield in squared magnitude units
	private float shieldThickness; //projectiles within this distance of the shield's size are considered a shield hit
	private bool canRegen;
	private bool shieldOn; //shield collision enabled or disabled

	private int overclock; //multiplies by the energy consumed and the shield power regenerated

	void OnTriggerEnter(Collider other){
		if (shieldOn) {
			if (other.tag == "Missile") {
				float dist = (other.transform.position - GetComponent<Collider> ().bounds.center).sqrMagnitude; //can use transform.position instead
				if (Mathf.Abs (dist - shieldSize) < shieldThickness) {
					//Acceptable collision on the outside of the sphere
					//	Debug.Log ("detected: "+ other.transform.root.gameObject + " at " + dist);
					processProjectile (other.gameObject, other.transform.position);
				} else {
					//Discarded collision that comes from within the sphere
					//	Debug.Log ("ignored: "+ other.transform.root.gameObject + " at " + dist);
				}

			}
		}
	}

	void processProjectile(GameObject proj, Vector3 impactArea){
		int rand = Random.Range (0, 100);
		if (rand > blockChance) { //exits if random is greater than blockchance
			return;
		}

		string identifier = "";
		int damage;
		identifier = proj.GetComponent<ProjectileIdentifier>().identifier;
		switch(identifier){
		case "cannonball":
			damage =100;
			break;
		case "weakcannonball":
			damage =25;
			break;
		case "flak":
			damage =5;
			break;
		case "bombermissile":
			damage =100;
			break;
		case "plasmaball":
			damage =100;
			break;
		case "orbitalrocket":
			damage =50;
			break;
		case "smallmissile":
			damage =150;
			break;
		case "nuke":
			damage =500;
			break;
		case "laser": //not in use
			damage =10;
			break;
		case "machinegun":
			damage =3;
			break;
		case "airmissile":
			damage =100;
			break;
		default:
			Debug.Log ("couldn't find object");
			damage =10;
			break;
		}
		GameObject impact = Instantiate (shieldImpact, impactArea, Quaternion.identity);
		Destroy (proj);
		doDamage (damage);
	}

	/// <summary>
	/// Play the damage animation
	/// </summary>
	void renderDamage(){

	}

	/// <summary>
	/// Do damage to the shield
	/// </summary>
	/// <param name="damage">Damage.</param>
	void doDamage(int damage){
		shieldHealth = shieldHealth - damage;
		if (shieldHealth < 0) {
			shieldHealth = 0;
			StartCoroutine(rebootShields());
		}
	}

	IEnumerator rebootShields(){
		canRegen = false;
		//TODO could animate this
		GetComponent<Collider> ().enabled = false;
		for (int i=0; i<5; i++) {
			GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds (.1f);
			GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds (.1f);
		}
		GetComponent<Renderer>().enabled = false;
		GetComponent<ParticleSystem> ().Emit (5);
		yield return new WaitForSeconds (rebootTime);
		GetComponent<Renderer>().enabled = true;
		canRegen = true;
		GetComponent<Collider> ().enabled = true;
	}

	IEnumerator regenShield(){
		while (true) {
			yield return new WaitForSeconds(shieldRegenInterval);
			if(canRegen){
				//if has power and shield not maxed
				if( !(GetComponentInParent<PowerControl>().isEmpty(powerPerTick * overclock)) 
				   && (shieldHealth + (shieldRegen * overclock)) < maxShieldHealth ){
					GetComponentInParent<PowerControl>().drain(powerPerTick * overclock);
					shieldHealth += (shieldRegen * overclock);
				}
			}
		}
	}

	void Start () {
		//calculate roughly the squared magnitude distance to the outside of the sphere along the x and z axis
		Collider col = GetComponent<Collider> ();
		Vector3 bounds = col.bounds.size;
		//use the radius on x and z axis, ignore y axis distance
		bounds = new Vector3(bounds.x/2, 0f, bounds.z/2);
		//Alternative method
		//Vector3 center = col.bounds.center;
		//shieldSize = ((center + bounds) - center).sqrMagnitude / 2;
		shieldSize = bounds.x * bounds.x; //square magnitude distance to outside of sphere

		//set the shield's thickness to a fraction of the shield's overall size
		//Note that thickness is checked against square magnitude, so consider squaring it
		shieldThickness = shieldSize / 4f; 
		//	Debug.Log ( shieldSize   + " " + shieldThickness);
		canRegen = true; //initialize shield regeneration
		shieldOn = true;
		overclock = 1;

		StartCoroutine (regenShield());
	}

	/// <summary>
	/// Enable or disable the shield collider
	/// </summary>
	public void toggleShield(){
		GetComponent<Renderer> ().enabled = !GetComponent<Renderer> ().enabled;
		shieldOn = !shieldOn;
	}

	/// <summary>
	/// Upgrade shield via increment to the shield overclock, which multiplies the shield energy and regen per second
	/// Also increases max possible shield
	/// Returns true if possible
	/// </summary>
	/// <param name="clock">Clock.</param>
	public bool boostOverclock(){
		if (overclock < 5) {
			overclock += 1;
			maxShieldHealth += 250;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Get overlock, which multiplies the shield energy and regen per second
	/// </summary>
	/// <param name="clock">Clock.</param>
	public int getOverclock(){
		return overclock;
	}

	/// <summary>
	/// Current amount of shield life
	/// </summary>
	/// <returns>The shield current health.</returns>
	public int getShieldCurrentHealth(){
		return shieldHealth;
	}

	/// <summary>
	/// Max shield
	/// </summary>
	/// <returns>The shield current health.</returns>
	public int getShieldMaxHealth(){
		return maxShieldHealth;
	}

	void Update () {
		
	}

	//Draws a sphere in the editor for visualization
	private void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere (transform.position, Mathf.Sqrt(shieldSize));
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, Mathf.Sqrt(shieldSize-shieldThickness));
	}
}
