/* 
 * Deals damage around a point in space, and applies an explosive physics effect. Used when a unit dies or any
 * other time an explosion should deal damage around a point. Also supports pulsing periodic damage several times.
 * Should generally be used alongside an explosion animation object.
 * 
 *
 * 01.06.2020 v1.0
 * initial commit
 *
 * @author v1 Jonathan Gurary 01.06.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// After creating an object with this script, call the detonate method to create the underlying explosion damage.
/// Alternatively, call the detonateOverTime method to pulse damage several times.
/// </summary>
public class Explosion : MonoBehaviour {

    void Start() {

    }

    void Update() {

    }

    /// <summary>
    /// Deals damage to all specified layer targets inside the blast radius, using linear distance to determine damage to inflict. <br></br>
    /// (Zero distance = max damage, max distance = 1% of missile Damage) <br></br>
    /// Also applies physics force to objects depending on part damage inflicted.
    /// </summary>
    /// <param name="layers">Layers to seek targets on, other layers ignored. Example: 1 << LayerMask.NameToLayer("Ship Parts")</param>
    /// <param name="blastRadius"></param>
    /// <param name="origin"></param>
    /// <param name="damage"></param>
    public void detonate(LayerMask layers, float blastRadius, Vector3 origin, int damage) {
        Collider[] cols = Physics.OverlapSphere(origin, blastRadius, layers);
        for (int i = 0; i < cols.Length; i++) {
            float distance = Vector3.Distance(origin, cols[i].transform.position);
            if (distance < blastRadius) {
                float damageMultiplier = distance / blastRadius + .01f;
                PartLife life = cols[i].gameObject.GetComponent<PartLife>();
                if (life != null) {
                    life.doDamage((int)(damage * damageMultiplier), false);
                    Rigidbody rb = cols[i].transform.root.gameObject.GetComponent<Rigidbody>();
                    rb.AddExplosionForce(damage * damageMultiplier * 50, origin, blastRadius, 0f);
                }
            }


        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Deals damage to all ship part targets inside the blast radius, using linear distance to determine damage to inflict. <br></br>
    /// (Zero distance = max damage, max distance = 1% of missile Damage) <br></br>
    ///  Also applies physics force to objects depending on part damage inflicted.
    /// </summary>
    /// <param name="layers">Layers to seek targets on, other layers ignored</param>
    /// <param name="blastRadius"></param>
    /// <param name="damage"></param>
    /// <param name="origin"></param>
    /// <param name="pulses"></param>
    /// <param name="pulseInterval"></param>
    public void detonateOverTime(LayerMask layers, float blastRadius, Vector3 origin, int damage, int pulses, float pulseInterval) {
        StartCoroutine(detonateOverTimeRoutine(layers, blastRadius, origin, damage, pulses, pulseInterval));
    }

    IEnumerator detonateOverTimeRoutine(LayerMask layers, float blastRadius, Vector3 origin, int damage, int pulses, float pulseInterval) {
        while (pulses > 0) {
            Collider[] cols = Physics.OverlapSphere(origin, blastRadius, layers);
            for (int i = 0; i < cols.Length; i++) {
                float distance = Vector3.Distance(origin, cols[i].transform.position);
                if (distance < blastRadius) {
                    float damageMultiplier = distance / blastRadius + .01f;
                    PartLife life = cols[i].gameObject.GetComponent<PartLife>();
                    if (life != null) {
                        life.doDamage((int)(damage * damageMultiplier), false);
                        Rigidbody rb = cols[i].transform.root.gameObject.GetComponent<Rigidbody>();
                        rb.AddExplosionForce(damage * damageMultiplier * 50/pulses, origin, blastRadius, 0f);
                    }
                }
            }
            pulses--;
            yield return new WaitForSeconds(pulseInterval);
        }

        Destroy(gameObject);
        yield break;
    }
}
