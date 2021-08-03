using UnityEngine;
using System.Collections;

public class MasterAuthObjectControl : MonoBehaviour {

	public GameObject Cube1;
	public GameObject Cube2;
	public GameObject Cube3;
	public GameObject Cube4;
	public GameObject Cube5;

//	private Transform transform1;
//	private Transform transform2;
//	private Transform transform3;
//	private Transform transform4;
//	private Transform transform5;

	
	void Start () {
//		transform1 = Cube1.transform;
//		transform2 = Cube2.transform;
//		transform3 = Cube3.transform;
//		transform4 = Cube4.transform;
//		transform5 = Cube5.transform;
	}

	void Update () {
	
	}

	public void Scramble(){
		StartCoroutine (ScrambleCubes ());
	}

	void swap(GameObject obj1, GameObject obj2){
		Vector3 temp = obj1.transform.position;
		obj1.transform.position = obj2.transform.position;
		obj2.transform.position = temp;
		return;
	}

	IEnumerator ScrambleCubes (){
		int RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube1, Cube2);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube1, Cube3);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube1, Cube4);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube1, Cube5);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube2, Cube3);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube2, Cube4);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube2, Cube5);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube3, Cube4);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube3, Cube5);
		RandomSwap = Random.Range (1, 3);
		if(RandomSwap==1)
			swap (Cube4, Cube5);
		yield return null;
	}
}
