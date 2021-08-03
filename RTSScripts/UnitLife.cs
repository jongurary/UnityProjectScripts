using UnityEngine;
using System.Collections;

public class UnitLife : MonoBehaviour {
	public int health;
	public int regen;
	public int maxHealth;
	public float regenTime;
	public int owner;

	private const int ENEMY_OWNER = 2;

	void Start () {
		StartCoroutine(Regen(regenTime));
	}
	
	// Kill at zero hp
	void Update () {
		if (health <= 0) {
			//TODO Scheduled for removal.
			//			if(GetComponent<Controls>()!=null){
			//				GetComponent<Controls>().Remove();
			//			}
			if (owner == ENEMY_OWNER) {
				AISpawner spawner = GameObject.FindGameObjectWithTag("AISpawner").GetComponent<AISpawner>();
				if (spawner != null) {
					spawner.killCount++;
				}
			}
			Destroy(gameObject);
		}
	}

	//damage
	public void Damage(int damage){
		health = health - damage;
	}

	//healing
	public void Heal(int heal){
		if(health+heal<maxHealth)
		{
			health=health+heal;
		}else if(health<maxHealth){
			health=maxHealth;
		}
	}
	
	//passive hp regen
	IEnumerator Regen(float waitTime) {
		while (true) {
			yield return new WaitForSeconds (waitTime);
			if(health+regen<maxHealth)
			{
				health=health+regen;
			}else if(health<maxHealth){
				health=maxHealth;
			}
		}
	}

	public int getHealth(){ return health; }
	public int getMaxHealth(){ return maxHealth; }
	public int getOwner(){ return owner; }

}
