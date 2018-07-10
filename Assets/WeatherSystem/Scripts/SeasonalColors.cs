using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class SeasonalColors : MonoBehaviour {
		
		#region Constants
        const int R = 0;
        const int G = 1;
        const int B = 2;
        const int A = 3;
		
		const int TRANSITION_IN = 0;
        const int WAITING = 1;
		
        #endregion Constants
        
        #region Public atributes
        [Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
        public bool debug;

        public int transiotion_days = 10;
		public Color[] colors;
		
        #endregion  Public atributes

        #region Private Atributes
        private CentralClock clock;
        private Renderer renderer;
        private int phase; //valor int igual al indice de la estacion, se tien que obtener de season.next
        private Time previousTime;
        private Time currentTime;
		private Season currentSeason;
		private Season previousSeason;

        private float[,] transitionValues;
        #endregion Private Atributes

        private void ChangeColor(int days, int seconds){
			Color result = renderer.material.color;
			
			switch(phase){
				case WAITING: 
					result = colors[currentSeason.GetIndex()];
					break;
                case TRANSITION_IN:
                    int previousSeasonIndex = previousSeason.GetIndex();
					result.r = colors[previousSeasonIndex].r + transitionValues[previousSeasonIndex, R] * (days*86400 + seconds);
					result.g = colors[previousSeasonIndex].g + transitionValues[previousSeasonIndex, G] * (days*86400 + seconds);
					result.b = colors[previousSeasonIndex].b + transitionValues[previousSeasonIndex, B] * (days*86400 + seconds);
					result.a = colors[previousSeasonIndex].a + transitionValues[previousSeasonIndex, A] * (days*86400 + seconds);

                    if (debug) {
                        Debug.Log("TRANSITIONING A NEW COLOR, phase = " + phase + ": TRANSITIONING_IN");
                        Debug.Log("previous season index: " + previousSeasonIndex + "\n" +
                                "Current season:" + currentSeason.GetIndex() + currentSeason.ToString() +
                                "previous color: " + colors[previousSeasonIndex] + "\n" + 
                                "next color: " + colors[currentSeason.GetIndex()]);
                        Debug.Break();
                    }

                    break;
			}
			renderer.material.color = result;
            if (debug) {
                Debug.Log(result);
            }
		}

        //internal altributes initialization
        private void Awake() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("SEASONALCOLOR " + this.name + ": No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }

            renderer = GetComponent<Renderer>();
            if (renderer == null) {
                Debug.LogError("SEASONALCOLOR: GameObject " + this.name + "Needs a Renderer to make moonPhases visible");
                Debug.Break();
            }

            Season[] s = GetSeasons(); //TODO: estan ya en la posicion seasonIndex

            //TODO: comprobar que los dias de transicion son menores o iguales que la duracion de la estacion mas corta.

            transitionValues = new float[colors.Length, 4];
            // calculando los valores de transicion por segundo
            for (int i = 0; i < colors.Length; i++) {
                transitionValues[i, R] = (colors[s[i].next_season.GetIndex()].r - colors[i].r) / (transiotion_days * 24 * 3600);
                transitionValues[i, G] = (colors[s[i].next_season.GetIndex()].g - colors[i].g)/ (transiotion_days * 24 * 3600);
                transitionValues[i, B] = (colors[s[i].next_season.GetIndex()].b - colors[i].b)/ (transiotion_days * 24 * 3600);
                transitionValues[i, A] = (colors[s[i].next_season.GetIndex()].a - colors[i].a)/ (transiotion_days * 24 * 3600);

                if (debug) {
                    Debug.Log("Transition values for seasosn: " + i + s[i].name);
                    Debug.Log("color on season: " + colors[i] );
                    Debug.Log("Color on next season: " + colors[s[i].next_season.GetIndex()]);
                    Debug.Log("TRANSITION VALUES: \n " +
                        "\r R : " + transitionValues[i, R] + "\t " + ((colors[s[i].next_season.GetIndex()].r - colors[i].r)  + "\t " +  (transiotion_days * 24 * 3600)) + "\n" +
                        "\r G : " + transitionValues[i, G] + "\t " + ((colors[s[i].next_season.GetIndex()].g - colors[i].g) + "\t" + (transiotion_days * 24 * 3600)) + "\n" +
                        "\r B : " + transitionValues[i, B] + "\t " + ((colors[s[i].next_season.GetIndex()].b - colors[i].b) + "\t" +  (transiotion_days * 24 * 3600)) + "\n" +
                        "\r A : " + transitionValues[i, A] + "\t " + ((colors[s[i].next_season.GetIndex()].a - colors[i].a) + "\t" + (transiotion_days * 24 * 3600)) + "\n");
                }
            }
        }

		
		private Season[] GetSeasons(){
			GameObject[] s = GameObject.FindGameObjectsWithTag("Season");
			Season[] result = new Season[s.Length];
			for (int i = 0; i<s.Length; i++){
				Season season = s[i].GetComponent<Season>();
				if(season == null){
					Debug.LogError("GameObject "+ s[i].name + " is tagged as Season, but it does not have a component with the Season script");
					Debug.Break();
				}else{
					result[season.GetIndex()] = season;
				}
			}
			return result;
		}
		
		private Season GetPreviousSeason(Season s){
            
            Season result = currentSeason;
            Season[] seasons = GetSeasons();
            for (int i = 0; i<seasons.Length; i++) {
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


        // Use this for initialization
        void Start() {
           
			Date currentDate = clock.getCurrentDate();
            GetInitialSeason(currentDate);
            currentTime = clock.getCurrentTime();

            if (debug) {
                Debug.Log(this.name);
                Debug.Log("Current season: " + currentSeason);
                Debug.Log("start date: " + currentSeason.GetStartDate());
                Debug.Log("current date: "+ currentDate);
                Debug.Log("current time: " + currentTime);
            }

            Date transitionInEnd= currentSeason.GetStartDate().DateWithDays(transiotion_days);

            if (debug) {
                Debug.Log("TransitionIn end: " + transitionInEnd );
            }
			
			//TODO: transiotion_days/2 es resultado de division entera, si se mete un numero de dias impar faltará un dia en la transicion. habra que hacer comprobaciones o algo, pero no antes de la presentacion, que hay prisa
			if(currentDate.CompareTo(transitionInEnd)<0){
                phase = TRANSITION_IN;
				previousSeason = GetPreviousSeason(currentSeason); //TODO: hay que implementar el modo de recuperar la estacion anterior
				ChangeColor(currentSeason.GetStartDate().DaysBetween(currentDate), currentTime.GetHours() * 3600 + currentTime.GetMinutes() * 60 + currentTime.GetSeconds());
                if (debug) {
                    Debug.Log(" initial state detected to be transitioning in from season " + previousSeason.name);
                    Debug.Log("time since the season change: " + (currentSeason.GetStartDate().DaysBetween(currentDate) ) + " days and " + (currentTime.GetHours() * 3600 + currentTime.GetMinutes() * 60 + currentTime.GetSeconds()) + "Seconds");
                    Debug.Break();
                }
				
            }
			else{
				phase = WAITING;
				previousSeason = currentSeason;
                ChangeColor(0, 0);
                if (debug) {
                    Debug.Log(" initial state detected to be waiting to season " + currentSeason.ToString());
                }
            }
			

        }

        // Update is called once per frame
        void Update() {
            currentTime = clock.getCurrentTime();
            Date currentDate = clock.getCurrentDate();
            currentSeason = clock.GetSeason();

            Date transitionInEnd= currentSeason.GetStartDate().DateWithDays(transiotion_days);


			switch(phase){
				case TRANSITION_IN: 
					if(currentDate.CompareTo(transitionInEnd)>0){
						phase = WAITING;
                        if (debug) {
                            Debug.Log(" detected the end of the transitioning in phase, now waiting in season:  " + currentSeason.ToString());
                            Debug.Log("time since the season change: " + (transitionInEnd.DaysBetween(currentDate)) + " days and " + (currentTime.GetHours() * 3600 + currentTime.GetMinutes() * 60 + currentTime.GetSeconds()) + "Seconds");
                            Debug.Break();
                        }
                        ChangeColor(0, 0);
                        previousSeason = currentSeason;

					}else{
                        if (debug) {
                            Debug.Log(" transitioning into season " + currentSeason);
                            Debug.Log("time since the season change: " + (currentSeason.GetStartDate().DaysBetween(currentDate)) + " days and " + (currentTime.GetHours() * 3600 + currentTime.GetMinutes() * 60 + currentTime.GetSeconds()) + "Seconds");
                            Debug.Break();
                        }
                        ChangeColor(currentSeason.GetStartDate().DaysBetween(currentDate), currentTime.GetHours()*3600 + currentTime.GetMinutes()*60 + currentTime.GetSeconds());
					}
					break;
				case WAITING:
                    //if (debug) {
                    //    Debug.Log("current season: " + currentSeason + "\t  previousSeason :" + previousSeason + "\n" + 
                    //       "index match: " + (currentSeason.GetIndex() != previousSeason.GetIndex()));
                    //    Debug.Break();
                    //}
                    if (currentSeason.GetIndex()!=previousSeason.GetIndex()){
						phase = TRANSITION_IN;
                        if (debug) {
                            Debug.Log(" reached end of season: \n" +
                                "current season: " + currentSeason.ToString() + "\t  previousSeason :" + previousSeason.ToString() +
                                "\n phase: " + phase);
                            Debug.Break();
                        }
                        ChangeColor(currentSeason.GetStartDate().DaysBetween(currentDate),currentTime.GetHours()*3600 + currentTime.GetMinutes()*60 + currentTime.GetSeconds());
					}
					break;
			}	
        }
    }
}
