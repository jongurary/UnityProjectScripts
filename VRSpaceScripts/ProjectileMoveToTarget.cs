/*
 * Controls a projectile to move to a target when it's born and kill itself after some time
 * Also handles collision with objects, triggering damage and shield effects.
 * Extended by LaserMoveToTarget for weapons that use a linerenderer as the main display element
 * 
 * TODO
 * might be more efficient to go to fixedupdate or coroutine
 * 
 * 01.23.2021 v1.2b
 * Handles a null error when owner of projectile dies
 * Now flies into the ether when target dies instead of freezing up
 * 
 * 07.20.2020 v1.1a
 * Added support for shields
 * 
 * 06.19.20 v1.1
 * Now does damage at hit-time, allowing weapons to do zero damage upfront and instead do damage on hit
 * Supports laser weapons via extension (LaserMoveToTarget class)
 * 
 * 06.18.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.18.2020
 */
using UnityEngine;

public class ProjectileMoveToTarget : MonoBehaviour{

    [Tooltip("Where is this projectile headed? Set by the WeaponAnimator module")]
    public GameObject target;
    [Tooltip("Who fired this projectile? Also set by WeaponAnimator. Should be the actual shot origin for animation purposes.")]
    public GameObject owner;
    public int damage;
    [Tooltip("Does this weapon's damage ignore shield?")]
    public bool ignoreShield;
    public float moveSpeed;

    protected bool isMoving = false;

    [Tooltip("Explosion spawned when this projectile strikes the target")]
    public GameObject explosionPrefab;
    public float explosionTimeToLive;

    [Tooltip("How long does this projectile leave a mark on the target's shield? (in seconds)")]
    public float shieldImpactTime;
    [Tooltip("How large is this projectile's impact on the shield? (10 is a small hit, 100 is roughly the whole shield)")]
    public float shieldImpactStrength;

    void Start(){
        //placeholder moveSpeed so projectiles don't have a zero-speed
        if (moveSpeed == 0) {
            moveSpeed = .5f;
        }
    }

    void Update(){
        if (isMoving) {
            if (target == null) {
                //just fly forward, die eventually due to lifetime expiring...
                transform.position = transform.position + Vector3.right * Time.deltaTime * moveSpeed;
            } else {
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * moveSpeed);
                transform.rotation = Quaternion.LookRotation((transform.position - target.transform.position).normalized);
            }
        }
    }

    /// <summary>
    /// Die when striking the destination target, do damage if applicable
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {

        if(other.transform == null || owner == null || other.transform.root == null) {
            return;
        }

        //don't collide with self
        if (other.transform.root != owner.transform.root) {
            //TODO make sure collisions with MainCollider don't mess with this...

            //Only register hits against targets that have a life component
            PartLife partLife = other.gameObject.GetComponent<PartLife>();
            if (partLife != null) {
                //make explosion animation
                GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);

                partLife.doDamage(damage, ignoreShield);

                //cleans up self and explosion
                Destroy(explosion, explosionTimeToLive);
                Destroy(gameObject);
            }

            FXVShield_Redux shield = other.GetComponent<FXVShield_Redux>();
            if (shield != null) {
                shield.OnHit(transform.position, shieldImpactStrength, shieldImpactTime);
            }
        }
    }

    /// <summary>
    /// Begin moving towards a target, and set to die after the time to live is expired
    /// </summary>
    /// <param name="target"></param>
    /// <param name="timeToLive"></param>
    public void startMoving(GameObject target, float timeToLive) {
        Destroy(gameObject, timeToLive);
        this.target = target;
        isMoving = true;
    }
}
