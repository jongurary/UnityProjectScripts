using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour {

	public GameObject AirBase;
	public GameObject Cube;
	
	void Start () {
	
	}

	void Update () {
		if (this.isServer) {
			if (Input.GetKeyDown ("m")) {
				GameObject obj = (GameObject)GameObject.Instantiate (AirBase, new Vector3 (450, 0, 660), Quaternion.identity);
				NetworkServer.Spawn (obj);

			}

			if (Input.GetKeyDown ("n")) {
				GameObject obj = (GameObject)GameObject.Instantiate (Cube, new Vector3 (Random.Range (350, 500), 5, Random.Range (650, 750)), Quaternion.identity);
				NetworkServer.Spawn (obj);
			}
		}
	}
}
