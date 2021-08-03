using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages all types of resources for units that process resources into other resources
public class ResourceControl : MonoBehaviour {
	
	public string[] inputType = new string[5];
	public string[] outputType = new string[5];

	//output storage
	public int[] maxOutputResource = new int[5];
	public int[] currentOutputResource = new int[5];

	//input storage
	public int[] maxInputResource = new int[5];
	public int[] currentInputResource = new int[5];

	void Start () {
		
	}

	void Update () {
		
	}

	public string getInputType(int index){ return inputType[index]; }
	public void setInputType(string type,int index){ inputType[index] = type; }
	public string getOutputType(int index){ return outputType[index]; }
	public void setOutputType(string type,int index){ outputType[index] = type; }

	public int getCurrentOutputResource(int index){ return currentOutputResource[index]; }
	public int getMaxOutputResource(int index){ return maxOutputResource[index]; }

	public int getCurrentInputResource(int index){ return currentInputResource[index]; }
	public int getMaxInputResource(int index){ return maxInputResource[index]; }

	/// <summary>
	/// Test if unit will be empty if an amount of output resource is drained, does not apply drain. 
	/// Returns <c>true</c>, if empty (or negative) after resource drain, <c>false</c> otherwise.
	/// </summary>
	/// <param name="toDrain">Amount of resource to drain.</param>
	/// <param name="toDrain">Index of resource.</param>
	public bool isEmpty(int toDrain, int index){
		if (currentOutputResource[index] >= toDrain) {
			return false;
		} else {
			return true;
		}
	}

	/// <summary>
	/// Test if unit will be empty if an amount of input resource is drained.
	/// Returns <c>true</c>, if empty after resource, <c>false</c> otherwise.
	/// </summary>
	/// <param name="toDrain">Amount of resource to drain.</param>
	public bool isEmptyInput(int toDrain, int index){
		if (currentInputResource[index] >= toDrain) {
			return false;
		} else {
			return true;
		}
	}
	
	/// <summary>
	/// Drain the mine's output resource by a certain amount, true if successfull, but the drain will always occur even if it goes negative
	/// </summary>
	/// <param name="toDrain">Amount of resource to drain.</param>
	public bool drain(int toDrain, int index){
		currentOutputResource[index] = currentOutputResource[index] - toDrain; //a negative charge indicates an error, but drains must always be applied
		if (currentOutputResource[index] >= toDrain) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Drain the mine's input resource by a certain amount, true if successfull, but the drain will always occur even if it goes negative
	/// </summary>
	/// <param name="toDrain">Amount of resource to drain.</param>
	public bool drainInput(int toDrain, int index){
		currentInputResource[index] = currentInputResource[index] - toDrain; //a negative charge indicates an error, but drains must always be applied
		if (currentInputResource[index] >= toDrain) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Add output resource at index to the unit, return false if full
	/// </summary>
	/// <param name="toFill">Amount to fill, index</param>
	public bool fill(int toFill, int index){
		if (currentOutputResource[index] + toFill <= maxOutputResource[index]) {
			currentOutputResource[index] = currentOutputResource[index] + toFill;
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Add input resource at index to the unit, return false if full
	/// </summary>
	/// <param name="toFill">Amount to fill, index</param>
	public bool fillInput(int toFill, int index){
		if (currentInputResource[index] + toFill <= maxInputResource[index]) {
			currentInputResource[index] = currentInputResource[index] + toFill;
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Test if output at index would be full if given fill amount, true if not full
	/// </summary>
	/// <param name="toFill">Amount to fill, index</param>
	public bool isNotFull(int toFill, int index){
		if (currentOutputResource[index] + toFill <= maxOutputResource[index]) {
			return true;
		} else { return false; }
	}
	
	/// <summary>
	/// Test if input at index would be full if given fill amount, true if not full
	/// </summary>
	/// <param name="toFill">Amount to fill, index</param>
	public bool isNotFullInput(int toFill, int index){
		if (currentInputResource[index] + toFill <= maxInputResource[index]) {
			return true;
		} else { return false; }
	}

	/// <summary>
	/// If the type is available in the list of inputs, return its index
	/// </summary>
	/// <returns>Index of the inputType array containing type.</returns>
	/// <param name="type">Type.</param>
	public int getIndexofInputType(string type){
		for (int i=0; i<inputType.Length; i++) {
			if(inputType[i]==type){
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Attempts to add another input type to the list, returns the index of the type is successfull, otherwise -1
	/// </summary>
	/// <returns>index, if next empty input type was set, -1 otherwise.</returns>
	public int setNextEmptyInputType(string type){
		for (int i=0; i<4; i++) {
			if(inputType[i]==null || inputType[i]==""){
				inputType[i] = type;
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// If the type is available in the list of outputs, return its index
	/// </summary>
	/// <returns>Index of the inputType array containing type.</returns>
	/// <param name="type">Type.</param>
	public int getIndexofOutputType(string type){
		for (int i=0; i<outputType.Length; i++) {
			if(outputType[i]==type){
				return i;
			}
		}
		return -1;
	}
	
	/// <summary>
	/// Attempts to add another output type to the list, returns the index of the type is successfull, otherwise -1
	/// </summary>
	/// <returns>index, if next empty input type was set, -1 otherwise.</returns>
	public int setNextEmptyOutputType(string type){
		for (int i=0; i<outputType.Length; i++) {
			if(outputType[i]==null || outputType[i]==""){
				outputType[i] = type;
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the number of active inputs, -1 for error
	/// </summary>
	/// <returns>The input count, -1 for error.</returns>
	public int getInputCount(){
		for (int i=0; i<inputType.Length; i++) {
			if(inputType[i]==null || inputType[i]==""){
				return i;
			}
		}
		return inputType.Length;
	}

	/// <summary>
	/// Returns the number of active outputs, -1 for error
	/// </summary>
	/// <returns>The output count, -1 for error.</returns>
	public int getOutputCount(){
		for (int i=0; i<outputType.Length; i++) {
			if(outputType[i]==null || outputType[i]==""){
				return i;
			}
		}
		return inputType.Length;
	}


}
