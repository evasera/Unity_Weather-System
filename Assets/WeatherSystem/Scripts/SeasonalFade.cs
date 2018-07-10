using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace weatherSystem {
    public class SeasonalFade : MonoBehaviour{		
		
		#region Constants
		const int FADED = 0;
        const int FADING_IN = 1;
        const int SHINING = 2; //todo: renombrar a visible
        const int FADING_OUT = 3;
        #endregion Constants

        #region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
		[Tooltip("In game minutes the satrs will take to transition from transparent to fully visible, no matter the amount")]
		public float minutesToFade = 60;
		public Season fade_start;
		public Season fade_end;
		#endregion Public Atributes
		
		#region Private Atributes
		private CentralClock clock;
		private new MeshRenderer renderer;
		//private bool daytime;
		private int phase;
		private float opacityChangePerSecond;
		private Time currentTime;
		private Season currentSeason;
        private Season previousSeason;
		private Time previousTime;
        private Renderer rend;
		#endregion Private Atributes
		
		
		private void setOpacity(float opacity){
			Color color;
			//rend.material.color = color;
           // Material[] m = rend.materials;
            Material[] m = rend.sharedMaterials;
            //if (debug) {
            //    Debug.Log("Retrieved " + m.Length + " materials");
            //}
            for(int i = 0; i<m.Length; i++) {
                color = m[i].color;
                color.a = opacity;
                m[i].color = color;
            }
		}

        private Season[] GetSeasons() {
            GameObject[] s = GameObject.FindGameObjectsWithTag("Season");
            Season[] result = new Season[s.Length];
            for (int i = 0; i < s.Length; i++) {
                Season season = s[i].GetComponent<Season>();
                if (season == null) {
                    Debug.LogError("GameObject " + s[i].name + " is tagged as Season, but it does not have a component with the Season script");
                    Debug.Break();
                } else {
                    result[season.GetIndex()] = season;
                }
            }
            return result;
        }

        private Season GetPreviousSeason(Season s) {
            Season result = null;
            Season[] seasons = GetSeasons();
            for (int i = 0; i < seasons.Length; i++) {
                if (seasons[i].GetNextSeason().GetIndex() == s.GetIndex()) {
                    return seasons[i];
                }
            }
            return result;
        }

        void GetInitialSeason(Date currentDate) {
            GameObject[] s = GameObject.FindGameObjectsWithTag("Season");
            bool[] indexesUsed = new bool[s.Length];
            if (s.Length == 0) {
                Debug.LogError("CENTRALCLOCK: No object with tag 'Season' found");
            }
            for (int i = 0; i < s.Length; i++) {
                Season aux = s[i].GetComponent<Season>();
                int index = aux.GetIndex();
                if (index >= s.Length) {
                    Debug.LogError("CENTRALCLOCK: Season " + s + "has an index number to high. \n Please Remember that season indexes need to start in 0 and be consecutive");
                } else {
                    if (indexesUsed[i]) {
                        Debug.LogError("CENTRALCLOCK: Season index " + i + " is duplicated");
                    } else {
                        indexesUsed[i] = true;
                    }
                }
                if (aux == null) {
                    Debug.LogError("CENTRALCLOCK: GameObject " + s[i].name + "has 'Season' tag but no Season script");
                }
                if (aux.GetEndDate().CompareTo(aux.GetStartDate()) < 0) {
                    if (currentDate.CompareTo(aux.GetStartDate()) >= 0 || currentDate.CompareTo(aux.GetEndDate()) < 0) {
                        currentSeason = aux;
                        break;
                    }
                } else if (currentDate.CompareTo(aux.GetStartDate()) >= 0 && currentDate.CompareTo(aux.GetEndDate()) < 0) {
                    currentSeason = aux;
                    break;
                }
            }
            if (currentSeason == null) {
                Debug.LogError("CENTRALCLOCK: Initial season could not be found");
            }
        }



        void Awake(){
			clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("SEASONALFADE " + this.name + ": No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }
			
			if(minutesToFade <0){
				Debug.LogError("SEASONALFADE " + this.name + ": The minutesToFade parameter can not be negative");
			}
			
			renderer = GetComponent<MeshRenderer>();
            if (renderer == null) {
                Debug.LogError("SEASONALFADE: GameObject " + this.name + "Needs a Renderer to make moonPhases visible"); 
                Debug.Break();
            }

            rend = GetComponent<Renderer>();
            if(rend == null) {
                Debug.LogError("SEASONALFADE: GameObject containing the script needs a renderer");
            }

            opacityChangePerSecond = 1.0f / ((float)minutesToFade * 60);
            if (debug) {
                Debug.Log("OPACITY CHANGE PER SECOND: " + opacityChangePerSecond + "\n" +
                        "CURRENT OPACITY: " + rend.material.color.a);
            }
        }

        
		void Start(){

            if (debug) {
                Debug.Log(this.name);
            }
			currentTime = clock.getCurrentTime();
            Date currentDate = clock.getCurrentDate();
            GetInitialSeason(currentDate);
            

            Time midnight = clock.GetMidnightTime();


            if(currentSeason.GetStartDate().CompareTo(fade_start.GetStartDate()) >=0  && currentSeason.GetEndDate().CompareTo(fade_end.GetEndDate()) < 0) {
                if (currentDate.Equals(fade_start.GetStartDate())) {
                    if (midnight.SecondsBetween(currentTime) < minutesToFade*60) {
                        phase = FADING_IN;
                        float opacity = midnight.SecondsBetween(currentTime) * opacityChangePerSecond;
                        if (debug) {
                            Debug.Log("initial phase determined to be: " + phase + " FADING_IN \n" + 
                                "Seconds since midnight: " + midnight.SecondsBetween(currentTime) + 
                                "opacity per second: " + opacityChangePerSecond + 
                                 "Opacity set to: " + opacity);
                        }
                        setOpacity(opacity);
                    } else {
                        phase = SHINING;
                        if (debug) {
                            Debug.Log("initial phase determined to be: " + phase + " SHINING \n" +
                                  "Opacity set to: " + 1);
                        }
                        setOpacity(1);
                    }
                } else {
                    phase = SHINING;
                    if (debug) {
                        Debug.Log("initial phase determined to be: " + phase + " SHINING \n" +
                              "Opacity set to: " + 1);
                    }
                    setOpacity(1);
                }
            }else if (currentDate.Equals(fade_end.GetStartDate())){
                if (midnight.SecondsBetween(currentTime) < minutesToFade * 60) {
                    phase = FADING_OUT;
                    float opacity = 1 - midnight.SecondsBetween(currentTime) * opacityChangePerSecond;
                    if (debug) {
                        Debug.Log("initial phase determined to be: " + phase + " FADING_OUT \n" +
                              "Opacity set to: " + opacity);
                    }
                    setOpacity(opacity);
                } else {
                    phase = FADED;
                    if (debug) {
                        Debug.Log("initial phase determined to be: " + phase + " FADED \n" +
                              "Opacity set to: " + 0);
                    }
                    setOpacity(0);
                }

            } else {
                phase = FADED;
                if (debug) {
                    Debug.Log("initial phase determined to be: " + phase + " FADED \n" +
                          "Opacity set to: " + 0);
                }
                setOpacity(0);
            }

            previousTime = currentTime.Clone();
            previousSeason = currentSeason;
		}
		
		void Update(){
			currentTime = clock.getCurrentTime();
            currentSeason = clock.GetSeason();
            float opacity;
            Time midnight = clock.GetMidnightTime();

            switch (phase) {
                case FADING_IN: 
                    if(midnight.SecondsBetween(currentTime)>= minutesToFade * 60){
                        phase = SHINING;
                        if (debug) {
                            Debug.Log("end of the fasing_in period detected, phase set to " + phase + " SHINING \n" +
                            "Seconds since midnight: " + midnight.SecondsBetween(currentTime) +
                            "opacity per second: " + opacityChangePerSecond +
                             "Opacity set to: " + 1);
                            Debug.Break();
                        }
                        setOpacity(1);
                    } else {
                        opacity = midnight.SecondsBetween(currentTime) * opacityChangePerSecond;
                        setOpacity(opacity);
                    }
                    break;
                case FADING_OUT:
                    if (midnight.SecondsBetween(currentTime) >= minutesToFade * 60) {
                        phase = FADED;
                        if (debug) {
                            Debug.Log("end of the fasing_out period detected, phase set to " + phase + " FADED \n" +
                            "Seconds since midnight: " + midnight.SecondsBetween(currentTime) +
                            "opacity per second: " + opacityChangePerSecond +
                             "Opacity set to: " + 0);
                            Debug.Break();
                        }
                        setOpacity(0);
                    } else {
                        opacity = 1 - midnight.SecondsBetween(currentTime) * opacityChangePerSecond;
                        setOpacity(opacity);
                    }
                    break;
                case SHINING:
                    if (previousSeason != currentSeason) {
                        previousSeason = currentSeason;
                        if (currentSeason.GetIndex() == fade_end.GetIndex()) {
                            phase = FADING_OUT;
                            opacity = 1 - midnight.SecondsBetween(currentTime) * opacityChangePerSecond;
                            if (debug) {
                                Debug.Log("Fading_out season start detected, phase set to " + phase + " FADING_OUT \n" +
                                "Seconds since midnight: " + midnight.SecondsBetween(currentTime) +
                                "opacity per second: " + opacityChangePerSecond +
                                 "Opacity set to: " + opacity);
                                Debug.Break();
                            }
                            setOpacity(opacity);
                        }
                    }
                    break;
                case FADED:
                    if (previousSeason != currentSeason) {
                        previousSeason = currentSeason;
                        if (currentSeason.GetIndex() == fade_start.GetIndex()) {
                            phase = FADING_IN;
                            opacity = midnight.SecondsBetween(currentTime) * opacityChangePerSecond;
                            if (debug) {
                                Debug.Log("Fading in season start detected, phase set to " + phase + " FADING_IN \n" +
                                "Seconds since midnight: " + midnight.SecondsBetween(currentTime) +
                                "opacity per second: " + opacityChangePerSecond +
                                 "Opacity set to: " + opacity);
                                Debug.Break();
                            }
                            setOpacity(opacity);
                        }
                    }
                    break;
            }

			previousTime = currentTime.Clone();
		}		
    }
}
