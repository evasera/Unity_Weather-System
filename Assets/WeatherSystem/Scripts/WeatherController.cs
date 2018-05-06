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
		public float previous_state_influence;
		public float biome_influence;
		
		[Header ("Map settings:")]
		public Vector3 originPosition;
		public Vector3 endPosition;
		public int chunkSize;
		public Biome[,] biomeDistribution;
		
		[Header ("softening settings:")]
		public double samePosition = 0.4;
		public double directlyAdjacent = 0.1;
		public double diagonalPosition= 0.05;
		
		[Header ("Random Generation Values:")]
		public float probability_stdDev;
        public float intensity_stdDev;
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
		private Season currentSeason;
		private WeatherState [,] currentState;
		private WeatherState [,] nextState;
		private WeatherState [,] previousState;
        #endregion private Atributes 

		private WeatherState[,] GetNextState(){
            WeatherState[,] result = new WeatherState[currentState.GetLength(0), currentState.GetLength(1)]; 
			int seasonIndex = currentSeason.GetIndex(); //it is common for all the map
			for(int i = 0; i<result.GetLength(0); i++){
				for(int j = 0; j<result.GetLength(1); j++){
					Biome biome = biomeDistribution[i,j];
					WeatherState previous = currentState[i, j];
					WeatherState current = new WeatherState(0,0,0,0,0);
					
					//TODO: ¿añadir la intensidad de las nubes a los calculos de probabilidad de la simulacion? (lluvia, nieve, etcetera etcetera )
					
					//New State cloud calculations
					//The probability of some clouds is 100%, so no probability calculated for clouds
					int cloudIntensity = (int) (previous.GetClouds()*previous_state_influence + biome.GetIntensity(Biome.CLOUDS, seasonIndex)*biome_influence + GenerateNormalRandom(0, intensity_stdDev));
					cloudIntensity = Mathf.Min(10, Mathf.Max(0, cloudIntensity));
					
					
					//New State Rain calculations
					//probability:
					float rainProbability = (float) ((previous.GetRain()/10.0)*previous_state_influence + biome.GetProbability(Biome.RAIN, seasonIndex)*biome_influence + GenerateNormalRandom(0, probability_stdDev));
                    rainProbability = Mathf.Max(0, Mathf.Min(1, rainProbability));
                    //intensity
                    int rainIntensity = 0;

                    if (Random.value > rainProbability){
						rainIntensity = (int)(previous.GetRain()*previous_state_influence + biome.GetIntensity(Biome.RAIN, seasonIndex)*biome_influence);
						rainIntensity+= (int) GenerateNormalRandom(0, intensity_stdDev);
						// the rain intensity is at most as big as the clouds allow
						rainIntensity = Mathf.Min(cloudIntensity, Mathf.Max(0, rainIntensity)); 
						
					}					
					//New State Snow calculations
					//probability:
					float snowProbability = (float)((previous.GetSnow()/10.0)*previous_state_influence + biome.GetProbability(Biome.SNOW, seasonIndex)*biome_influence + GenerateNormalRandom(0, probability_stdDev));
					snowProbability = Mathf.Max(0, Mathf.Min(1, snowProbability));
                    //intensity
                    int snowIntensity = 0;
                    if (Random.value > snowProbability){
						snowIntensity = (int) (previous.GetSnow()*previous_state_influence + biome.GetIntensity(Biome.SNOW, seasonIndex)*biome_influence + GenerateNormalRandom(0, intensity_stdDev));
						// the snow intensity is at most as big as the clouds allow
						snowIntensity = Mathf.Min(cloudIntensity, Mathf.Max(0, snowIntensity)); 
						
					}
					//coherence test, no rain and snow at the same time:
					if(rainIntensity > 0 && snowIntensity >0){
						if(rainProbability>snowProbability){
							snowIntensity = 0;
						}else{
							rainIntensity = 0;
						}
					}
					
					//New State storm calculations
					int stormIntensity = 0;
					if(rainIntensity>=5){
						//probability
						float stormProbability = (float) (((previous.GetLightning()/10.0)*0.4 + (rainIntensity/10)*0.6 )* previous_state_influence + biome.GetProbability(Biome.LIGHTNING, seasonIndex)*biome_influence + GenerateNormalRandom(0.0f, probability_stdDev));
						stormProbability = Mathf.Max(0, Mathf.Min (1, rainIntensity));
						
						if(Random.value>stormProbability){
							stormIntensity = (int) (previous.GetLightning()*previous_state_influence + biome.GetIntensity(Biome.LIGHTNING, seasonIndex)*biome_influence + GenerateNormalRandom(0.0f, intensity_stdDev));
							stormIntensity = Mathf.Min(rainIntensity, Mathf.Max (0, stormIntensity));
						}
					}					
					
					current.SetClouds(cloudIntensity);
					current.SetRain(rainIntensity);
					current.SetSnow(snowIntensity);
					current.SetLightning(stormIntensity);
					
					result[i,j] = current;
				}
			}
			//Softening phase, to avoid drastic changes in the weather.
			result = Softening(result);
            return result;
		}
		
		public WeatherState[,] Softening (WeatherState[,] initialState){
            int size0 = initialState.GetLength(0);
			int size1 = initialState.GetLength(1);
			WeatherState[,] result = new WeatherState[size0, size1];
			for(int i= 0; i<size0; i++){
				for(int j= 0; j<size1; j++){
					if(i == 0){
						if(j==0){ //esquina sup.izq
							result[i,j].SetClouds((int)( initialState[i, j].GetClouds()*samePosition + 2*(initialState[i+1, j].GetClouds() + initialState[i, j+1].GetClouds())*directlyAdjacent + 4*initialState[i+1, j+1].GetClouds()*diagonalPosition));
							result[i,j].SetRain((int)( initialState[i, j].GetRain()*samePosition + 2*(initialState[i+1, j].GetRain() + initialState[i, j+1].GetRain())*directlyAdjacent + 4*initialState[i+1, j+1].GetRain()*diagonalPosition));
							result[i,j].SetSnow((int)( initialState[i, j].GetSnow()*samePosition + 2*(initialState[i+1, j].GetSnow() + initialState[i, j+1].GetSnow())*directlyAdjacent + 4*initialState[i+1, j+1].GetSnow()*diagonalPosition));
							result[i,j].SetLightning((int)( initialState[i, j].GetLightning()*samePosition + 2*(initialState[i+1, j].GetLightning() + initialState[i, j+1].GetLightning())*directlyAdjacent + 4*initialState[i+1, j+1].GetLightning()*diagonalPosition));
							//TODO: falta comprobar queno haya nieve y lluvia a la vez
						}else if(j == size1){ // esquina sup derecha
							result[i, j].SetClouds((int)( initialState[i, j].GetClouds()*samePosition + 2*(initialState[i+1, j].GetClouds() + initialState[i, j++].GetClouds())*directlyAdjacent + 4*initialState[i+1, j-1].GetClouds()*diagonalPosition  ));
							result[i, j].SetRain((int)( initialState[i, j].GetRain()*samePosition + 2*(initialState[i+1, j].GetRain() + initialState[i, j++].GetRain())*directlyAdjacent + 4*initialState[i+1, j-1].GetRain()*diagonalPosition  ));
							result[i, j].SetSnow((int)( initialState[i, j].GetSnow()*samePosition + 2*(initialState[i+1, j].GetSnow() + initialState[i, j++].GetSnow())*directlyAdjacent + 4*initialState[i+1, j-1].GetSnow()*diagonalPosition  ));
							result[i, j].SetLightning((int)( initialState[i, j].GetLightning()*samePosition + 2*(initialState[i+1, j].GetLightning() + initialState[i, j++].GetLightning())*directlyAdjacent + 4*initialState[i+1, j-1].GetLightning()*diagonalPosition ));
							////TODO: falta comprobar queno haya nieve y lluvia a la vez
						}else{ //fila superior
							result[i,j].SetClouds((int)( initialState[i,j].GetClouds()*samePosition + (2*initialState[i+1, j].GetClouds() + initialState[i, j+1].GetClouds() + initialState[i, j-1].GetClouds())*directlyAdjacent + 2*(initialState[i-1, j+1].GetClouds() + initialState[i+1, j-1].GetClouds())*diagonalPosition ));
							result[i,j].SetRain((int)( initialState[i,j].GetRain()*samePosition + (2*initialState[i+1, j].GetRain() + initialState[i, j+1].GetRain() + initialState[i, j-1].GetRain())*directlyAdjacent + 2*(initialState[i-1, j+1].GetRain() + initialState[i+1, j-1].GetRain())*diagonalPosition ));
							result[i,j].SetSnow((int)( initialState[i,j].GetSnow()*samePosition + (2*initialState[i+1, j].GetSnow() + initialState[i, j+1].GetSnow() + initialState[i, j-1].GetSnow())*directlyAdjacent + 2*(initialState[i-1, j+1].GetSnow() + initialState[i+1, j-1].GetSnow())*diagonalPosition ));
							result[i,j].SetLightning((int)( initialState[i,j].GetLightning()*samePosition + (2*initialState[i+1, j].GetLightning() + initialState[i, j+1].GetLightning() + initialState[i, j-1].GetLightning())*directlyAdjacent + 2*(initialState[i-1, j+1].GetLightning() + initialState[i+1, j-1].GetLightning())*diagonalPosition ));
							
						}
					}else if(i == size0){
						if(j==0){ //esquina inf. izq
					
					//TODO: cambiar para hacer los calculos de cada una de las variables (nubes, lluvia, nieve, tormenta
					//		result[i, j] = initialState[i, j]*samePosition + 2*(initialState[i-1, j] + initialState[i, j+1])*directlyAdjacent + 4*initialState[i-1, j+1]*diagonalPosition;
						}else if(j == size1){ // esquina inf. derecha
					
					//TODO: cambiar para hacer los calculos de cada una de las variables (nubes, lluvia, nieve, tormenta
					//		result[i, j] = initialState[i, j]*samePosition + 2*(initialState[i-1, j] + initialState[i, j-1])*directlyAdjacent + 4*initialState[i-1, j-1]*diagonalPosition;
						}else{ //fila inferior
					
					//TODO: cambiar para hacer los calculos de cada una de las variables (nubes, lluvia, nieve, tormenta
					//		result[i,j] = initialState[i,j]*samePosition + (2*initialState[i-1, j] + initialState[i, j+1] + initialState[i, j-1])*directlyAdjacent + 2*(initialState[i-1, j+1] + initialState[i-1, j-1])*diagonalPosition;
						}
					}else if(j==0){//columna borde izquierdo
					
					//TODO: cambiar para hacer los calculos de cada una de las variables (nubes, lluvia, nieve, tormenta
					//	result[i, j] = initialState[i, j]* samePosition + (initialState[i+1, j] + initialState[i-1, j] + 2* initialState[i, j+1])*directlyAdjacent + 2*(initialState[i+1, j+1] + initialState[i-1, j+1])*diagonalPosition;
					}else if(j == size1){//columna borde derecho
					
					//TODO: cambiar para hacer los calculos de cada una de las variables (nubes, lluvia, nieve, tormenta
					//	result[i, j] = result[i, j] = initialState[i, j]* samePosition + (initialState[i+1, j] + initialState[i-1, j] + 2* initialState[i, j-1])*directlyAdjacent + 2*(initialState[i+1, j-1] + initialState[i-1, j-1])*diagonalPosition;
					}else{ //celdas sin borde
					
					//TODO: cambiar para hacer los calculos de cada una de las variables (nubes, lluvia, nieve, tormenta
					//	result[i,j] = initialState[i,j]*samePosition + (initialState[i-1, j] + initialState[i+1, j] + initialState[i, j-1] + initialState[i, j+1])*directlyAdjacent + (initialState[i-1, j-1] + initialState[i-1, j+1] + initialState[i+1, j-1] + initialState[i+1, j+1])*diagonalPosition;
					}
				}
			}
            return result;
		}
 
        public static float GenerateNormalRandom(float mu, float sigma) {
            float rand1 = Random.Range(0.0f, 1.0f);
            float rand2 = Random.Range(0.0f, 1.0f);

            float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

            return (mu + sigma * n);
        }

        public override string ToString() {
            string result = "SEASON: " + currentSeason.name + "\n";
            int seasonIndex = currentSeason.GetIndex(); //it is common for all the map
            for (int i = 0; i < currentState.GetLength(0); i++) {
                for (int j = 0; j < currentState.GetLength(1); j++) {
                    result += "CHUNK " + i + ", " + j;
                    WeatherState s = currentState[i, j];
                    result += s.ToString();
                }
            }
            return result;
        }

        void Awake() {
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
			
			//checking softening values
			if((samePosition + directlyAdjacent*4 + diagonalPosition*4)!= 1){
				Debug.LogError("WEATHERCONTROLLER: There is an error with the softening values, please revise it \n" + 
								"samePosition + directlyAdjacent*4 + diagonalPosition*4 is not 1");
			}
			if(previous_state_influence + biome_influence != 1){
				Debug.LogError("WEATHERCONTROLLER: There is an error with the simulation parameters, previous_state_influence and biome_influence need to sum 1"); //TODO: esta frase esta bien escrita???
			}
			
			
            if (clock == null) {
                Debug.LogError("WEATHERCONTROLLER: No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }
            
            if(rainSystem == null) {
                Debug.LogError("WEATHERCONTROLLER: rain system reference can not be null");
            }
            if (snowSystem == null) {
                Debug.LogError("WEATHERCONTROLLER: snow system reference can not be null");
            }
            if (lightningSystem == null) {
                Debug.LogError("WEATHERCONTROLLER: lighning system reference can not be null");
            }
			
			//currentState = new WeatherState[,];
		}

        // Use this for initialization
        void Start() {
            lastUpdate = clock.getCurrentTime().Clone();
        }

        // Update is called once per frame
        void Update() {
            Time currentTime = clock.getCurrentTime();
            if(lastUpdate.SecondsBetween(currentTime)>= (update_frequency * 60)) {
				currentSeason = clock.GetSeason();
                //WeatherState next = GetNextState(currentState, biome);//TODO: aqui se llama a metodo para calcular nuevo estado
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: A new Weather has been calculated: ");
					Debug.Log(ToString());
					Debug.Break();
                }
                lastUpdate = currentTime.Clone();
            }
          

        }
    }
}
