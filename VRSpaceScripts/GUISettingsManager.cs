/*
 * Polls the selected unit and opens up the proper GUI context menu, communicates inputs back
 * to the selected unit
 * 
 * 
 * 07.10.20 initial commit v1
 * Polls reactor for priority system and allows user to change system priority
 * 
 * Version 1 by Jon Gurary 07.10.20
 */

using UnityEngine;
using UnityEngine.UI;

public class GUISettingsManager : MonoBehaviour{

    public bool isActive = false;

    public GameObject rootCanvasReactorSettings;

    //These are all images, but the link is to the gameobject because we only move the transforms around
    public GameObject shieldDisplay;
    public GameObject thrusterDisplay;
    public GameObject weaponDisplay;
    public GameObject emptyDisplay;
    public GameObject selectedOutlineDisplay;

    public MasterSelector masterSelector;

    private GameObject selected;
    private ReactorManager reactor;

    public enum Direction {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    void Start(){
        if (masterSelector == null) {
            masterSelector = GameObject.Find("Master Selector").GetComponent<MasterSelector>();
        }

        rootCanvasReactorSettings.SetActive(false);
    }

    void Update(){
        
    }

    private void OnGUI() {
        
    }

    /// <summary>
    /// Picks the option in some direction
    /// </summary>
    /// <param name="pick"></param>
    public void pickDirection(Direction pick) {
        if (reactor != null) {
            if (pick == Direction.North) {
                reactor.priority = ReactorManager.SystemType.Shields;
            }else if(pick == Direction.East) {
                reactor.priority = ReactorManager.SystemType.Weapons;
            } else if (pick == Direction.South) {
                reactor.priority = ReactorManager.SystemType.Thrusters;
            } else if (pick == Direction.West) {
                reactor.priority = ReactorManager.SystemType.None;
            }
        }
    }


    /// <summary>
    /// Turns on this menu and updates the initial display
    /// </summary>
    public void activate() {
        if (masterSelector != null) {
            selected = masterSelector.selected;
        }
        if (selected != null) {
            isActive = true;
            reactor = selected.GetComponentInChildren<ReactorManager>();
        }
        updateDisplay();
    }

    /// <summary>
    /// Turns off this menu
    /// </summary>
    public void deactivate() {
        isActive = false;
        rootCanvasReactorSettings.SetActive(false);
    }

    /// <summary>
    /// Should be used whenever the settings have been changed, as this component does not update otherwise.
    /// If the menu is off, turns it back on.
    /// </summary>
    public void updateDisplay() {
        if (reactor != null) {
            rootCanvasReactorSettings.SetActive(true);
            displayReactorSettingsChoice();
        } else {
            rootCanvasReactorSettings.SetActive(false);
        }
        //TODO other kinds of menus may go here...
        //TODO if none of the menus should open, rootCanvasReactorSettings.SetActive(false);
    }

    /// <summary>
    /// Moves the "selected" outline to whichever is the priority system.
    /// If no reactor component was found on the selected object, does nothing.
    /// </summary>
    void displayReactorSettingsChoice() {
        if (reactor == null) {
            return;
        }

        //puts the outline over the correct thing
        if (reactor.priority == ReactorManager.SystemType.None) {
            selectedOutlineDisplay.GetComponent<RectTransform>().position =
                emptyDisplay.GetComponent<RectTransform>().position;
        } else if (reactor.priority == ReactorManager.SystemType.Shields) {
            selectedOutlineDisplay.GetComponent<RectTransform>().position =
                shieldDisplay.GetComponent<RectTransform>().position;
        } else if (reactor.priority == ReactorManager.SystemType.Thrusters) {
            selectedOutlineDisplay.GetComponent<RectTransform>().position =
                thrusterDisplay.GetComponent<RectTransform>().position;
        } else if (reactor.priority == ReactorManager.SystemType.Weapons) {
            selectedOutlineDisplay.GetComponent<RectTransform>().position =
                weaponDisplay.GetComponent<RectTransform>().position;
        }

    }
}
