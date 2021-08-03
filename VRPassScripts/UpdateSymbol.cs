using UnityEngine;
using System.Collections;

public class UpdateSymbol : MonoBehaviour {

	public GameObject Quad1;
	public GameObject Quad2;

	public Material notSelected;
	public Material selected;

	private Renderer rend1;
	private Renderer rend2;

	// Use this for initialization
	void Start () {
		rend1 = Quad1.GetComponent<Renderer> ();
		rend2 = Quad2.GetComponent<Renderer> ();
	//	StartCoroutine ("SwapMats");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setSymbol(Material Mat){
		rend1.sharedMaterial = Mat;
		rend2.sharedMaterial = Mat;
	}

	public void Select(){
		rend1.sharedMaterial = selected;
		rend2.sharedMaterial = selected;
	}

	public void DeSelect(){
		rend1.sharedMaterial = notSelected;
		rend2.sharedMaterial = notSelected;
	}

	//randomly swaps between the materials, for debug only.
	IEnumerator SwapMats(){
		int count = 0;
		while (true) {
			if(count%2==0){
				rend1.material=notSelected;
				rend2.material=notSelected;
			}else{
				rend1.material=selected;
				rend2.material=selected;
			}
			count++;
			yield return new WaitForSeconds(5f);
		}
	}
}
