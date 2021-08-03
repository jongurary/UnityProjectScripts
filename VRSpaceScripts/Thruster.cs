/*
 * Defines a single thruster: thrust power, which direction it pushes in, etc
 * 
 * 
 * 07.17.2020 v1.2a
 * Added isFunctional and isPaused to allow weapon to be disabled by other scripts
 * 
 * 07.05.20 v1.2
 * Thrusters now consume reactor energy
 * 
 * 06.16.20 v1.1
 * Added intertial thruster
 * tooltips and FuNcTiOnal ProGramMinG
 * 
 * 06.12.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.12.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour{

    [Tooltip("Object that has the rigidbody that actually gets the force applied to it")]
    public GameObject root;

    public enum ThrustDirection { 
        Left, 
        Right, 
        Forward, 
        Back,
        Inertia
    }
    [Tooltip("If you are above this ship, and the ship is moving away from you, which way is the thruster pointed? (Inertial" +
        "thrusters can exert force in any direction)")]
    public ThrustDirection thrustDirection;
    //Only inertial thrusters use this, to compute the correct force vector. Should be modified in the ThrusterController when computing destination
    private Vector3 inertialThrustDirection;

    [Tooltip("How much force does this thruster exert (For scale: 5000 is a little, 100,000 is a lot on a small ship)")]
    public float thrustPower;
    [Tooltip("Energy required to make one push of thrust")]
    public int energyPerThrust;
    [Tooltip("How many particles to emit per second when thruster is fully engaged")]
    public float emissionsWhenActive;
    public ParticleSystem thrustParticles;
    private ParticleSystem.EmissionModule emission;

    private bool isEngaged;

    public ReactorManager reactor;

    [Tooltip("Set to true automatically at start, set to false when this part is dead")]
    public bool isFunctional;
    [Tooltip("Set to false automatically at start, set to true by certain other scripts")]
    public bool isPaused;

    void Start() {
        emission = thrustParticles.emission;
        endThrust();

        isFunctional = true;
        isPaused = false;

        //Finding the reactor requires an expensive child search, so preferably don't be lazy and remember to link it.
        if (reactor == null) {
            reactor = transform.root.GetComponent<ReactorManager>();
        }
    }

    void Update(){

    }

    private void FixedUpdate() {
        burnThrust();
    }

    /// <summary>
    /// Drains reactor energy, applies force in thruster's desired direction
    /// </summary>
    private void burnThrust() {
        if (isEngaged && isFunctional && !isPaused) {
            if (!reactor.drainEnergy(energyPerThrust, ReactorManager.SystemType.Thrusters)) {
                //When not powered, reduce emissions to show the thruster isn't really working.
                emission.rateOverTime = emissionsWhenActive/10;
                return;
            }
            Vector3 direction = getDirection();
            Rigidbody rb = root.GetComponentInChildren<Rigidbody>();
            rb.AddForce(direction * thrustPower);
            emission.rateOverTime = emissionsWhenActive;
        }
    }

    /// <summary>
    /// Returns the direction this thruster's force should go in. 
    /// Note that inertial thrust angle is set externally (usually in ThrusterController) using setInertialThrusterAngle
    /// </summary>
    /// <returns></returns>
    private Vector3 getDirection() {
        if (thrustDirection == ThrustDirection.Inertia) {
            return inertialThrustDirection;
        } else {
            return computeThrustAngle();
        }
    }

    /// <summary>
    /// Recomputes forward/right/left/back relative to this object's current position/rotation in world space
    /// </summary>
    private Vector3 computeThrustAngle() {
        if (thrustDirection == ThrustDirection.Left) {
            return -transform.right;
        } else if (thrustDirection == ThrustDirection.Right) {
            return transform.right;
        } else if (thrustDirection == ThrustDirection.Forward) {
            return transform.forward;
        } else if (thrustDirection == ThrustDirection.Back) {
            return -transform.forward;
        } else if(thrustDirection == ThrustDirection.Inertia){
            return Vector3.zero;
        } else {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Intertial thrusters always exert force towards the destination, which is set by the thrust controller
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void setInertialThrustAngle(Vector3 from, Vector3 to) {
        if (thrustDirection == ThrustDirection.Inertia) {
            inertialThrustDirection = (from - to).normalized;
        } else {
            inertialThrustDirection = Vector3.zero;
        }
     }

    /// <summary>
    /// Turns on the thruster and applies force to the root object
    /// TODO rampup time?
    /// </summary>
    public void startThrust() {
        isEngaged = true;
    }

    /// <summary>
    /// Turns off the thruster
    /// TODO wind down time?
    /// </summary>
    public void endThrust() {
        isEngaged = false;
        emission.rateOverTime = 0;
    }
}
