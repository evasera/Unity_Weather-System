using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class ModelSwap : MonoBehaviour {
		
		        
        #region Public atributes
        [Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
        public bool debug;

		public GameObject[] models;
        #endregion  Public atributes

        #region Private Atributes
        private CentralClock clock;
        private Season currentSeason;
		private Season previousSeason;
        #endregion Private Atributes


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

        //internal altributes initialization
        private void Awake() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("SEASONALCOLOR " + this.name + ": No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }
        }	
		
        // Use this for initialization
        void Start() {
            
			GetInitialSeason(clock.getCurrentDate());
            if (debug) {
                Debug.Log("initial season: " + currentSeason);
            }
			for(int i = 0; i<models.Length; i++){
				if(currentSeason.GetIndex() == i){
					models[i].SetActive(true);
                    if (debug) {
                        Debug.Log("Model in position " +  i + "set to active" );
                    }
                }
				else{
					models[i].SetActive(false);
                    if (debug) {
                        Debug.Log("Model in position " + i + "set to not active");
                    }
                }
			}
			
			previousSeason = currentSeason;
        }

        // Update is called once per frame
        void Update() {
            //Debug.Break();
			currentSeason = clock.GetSeason();
            if(!(currentSeason.GetIndex() == previousSeason.GetIndex())){
                if (debug) {
                    Debug.Log("New Season detected, updating models.....");
                }
                for (int i = 0; i<models.Length; i++){
					if(currentSeason.GetIndex() == i){
						models[i].SetActive(true);
                        if (debug) {
                            Debug.Log("Model in position " + i + "set to active");
                        }
                    }
					else{
						models[i].SetActive(false);
                        if (debug) {
                            Debug.Log("Model in position " + i + "set to  not active");
                        }
                    }
				}
                if (debug) {
                    Debug.Break();
                 
                }
                previousSeason = currentSeason;
			}
        }
		
		
    }
}
