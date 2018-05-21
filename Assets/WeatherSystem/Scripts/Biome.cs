using System.Collections;
using System.Collections.Generic;
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
        [Tooltip("text file with the configuration matices")] //TODO: matrices en ingles???
		public TextAsset configuration; 
		#endregion Public Variables
		
		#region Private Variables
		private int[,] intensities;
		private double[,] probabilities;
		#endregion Private Variables
		
		
		public int[,] GetIntensities (){return intensities;}
		public int GetIntensity(int season, int weatherCondition){return intensities[season,weatherCondition];}
		public double[,] GetProbabilities(){return probabilities; }
		public double GetProbability(int season, int weatherCondition){return probabilities[season,weatherCondition];}
        
		public void ParseConfiguration(string text){
			string[] lines = text.Split('\n');
			int i = 0;
			int n = 0; //the first dimension of the matices
			int m = 0; //the second dimension of the matices
			bool sizeDeclared = false;
			bool probabilitiesDeclared = false;
			bool intensitiesDeclared = false;
			while (i<lines.Length){
				if (lines[i].Trim().Equals("SIZE:")){ //TODO: hay que revisar esta expresion, ¿seria necesario incluir \n?, algo a parte del trim???
					i++;
					string[] sizes = lines[i].Split(',');
					if (sizes.Length != 2){ //TODO: comprobar esta linea. el tamaño es 2 seguro???
                        Debug.LogError("BIOME " + this.name + ": the configuration file does not have a correct size declaration. please revise the biome configuration example for mor details on the size declaration.");
					}
					n = int.Parse(sizes[0]);
					m = int.Parse(sizes[1]);
					if(n==0|| m ==0){
						Debug.LogError("BIOME " + this.name + ": one or both of the sizes declared in the configuration file is 0. The size needs to be at least 1 in each dimension.");
					}
					probabilities = new double [n,m];
					intensities = new int [n,m];
				}
				if(sizeDeclared && lines[i].Trim().Equals("PROBABILITIES:")){
					for(int j = i; j<n; j++){
                        
                    }
                    probabilitiesDeclared = true;
				}
				if(sizeDeclared && lines[i].Trim().Equals("INTENSITIES:")){
                    for (int j = i; j < n; j++) {

                    }
                    intensitiesDeclared = true;
                }
				
				i++;
			}
            if(! (sizeDeclared && probabilitiesDeclared && intensitiesDeclared)) {
                Debug.LogError("BIOME " + this.name + ": not all sections of the declaration have been found, pleas include SIZE, PROBABILITIES and INTENSITIES.");
            }
			/*TODO: tratar el texto. Basicamente:
				while (linea != "SIZE:")
					cambiar a siguiente linea;
				cambiar a la lina de despues de SIZE:
				hacer split con ','. el primer valor se guarda como int rows
				el segunto valor int columns
				inicializar las dos matrices como  [rows-1, columns-1] o [columns-1, rows-1] no estoy segura
				
				while(linea != "PROBABILITIES")
						ignorar
				repetir row veces:
					linea.split(',')
					por cada elemento en linea
						buscar en la lista de seasons el objeto cuyo nombre coincide con la variable correcta. 
						hacer que weatherController añada la estacion a un diccionario. devolverá el indice de esa estacion en la matriz interna.
						introducir el resto de  elementos en la matriz en el mismo orden
			*/
		}

        void Awake(){
			if(configuration == null){
				Debug.LogError("BIOME " + this.name + ": the configuration file is necessary");
			}
			if(configuration.text.Equals("")){ //TODO: asi es como se comparaba con Strings, no?
				Debug.LogError("BIOME " + this.name + ": the configuration file cannot be empty");
			}
			
			ParseConfiguration(configuration.text);
			//TODO: llamar aqui al metodo para hacer parse del fichero de configuracion
		}
		void start(){}
		void update(){}
	}
}
