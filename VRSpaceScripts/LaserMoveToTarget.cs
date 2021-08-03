/*
 * Lasers inherit most of the properties of standard projectiles but travel to their target as a line
 * 
 * TODO
 * Split the "Beam" part from the physical projectile to allow faster expansion/recession that doesn't break physics clipping
 * might be more efficient to go to fixedupdate or coroutine
 * Render laser flare at strike point rather than at target (use a trigger collider?)
 * Move/kill flares in instances where the target dies/vanishes/etc
 * 
 * 07.20.2020 v1.2a
 * Added support for shields
 * 
 * 06.26.20 v1.2
 * Default behavior of lasers is now to do damage on collision, to facilitate obstruction, hitting multiple targets, etc
 *  
 * 06.25.20 v1.1
 * Flare now renders at target, began developing flare-renders-at-strike-point mechanism.
 * (REMOVED) Beam does damage instantly are fire-point
 * 
 * 06.19.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.19.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserMoveToTarget : ProjectileMoveToTarget{

    public LineRenderer line;
    [Tooltip("Lasers don't make explosions, instead, they have a flare that turns on when when they hit something. Should be a child of this prefab.")]
    public GameObject flare;

    [Range(0f, 1f)]
    [Tooltip("How fast the laser expands towards the target when deployed (lower is slower)")]
    public float expansionRate;
    private bool doneExpanding = false;
    //How long the beam stays in place
    [Range(0f, 10f)]
    [Tooltip("How long the beam stays in place (lower is shorter)")]
    public float lingerTime;
    private bool doneLingering = false;
    [Range(0f, 1f)]
    [Tooltip("How fast the laser receeds away from the origin when it ends")]
    public float recessionRate;
    private bool hasHit = false;

    [Range(0f, 1f)]
    [Tooltip("What percent of the laser's power is retained when penetrating a target")]
    public float penetrationPower;

    private float expansion;
    private float recession;

    //Animation system copied from F3DBeam
    private float animateUVTime;
    private float UVTime = -6; // UV Animation speed
    private float initialBeamOffset;

    void Start(){
        //Note: lasers don't move, the line moves
        if (moveSpeed == 0) {
            moveSpeed = 100f;
        }
        if (line == null) {
            line = GetComponentInChildren<LineRenderer>();
        }
        //Laser lines use the point system to determine start/end, so the transform itself should be at origin
        line.gameObject.transform.position = Vector3.zero;
        line.gameObject.transform.parent = null;
        line.gameObject.transform.rotation = Quaternion.identity;
        if (line!=null && owner != null) {
            line.SetPosition(0, owner.transform.position);
            line.SetPosition(1, owner.transform.position);
        }
        flare.SetActive(false);
        initialBeamOffset = Random.Range(0f, 5f);
        expansion = 0f;
        recession = 0f;
    }

    void Update(){
        animateBeam();
        if (isMoving) {
            if (target == null) {
                //lasers receed immediately if the target dies
            } else {
                //expand towards target
                if (!doneExpanding && expansion < 1f) {
                    expansion += expansionRate;
                } else {
                    expansion = 1f;
                    if (!doneExpanding) {
                        StartCoroutine("EndLaser");
                        doneExpanding = true;
                    }
                }

                //receed away from target
                if (doneLingering && expansion >= .99f) {
                    if (recession < 1f) { 
                        recession += recessionRate;
                    } else {
                        Destroy(flare);
                        recession = 1f;
                    }
                }

                Vector3 startPoint = Vector3.Lerp(owner.transform.position, target.transform.position, recession);
                Vector3 endPoint = Vector3.Lerp(owner.transform.position, target.transform.position, expansion);
                transform.position = endPoint;
                line.SetPosition(0, startPoint);
                line.SetPosition(1, endPoint);
            }
        }
    }

    /// <summary>
    /// Used when the beam first strikes the target, places the beam target flare
    /// </summary>
    /// <param name="location"></param>
    private void placeFlare(Vector3 location) {
        flare.transform.parent = null;
        flare.SetActive(true);
        flare.transform.position = location;

    }

    private void animateBeam() {
        animateUVTime += Time.deltaTime;

        if (animateUVTime > 1.0f) {
            animateUVTime = 0f;
        }

        line.material.SetTextureOffset("_MainTex", new Vector2(animateUVTime * UVTime + initialBeamOffset, 0f));
    }

    private void OnDestroy() {
        Destroy(line.gameObject);
        Destroy(flare.gameObject);
    }

    /// <summary>
    /// Damage hit targets, apply flare when first striking the target
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {
        //ignore collisions with unit that fired this weapon
        if (other.transform.root != owner.transform.root) {

            if (damage > 0) {
                PartLife partLife = other.gameObject.GetComponent<PartLife>();
                if (partLife != null) {
                    //Only places the flare animation when striking the FIRST part
                    if (!hasHit) {
                        hasHit = true;
                        //place flare animation at the first hit point only
                        //placeFlare(other.ClosestPointOnBounds(transform.position));
                        placeFlare(transform.position);
                    }

                    partLife.doDamage(damage, ignoreShield);
                    //reduces the laser's damage for subsequent hits
                    damage = (int) (damage * penetrationPower);
                }
            }

            FXVShield_Redux shield = other.GetComponent<FXVShield_Redux>();
            if (shield != null) {
                shield.OnHit(transform.position, shieldImpactStrength, shieldImpactTime);
            }

        }
    }

    IEnumerator EndLaser() {
        yield return new WaitForSeconds(lingerTime);
        doneLingering = true;
    }
 
}
