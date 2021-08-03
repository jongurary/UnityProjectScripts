/*
 * Defines a the health of a single part. When a part has zero health, it is destroyed, causing the corresponding system to stop working.
 * Some parts of the ship, such as the inertial thruster, are "immortal", and only die if the ship is completely destroyed.
 * Immortal parts should not have this module.
 * Parts can repaired.
 * 
 * TODO
 * Explode at a specific location
 * 
 * 07.17.20 v1.2
 * Attempted to restore damage indicator by just having a few preset materials instead of programatically generating materials
 * Added functionality to disable part when it dies, restore it when repaired
 * Added damage and death animations
 * 
 * 07.08.2020 v1.1
 * Attempted to add damage indicator that changes color in proportion to damage, however this appears to crash Unity
 * 
 * 06.16.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.16.2020
 */
using System;
using System.Collections;
using UnityEngine;

public class PartLife : MonoBehaviour {

    [Tooltip("Life manager should be attached to the root game object. All parts must be linked to the life manager to function properly")]
    public LifeManager lifeManager;

    [Tooltip("Life of this part when in perfect condition. Not all parts may start at full life.")]
    public int maxLife;
    [Tooltip("Part's current life. Lose it all, and ur ded.")]
    public int currentLife;
    [Tooltip("Life regenerated per second on this part. Typically zero unless the ship is undergoing repair.")]
    public int regenPerSecond;

    [Tooltip("True if this unit is dead.")]
    public bool isDead;

    [Tooltip("The object displayed to the preview camera to show part health")]
    public GameObject lifeIndicator;
    private Renderer lifeIndicatorRenderer;

    public MaterialsManager materialsManager;

    private static float HIGH_HEALTH = .9f;
    private static float MID_HEALTH = .5f;
    private static float LOW_HEALTH = .05f;
    private static float DEAD_HEALTH = 0f;

    private static float EMISSIONS_WHEN_LOW = .05f;
    private static float EMISSIONS_WHEN_MID = .01f;

    [Tooltip("Contains the particle system that shows damage to this part in sparks/fire/whatever")]
    public ParticleSystem damageAnimation;
    [Tooltip("How many particles come out of the damage animation when the part is completely dead")]
    public float emissionsWhenDead;
    private ParticleSystem.EmissionModule emission;

    //How often parts regenerate life
    private static float REGEN_TIME_INTERVAL = 1f;

    private Thruster thruster;
    private Weapon weapon;
    private ReactorManager reactor;

    [Tooltip("Explosion generated when part reaches zero health")]
    public GameObject deathAnimation;

    void Start() {
        if (lifeManager == null) {
            lifeManager = transform.root.GetComponent<LifeManager>();
        }
        StartCoroutine("regenLife");
        if (currentLife <= 0) {
            isDead = true;
        } else {
            isDead = false;
        }
        thruster = GetComponent<Thruster>();
        weapon = GetComponent<Weapon>();
        reactor = GetComponent<ReactorManager>();

        if (damageAnimation != null) {
            emission = damageAnimation.emission;
        }

        if (materialsManager == null) {
            materialsManager = GameObject.Find("Materials Manager").GetComponent<MaterialsManager>();
        }

        if (lifeIndicator != null) {
            lifeIndicatorRenderer = lifeIndicator.GetComponent<Renderer>();
            updateDamageIndicators();
        }
    }


    void Update() {

    }

    /// <summary>
    /// Tells the particle emitter that demonstrate part damage to emit the given number of particles per second.
    /// Generally called by updateDamageIndicators
    /// </summary>
    /// <param name="particles"></param>
    private void updateDamageAnimation(float particles) {
        if (damageAnimation != null) {
            emission.rateOverTime = particles;
        }
    }

