using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace weatherSystem
{
    public class GUI : MonoBehaviour {

        public CentralClock clock;
        public WeatherController weatherController;

        public UnityEngine.UI.Text timeText;
        public UnityEngine.UI.Text dateText;
        public UnityEngine.UI.Text SeasonText;
        public UnityEngine.UI.Text weatherStateText;

        // Use this for initialization
        void Start() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
        }

        // Update is called once per frame
        void Update() {
            timeText.text = clock.getCurrentTime().ToString();
            dateText.text = clock.getCurrentDate().ToString();
            SeasonText.text = clock.GetSeason().name;
            weatherStateText.text = weatherController.ToString();
        }


    }
}