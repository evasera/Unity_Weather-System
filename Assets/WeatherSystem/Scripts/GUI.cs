using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace weatherSystem
{
    public class GUI : MonoBehaviour {

        public CentralClock clock;

        float seconds = 0.0f;

        // Use this for initialization
        void Start() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
        }

        // Update is called once per frame
        void Update() {
            seconds += UnityEngine.Time.deltaTime;

            if (seconds > 10)
            {
                Debug.Log("Time: " + clock.getCurrentTime().ToString());
                seconds -= 10;
            }
    }


    }
}