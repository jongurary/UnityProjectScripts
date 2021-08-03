using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileIdentifier : MonoBehaviour {

	[Tooltip("Identifies this projectile for other classes, such as the shield manager, to compare against")]
	public string identifier;
	[Tooltip("Identifies the owner of this projectile. Not all projectiles have an owner. Default (no owner) = 0")]
	public int owner;

	void Start () {
		//do nothing
	}

	void Update(){
	}

}
