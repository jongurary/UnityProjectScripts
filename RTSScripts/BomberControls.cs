using UnityEngine;
using System.Collections;

public class BomberControls : MonoBehaviour {

	readonly float FLIGHTALTITUDE= 15f;

	private Rigidbody rb;

	public float speedCoefficient;  // how fast you want to go, this will be multiplied by the distance
	public float maxSpeed;   // The maximum speed you want to move
	public float turnspeed; //only aesthetic, since you don't actually turn this unit.
	private Vector3 targetposition;    // Whatever your final position is
	private Vector3 targetpositionxz; //excludes the y coordinate
	private Vector3 target; //ground target for missiles, sets y to terrian height or 0
	private Vector3 direction;
	private Vector3 directionxz;
	private Vector3 lookDirection;
	private Vector3 startPosition;

	//State controls
	private bool missionOver=false;
	private bool landing = false;
	private bool hasMission=false;
	private bool takeoff=true;
	private bool VTOLON = false; //gets rid of annoying stutter between drift into landing and VTOL
	private bool armed = true; //have missiles ready to fire

	private int reverseTurboCharges = 1; //adds extra turn and reverse thrust when turning for smooth experience
	Vector3 turnbank=new Vector3(0f,0f,0f); //Force added to make a quick "turn"

	public ParticleSystem ThrusterLeft;
	public ParticleSystem ThrusterRight;
	public GameObject missile;
	public GameObject MissileRight;
	public GameObject MissileLeft;
	public GameObject OwnerBase;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		startPosition = transform.position;
		startPosition.y = FLIGHTALTITUDE;
//		Physics.IgnoreCollision (OwnerBase.GetComponents<Collider>()[0], GetComponents<Collider>()[0]);
//		Physics.IgnoreCollision (OwnerBase.GetComponents<Collider>()[1], GetComponents<Collider>()[0]);
//		Physics.IgnoreCollision (OwnerBase.GetComponents<Collider>()[0], GetComponents<Collider>()[1]);
//		Physics.IgnoreCollision (OwnerBase.GetComponents<Collider>()[1], GetComponents<Collider>()[1]);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate(){

		//TODO raycast forward for terrian and fly up if hit
		float distance;
		float speed;

		if(hasMission && takeoff){ //handles VTOL to altitude before acceleration
			targetpositionxz = transform.position;
			targetpositionxz.y=FLIGHTALTITUDE;
			directionxz = (targetpositionxz - transform.position).normalized;
			rb.velocity = (directionxz * speedCoefficient);

				if(Mathf.Abs (transform.position.y-FLIGHTALTITUDE)<1){
				takeoff=false;
				}
		}

