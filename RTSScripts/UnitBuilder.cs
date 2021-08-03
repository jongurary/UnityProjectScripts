using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the assembly of units built by this "factory"
/// </summary>
public class UnitBuilder : MonoBehaviour {

	public GameObject[] Spawn; //Units that this unit can spawn
	[Tooltip("Build time intervals per spawn unit, lower is faster")]
	public float[] buildTimeIntervals; //in order of spawns, the build time interval for each, lower is better
	
	public GameObject Slave1; //A unit bound to this building that does not permenantly leave, e.g. a bomber to an airfield
	private GameObject buildingObj; //currently under construction object

	public GameObject animParent; //Gameobject holding the animation controller
	private Animator anim; //the animation controller itself

	private int objectTag; //determines which set of commands to run
	private bool isBuilding; //are we currently building a unit?
	private Controls control;

	//list of object tags
	private const int airbase = 100;

	void Start () {
		//all unit builders should have a controls module, if this returns null there's a serious issue
		control = GetComponent<Controls> ();
		setTag (control.objectTag);
		isBuilding = false;

		anim = animParent.GetComponent<Animator> ();
		//reference: 100 = airbase
	}

	void Update () {
		
	}

	void OnDestroy()
	{
		if (Slave1 != null) {
			Destroy(Slave1);
		}

		if (buildingObj != null) {
			Destroy(buildingObj);
		}
	}

	/// <summary>
	/// Builds a unit that is slaved to the owner
	/// </summary>
	/// <param name="spawnlocation">Location of spawn</param>
	/// <param name="offset">Offset location away from the based position</param>
	/// <param name="spin">Quaternion of spawn rotation</param>
	/// <param name="status">status flag</param>
	public void BuildSlaveUnit(Vector3 spawnlocation, Vector3 offset, Quaternion spin, int status){
		StartCoroutine(BuildSlaveUnitRoutine(spawnlocation, offset, spin, status));
		return;
	}

	/// <summary>
	/// Builds a unit that is independent after construction
	/// </summary>
	/// <param name="spawnlocation">Location of spawn</param>
	/// <param name="offset">Offset location away from the based position</param>
	/// <param name="spin">Quaternion of spawn rotation</param>
	/// <param name="status">status flag</param>
	public void BuildUnit(Vector3 spawnlocation, Vector3 offset, Quaternion spin, int status){
		StartCoroutine(BuildUnitRoutine(spawnlocation, offset, spin, status));
		return;
	}

