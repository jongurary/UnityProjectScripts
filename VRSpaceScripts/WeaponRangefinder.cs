/*
 * Controls target acquisition for a link weapon module.
 * Adds and removes targets to the Weapon modules list of targets as they enter and exit the rangefinder
 * Note that dead targets are purged by the Weapon module.
 * 
 * TODO
 * Some weapons may only target "friendly" units, for example a healing beam. Consider a simple patch to implement this.
 * Part targetting priorities
 * 
 * 07.08.2020 v1.1a
 * Bug where enemies could not target allies fixed (conditional issue in Start method)
 * 
 * 07.05.20 v1.1
 * Now connects to UnitManager to find ownership and only adds targets that are on the opposing side
 * CheckOwnership method to determine if a target is viable
 * Player/Ally can target Enemy/Nuetral
 * Enemies target Player/Ally/Nuetral
 * Nuetral tragets all
 * 
 * 06.16.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.16.2020
 */

using UnityEngine;

public class WeaponRangefinder : MonoBehaviour{

    [Tooltip("The Weapon control script that this rangefinder services")]
    public Weapon weapon;

    [Tooltip("Typically on the root object, used to determine ownership")]
    public UnitManager unitManager;
    private UnitManager.Ownership ownership;

    void Start(){
        if (unitManager == null) {
            unitManager = transform.root.GetComponent<UnitManager>();
        }
        if (unitManager != null) {
            ownership = unitManager.owner;
        } else { //defaults to neutral if no owner is found
            ownership = UnitManager.Ownership.Nuetral;
        }
    }

    void Update(){
        
    }

    /// <summary>
    /// Returns true if the unit that owns this weapon can target the unit in question
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool isViableTarget(GameObject target) {
        UnitManager.Ownership targetOwnership;
        UnitManager targetUnitManager = target.GetComponent<UnitManager>();
        if (targetUnitManager != null) {
            targetOwnership = targetUnitManager.owner;
        } else { //defaults to neutral if no owner is found
            targetOwnership = UnitManager.Ownership.Nuetral;
        }

        if (ownership == UnitManager.Ownership.Ally) {
            switch (targetOwnership) {
                case UnitManager.Ownership.Ally:
                    return false;
                case UnitManager.Ownership.Enemy:
                    return true;
                case UnitManager.Ownership.Nuetral:
                    return true;
                case UnitManager.Ownership.Player:
                    return false;
                default:
                    return true;
            }
        }

        if (ownership == UnitManager.Ownership.Enemy) {
            switch (targetOwnership) {
                case UnitManager.Ownership.Ally:
                    return true;
                case UnitManager.Ownership.Enemy:
                    return false;
                case UnitManager.Ownership.Nuetral:
                    return true;
                case UnitManager.Ownership.Player:
                    return true;
                default:
                    return true;
            }
        }

        //TODO neutral units are expected to be missiles and such, but if applies to something like "pirates" will need a new category
        //otherwise neutrals will shoot each other.
        if (ownership == UnitManager.Ownership.Nuetral) {
            switch (targetOwnership) {
                case UnitManager.Ownership.Ally:
                    return true;
                case UnitManager.Ownership.Enemy:
                    return true;
                case UnitManager.Ownership.Nuetral:
                    return true;
                case UnitManager.Ownership.Player:
                    return true;
                default:
                    return true;
            }
        }

        if (ownership == UnitManager.Ownership.Player) {
            switch (targetOwnership) {
                case UnitManager.Ownership.Ally:
                    return false;
                case UnitManager.Ownership.Enemy:
                    return true;
                case UnitManager.Ownership.Nuetral:
                    return true;
                case UnitManager.Ownership.Player:
                    return false;
                default:
                    return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other) {
        GameObject hit = other.transform.root.gameObject;
        if (isViableTarget(hit)) {
            weapon.addTarget(hit);
        }
    }

    private void OnTriggerExit(Collider other) {
        GameObject hit = other.transform.root.gameObject;
        if (isViableTarget(hit)) {
            weapon.removeTarget(other.transform.root.gameObject);
        }
    }
}
