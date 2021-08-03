using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalDebrisManager : MonoBehaviour {

	public GameObject debris;
	/**
	 * How often debris rains down, in seconds. (Lower is more often).
	 */
	public float interval;
	public float xMin, xMax;
	public float zMin, zMax;
	public float yStart;
	
	void Start () {

	}

	void Update () {
		
	}

	/**
	 * Start raining debris down from the heavens
	 */
	public void Engage(){
		StartCoroutine (rainDebris());
	}

	/**
	 * Stop raining debris down
	 */
	public void Stop(){
		StopCoroutine (rainDebris());
	}

	IEnumerator rainDebris(){
		while (true) {
			yield return new WaitForSeconds (interval);
			Instantiate (debris, new Vector3(Random.Range(xMin, xMax), yStart, Random.Range(zMin, zMax)), Quaternion.identity);
		}
	}
}
