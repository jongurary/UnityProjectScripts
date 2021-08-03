using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnonDeath : MonoBehaviour {

	public GameObject[] toSpawn; //whatever needs to be spawned on death

	[Tooltip("Use randomly generated offsets for spawns. Objects will be spawned within random offset of position.")]
	public bool[] useRandomOffsets = new bool[3]; //if true, spawn objects with some random offset on x, y, and z axises respectively.
	public float randOffsetRange; //if useRandomOffset is true, this is the range on each axis to roll randoms

	private bool isQuitting; //for Unity editor, so objects are not instantiated when leaving the scene.

	void Start () {
		
	}

	void Update () {
	
	}

	void OnApplicationQuit()
	{
		isQuitting = true;
	}
	
	public void OnDestroy(){
		if (!isQuitting) {
			float xVector = 0f, yVector = 0f, zVector = 0f;
			if (useRandomOffsets [0] == true) {
				xVector = Random.Range (-randOffsetRange, randOffsetRange);
			} 
			if (useRandomOffsets [1] == true) {
				yVector = Random.Range (-randOffsetRange, randOffsetRange);
			} 
			if (useRandomOffsets [2] == true) {
				zVector = Random.Range (-randOffsetRange, randOffsetRange);
			}

			foreach (GameObject obj in toSpawn) {
				Instantiate (obj, transform.position + new Vector3 (xVector, yVector, zVector), Quaternion.identity);
			}
		}
	}
}
