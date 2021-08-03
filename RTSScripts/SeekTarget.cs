using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekTarget : MonoBehaviour {

	//TODO make a physics-based version using rigidbodies!!

	public GameObject target;
	public Transform spawnArea; //where the object is rendered, since the navmesh is on the ground
	public GameObject[] spawnAtTarget;

	void Start () {
		if (target != null) {
			GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (target.transform.position);
		}
	}


	void Update () {
		if (target != null) {
			GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (target.transform.position);
		}

		//TODO only distance along x and z axis should matter for air collision purposes
		//All air units float at y=10, TODO, adjust y to air unit current y?
		if (target != null) {
			//Spawns all death-related gameobjects when the target is reached, then kills self
			if (Vector3.Distance (target.transform.position, spawnArea.position) < 3f) {
				for (int i=0; i<spawnAtTarget.Length; i++) {
					GameObject spawn = Instantiate (spawnAtTarget [i], spawnArea.position, spawnArea.rotation);
				}
				Destroy (gameObject);
			}
		}
	}

	public void setTarget(GameObject obj){ 
		target = obj; 
		gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (target.transform.position);
	}

}
