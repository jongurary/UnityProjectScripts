/*
 * Maintains an array of all thrusters attached to the ship
 * Determines when to fire the thrusters on the different sides of the ship
 * Gyroscopically rotates toward destination while burning the correct thrusters
 * 
 * 
 * TODO 
 * Make line renderers dotted lines or something else more easy to differentiate from each other
 * Rear-facing thrust isn't tested yet
 * 
 * NOTES
 * Re: hitting the retros/brakes, and damn near everything else force-related....
 * brakes currently handled by pretending there is drag on the rigidbody, not a bad approach, consider keeping.....
 * TODO allow thruster "turbo mode" by disabling drag on the ship's rigidbody (will overshoot targets when moving to them)
 * 
 * BUGS
 * The custom rotation template persists when a new rotation order is given, often somewhere off to the side of the screen
 * Before the first rotation target is set, if a destination is set, ships occasionaly spin towards where they started
 * (NOW A FEATURE) Ships will occasionaly overshoot movement destinations, without disengaging frontwards burners
 * After reaching a destination, ships often "drift" and rotate out of position (probably from inertia)
 * Gimbal lock-type defect can occasionaly stop thrusters from engaging in straight up/down direction
 * 
 * 01.05.2021 v1.4b
 * Fixed a critical bug where units stopped rotating towards an object after reaching the rotation destination
 * (Added null check for rotation target object as fix)
 * 
 * 07.21.2020 v1.4a
 * Attempted to fix custom rotation marker not going away in some instances
 * 
 * 07.02.2020 v1.4
 * Directional thrust works in left/right/for/back directions thanks to vector demon magic math
 * Override thruster functionality added
 * 
 * 07.01.2020 v1.3
 * Supports custom rotations by following the controller. Added marker and two private vars to support this functionality
 * Custom rotation uses a red outline to track the user's controller. On release, object will rotate towards that orientation.
 * 
 * 06.28.2020 v1.2
 * Rotation towards and object (follow target)
 * Forward thrusters should now activate only when rotation is finished to the DESTINATION target.
 * Various bugfixes
 * 
 * 06.16.2020 v1.1
 * Implemented rotation towards a Vector3
 * Added rotation tracer/markers
 * Updated destination marker/tracers visually, replaced cubes with line renderers
 * FunCtIoNaL PrOgRamMiNg
 * Bug fixes....
 * 
 * 06.12.2020 v1
 * Moved thrust to fixed update
 * Now starts only forward thrusters when facing forward 
 * TODO start other directions when facing those...
 * 
 * 06.12.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.12.2020
 */
using UnityEngine;

public class ThrusterController : MonoBehaviour{

    public Thruster[] thrusters;

    [Tooltip("The point in 3D space this unit is attmpting to approach.")]
    public Vector3 destination;
    //Set to true when a target is set, otherwise false
    private bool hasDestination = false;
    [Tooltip("The point in 3D space this unit is attmpting to face. May differ from destination.")]
    public Vector3 rotationTarget;
    //Set to true when a target is set, otherwise false
    private bool hasRotationTarget = false;
    [Tooltip("The object this unit is attmpting to face.")]
    public GameObject rotationTargetObject;

    public enum WeaponSide {
        Left,
        Right,
        Forward,
        Back
    }
    [Tooltip("Which side of this ship should face the rotation target (generally the side with the weapons)")]
    public WeaponSide weaponside;

    [Tooltip("Use Destination Marker prefab for this specific craft. This is the craft's outline that shows at the object's destination.")]
    public GameObject destinationMarker;
    [Tooltip("Use Destination Tracer prefab. This is the tracer bar that shows the path this unit is taking to its destination.")]
    public GameObject destinationTracer;
    //TODO there is no rotation marker prefab yet
    [Tooltip("Use Rotation Marker prefab. This is the marker that shows the object's look target.")]
    public GameObject rotationMarker;
    [Tooltip("Use Rotation Marker prefab for this specific craft. This is the outline of the craft that appears superimposed when setting a custom rotation")]
    public GameObject customRotationMarker;
    [Tooltip("Use Rotation Tracer prefab. This is the tracer bar that shows a line to the look target.")]
    public GameObject rotationTracer;

