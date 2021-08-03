/*
 * Manages casting skills, putting them on/off cooldown, etc.
 * Also manages the skill icon, animations related to the icon such as cooldown, etc
 * 
 * TODO Migrate skill properties to their own class for easier game balancing down the road
 * Thruster override should do something better than just saving the old drag and restoring it
 * 
 * NOTE: This object must never be set to inactive, or all skill actions/cooldowns may pause
 * 
 * 
 * 07.29.2020 v1.1
 * Ability to hide/show the entire icon instead of disabling it (remember, object should never be inactive)
 * Skills now actually do something: (emergency repair, dark matter, and thruster overheat are added)
 * 
 * 07.23.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 07.23.2020
 */
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillCasting : MonoBehaviour{

    /// <summary>
    /// Always the root gameObject that owns this skill. Skills will attempt to find components in this unit.
    /// Should be set by the skillsManager before the icon is re-parented to the GUI
    /// </summary>
    public GameObject owner;

    public Image skillImage;
    public Image outline;
    public Image cooldown;

    public float cooldownTime;
    private float cooldownRemaining;

    public float duration;
    private float durationRemaining;

    private bool isReady;

    public Color outlineWhenReady;
    public Color outlineWhenOnCooldown;
    public Color outlineWhenActive;

    //Determines how often the coroutine that subtracts cooldown time runs. Lower = more smooth cooldown animation. Higher = efficiency
    private static float COOLDOWN_INTERVAL =.1f;

    private static int EMERGENCY_REPAIR_REGEN = 100;

    private static int DARK_MATTER_REACTOR_DAMAGE = 5000;
    private static float DARK_MATTER_REACTOR_BOOST = 2f;

    public enum Skill {
        EmergencyRepair,
        DarkMatter,
        ThrusterOverheat,
    }
    [Tooltip("Effect that occurs when the skill is cast, also controls which skill description will show")]
    public Skill skill;

    void Start(){
        isReady = true;
        cooldownRemaining = 0;
    //    StartCoroutine("CastSkillOnDelay", 2f); //for testing only...
    }

    void Update(){
        
    }

    public string getDescription() {
        switch (skill) {
            case Skill.EmergencyRepair:
                return "Emergency Repair: All parts regenerate health, but all weapons are disabled for the duration.";
            case Skill.DarkMatter:
                return "Dark Matter: The reactor is fed with unfiltered dark matter, dealing significant damage to the core, but substantially increasing output temporarily.";
            case Skill.ThrusterOverheat:
                return "Thruster Overheat: Temporarily disables inertia recycling, causing a significant increase to thrust, but crippling manueverability and braking.";
            default:
                return ("Not a valid skill. Woops!");
        }
    }

    /// <summary>
    /// Hides all visuals of the skill without disabling the gameObject itself.
    /// Use this whenever this skill should be disabled/hidden, so that cooldowns and such can continue behind the scenes
    /// </summary>
    public void hideAllAnimations() {
        skillImage.enabled = false;
        outline.enabled = false;
        cooldown.enabled = false;
    }

    /// <summary>
    /// Unhides all skill visuals
    /// </summary>
    public void showAllAnimations() {
        skillImage.enabled = true;
        outline.enabled = true;
        cooldown.enabled = true;
    }

    /// <summary>
    /// Attempts to cast the selected skill. Uses the "skill" enum to determine which effect will occur.
    /// </summary>
    public void castSkill() {
        if (isReady) {
            isReady = false;
            durationRemaining = duration;
            StartCoroutine("ActivateSkill");
            switch (skill) {
                case Skill.EmergencyRepair:
                    emergencyRepair();
                    break;
                case Skill.DarkMatter:
                    darkMatter();
                    break;
                case Skill.ThrusterOverheat:
                    thrusterOverheat();
                    break;
                default:
                    Debug.Log("Attempted to cast a skill that doesn't exist");
                    break;
            }
        } else {
            //TODO do some UI thing to show the skill is not yet ready
        }
    }

    /// <summary>
    /// Thruster Overheat increases the effectiveness of all thrusters considerably, by removing drag from the unit's rigidbody
    /// </summary>
    private void thrusterOverheat() {
        Rigidbody rb = owner.GetComponent<Rigidbody>();
        if (rb != null) {
            StartCoroutine("ThrusterOverheatSkillEffect");
        }
    }

    IEnumerator ThrusterOverheatSkillEffect() {
        Rigidbody rb = owner.GetComponent<Rigidbody>();
        float oldDrag = rb.drag;
        rb.drag = 0;

        yield return new WaitForSecondsRealtime(duration);

        rb.drag = oldDrag;

        yield break;
    }

    /// <summary>
    /// Dark Matter causes significant instant damage to the unit's reactor core but temporarily increases the reactor output
    /// </summary>
    private void darkMatter() {
        LifeManager life = owner.GetComponent<LifeManager>();
        if (life != null) {
            StartCoroutine("DarkMatterSkillEffect");
        }
    }

    IEnumerator DarkMatterSkillEffect() {
        LifeManager life = owner.GetComponent<LifeManager>();
        //NOTE: Index zero should always be the reactor core by convention
        life.parts[0].doDamage(DARK_MATTER_REACTOR_DAMAGE, true);
        ReactorManager reactor = life.reactor;
        int oldRegen = reactor.regenPerSecond;
        reactor.regenPerSecond = (int)(reactor.regenPerSecond * DARK_MATTER_REACTOR_BOOST);
        int delta = reactor.regenPerSecond - oldRegen;

        yield return new WaitForSecondsRealtime(duration);

        reactor.regenPerSecond = reactor.regenPerSecond - delta;

        yield break;
    }

    /// <summary>
    /// Emergency Repair causes all parts to regenerate life, but disables all weapons for the duration
    /// </summary>
    private void emergencyRepair() {
        //Debug.Log("Cast emergency repair " + owner.transform.root.name);
        LifeManager life = owner.GetComponent<LifeManager>();
        if (life != null) {
            StartCoroutine("EmergencyRepairSkillEffect");
        }
    }

    IEnumerator EmergencyRepairSkillEffect() {
        LifeManager life = owner.GetComponent<LifeManager>();
        //Save the old regen of each part
        //Note: if a part is somehow added after the fact, this *should* ignore it since it would be added on the end...
        int numParts = life.parts.Length;
        for (int i = 0; i < numParts; i++) {
            life.parts[i].regenPerSecond = life.parts[i].regenPerSecond + EMERGENCY_REPAIR_REGEN;
        }

        //expensive seeking-out of every weapon attached to the ship. Could be done better...
        Weapon[] weapons = owner.GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons) {
            weapon.isPaused = true;
        }

        yield return new WaitForSecondsRealtime(duration);

        for (int i = 0; i < numParts; i++) {
            life.parts[i].regenPerSecond = life.parts[i].regenPerSecond - EMERGENCY_REPAIR_REGEN;
        }

        foreach (Weapon weapon in weapons) {
            weapon.isPaused = false;
        }
        yield break;
    }

    /// <summary>
    /// All skills use this coroutine to initiate the countdown, its duration, and eventually start the cooldown.
    /// The castSkill should also be activated at the same time.
    /// </summary>
    /// <returns></returns>
    IEnumerator ActivateSkill() {
        outline.color = outlineWhenActive;
        while (durationRemaining > 0) {
            yield return new WaitForSecondsRealtime(COOLDOWN_INTERVAL);
            durationRemaining -= COOLDOWN_INTERVAL;
            if (durationRemaining > 0) {
                outline.fillAmount = durationRemaining / duration;
            }
        }
        outline.fillAmount = 1;
        cooldownRemaining = cooldownTime;
        StartCoroutine("Cooldown");
        yield break;
    }

    IEnumerator Cooldown() {
        outline.color = outlineWhenOnCooldown;
        while (cooldownRemaining > 0) {
            yield return new WaitForSecondsRealtime(COOLDOWN_INTERVAL);
            cooldownRemaining -= COOLDOWN_INTERVAL;
            if (cooldownRemaining > 0) {
                cooldown.fillAmount = (cooldownRemaining / cooldownTime);
            }
        }
        cooldown.fillAmount = 0;
        isReady = true;
        outline.color = outlineWhenReady;
        yield break;
    }

    IEnumerator CastSkillOnDelay(float delay) {
        yield return new WaitForSecondsRealtime(delay);
        castSkill();
        yield break;
    }
}
