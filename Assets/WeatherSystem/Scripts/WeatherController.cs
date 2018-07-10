using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class WeatherController : MonoBehaviour {
        #region constants
        public const float E  = 2.7182818284590452353f;
        public const float PI = 3.14159265359f;
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
        public CloudController lightningController;
		#endregion public Atributes
		
        #region private Atributes
        private CentralClock clock;
        private Time lastUpdate;
        private Season currentSeason;
		private int currentSeasonIndex;
		private WeatherState currentState;
        private int maxIntensityValue;

        private float[,] normalDistributionValues;

		private float[,] cloudAcumulativeValues;
        private float[,] rainAcumulativeValues;
        private float[,] snowAcumulativeValues;
        private float[,] lightningAcumulativeValues;

        #endregion private Atributes 

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
            Season result = currentSeason;
            Season[] seasons = GetSeasons();
            for (int i = 0; i < seasons.Length; i++) {
                if (seasons[i].GetNextSeason().Equals(s)) {
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

        public string matrixToString( float[,] matrix) {
            string str = "";
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    str += matrix[i, j] + "\t";
                }
                str += "\n";
            }
            return str;
        }

        private float NormalDistributionValue(int x, float mean, float stdDev) {
            
            float result;
            //result = (1/(stdDev*Mathf.Sqrt(2*PI))) * Mathf.Pow(E, (float) -(1.0f/2.0)*Mathf.Pow(((x-mean)/stdDev), 2.0f));
            result = (1 / (stdDev*Mathf.Sqrt(2*PI))) * Mathf.Pow( E,  (- Mathf.Pow((x-mean), 2.0f))  /  (2* Mathf.Pow(stdDev, 2)) ) ;
            //if (debug) {
            //    Debug.Log("Calculating normal distribution value for: x: " + x + " mean: " + mean + " stdDev: " + stdDev + "\n Result: " + result);
            //    Debug.Break();
            //}
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
                //if (debug) {
                //    Debug.Log("WEATHERCONTROLLER: biome's probability of cloud intensity " + i + ": " + biome.GetCloudProbability(i, currentSeasonIndex));
                //    Debug.Break();
                //}
                if (biome.GetCloudProbability(i, currentSeasonIndex) > max) {
                    max = biome.GetCloudProbability(i, currentSeasonIndex);
                    clouds = i;
                    //if (debug) {
                    //    Debug.Log("WEATHERCONTROLLER: a provisional intensity has been found. \n" +
                    //        "max probability value: " + max + "\t intensity value: " + i);
                    //    Debug.Break();
                    //}
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
                if(random<cloudAcumulativeValues[currentState.GetClouds(), i]) {
                    clouds = i;
                    break;
                }
            }
            if (debug) {
                Debug.Log("CALCULATING CLOUD VALUE: ");
                Debug.Log("Acumulative values: " + matrixToString(cloudAcumulativeValues));
                Debug.Log("Random value: " + random + ", \t Selected value:" + clouds);
            }

            //RAIN VALUE
            random = UnityEngine.Random.value;
            for (int i = 0; i <= maxIntensityValue; i++) {
                if (random < rainAcumulativeValues[currentState.GetClouds(), i]) {
                    rain = i;
                    break;
                }
            }

            //SNOW VALUE
            random = UnityEngine.Random.value;
            for (int i = 0; i <= maxIntensityValue; i++) {
                if (random < snowAcumulativeValues[currentState.GetClouds(), i]) {
                    snow = i;
                    break;
                }
            }

            //LIGHTNING VALUE
            random = UnityEngine.Random.value;
            for (int i = 0; i <= maxIntensityValue; i++) {
                if (random < lightningAcumulativeValues[currentState.GetClouds(), i]) {
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
                //if (debug) {
                //    string str = "";
                //    for (int i = 0; i <= maxIntensityValue; i++) {
                //        for (int j = 0; j <= maxIntensityValue; j++) {
                //            str += normalDistributionValues[i, j] + "\t";
                //        }
                //        str += "\n";
                //    }
                //    Debug.Log("Normal distrubution values calculated: \n" + str);
                //    Debug.Break();
                //}

            }

            //CLOUDS  
            //get the biome probabilities
            float[] cloudPI = biome.GetCloudProbabiliyRow(season);
            //if (debug) {
            //    string str = "";
            //    for (int i = 0; i <= maxIntensityValue; i++) {
            //        str += cloudPI[i] + "\t";
            //    }
            //    Debug.Log("Cloud P(i): " + str);
            //    Debug.Break();
            //}

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] cloudPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i<=maxIntensityValue; i++) {
                for(int j = 0; j<= maxIntensityValue; j++) {
                    cloudPIJ[j, i] = normalDistributionValues[j, i] * cloudPI[i];
                }
            }

            //if (debug) {
            //    string str = "";
            //    for(int i = 0; i<= maxIntensityValue; i++) {
            //        for(int j = 0; j<= maxIntensityValue; j++) {
            //            str += cloudPIJ[i, j] + "\t";
            //        }
            //    }
            //    Debug.Log("Calculated CloudPIJ: \n" + str);
            //    Debug.Break();
            //}

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

            //if (debug) {
            //    string str = "";
            //    for (int i = 0; i <= maxIntensityValue; i++) {
            //        for (int j = 0; j <= maxIntensityValue; j++) {
            //            str += normCloudPIJ[i, j] + "\t";
            //        }
            //    }
            //    Debug.Log("Calculated normCloudPIJ: \n" + str);
            //    Debug.Break();
            //}

            //calculate the acumulative P(i,j) matrix
            if (cloudAcumulativeValues == null)
                cloudAcumulativeValues = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for(int i = 0; i<= maxIntensityValue; i++) {
                for(int j = 0; j <= maxIntensityValue; j++) {
                    if (j == 0) {
                        cloudAcumulativeValues[i, j] = normCloudPIJ[i, j];
                    } else {
                        cloudAcumulativeValues[i, j] = normCloudPIJ[i, j] + cloudAcumulativeValues[i, j-1];
                    }
                }
            }
            //if (debug) {
            //    Debug.Log("CLOUD ACCUMULATIVE VALUES: : \n" + matrixToString(cloudAcumulativeValues));
            //    Debug.Break();
            //}

            //RAIN
            //get the biome probabilities
            float[] rainPI = biome.GetRainProbabiliyRow(season);
            //if (debug) {
            //    string str = "";
            //    for (int i = 0; i <= maxIntensityValue; i++) {
            //        str += rainPI[i] + "\t";
            //    }
            //    Debug.Log("rainPI P(i): " + str);
            //    Debug.Break();
            //}

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] rainPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    rainPIJ[j, i] = normalDistributionValues[j, i] * rainPI[i];
                }
            }
            //if (debug) {
            //    Debug.Log("Calculated rainPIJ: \n" + matrixToString(rainPIJ));
            //    Debug.Break();
            //}

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
            //if (debug) {
            //    Debug.Log("Calculated normRainPIJ: \n" + matrixToString(normRainPIJ));
            //    Debug.Break();
            //}

            //calculate the acumulative P(i,j) matrix
            if (rainAcumulativeValues == null)
                rainAcumulativeValues = new float[maxIntensityValue + 1, maxIntensityValue + 1];

            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    if (j == 0) {
                        rainAcumulativeValues[i, j] = normRainPIJ[i, j];
                    } else {
                        rainAcumulativeValues[i, j] = normRainPIJ[i, j] + rainAcumulativeValues[i , j-1];
                    }
                }
            }
            //if (debug) {
            //    Debug.Log("RAIN ACCUMULATIVE VALUES: : \n" + matrixToString(rainAcumulativeValues));
            //    Debug.Break();
            //}

            //SNOW
            //get the biome probabilities
            float[] snowPI = biome.GetSnowProbabiliyRow(season);
            //if (debug) {
            //    string str = "";
            //    for (int i = 0; i <= maxIntensityValue; i++) {
            //        str += snowPI[i] + "\t";
            //    }
            //    Debug.Log("snowPI P(i): " + str);
            //    Debug.Break();
            //}

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] snowPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    snowPIJ[j, i] = normalDistributionValues[j, i] * snowPI[i];
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
                    if (j == 0) {
                        snowAcumulativeValues[i, j] = normSnowPIJ[i, j];
                    } else {
                        snowAcumulativeValues[i, j] = normSnowPIJ[i, j] + snowAcumulativeValues[i , j-1];
                    }
                }
            }
            //if (debug) {
            //    Debug.Log("SNOW ACCUMULATIVE VALUES: : \n" + matrixToString(snowAcumulativeValues));
            //    Debug.Break();
            //}

            //LIGHTNING
            //get the biome probabilities
            float[] lightningPI = biome.GetLightningProbabiliyRow(season);
            //if (debug) {
            //    string str = "";
            //    for (int i = 0; i <= maxIntensityValue; i++) {
            //        str += lightningPI[i] + "\t";
            //    }
            //    Debug.Log("lightningPI: " + str);
            //    Debug.Break();
            //}

            //probability if intensity i given a prevous state of j - P(i, j)
            float[,] lightningPIJ = new float[maxIntensityValue + 1, maxIntensityValue + 1];
            for (int i = 0; i <= maxIntensityValue; i++) {
                for (int j = 0; j <= maxIntensityValue; j++) {
                    lightningPIJ[j, i] = normalDistributionValues[j, i] * lightningPI[i];
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
                    if (j == 0) {
                        lightningAcumulativeValues[i, j] = normLightningPIJ[i, j];
                    } else {
                        lightningAcumulativeValues[i, j] = normLightningPIJ[i, j] + lightningAcumulativeValues[i, j-1];
                    }
                }
            }
            //if (debug) {
            //    Debug.Log("LIGHTNINF ACCUMULATIVE VALUES: : \n" + matrixToString(lightningAcumulativeValues));
            //    Debug.Break();
            //}
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

        }
		
		void Start(){

            maxIntensityValue = biome.GetMaxIntensityValue();
            //obtener estado inicial como el valor mas probable de cada estado a menos que hayan inconsistencias ( mas lluvia que nubes, lluvia y nieve a la vez....)
            GetInitialSeason(clock.getCurrentDate());
            currentState = GetInitialState();
            visualizeState();
            lastUpdate = clock.getCurrentTime().Clone();

            //calcular matrices acumulativas 
            updateAcumulativeValues(currentSeason.GetIndex());
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
            lightningController.SetIntensity(currentState.GetLightning());
        }
    }
}
