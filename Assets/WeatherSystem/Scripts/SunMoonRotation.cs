using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Nota para mi: quizá sea mejor cambiar sunrise y sunset por sonrise y sunset, por esas de que resulta que no son sinonimos y tal.ñ
namespace weatherSystem {
    public class SunMoonRotation : MonoBehaviour{
		
		
		#region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
		
		//public variables (User Configuration)
        public CentralClock clock; //TODO: hacer el sol privado y buscarlo por tag en awake
        public bool sun;
		#endregion Public Atributes
		#region Private Atributes
		//private Variables
        private Time previousTime = new Time(0, 0, 0);
        private Time currentTime;
        private Time nextSunrise;
        private Time nextSunset;
        private bool daytime;
		private static bool sunsetReached= false;
		private static bool sunriseReached = false;
        private float rotationSpeed;
		private float sunRotationAxis;
		//private Vector3 sunRotationAxis;
		#endregion Private Atributes
		#region Event System
		//registering on events is done on awake()
		static void SunsetReached(object sender, System.EventArgs e){
			sunsetReached = true;
            Debug.Log("Sun/moon rotation detected sunset event" );
		}
		static void SunriseReached(object sender, System.EventArgs e){
			sunriseReached= true;
            Debug.Log("Sun/moon rotation detected sunrise event");
        }
		#endregion Event System

	   public void Awake(){
			//Clock not null check
            if (clock == null){
                Debug.LogError("SunMoonRotation clock must be specified");
                Debug.Break();
            }
			//not sun warning
            if (!sun)
                Debug.LogWarning(this.name + " has sun value set so false, it will be asumed a moon");
			
			//Registering on relevant events:
			clock.sunsetReached += SunsetReached;
			clock.sunriseReached += SunriseReached;
        }

        // Use this for initialization
        void Start(){
            currentTime = clock.getCurrentTime();
            //recordar que tanto el sol como la luna se tienen que dejar en el editor en la posicion de medianoche

            // getting currentTime, nextSunrise and nextSunset from cenctal clock. 
            //PS: currentTime is  passed as reference, wich means it will always be updated, no need to call the method every frame
             
			
			Time startDaySunrise = clock.getSunriseTime();
			Time startDaySunset = clock.getSunsetTime();

            Time start = previousTime;
            Time end = clock.getCurrentTime();

				
            //Speed and transformation
            if (debug) {
                Debug.Log("INITIALIZING SUN/MOON POSITION-----------------------");
                Debug.Log("dayTime: " + daytime + "\n" +
                         "start: " + start.ToString() + "\t end: " + end.ToString());
            }
				 
			//the first speed will always be the night speed becouse we are forcing the user to set the environment as midnight
			//TODO: change sun moon rotation so that the initial position is at midday not midnight
			
			//night speed:
			Date yesterday = new Date (clock.GetDay(), clock.getMonth(), clock.GetYear());
			yesterday.SubstractDay(1);
			Time yesterdaySunset = clock.getSunsetTime(yesterday);

            if (debug) {
                Debug.Log("  CALCULATING INITIAL SPEED -------------------------");
                Debug.Log("yesterday: " + yesterday);
                Debug.Log("Yesterda's sunset: " + yesterdaySunset.ToString() + "\n" + 
                        "today sunrise: " + startDaySunrise);
                Debug.Log("Time between them: " + yesterdaySunset.SecondsBetween(startDaySunrise));
            }
			int PhaseTotalTime = yesterdaySunset.SecondsBetween(startDaySunrise);
            rotationSpeed = 180.0f / PhaseTotalTime; //speed in degrees per second
			int timePased;
			float rotationAngle;
			
			if(end.CompareTo(startDaySunrise)>=0){
				if(debug){
					Debug.Log("end time is past sunrise, therefore the total movement will be seccioned. \n" +
 					"the firs secction is from start: " + start.ToString() + "  to sunrise: " + startDaySunrise.ToString());
					Debug.Break();
				}
                Time provisionalEnd = startDaySunrise.Clone();
				timePased = start.SecondsBetween(provisionalEnd);
				rotationAngle = rotationSpeed * timePased;
				if(debug){
					Debug.Log("soconds passed from start to end: " + timePased + "\n" 
					+ " the speed used is: " + rotationSpeed + "degrees per second");
					Debug.Log("new rotation angle generated: " + rotationAngle);
					Debug.Break();
				}
				
				if(rotationAngle<89||rotationAngle>91 && debug){
					Debug.LogError("The angle increase might not be correct, expected: 90   calculated: " + rotationAngle);
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
						Debug.Log("end time is past sunset, therefore the remaining movement will be secctioned. \n" + 
						"The firs section is from start: "+ start.ToString() + "  to sunset: " + startDaySunset.ToString() );
						Debug.Break();
					}
                    provisionalEnd = startDaySunset.Clone();
					timePased = start.SecondsBetween(provisionalEnd);
					rotationAngle = rotationSpeed * timePased;
					if(debug){
						Debug.Log("soconds passed since sunrise: " + timePased + "\n" + 
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
						Debug.Log("soconds passed since sunset: " + timePased + "\n" + 
							"new rotation angle generated: " + rotationAngle);
						Debug.Break();
					}
                    transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);

                }
                else{
					timePased = start.SecondsBetween(end);
					rotationAngle = rotationSpeed * timePased;
					if(debug){
						Debug.Log("soconds passed since sunrise: " + timePased + "\n" + 
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
					Debug.Log("soconds passed since midnight: " + timePased + "\n" + 
						"new rotation angle generated: " + rotationAngle);
						Debug.Break();
				}
                transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);

                daytime = false;
			}
			
			previousTime = currentTime.Clone();
			
			if(sun){
				if(debug){
					Debug.Log("Calculating sun rotation on z axis (inclination): -------");
				}
				int secondsOfLight = startDaySunrise.SecondsBetween(startDaySunset); 
				float angle = secondsOfLight*90/(12*3600);
				angle =Mathf.Max(0, Mathf.Min(angle, 1));
				if(debug){
					Debug.Log("Seconds of light: " + secondsOfLight + "\n" +
							"Axis angle: " + angle);
					Debug.Break();
				}
				transform.RotateAround(Vector3.zero, Vector3.forward, angle);					
				
			}else{
				if(debug){
					Debug.Log("Calculating moon rotation on z axis (inclination): -------");
				}
				int secondsOfDarkness = 24*3600 - startDaySunrise.SecondsBetween(startDaySunset); 
				float angle = secondsOfDarkness*90/(12*3600);
				angle = Mathf.Max(0, Mathf.Min(angle, 1));
                if (debug){
					Debug.Log("Seconds of darkness: " + secondsOfDarkness + "\n" +
							"Axis angle: " + angle);
					Debug.Break();
				}
				transform.RotateAround(Vector3.zero, Vector3.forward, angle);
			}
			
			if(debug){
				Debug.Break();
			}
        }

