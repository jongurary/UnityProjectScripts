/* 
 * A singleton that manages assets used by all non-player ships. Stores, among other things,
 * an array of all allied ships, which individual enemy AI will query to find optimal targets.
 * 
 * All ships must MANUALLY REGISTER here, this script does NOT scan for ships.
 * 
 * TODO
 * Manage shared target goals
 * What happens to play-owned ships in the array when they die? (Perhaps nothing, the performance
 *  cost is negligible)
 * Convert to arraylist to support ships being added to the field more easily? Add blank spaces to the array?
 *
 * 01.30.2020 v1.1
 * Now also tracks enemy ships, performance be damned
 *
 * 01.06.2020 v1.0
 * initial commit
 *
 * @author v1 Jonathan Gurary 01.06.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAICommander : MonoBehaviour{

    public GameObject[] playerShips;
    /// <summary>
    /// Tracks if ships are null, because Threads can't null check gameObjects
    /// </summary>
    public bool[] isLiving;
    public Vector3[] playerShipPositions;

    public GameObject[] enemyShips;
    /// <summary>
    /// Tracks if ships are null, because Threads can't null check gameObjects
    /// </summary>
    public bool[] isLivingEnemy;
    public Vector3[] enemyShipPositions;


    /// <summary>
    /// Tracks life of potential targets
    /// </summary>
    public int[] coreLife;

    /// <summary>
    /// Tracks life of allies
    /// </summary>
    public int[] coreLifeEnemy;


    private static EnemyAICommander instance;
    
    void Start(){
        instance = this;
        playerShipPositions = new Vector3[playerShips.Length];
        isLiving = new bool[playerShips.Length];
        coreLife = new int[playerShips.Length];

        enemyShipPositions = new Vector3[enemyShips.Length];
        isLivingEnemy = new bool[enemyShips.Length];
        coreLifeEnemy = new int[enemyShips.Length];
        
        StartCoroutine("UpdateFleetPositions");
    }

    void Update(){
        
    }

    public static EnemyAICommander getInstance() {
        if (instance != null) {
            return instance;
        } else {
            instance = new EnemyAICommander();
            return instance;
        }
    }

    /// <summary>
    /// Returns the index of the given object in the enemy ships array, -1 if doesn't exist
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public int getIndexOfInEnemyArray(GameObject gameObject) {
        for(int i=0; i<enemyShips.Length; i++) {
            if (enemyShips[i] != null) {
                if(enemyShips[i] == gameObject) {
                    return i;
                }
            }
        }
        return -1;
    }

    IEnumerator UpdateFleetPositions() {
        while (true) {
            for(int i=0; i<playerShips.Length; i++) {
                if (playerShips[i] != null) {
                    playerShipPositions[i] = playerShips[i].transform.position;
                    try {
                        coreLife[i] = playerShips[i].transform.root.gameObject.GetComponent<LifeManager>().parts[0].currentLife;
                    }catch(UnassignedReferenceException e) {
                        coreLife[i] = -2;
                    }
                    isLiving[i] = true;
                } else {
                    coreLife[i] = -1;
                    isLiving[i] = false;
                }
            }

            for (int i = 0; i < enemyShips.Length; i++) {
                if (enemyShips[i] != null) {
                    enemyShipPositions[i] = enemyShips[i].transform.position;
                    try {
                        coreLifeEnemy[i] = enemyShips[i].transform.root.gameObject.GetComponent<LifeManager>().parts[0].currentLife;
                    } catch (UnassignedReferenceException e) {
                        coreLifeEnemy[i] = -2;
                    }
                    isLivingEnemy[i] = true;
                } else {
                    coreLifeEnemy[i] = -1;
                    isLivingEnemy[i] = false;
                }
            }
            yield return new WaitForSeconds(4f);
        }
    }
}
