/*
 * Missiles fly towards a target with a constant thrust until they strike the target, or run out of fuel (time out).
 * This script handles thrust and death due to fuel running out.
 * Missiles turn relatively slowly towards their target and only thrust in the forward vector
 * 
 * TODO
 * Add smaller animation when expiring due to fuel
 * Make sure missiles don't hit their own host
 * 
 * 01.23.2021 v1.2b
 * Added null checks for target
 * 
 * 01.06.2021 v1.2
 * Now creates a seperate gameObject for explosion, see Explosion class for details
 * Also creates a gameObject for animating the explosion
 * Now triggers an animation and explosion when out of fuel
 * Gizmo for visualizing explosion radius
 * Now moves towards a GameObject target instead of a transform
 * 
 * 01.05.2021 v1.1
 * Now destroys self after a timeout period
 * Damages parts around the hit area (REMOVED) (without spawning a damage object!)
 * Added a "Ship Parts" layer to accomodate new behavior
 * 
 * 09.18.2020 v1.0
 * initial commit
 * 
 * @author v1 Jonathan Gurary 09.18.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileMover : MonoBehaviour
{
    [Tooltip("What the missile is flying towards")]
    public GameObject target;
    [Tooltip("Missiles do 1 (distance max) to 100 (distance zero) percent of their possible damage within the blast radius, computed using linear distance")]
    public float blastRadius;
    [Tooltip("Prefab containing the explosion script responsible for dealing damage")]
    public GameObject explosionDamage;
    [Tooltip("Prefab containing the script responsible for the explosion animation")]
    public GameObject explosionAnimation;
    [Tooltip("Sets animation object to die after this amount of time (seconds)")]
    public float explosionAnimationDuration;

    [Tooltip("Unit's missile defense systems will ignore missiles with the same ownership.")]
    public UnitManager.Ownership ownerType;

    private bool diedByFuelExpiration = true;
    private static int MISSILE_DEFENSE_LAYER = 17; //ignore collisions with this layer...

    void Start(){
        Destroy(gameObject, UpgradeSettings.MISSILE_FUEL_AMOUNT);
    }

    void FixedUpdate(){
        turnTowardsTarget();
        thrustForward();
    }

    private void OnDestroy() {
        if (diedByFuelExpiration) {
            explode();
        }
    }

    public void setTarget(GameObject target) {
        this.target = target;
    }

    /// <summary>
    /// Turn to the face the target
    /// TODO angular velocity instead?
    /// </summary>
    void turnTowardsTarget() {
        if (target == null) {
            return;
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(target.transform.position - transform.position),
            UpgradeSettings.PLAYER_MISSILE_ROTATION_SPEED * Time.deltaTime);
    }

    /// <summary>
    /// Fly forward, with the same force regardless of facing the target or not
    /// </summary>
    void thrustForward() {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * UpgradeSettings.PLAYER_MISSILE_THRUST_FORCE);
    }

    /// <summary>
    /// Creates explosion and animation objects.
    /// See <see cref="UpgradeSettings"/> for max missile damage.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {
        //ignore collisions with missile defense systems //TODO make a tag instead? is layer performance better?
        if(other.gameObject.layer == MISSILE_DEFENSE_LAYER) {
            return;
        }
        diedByFuelExpiration = false;
        explode();
        Destroy(gameObject);
    }

    private void explode() {
        Explosion explosion = Instantiate(explosionDamage).GetComponent<Explosion>();
        if (explosion != null) {
            explosion.detonate(1 << LayerMask.NameToLayer("Ship Parts"), blastRadius, transform.position, UpgradeSettings.MISSILE_DAMAGE);
        }
        GameObject animation = Instantiate(explosionAnimation, transform.position, transform.rotation);
        Destroy(animation, explosionAnimationDuration);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
