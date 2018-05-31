using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class WeatherState{
		
		#region Public Atributes
		private int clouds;
		private int rain;
		private int snow;
		private int lightning;
		#endregion Public Atributes
		
		#region Constructors
		public WeatherState( int clouds, int rain, int lightning, int snow){
			this.clouds = clouds;
			this.rain = rain;
			this.lightning = lightning;
			this.snow = snow;
		}
		
		#endregion Constructors
		
		#region Gettes and Setters    
		public int GetClouds(){return clouds;}
		public int GetRain(){return rain;}
		public int GetSnow(){return snow;}
		public int GetLightning(){return lightning;}
		public void SetClouds(int c){clouds = c;}
		public void SetRain(int r) {rain = r;}
		public void SetSnow( int s){snow = s;}
		public void SetLightning(int l) {lightning = l;}
		#endregion Getters and Setters
		
		public override string ToString(){
			string result = "Cloud Coverage: " + clouds + "\n \t" 
								+ "Rain intensity: " + rain + "\n \t"
								+ "Lightning: " + lightning + "\n \t"
								+ "Snow Intensity: " + snow;
            return result;
		}
	}
}
