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

        [Tooltip("a matrix of size[num. Seasons][num. weather conditions], filled with the probability of a cetrain weather condition for a certain season. All values go from 0 to 1. \n " + 
		"The index for each Season needs to match the one declared as parameter. \n" + 
		"The Weather condition order is as follows: Clouds, rain, lightning, snow")]
		public double[,] probaibilities = {}; //each line is a season, every column a weather event in the following order: clouds, rain, lightning, snow
		
		[Tooltip("a matrix of size[num. Seasons][num. weather conditions], filled with the average inensity of a cetrain weather condition for a certain season. All values go from 0 to 1. \n " + 
		"The index for each Season needs to match the one declared as parameter. \n" + 
		"The Weather condition order is as follows: Clouds, rain, lightning, snow")]
		public int[,] intensities = {};
		
		[Tooltip("a matrix of size[num. Seasons][2], filled with the minimum and maximum temperature in the biome for a certain season. All values go from 0 to 1. \n " + 
		"The index for each Season needs to match the one declared as parameter. \n" )]
		public int[,] temperatures;

		public Biome(double[,] probabilities, int[,] intensities, int[,] temperatures){
			this.probaibilities = probabilities;
			this.intensities = intensities;
			this.temperatures = temperatures; 
			
		}
		
		public int[,] GetIntensities (){return intensities;}
		public int GetIntensity(int season, int weatherCondition){return intensities[season,weatherCondition];}
		public double[,] GetProbabilities(){return probaibilities; }
		public double GetProbability(int season, int weatherCondition){return probaibilities[season,weatherCondition];}
        //TODO: falta get and set int[][] temperatures
        
            //// Accessing array elements.
        //System.Console.WriteLine(array2D[0, 0]);

        void Awake(){}
		void start(){}
		void update(){}
	}
}
