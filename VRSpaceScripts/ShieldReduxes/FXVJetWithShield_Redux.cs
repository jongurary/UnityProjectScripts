using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXVJetWithShield_Redux : MonoBehaviour
{
    public FXVShield_Redux shield;

    void Start()
    {
        shield.SetShieldEffectDirection((transform.right + transform.up).normalized);
    }

    void Update()
    {

    }
}