    /// <summary>
    /// Updates visual indicators of part health, in general should be done whenever damage is taken or healed.
    /// TODO the thresholds here probably need some better consideration
    /// </summary>
    public void updateDamageIndicators() {
        if (lifeIndicatorRenderer != null) {
            if (!isDead) {
                float lifePercent = (float) currentLife / (float) maxLife;

                if (lifePercent > HIGH_HEALTH) {
                    lifeIndicatorRenderer.material = materialsManager.damageIndicatorMaterials[0];
                    updateDamageAnimation(0);
                } else if (lifePercent > MID_HEALTH) {
                    lifeIndicatorRenderer.material = materialsManager.damageIndicatorMaterials[1];
                    updateDamageAnimation(emissionsWhenDead * EMISSIONS_WHEN_MID);
                } else if (lifePercent > LOW_HEALTH) {
                    lifeIndicatorRenderer.material = materialsManager.damageIndicatorMaterials[2];
                    updateDamageAnimation(emissionsWhenDead * EMISSIONS_WHEN_LOW);
                } else if (lifePercent > DEAD_HEALTH) {
                    lifeIndicatorRenderer.material = materialsManager.damageIndicatorMaterials[3];
                    updateDamageAnimation(emissionsWhenDead);
                }
            } else {
                lifeIndicatorRenderer.material = materialsManager.damageIndicatorMaterials[3];
                updateDamageAnimation(emissionsWhenDead);
            }
        }
    }

    /// <summary>
    /// Sets the part to dead and disables the functionality of this part
    /// </summary>
    public void killPart() {
        if (thruster != null) {
            thruster.isFunctional = false;
        }

        if (weapon != null) {
            weapon.isFunctional = false;
        }

        if (reactor != null) {
            Destroy(transform.root.gameObject);
        }
    }

    /// <summary>
    /// Used when a part recovers health from zero to re-enable the part's functionality
    /// </summary>
    public void revivePart() {
        if (thruster != null) {
            thruster.isFunctional = true;
        }

        if (weapon != null) {
            weapon.isFunctional = true;
        }
    }

    /// <summary>
    /// Damages a part's life. If the part's life is reduced to zero, it is rendered non-functional until it is repaired.
    /// If the reactor core dies, the unit is destroyed.
    /// Set ignore to true if this damage should bypass the shield entirely.
    /// </summary>
    /// <param name="damage"></param><param name="ignoreShield"></param>
    public void doDamage(int damage, bool ignoreShield) {
        //if this damage ignores the shield, or the shield cannot absorb it, do true damage
        if (ignoreShield || !lifeManager.testShield( (int)(damage*lifeManager.shieldMitigationFactor) )) {
            if (currentLife - damage > 0) {
                currentLife = currentLife - damage;
            } else {
                if (!isDead && deathAnimation != null) { //trigger death animation on first time reaching 0 health
                    GameObject deathExplosion = Instantiate(deathAnimation, transform.position, transform.rotation);
                    Destroy(deathExplosion, 5f);
                }
                currentLife = 0;
                isDead = true;
                killPart();
            }
        } else { //otherwise hit the shield, then apply remaining damage to the part
            int remainingDamage = lifeManager.damageShieldWithMitigation(damage);
            if (currentLife - remainingDamage > 0) {
                currentLife = currentLife - remainingDamage;
            } else {
                if (!isDead && deathAnimation != null) { //trigger death animation on first time reaching 0 health
                    GameObject deathExplosion = Instantiate(deathAnimation, transform.position, transform.rotation);
                    Destroy(deathExplosion, 5f);
                }
                currentLife = 0;
                isDead = true;
                killPart();
            }
        }
        updateDamageIndicators();
    }


    /// <summary>
    /// Regenerates life every second equal to the regenPerSecond rate of this part
    /// </summary>
    /// <returns></returns>
    IEnumerator regenLife() {
        while (true) {
            yield return new WaitForSeconds(REGEN_TIME_INTERVAL);
            if (currentLife + regenPerSecond < maxLife) {
                currentLife += regenPerSecond;
                updateDamageIndicators();
            } else {
                currentLife = maxLife;
            }
            if (currentLife >= 1) {
                isDead = false;
                revivePart();
            }
        }
    }
}
