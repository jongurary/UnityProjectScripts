/*
 * A single instance of this class should exist (singleton-ish).
 * Creates a common place for scripts to access materials that are used in various locations
 * 
 * 07.17.2020 v1
 * initial commit
 * 
 * @author v1 Jonathan Gurary 07.17.2020
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialsManager : MonoBehaviour
{
    [Tooltip("Materials used to indicate a part's health, index 0 should be healthiest and the last should be dead")]
    public Material[] damageIndicatorMaterials = new Material[4];

    void Start(){

        
    }

    void Update(){
        
    }
}
