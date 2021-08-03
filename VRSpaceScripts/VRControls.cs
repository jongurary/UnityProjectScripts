/*
 * Movement and controls code in VR
 * 
 * TODO
 * Convert remaining code in Update into the listener system
 * 
 * 01.05.2021 v1.9a
 * closeShipSettingsMenu now also deactivates the skills menu, affecting various situations where the ship is deselected
 * 
 * 07.29.2020 v1.9
 * Ability to select skills using the right stick (secondary functionality of thruster override)
 * Ability to cast skills using move button (secondary function on moveStick)
 * Migrated several methods to listener system
 * 
 * 07.23.2020 v1.8
 * Added rangefinder toggle switch
 * Fixed various bugs with ship settings menu, now closes menus when selecting a new ship
 * Properly handles compass directions in alternate movestick mode for ship settings
 * 
 * 07.10.2020 v1.7
 * Added support for ship settings menu, alternative movestick functionality when this menu is open
 * 
 * 07.08.2020 v1.6
 * Pointer sphere added for position/rotation targetting
 * Now attaches laser/pointer to a real gameObject instead of programatically creating primites
 * 
 * 07.07.2020 v1.5
 * A-button on movement hand enters the ship settings menu and opens the GUI and dismisses it.
 * Pulling the selection hand trigger also clears the settings menu
 * Finally figured out how to use listeners, more or less, flagged conversion to listeners as a TODO task
 * 
 * 07.02.20 v1.4
 * Thruster ovveride functionality on laser hand stick and clicking in stick
 * 
 * 07.01.20 v1.3
 * Changed B-Hold to a custom rotation command for ThrusterController
 * Coroutines and affiliated bugs removed from latch functions, now use single deltatimer (applies to Move and Rotate methods)
 * 
 * 06.27.20 v1.2
 * Rotate-towards-object on trigger pull
 * 
 * 06.16.20 v1.1
 * Rotation controls
 * 
 * 06.12.20 v1
 * Initial commit
 * 
 * Version 1 by Jon Gurary 06.12.2020
 */

using System;
using UnityEditor;
using UnityEngine;
using Valve.VR;


public class VRControls : MonoBehaviour{

    //TODO I have no idea what inputType does
    public SteamVR_Input_Sources inputType;
    //NOTE a "pose" is basically a controller object

    [Tooltip("The controller object from which the movement is controlled (left hand by default).")]
    public GameObject movementHand;
    private SteamVR_Behaviour_Pose movementHandPose;

    [Tooltip("The controller object from which the laser is emitted (right hand by default). Note that this hand does all targeting controls.")]
    public GameObject laserHand;
    private SteamVR_Behaviour_Pose laserHandPose;

    public SteamVR_Action_Vector2 moveStick;
    public SteamVR_Action_Boolean selectTarget;
    public SteamVR_Action_Boolean selectRotationTarget;
    public SteamVR_Action_Boolean commandMove;
    public SteamVR_Action_Vector2 zoomTrigger;
    public SteamVR_Action_Boolean resetCameraClick;
    public SteamVR_Action_Boolean commandRotate;
    public SteamVR_Action_Vector2 thrusterStick;
    public SteamVR_Action_Boolean thrusterReset;
    public SteamVR_Action_Boolean shipSettings;
    public SteamVR_Action_Vector2 movePointer;
    public SteamVR_Action_Boolean showRangeFinders;


    [Tooltip("The object that moves when the headset camera moves")]
    public GameObject cameraObject;
    public MasterSelector masterSelector;

    [Tooltip("Camera returns to this point in world space when it is reset")]
    public Vector3 cameraOrigin;

    [Tooltip("Manages picking the reactor priority and other submenus when a unit is selected")]
    public GUISettingsManager shipSettingsManager;

    //Laser Pointer variables
    private Color defaultLaserColor = Color.grey;
    private float laserThickness = 0.002f;
    private Color clickLaserColor = Color.green;
    private Color enemyClickLaserColor = Color.red;
    private GameObject holder; //used to align laser
    public GameObject pointer; //the laser itself
    public GameObject pointerSphere; //the ball along the laser that indicates where move orders will travel
    private float pointerSphereDistance = 40f;
    //the closest the pointer sphere can be along the laser beam
    private static float POINTER_SPHERE_MIN_DISTANCE = 10;
    //Max distance is (2* this value) + min distance
    private static float POINTER_SPHERE_DISTANCE_SPREAD = 200f;

