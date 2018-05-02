using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace weatherSystem {
    public class MoonPhases : MonoBehaviour {
        
		#region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
        public bool debug;

        [Header("Moon Cycle Setup")]
        [Tooltip("number of days it will take to complete a full moon cycle")]
        public int Cycle_Length;
        [Tooltip("introduce the moon phase materials in order, each image shoud be the following stage to its predecessor")]
        public Material[] moonPhases;
		#endregion Public Atributes
		
        #region Private Atributes
        private int phaseIndex;
        private int[] phaseLengths;
        private int daysSincePhaseChange;
        private new MeshRenderer renderer;
        private CentralClock clock;
		private static bool middayReached;
        private Time previousTime = new Time(0,0,0);
        private Time currentTime = new Time(0, 0, 0);
        #endregion Private Atributes

        private void calculatePhaseLengths() {
            if (debug) {
                Debug.Log("MOONPHASES: CALCULATING PHASES LENGTH-----------");
            }
            int numberOfPhases = moonPhases.Length;
            phaseLengths = new int[numberOfPhases];
            if (debug) {
                Debug.Log("MOONPHASES: Moon Cycle Length: " + Cycle_Length + "\n" +
                        "Number of Phases: " + numberOfPhases);
            }
            int length = Cycle_Length / numberOfPhases;
            int leftover = Cycle_Length % numberOfPhases;
            if (debug) {
                Debug.Log("MOONPHASES: Each phase will have a base length of: " + length + "\n" +
                        "The leftover pending to asign is: " + leftover);
                Debug.Break();
            }
            for (int ind=0; ind < numberOfPhases; ind++) {
                phaseLengths[ind] = length;
            }


            if (debug && leftover > 0) {
                Debug.Log("MOONPHASES: Leftover is greater than 0, prociding to increase some phases length: ---");
            }
            if (leftover >= 1) {
                phaseLengths[numberOfPhases / 2] += 1;
                leftover--;
                if (debug) {
                    Debug.Log("MOONPHASES: Increased the length of phase " + numberOfPhases / 2 + "\n" +
                            "Remaining days to assign: " + leftover);
                    Debug.Break();
                }
            }
            int i = (numberOfPhases / 2) - 1;
            while (leftover >= 2 && i > 0) {
                phaseLengths[i] += 1;
                phaseLengths[numberOfPhases - i] += 1;
                leftover -= 2;
                i--;
                if (debug) {
                    Debug.Log("MOONPHASES: increased the length of phases " + i + " and " + (numberOfPhases - i) + "\n" +
                            "Remaining days to assign: " + leftover);
                    Debug.Break();
                }
            }
            if (leftover > 0) {
                //check de paranoia
                if (leftover > 1) {
                    Debug.LogError("MOONPHASES: There is a higher number of leftover then expected: " + leftover);
                }
                phaseLengths[0] += 1;
                leftover--;
                if (debug) {
                    Debug.Log("MOONPHASES: increased the length of phase 0" + "\n" +
                            "Remaining days to assign: " + leftover);
                    Debug.Break();
                }
            }

            if (debug) {
                Debug.Log("MOONPHASES: Moon Phases calculations has been completed" + "\n" +
                        "Lengths: ");
                for (int ind = 0; ind < phaseLengths.Length; ind += 2) {
                    Debug.Log("\t" + "Phase " + ind + "\t" + phaseLengths[ind] + "\n" +
                            "\t" + "Phase " + (ind + 1) + "\t" + phaseLengths[ind + 1]);
                }
            }
        }
        private void changePhase() {
            phaseIndex = (phaseIndex + 1)%phaseLengths.Length;
            daysSincePhaseChange = 1;
            renderer.material = moonPhases[phaseIndex];
            if (debug) {
                Debug.Log("MOONPHASES: phase changed. new phase: " + phaseIndex);
                Debug.Break();
            }
        }

        void Awake() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("MOONPHASES: No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }
            renderer = GetComponent<MeshRenderer>();
            if (renderer == null) {
                Debug.LogError("MOONPHASES: GameObject " + this.name + "Needs a Renderer to make moonPhases visible"); 
                Debug.Break();
            }

            if (Cycle_Length <= 0) {
                Debug.LogError("MOONPHASES: The Moon Cycle cannot have negative or zero length");
                Debug.Break();
            }
            if (moonPhases.Length > Cycle_Length) {
                Debug.LogError("MOONPHASES: There cannot be more phases than days in a cycle");
                Debug.Break();
            }
            if (moonPhases.Length % 2 == 1) {
                Debug.LogError("MOONPHASES: There seems to be an odd number of moonPhases. Please remember that the first phase shoud not be repeated at the end of the cycle.");
                Debug.Break();
            }
            for (int i = 0; i < moonPhases.Length; i++) {
                if (moonPhases[i] == null) {
                    Debug.LogError("MOONPHASES: Ther ecan not be null moon phase images");
                    Debug.Break();
                }
            }

            calculatePhaseLengths();
            phaseIndex = 0;
            daysSincePhaseChange = 1;
            renderer.material = moonPhases[0];
            
        }
        // Use this for initialization
        void Start() {
            currentTime = clock.getCurrentTime();
            if (currentTime == null) {
                Debug.LogError("MOONPHASES: MoonPhases currentTime is null");
            }
        }

        // Update is called once per frame
        void Update() {
           
            Time midday = clock.GetMiddayTime();
            currentTime = clock.getCurrentTime();
            if(previousTime.CompareTo(midday)<0 && currentTime.CompareTo(midday) >= 0) {
                if (debug) {
                    Debug.Log("MOONPHASES: midday reached");
                    Debug.Break();
                }
                daysSincePhaseChange++;
                if (daysSincePhaseChange > phaseLengths[phaseIndex]) {
                    changePhase();
                }
            }
            previousTime = currentTime.Clone();
        }
    }
}