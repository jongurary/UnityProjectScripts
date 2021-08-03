/* 
 * Holds constants that may, in future versions, be upgradable somehow
 * 
 * 01.05.2021 v1.0a
 * Added missile fuel amount
 * Added missile damage
 *
 * 09.18.2020 v1.0
 * initial commit
 *
 * @author v1 Jonathan Gurary 09.18.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UpgradeSettings{
    public static float PLAYER_MISSILE_ROTATION_SPEED = 300f;
    public static float PLAYER_MISSILE_THRUST_FORCE = 30000f;
    /// <summary>
    /// Time until a missile runs out of fuel and destroys itself, in seconds
    /// </summary>
    public static float MISSILE_FUEL_AMOUNT = 60f;
    public static int MISSILE_DAMAGE = 1000;
    /// <summary>
    /// Max time for a missile defense beam to destroy a missile
    /// </summary>
    public static float MISSILE_DEFENSE_TIME_TO_KILL = 1.5f;
    /// <summary>
    /// Energy required to destroy an inbound missile
    /// </summary>
    public static int MISSILE_DEFENSE_ENERGY_COST = 100;
}