    //used to track the controller when displaying a custom rotation marker
    private bool followingCustomMarker = false;
    private GameObject controllerToFollow;

    [Tooltip("How fast this ship's gyro turns it")]
    public float rotationSpeed;
    //Buffer distance, within this range the destination is considered reached, and thrusters will stop
    private float MIN_DISTANCE_UNTIL_STOP = 10f;
    //Buffer distance to remove the destination marker outline
    private float MIN_DISTANCE_UNTIL_REMOVE_MARKER = 100f;
    //Buffer angle, within this range the rotationg destination is considered reached and forward thrusters can turn on
    private float DONE_ROTATING_TOLERANCE = 5f;
    //For relative positioning to determine left/right of destination
    private float MIN_DISTANCE_CONSIDERED_CENTERED = 50;

    [Tooltip("If true, overrides thrusters facing Forward to burn at full power regardless of destination")]
    public bool overrideForward;
    [Tooltip("If true, overrides thrusters facing Left to burn at full power regardless of destination")]
    public bool overrideLeft;
    [Tooltip("If true, overrides thrusters facing Right to burn at full power regardless of destination")]
    public bool overrideRight;
    [Tooltip("If true, overrides thrusters facing Back to burn at full power regardless of destination")]
    public bool overrideBack;

    void Start(){
        destination = transform.position;
        rotationTarget = transform.position;

        //Markers are placed relative to world space and shouldn't be a child of the ship itself.
        destinationMarker.transform.parent = null;
        rotationMarker.transform.parent = null;
        customRotationMarker.transform.parent = null;

        //TODO create a line renderer for the tracers if dev forgets to link them
    }

    private void OnDestroy() {
        Destroy(destinationMarker);
        Destroy(rotationMarker);
        Destroy(customRotationMarker);
    }

    void Update(){

        
    }

    private void FixedUpdate() {
        if (rotationTargetObject != null) {
            rotationTarget = rotationTargetObject.transform.position;
        }
        approachDestination();
        overrideThrusterMovement();
        rotateTowardsTarget();
        updateTargetTracer();
        updateCustomRotationMarker();
    }

    /// <summary>
    /// Engages thrusters manually in response to stick commands
    /// </summary>
    public void overrideThrusterMovement() {
        //quick escape if override modes are all off (a janky fix for making override play nice with main controls, do not remove)
        if (!overrideForward && !overrideBack && !overrideLeft && !overrideRight) {
            return;
        }
            
        foreach (Thruster t in thrusters) {
            if (t.thrustDirection == Thruster.ThrustDirection.Forward) {
                if (overrideForward) {
                    t.startThrust();
                } else {
                    t.endThrust();
                }
            } else if (t.thrustDirection == Thruster.ThrustDirection.Back) {
                if (overrideBack) {
                    t.startThrust();
                } else {
                    t.endThrust();
                }
            } else if (t.thrustDirection == Thruster.ThrustDirection.Left) {
                if (overrideLeft) {
                    t.startThrust();
                } else {
                    t.endThrust();
                }
            } else if (t.thrustDirection == Thruster.ThrustDirection.Right) {
                if (overrideRight) {
                    t.startThrust();
                } else {
                    t.endThrust();
                }
            }else if (t.thrustDirection == Thruster.ThrustDirection.Inertia) {
                //TODO inertial thrusters should assist in some more complex way than just helping a cardinal direction
                //Perhaps divide the force over the active directions?...
                if (overrideForward || overrideBack || overrideLeft || overrideRight) {
                    if (overrideForward) {
                        t.setInertialThrustAngle(transform.forward * 10000, transform.position);
                    } else if (overrideBack) {
                        t.setInertialThrustAngle(-transform.forward * 10000, transform.position);
                    } else if (overrideLeft) {
                        t.setInertialThrustAngle(transform.right * 10000, transform.position);
                    } else if (overrideRight) {
                        t.setInertialThrustAngle(-transform.right * 10000, transform.position);
                    }
                    t.startThrust();
                } else {
                    t.endThrust();
                }
            }       
        }
    }

