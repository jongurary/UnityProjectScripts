/*
 * Draws the outline of a range finder so the player can see where the weapon is able to fire
 * 
 * 07.22.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 07.22.2020
 */
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class RangeFinderDisplay : MonoBehaviour{

    [Tooltip("Rangefinder collider that this script attempts to display")]
    public BoxCollider rangeFinder;
    public bool isEnabled;

    [Tooltip("Borders outlines will be drawn using this prefab")]
    public GameObject borderPrefab;
    [Range(.5f, 3f)]
    [Tooltip("Thickness of the lines comprising the wireframe")]
    public float frameThickness = .5f;

    /// <summary>
    /// Holds the lines segments comprising the wireframe border
    /// </summary>
    private GameObject[] borders = new GameObject[8];

//    LineRenderer

    void Start(){
        if (rangeFinder == null) {
            rangeFinder = GetComponent<BoxCollider>();
        }

        for(int i=0; i<borders.Length; i++) {
            borders[i] = Instantiate(borderPrefab, transform.position, transform.rotation);
            borders[i].transform.parent = transform;
            if (i < borders.Length / 2) {
                borders[i].transform.localScale = new Vector3(frameThickness, frameThickness, rangeFinder.size.z);
            } else if(i<borders.Length/2 + 2){
                borders[i].transform.localScale = new Vector3(rangeFinder.size.x, frameThickness, frameThickness);
            } else {
                borders[i].transform.localScale = new Vector3(frameThickness, rangeFinder.size.y, frameThickness);
            }
        }

        borders[0].transform.localPosition = new Vector3(0 - rangeFinder.size.x / 2, 
            0 - rangeFinder.size.y / 2, 
            0);
        borders[1].transform.localPosition = new Vector3(rangeFinder.size.x - rangeFinder.size.x / 2, 
            0 - rangeFinder.size.y / 2,
            0);
        borders[2].transform.localPosition = new Vector3(-rangeFinder.size.x / 2, 
            rangeFinder.size.y - rangeFinder.size.y / 2, 
            0);
        borders[3].transform.localPosition = new Vector3(rangeFinder.size.x - rangeFinder.size.x / 2, 
            rangeFinder.size.y - rangeFinder.size.y / 2, 
            0);

        borders[4].transform.localPosition = new Vector3(0, 
            0 - rangeFinder.size.y / 2, 
            rangeFinder.size.z / 2);
        borders[5].transform.localPosition = new Vector3(0, 
            0 + rangeFinder.size.y / 2, 
            rangeFinder.size.z / 2);
        borders[6].transform.localPosition = new Vector3(0 - rangeFinder.size.x / 2, 
            0, 
            rangeFinder.size.z / 2);
        borders[7].transform.localPosition = new Vector3(rangeFinder.size.x / 2, 
            0, 
            rangeFinder.size.z / 2);

        disableDisplay();
    }

    public void disableDisplay() {
        for (int i = 0; i < borders.Length; i++) {
            if (borders[i] != null) {
                borders[i].SetActive(false);
            }
        }
    }

    public void enableDisplay() {
        for (int i = 0; i < borders.Length; i++) {
            if (borders[i] != null) {
                borders[i].SetActive(true);
            }
        }
    }
    void Update(){
        
    }
}
