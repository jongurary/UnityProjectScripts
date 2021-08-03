using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissileControl : MonoBehaviour {

	public float speedCoefficient;  // how fast you accelerate
	public float maxSpeed;   // The maximum speed you can move
	public float turnspeed; //speed the missle faces target, should be high
	public int explosionDamage;
	public Vector3 targetposition;
	public GameObject Kaboom;
	public GameObject DamageRadius;

	private Rigidbody rb;
	private Vector3 direction;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate(){

		float speed;
		speed = Mathf.Sqrt (Mathf.Pow (rb.velocity.x, 2) + Mathf.Pow (rb.velocity.y, 2) + Mathf.Pow (rb.velocity.z, 2));
		direction = (targetposition - transform.position).normalized;  // Get the normalized direction to target

		if (speed < maxSpeed) {
		//	print (targetposition);
			//look towards direction of movement
			Quaternion look =Quaternion.LookRotation (-rb.velocity+new Vector3(0f,90f,0f));
			rb.MoveRotation(Quaternion.Slerp (rb.rotation, look, Time.deltaTime*turnspeed));
			
			//add acceleration in the direction of target
			//	print (speed);
			rb.AddForce (direction * speedCoefficient, ForceMode.Acceleration);

		}
	}

	void OnCollisionEnter(Collision collision) {
	//	print ("kaboom");
		Instantiate (Kaboom, transform.position+new Vector3(0f, 3f, 0f), Quaternion.identity);
		StartCoroutine(ApplyDamage());
		Destroy (gameObject);
	}

	IEnumerator ApplyDamage() {
		List<GameObject> toDamage;
	//	yield return new WaitForSeconds(waitTime);
		//TODO this can be done with physics.overlapsphere
		toDamage= DamageRadius.GetComponent<AoECaclulator>().inAoe;

		foreach (GameObject obj in toDamage) {
			//		print (obj);
			if(obj==null){ //do nothing if the object already died
			}else if(obj.tag=="Unit" || obj.tag=="Building"){
				obj.GetComponent<UnitLife>().Damage(100);
			//	print ("Damaged");
			}
		}
		yield return null;
	}
}
