using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class WeatherController : MonoBehaviour {
        #region public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug; 

		[Header ("Symulation parameters:")]
		public int update_frequency = 60;
		public double previous_state_influence;
		public double biome_influence;
		
		[Header ("Map settings:")]
		public Vector3 originPosition;
		public Vector3 endPosition;
		public int chunkSize;
		public Biome[][] biomeDistribution;
		
		[Header ("Random Generation Values:")]
		public float probability_stdDev;
        public float Intensity_stdDev;
		public float temperature_stdDev;
		
		[Header ("Weather Visualization (Particle Systems):")]
        public ParticleSystem rainSystem;
        public ParticleSystem snowSystem;
        public ParticleSystem lightningSystem;
        //TODO: falta visualizacion de las nubes

		#endregion public Atributes
		
		
        #region private Atributes
        private CentralClock clock;
        private Time lastUpdate;
		private WeatherState currentState;
		private WeatherState nextState;
		private WeatherState previousState;
		private Biome[][] biomes;
        #endregion private Atributes 

		private WeatherState GetNextState(WeatherState currentState, Biome biome){
            float temperature = currentState.GetTemperature() + GenerateNormalRandom(0, temperature_stdDev );
            int clouds = currentState.GetClouds() + (int) GenerateNormalRandom(0, Intensity_stdDev);
            int rain = currentState.GetRain() + (int)GenerateNormalRandom(0, Intensity_stdDev);
            int snow = currentState.GetSnow() + (int)GenerateNormalRandom(0, Intensity_stdDev);
            int lightning = currentState.GetLightning() + (int)GenerateNormalRandom(0, Intensity_stdDev);
            return new WeatherState(temperature, clouds, rain, snow, lightning);
		}
 
        public static float GenerateNormalRandom(float mu, float sigma) {
            float rand1 = Random.Range(0.0f, 1.0f);
            float rand2 = Random.Range(0.0f, 1.0f);

            float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

            return (mu + sigma * n);
        }
		
		private Biome createDesertBiome(){
            //NOTE: this values need to be modified acording to user preference. Also, remember to add as many lines as seasons on the system, and even more important, the identifier in the season script needs to match the index in theese tables.
            int[,] intensities = new int[4,4] {{ 3, 1, 0, 0 },{ 4, 1, 0, 0 }, { 4, 1, 0, 0 }, { 4, 1, 0, 0 } }; //for every season: clouds, rain, lighning, snow. //season order: winter, spring, summer, autumn
            double[,] probabilities = new double [4, 4] {{1,0.1,0.1,0.0}, {1,0.1,0.1,0.0 }, {1,0.1,0.1,0.0 }, {1,0.2,0.1,0.0 } };
            int[,] temperatures = new int[4, 2] { { 16, 40 }, { 16, 40}, { 16, 40}, { 16, 40 } };
            Biome result = new Biome(probabilities, intensities, temperatures);
			return result;
		}

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
                Debug.LogError("Weather controller needs a snow system reference");
            }
            if (lightningSystem == null) {
                Debug.LogError("Weather controller needs a lighning system reference");
            }
			
			currentState = new WeatherState(20.0f, 8, 2, 0, 0);
        }

        // Use this for initialization
        void Start() {
            lastUpdate = clock.getCurrentTime().Clone();
        }

        // Update is called once per frame
        void Update() {
            Time currentTime = clock.getCurrentTime();
            if(lastUpdate.SecondsBetween(currentTime)>= (update_frequency * 60)) {
                WeatherState next = GetNextState(currentState, biome);//TODO: aqui se llama a metodo para calcular nuevo estado
                if (debug) {
                    Debug.Log("new weather State: \n" + next.ToString());
                    Debug.Break();
                }
                lastUpdate = currentTime.Clone();
            }
          

        }
    }
}
