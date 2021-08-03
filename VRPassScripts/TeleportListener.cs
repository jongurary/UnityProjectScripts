using UnityEngine;
using System.Collections;

public class TeleportListener : MonoBehaviour {
	public GameObject teleportEntrance;
	public GameObject player;

	void Start () {
	
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha9) || Input.GetButtonDown ("Teleport")) {
			player.transform.position = teleportEntrance.transform.position;
			player.transform.rotation = teleportEntrance.transform.rotation;
		}
	}
}
