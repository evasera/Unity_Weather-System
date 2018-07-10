using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class LightColorChange : MonoBehaviour {

        #region Constants
        const int DAY = 0;
        const int DAY_TO_DUSK = 1;
        const int DUSK_TO_NIGHT = 2;
        const int NIGHT = 3;
        const int NIGHT_TO_DAWN = 4;
        const int DAWN_TO_DAY = 5;
        #endregion Constants

        #region Public atributes
        [Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
        public bool debug;

        [Header("Color settings:")]
        public Color day;
        public Color duskAndDown;
        public Color night;

        [Header("Dawn and Dusk settings:")]
        public int startMinutesBeforeDawnDusk;
        public int endMinutesAfterDawnDusk;
        #endregion  Public atributes

        #region Private Atributes
        private CentralClock clock;
        //private Renderer renderer;
		private Light light;
        private int phase;
        private Time previousTime;
        private Time currentTime;

        //day to dusk color change
        private float dayToDusk_r;
        private float dayToDusk_g;
        private float dayToDusk_b;
        private float dayToDusk_a;

        //dusk to night color change
        private float duskToNight_r;
        private float duskToNight_g;
        private float duskToNight_b;
        private float duskToNight_a;

        //night to dawn color change
        private float nightToDawn_r;
        private float nightToDawn_g;
        private float nightToDawn_b;
        private float nightToDawn_a;

        //dawn to day color change
        private float dawnToDay_r;
        private float dawnToDay_g;
        private float dawnToDay_b;
        private float dawnToDay_a;

        #endregion Private Atributes

        private void ChangeColor(int secondsSincceStateChange) {
            //Color color = renderer.material.color;
			Color color = light.color;
			if(debug){
				Debug.Log("CLOUDCOLOR: Canging material color");
				Debug.Break();
			}
            switch (phase) {
                case DAY:
                    //renderer.material.color = day;
					light.color = day;
					if(debug){
						Debug.Log("CLOUDCOLOR: color set to day: \n" ); 
						Debug.Break();
					}
                    break;

                case DAY_TO_DUSK:
                    color.r = day.r + dayToDusk_r*secondsSincceStateChange;
                    color.g = day.g + dayToDusk_g * secondsSincceStateChange;
                    color.b = day.b + dayToDusk_b * secondsSincceStateChange;
                    color.a = day.a + dayToDusk_a * secondsSincceStateChange;
					
					if(debug){
						Debug.Log("CLOUDCOLOR: transitioning from day to dusk \n" + 
								"R: " + color.r + "\t G: " + color.g + "\t B: " + color.b + "\t A: " + color.a);
						Debug.Break();
					}
                    //renderer.material.color = color;
					light.color = color;
                    break;

                case DUSK_TO_NIGHT:
                    color.r = duskAndDown.r + duskToNight_r * secondsSincceStateChange;
                    color.g = duskAndDown.g + duskToNight_g * secondsSincceStateChange;
                    color.b = duskAndDown.b + duskToNight_b * secondsSincceStateChange;
                    color.a = duskAndDown.a + duskToNight_a * secondsSincceStateChange;
						
					if(debug){
						Debug.Log("CLOUDCOLOR: transitioning from dusk to night \n" + 
								"R: " + color.r + "\t G: " + color.g + "\t B: " + color.b + "\t A: " + color.a);
						Debug.Break();
					}
			
                    //renderer.material.color = color;
					light.color = color;
                    break;

                case NIGHT:
                    //renderer.material.color = night;
					light.color = night;
					if(debug){
						Debug.Log("CLOUDCOLOR: color set to night");
						Debug.Break();
					}
                    break;

                case NIGHT_TO_DAWN:
                    color.r = night.r + nightToDawn_r * secondsSincceStateChange;
                    color.g = night.g + nightToDawn_g * secondsSincceStateChange;
                    color.b = night.b + nightToDawn_b * secondsSincceStateChange;
                    color.a = night.a + nightToDawn_a * secondsSincceStateChange;
					if(debug){
						Debug.Log("CLOUDCOLOR: transitioning from night to dawn \n" + 
								"R: " + color.r + "\t G: " + color.g + "\t B: " + color.b + "\t A: " + color.a);
						Debug.Break();
					}
                    //renderer.material.color = color;
					light.color = color;
                    break;

                case DAWN_TO_DAY:
                    color.r = duskAndDown.r + dawnToDay_r * secondsSincceStateChange;
                    color.g = duskAndDown.g + dawnToDay_g * secondsSincceStateChange;
                    color.b = duskAndDown.b + dawnToDay_b * secondsSincceStateChange;
                    color.a = duskAndDown.a + dawnToDay_a * secondsSincceStateChange;
					if(debug){
						Debug.Log("CLOUDCOLOR: transitioning from dawn to day \n" + 
								"R: " + color.r + "\t G: " + color.g + "\t B: " + color.b + "\t A: " + color.a);
						Debug.Break();
					}
                    //renderer.material.color = color;
					light.color = color;
                    break;

            }
        }

        //internal altributes initialization
        private void Awake() {
            //day to dusk color change
            dayToDusk_r = (duskAndDown.r - day.r) / (startMinutesBeforeDawnDusk * 60);
            dayToDusk_g = (duskAndDown.g - day.g) / (startMinutesBeforeDawnDusk * 60);
            dayToDusk_b = (duskAndDown.b - day.b) / (startMinutesBeforeDawnDusk * 60);
            dayToDusk_a = (duskAndDown.a - day.a) / (startMinutesBeforeDawnDusk * 60);

            //dusk to night color change
            duskToNight_r = (night.r - duskAndDown.r) / (startMinutesBeforeDawnDusk * 60);
            duskToNight_g = (night.g - duskAndDown.g) / (startMinutesBeforeDawnDusk * 60);
            duskToNight_b = (night.b - duskAndDown.b) / (startMinutesBeforeDawnDusk * 60);
            duskToNight_a = (night.a - duskAndDown.a) / (startMinutesBeforeDawnDusk * 60);

            //night to dawn color change
            nightToDawn_r = (duskAndDown.r - night.r) / (startMinutesBeforeDawnDusk * 60);
            nightToDawn_g = (duskAndDown.g - night.g) / (startMinutesBeforeDawnDusk * 60);
            nightToDawn_b = (duskAndDown.b - night.b) / (startMinutesBeforeDawnDusk * 60);
            nightToDawn_a = (duskAndDown.a - night.a) / (startMinutesBeforeDawnDusk * 60);

            //dawn to day color change
            dawnToDay_r = (day.r - duskAndDown.r) / (startMinutesBeforeDawnDusk * 60);
            dawnToDay_g = (day.g - duskAndDown.g) / (startMinutesBeforeDawnDusk * 60);
            dawnToDay_b = (day.b - duskAndDown.b) / (startMinutesBeforeDawnDusk * 60);
            dawnToDay_a = (day.a - duskAndDown.a) / (startMinutesBeforeDawnDusk * 60);

            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("CLOUDCOLOR " + this.name + ": No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }

            if (startMinutesBeforeDawnDusk < 0) {
                Debug.LogError("CLOUDCOLOR " + this.name + ": The minutesToFade parameter can not be negative");
            }

            if (endMinutesAfterDawnDusk < 0) {
                Debug.LogError("CLOUDCOLOR " + this.name + ": The minutesToFade parameter can not be negative");
            }

			/*
            renderer = GetComponent<Renderer>();
            if (renderer == null) {
                Debug.LogError("CLOUDCOLOR: GameObject " + this.name + "Needs a Renderer to apply the color changes");
                Debug.Break();
            }
			*/
			
			light = GetComponent<Light>();
			if (light == null) {
                Debug.LogError("CLOUDCOLOR: GameObject " + this.name + "Needs a light to to apply the color changes");
                Debug.Break();
            }
        }

        // Use this for initialization
        void Start() {
            currentTime = clock.getCurrentTime();
            Time sunrise = clock.getSunriseTime();
            Time sunset = clock.getSunsetTime();
            Time dayToDuskEnd = sunset.TimeWithSeconds(sunrise, (int)(endMinutesAfterDawnDusk * 60));
            Time dayToDuskStart = sunset.TimeWithSeconds(sunset, -(int)(startMinutesBeforeDawnDusk * 60));
            Time nightToDawnEnd = sunrise.TimeWithSeconds(sunrise, (int)(startMinutesBeforeDawnDusk * 60));
            Time nightToDawnStart = sunrise.TimeWithSeconds(sunset, -(int)(endMinutesAfterDawnDusk * 60));

            if (debug) {
                Debug.Log("CLOUDCOLOR: CALCULATING INITIAL STATE ---------------------");
                Debug.Log("CLOUDCOLOR: currentTime: " + currentTime.ToString() + "\n" +
                            "\t Sunrise: " + sunrise + "\t sunset: " + sunset);
				Debug.Log("CLOUDCOLOR: dayToDuskStart: " + dayToDuskStart.ToString() + "\t dayToDuskEnd: "+ dayToDuskEnd+  "\n" +
                            "\t nightToDawnStart: " + nightToDawnStart + "\t nightToDawnEnd: " + nightToDawnEnd);
                Debug.Break();
            }

            if (currentTime.CompareTo(nightToDawnStart) < 0) {
                phase = NIGHT;
                if (debug) {
                    Debug.Log("CLOUDCOLOR: Current time fount out to be bewteen midnight and the start of sunrise color change\n" +
                                "Phase set to " + phase + " NIGHT, color set to night");
                    Debug.Break();
                }
                ChangeColor(0);

            } else if (currentTime.CompareTo(nightToDawnStart) >= 0 && currentTime.CompareTo(sunrise) < 0) {
                phase = NIGHT_TO_DAWN;
                if (debug) {
                    Debug.Log("CLOUDCOLOR: Current time fount out to be bewteen start and end of night to dawn \n" +
                                "Phase set to " + phase + " NIGHT_TO_DAWN");
                    Debug.Break();
                }
                ChangeColor(nightToDawnStart.SecondsBetween(sunrise));
            } else if (currentTime.CompareTo(sunrise) >= 0 && currentTime.CompareTo(nightToDawnEnd) < 0) {
                phase = DAWN_TO_DAY;
                if (debug) {
                    Debug.Log("CLOUDCOLOR: Current time fount out to be bewteen start and end of night to dawn \n" +
                                "Phase set to " + phase + " NIGHT_TO_DAWN");
                    Debug.Break();
                }
                ChangeColor(sunrise.SecondsBetween(nightToDawnEnd));
            } else if (currentTime.CompareTo(nightToDawnEnd) >= 0 && currentTime.CompareTo(dayToDuskStart) < 0) {
                phase = DAY;
                if (debug) {
                    Debug.Log("CLOUDCOLOR: Current time fount out to be bewteen fadeOutEnd and fadeInStart \n" +
                                "Phase set to " + phase + " DAY");
                    Debug.Break();
                }
                ChangeColor(0);
            } else if (currentTime.CompareTo(dayToDuskStart) >= 0 && currentTime.CompareTo(sunset) < 0) {
                phase = DAY_TO_DUSK;
                if (debug) {
                    Debug.Log("CLOUDCOLOR: Current time fount out to be bewteen fadeInStart and sunset \n" +
                                "Phase set to " + phase + " DAY_TO_DUSK");
                    Debug.Break();
                }
                ChangeColor(dayToDuskStart.SecondsBetween(sunset));
            } else if (currentTime.CompareTo(sunset) >= 0 && currentTime.CompareTo(dayToDuskEnd) < 0) {
                phase = DUSK_TO_NIGHT;
                if (debug) {
                    Debug.Log("CLOUDCOLOR: Current time fount out to be bewteen fadeInStart and sunset \n" +
                                "Phase set to " + phase + " DAY_TO_DUSK");
                    Debug.Break();
                }
                ChangeColor(sunset.SecondsBetween(dayToDuskEnd));
            } else {
                phase = NIGHT;
                if (debug) {
                    Debug.Log("CLOUDCOLOR: Current time fount out to be past sunset and before midnight \n" +
                                "Phase set to " + phase + " SHINING, \t opacity set to 1");
                    Debug.Break();
                }
                ChangeColor(0);
            }

            previousTime = currentTime.Clone();

        }

        // Update is called once per frame
        void Update() {
            currentTime = clock.getCurrentTime();
            Time sunrise = clock.getSunriseTime();
            Time sunset = clock.getSunsetTime();
            Time dayToDuskEnd = sunset.TimeWithSeconds(sunrise, (int)(endMinutesAfterDawnDusk * 60));
            Time dayToDuskStart = sunset.TimeWithSeconds(sunset, -(int)(startMinutesBeforeDawnDusk * 60));
            Time nightToDawnEnd = sunrise.TimeWithSeconds(sunrise, (int)(startMinutesBeforeDawnDusk * 60));
            Time nightToDawnStart = sunrise.TimeWithSeconds(sunset, -(int)(endMinutesAfterDawnDusk * 60));
            bool completed;
            float increase = 0;
            float decrease = 0;

            switch (phase) {
                case DAY:
                    if (currentTime.CompareTo(dayToDuskStart) >= 0) {
                        phase = DAY_TO_DUSK;
						if(debug){
							Debug.Log("CLOUDCOLOR: Phase changed from DAY to DAY_TO_DUSK");
						}
                        ChangeColor(dayToDuskStart.SecondsBetween(currentTime));
                    }
                    break;
                case NIGHT:
                    if (currentTime.CompareTo(nightToDawnStart) >= 0 && currentTime.CompareTo(dayToDuskStart) < 0) {
                        phase = NIGHT_TO_DAWN;
                        if(debug){
							Debug.Log("CLOUDCOLOR: Phase changed from NIGHT to NIGHT_TO_DAWN");
						}
						ChangeColor(nightToDawnStart.SecondsBetween(currentTime));
                    }
                    break;
                case DAY_TO_DUSK:
                    if (currentTime.CompareTo(sunset) >= 0) {
                        phase = DUSK_TO_NIGHT;
						if(debug){
							Debug.Log("CLOUDCOLOR: Phase changed from DAY_TO_DUSK to DUSK_TO_NIGHT");
						}
                        ChangeColor(sunset.SecondsBetween(currentTime));
                    } else {
						if(debug){
							Debug.Log("CLOUDCOLOR: phase is DAY_TO_DUSK, updating color...");
						}
                        ChangeColor(dayToDuskStart.SecondsBetween(currentTime));
                    }
                    break;
                case DUSK_TO_NIGHT:
                    if (currentTime.CompareTo(dayToDuskEnd) >= 0) {
                        phase = NIGHT;
						if(debug){
							Debug.Log("CLOUDCOLOR: Phase changed from DUSK_TO_NIGHT to NIGHT");
						}
                        ChangeColor(dayToDuskEnd.SecondsBetween(currentTime));
                    } else {
                        ChangeColor(sunset.SecondsBetween(currentTime));
						if(debug){
							Debug.Log("CLOUDCOLOR: phase is DUSK_TO_NIGHT, updating color...");
						}
                    }
                    break;
                case NIGHT_TO_DAWN:
                    if (currentTime.CompareTo(sunrise) >= 0) {
                        phase = DAWN_TO_DAY;
						if(debug){
							Debug.Log("CLOUDCOLOR: Phase changed from NIGHT_TO_DAWN to DAWN_TO_DAY");
						}
                        ChangeColor(sunrise.SecondsBetween(currentTime));
                    } else {
                        ChangeColor(dayToDuskStart.SecondsBetween(currentTime));
						if(debug){
							Debug.Log("CLOUDCOLOR: phase is NIGHT_TO_DAWN, updating color...");
						}
                    }
                    break;
                case DAWN_TO_DAY:
                    if (currentTime.CompareTo(nightToDawnEnd) >= 0) {
                        phase = DAY;
						if(debug){
							Debug.Log("CLOUDCOLOR: Phase changed from DAWN_TO_DAY to DAY");
						}
                        ChangeColor(sunrise.SecondsBetween(currentTime));
                    } else {
                        ChangeColor(dayToDuskStart.SecondsBetween(currentTime));
						if(debug){
							Debug.Log("CLOUDCOLOR: phase is DAWN_TO_DAY, updating color...");
						}
                    }
                    break;
            }

            previousTime = currentTime.Clone();
        }
    }
}