		if (hasMission && !takeoff) { //handles movement to destination and back, plus landing

			speed = Mathf.Sqrt (Mathf.Pow (rb.velocity.x, 2) + Mathf.Pow (rb.velocity.y, 2) + Mathf.Pow (rb.velocity.z, 2));
			direction = (targetposition - transform.position).normalized;  // Get the normalized direction to target
			distance = Vector3.Distance (targetposition, transform.position); // Get distance to target
			targetpositionxz = targetposition;
			targetpositionxz.y=FLIGHTALTITUDE;
			directionxz = (targetpositionxz - transform.position).normalized;

			if (speed < maxSpeed && !landing) {
				//look towards direction of movement
				Quaternion look =Quaternion.LookRotation (-rb.velocity);
				rb.MoveRotation(Quaternion.Slerp (rb.rotation, look, Time.deltaTime*turnspeed));

				//add acceleration in the direction of target
			//	print (speed);
				rb.AddForce (directionxz * speedCoefficient, ForceMode.Acceleration);
				if (reverseTurboCharges > 1){ //TODO put in coroutine and repeat
					rb.AddForce (turnbank, ForceMode.Acceleration);
				//	print (turnbank);
					reverseTurboCharges = reverseTurboCharges-1;
				}
			}

			if (distance<speed*2 && armed){
				GameObject MissileLaunched; 
				//FIRE ZE SHIT
				float offsetrange= Random.Range (.5f, 4f);
				Vector3 offset = new Vector3(offsetrange, 0f, offsetrange);
				target=targetposition;
				target.y=0;
				MissileLaunched = Instantiate(missile, MissileLeft.transform.position-offset, Quaternion.identity);
				foreach(Collider col in GetComponents<Collider>()){ //ignore collisions with owner
					Physics.IgnoreCollision(col, MissileLaunched.GetComponent<Collider>());
				}
				MissileLaunched.GetComponent<MissileControl>().targetposition=target-offset;
				MissileLaunched = Instantiate(missile, MissileRight.transform.position+offset, Quaternion.identity);
				foreach(Collider col in GetComponents<Collider>()){ //ignore collisions with owner
					Physics.IgnoreCollision(col, MissileLaunched.GetComponent<Collider>());
				}
				MissileLaunched.GetComponent<MissileControl>().targetposition=target+offset;
				armed=false;
			}

			//	print (distance); //Check distances and determine if target is reached
			if (distance < speed*1.5 && !missionOver) {
			//TARGET IS REACHED, BOMB AND TURN AROUND

				//	print ("turning back"); //turn around and fly back to base;
				targetposition = startPosition;

				//calculate amount to bank randomly because evasive action may as well be random.
				float banking =Random.Range(25, 75);
				int plusorminus=Random.Range(1,5);
				switch (plusorminus){
				case 1:
					turnbank=new Vector3(banking, 0f, banking);
					break;
				case 2:
					turnbank=new Vector3(-banking, 0f, -banking);
					break;
				case 3:
					turnbank=new Vector3(banking, 0f, -banking);
					break;
				case 4:
					turnbank=new Vector3(-banking, 0f, banking);
					break;
				}

			//	Tell the jets to bank hard and cheese it
				reverseTurboCharges = 20;
				missionOver = true;

			} else if (distance < maxSpeed/2 && missionOver) { //engage landing mechanics
			//	print ("landing");

				landing = true;
				//add a small offset to land in the corner of the landing pad
				Vector3 landingpad = OwnerBase.transform.position+ new Vector3(0f, .6f, 0f);
				targetposition = landingpad;
				targetpositionxz = targetposition;
				targetpositionxz.y = FLIGHTALTITUDE;
				directionxz = (targetpositionxz - transform.position).normalized;

				//engage VTOL
				if (Mathf.Abs (targetposition.x - transform.position.x) < 3 && Mathf.Abs (targetposition.z - transform.position.z) < 3) {
					rb.velocity = (direction * speedCoefficient*.5f);
					rb.MoveRotation(Quaternion.Slerp (rb.rotation, Quaternion.identity, Time.deltaTime*turnspeed*8));
					VTOLON=true;
			//		print ("VTOL ON");
				} else if(!VTOLON) {
					rb.MoveRotation(Quaternion.Slerp (rb.rotation, Quaternion.identity, Time.deltaTime*turnspeed));
					rb.velocity = (directionxz * speedCoefficient);
			//		print ("floating into position");
				}
			} 
			if (distance <.5 && targetposition.y - transform.position.y < .1 && missionOver) {
			//	print ("landed");
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
				missionOver = false;
				landing = false;
				hasMission = false;
				takeoff=true;
				VTOLON=false;
				armed=true;
			}

			//Make the thruster spit out more flame
			ThrusterLeft.emissionRate = (speed+speed*reverseTurboCharges);
			ThrusterRight.emissionRate = (speed+speed*reverseTurboCharges);
	
		} else {
			ThrusterLeft.emissionRate = 5;
			ThrusterRight.emissionRate = 5;
		}
	}

	public void AssignMission(Vector3 target){
		if (!hasMission) {
			targetposition = target;
			hasMission = true;
		}
	}

}
