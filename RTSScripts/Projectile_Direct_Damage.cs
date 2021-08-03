using UnityEngine;
using System.Collections;

public class Projectile_Direct_Damage : MonoBehaviour {

	public GameObject Kaboom; //explosion effect
	public int damage;
	public string attackType1;
	public string attackType2;
	public string attackType3;

	void OnCollisionEnter(Collision collision) {
		GameObject hit;
		//	print ("kaboom");
	//	Debug.Log (collision.gameObject);
		//TODO make this AOE
		Instantiate (Kaboom, transform.position, transform.rotation);
		hit = collision.gameObject;

		if (hit.tag == attackType1 || hit.tag == attackType2 || hit.tag == attackType3) {
			if(hit.GetComponent<UnitLife>() != null) {
				hit.GetComponent<UnitLife>().Damage(damage);
			}
		}

		//Move to a coroutine if this becomes too resource intensive
	//	StartCoroutine(ApplyDamage());
		Destroy (gameObject);
	}

}