    //enables long-tapping of the move command button
    private bool isMoveLatched = false;
    //does exactly the same thing for the rotate command button
    private bool isRotateLatched = false;
    //Used to end a long button press on release
    private bool usedRotateLatch = false;
    //Set to true when button is pressed, reset to false on release
    private bool rotatePressed = false;
    private bool movePressed = false;
    //how long you have, in seconds, for the move button to be considered a long press
    private static float MOVE_LATCH_DELAY = .5f;

    //Timers used to determine button-holds vs quick presses
    private float moveLatchTimer = 0f;
    private float rotateLatchTimer = 0f;

    //How fast the camera pans, sans acceleration.
    private static float CAMERA_PAN_SPEED = 10;
    //How fast the camera zooms up and down, sans acceleration
    private static float CAMERA_ZOOM_SPEED = 400; 
    //as the camera moves, it accelerates, this is the multiplier used for that and should always start at 1f
    private float cameraPanAcceleration = 1f;
    //this is the rate the multiplier grows per frame and can be edited to increase pan acceleration rate
    private static float CAMERA_ACCELERATION_RATE = .1f;
    //the camera will not accelerate beyond this point
    private static float CAMERA_TOP_SPEED = 500;
    [Tooltip("Disables camera acceleration during pan if false")]
    public static bool cameraAccelerationEnabled;

