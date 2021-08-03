/* 
 * Animates and controls the behavior of the missile defense system, which launches a laser (a line renderer)
 * between the ship's core and any inbound missiles, if the reactor permits it when the missile first enters. A single missile
 * defense can handle only one missile at a time.
 * 
 * TODO
 * enum for different "kill speed" types corresponding on upgradesettings
 * 
 *
 * 01.22.21 v1.1
 * Takes an array of other MissileDefenseSystem for cooperation purposes (to not shoot the same target twice)
 *
 * 01.06.2020 v1.0
 * initial commit
 *
 * @author v1 Jonathan Gurary 01.06.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileDefenseSystem : MonoBehaviour{

    public ReactorManager reactor;
    [Tooltip("The defense system only reacts to foreign missiles, so it needs a link to this unit's ownership state.")]
    public UnitManager unitManager;
    private UnitManager.Ownership ownerType;
    public LineRenderer line;

    public MissileDefenseSystem[] cooperators;

    /// <summary>
    /// Is defense system actively trained on a missile?
    /// </summary>
    private bool hasLock = false;
    private GameObject lockedTarget;

    //Animation system copied from F3DBeam
    private float animateUVTime;
    private float UVTime = -6; // UV Animation speed
    private float initialBeamOffset;

    void Start(){
        //Finding the reactor requires an expensive child search, so preferably don't be lazy and remember to link it.
        if (reactor == null) {
            reactor = transform.root.GetComponentInChildren<ReactorManager>();
        }

        if(unitManager == null) {
            unitManager = transform.root.GetComponentInChildren<UnitManager>();
        }
        ownerType = unitManager.owner;

        //Lines use the point system to determine start/end, so the transform itself should be at origin
        line.gameObject.transform.position = Vector3.zero;
        line.gameObject.transform.parent = null;
        line.gameObject.transform.rotation = Quaternion.identity;
        line.gameObject.SetActive(false);
    }

    void Update(){
        animateBeam();
        if (hasLock) {
            if (lockedTarget != null) {
                line.SetPosition(1, lockedTarget.transform.position);
            } else { //theoretically unreachable...
                hasLock = false;
                line.gameObject.SetActive(false);
            }
        }
    }

    private void animateBeam() {
        animateUVTime += Time.deltaTime;

        if (animateUVTime > 1.0f) {
            animateUVTime = 0f;
        }

        line.material.SetTextureOffset("_MainTex", new Vector2(animateUVTime * UVTime + initialBeamOffset, 0f));
    }

    private void OnTriggerEnter(Collider other) {
        //First, "fail" conditions are handing, in descending order of likeliness.
        //each missile defense system can only handle a single inbound missile at a time, do nothing if already busy
        if (hasLock) {
            return;
        }

        //if insufficient power to launch defense, do nothing
        if(!reactor.testEnergy(UpgradeSettings.MISSILE_DEFENSE_ENERGY_COST, ReactorManager.SystemType.MissileDefense)) {
            return;
        }

        //Ignore non-missile collisions (should be impossible anyways)
        if (!other.CompareTag("Missile")) {
            return;
        }

        //if a cooperator already has this missile locked
        for(int i=0; i<cooperators.Length; i++) {
            if(cooperators[i].lockedTarget == other.gameObject) {
                return;
            }
        }

        MissileMover controls = other.gameObject.GetComponent<MissileMover>();
        if (controls != null) {
            UnitManager.Ownership missileOwnerType = controls.ownerType;
            if(ownerType != missileOwnerType) {
                reactor.drainEnergy(UpgradeSettings.MISSILE_DEFENSE_ENERGY_COST, ReactorManager.SystemType.MissileDefense);
                line.gameObject.SetActive(true);
                line.SetPosition(0, transform.position);
                line.SetPosition(1, other.transform.position);
                hasLock = true;
                lockedTarget = other.gameObject;
                StartCoroutine("destroyMissile");
            }
        }
        
    }

    IEnumerator destroyMissile() {
        yield return new WaitForSeconds(Random.Range(0, UpgradeSettings.MISSILE_DEFENSE_TIME_TO_KILL));
        if (lockedTarget != null) {
            Destroy(lockedTarget);
        }
        hasLock = false;
        line.gameObject.SetActive(false);
        yield break;
    }
}
