using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class WeatherController : MonoBehaviour {
        #region constants
        public const float E  = 2.71828f;
        public const float PI = 1.61803399f;
        #endregion constants


        #region public Atributes
        [Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug; 

		[Header ("Symulation parameters:")]
		public int update_frequency = 60;
		public float standard_deviation = 1;
		public Biome biome; //TODO: cambiar esto en el futuro
		
		[Header("Weather Visualization (Particle Systems):")]
        public CloudController cloudController;
        public ParticleController rainController;
        public ParticleController snowController;
        //public ParticleSystem lightningSystem;
		#endregion public Atributes
		
        #region private Atributes
        private CentralClock clock;
        private Time lastUpdate;
		private int currentSeasonIndex;

		private WeatherState currentState;
        private int maxIntensityValue;

        private float[,] normalDistributionValues;

		private float[,] cloudAcumulativeValues;
        private float[,] rainAcumulativeValues;
        private float[,] snowAcumulativeValues;
        private float[,] lightningAcumulativeValues;

        #endregion private Atributes 

        private float NormalDistributionValue(int x, float mean, float stdDev) {
            float result;
            result = (1/(stdDev*Mathf.Sqrt(2*PI))) * Mathf.Pow(E, (float) -(1.0f/2.0)*Mathf.Pow(((x-mean)/stdDev), 2.0f));
            
            return result;
        }
        
		private WeatherState GetInitialState(){
            if (debug) {
                Debug.Log("WEATHERCONTROLLER: calculating initial state.... \n" +
                    "Season: " + currentSeasonIndex);
                Debug.Break();
            }
            //DECIDING CLOUD INTENSITY
            float max = 0.0f;
            int clouds = 0;
            for(int i=0; i<maxIntensityValue; i++) {
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: biome's probability of cloud intensity " + i + ": " + biome.GetCloudProbability(i, currentSeasonIndex));
                    Debug.Break();
                }
                if (biome.GetCloudProbability(i, currentSeasonIndex) > max) {
                    max = biome.GetCloudProbability(i, currentSeasonIndex);
                    clouds = i;
                    if (debug) {
                        Debug.Log("WEATHERCONTROLLER: a provisional intensity has been found. \n" +
                            "max probability value: " + max + "\t intensity value: " + i);
                        Debug.Break();
                    }
                }
            }
            if (debug) {
                Debug.Log("WEATHERCONTROLLER: clouds set to " + clouds);
                Debug.Break();
            }

            //DECIDING RAIN INTENSITY
            max = 0.0f;
            int rain = 0;
            for (int i = 0; i < maxIntensityValue; i++) {
                if (biome.GetRainProbability(i, currentSeasonIndex) > max) {
                    max = biome.GetRainProbability(i, currentSeasonIndex);
                    rain = i;
                }
            }
            if (debug) {
                Debug.Log("WEATHERCONTROLLER: rain set to " + rain);
                Debug.Break();
            }

            //DECIDING SNOW INTENSITY
            max = 0.0f;
            int snow = 0;
            for (int i = 0; i < maxIntensityValue; i++) {
                if (biome.GetSnowProbability(i, currentSeasonIndex) > max) {
                    max = biome.GetSnowProbability(i, currentSeasonIndex);
                    snow = i;
                }
            }
            if (debug) {
                Debug.Log("WEATHERCONTROLLER: snow set to " + snow);
                Debug.Break();
            }

            //DECIDING LIGHTNING INTENSITY
            max = 0.0f;
            int lightning = 0;
            for (int i = 0; i < maxIntensityValue; i++) {
                if (biome.GetLightningProbability(i, currentSeasonIndex) > max) {
                    max = biome.GetLightningProbability(i, currentSeasonIndex);
                    lightning = i;
                }
                
            }
            if (debug) {
                Debug.Log("WEATHERCONTROLLER: lightning set to " + lightning);
                Debug.Break();
            }


            //Coherency tests(
            if (rain > 0 && snow > 0) {
                if (biome.GetCloudProbability(rain, currentSeasonIndex) > biome.GetCloudProbability(snow, currentSeasonIndex)) {
                    snow = 0;
                    if (debug) {
                        Debug.Log("WEATHERCONTROLLER: rain found to be more likely than snow, therefore snow has been set to 0 ");
                        Debug.Break();
                    }
                } else {
                    rain = 0;
                    if (debug) {
                        Debug.Log("WEATHERCONTROLLER: snow found to be more likely than rain, therefore rain has been set to 0 ");
                        Debug.Break();
                    }
                }
            }

            if (snow > clouds) {
                snow = clouds;
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: snow value was greater then the cloud intensity. The snow value has been changed to: " + snow);
                    Debug.Break();
                }
            }
            if (rain > clouds) {
                rain = clouds;
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: rain value was greater then the cloud intensity. The rain value has been changed to: " + rain);
                    Debug.Break();
                }
            }
            if (lightning >= clouds) {
                lightning = clouds;
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: lightning value was greater then the cloud and rain intensities. The lighning value has been changed to: " + lightning);
                    Debug.Break();
                }
            }
            return new WeatherState(clouds, rain, lightning, snow);
			
		}

        // accumulative values are asumed to be not null and correct for the season
        private WeatherState getNextState( WeatherState currentState){
            int clouds, rain, snow, lightning;
            clouds = rain = snow = lightning = 0;


            //CLOUD VALUE:
            float random = UnityEngine.Random.value;
            for(int i = 0; i<= maxIntensityValue; i++) {
                if(random<cloudAcumulativeValues[i, currentState.GetClouds()]) {
                    clouds = i;
                    break;
                }
            }

            //RAIN VALUE
            random = UnityEngine.Random.value;
            for (int i = 0; i <= maxIntensityValue; i++) {
                if (random < rainAcumulativeValues[i, currentState.GetRain()]) {
                    rain = i;
                    break;
                }
            }

            //SNOW VALUE
            random = UnityEngine.Random.value;
            for (int i = 0; i <= maxIntensityValue; i++) {
                if (random < snowAcumulativeValues[i, currentState.GetSnow()]) {
                    snow = i;
                    break;
                }
            }

            //LIGHTNING VALUE
            random = UnityEngine.Random.value;
            for (int i = 0; i <= maxIntensityValue; i++) {
                if (random < lightningAcumulativeValues[i, currentState.GetLightning()]) {
                    lightning = i;
                    break;
                }
            }

            //COHERENCY TEST
            if(rain>0 && snow > 0) {
                if (biome.GetCloudProbability(rain, currentSeasonIndex) > biome.GetCloudProbability(snow, currentSeasonIndex)) {
                    snow = 0;
                    if (debug) {
                        Debug.Log("WEATHERCONTROLLER: rain found to be more likely than snow, therefore snow has been set to 0 ");
                        Debug.Break();
                    }
                } else {
                    rain = 0;
                    if (debug) {
                        Debug.Log("WEATHERCONTROLLER: snow found to be more likely than rain, therefore rain has been set to 0 ");
                        Debug.Break();
                    }
                }
            }

            if (snow > clouds) {
                snow = clouds;
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: snow value was greater then the cloud intensity. The snow value has been changed to: " + snow);
                    Debug.Break();
                }
            }
            if (rain > clouds) {
                rain = clouds;
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: rain value was greater then the cloud intensity. The rain value has been changed to: " + rain);
                    Debug.Break();
                }
            }
            if (lightning >= clouds) {
                lightning = clouds;
                if (debug) {
                    Debug.Log("WEATHERCONTROLLER: lightning value was greater then the cloud and rain intensities. The lighning value has been changed to: " + lightning);
                    Debug.Break();
                }
            }

            return new WeatherState(clouds, rain, lightning, snow);
		}

		private void updateAcumulativeValues( int season) {
            //checking the normal distribution values matrix - N(i, j)
            if (normalDistributionValues == null) {
                normalDistributionValues = new float[maxIntensityValue+1, maxIntensityValue+1];
                for (int i = 0; i <= maxIntensityValue; i++) {
                    for (int j = 0; j <= maxIntensityValue; j++) {
                        normalDistributionValues[i, j] = NormalDistributionValue(i, j, standard_deviation);
                    }
                }
            }

            //CLOUDS  
            //get the biome probabilities
            float[] cloudPI = biome.GetCloudProbabiliyRow(season);

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] cloudPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i<=maxIntensityValue; i++) {
                for(int j = 0; j<= maxIntensityValue; j++) {
                    cloudPIJ[i, j] = normalDistributionValues[i, j] * cloudPI[i];
                }
            }

            //normalizing each row on Probability(i,j) matrix
            float[,] normCloudPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for(int i = 0; i<= maxIntensityValue; i++) {
                float rowSum = 0;
                for(int j = 0; j<= maxIntensityValue; j++) {
                    rowSum += cloudPIJ[i, j];
                }

                for (int j = 0; j <= maxIntensityValue; j++) {
                   normCloudPIJ[i,j] = cloudPIJ[i, j]/rowSum;
                }
            }

            //calculate the acumulative P(i,j) matrix
            if (cloudAcumulativeValues == null)
                cloudAcumulativeValues = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for(int i = 0; i<= maxIntensityValue; i++) {
                for(int j = 0; j <= maxIntensityValue; j++) {
                    if (i == 0) {
                        cloudAcumulativeValues[i, j] = normCloudPIJ[i, j];
                    } else {
                        cloudAcumulativeValues[i, j] = normCloudPIJ[i, j] + cloudAcumulativeValues[i - 1, j];
                    }
                }
            }

            //RAIN
            //get the biome probabilities
            float[] rainPI = biome.GetRainProbabiliyRow(season);

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] rainPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    rainPIJ[i, j] = normalDistributionValues[i, j] * rainPI[i];
                }
            }

            //normalizing each row on Probability(i,j) matrix
            float[,] normRainPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                float rowSum = 0;
                for (int j = 0; j <= maxIntensityValue; j++) {
                    rowSum += rainPIJ[i, j];
                }

                for (int j = 0; j <= maxIntensityValue; j++) {
                    normRainPIJ[i, j] = rainPIJ[i, j] / rowSum;
                }
            }

            //calculate the acumulative P(i,j) matrix
            if (rainAcumulativeValues == null)
                rainAcumulativeValues = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    if (i == 0) {
                        rainAcumulativeValues[i, j] = normRainPIJ[i, j];
                    } else {
                        rainAcumulativeValues[i, j] = normRainPIJ[i, j] + rainAcumulativeValues[i - 1, j];
                    }
                }
            }

            //SNOW
            //get the biome probabilities
            float[] snowPI = biome.GetSnowProbabiliyRow(season);

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] snowPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    snowPIJ[i, j] = normalDistributionValues[i, j] * snowPI[i];
                }
            }

            //normalizing each row on Probability(i,j) matrix
            float[,] normSnowPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                float rowSum = 0;
                for (int j = 0; j <= maxIntensityValue; j++) {
                    rowSum += snowPIJ[i, j];
                }

                for (int j = 0; j <= maxIntensityValue; j++) {
                    normSnowPIJ[i, j] = snowPIJ[i, j] / rowSum;
                }
            }

            //calculate the acumulative P(i,j) matrix
            if (snowAcumulativeValues == null)
                snowAcumulativeValues = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    if (i == 0) {
                        snowAcumulativeValues[i, j] = normSnowPIJ[i, j];
                    } else {
                        snowAcumulativeValues[i, j] = normSnowPIJ[i, j] + snowAcumulativeValues[i - 1, j];
                    }
                }
            }

            //LIGHTNING
            //get the biome probabilities
            float[] lightningPI = biome.GetLightningProbabiliyRow(season);

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] lightningPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    lightningPIJ[i, j] = normalDistributionValues[i, j] * lightningPI[i];
                }
            }

            //normalizing each row on Probability(i,j) matrix
            float[,] normLightningPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                float rowSum = 0;
                for (int j = 0; j <= maxIntensityValue; j++) {
                    rowSum += lightningPIJ[i, j];
                }

                for (int j = 0; j <= maxIntensityValue; j++) {
                    normLightningPIJ[i, j] = lightningPIJ[i, j] / rowSum;
                }
            }

            //calculate the acumulative P(i,j) matrix
            if (lightningAcumulativeValues == null)
                lightningAcumulativeValues = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    if (i == 0) {
                        lightningAcumulativeValues[i, j] = normLightningPIJ[i, j];
                    } else {
                        lightningAcumulativeValues[i, j] = normLightningPIJ[i, j] + lightningAcumulativeValues[i - 1, j];
                    }
                }
            }
        }

        public override string ToString() {
            string result = "";

            result += "Weather State " + lastUpdate.ToString() + ":\n ";
            result += "Clouds " + currentState.GetClouds() + "\n";
            result += "Rain " + currentState.GetRain() + "\n";
            result += "Lightning " + currentState.GetLightning() + "\n";
            result += "Snow " + currentState.GetSnow() + "\n";
            return result;
        }
		void Awake(){
			//Getting Clock reference
			clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("SUN/MOON ROTATION: No clock could be found, please remember to tag the object with the CentralClock script");
                Debug.Break();
            }

            maxIntensityValue = biome.GetMaxIntensityValue();
        }
		
		void Start(){
            //obtener estado inicial como el valor mas probable de cada estado a menos que hayan inconsistencias ( mas lluvia que nubes, lluvia y nieve a la vez....)
            currentSeasonIndex = clock.GetSeason().GetIndex();
            currentState = GetInitialState();
            lastUpdate = clock.getCurrentTime().Clone();

            //calcular matrices acumulativas 
            updateAcumulativeValues(currentSeasonIndex);
		}
		
		void Update(){

            if(lastUpdate.SecondsBetween(clock.getCurrentTime()) >= update_frequency * 60) {
                if (debug) {
                    Debug.Log("It is time to calculate a new weather state");
                    Debug.Break();
                }
                if (!(clock.GetSeason().GetIndex() == currentSeasonIndex)) {
                    currentSeasonIndex = clock.GetSeason().GetIndex();
                    updateAcumulativeValues(currentSeasonIndex);

                }
                currentState = getNextState(currentState);
                lastUpdate = clock.getCurrentTime().Clone();
                visualizeState();
            }

		}

        private void visualizeState() {
            cloudController.SetIntensity(currentState.GetClouds());
            rainController.SetIntensity(currentState.GetRain());
            snowController.SetIntensity(currentState.GetSnow());
            //TODO: representacion rayos
        }
    }
}