        void Update(){
            currentTime = clock.getCurrentTime();
			Time sunset = clock.getSunsetTime();
			Time sunrise = clock.getSunriseTime();
			

            //Sun Rotation Axis Change
            Time midnight = clock.GetMidnightTime();
			Time midday = clock.GetMiddayTime();
			if(sun){
				if(previousTime.CompareTo(new Time(23,59,59)) <=0 && currentTime.CompareTo(midnight)>= 0){
					if(debug)
                        Debug.Log("Calculating moon rotation on z axis (inclination): -------");
					int secondsOfLight = sunset.SecondsBetween(sunrise);
					float angle = secondsOfLight*90/(12*3600);
					angle = Mathf.Max(0, Mathf.Min(angle, 1));
                    float oldAngle = transform.rotation.z;
					if(debug){
						Debug.Log("Seconds of light: " + secondsOfLight + "\n" +
								"Axis angle: " + angle);
						Debug.Break();
					}
					transform.RotateAround(Vector3.zero, Vector3.forward, angle);	
				}
			}else{
				if(previousTime.CompareTo(midday)<0 && currentTime.CompareTo(midday)>=0){
					if(debug){
						Debug.Log("Calculating moon rotation on z axis (inclination): -------");
					}
					int secondsOfDarkness = 24*3600 - sunrise.SecondsBetween(sunset); 
					float angle = secondsOfDarkness*90/(12*3600);
					angle = Mathf.Max(0, Mathf.Min(angle, 1));
                    if (debug){
						Debug.Log("Seconds of darkness: " + secondsOfDarkness + "\n" +
							"Axis angle: " + angle);
						Debug.Break();
					}
					transform.RotateAround(Vector3.zero, Vector3.forward, angle);
				}
			}
			
			//check if sunrise or sunset have been reached		
			if(sunsetReached){
				if(debug){
					Debug.Log("REACHED SUNSET -------------------------");
				}
				daytime = false;
				sunsetReached = false;
				int timePased = previousTime.SecondsBetween(sunset);
				float rotationAngle = rotationSpeed*timePased;
                if (debug) {
                    Debug.Log("applying rotation to get to sunset position: " + rotationAngle);
                    Debug.Break();
                }

                transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
                previousTime = sunset.Clone();

                //new rotation speed:
                if (debug) {
                    Debug.Log("--Calculating new rotation speed: -------------- ");
                }
				Time start = sunset.Clone();
				Time end = clock.getNextSunriseTime();
                int secondsBetweenPhases = start.SecondsBetween(end);
                if (debug) {
                    Debug.Log("start: " + start.ToString() + "\t end: " + end.ToString() + "\n" +
                            "Seconds between them: " + secondsBetweenPhases);
                }
                rotationSpeed = 180.0f / secondsBetweenPhases;
                if (debug) {
                    Debug.Log("new rotationspeed: " + rotationSpeed);
					Debug.Break();
                }
				//esta parte esta aqui solo para depurar y no llenar la pantalla de mensajes en cada update, pero a la larga hay que quitarlo
				if(previousTime!= currentTime){
                    if (debug) {
                        Debug.Log("Completar la rotacion desde atardecer a currentTime");
                    }
                    timePased = previousTime.SecondsBetween(currentTime);
					rotationAngle = rotationSpeed*timePased;
					transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
				}
            }
			if(sunriseReached){
				if(debug){
					Debug.Log("REACHED SUNRISE --------------------------");
					Debug.Break();
				}
				daytime = true;
				sunriseReached = false;
				int timePased = previousTime.SecondsBetween(sunrise);
				float rotationAngle = rotationSpeed*timePased;
                if (debug) {
                    Debug.Log("applying rotation to get to sunrise position: " + rotationAngle);
                    Debug.Break();
                }
                transform.RotateAround(Vector3.zero, Vector3.right, rotationAngle);
                previousTime = sunrise.Clone();

                //new rotation speed:
                if (debug) {
                    Debug.Log("--Calculating new rotation speed: -------------- ");
                }
                Time start = sunrise.Clone();
				Time end = clock.getNextSunsetTime();
                int secondsBetweenPhases = start.SecondsBetween(end);
                if (debug) {
                    Debug.Log("start: " + start.ToString() + "\t end: " + end.ToString() + "\n" +
                            "Seconds between them: " + secondsBetweenPhases);
                }
                rotationSpeed = 180.0f/secondsBetweenPhases;
                timePased = sunrise.SecondsBetween(currentTime);
                rotationAngle = rotationSpeed * timePased;
                if (debug) {
                    Debug.Log("new rotationspeed: " + rotationSpeed);
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