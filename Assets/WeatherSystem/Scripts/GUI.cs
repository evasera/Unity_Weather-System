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
        public UnityEngine.UI.Text sunsetAndSunrise;
        public UnityEngine.UI.Text weatherStateText;

        // Use this for initialization
        void Start() {
			//Getting Clock reference
           clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("GUI: No clock could be found, please remember to tag the object with the CentralClock script");
                Debug.Break();
            }
        }

        // Update is called once per frame
        void Update() {
            timeText.text = clock.getCurrentTime().ToString();
            dateText.text = clock.getCurrentDate().ToString();
            SeasonText.text = clock.GetSeason().name;
            sunsetAndSunrise.text = "Sunrise: " + clock.getSunriseTime().ToString() + "\tSunset: " + clock.getSunsetTime().ToString();
            weatherStateText.text = weatherController.ToString();
        }


    }
}