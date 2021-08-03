/*
 * Creates a common link to all weapon rangefinders on this object. Should be placed on the root and manually linked to rangefinderdisplay scripts
 * 
 * 07.22.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 07.22.2020
 */
using UnityEngine;

public class RangeFinderDisplayManager : MonoBehaviour{

    public RangeFinderDisplay[] displays;
    public bool isEnabled = false;

    void Start(){
        //for testing only
        //for (int i = 0; i < displays.Length; i++) {
        //    if (displays[i] != null) {
        //        displays[i].enableDisplay();
        //    }
        //}
    }

    void Update(){
        
    }

    public void enableDisplays() {
        isEnabled = true;
        for(int i=0; i<displays.Length; i++) {
            if (displays[i] != null) {
                displays[i].enableDisplay();
            }
        }
    }


    public void disableDisplays() {
        isEnabled = false;
        for (int i = 0; i < displays.Length; i++) {
            if (displays[i] != null) {
                displays[i].disableDisplay();
            }
        }
    }
}