    void Start(){
        //null checks in case dev forgot to link bindings in editor
        if (moveStick == null) {
            moveStick = SteamVR_Input.GetVector2Action("MoveStick");
        }
        if (selectTarget == null){
            selectTarget = SteamVR_Input.GetBooleanAction("SelectTarget");
        }
        if (selectTarget == null) {
            selectTarget = SteamVR_Input.GetBooleanAction("SelectRotationTarget");
        }
        if (commandMove == null) {
            commandMove = SteamVR_Input.GetBooleanAction("CommandMove");
        }
        if (zoomTrigger == null) {
            zoomTrigger = SteamVR_Input.GetVector2Action("ZoomCamera");
        }
        if (resetCameraClick == null) {
            resetCameraClick = SteamVR_Input.GetBooleanAction("ResetCamera");
        }
        if (commandRotate == null) {
            commandRotate = SteamVR_Input.GetBooleanAction("CommandRotate");
        }
        if (thrusterStick == null) {
            thrusterStick = SteamVR_Input.GetVector2Action("ThrusterStick");
        }
        if (thrusterReset == null) {
            thrusterReset = SteamVR_Input.GetBooleanAction("ThrusterReset");
        }
        if (shipSettings == null) {
            shipSettings = SteamVR_Input.GetBooleanAction("ShipSettings");
        }
        if (movePointer == null) {
            movePointer = SteamVR_Input.GetVector2Action("MovePointer");
        }
        if (showRangeFinders == null) {
            showRangeFinders = SteamVR_Input.GetBooleanAction("ShowRangeFinders");
        }
        if (cameraObject == null) {
            cameraObject = GameObject.Find("Camera");
        }
        if (laserHand == null) {
            laserHand = GameObject.Find("Controller (right)");
        }
        if (movementHand == null) {
            movementHand = GameObject.Find("Controller (left)");
        }
        if (masterSelector == null) {
            masterSelector = GameObject.Find("Master Selector").GetComponent<MasterSelector>();
        }
        if (shipSettingsManager == null) {
            shipSettingsManager = GetComponentInChildren<GUISettingsManager>();
        }

        //instantiating the laser object (copy/edited from SteamVR_LaserPointer)
        laserHandPose = laserHand.GetComponentInChildren<SteamVR_Behaviour_Pose>();
        movementHandPose = movementHand.GetComponentInChildren<SteamVR_Behaviour_Pose>();
        holder = new GameObject();
        holder.transform.parent = laserHand.transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(laserThickness, laserThickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        pointer.transform.localRotation = Quaternion.identity;


        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", defaultLaserColor);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;
        pointerSphere.transform.position = holder.transform.position + holder.transform.forward * POINTER_SPHERE_MIN_DISTANCE;

        moveStick.AddOnAxisListener(moveCamera, SteamVR_Input_Sources.Any);
        thrusterStick.AddOnAxisListener(thusterOverrides, SteamVR_Input_Sources.Any);
        commandMove.AddOnStateDownListener(orderMoveDown, SteamVR_Input_Sources.Any);
        commandMove.AddOnStateUpListener(orderMoveUp, SteamVR_Input_Sources.Any);

        shipSettings.AddOnStateDownListener(toggleShipSettings, SteamVR_Input_Sources.Any);
        thrusterReset.AddOnStateDownListener(resetThrusters, SteamVR_Input_Sources.Any);
        movePointer.AddOnAxisListener(movePointerLocation, SteamVR_Input_Sources.Any);
        showRangeFinders.AddOnStateDownListener(toggleRangeFinders, SteamVR_Input_Sources.Any);
    }


    private void resetThrusters(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        if (masterSelector.selected == null) {
            return;
        }
        ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
        if (thrustController == null) {
            return;
        }
        thrustController.resetOverrides();
    }

    /// <summary>
    /// Toggles rangefinder display on and off for the selected unit
    /// TODO potentially a three-stage toggle, where the third is all units?
    /// </summary>
    /// <param name="fromAction"></param>
    /// <param name="fromSource"></param>
    private void toggleRangeFinders(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        if (masterSelector.selected != null) {
            RangeFinderDisplayManager manager = masterSelector.selected.GetComponent<RangeFinderDisplayManager>();
            if (manager != null) {
                if (manager.isEnabled) {
                    manager.disableDisplays();
                } else {
                    manager.enableDisplays();
                }
            }
        }
    }


    /// <summary>
    /// Moves the pointer to the location of the laser hand's trackpad y axis
    /// </summary>
    /// <param name="fromAction"></param>
    /// <param name="fromSource"></param>
    /// <param name="axis"></param>
    /// <param name="delta"></param>
    private void movePointerLocation(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
        pointerSphereDistance = POINTER_SPHERE_MIN_DISTANCE + (axis.y + 1) * POINTER_SPHERE_DISTANCE_SPREAD;
        //Note that the sphere moves in the laser pointer method, not here, because it moves along with the laser
    }

    /// <summary>
    /// Toggles the ship status menu and skills menu when the button is pressed down
    /// </summary>
    /// <param name="fromAction"></param>
    /// <param name="fromSource"></param>
    private void toggleShipSettings(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        if (masterSelector.selected != null) {
            GUIShipManager manager = masterSelector.selected.GetComponent<GUIShipManager>();
            if (manager != null) {
                if (manager.isActive) {
                    manager.deactivate();
                    shipSettingsManager.deactivate();
                } else {
                    manager.activate();
                    shipSettingsManager.activate();
                    shipSettingsManager.updateDisplay();
                }
            }

            GUISkillsManager skillsManager = masterSelector.selected.GetComponent<GUISkillsManager>();
            if (skillsManager != null) {
                if (skillsManager.isActive) {
                    skillsManager.deactivate();
                } else {
                    skillsManager.activate();
                }
            }

        }

    }

    //TODO remove all of these and move to listeners
    /*
     * It may make sense to keep this in update for responsiveness
     * 
     */
    void Update(){
        resetCamera();
        zoomCamera();
        processLaserHand();
        orderMove();
        orderRotate();
    }


    /// <summary>
    /// Overrides the thrusters on the selected ship to burn in the direction the stick is tilted. 
    /// Overrides continue until a direction order is issued, a new thrust override direction is issued, or the stick is pressed to reset
    /// </summary>
    private void thusterOverrides(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
        if (masterSelector.selected == null) {
            return;
        }

        if (!shipSettingsManager.isActive) { //normal functionality, overrides thrusters 
            ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
            if (thrustController == null) {
                return;
            }

            Vector2 thrustStickAxis = axis;
            if (thrustStickAxis.x > .7f) {
                thrustController.overrideThrust(Thruster.ThrustDirection.Left);
            } else if (thrustStickAxis.x < -.7f) {
                thrustController.overrideThrust(Thruster.ThrustDirection.Right);
            }
            if (thrustStickAxis.y > .7f) {
                thrustController.overrideThrust(Thruster.ThrustDirection.Forward);
            } else if (thrustStickAxis.y < -.7f) {
                thrustController.overrideThrust(Thruster.ThrustDirection.Back);
            }
        } else { //ship settings menu is open, selects between ship abilities

            GUISkillsManager skillsManager = masterSelector.selected.GetComponent<GUISkillsManager>();
            if (skillsManager == null) {
                return;
            }
            //Determines the stick direction based on fancy pants compass maths
            //TODO see if it works better usability-wise if small axis magnitudes are ignored (could help with drifting sticks?)
            float angle = Mathf.Atan2(axis.y, axis.x) / (float)Math.PI * 180f;
            if (angle < 0) {
                angle += 360;
            }
            float degreesPerChoice = 360 / 8 / 2;

            if (angle > 90 - degreesPerChoice && angle < 90 + degreesPerChoice) {
                skillsManager.pickDirection(GUISkillsManager.Direction.North);
            } else if (angle > 135 - degreesPerChoice && angle < 135 + degreesPerChoice) {
                skillsManager.pickDirection(GUISkillsManager.Direction.NorthWest);
            } else if (angle > 180 - degreesPerChoice && angle < 180 + degreesPerChoice) {
                skillsManager.pickDirection(GUISkillsManager.Direction.West);
            } else if (angle > 225 - degreesPerChoice && angle < 225 + degreesPerChoice) {
                skillsManager.pickDirection(GUISkillsManager.Direction.SouthWest);
            } else if (angle > 270 - degreesPerChoice && angle < 270 + degreesPerChoice) {
                skillsManager.pickDirection(GUISkillsManager.Direction.South);
            } else if (angle > 315 - degreesPerChoice && angle < 315 + degreesPerChoice) {
                skillsManager.pickDirection(GUISkillsManager.Direction.SouthEast);
            } else if ((angle > 360 - degreesPerChoice && angle < 360) || (angle > 0 && angle < 0 + degreesPerChoice)) {
                skillsManager.pickDirection(GUISkillsManager.Direction.East);
            } else if (angle > 45 - degreesPerChoice && angle < 45 + degreesPerChoice) {
                skillsManager.pickDirection(GUISkillsManager.Direction.NorthEast);
            }
        }
    }

    /// <summary>
    /// Moves the camera in response to analog stick movement.
    /// When the settings menu is open, selects left sub-menu options instead
    /// </summary>
    private void moveCamera(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
        if (!shipSettingsManager.isActive) { //normal functionality, moves around the camera
            if (axis.x > .5f || axis.y > .5f || axis.x < -.5f || axis.y < -.5f) {
                if (cameraPanAcceleration < CAMERA_TOP_SPEED) {
                    cameraPanAcceleration += CAMERA_ACCELERATION_RATE;
                }
            } else {
                cameraPanAcceleration = 1f;
            }
            transform.Translate(cameraObject.transform.forward * axis.y * Time.deltaTime * CAMERA_PAN_SPEED * cameraPanAcceleration);
            transform.Translate(cameraObject.transform.right * axis.x * Time.deltaTime * CAMERA_PAN_SPEED * cameraPanAcceleration);
        } else { //ship settings menu is open, picks between the available options
            //Determines the stick direction based on fancy pants compass maths
            //TODO see if it works better usability-wise if small axis magnitudes are ignored (could help with drifting sticks?)
            float angle = Mathf.Atan2(axis.y, axis.x) / (float) Math.PI * 180f;
            if (angle < 0) {
                angle += 360;
            }
            float degreesPerChoice = 360 / 8 / 2;

            if (angle > 90 - degreesPerChoice && angle < 90 + degreesPerChoice) {
                shipSettingsManager.pickDirection(GUISettingsManager.Direction.North);
            } else if (angle > 180 - degreesPerChoice && angle < 180 + degreesPerChoice) {
                shipSettingsManager.pickDirection(GUISettingsManager.Direction.West);
            } else if (angle > 270 - degreesPerChoice && angle < 270 + degreesPerChoice) {
                shipSettingsManager.pickDirection(GUISettingsManager.Direction.South);
            } else if ((angle > 360 - degreesPerChoice && angle < 360) || (angle > 0 && angle < 0 + degreesPerChoice)) {
                shipSettingsManager.pickDirection(GUISettingsManager.Direction.East);
            }
            shipSettingsManager.updateDisplay();
        }
    }

    /// <summary>
    /// Returns the camera to the starting vector when the movestick is clicked in
    /// </summary>
    void resetCamera() {
        if (resetCameraClick.GetState(movementHandPose.inputSource)) {
            transform.position = cameraOrigin;
        }
    }

    /// <summary>
    /// "Zooms" the camera by rapdily panning up/down
    /// You can't zoom in VR because vomit comet...
    /// </summary>
    void zoomCamera() {
        transform.Translate(cameraObject.transform.up * zoomTrigger.axis.y * Time.deltaTime * CAMERA_ZOOM_SPEED);
    }

    /// <summary>
    /// Handles everything that happens when a unit is to be selected by laser target.
    /// Assumes the selection is valid and everything is non-null
    /// </summary>
    private void selectUnit(GameObject selection) {
        masterSelector.selected = selection;
    }

    //TODO sending out infinite length rays every frame MIGHT BE A TERRIBLE FUCKING IDEA

    /// <summary>
    /// Targets things with the laser beam when the trigger is pressed down.
    /// Commands the currently selected unit to continuosly rotate towards a selected enemy
    /// Only objects on the "Targeting" layer can be struck by a targeting laser!
    /// 
    /// Also closes the ship settings menu quickly
    /// </summary>
    void processLaserHand() {

        //Laser hit detection (copy/edited from SteamVR_LaserPointer)
        float dist = 100f;
        bool hasHit = false;
        Color laserColor = defaultLaserColor;
        //TODO these two blocks of code differ only by the outermost and innermost code (input type and action), could maybe be combined
        if (selectTarget.GetState(laserHandPose.inputSource)) {
            Ray ray = new Ray(laserHand.transform.position, laserHand.transform.forward);
            int layerMask = LayerMask.GetMask("Targeting");
            RaycastHit hit;
            GameObject target;
            hasHit = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
            if (hasHit) {
                closeShipSettingsMenu();
                closeRangeFinders();
                target = hit.transform.gameObject; //BUG? Should be transform.root?
                UnitManager manager = hit.transform.root.gameObject.GetComponent<UnitManager>();
                if (manager != null) {
                    if (manager.owner == UnitManager.Ownership.Player) { //Selection laser behavior on a friendly unit
                        selectUnit(target);
                        laserColor = clickLaserColor;
                    }
                }
            }   
        }

        if (selectRotationTarget.GetState(laserHandPose.inputSource)) {
            //NOTE: Not sure if these submenus should close here or not. Consider...
            //closeShipSettingsMenu();
            //closeRangeFinders();
            Ray ray = new Ray(laserHand.transform.position, laserHand.transform.forward);
            int layerMask = LayerMask.GetMask("Targeting");
            RaycastHit hit;
            GameObject target;
            hasHit = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
            if (hasHit) {
                target = hit.transform.gameObject;
                UnitManager manager = hit.transform.root.gameObject.GetComponent<UnitManager>();
                if (manager != null) {
                    if (manager.owner == UnitManager.Ownership.Enemy) { //Rotation order behavior on an enemy unit
                        if (masterSelector.selected != null) {
                            ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
                            thrustController.setRotationTargetObject(hit.transform.gameObject);
                            laserColor = enemyClickLaserColor;
                        }
                    }
                }

            }
        }

        //Update color of laser and fix length to not clip through target (copy/edited from SteamVR_LaserPointer)
        if (selectTarget != null && selectTarget.GetState(laserHandPose.inputSource) && hasHit) {
            pointer.transform.localScale = new Vector3(laserThickness * 5f, laserThickness * 5f, dist);
            pointer.GetComponent<MeshRenderer>().material.color = laserColor;
        } else {
            pointer.transform.localScale = new Vector3(laserThickness, laserThickness, dist);
            pointer.GetComponent<MeshRenderer>().material.color = laserColor;
        }
        
        pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        pointerSphere.transform.position = holder.transform.position + holder.transform.forward * pointerSphereDistance;
    }

    /// <summary>
    /// Closes the ship settings menu, if a ship is selected and the menu is open
    /// </summary>
    private void closeShipSettingsMenu() {
        if (masterSelector.selected != null) {
            GUIShipManager manager = masterSelector.selected.GetComponent<GUIShipManager>();
            if (manager != null) {
                  manager.deactivate();
            }
            GUISkillsManager skillsManager = masterSelector.selected.GetComponent<GUISkillsManager>();
            if (skillsManager != null) {
                if (skillsManager.isActive) {
                    skillsManager.deactivate();
                }
            }
        }
        shipSettingsManager.deactivate();


    }

    private void closeRangeFinders() {
        if (masterSelector.selected != null) {
            RangeFinderDisplayManager manager = masterSelector.selected.GetComponent<RangeFinderDisplayManager>();
            if (manager != null) {
                if (manager.isEnabled) {
                    manager.disableDisplays();
                }
            }
        }
    }


    //TODO Move has been updated to be non-trash, fix rotate to also be non-trash. Some variables can be purged now.


    private void orderMoveDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        if (!shipSettingsManager.isActive) { //normal functionality, moves ships, triggers long press eventually
            if (!isMoveLatched) {
                isMoveLatched = true;
            }
            moveLatchTimer = Time.time;

            //Move order is executed immediately when the button is presssed
            if (masterSelector.selected != null) {
                ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
                thrustController.setDestination(pointerSphere.transform.position);
            }
        } else { //alternate functionality, casts the selected menu skill
            GUISkillsManager skillsManager = masterSelector.selected.GetComponent<GUISkillsManager>();
            if (skillsManager == null) {
                return;
            }
            SkillCasting skill = skillsManager.activeSkill.GetComponent<SkillCasting>();
            if (skill != null) {
                skill.castSkill();
            }
        }
    }

