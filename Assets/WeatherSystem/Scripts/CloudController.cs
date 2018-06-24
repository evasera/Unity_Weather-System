using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class CloudController : MonoBehaviour {
        #region public atributes
        [Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
        public bool debug;

        [Header("Intensity Gameobjects:")]
        public GameObject intensity1;
        public GameObject intensity2;
        public GameObject intensity3;
        public GameObject intensity4;
        public GameObject intensity5;
        #endregion public atributes

        public void SetIntensity(int intensity) {
            if(intensity > 0){
                if(!intensity1.active)
                    intensity1.SetActive(true);
            } else {
                intensity1.SetActive(false);
            }
            if (intensity > 1) {
                if(!intensity2.active)
                    intensity2.SetActive(true);
            } else {
                intensity2.SetActive(false);
            }
            if (intensity > 2) {
                if(!intensity3.active)
                    intensity3.SetActive(true);
            } else {
                intensity3.SetActive(false);
            }
            if (intensity > 3 ) {
                if(!intensity4.active)
                    intensity4.SetActive(true);
            } else {
                intensity4.SetActive(false);
            }
            if (intensity > 4 ) {
                if(!intensity5.active)
                    intensity5.SetActive(true);
            } else {
                intensity5.SetActive(false);
            }
        }

        void Awake() {
            intensity1.SetActive(false);
            intensity2.SetActive(false);
            intensity3.SetActive(false);
            intensity4.SetActive(false);
            intensity5.SetActive(false);
        }

        
    }
}
