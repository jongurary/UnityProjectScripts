/*
 * Provides a common place for all skills that a ship can cast to be accessed.
 * Manages which skill appears in which position in the skills menu, and facilitates connections to modules that skills affect
 * Every unit that has castable skills should have this module. The unit should also carry its own skills, which should be linked manually to this module.
 * 
 * BUGS
 * (FIXED in VRControls) This menu often persists when a different ship is selected
 * 
 * 01.05.2020 v1.1a
 * When deactivating, selector now resets to default position, and active skill is purged (TODO apply same to reactor controls...)
 * 
 * 07.29.2020 v1.1
 * Skills are placed in their correct positions programatically to form a perfect hexagon
 * Handles the selector outline
 * Activate/Deactivate functionality, skills list
 * Links to skillCasting to execute actual skill functionality
 * 
 * 07.23.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 07.23.2020
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUISkillsManager : MonoBehaviour{

    public GameObject NorthSkill;
    public GameObject NorthEastSkill;
    public GameObject EastSkill;
    public GameObject SouthEastSkill;
    public GameObject SouthSkill;
    public GameObject SouthWestSkill;
    public GameObject WestSkill;
    public GameObject NorthWestSkill;

    [Tooltip("Outline used for picking which skill you want to activate")]
    public GameObject selectorIcon;
    [Tooltip("Skill that's picked by the selector. Can be set manually to test casts (you still have to push the cast button)")]
    public GameObject activeSkill;

    //A list of all the above skills, for easier access to all items
    private List<GameObject> listOfSkills = new List<GameObject>();

    public GameObject rootCanvas;

    public bool isActive = false;

    private float SPACING_SIZE = 40;
    private float LOCAL_SCALE = .5f;
    private float BASE_X = 130;
    private float BASE_Y = -100;

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
        fillList();
        placeSkillsInLocations();
        deactivate();

        //expensive af, better to link manually.
        if (rootCanvas == null) {
            rootCanvas = GameObject.Find("ShipSkillsManager");
        }

    }

    void Update(){
        
    }

    /// <summary>
    /// Highlights the icon in some direction
    /// </summary>
    /// <param name="pick"></param>
    public void pickDirection(Direction pick) {
        if (pick == Direction.North) {
            if (NorthSkill != null) {
                activeSkill = NorthSkill;
            }
        } else if (pick == Direction.East) {
            if (EastSkill != null) {
                activeSkill = EastSkill;
            }
        } else if (pick == Direction.South) {
            if (SouthSkill != null) {
                activeSkill = SouthSkill;
            }
        } else if (pick == Direction.West) {
            if (WestSkill != null) {
                activeSkill = WestSkill;
            }
        } else if (pick == Direction.NorthEast) {
            if (NorthEastSkill != null) {
                activeSkill = NorthEastSkill;
            }
        } else if (pick == Direction.NorthWest) {
            if (NorthWestSkill != null) {
                activeSkill = NorthWestSkill;
            }
        } else if (pick == Direction.SouthEast) {
            if (SouthEastSkill != null) {
                activeSkill = SouthEastSkill;
            }
        } else if (pick == Direction.SouthWest) {
            if (SouthWestSkill != null) {
                activeSkill = SouthWestSkill;
            }
        }
        if (selectorIcon != null && activeSkill != null) {
            RectTransform selectorIconRect = selectorIcon.GetComponent<RectTransform>();
            if (selectorIconRect != null) {
                selectorIconRect.localPosition = activeSkill.GetComponent<RectTransform>().localPosition;
            }
        } else {
            //do nothing if no new active skill is set
        }
    }

    /// <summary>
    /// Provides a simple interface to update the entire cadre of skills at once. Skills that are not needed can be set to null.
    /// Skills should probably be placed again after this is used.
    /// </summary>
    /// <param name="north"></param>
    /// <param name="northEast"></param>
    /// <param name="east"></param>
    /// <param name="southEast"></param>
    /// <param name="south"></param>
    /// <param name="southWest"></param>
    /// <param name="west"></param>
    /// <param name="northWest"></param>
    public void updateAllSkills(GameObject north, GameObject northEast, GameObject east, GameObject southEast, GameObject south,
        GameObject southWest, GameObject west, GameObject northWest) {
        NorthSkill = north;
        NorthEastSkill = northEast;
        EastSkill = east;
        SouthEastSkill = southEast;
        SouthSkill = south;
        SouthWestSkill = southWest;
        WestSkill = west;
        NorthWestSkill = northWest;
    }

    /// <summary>
    /// Resets the selector to the default position and set the active skill back to none
    /// </summary>
    public void resetSelector() {
        if (selectorIcon != null) {
            RectTransform selectorIconRect = selectorIcon.GetComponent<RectTransform>();
            selectorIconRect.localPosition = new Vector3(BASE_X, BASE_Y, 0);
        }
        activeSkill = null;
    }

    /// <summary>
    /// Hides displayed skills and disables skill functionality. Note: Does not deactivate skill objects, so their coroutines will continue to run
    /// </summary>
    public void deactivate() {
        isActive = false;
        resetSelector();
        foreach(GameObject skill in listOfSkills) {
            SkillCasting castable = skill.GetComponent<SkillCasting>();
            if (castable != null) {
                castable.hideAllAnimations();
            }
        }
        if (selectorIcon != null) {
            selectorIcon.GetComponent<Image>().enabled = false;
        }
    }

    public void activate() {
        isActive = true;
        foreach (GameObject skill in listOfSkills) {
            SkillCasting castable = skill.GetComponent<SkillCasting>();
            if (castable != null) {
                castable.showAllAnimations();
            }
        }
        if (selectorIcon != null) {
            selectorIcon.GetComponent<Image>().enabled = true;
        }
    }

    /// <summary>
    /// Populates the list of skills with all non-null skills
    /// </summary>
    private void fillList() {
        if (NorthSkill != null) {
            listOfSkills.Add(NorthSkill);
        }
        if (NorthEastSkill != null) {
            listOfSkills.Add(NorthEastSkill);
        }
        if (EastSkill != null) {
            listOfSkills.Add(EastSkill);
        }
        if (SouthEastSkill != null) {
            listOfSkills.Add(SouthEastSkill);
        }
        if (SouthSkill != null) {
            listOfSkills.Add(SouthSkill);
        }
        if (SouthWestSkill != null) {
            listOfSkills.Add(SouthWestSkill);
        }
        if (WestSkill != null) {
            listOfSkills.Add(WestSkill);
        }
        if (NorthWestSkill != null) {
            listOfSkills.Add(NorthWestSkill);
        }
    }

    /// <summary>
    /// Places all the skill icons in the correct locations on startup programatically
    /// Skills are children of their gameobject until this script parents them to the GUI ShipSkillsManager
    /// Skills are also renamed for better management in editor
    /// </summary>
    private void placeSkillsInLocations() {

        foreach(GameObject skill in listOfSkills) {
            SkillCasting castable = skill.GetComponent<SkillCasting>();
            if (castable != null) {
                castable.owner = skill.transform.root.gameObject;
            }
            skill.transform.SetParent(rootCanvas.transform, true);
            skill.transform.localScale = new Vector3(LOCAL_SCALE, LOCAL_SCALE, LOCAL_SCALE);
            skill.transform.localRotation = Quaternion.identity;
            skill.name = skill.name + "_" + transform.root.gameObject.name;
        }

        if (selectorIcon != null) {
            selectorIcon.transform.SetParent(rootCanvas.transform, true);
            selectorIcon.transform.localRotation = Quaternion.identity;
            if (selectorIcon != null) {
                RectTransform selectorIconRect = selectorIcon.GetComponent<RectTransform>();
                if (selectorIconRect != null) {
                    selectorIconRect.transform.localScale = new Vector3(LOCAL_SCALE * 1.1f, LOCAL_SCALE * 1.1f, LOCAL_SCALE * 1.1f);
                    selectorIconRect.localPosition = new Vector3(BASE_X, BASE_Y, 0);
                    selectorIconRect.name = selectorIconRect.name + "_" + transform.root.gameObject.name;
                }
            }
        }

        if (NorthSkill != null) {
            RectTransform NorthSkillRect = NorthSkill.GetComponent<RectTransform>();
            if (NorthSkillRect != null) {
                NorthSkillRect.localPosition = new Vector3(BASE_X, BASE_Y + SPACING_SIZE * 2, 0);
            }
        }

        if (SouthSkill != null) {
            RectTransform SouthSkillRect = SouthSkill.GetComponent<RectTransform>();
            if (SouthSkillRect != null) {
                SouthSkillRect.localPosition = new Vector3(BASE_X, BASE_Y - SPACING_SIZE * 2, 0);
            }
        }

        if (EastSkill != null) {
            RectTransform EastSkillRect = EastSkill.GetComponent<RectTransform>();
            if (EastSkillRect != null) {
                EastSkillRect.localPosition = new Vector3(BASE_X + SPACING_SIZE * 2, BASE_Y, 0);
            }
        }

        if (WestSkill != null) {
            RectTransform WestSkillRect = WestSkill.GetComponent<RectTransform>();
            if (WestSkillRect != null) {
                WestSkillRect.localPosition = new Vector3(BASE_X - SPACING_SIZE * 2, BASE_Y, 0);
            }
        }

        if (NorthEastSkill != null) {
            RectTransform NorthEastSkillRect = NorthEastSkill.GetComponent<RectTransform>();
            if (NorthEastSkillRect != null) {
                NorthEastSkillRect.localPosition = new Vector3(BASE_X + SPACING_SIZE, BASE_Y + SPACING_SIZE, 0);
            }
        }

        if (SouthEastSkill != null) {
            RectTransform SouthEastSkillRect = SouthEastSkill.GetComponent<RectTransform>();
            if (SouthEastSkillRect != null) {
                SouthEastSkillRect.localPosition = new Vector3(BASE_X + SPACING_SIZE, BASE_Y - SPACING_SIZE, 0);
            }
        }

        if (SouthWestSkill != null) {
            RectTransform SouthWestSkillRect = SouthWestSkill.GetComponent<RectTransform>();
            if (SouthWestSkillRect != null) {
                SouthWestSkillRect.localPosition = new Vector3(BASE_X - SPACING_SIZE, BASE_Y - SPACING_SIZE, 0);
            }
        }

        if (NorthWestSkill != null) {
            RectTransform NorthWestSkillRect = NorthWestSkill.GetComponent<RectTransform>();
            if (NorthWestSkillRect != null) {
                NorthWestSkillRect.localPosition = new Vector3(BASE_X - SPACING_SIZE, BASE_Y + SPACING_SIZE, 0);
            }
        }
    }
}
