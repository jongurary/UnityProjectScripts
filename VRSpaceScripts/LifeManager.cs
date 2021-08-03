/*
 * A central location for all the life of all parts of the ship to be tracked.
 * Also controls shield life, which is shared ship-wide.
 * 
 * TODO
 * Pass damage from dead parts to reactor core
 * 
 * BUGS 
 * pickRandomPart can return null, but this is potentially not handled elsewhere
 * 
 * 01.23.2020 v1.2a
 * Fixed a bug where occasionaly pickRandomPart would encounter a nullRef.
 * 
 * 07.21.2020 v1.2
 * Linked to shield visuals
 * Shield mitigation now degrades as it takes damage
 * 
 * 07.17.2020 v1.1a
 * Reactor's Partlife script now kills this unit and causes the reactor to play the ship's explosion animation when dead.
 * No changes made to this script, but this fixes a long-standing TODO for this script
 * 
 * 07.05.20 v1.1
 * Shields now consume reactor power
 * 
 * 06.16.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.16.2020
 */
using System.Collections;
using UnityEngine;

public class LifeManager : MonoBehaviour{

    [Tooltip("All parts attached to the ship (which have life) should be included here. The reactor core should always be index zero")]
    public PartLife[] parts;

    [Tooltip("Life of this ship's shield at full capacity")]
    public int maxShield;
    [Tooltip("Life of the ship's shield, which is shared across all parts")]
    public int shieldLife;
    [Tooltip("Shield regenerated per second on this part, if there is enough power.")]
    public int regenPerSecond;
    [Tooltip("Energy required to regenerate the shield one tick (to apply one regenPerSecond)")]
    public int energyCostPerRegen;

    [Range(0f, 1f)]
    [Tooltip("What percentage of a shot is absorbed by the shield (this value will be used when the shield is at max capacity)")]
    public float shieldMitigationFactor;
    private float shieldMitigationFactorMax;

    public ReactorManager reactor;

    [Tooltip("The shield management script")]
    public FXVShield_Redux shield;

    //How often the shield regenerates power
    private static float REGEN_TIME_INTERVAL = 1f;

    void Start(){
        //Finding the reactor requires an expensive child search, so preferably don't be lazy and remember to link it.
        if (reactor == null) {
            reactor = transform.root.GetComponentInChildren<ReactorManager>();
        }

        if (shield == null) {
            shield = transform.root.GetComponentInChildren<FXVShield_Redux>();
        }
        shieldMitigationFactorMax = shieldMitigationFactor;

        StartCoroutine("regenShield");
    }

    
    void Update(){
        
    }

    /// <summary>
    /// Picks a part at random
    /// </summary>
    /// <returns></returns>
    public GameObject pickRandomPart() {
        PartLife pick = parts[Random.Range(0, parts.Length)];
        if (pick == null) {
            return null;
        }
        return pick.gameObject;
    }

    /// <summary>
    /// Pick a part at random and strike it for some damage
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>Attached gameobject</returns>
    public GameObject damageRandomPart(int damage) {
        PartLife pick = parts[Random.Range(0, parts.Length)];
        pick.doDamage(damage, false);
        return pick.gameObject;
    }

    /// <summary>
    /// Damages the shield, accounting for shield mitigation, and returns leftover damage that was not absorbed. If the shield
    /// cannot absorb this shot (for example if it has failed), the original damage amount will be returned.
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>Damage that was not absorbed</returns>
    public int damageShieldWithMitigation(int damage) {
        int toShield = (int) (damage * shieldMitigationFactor);
        int leftover = (int)(damage * (1 - shieldMitigationFactor));
        if (shieldLife - toShield > 0) {
            shieldLife = shieldLife - toShield;
            return leftover;
        } else {
            return damage;
        }
    }

    /// <summary>
    /// Damages the shield for some amount of true damage, if it is online
    /// Returns true if the damage was absorbed, false if the shield has failed and can't handle it
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public bool damageShield(int damage) {
        if(shieldLife - damage > 0) {
            shieldLife = shieldLife - damage;
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Test if the shield is able to handle some amount of damage *without* actually applying it
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public bool testShield(int damage) {
        if (shieldLife - damage > 0) {
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Any time the shield is damaged, this method should be invoked to recompute the shield's new mitigation rate
    /// As the shield takes more damage, mitigation is reduced to eventual zero, and heals as the shield regenerates
    /// </summary>
    private void recomputeMitigation() {
        if (shield == null) {
            return;
        }
        shieldMitigationFactor = shieldMitigationFactorMax * (float) shieldLife / (float) maxShield;
        shield.hitColor = new Color(1f - (shieldMitigationFactor/ shieldMitigationFactorMax), (shieldMitigationFactor / shieldMitigationFactorMax), 0f);
    }

    /// <summary>
    /// Regenerates shield every second equal to the regenPerSecond rate of this part
    /// </summary>
    /// <returns></returns>
    IEnumerator regenShield() {
        if (shield == null) {
            yield break;
        }
        while (true) {
            yield return new WaitForSeconds(REGEN_TIME_INTERVAL);
            recomputeMitigation();
            if (shieldLife + regenPerSecond < maxShield) {
                if (reactor.drainEnergy(energyCostPerRegen, ReactorManager.SystemType.Shields)) {
                    shieldLife += regenPerSecond;
                }
            } else {
                shieldLife = maxShield;
            }
        }
        
    }
}
