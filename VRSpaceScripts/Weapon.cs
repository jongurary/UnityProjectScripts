/*
 * Controls all aspects of weapon firing and target management.
 * 
 * TODO
 * allow targeting of a specific part
 * priority targetting of a specific unit
 * Targetting types besides Furthest/Closest are not finished yet
 * 
 * 07.17.2020 v1.3a
 * Fixed bug related to min/max distance for Furthest/Closest targeting
 * Added isFunctional and isPaused to allow weapon to be disabled by other scripts
 * 
 * 07.05.20 v1.3
 * Weapons now consume reactor energy
 * Ownership is now checked in WeaponRangefinder, so it can be assumed that all targets in the array are viable targets
 * 
 * 06.18.20 v1.2
 * Supports lasers
 * Supports instant damage and damage by projectile collision (see the projectile prefab for that damage)
 * 
 * 06.18.20 v1.1
 * Target selection, target management
 * 
 * 06.16.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.16.2020
 */
using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour{

    [Tooltip("Interval between shots, in seconds")]
    public float cooldown;
    [Tooltip("Energy cost to fire this weapon. Weapons cannot fire if the reactor doesn't have enough energy.")]
    public int energyCost;
    [Tooltip("Damage done instantly when this weapon is fired. For delayed damage, set on the projectile itself instead")] //TODO more nuance in damage to shield/armor etc
    public int instantDamage;

    //TODO this should be some kind of tree that's sorted by the targetting criteria. Hurray data structures...
    [Tooltip("The rangefinder populates this list of potential targets in range.")]
    public GameObject[] targets = new GameObject[15];
    //index of the last target in the targets list
    private int lastTarget = 0;

    /// <summary>
    /// The target currently being attacked
    /// </summary>
    public GameObject target;
    public enum TargetingType {
        Closest,
        Furthest,
        Strongest,
        Weakest,
        Smallest,
        Largest
    }
    [Tooltip("How does this determine how it will choose a target from the targets pool?")]
    public TargetingType targetingType;

    //how often to scan for dead targets in the targetting pool
    //TODO remove if still unused
    private static float PURGE_TARGETS_INTERVAL=1f;
    //how often to rebuild the array to purge dead targets
    private static float RESTRUCTURE_INTERVAL = 5f;

    [Tooltip("Shot animations should emerge from here")]
    public GameObject shotOrigin;
    [Tooltip("This script is responsible for creating actual projectiles, muzzle flares, etc")]
    public WeaponAnimator weaponAnimator;
    public ReactorManager reactor;

    [Tooltip("Set to true automatically at start, set to false when this part is dead")]
    public bool isFunctional;
    [Tooltip("Set to false automatically at start, set to true by certain other scripts")]
    public bool isPaused;

    void Start(){
        //Finding the reactor requires an expensive child search, so preferably don't be lazy and remember to link it.
        if (reactor == null) {
            reactor = transform.root.GetComponentInChildren<ReactorManager>();
        }

        isFunctional = true;
        isPaused = false;

        StartCoroutine("restructureArray");
        StartCoroutine("fireWeapon");
    }

    void Update(){
        //if we have no target, and there is at least one in the targets list, pick a new target asap
        if (target == null && lastTarget > 0) {
            selectTarget();
        }
    }

    /// <summary>
    /// Picks a target according to this unit's targetting preferences
    /// </summary>
    public void selectTarget() {
        GameObject newTarget = null;
        switch (targetingType) {
            case TargetingType.Closest:
                float minDist = 1000000;
                float dist;
                for (int i = 0; i < targets.Length; i++) {
                    if (targets[i] != null) {
                        dist = Vector3.Distance(shotOrigin.transform.position, targets[i].transform.position);
                        if (dist < minDist) {
                            minDist = dist;
                            newTarget = targets[i];
                        }
                    }
                }
                break;
            case TargetingType.Furthest:
                float maxDist = 0;
                for (int i = 0; i < targets.Length; i++) {
                    if (targets[i] != null) {
                        dist = Vector3.Distance(shotOrigin.transform.position, targets[i].transform.position);
                        if (dist > maxDist) {
                            maxDist = dist;
                            newTarget = targets[i];
                        }
                    }
                }
                break;
            //TODO other kinds of targeting..
            default:
                break;
        }

        target = newTarget;
    }

    /// <summary>
    /// Adds a target to the end of the targets array, if there is room
    /// </summary>
    /// <param name="target"></param>
    public void addTarget(GameObject target) {
        if(lastTarget >= targets.Length - 1) {
            return;
        }

        targets[lastTarget] = target;
        lastTarget++;
        
    }

    /// <summary>
    /// Purges a target from the list by index.
    /// Order of remaining targets is *not* preserved
    /// </summary>
    /// <param name="index"></param>
    public void removeTarget(int index) {
        if (index > targets.Length || index < 0) {
            return;
        }

        //if this unit is the current target, also purge it as the current target
        if (targets[index] == target) {
            target = null;
        }

        if (lastTarget > 0 && targets[index] != null) {
            targets[index] = targets[lastTarget-1];
            targets[lastTarget-1] = null;
            lastTarget--;
        }
    }

    /// <summary>
    /// Purges a target from the list by object reference.
    /// Order of remaining targets is *not* preserved
    /// Substantially slower than by-index removal due to search requirements.
    /// </summary>
    /// <param name="index"></param>
    public void removeTarget(GameObject toRemove) {
        int index = -1;
        for (int i = 0; i < targets.Length; i++) {
            if (targets[i] != null) {
                if (targets[i] == toRemove) {
                    index = i;
                }
            }
        }

        //if search failed, escapes lazily because index < 0
        if (index > targets.Length || index < 0) {
            return;
        }

        //if this unit is the current target, also purge it as the current target
        if (toRemove == target) {
            target = null;
        }

        if (lastTarget > 0 && targets[index] != null) {
            targets[index] = targets[lastTarget - 1];
            targets[lastTarget - 1] = null;
            lastTarget--;
        }
    }

    /// <summary>
    /// Fires the weapon perdiodically. See globals to change fire interval and other stats
    /// </summary>
    /// <returns></returns>
    IEnumerator fireWeapon() {
        //TODO raycast check that the target is unobstrucuted, otherwise pick a new target
        while (true) {
            yield return new WaitForSeconds(cooldown);
            if (target != null && isFunctional && !isPaused) {
                if (reactor.drainEnergy(energyCost, ReactorManager.SystemType.Weapons)) {
                    //float dist = Vector3.Distance(shotOrigin.transform.position, target.transform.position);
                    //TODO compute the weapon travel time and set projectile lifetime and such accordingly
                    weaponAnimator.flareMuzzle(shotOrigin.transform.position, shotOrigin.transform.rotation);
                    
                    GameObject partToDamage;
                    if (instantDamage > 0) { //direct-damage weapons
                        partToDamage = target.GetComponent<LifeManager>().damageRandomPart(instantDamage);
                    } else { //a weapon where the projectile does damage on hit
                        partToDamage = target.GetComponent<LifeManager>().pickRandomPart();
                    }
                    //projectile flies at the specific part we want to damage
                    weaponAnimator.createProjectile(shotOrigin, partToDamage);
                }
            }
        }
    }

    /// <summary>
    /// Because spots are nulled as units die, the array must occasionally be recompressed to get rid of null spots
    /// </summary>
    IEnumerator restructureArray() {
        while (true) {
            yield return new WaitForSeconds(RESTRUCTURE_INTERVAL);
            GameObject[] rebuild = new GameObject[targets.Length];
            int k = 0;
            for (int i = 0; i < targets.Length; i++) {
                if (targets[i] != null) {
                    rebuild[k] = targets[i];
                    k++;
                }
            }
            targets = rebuild;
            lastTarget = k;
        }
    }

    //Alternatively, something like this may be used to purge dead targets if they aren't automatically resolved when destroyed...
    IEnumerator purgeDeadTargets() {
        while (true) {
            yield return new WaitForSeconds(PURGE_TARGETS_INTERVAL);
            for(int i=0; i<targets.Length; i++) {
                if (targets[i] != null) {
                    //TODO if is dead, remove...
                }
            }
        }
    }
}