	/// <summary>
	/// Coroutine that handles build animations and final initalization for construction
	/// </summary>
	IEnumerator BuildUnitRoutine(Vector3 spawnlocation, Vector3 offset, Quaternion spin, int status){
	//	GameObject obj;
		if (isBuilding == false) {
			//Unit build animation begins here
			isBuilding=true;
			
			//initiate the animator
			animParent.SetActive(true); //should be disabled initially for orbital drop
			anim.Play ("Raise"); //raise the animation pylons
			
			//wait a bit before the building starts for the construction pylons to rise into place
			yield return new WaitForSeconds(2f); //the rise animation takes 3 seconds
			
			//spawn the buildable object, it is not yet finished building, but already exists
			//TODO check if disabling colliders causes any issues
			
			//Note, will error out of the spawn[status] isn't properly set
			buildingObj = (GameObject) Instantiate (Spawn[status], spawnlocation + offset, spin);
			disableAttackScripts(buildingObj);
			
			string origTag = buildingObj.tag;
			buildingObj.tag = "UnderConstruction"; //change tag while building to avoid attack scripts
			foreach(Transform t in buildingObj.transform)
			{
				t.gameObject.tag = "UnderConstruction";
			}
			
			
			foreach ( Collider col in buildingObj.GetComponents<Collider>() ) { //disable colliders during construction
				col.enabled = false;
			}
			
			//Get the renderers from the buildable object
			Component[] renderers = buildingObj.GetComponentsInChildren<Renderer>();
			List<Material> materials = new List<Material>(); //materials found in the renderer
			Color color;
			
			//NOTE TODO there should an exception list for materials that should remain transparent
			//NOTE TODO alternatively, instead of getting materials programatically, set them in editor as public var
			//set all renderers to invisible (0 alpha)
			//Note: all renderers must be transparent initially.
			foreach (Renderer curRenderer in renderers)
			{
				
				foreach (Material material in curRenderer.materials)
				{
					materials.Add(material);
					if(material.HasProperty("_Color")){
						
						material.SetFloat("_Mode", 3);
						material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
						material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
						material.SetInt("_ZWrite", 0);
						material.DisableKeyword("_ALPHATEST_ON");
						material.DisableKeyword("_ALPHABLEND_ON");
						material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
						material.renderQueue = 3000;
						
						color = material.color;
						color.a = 0;                   
						material.color = color;
					}
				}
			}
			
			//build the unit for increasing alpha until it is fully visible
			for(int i=0; i<100; i++){
				yield return new WaitForSeconds(buildTimeIntervals[status]);
				foreach (Material material in materials)
				{
					if(material.HasProperty("_Color")){
						color = material.color;
						color.a += 0.01f;
						if (color.a > 1)
						{
							color.a = 1;                   
						}
						material.color = color;
					}
				}
			} //end for loop
			
			//restore the renderer to opaque mode
			foreach (Material material in materials)
			{
				material.SetFloat("_Mode", 0);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				
			}
			
			//begin the animation trigger that moves the builder module back down
			anim.SetTrigger("isFinished");
			//Note: there is no need to disable the animator again, it should be insvisible at idle.
			
			//FINISHED BUILDING UNIT
			isBuilding=false;
			
			buildingObj.tag = origTag; //change tag while building to avoid attack scripts
			foreach(Transform t in buildingObj.transform)
			{
				t.gameObject.tag = origTag;
			}
			enableAttackScripts(buildingObj);
			
			foreach ( Collider col in buildingObj.GetComponents<Collider>() ) { //restore colliders
				col.enabled = true;
			}
			
			setUniqueControls(buildingObj);
		}
		yield return null;
	}

	/// <summary>
	/// Coroutine that handles build animations and final initalization for construction
	/// </summary>
	IEnumerator BuildSlaveUnitRoutine (Vector3 spawnlocation, Vector3 offset, Quaternion spin, int status){
	//	GameObject obj;
		if (Slave1==null && isBuilding == false) {
			//Unit build animation begins here
			isBuilding=true;

			//initiate the animator
			animParent.SetActive(true); //should be disabled initially for orbital drop
			anim.Play ("Raise"); //raise the animation pylons
			
			//wait a bit before the building starts for the construction pylons to rise into place
			yield return new WaitForSeconds(2f); //the rise animation takes 3 seconds

			//spawn the buildable object, it is not yet finished building, but already exists
			//TODO check if disabling colliders causes any issues

			//Note, will error out if the spawn[status] isn't properly set
			buildingObj = (GameObject) Instantiate (Spawn[status], spawnlocation + offset, spin);

			string origTag = buildingObj.tag;
			buildingObj.tag = "UnderConstruction"; //change tag while building to avoid attack scripts
			disableAttackScripts(buildingObj);
			foreach(Transform t in buildingObj.transform)
			{
				t.gameObject.tag = "UnderConstruction";
			}


			foreach ( Collider col in buildingObj.GetComponents<Collider>() ) { //disable colliders during construction
				col.enabled = false;
			}

			//Get the renderers from the buildable object
			Component[] renderers = buildingObj.GetComponentsInChildren<Renderer>();
			List<Material> materials = new List<Material>(); //materials found in the renderer
			Color color;

			//NOTE TODO there should an exception list for materials that should remain transparent
			//NOTE TODO alternatively, instead of getting materials programatically, set them in editor as public var
			//set all renderers to invisible (0 alpha)
			//Note: all renderers must be transparent initially.
			foreach (Renderer curRenderer in renderers)
			{

				foreach (Material material in curRenderer.materials)
				{
					materials.Add(material);
					if(material.HasProperty("_Color")){

						material.SetFloat("_Mode", 3);
						material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
						material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
						material.SetInt("_ZWrite", 0);
						material.DisableKeyword("_ALPHATEST_ON");
						material.DisableKeyword("_ALPHABLEND_ON");
						material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
						material.renderQueue = 3000;

						color = material.color;
						color.a = 0;                   
						material.color = color;
					}
				}
			}

			//build the unit for increasing alpha until it is fully visible
			for(int i=0; i<100; i++){
				yield return new WaitForSeconds(buildTimeIntervals[status]); //TODO this should be what scales with build rate
				foreach (Material material in materials)
				{
							if(material.HasProperty("_Color")){
								color = material.color;
								color.a += 0.01f;
								if (color.a > 1)
								{
									color.a = 1;                   
								}
								material.color = color;
							}
				}
			} //end for loop

			//restore the renderer to opaque mode
			foreach (Material material in materials)
			{
					material.SetFloat("_Mode", 0);
					material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					material.SetInt("_ZWrite", 1);
					material.DisableKeyword("_ALPHATEST_ON");
					material.DisableKeyword("_ALPHABLEND_ON");
					material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					material.renderQueue = -1;
			
			}

			//begin the animation trigger that moves the builder module back down
			anim.SetTrigger("isFinished");
			//Note: there is no need to disable the animator again, it should be insvisible at idle.

			//FINISHED BUILDING UNIT
			isBuilding=false;

			buildingObj.tag = origTag; //restore tags
			foreach(Transform t in buildingObj.transform)
			{
				t.gameObject.tag = origTag;
			}
			enableAttackScripts(buildingObj);

			foreach ( Collider col in buildingObj.GetComponents<Collider>() ) { //restore colliders
				col.enabled = true;
			}

			setUniqueControls(buildingObj);
			Slave1=buildingObj;
		}
		yield return null;
	}