    /// <summary>
    /// Controls all functionality related to approaching movement destinations
    /// </summary>
    public void approachDestination() {
        if (!hasDestination) {
            return;
        }

        float dist = Vector3.Distance(destination, transform.position);

        //removes the destination tracer when the object is close enough
        if (dist < MIN_DISTANCE_UNTIL_REMOVE_MARKER) {
            destinationMarker.SetActive(false);
        }

        if (dist > MIN_DISTANCE_UNTIL_STOP) {

            //Activate thrusters pointing to destination and deactivate all others

            //Inertial thrusters move in any direction, and are always active
            foreach (Thruster t in thrusters) {
                if (t.thrustDirection == Thruster.ThrustDirection.Inertia) {
                    t.setInertialThrustAngle(destination, transform.position);
                    t.startThrust();
                }
            }

            //Deprecated in v1.4 by relativePoint method
            //Alternate way to drive forward thrusters based on facing target
            //Vector3 destinationVector = (destination - transform.position).normalized;
            //Quaternion lookRotation = Quaternion.LookRotation(destinationVector);
            //if (Quaternion.Angle(transform.rotation, lookRotation) <= DONE_ROTATING_TOLERANCE) {
            //    foreach (Thruster t in thrusters) {
            //        if (t.thrustDirection == Thruster.ThrustDirection.Forward) {
            //            t.startThrust();
            //        }
            //    }
            //} else { //kills forward thrusters if rotation isn't finished yet
            //    foreach (Thruster t in thrusters) {
            //        if (t.thrustDirection == Thruster.ThrustDirection.Forward) {
            //            t.endThrust();
            //        }
            //    }
            //}

            //Activates directional thruster based on ancient Vector based demon magiks. 
            //TODO technically this is affected by the scale between the two points in local space, confirm that isn't an issue...
            //Note that InverseTransformDirection doesn't properly zero-out when facing forward for some maths reason I'm too dumb to understand
            Vector3 relativePoint = transform.InverseTransformPoint(destination);

            //Destination is roughly on the left
            if (relativePoint.x < -MIN_DISTANCE_CONSIDERED_CENTERED) {
                foreach (Thruster t in thrusters) {
                    if (t.thrustDirection == Thruster.ThrustDirection.Right) {
                        t.startThrust();
                    }
                }
            } else {
                foreach (Thruster t in thrusters) {
                    if (t.thrustDirection == Thruster.ThrustDirection.Right) {
                        t.endThrust();
                    }
                }
            }

            //Destination is roughly on the right
            if (relativePoint.x > MIN_DISTANCE_CONSIDERED_CENTERED) {
                foreach (Thruster t in thrusters) {
                    if (t.thrustDirection == Thruster.ThrustDirection.Left) {
                        t.startThrust();
                    }
                }
            } else {
                foreach (Thruster t in thrusters) {
                    if (t.thrustDirection == Thruster.ThrustDirection.Left) {
                        t.endThrust();
                    }
                }
            }

            //Facing target (roughly centered)
            //TODO this might not be the smartest way to handle forward/back
            if (relativePoint.x > -MIN_DISTANCE_CONSIDERED_CENTERED && relativePoint.x < MIN_DISTANCE_CONSIDERED_CENTERED) {
                if (relativePoint.z > MIN_DISTANCE_CONSIDERED_CENTERED) {
                    foreach (Thruster t in thrusters) {
                        if (t.thrustDirection == Thruster.ThrustDirection.Forward) {
                            t.startThrust();
                        } else if (t.thrustDirection == Thruster.ThrustDirection.Back) {
                            t.endThrust();
                        }
                    }
                } else if (relativePoint.z < -MIN_DISTANCE_CONSIDERED_CENTERED) {
                    foreach (Thruster t in thrusters) {
                        if (t.thrustDirection == Thruster.ThrustDirection.Forward) {
                            t.endThrust();
                        } else if (t.thrustDirection == Thruster.ThrustDirection.Back) {
                            t.startThrust();
                        }
                    }
                } else {
                    foreach (Thruster t in thrusters) {
                        if (t.thrustDirection == Thruster.ThrustDirection.Forward || t.thrustDirection == Thruster.ThrustDirection.Back) {
                            t.endThrust();
                        }
                    }
                }

            }

            updateDestinationTracer(dist);

        } else { //reached destination
            hasDestination = false;
            foreach (Thruster t in thrusters) {
                t.endThrust();
            }
            destinationTracer.SetActive(false);
        }
    }