    private void orderMoveUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        isMoveLatched = false;

        if (Time.time - moveLatchTimer < MOVE_LATCH_DELAY) { //short press release only actions
            
        } else { //long press release only actions

        }
    }

    /// <summary>
    /// Fires off the long-press functionality of orderMove when the appropriate time has elapsed
    /// </summary>
    void orderMove() {
        //button held down functionality
        if(isMoveLatched && Time.time - moveLatchTimer > MOVE_LATCH_DELAY) {
            if (masterSelector.selected != null) {
                ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
                thrustController.setDestination(pointerSphere.transform.position);
                thrustController.setRotationTarget(pointerSphere.transform.position);
            }
        }
    }

    /// <summary>
    /// Command the selected unit to rotate towards the controller.
    /// A long press begins the custom rotation process, which ends when the button is released
    /// </summary>
    void orderRotate() {
        //if the button is pressed
        if (commandRotate.GetState(laserHandPose.inputSource) == true) {
            rotatePressed = true;
            if (!isRotateLatched) { //if this is the first frame where the button has been pressed, begin the latch and timer
                isRotateLatched = true;
                rotateLatchTimer = 0;
            }

            //increment latch timer
            rotateLatchTimer += Time.deltaTime;

            //if latch is locked and timer has elapsed, this is a long press, do the long-press held down action
            if(isRotateLatched && rotateLatchTimer > MOVE_LATCH_DELAY) {
                if (masterSelector.selected != null) {
                    ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
                    thrustController.showCustomRotation(laserHand);
                    usedRotateLatch = true;
                }
            }
        } else { //button was released (or not pressed! This is why the rotatePressed state is required)
            if (rotatePressed) { //avoids execution of actions when the button is never pressed
                rotatePressed = false;
                isRotateLatched = false;
                if (usedRotateLatch) { //long press release action, order a custom move
                    ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
                    if (thrustController != null) {
                        thrustController.setCustomRotation(laserHand.transform.rotation);
                    }
                    usedRotateLatch = false;
                } else { //regular release, order a simple move
                    if (masterSelector.selected != null) {
                        ThrusterController thrustController = masterSelector.selected.GetComponent<ThrusterController>();
                        if (thrustController != null) {
                            thrustController.setRotationTarget(pointerSphere.transform.position);
                        }
                    }
                }
            }
        }


    }

}
