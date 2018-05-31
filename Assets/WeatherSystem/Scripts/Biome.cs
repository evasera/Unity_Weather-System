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
		private int[,] intensities;
		private double[,] probabilities;
        #endregion Private Variables


        public int[,] GetIntensities (){return intensities;}
		public int GetIntensity(Season season, int weatherCondition){return intensities[season.GetIndex(),weatherCondition];}
		public double[,] GetProbabilities(){return probabilities; }
		public double GetProbability(Season season, int weatherCondition){return probabilities[season.GetIndex(),weatherCondition];}
        
		public void ParseConfiguration(string text){
            if (debug)
                Debug.Log("BIOME " + this.name + ": Parsing the imput file.... ");
			string[] lines = text.Split('\n');
			int i = 0;
			int n = 0; //the first dimension of the matices
			int m = 0; //the second dimension of the matices
			bool sizeDeclared = false;
			bool probabilitiesDeclared = false;
			bool intensitiesDeclared = false;
			Season[] seasons = GetSeasons();
			
			while (i<lines.Length){
                if (debug) {
                    Debug.Log("Line " + i + ": " + lines[i]);
                }
                if (lines[i].Trim().Equals("SIZE:")){ //TODO: hay que revisar esta expresion, ¿seria necesario incluir \n?, algo a parte del trim???
                    if (debug) {
                        Debug.Log("Line mathches the size section header");
                    }
                    i++;

                    string[] sizes = lines[i].Split(',');
					if (sizes.Length != 2){ 
                        Debug.LogError("BIOME " + this.name + ": the configuration file does not have a correct size declaration. please revise the biome configuration example for mor details on the size declaration.");
					}
					n = int.Parse(sizes[0].Trim());
					m = int.Parse(sizes[1].Trim());
                    if (debug) {
                        Debug.Log("Line " + i + ": " + lines[i]);
                        Debug.Log("Calculated matrix sizes, n = " + n + "\t m = " + m);
                    }

                    if (n==0|| m ==0){
						Debug.LogError("BIOME " + this.name + ": one or both of the sizes declared in the configuration file is 0. The size needs to be at least 1 in each dimension.");
					}
					probabilities = new double [n,m];
					intensities = new int [n,m];
                    sizeDeclared = true;
                    if (debug) {
                        Debug.Log("Matrices initialized, sizes: \n " +
                            "probabilities: " + probabilities.GetLength(0) + ", " + probabilities.GetLength(1) + "\t intensities: " + intensities.GetLength(0) + ", " + intensities.GetLength(1));
                    }

                } else if(sizeDeclared && lines[i].Trim().Equals("PROBABILITIES:")){
                    if (debug) {
                        Debug.Log("Line mathches the probabilities section header");
                        Debug.Log("Skiping over the column name row....");
                    }
                    i += 2; //we skip over the first table row becouse it only has the column names

                    for (int j = 0; j<n; j++){
                        if (debug) {
                            Debug.Log("Line " + i + ": " + lines[i]);
                        }
                        string[] split = lines[i].Split(',');
						if(split.Length != m + 1){
							Debug.LogError("BIOME " + this.name + ": There seems to be an incorrect number of values in the probabilities table \n" +
                                "size: " + split.Length + "\t expected: " + (m+1) + "m: " + m);
						}
						int index = 0;
						string seasonName = split[0];
						for(int k =0; k<seasons.Length; k++){
							if(seasons[k].name.Equals(seasonName)){
								index = seasons[k].GetIndex();
                                if (debug) {
                                    Debug.Log("season index calculated: " + index);
                                }
                                break;
							}
						}
						for(int k = 1; k<=m; k++) {
                            if (debug) {
                                Debug.Log("Segment " + k + ": " + split[k]);
                            }
                            probabilities[index, k-1] = int.Parse(split[k].Trim())/100.0;
                            if (debug) {
                                Debug.Log("probabilities position [" + index + ", " + (k - 1) + "] set to " + probabilities[index, k - 1]);
                            }
                        }
						i++;
                    }
                    probabilitiesDeclared = true;
                    if (debug) {
                        Debug.Log("probabilities parsing completed");
                    }
                } else if(sizeDeclared && lines[i].Trim().Equals("INTENSITIES:")){
                    if (debug) {
                        Debug.Log("Line mathches the probabilities section header");
                        Debug.Log("Skiping over the column name row....");
                    }
                    i += 2; //we skip over the first table row becouse it only has the column names
                    if (debug) {
                        Debug.Log("Line " + i + ": " + lines[i]);
                    }
                    for (int j = 0; j < n; j++) {
                        string[] split = lines[i].Split(',');
                        if (split.Length != m + 1) {
                            Debug.LogError("BIOME " + this.name + ": There seems to be an incorrect number of values in the probabilities table, row; " + j + 1);
                        }
                        int index = 0;
                        string seasonName = split[0];
                        for (int k = 0; k < seasons.Length; k++) {
                            if (seasons[k].name.Equals(seasonName)) {
                                index = seasons[k].index;
                                if (debug) {
                                    Debug.Log("season index calculated: " + index);
                                }
                                break;
                            }
                        }
                        for (int k = 1; k <= m; k++) {
                            if (debug) {
                                Debug.Log("Segment " + k + ": " + split[k]);
                            }
                            intensities[index, k-1] = int.Parse(split[k].Trim(), CultureInfo.InvariantCulture);
                            if (debug) {
                                Debug.Log("intensities position [" + index + ", " + (k - 1) + "] set to " + probabilities[index, k - 1]);
                            }
                        }
                        i++;
                    }
                    intensitiesDeclared = true;
                    if (debug) {
                        Debug.Log("intensities parsing completed");
                    }
                }else if (debug) {
                    Debug.Log("Line ignored");
                }
				i++;
			}
            if(! (sizeDeclared && probabilitiesDeclared && intensitiesDeclared)) {
                Debug.LogError("BIOME " + this.name + ": not all sections of the declaration have been found, pleas include SIZE, PROBABILITIES and INTENSITIES.");
            }
            if (debug) {
                Debug.Log("parsing completed");
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

        public override string ToString(){
            string result = "";
            int n = probabilities.GetLength(0);
            int m = probabilities.GetLength(1);
            result += "SIZE: " + probabilities.GetLength(0) + " , " + probabilities.GetLength(1) + "\n \n"
                    + "PROBABILITIES: \n";
            for(int i = 0; i<n; i++) {
                for(int j = 0; j<m; j++) {
                    if(j == m - 1){
                        result += probabilities[i, j] + "\n";
                    } else {
                        result += probabilities[i, j] + ", ";
                    }
                }
            }
            result += "\n" + "INTENSITIES: \n";
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    if (j == m - 1) {
                        result += intensities[i, j] + "\n";
                    } else {
                        result += intensities[i, j] + ", ";
                    }
                }
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
		void Start(){}
		void Update(){}
	}
}
