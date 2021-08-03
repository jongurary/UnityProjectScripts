using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectorGUI : MonoBehaviour {
	//TODO ABANDONED FOR NOW, TOO DAMN HARD
	public Texture marqueeGraphics;
	private Vector2 marqueeOrigin;
	private Vector2 marqueeSize;
	public Rect marqueeRect;
	public GameObject Selector;

//	private Collider SelectorBox;
	
	private void OnGUI()
	{
		//Draw selector rectangle
		marqueeRect = new Rect(marqueeOrigin.x, marqueeOrigin.y, marqueeSize.x, marqueeSize.y);
		GUI.color = new Color(0, 0, 0, .3f);
		GUI.DrawTexture(marqueeRect, marqueeGraphics);
	}
	
	void Start () {
//		SelectorBox = Selector.GetComponent<Collider> ();
	}

	void Update () {

		//mouse released, remove gui selector
		if (Input.GetMouseButtonUp (0)) {
			marqueeRect.width = 0;
			marqueeRect.height = 0;
			marqueeSize = Vector2.zero;
		//restore selection collider to normal size
			Selector.transform.localScale = new Vector3(0f, 400F, 0f);
			
		} else if (Input.GetMouseButtonDown (0)) {
			//on left click, begin drawing the selection square
			float _invertedY = Screen.height - Input.mousePosition.y;
			marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);
		}

		if (Input.GetMouseButton (0)) {
			//on left click held, extend size of selection square
			float _invertedY = Screen.height - Input.mousePosition.y;
			//extend selector collider to fit gui square
			marqueeSize = new Vector2 (Input.mousePosition.x - marqueeOrigin.x, (marqueeOrigin.y - _invertedY) * -1);
			Selector.transform.localScale = new Vector3((Input.mousePosition.x - marqueeOrigin.x)/10, 
			                                            400F, (marqueeOrigin.y - _invertedY)/10);

			Vector3 Viewpoint = Camera.main.ViewportToWorldPoint(
				new Vector3(0f, 0f, 0f));
			Selector.transform.position = Viewpoint+ new Vector3(0f, 0f, 40f);
		//	Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
		//	Vector3 point= ray.origin + (ray.direction * 1f);
		//	Debug.Log ("point" + point);
			float xinput= 2* (-.5f+Input.mousePosition.x/Screen.width);
			float yinput= 2* (-.5f+Input.mousePosition.y/Screen.width);
			Debug.Log("x: " + xinput + " y: " + yinput + " SWidth " + Camera.main.pixelWidth + " SHeight " + Camera.main.pixelHeight);

			Selector.transform.localPosition = Selector.transform.localPosition + new Vector3(
				xinput*70, 0f, yinput*80+40f);
			Selector.transform.localPosition=
				new Vector3(Selector.transform.localPosition.x, 20f, Selector.transform.localPosition.z);

		//	Selector.transform.position = point+ new Vector3(0f, 20f, 40f);

		}
	
	}
}
