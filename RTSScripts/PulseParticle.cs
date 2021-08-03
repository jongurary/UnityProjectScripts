using UnityEngine;
using System.Collections;

public class PulseParticle : MonoBehaviour {

	public ParticleSystem Particle1;
	public ParticleSystem Particle2;
	public ParticleSystem Particle3;
	public ParticleSystem Particle4;
	
	void Start () {
	}

	void Update () {
	}

	public void pulse1(int pulse){
		Particle1.Emit (pulse);
	}
	public void pulse2(int pulse){
		Particle2.Emit (pulse);
	}
	public void pulse3(int pulse){
		Particle3.Emit (pulse);
	}
	public void pulse4(int pulse){
		Particle4.Emit (pulse);
	}

	public void engage1(bool value){
		Particle1.enableEmission.Equals(value);
	}
	public void engage2(bool value){
		Particle2.enableEmission.Equals(value);
	}
	public void engage3(bool value){
		Particle3.enableEmission.Equals(value);
	}
	public void engage4(bool value){
		Particle4.enableEmission.Equals(value);
	}

}
