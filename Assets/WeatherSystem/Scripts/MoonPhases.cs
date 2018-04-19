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
        private Renderer renderer;
        private CentralClock clock;
		private static bool middayReached;
		#endregion Private Atributes
		#region Event System
		//registering on events is done on awake()
		static void MiddayReached(object sender, System.EventArgs e){
			middayReached = true;
            Debug.Log("Moonphases detecta mediodia");
            Debug.Break();
		}
		#endregion Event System
        private void calculatePhaseLengths() {
            if (debug) {
                Debug.Log("CALCULATING MOON PHASES' LENGTH-----------");
            }
            int numberOfPhases = moonPhases.Length;
            phaseLengths = new int[numberOfPhases];
            if (debug) {
                Debug.Log("Moon Cycle Length: " + Cycle_Length + "\n" +
                        "Number of Phases: " + numberOfPhases);
            }
            int length = Cycle_Length / numberOfPhases;
            int leftover = Cycle_Length % numberOfPhases;
            if (debug) {
                Debug.Log("Each phase will have a base length of: " + length + "\n" +
                        "The leftover pending to asign is: " + leftover);
                Debug.Break();
            }
            for (int ind=0; ind < numberOfPhases; ind++) {
                phaseLengths[ind] = length;
            }


            if (debug && leftover > 0) {
                Debug.Log("Leftover is greater than 0, prociding to increase some phases length: ---");
            }
            if (leftover >= 1) {
                phaseLengths[numberOfPhases / 2] += 1;
                leftover--;
                if (debug) {
                    Debug.Log("Increased the length of phase " + numberOfPhases / 2 + "\n" +
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
                    Debug.Log("increased the length of phases " + i + " and " + (numberOfPhases - i) + "\n" +
                            "Remaining days to assign: " + leftover);
                    Debug.Break();
                }
            }
            if (leftover > 0) {
                //check de paranoia
                if (leftover > 1) {
                    Debug.LogError("There is a higher number of leftover then expected: " + leftover);
                }
                phaseLengths[0] += 1;
                leftover--;
                if (debug) {
                    Debug.Log("increased the length of phase 0" + "\n" +
                            "Remaining days to assign: " + leftover);
                    Debug.Break();
                }
            }

            if (debug) {
                Debug.Log("Moon Phases calculations has been completed" + "\n" +
                        "Lengths: ");
                for (int ind = 0; ind < phaseLengths.Length; ind += 2) {
                    Debug.Log("\t" + "Phase " + ind + "\t" + phaseLengths[ind] + "\n" +
                            "\t" + "Phase " + (ind + 1) + "\t" + phaseLengths[ind + 1]);
                }
            }
        }
        private void changeMaterial() {
            //TODO: cambiar material
            renderer.material = moonPhases[phaseIndex];
        }

        void Awake() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }
            renderer = GetComponent<Renderer>();
            if (renderer == null) {
                Debug.LogError("GameObject " + this.name + "Needs a Renderer to make moonPhases visible"); 
                Debug.Break();
            }

            if (Cycle_Length <= 0) {
                Debug.LogError("The Moon Cycle cannot have negative or zero length");
                Debug.Break();
            }
            if (moonPhases.Length > Cycle_Length) {
                Debug.LogError("There cannot be more phases than days in a cycle");
                Debug.Break();
            }
            if (moonPhases.Length % 2 == 1) {
                Debug.LogError("There seems to be an odd number of moonPhases. Please remember that the first phase shoud not be repeated at the end of the cycle.");
                Debug.Break();
            }
            for (int i = 0; i < moonPhases.Length; i++) {
                if (moonPhases[i] == null) {
                    Debug.LogError("Ther ecan not be null moon phase images");
                    Debug.Break();
                }
            }

            calculatePhaseLengths();
            phaseIndex = 0;
            daysSincePhaseChange = 0;
        }
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
			if(middayReached){
				middayReached = false;
				daysSincePhaseChange ++; 
				int phaseLength = phaseLengths[phaseIndex];
				if(debug){
					Debug.Log("Midday reached, updating moon phase data: " +  "\n" +
							"days in this phase: " + daysSincePhaseChange + "\t" + "phase length: " + phaseLength);
				}
				if(daysSincePhaseChange>phaseLength){
					Debug.LogError("Current moon phase has lasted longer than it should have, please revise MoonPhases' update function");
					Debug.Break();
				}
				if(daysSincePhaseChange == phaseLength){
					phaseIndex ++;
					daysSincePhaseChange = 0;
					changeMaterial();
				}
			}
        }
    }
}