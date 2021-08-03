/*
 * Manages properties of units such as ownership and type.
 * TODO
 * Visual effect showing who owns a particular ship
 * 
 * 06.16.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 06.16.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour{

    public enum Ownership {
        Player,
        Enemy,
        Ally,
        Nuetral
    }
    [Tooltip("Who is in control of this vessel? Used primarily for weapons targeting.")]
    public Ownership owner;

    public enum ShipSizeClass {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        Titan
    }
    [Tooltip("What is the size of this ship class? Used in target prioritization.")]
    public ShipSizeClass sizeClass;

    public enum ShipSizeType {
        Vanguard,
        Ranged,
        Support,
        NonCombat,
        Strikecraft
    }
    [Tooltip("What is the type of this ship class? Used in target prioritization.")]
    public ShipSizeType typeClass;

    void Start(){
        
    }

    void Update(){
        
    }
}
