using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace weatherSystem {
    public class StarFade : MonoBehaviour{		
		
		#region Constants
		const int FADED = 0;
        const int FADING_IN = 1;
        const int SHINING = 2;
        const int FADING_OUT = 3;
        #endregion Constants

        #region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
		[Tooltip("In game minutes the satrs will take to transition from transparent to fully visible, no matter the amount")]
		public double minutesToFade = 60;
		#endregion Public Atributes
		
		#region Private Atributes
		private CentralClock clock;
		private new MeshRenderer renderer;
		private bool daytime;
		private int phase;
		private float opacityChangePerSecond;
		private Time previousTime;
		private Time currentTime;
        private Renderer rend;
		#endregion Private Atributes
		
		

		
		private bool changeOpacity(float change){
			bool transitionCompleted = false;
			
			float current = renderer.material.color.a ;
			float newOpacity = current + change;
			
			if(debug){
				Debug.Log("STARFADE: CHANGING OPACITY --------");
				Debug.Log("STARFADE: current opacity: " + current + "\t change: " + change + "\n" + 
							"new opacity: " + newOpacity);
				Debug.Break();
			}
			if(newOpacity>=1){ 
				if(debug){
					Debug.Log("STARFADE: new opacity found to be equal or greater than 1, transition completed and opacity set to 1");
					Debug.Break();
				}
				transitionCompleted = true;
				newOpacity = 1;
			}else if(newOpacity <= 0){
				if(debug){
					Debug.Log("STARFADE: new opacity found to be equal or lower than 0, transition completed and opacity set to 0");
					Debug.Break();
				}
				transitionCompleted = true;
				newOpacity  = 0;
			}
			
			Color color = rend.material.color;
			color.a = newOpacity;
			rend.material.color = color;
			
			return transitionCompleted;
		}
		
		private void setOpacity(float opacity){
			Color color = rend.material.color;
			color.a = opacity;
			rend.material.color = color;
		}
		
		
		void Awake(){
			clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
            if (clock == null) {
                Debug.LogError("STARFADE " + this.name + ": No clock could be found, please remember to tag the object with the CentralClock script with the tag 'Clock' ");
                Debug.Break();
            }
			
			if(minutesToFade <0){
				Debug.LogError("STARFADE " + this.name + ": The minutesToFade parameter can not be negative");
			}
			
			renderer = GetComponent<MeshRenderer>();
            if (renderer == null) {
                Debug.LogError("STARFADE: GameObject " + this.name + "Needs a Renderer to make moonPhases visible"); 
                Debug.Break();
            }

            rend = GetComponent<Renderer>();
            if(rend == null) {
                Debug.LogError("STARFADE: GameObject containing the script needs a renderer");
            }

            opacityChangePerSecond = 1.0f / ((float)minutesToFade * 60);
            //if (debug) {
            //    Debug.Log("OPACITY CHANGE PER SECOND: " + opacityChangePerSecond + "\n" + 
            //            "CURRENT OPACITY: " + rend.material.color.a);
            //}
        }
		
		
		void Start(){
			currentTime = clock.getCurrentTime();
			Time sunrise = clock.getSunriseTime();
			Time sunset = clock.getSunsetTime();
			Time fadeOutEnd =  sunrise.TimeWithSeconds(sunrise, (int)( minutesToFade*60));
			Time fadeInStart = sunset.TimeWithSeconds (sunset, -(int)(minutesToFade*60));
			
			if(debug){
				Debug.Log("STARFADE: CALCULATING INITIAL STATE ---------------------");
				Debug.Log("STARFADE: currentTime: " + currentTime.ToString() + "\n" + 
							"\t Sunrise: " + sunrise + "\t sunset: " + sunset );
				Debug.Log("STARFADE: Transition length (in minutes): " + minutesToFade + "\n" + 
							"\t Fade out start: " + fadeInStart + "\t Fade in end: " + fadeOutEnd );
				Debug.Break();
			}
			
			if(currentTime.CompareTo(sunrise)<0){
				phase = SHINING;
				if(debug){
					Debug.Log("STARFADE: Current time fount out to be bewteen midnight and sunrise\n"+
								"Phase set to " + phase +" SHINING, \t opacity set to 1");
					Debug.Break();
				}
				setOpacity(1); 

			}else if(currentTime.CompareTo(sunrise)>=0 && currentTime.CompareTo(fadeOutEnd)<0){
				phase = FADING_OUT;
				float decrease = - currentTime.SecondsBetween(sunrise.TimeWithSeconds(sunrise, - (int) (minutesToFade*60)))*opacityChangePerSecond; 
				if(debug){
					Debug.Log("STARFADE: Current time fount out to be bewteen sunrise and fadeOutEnd\n"+
								"Phase set to " + phase +" FADING_OUT, \t opacity set to :" + (1- decrease));
					Debug.Break();
				}
				setOpacity(1-decrease);
			}else if(currentTime.CompareTo(fadeOutEnd)>=0 && currentTime.CompareTo(fadeInStart)<0){
				phase = FADED;
				if(debug){
					Debug.Log("STARFADE: Current time fount out to be bewteen fadeOutEnd and fadeInStart \n"+
								"Phase set to " + phase +" FADED, \t opacity set to :" + (0));
					Debug.Break();
				}
				setOpacity(0);
			}else if(currentTime.CompareTo(fadeInStart)>=0 && currentTime.CompareTo(sunset)<0){
				phase = FADING_IN;
				float increase =currentTime.SecondsBetween(sunset)*opacityChangePerSecond;
				if(debug){
					Debug.Log("STARFADE: Current time fount out to be bewteen fadeInStart and sunset \n"+
								"Phase set to " + phase +" FADING_IN, \t opacity set to :" + (0 + increase));
					Debug.Break();
				}
				setOpacity(0+ increase);
			}else{
				phase = SHINING;
				if(debug){
					Debug.Log("STARFADE: Current time fount out to be past sunset and before midnight \n"+
								"Phase set to " + phase +" SHINING, \t opacity set to 1");
					Debug.Break();
				}
                setOpacity(1);
			}
			
			previousTime = currentTime.Clone();
		}
		
		void Update(){
			currentTime = clock.getCurrentTime();
			Time sunset = clock.getSunsetTime();
			Time sunrise = clock.getSunriseTime();
			Time fadeOutEnd  =  sunrise.TimeWithSeconds(sunrise, (int)( minutesToFade*60));
			Time fadeInStart =  sunset.TimeWithSeconds (sunset, -(int)( minutesToFade*60));
            bool completed;
            float increase = 0;
            float decrease = 0;

            switch (phase){
				case FADING_IN:
					increase = previousTime.SecondsBetween(currentTime)*opacityChangePerSecond;
					if(debug){
						Debug.Log("STARFADE: phase is FADING_IN, currentTime is: " + currentTime.ToString() + "\n" + 
								"\t fadeInStart: " + fadeInStart.ToString() + "\t sunset: " + sunset.ToString());
						Debug.Log("STARFADE: current opacity: " + rend.material.color.a + "\n" + 
								"previous time: " + previousTime.ToString()  + "\t increase: " + increase );
						Debug.Break();
					}
					completed = changeOpacity(increase);
                    if (completed){
						if(debug){
						    Debug.Log("STARFADE: After the increase, it is determined that the transition has been completed \n" + 
								"\t opacity: " + rend.material.color.a);
						    Debug.Break();
						}
						//TODO: por paranoia, comprobar que currentTime = sunset  o mayor. si es menor la he liado con los calculos 
						if(currentTime.CompareTo(sunset)<0){
							Debug.LogError("Eva eres tonta y tus matematicas necesitan repaso, La fase FADING_IN ha acabado antes de lo que deberia");
							Debug.Break();
						}
						phase = SHINING;
					}
					break;
				case FADING_OUT:
					decrease = -(previousTime.SecondsBetween(currentTime)*opacityChangePerSecond);
					if(debug){
						Debug.Log("STARFADE: phase is FADING_OUT, currentTime is: " + currentTime.ToString() + "\n" + 
								"sunrise: " + sunrise.ToString() + "\t fadeOutEnd: " + fadeOutEnd.ToString());
						Debug.Log("STARFADE: current opacity: " + rend.material.color.a + "\n" + 
								"previous time: " + previousTime.ToString()  + "\t increase: " + decrease );
						Debug.Break();
					}
					completed = changeOpacity(decrease);
					if(completed){
						if(debug){
						Debug.Log("STARFADE: After the decrease, it is determined that the transition has been completed \n" + 
								"\t opacity: " + rend.material.color.a);
						Debug.Break();
						}
						if(currentTime.CompareTo(fadeOutEnd)<0){
							Debug.LogError("Eva eres tonta y tus matematicas necesitan repaso, La fase FADING_OUT ha acabado antes de lo que deberia");
							Debug.Break();
						}
						phase = FADED;
					}
					break;
				case FADED:
					if(currentTime.CompareTo(fadeInStart)>=0){
						phase = FADING_IN;
						increase =fadeInStart.SecondsBetween(currentTime)*opacityChangePerSecond;
						if(debug){
							Debug.Log("STARFADE: while in phase FADED, fadeInStart has been reached \n" + 
										"\t currentTime: "+ currentTime.ToString() + "\t fadeInStart: " + fadeInStart);
							Debug.Log("STARFADE: increase calculated: " + increase);
							Debug.Break();
						}
						changeOpacity(increase);
					}
					break;
				case SHINING:
					
					if (currentTime.CompareTo(sunrise)>=0 && currentTime.CompareTo(fadeOutEnd)<0){
						phase = FADING_OUT;
                      
                        decrease = 0;
                        int secondsPased = sunrise.SecondsBetween(currentTime);
						decrease = - secondsPased*opacityChangePerSecond;
						if(debug){
							Debug.Log("STARFADE: while in phase SHINING, sunrise has been reached \n" + 
										"\t currentTime: "+ currentTime.ToString() + "\t sunrise: " + sunrise);
                            Debug.Log("STARFADE: seconds pased: " + secondsPased  + " compare: " + sunrise.CompareTo(currentTime) + "\n" + 
                                    "opacity change per second: " + opacityChangePerSecond);
							Debug.Log("STARFADE: increase calculated: " + increase);
							Debug.Break();
						}
						changeOpacity(decrease);
					}
					break;
			}

			previousTime = currentTime.Clone();
		}		
    }
}
