/* 
 * Computes the optimal move for a Corvette inside a thread, and periodically executes it
 * on the main thread
 *
 * 01.30.2021
 * Approachs targets on closest point in 3D sphere around target
 * Targets are no longer "sticky", every AI tick will re-evaluate a new target
 * Added retreat range, will now abandon retreat if no player ships within that distance
 * Now attempts to retreat in 4 directions (forward, left, right, back)
 * Added collision avoidance, will now prioritize avoiding collisions over all other thrust activies (unless ramming)
 * Collision avoidance required adding enemy fleet positions to EnemyAICommander
 * Ramming still TBD but framework started
 *
 * 01.23.2020 v1.0
 * initial commit
 *
 * @author v1 Jonathan Gurary 01.06.2020
 */
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CorvetteEnemyAI : MonoBehaviour{

    public enum Behavior_State{
        IS_ATTACKING,
        IS_RETREATING,
        IS_HOLDING,
        IS_CHASING,
        IS_THINKING
    }

    public Behavior_State currentState;

    public GameObject attackTarget;
    [Tooltip("When a collision is close, ship will attempt to avoid ONLY the closest threat, this one")]
    public GameObject avoidCollisionTarget;
    public Vector3 retreatTarget;

    public EnemyAICommander commander;
    /// <summary>
    /// Index of this ship in the commander, to ignore for most checks
    /// </summary>
    private int selfIndexInCommander;

    public UnitManager unitManager;
    public ThrusterController thrusterController;
    public LifeManager lifeManager;

    public SkillCasting repairSkill;

    [Tooltip("Distance to attempt to maintain from target")]
    public float mostEffectiveAttackRange;
    [Tooltip("Max distance to run when retreating")]
    public float retreatDistance;
    [Tooltip("Distance before collision avoidance takes priority over all other thrust actions")]
    public float collisionAvoidanceRange;
    [Range(0f, 1f)]
    [Tooltip("What percentage of the ship's parts must fail before triggering repair")]
    public float partFailsToRepair;
    [Range(0f, 1f)]
    [Tooltip("Below this % core health, unit will attempt to retreat")]
    public float reactorLifeToRetreat;

    //Decision Engine factors......
    private Vector3 position;
    private Vector3 forwardVector;
    private Vector3 rightVector;
    private float reactorLife;
    private float partsFailed;
    private float weaponsFailed;
    //End factors.....

    //True if attempting to repair when skill is off CD
    private bool flagRepair = false;
    //True if not avoiding collisions and attempting to ram target
    private bool flagRamming = false;
    //True if abandoning all other thrust priorities to avoid a collision
    private bool flagAvoidCollision;

    private bool killThread = false;

    void Start(){
        if (unitManager == null) {
            unitManager = gameObject.transform.root.gameObject.GetComponent<UnitManager>();
        }

        //Disable the AI script if this is a player-owned prefab
        if (unitManager == null || unitManager.owner == UnitManager.Ownership.Player) {
            this.enabled = false;
            return;
        }


        if (thrusterController == null) {
            thrusterController = gameObject.transform.root.gameObject.GetComponent<ThrusterController>();
        }
        if (lifeManager == null) {
            lifeManager = gameObject.transform.root.gameObject.GetComponent<LifeManager>();
        }

        if (commander == null) {
            commander = EnemyAICommander.getInstance();
        }

        selfIndexInCommander = commander.getIndexOfInEnemyArray(this.gameObject);

        StartCoroutine("UpdateStatsAndOrders");

        Thread thread = new Thread(this.threadedAI);
        thread.IsBackground = true;
        thread.Start();
        
    }


    void Update(){
        
    }

    private void OnDestroy() {
        killThread = true;
    }

    private void OnDisable() {
        killThread = true;
    }

    public void threadedAI() {
   //     long start = System.DateTime.UtcNow.Millisecond;
   //     long now = System.DateTime.UtcNow.Millisecond;
        int start = System.Environment.TickCount;
        int now = System.Environment.TickCount;

        while (!killThread) {
            if(now - start > 1000) {
                  decisionEngine();
           //     Debug.Log("Executing a decision");
                start = System.Environment.TickCount;
            } else {
                now = System.Environment.TickCount;
              //  Debug.Log(start + " " + now);
            }
        }
    }


    public void decisionEngine() {
        determineCollisionThreat();
        /*
         * When current target dies, seek out the nearest viable target
         */
        if (currentState != Behavior_State.IS_RETREATING && currentState != Behavior_State.IS_HOLDING) {
            pickNearestTarget();
            currentState = Behavior_State.IS_ATTACKING;
            
        }
        determineRepair();
        determineRetreat();
    }


    /// <summary>
    /// If sufficiently isolated, or mulitple part damage is sustained, begin repair process
    /// </summary>
    public void determineRepair() {
        if(partsFailed > partFailsToRepair) {
            flagRepair = true;
        }

        //Always repair if most or all weapons have failed
        if (weaponsFailed >= .9f){
            flagRepair = true;
        }
    }

    /// <summary>
    /// Scans for potential collisions and raises flag to divert all thrust priorities to avoiding one
    /// if a ship is within the warning distance
    /// </summary>
    public void determineCollisionThreat() {
        avoidCollisionTarget = null;

        float closest = collisionAvoidanceRange;
        for (int i = 0; i < commander.playerShips.Length; i++) {
            if (commander.isLiving[i]) {
                float dist = Vector3.Distance(position, commander.playerShipPositions[i]);
                if (dist < closest) {
                    closest = dist;
                    avoidCollisionTarget = commander.playerShips[i];
                }
            }
        }

        for (int i = 0; i < commander.enemyShips.Length; i++) {
            if (i == selfIndexInCommander) {
                continue;
            }
            if (commander.isLivingEnemy[i]) {
                float dist = Vector3.Distance(position, commander.enemyShipPositions[i]);
                if (dist < closest) { 
                    closest = dist;
                    avoidCollisionTarget = commander.enemyShips[i];
                }
            }
        }
        
        if (avoidCollisionTarget != null) {
            flagAvoidCollision = true;
        } else {
            flagAvoidCollision = false;
        }
    }

    /// <summary>
    /// If damaged, and escape seems possible, enters retreat mode and sets a retreat target.
    /// Ship should queue thruster overheat after reaching rotation and set reactor
    /// priority to thrust
    /// </summary>
   public void determineRetreat() {

        //TODO some ships should retreat whenever a ship enters a certain range
        /**
         * if( coward type ship && has an enemy in range){
         * retreat
         * }
         */

        //TODO some ships should "organized retreat" while continuing to face target
        /*
         * if( organized type ship && meets retreat criteria){
         * Behavior State is organized retreat
         */

        if(reactorLife < reactorLifeToRetreat) {
            //TODO consider if some ships should try other directions first? 
            //Attempts to retreat forward first, then left/right, and finally rear
            //Checks if hostiles in that area, aborts and tries the next location if there are


            bool validPoint = true;
            //check forward
            Vector3 retreatPoint = position + forwardVector * mostEffectiveAttackRange;
            for (int i = 0; i < commander.playerShips.Length; i++) {
                if (commander.isLiving[i]) {
                    float dist = Vector3.Distance(retreatPoint, commander.playerShipPositions[i]);
                    if (dist < mostEffectiveAttackRange) {
                        validPoint = false;
                        break;
                    }
                }
            }
            if (!validPoint) {
                validPoint = true;
                //check right
                retreatPoint = position + rightVector * mostEffectiveAttackRange;
                for (int i = 0; i < commander.playerShips.Length; i++) {
                    if (commander.isLiving[i]) {
                        float dist = Vector3.Distance(retreatPoint, commander.playerShipPositions[i]);
                        if (dist < mostEffectiveAttackRange) {
                            validPoint = false;
                            break;
                        }
                    }
                }
            }
            if (!validPoint) {
                validPoint = true;
                //check left
                retreatPoint = position - rightVector * mostEffectiveAttackRange;
                for (int i = 0; i < commander.playerShips.Length; i++) {
                    if (commander.isLiving[i]) {
                        float dist = Vector3.Distance(retreatPoint, commander.playerShipPositions[i]);
                        if (dist < mostEffectiveAttackRange) {
                            validPoint = false;
                            break;
                        }
                    }
                }
            }

            if (!validPoint) {
                validPoint = true;
                //check rear
                retreatPoint = position - forwardVector * mostEffectiveAttackRange;
                for (int i = 0; i < commander.playerShips.Length; i++) {
                    if (commander.isLiving[i]) {
                        float dist = Vector3.Distance(retreatPoint, commander.playerShipPositions[i]);
                        if (dist < mostEffectiveAttackRange) {
                            validPoint = false;
                            break;
                        }
                    }
                }
            }

            if (validPoint) {
                currentState = Behavior_State.IS_RETREATING;
                retreatTarget = retreatPoint;
                attackTarget = null;
            }

        } else {
            if(currentState == Behavior_State.IS_RETREATING) {
                currentState = Behavior_State.IS_THINKING;
            }
        }

        //end retreat after enough distance is gained from all hostiles...
        float closest = float.PositiveInfinity;
        for (int i = 0; i < commander.playerShips.Length; i++) {
            if (commander.isLiving[i]) {
                float dist = Vector3.Distance(position, commander.playerShipPositions[i]);
                if (dist < closest) {
                    closest = dist;
                }
            }
        }
        if (closest > retreatDistance) {
            if (currentState == Behavior_State.IS_RETREATING) {
                currentState = Behavior_State.IS_THINKING;
            }
        }

    }

    /// <summary>
    /// Picks the nearest target and sets it to the attack target.
    /// <br></br>
    /// Note: This method can fail to pick a target if all allied ships are dead
    /// </summary>
    public void pickNearestTarget() {
        GameObject target = null;
        float closest = float.PositiveInfinity;
        for (int i = 0; i < commander.playerShips.Length; i++) {
            if (commander.isLiving[i]) {
                float dist = Vector3.Distance(position, commander.playerShipPositions[i]);
                if (dist < closest) {
                    closest = dist;
                    target = commander.playerShips[i];
                }
            }
        }
        attackTarget = target;
    }

    /// <summary>
    /// Picks the target with the lowest life and sets it to the attack target
    /// <br></br>
    /// Note: This method can fail to pick a target if all allied ships are dead
    /// </summary>
    public void pickWeakestTarget() {
        GameObject target = null;
        int weakest = 9999999;
        for (int i = 0; i < commander.playerShips.Length; i++) {
            if (commander.isLiving[i]) {
                if (commander.coreLife[i] < weakest) {
                    weakest = commander.coreLife[i];
                    target = commander.playerShips[i];
                }
            }
        }
        attackTarget = target;
    }

    IEnumerator UpdateStatsAndOrders() {
        while (true) {
            position = transform.position;
            forwardVector = transform.forward;
            rightVector = transform.right;
            reactorLife = (float) lifeManager.parts[0].currentLife / (float) lifeManager.parts[0].maxLife;
            int failed = 0, total = 0 ;
            //TODO add count of failed weapons
            for(int i=0; i<lifeManager.parts.Length; i++) {
                if (lifeManager.parts[i] != null) {
                    total++;
                    if (lifeManager.parts[i].isDead) {
                        failed++;
                    }
                }
            }
            partsFailed = (float) failed / (float) total;

            executeOrders();
            yield return new WaitForSeconds(5f);
        }

    }

    public void executeOrders() {
        //Always avoid collisions, unless ready to ram target...
        if (!flagRamming) {
            
        } else {
            if (flagAvoidCollision) { //attempts to move exactly opposite to collision avoidance target, to 2x collision avoid range
                Vector3 approachAngleNormed = Vector3.Normalize(avoidCollisionTarget.transform.position - gameObject.transform.position);
                Vector3 approachPoint = avoidCollisionTarget.transform.position - approachAngleNormed * collisionAvoidanceRange * 2;
                thrusterController.setDestination(approachPoint);
            }
        }

        if (currentState == Behavior_State.IS_ATTACKING) {
            if (attackTarget != null) {
                thrusterController.setRotationTargetObject(attackTarget);

                //TODO attack angle depends on ship's attack type, should not enter side facing against side attack ships...

                //Now approachs closest point on the sphere
                if (!flagAvoidCollision) { //collision avoidance overrides other thrust changes
                    Vector3 approachAngleNormed = Vector3.Normalize(attackTarget.transform.position - gameObject.transform.position);
                    Vector3 approachPoint = attackTarget.transform.position - approachAngleNormed * mostEffectiveAttackRange;
                    //attackTarget.transform.position - attackTarget.transform.forward * mostEffectiveAttackRange //old method for targeting rear...
                    thrusterController.setDestination(approachPoint);
                }
            } 
        }else if(currentState == Behavior_State.IS_RETREATING) {
            //Don't use the vector computed by the thread because it may be out of date, though it's close enough for purposes of checking safety
            Vector3 realRetreatTarget = position + forwardVector * mostEffectiveAttackRange * 2;
            thrusterController.setRotationTarget(realRetreatTarget);
            if (!flagAvoidCollision) { //collision avoidance overrides other thrust changes
                thrusterController.setDestination(realRetreatTarget);
            }
        }

        if (flagRepair) {
            if (repairSkill != null) {
                repairSkill.castSkill();
                flagRepair = false;
            }
        }
    }
}
