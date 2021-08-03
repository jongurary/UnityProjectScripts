using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns a gameobject on collision
/// </summary>
public class CreationSpawnThings : MonoBehaviour {

	public GameObject[] toSpawn; //whatever needs to be spawned on collision
	[Header("x-axis, y-axis, z-axis")]
	public bool[] useRandomOffsets = new bool[3]; //if true, spawn objects with some random offset on x, y, and z axises respectively.
	public float randOffsetRange; //if useRandomOffset is true, this is the range on each axis to roll randoms

	void Start() {
		
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
				Instantiate (obj, transform.position + new Vector3 (xVector, yVector, zVector), transform.rotation);
			}

			Destroy (gameObject);
	}

}
