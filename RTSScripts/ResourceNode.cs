using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour {

	//Types of resources:
	//Iron
	public string type;
	public int resource; //quantity of resource
	private int startingResource; //how much the node starts with
	

	void Start () {
		startingResource = resource;
	}

	void Update () {
		
	}

	//Add resource
	public void toAdd(int toGive){
		resource = resource + toGive;
	}
	
	//Drain resource
	public bool consume(int toDrain){
		resource = resource - toDrain; //a negative charge indicates an error, but drains must always be applied
		if (resource >= toDrain) {
			return true;
		} else {
			return false;
		}
	}
	
	//test if resource would be empty if drained
	public bool isEmpty(int toGive){
		if (resource > toGive) {
			return false;
		} else {
			return true;
		}
	}

	public int getResource(){
		return resource;
	}

	public int getStartingResource(){
		return startingResource;
	}

	/// <summary>
	/// Returns type of resource as string. Possible resources: "Iron" "Geothermal"
	/// </summary>
	/// <returns>The resource type.</returns>
	public string getType(){
		return type;
	}

}
