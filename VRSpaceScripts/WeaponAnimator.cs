/*
 * Generates muzzle flares and weapon projectiles, and cleans them up after some time.
 * 
 * 01.22.21 v1.2
 * Supports Missiles
 * 
 * 06.19.20 v1.1
 * Supports lasers
 * 
 * 06.18.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.18.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    [Tooltip("Prefab for the projectile this weapon fires")]
    public GameObject projectile;
    [Tooltip("Prefab for this weapon's muzzle flare")]
    public GameObject muzzleFlare;

    [Tooltip("Duration a muzzle flare appears for")]
    public float muzzleFlareLifeTime;


    void Start(){
        
    }

    void Update(){
        
    }

    /// <summary>
    /// Animates a muzzle flare at the given position (typically the shot origin)
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void flareMuzzle(Vector3 position, Quaternion rotation) {
        GameObject flare = GameObject.Instantiate(muzzleFlare, position, rotation);
        Destroy(flare, muzzleFlareLifeTime);
    }

    /// <summary>
    /// Generates a weapon projectile at the location and trigger's that projectile to start moving towards its target
    /// </summary>
    /// <param name="shotOrigin"></param>
    /// <param name="target"></param>
    public void createProjectile(GameObject shotOrigin, GameObject target) {
        Vector3 position = shotOrigin.transform.position;
        Quaternion rotation = shotOrigin.transform.rotation;
        GameObject shot = GameObject.Instantiate(projectile, position, rotation);
 //       Collider collider = shot.GetComponent<Collider>();
 //       Collider ownerCollider = gameObject.transform.root.parent.GetComponent<Collider>();
 //       Physics.IgnoreCollision(collider, ownerCollider);

        //TODO classify movers with the same base class to avoid searching for a mover twice
        ProjectileMoveToTarget mover = shot.GetComponent<ProjectileMoveToTarget>();
        if (mover != null) {
            //TODO compute projectile lifetime dynamically based on distance or something...
            mover.startMoving(target, 5f);
            mover.owner = shotOrigin;
            return;
        }
        MissileMover missileMover = shot.GetComponent<MissileMover>();
        if (missileMover != null) {
            missileMover.setTarget(target);
            missileMover.ownerType = shotOrigin.transform.root.gameObject.GetComponent<UnitManager>().owner;
            //TODO don't let missiles kill their owner? Or allow it?
            //Physics.IgnoreCollision(missileMover.transform.root.gameObject.GetComponent<Collider>(), shotOrigin.transform.parent.gameObject.GetComponent<Collider>());
            return;
        } 

        if(mover==null && missileMover==null){
             //if projectile lacks a mover, instantly get rid of it (sanity check)
             Destroy(shot);
        }
    }

    }