	/// <summary>
	/// Handles the setup of unique control units
	/// </summary>
	public void setUniqueControls(GameObject obj){

		if (obj.GetComponent<BomberControls> () != null) {
			obj.GetComponent<BomberControls> ().OwnerBase = gameObject;
		}

		if (obj.GetComponent<FighterControl> () != null) { 
			obj.GetComponent<FighterControl> ().OwnerBase = gameObject;
			obj.GetComponent<FighterControl> ().InitialRaise();
		}

	}

	/// <summary>
	/// Temporarily turns off attack control scripts
	/// </summary>
	public void disableAttackScripts(GameObject obj){
		//TODO this might be woefully inefficient
		if (obj.GetComponent<UnitAttack> () != null) {
			obj.GetComponent<UnitAttack>().enabled=false;
		}
		foreach (UnitAttack chil in obj.GetComponentsInChildren<UnitAttack>()){
			chil.enabled = false;
		}

		if(obj.GetComponent<UnitAttackArea>() != null){
			obj.GetComponent<UnitAttackArea>().enabled=false;
		}
		foreach (UnitAttackArea chil in obj.GetComponentsInChildren<UnitAttackArea>()){
			chil.enabled = false;
		}

		if(obj.GetComponent<UnitAttackLaser>() != null){
			obj.GetComponent<UnitAttackLaser>().enabled=false;
		}
		foreach (UnitAttackLaser chil in obj.GetComponentsInChildren<UnitAttackLaser>()){
			chil.enabled = false;
		}
	}

	/// <summary>
	/// Turns attack scripts back on
	/// </summary>
	public void enableAttackScripts(GameObject obj){

		if (obj.GetComponent<UnitAttack> () != null) {
			obj.GetComponent<UnitAttack>().enabled=true;
		}
		foreach (UnitAttack chil in obj.GetComponentsInChildren<UnitAttack>()){
			chil.enabled = true;
		}
		
		if(obj.GetComponent<UnitAttackArea>() != null){
			obj.GetComponent<UnitAttackArea>().enabled=true;
		}
		foreach (UnitAttackArea chil in obj.GetComponentsInChildren<UnitAttackArea>()){
			chil.enabled = true;
		}
		
		if(obj.GetComponent<UnitAttackLaser>() != null){
			obj.GetComponent<UnitAttackLaser>().enabled=true;
		}
		foreach (UnitAttackLaser chil in obj.GetComponentsInChildren<UnitAttackLaser>()){
			chil.enabled = true;
		}
	}
	
	public void setTag(int tag){
		objectTag = tag;
	}

	/// <summary>
	/// Is a unit under construction
	/// </summary>
	/// <returns><c>true</c>, if is unit is being constructed, <c>false</c> otherwise.</returns>
	public bool getIsBuilding(){ return isBuilding; }

}
