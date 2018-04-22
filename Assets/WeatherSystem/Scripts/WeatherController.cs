using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class WeatherController : MonoBehaviour {
        #region public Atributes
        public int updateFrequency = 60;

        public ParticleSystem rainSystem;
        public ParticleSystem snowSystem;
        public ParticleSystem stormSystem;
        #endregion public Atributes
        #region private Atributes
        private CentralClock clock;
        private Time lastUpdate;
        #endregion private Atributes 

        private void Awake() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }

            if(rainSystem == null) {
                Debug.LogError("Weather controller needs a rain system reference");
            }
            if (snowSystem == null) {
                Debug.LogError("Weather controller needs a rain system reference");
            }
            if (stormSystem == null) {
                Debug.LogError("Weather controller needs a rain system reference");
            }
        }

        // Use this for initialization
        void Start() {
            lastUpdate = clock.getCurrentTime();
        }

        // Update is called once per frame
        void Update() {
            Time currentTime = clock.getCurrentTime();
            if(lastUpdate.SecondsBetween(currentTime)>= updateFrequency * 60) {

            }

        }
    }
}
