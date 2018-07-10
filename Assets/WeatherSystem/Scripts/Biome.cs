using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace weatherSystem {
	public class Biome: MonoBehaviour{
		
		#region constants
		public const int CLOUDS = 0;
        public const int RAIN = 1;
        public const int LIGHTNING = 2;
        public const int SNOW = 3;
        #endregion constants

        #region Public Variables
        [Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
        public bool debug;

        [Tooltip("text file with the configuration matices")] //TODO: matrices en ingles???
		public TextAsset configuration; 
		#endregion Public Variables
		
		#region Private Variables
		private float [,] cloudProbabilities;
		private float [,] rainProbabilities;
		private float [,] snowProbabilities;
		private float [,] lightningProbabilities;
        private int maxValue;
        private int numSeasons;
        #endregion Private Variables

        #region getters and setters
        public float GetCloudProbability(int i, int s) {
            return cloudProbabilities[i, s];
        }
        public float GetRainProbability(int i, int s) {
            return rainProbabilities[i, s];
        }
        public float GetSnowProbability(int i, int s) {
            return cloudProbabilities[i, s];
        }
        public float GetLightningProbability(int i, int s) {
            return cloudProbabilities[i, s];
        }
        public int GetMaxIntensityValue() { return maxValue; }

        public float[,] GetCloudProbabiliyMatrix() { return cloudProbabilities; }
        public float[,] GetRainProbabiliyMatrix() { return rainProbabilities; }
        public float[,] GetSnowProbabiliyMatrix() { return snowProbabilities; }
        public float[,] GetLightningProbabiliyMatrix() { return lightningProbabilities; }

        public float[] GetCloudProbabiliyRow(int s) {
            float[] row = new float[maxValue+1];
            for(int i = 0; i<= maxValue; i++) {
                row[i] = cloudProbabilities[i, s];
            }

            return row;
        }
        public float[] GetRainProbabiliyRow(int s) {
            float[] row = new float[maxValue+1];
            for (int i = 0; i <= maxValue; i++) {
                row[i] = rainProbabilities[i, s];
            }

            return row;
        }
        public float[] GetSnowProbabiliyRow(int s) {
            float[] row = new float[maxValue+1];
            for (int i = 0; i <= maxValue; i++) {
                row[i] = snowProbabilities[i, s];
            }

            return row;
        }
        public float[] GetLightningProbabiliyRow(int s) {
            float[] row = new float[maxValue+1];
            for (int i = 0; i <= maxValue; i++) {
                row[i] = lightningProbabilities[i, s];
            }

            return row;
        }
        #endregion getters and setters





        public void ParseConfiguration(string text){
			if (debug)
                Debug.Log("BIOME " + this.name + ": Parsing the imput file.... ");
			
			Season[] seasons = GetSeasons();
			
			string[] lines = text.Split('\n');
			
			int i = 0; 
			maxValue = 0;
			numSeasons = 0;
			
			while(i<lines.Length){
				if (debug) {
                    Debug.Log("Line " + i + ": " + lines[i]);
                    Debug.Break();
                }		
				if(lines[i].Trim().Equals("//VALUE RANGE")){
					i++;
					
					string[] values = lines[i].Split(':');
					if(values[0].Trim().Equals("max intensity")) {
                        maxValue = int.Parse(values[1]);
                        i++;
                    }
                    values = lines[i].Split(':');
                    if (values[0].Trim().Equals("number of seasons")) {
                        numSeasons = int.Parse(values[1]);
                        i++;
                    }
                    
                    if (debug){
						Debug.Log("Value Range values readed \n" +
                            "Max Intensity: " + maxValue + "\t Number of Seasons: " + numSeasons);
						Debug.Break();
					}
				}
				if(lines[i].Trim().Equals("//CLOUDS")){
                    if (debug) {
                        Debug.Log("now reading cloud data...");
                        Debug.Break();
                    }
                    i += 2; //2 unit increase becouse we are skipping the header line
                    cloudProbabilities = new float[maxValue+1, numSeasons];

                    for (int s = 0; s < numSeasons; s++) {
                        string[] values = lines[i].Split(',');
                        int index = getSeasonIndex(values[0].Trim(), seasons);
                        //if (debug) {
                        //    Debug.Log("Reading line " + i + ": \n" +
                        //        lines[i]);
                        //    Debug.Log("Season name: " + values[0].Trim() + "\t index: " + index);
                        //    Debug.Break();
                        //}

                        for (int j = 0; j <= maxValue; j++) {
                            //if (debug) {
                            //    Debug.Log("cloud probability for season " + values[0].Trim() + " and intensity " + j + " :" + (values[j+1].Trim()));
                            //    Debug.Break();
                            //}
                            cloudProbabilities[j, index] = float.Parse(values[j+1].Trim());
                            //if (debug) {
                            //    Debug.Log("converted value: " + cloudProbabilities[j, index]);
                            //    Debug.Break(); 
                            //}
                        }
                        i++;
                    }
					
				}
				if(lines[i].Trim().Equals("//RAIN")){
                    if (debug) {
                        Debug.Log("now reading rain data...");
                        Debug.Break();
                    }
                    i += 2; //2 unit increase becouse we are skipping the header line
                    rainProbabilities = new float[maxValue + 1, numSeasons];

                    for (int s = 0; s < numSeasons; s++) {
                        string[] values = lines[i].Split(',');
                        int index = getSeasonIndex(values[0].Trim(), seasons);
                        //if (debug) {
                        //    Debug.Log("Reading line " + i + ": \n" +
                        //        lines[i]);
                        //    Debug.Log("Season name: " + values[0].Trim() + "\t index: " + index);
                        //    Debug.Break();
                        //}

                        for (int j = 0; j <= maxValue; j++) {
                            //if (debug) {
                            //    Debug.Log("rain probability for season " + values[0].Trim() + " and intensity " + j + " :" + (values[j + 1].Trim()));
                            //    Debug.Break();
                            //}
                            rainProbabilities[j, index] = float.Parse(values[j + 1].Trim());
                            //if (debug) {
                            //    Debug.Log("converted value: " + cloudProbabilities[j, index]);
                            //    Debug.Break();
                            //}
                        }
                        i++;
                    }

                }
                if (lines[i].Trim().Equals("//SNOW")){
                    if (debug) {
                        Debug.Log("now reading snow data...");
                        Debug.Break();
                    }
                    i += 2; //2 unit increase becouse we are skipping the header line
                    snowProbabilities = new float[maxValue + 1, numSeasons];

                    for (int s = 0; s < numSeasons; s++) {
                        string[] values = lines[i].Split(',');
                        int index = getSeasonIndex(values[0].Trim(), seasons);
                        //if (debug) {
                        //    Debug.Log("Reading line " + i + ": \n" +
                        //        lines[i]);
                        //    Debug.Log("Season name: " + values[0].Trim() + "\t index: " + index);
                        //    Debug.Break();
                        //}

                        for (int j = 0; j <= maxValue; j++) {
                            //if (debug) {
                            //    Debug.Log("snow probability for season " + values[0].Trim() + " and intensity " + j + " :" + (values[j + 1].Trim()));
                            //    Debug.Break();
                            //}
                            snowProbabilities[j, index] = float.Parse(values[j + 1].Trim());
                            //if (debug) {
                            //    Debug.Log("converted value: " + snowProbabilities[j, index]);
                            //    Debug.Break();
                            //}
                        }
                        i++;
                    }

                }
                if (lines[i].Trim().Equals("//LIGHTNING")){ //TODO: hay qeu ver como se escribe bien, que soy tontita y creo que no lo estoy escribiendo correctamente
                    if (debug) {
                        Debug.Log("now reading lightning data...");
                        Debug.Break();
                    }
                    i += 2; //2 unit increase becouse we are skipping the header line
                    lightningProbabilities = new float[maxValue + 1, numSeasons];

                    for (int s = 0; s < numSeasons; s++) {
                        string[] values = lines[i].Split(',');
                        int index = getSeasonIndex(values[0].Trim(), seasons);
                        //if (debug) {
                        //    Debug.Log("Reading line " + i + ": \n" +
                        //        lines[i]);
                        //    Debug.Log("Season name: " + values[0].Trim() + "\t index: " + index);
                        //    Debug.Break();
                        //}

                        for (int j = 0; j <= maxValue; j++) {
                            //if (debug) {
                            //    Debug.Log("lightning probability for season " + values[0].Trim() + " and intensity " + j + " :" + (values[j + 1].Trim()));
                            //    Debug.Break();
                            //}
                            lightningProbabilities[j, index] = float.Parse(values[j + 1].Trim());
                            //if (debug) {
                            //    Debug.Log("converted value: " + lightningProbabilities[j, index]);
                            //    Debug.Break();
                            //}
                        }
                        i++;
                    }

                }
                i++;
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
		
		private int getSeasonIndex(string name,  Season[] seasons ){
			int result = -1;
			
			for(int i = 0; i< seasons.Length; i++){
				Season s = seasons[i];
				
				if(s.name.Equals(name)){ //TODO: al comprobar pasar los dos nombres a minusculas
					result = s.index;
					break;
				}
			}
			if(result == -1){
                Debug.LogError("name " + name + " could not ne mathed with any Season object, please remember to include the tag 'Season' ");
				Debug.Break();
			}
			return result;
		}

        public override string ToString(){
            string result = "";
            result += "BIOME " + name + ": \n";
            result += "CLOUDS: \n";
            for(int j = 0;j<numSeasons; j++) {
                for(int i = 0; i<= maxValue; i++) {
                    result += cloudProbabilities[i, j] + "\t";
                }
                result += "\n";

            }
            result += "RAIN: \n";
            for (int j = 0; j < numSeasons; j++) {
                for (int i = 0; i <= maxValue; i++) {
                    result += rainProbabilities[i, j] + "\t";
                }
                result += "\n";

            }
            result += "SNOW: \n";
            for (int j = 0; j < numSeasons; j++) {
                for (int i = 0; i <= maxValue; i++) {
                    result += snowProbabilities[i, j] + "\t";
                }
                result += "\n";

            }
            result += "LIGHTNING: \n";
            for (int j = 0; j < numSeasons; j++) {
                for (int i = 0; i <= maxValue; i++) {
                    result += lightningProbabilities[i, j] + "\t";
                }
                result += "\n";

            }
            return result;
        }

        void Awake(){
			if(configuration == null){
				Debug.LogError("BIOME " + this.name + ": the configuration file is necessary");
			}
			if(configuration.text.Equals("")){ 
				Debug.LogError("BIOME " + this.name + ": the configuration file cannot be empty");
			}

            ParseConfiguration(configuration.text);
            if (debug) {
                Debug.Log("BIOME " + this.name + ": Biomme configuration set to: \n" + ToString());
            }

        }
		
	}
}
