using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class FXVShieldHit_Redux : MonoBehaviour
    {
        private float lifeTime = 0.5f;
        private float lifeStart = 0.5f;
        private float coveringTime = 0.3f;

        public MeshRenderer myRenderer;

        void Start()
        {
            if (myRenderer == null) {
                myRenderer = GetComponent<MeshRenderer>();
            }
            Destroy(gameObject, lifeTime);
        }

        void Update()
        {
            lifeTime -= Time.deltaTime;

            Color c = myRenderer.material.color;
            c.a = Mathf.Max(0.0f, (lifeTime - coveringTime) / lifeStart);
            myRenderer.material.color = c;

            if (lifeTime < coveringTime)
            {
                myRenderer.material.SetFloat("_HitShieldCovering", lifeTime / coveringTime);
            }
        }

        public void StartHitFX(float time)
        {
            lifeTime = lifeStart = time;
            lifeTime += coveringTime;

            //if (myRenderer == null) {
            //    myRenderer = GetComponent<MeshRenderer>();
            //}
            //Color c = myRenderer.material.color;
            //c.a = 1.0f;
            //myRenderer.material.color = c;
        }
    }