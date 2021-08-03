/*
 * The reactor generates energy used to regenerate shields, fire weapons, burn thrusters, and more.
 * Script controls reactor energy storage and energy regeneration, energy prioritization and turning on/off systems
 * 
 * 01.06.2021 v1.2a
 * Added support for Missile Defense as another subsystem
 * 
 * 07.04.2020 v1.2
 * Integrated with other components (power draining/management functionality)
 * Ability to turn off subsystems
 * 
 * 06.28.20 v1.1
 * System typing added
 * 
 * 06.27.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.27.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorManager : MonoBehaviour{

    [Tooltip("Maximum energy capacity of the reactor's batteries")]
    public int maxEnergy;
    [Tooltip("Current power stored")]
    public int currentEnergy;
    [Tooltip("Power regenerated per second")]
    public int regenPerSecond;

    /// <summary>
    /// Reactors allocate power based on system type. Sometimes certain systems get priority over others
    /// </summary>
    public enum SystemType {
        Weapons,
        Thrusters,
        Shields,
        MissileDefense,
        None
    }

    [Tooltip("The priority system is permitted to draw power beyond the reserve level")]
    public SystemType priority;

    [Tooltip("This amount of power is held in reserve for the priority system. If no priority system is set, reserve is ignored.")]
    public int reserve;

    [Tooltip("Allow weapons to draw power")]
    public bool allowWeapons;
    [Tooltip("Allow thrusters to draw power")]
    public bool allowThrusters;
    [Tooltip("Allow shields to draw power")]
    public bool allowShields;
    [Tooltip("Allow missile defense to draw power")]
    public bool allowMissileDefense;

    //How often the shield regenerates power
    private static float REGEN_TIME_INTERVAL = 1f;

    void Start() {
        StartCoroutine("regenEnergy");
    }


    void Update() {

    }

    /// <summary>
    /// Returns true if the requested subsystem is allowed to draw power from this reactor
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool checkAllowed(SystemType type) {
        if(type == SystemType.Weapons) {
            if (allowWeapons) {
                return true;
            } else {
                return false;
            }
        } else if (type == SystemType.Shields) {
            if (allowShields) {
                return true;
            } else {
                return false;
            }
        } else if (type == SystemType.Thrusters) {
            if (allowThrusters) {
                return true;
            } else {
                return false;
            }
        } else if (type == SystemType.MissileDefense) {
            if (allowMissileDefense) {
                return true;
            } else {
                return false;
            }
        } else {
            return true;
        }
    }

    /// <summary>
    /// Returns true if the reactor is prioritizing the given system, otherwise false
    /// </summary>
    /// <param name="rp"></param>
    /// <returns></returns>
    public bool checkPriority(SystemType type) {
        if (priority == type) {
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Drains the reactor of some amount of energy, returns true if successful.
    /// May return false because the reactor doesn't have enough power, the system is turned off, or reserve power is being held for another system
    /// </summary>
    /// <param name="toDrain"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool drainEnergy(int toDrain, SystemType type) {
        if (toDrain <= 0) { //zero cost or negative cost drains (adding power) always go through, though negatives are not the intended use of this method
            currentEnergy = currentEnergy - toDrain;
            return true;
        }

        //Escape if requested subsystem is disabled
        if (!checkAllowed(type)) {
            return false;
        }

        if (priority == SystemType.None && currentEnergy - toDrain > 0) {       //No priority, no reserve held
            currentEnergy = currentEnergy - toDrain;
            return true;
        } else if (priority == type && currentEnergy - toDrain > 0) {           //The part is priority, allowed to drain reserve
            currentEnergy = currentEnergy - toDrain;
            return true;
        } else if (priority != type && currentEnergy - toDrain > reserve) {     //The part is not priority, allowed to drain up to reserve
            currentEnergy = currentEnergy - toDrain;
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Test if the reactor is able to distribute some amount of energy *without* actually draining it.
    /// Ignores allowed/disallowed systems, but maintains priority systems and reserve energy.
    /// </summary>
    /// <param name="toDrain"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool testEnergy(int toDrain, SystemType type) {
        if (priority == SystemType.None && currentEnergy - toDrain > 0) {       //No priority, no reserve held
            return true;
        } else if (priority == type && currentEnergy - toDrain > 0) {           //The part is priority, allowed to drain reserve
            return true;
        } else if (priority != type && currentEnergy - toDrain > reserve) {     //The part is not priority, allowed to drain up to reserve
            return true;
        } else {
            return false;
        }
    }


    /// <summary>
    /// Regenerates energy every second equal to the regenPerSecond rate of this reactor
    /// </summary>
    /// <returns></returns>
    IEnumerator regenEnergy() {
        while (true) {
            yield return new WaitForSeconds(REGEN_TIME_INTERVAL);
            if (currentEnergy + regenPerSecond < maxEnergy) {
                currentEnergy += regenPerSecond;
            } else {
                currentEnergy = maxEnergy;
            }
        }
    }
}