    /// <summary>
    /// Forces thrusters to burn in the given direction until a new order is given or a reset is issued.
    /// Note that a forced burned left will turn off the right thruster override and vice-versa.
    /// </summary>
    /// <param name="direction"></param>
    public void overrideThrust(Thruster.ThrustDirection direction) {

        hasDestination = false;
        destinationMarker.SetActive(false);
        destinationTracer.SetActive(false);

        if (direction == Thruster.ThrustDirection.Forward) {
            overrideForward = true;
            overrideBack = false;
        } else if (direction == Thruster.ThrustDirection.Back) {
            overrideForward = false;
            overrideBack = true;
        } else if (direction == Thruster.ThrustDirection.Left) {
            overrideLeft = true;
            overrideRight = false;
        } else if (direction == Thruster.ThrustDirection.Right) {
            overrideLeft = false;
            overrideRight = true;
        }
    }

    /// <summary>
    /// Resets all manual thruster overrides
    /// </summary>
    public void resetOverrides() {
        overrideForward = false;
        overrideBack = false;
        overrideLeft = false;
        overrideRight = false;
        foreach (Thruster t in thrusters) {
            t.endThrust();
        }
    }

    /// <summary>
    /// Slerps/rotates towards the destination angle. Returns true if done rotating, false otherwise.
    /// Note that the rotation target is not neccessarily the same as the movement target!
    /// </summary>
    public bool rotateTowardsTarget() {
        if (!hasRotationTarget) {
            return true;
        }
        //ignore rotations to extremely close points because they create some odd unintended effects
        if(Vector3.Distance(rotationTarget, transform.position) < MIN_DISTANCE_UNTIL_STOP) {
            return false;
        }
        Vector3 destinationDirection = (rotationTarget - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(destinationDirection);

        //TODO decide if slerp or rotatetowards is better visually. Rotatetowards definitely makes more sense physically...
        //Or maybe even angular momentum...
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        if (Quaternion.Angle(transform.rotation, lookRotation) <= DONE_ROTATING_TOLERANCE) {
            rotationMarker.SetActive(false);
            if (rotationTargetObject == null) { 
                rotationTracer.SetActive(false); //only cease rotation if not tracking an object
                hasRotationTarget = false; //don't purge rotation tracer if tracking object ///TODO or should purge it? Is it cluttered UI?...
            }
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Updates the destination tracer's position and angle to draw a line between this object and its destination
    /// </summary>
    /// <param name="dist"></param>
    public void updateDestinationTracer(float dist) {
        LineRenderer lr = destinationTracer.GetComponent<LineRenderer>();
        if (lr != null) {
            lr.SetPositions(new Vector3[] { destination, transform.position });
        }

        //The destination marker also turns to match the parent's actual rotation
        destinationMarker.transform.rotation = transform.rotation;
    }

    /// <summary>
    /// Updates the rotation tracer's position and angle to draw a line between this object and its rotation target
    /// </summary>
    public void updateTargetTracer() {
        LineRenderer lr = rotationTracer.GetComponent<LineRenderer>();
        if (lr != null) {
            lr.SetPositions(new Vector3[] { rotationTarget, transform.position });
            lr.widthMultiplier = .6f;
        }
    }

    /// <summary>
    /// If active, updates the custom rotation marker's orientation to match the controller it's tracking
    /// </summary>
    public void updateCustomRotationMarker() {
        if (followingCustomMarker) {
            customRotationMarker.transform.localRotation = controllerToFollow.transform.rotation;
        }
    }

    /// <summary>
    /// Orders this unit to move towards the given point
    /// Sets an animation for the destination point
    /// </summary>
    /// <param name="destination"></param>
    public void setDestination(Vector3 destination) {
        this.destination = destination;
        hasDestination = true;
        resetOverrides();

        //if a rotation marker isn't set, create a "false" one at the transform's current location to avoid rotating
        if (!rotationMarker.activeSelf) {
            rotationTarget = transform.position;
        }
        destinationMarker.transform.position = destination;
        destinationMarker.transform.localScale = new Vector3(.1f, .1f, .1f);
        destinationMarker.transform.rotation = transform.rotation;
        destinationMarker.SetActive(true);

        destinationTracer.SetActive(true);
    }

    /// <summary>
    /// Orders this unit to rotate towards the given point
    /// Sets an animation for the target point
    /// </summary>
    /// <param name="target"></param>
    public void setRotationTarget(Vector3 target) {
        //Clear rotation target objects
        this.rotationTargetObject = null;
        this.rotationTarget = target;
        hasRotationTarget = true;

        rotationMarker.transform.position = rotationTarget;
        rotationMarker.transform.localScale = new Vector3(.1f, .1f, .1f);
        rotationMarker.transform.localRotation = Quaternion.identity;
        rotationMarker.SetActive(true);

        rotationTracer.SetActive(true);

        //clear custom rotation marker if applicable
        customRotationMarker.SetActive(false);
    }

    /// <summary>
    /// Orders this unit to rotate to face the given object, even if the object moves
    /// Sets an animation for the target object
    /// </summary>
    /// <param name="target"></param>
    public void setRotationTargetObject(GameObject targetObject) {
        this.rotationTargetObject = targetObject;
        //NOTE: Marker is not needed for a rotation target object
        hasRotationTarget = true;

        customRotationMarker.SetActive(false);
        rotationTracer.SetActive(true);
    }

    /// <summary>
    /// Invoked when the user holds the custom rotation button. Show the custom rotator outline
    /// </summary>
    /// <param name="toTrack">"The controller the outline will be following"</param>
    public void showCustomRotation(GameObject toTrack) {
        customRotationMarker.transform.position = transform.position;
        followingCustomMarker = true;
        controllerToFollow = toTrack;

        customRotationMarker.SetActive(true);
        //clear the "regular" rotation marker if applicable
        rotationMarker.SetActive(false);
    }

    /// <summary>
    /// Invoked when the user releases the custom rotation button to lock the desired rotation in.
    /// Orders a rotate towards the desired rotation
    /// </summary>
    /// <param name="controllerRotation"></param>
    public void setCustomRotation(Quaternion controllerRotation) {
        followingCustomMarker = false;
        //The custom rotation marker is in the desired rotation, so the "trick" here is to move it "forward" some distance and rotate to that point in 3D space
        //TODO this approach has several upsides and downsides, consider a less ratchet way to do this....
        customRotationMarker.transform.position += customRotationMarker.transform.forward * 500;
        hasRotationTarget = true;

        //Clear rotation target objects
        this.rotationTargetObject = null;
        //Set target and start tracer
        this.rotationTarget = customRotationMarker.transform.position;
        rotationTracer.SetActive(true);
    }
}
