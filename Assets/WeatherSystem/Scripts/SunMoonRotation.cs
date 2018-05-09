using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Nota para mi: quizá sea mejor cambiar sunrise y sunset por sonrise y sunset, por esas de que resulta que no son sinonimos y tal.ñ
namespace weatherSystem {
    public class SunMoonRotation : MonoBehaviour{
		
		
		#region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
		[Tooltip("Check if the object represents the sun, if it represents a moon then leave it unchecked")]        
        public bool sun;
		#endregion Public Atributes
		
		#region Private Atributes
		//private Variables
		private CentralClock clock; 
        private Time previousTime = new Time(0, 0, 0);
        private Time currentTime;
        private Time nextSunrise;
        private Time nextSunset;
        private bool daytime;
        private float rotationSpeed;
		private float previousInclinationAngle = 0.0f; 
		//private Vector3 sunRotationAxis;
		#endregion Private Atributes
	   public void Awake(){
			//Clock not null check
            clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("SUN/MOON ROTATION: No clock could be found, please remember to tag the object with the CentralClock script");
                Debug.Break();
            }
			//not sun warning
            if (!sun)
                Debug.LogWarning("SUN/MOON ROTATION" + this.name + ": has sun value set so false, it will be asumed a moon");
        }

        // Use this for initialization
        void Start(){
            currentTime = clock.getCurrentTime();
            //recordar que tanto el sol como la luna se tienen que dejar en el editor en la posicion de medianoche         
			
			Time startDaySunrise = clock.getSunriseTime();
			Time startDaySunset = clock.getSunsetTime();

            Time start = previousTime;
            Time end = clock.getCurrentTime();

            if (debug) {
                Debug.Log("INITIALIZING SUN/MOON POSITION-----------------------");
                Debug.Log("dayTime: " + daytime + "\n" +
                         "start: " + start.ToString() + "\t end: " + end.ToString());
            }
				 
			//the first speed will always be the night speed becouse we are forcing the user to set the environment as midnight
			//TODO: change sun moon rotation so that the initial position is at midday not midnight
			
			//night speed:
			Date yesterday = clock.getCurrentDate().DateWithDays(-1);
			Time yesterdaySunset = clock.getSunsetTime(yesterday);

            if (debug) {
                Debug.Log("  CALCULATING INITIAL SPEED -------------------------");
                Debug.Log("SUN/MOON ROTATION" + this.name + ": yesterday: " + yesterday);
                Debug.Log("SUN/MOON ROTATION" + this.name + ": Yesterda's sunset: " + yesterdaySunset.ToString() + "\n" + 
                        "today sunrise: " + startDaySunrise);
                Debug.Log("SUN/MOON ROTATION" + this.name + ": Time between them: " + yesterdaySunset.SecondsBetween(startDaySunrise));
            }
			int PhaseTotalTime = yesterdaySunset.SecondsBetween(startDaySunrise);
            rotationSpeed = 180.0f / PhaseTotalTime; //speed in degrees per second
			int timePased;
			float rotationAngle;
			
			if(end.CompareTo(startDaySunrise)>=0){
				if(debug){
					Debug.Log("SUN/MOON ROTATION" + this.name + ": end time is past sunrise, therefore the total movement will be seccioned. \n" +
 					"the firs secction is from start: " + start.ToString() + "  to sunrise: " + startDaySunrise.ToString());
					Debug.Break();
				}
                Time provisionalEnd = startDaySunrise.Clone();
				timePased = start.SecondsBetween(provisionalEnd);
				rotationAngle = rotationSpeed * timePased;
				if(debug){
					Debug.Log("SUN/MOON ROTATION" + this.name + ": soconds passed from start to end: " + timePased + "\n" 
					+ " the speed used is: " + rotationSpeed + "degrees per second");
					Debug.Log("SUN/MOON ROTATION" + this.name + ": new rotation angle generated: " + rotationAngle);
					Debug.Break();
				}
				transform.RotateAround(Vector3.zero,Vector3.right, rotationAngle );
				start = startDaySunrise.Clone();
				
				daytime = true;
				//dayTime speed calculations
				
				PhaseTotalTime = startDaySunrise.SecondsBetween(startDaySunset);
				rotationSpeed = 180.0f / PhaseTotalTime;
				
				if(end.CompareTo(startDaySunset)>=0){
					if(debug){
						Debug.Log("SUN/MOON ROTATION" + this.name + ": end time is past sunset, therefore the remaining movement will be secctioned. \n" + 
						"The firs section is from start: "+ start.ToString() + "  to sunset: " + startDaySunset.ToString() );
						Debug.Break();
					}
                    provisionalEnd = startDaySunset.Clone();
					timePased = start.SecondsBetween(provisionalEnd);
					rotationAngle = rotationSpeed * timePased;
					if(debug){
						Debug.Log("SUN/MOON ROTATION" + this.name + ": soconds passed since sunrise: " + timePased + "\n" + 
							"new rotation angle generated: " + rotationAngle);
						Debug.Break();
					}
                    transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
                    start = startDaySunset.Clone();
				
					daytime = false;
					
					PhaseTotalTime = startDaySunset.SecondsBetween(clock.getNextSunriseTime());
					rotationSpeed = 180.0f / PhaseTotalTime;
					timePased = start.SecondsBetween(end);
					rotationAngle = rotationSpeed * timePased;
					
					if(debug){
						Debug.Log("SUN/MOON ROTATION" + this.name + ": soconds passed since sunset: " + timePased + "\n" + 
							"new rotation angle generated: " + rotationAngle);
						Debug.Break();
					}
                    transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);

                }
                else{
					timePased = start.SecondsBetween(end);
					rotationAngle = rotationSpeed * timePased;
					if(debug){
						Debug.Log("SUN/MOON ROTATION" + this.name + ": soconds passed since sunrise: " + timePased + "\n" + 
							"new rotation angle generated: " + rotationAngle);
						Debug.Break();
					}
                    transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);

                    daytime = true;
				}
				
			}else{
				timePased = start.SecondsBetween(end);
				rotationAngle = rotationSpeed * timePased;
				if(debug){
					Debug.Log("SUN/MOON ROTATION" + this.name + ": soconds passed since midnight: " + timePased + "\n" + 
						"new rotation angle generated: " + rotationAngle);
						Debug.Break();
				}
                transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);

                daytime = false;
			}
			
			////INCLINATION ANGLE CHANGE:
			//float inclinationAngle;
			//float hoursOfLight = startDaySunrise.SecondsBetween(startDaySunset)/3600.0f;
			//if(hoursOfLight>= 12){
			//	if(!sun){
			//		double hoursOfDarkness = 24  - hoursOfLight;
			//		inclinationAngle = (float)(hoursOfDarkness*90.0f/12.0 ) - previousInclinationAngle;
			//	}else{
			//		inclinationAngle = 0-previousInclinationAngle;
			//	}

			//}else{
			//	if(sun){
			//		inclinationAngle = (float)(hoursOfLight*90.0/12.0) - previousInclinationAngle;
			//	} else{
			//		double hoursOfDarkness = 24 - hoursOfLight;
			//		if(hoursOfDarkness <12){
			//			inclinationAngle = (float)(hoursOfDarkness*90.0/12.0) - previousInclinationAngle;
			//		}else{
			//			inclinationAngle = 0-previousInclinationAngle;
			//		}
			//	}
			//}
			//transform.RotateAround(Vector3.zero, Vector3.forward, inclinationAngle);
			//previousInclinationAngle = inclinationAngle;
			
			
			previousTime = currentTime.Clone();
        }

        void Update(){
            currentTime = clock.getCurrentTime();
			Time sunset = clock.getSunsetTime();
			Time sunrise = clock.getSunriseTime();
			Time midday = clock.GetMiddayTime();
			
			
			////INCLINATION ANGLE CHANGE:
			//if(sun && currentTime.CompareTo(previousTime)<0){ //it is midnight
			//	if(debug){
			//		Debug.Log("SUN/MOON ROTATION" + this.name + " is a sun and it is aproximately midnight, therefore te inclination angle is going to change \n" + 
			//				"currentTime: " + currentTime.ToString());
			//		Debug.Break();
			//	}
			//	float inclinationAngle;
			//	float hoursOfLight = sunrise.SecondsBetween(sunset)/3600.0f;
			//	if(hoursOfLight < 12){
			//		inclinationAngle = (float)(hoursOfLight*90.0/12.0) - previousInclinationAngle;
			//		if(debug){
			//			Debug.Log("SUN/MOON ROTATION" + this.name + ": new day sunrise: " + sunrise + "\t new day sunset: " + sunset + "\n" +
			//				"calculated hours of ligh: " + hoursOfLight + "\t calculated angle change: " + inclinationAngle);
			//			Debug.Break();
			//		}
			//	}else{
			//		inclinationAngle = 0-previousInclinationAngle;
			//		if(debug){
			//			Debug.Log("SUN/MOON ROTATION" + this.name + ": new day sunrise: " + sunrise + "\t new day sunset: " + sunset + "\n" +
			//				"calculated hours of ligh: " + hoursOfLight + "\t calculated angle change: " + inclinationAngle);
			//			Debug.Break();
			//		}
			//	}
			//	transform.RotateAround(Vector3.zero, Vector3.forward, inclinationAngle);
			//	previousInclinationAngle = inclinationAngle;
			//	if(debug){
			//		Debug.Break();
			//	}
			//}else if(!sun && currentTime.CompareTo(midday)>=0 && currentTime.CompareTo(midday)<0){
			//	if(debug){
			//		Debug.Log("SUN/MOON ROTATION" + this.name + " is a moon and it is aproximately midday, therefore te inclination angle is going to change \n" + 
			//				"currentTime: " + currentTime.ToString());
			//		Debug.Break();
			//	}
			//	double hoursOfDarkness = 24  - (sunrise.SecondsBetween(sunset)/3600.0f);
			//	double hoursOfDarknessB = sunset.SecondsBetween(clock.getNextSunriseTime());
			//	Debug.Log("COMPROBANDO TEORIA: " + (hoursOfDarkness == hoursOfDarknessB));
			//	float inclinationAngle = (float)(hoursOfDarkness*90.0f/12.0 ) - previousInclinationAngle;
			//	if(debug){
			//		Debug.Log("SUN/MOON ROTATION" + this.name + ": sunset: " + sunset + "\t next sunrise: "+ clock.getNextSunriseTime() + "\n" +
			//				"calculated hours of darkness: " + hoursOfDarkness + "\t calculated angle change: " + inclinationAngle);
			//		Debug.Break();
			//	}
			//	transform.RotateAround(Vector3.zero, Vector3.forward, inclinationAngle);
			//	previousInclinationAngle = inclinationAngle;
			//	if(debug){
			//		Debug.Break();
			//	}
			//}				
			
			//SUN AND MOON ROTATION AROUND THE TERRAIN
			//check if sunset has been reached		
			if(daytime && currentTime.CompareTo(sunset)>=0){
				if(debug){
					Debug.Log("REACHED SUNSET -------------------------");
				}
				daytime = false;
				int timePased = previousTime.SecondsBetween(sunset);
				float rotationAngle = rotationSpeed*timePased;
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": applying rotation to get to sunset position: " + rotationAngle);
                    Debug.Break();
                }

                transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
                previousTime = sunset.Clone();

                //new rotation speed:
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": --Calculating new rotation speed: -------------- ");
                }
				Time start = sunset.Clone();
				Time end = clock.getNextSunriseTime();
                int secondsBetweenPhases = start.SecondsBetween(end);
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": start: " + start.ToString() + "\t end: " + end.ToString() + "\n" +
                            "Seconds between them: " + secondsBetweenPhases);
                }
                rotationSpeed = 180.0f / secondsBetweenPhases;
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": new rotationspeed: " + rotationSpeed);
					Debug.Break();
                }
				//esta parte esta aqui solo para depurar y no llenar la pantalla de mensajes en cada update, pero a la larga hay que quitarlo
				if(previousTime!= currentTime){
                    if (debug) {
                        Debug.Log("SUN/MOON ROTATION" + this.name + ": Completar la rotacion desde atardecer a currentTime");
                    }
                    timePased = previousTime.SecondsBetween(currentTime);
					rotationAngle = rotationSpeed*timePased;
					transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
				}
            }
            //check if sunrise has been reached
            if (!daytime && currentTime.CompareTo(sunrise)>=0 && currentTime.CompareTo(sunset)<0){
				if(debug){
					Debug.Log("SUN/MOON ROTATION" + this.name + ": REACHED SUNRISE --------------------------");
					Debug.Break();
				}
				daytime = true;
				int timePased = previousTime.SecondsBetween(sunrise);
				float rotationAngle = rotationSpeed*timePased;
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": applying rotation to get to sunrise position: " + rotationAngle);
                    Debug.Break();
                }
                transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
                previousTime = sunrise.Clone();

                //new rotation speed:
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": --Calculating new rotation speed: -------------- ");
                }
                Time start = sunrise.Clone();
				Time end = clock.getNextSunsetTime();
                int secondsBetweenPhases = start.SecondsBetween(end);
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": start: " + start.ToString() + "\t end: " + end.ToString() + "\n" +
                            "Seconds between them: " + secondsBetweenPhases);
                }
                rotationSpeed = 180.0f/secondsBetweenPhases;
                timePased = sunrise.SecondsBetween(currentTime);
                rotationAngle = rotationSpeed * timePased;
                if (debug) {
                    Debug.Log("SUN/MOON ROTATION" + this.name + ": new rotationspeed: " + rotationSpeed);
					Debug.Break();
                }
				
            }
			
			if(previousTime!= currentTime){
				int timePased = previousTime.SecondsBetween(currentTime);
				float rotationAngle = rotationSpeed*timePased;
				transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
            }			
			
			
			
			previousTime = currentTime.Clone();			
        }
    }

}