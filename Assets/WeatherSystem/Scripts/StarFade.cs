using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace weatherSystem {
    public class StarFade : MonoBehaviour{		
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
		[Tooltip("In game minutes the satrs will take to transition from transparent to fully visible, no matter the amount")]
		public double minutesToFade = 15;
		
		
		private CentralClock clock;
		private new MeshRenderer renderer;
		private bool daytime;
		private int phase;
		private float opacityChangePerSecond;
		private Time previousTime;
		private Time currentTime;
        private Renderer rend;

		const int FADED = 0;
        const int FADING_IN = 1;
        const int SHINING = 2;
        const int FADING_OUT = 3;
		
		private bool changeOpacity(float change){
			bool transitionCompleted = false;
			
			float current = renderer.material.color.a ;
			float newOpacity = current + change;
			if(newOpacity>=1){ 
				transitionCompleted = true;
				newOpacity = 1;
			}else if(newOpacity <= 0){
				transitionCompleted = true;
				newOpacity  = 0;
			}

          
        Color color = rend.material.color;
			color.a = newOpacity;
			renderer.material.color = color;
			
			return transitionCompleted;
		}
		
		private void setOpacity(float opacity){
			Color color = rend.material.color;
			color.a = opacity;
			renderer.material.color = color;
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
			
			opacityChangePerSecond = 1/((float)minutesToFade * 60);

            rend = GetComponent<Renderer>();
            if(rend == null) {
                Debug.Log("STARFADE: GameObject containing the script needs a renderer");
            }
        }
		
		
		void Start(){
			currentTime = clock.getCurrentTime();
			Time sunrise = clock.getSunriseTime();
			Time sunset = clock.getSunsetTime();
			Time fadeInStart =  sunrise.TimeWithSeconds(sunrise, - (int)( minutesToFade*60));
			Time fadeOutEnd = sunset.TimeWithSeconds (sunset, (int) (minutesToFade*60));
			if(currentTime.CompareTo(fadeInStart)<0){
				phase = SHINING;
				setOpacity(1); 
			}else if(currentTime.CompareTo(fadeInStart)>=0 && currentTime.CompareTo(sunrise)<0){
				phase = FADING_OUT;
				float decrease = - currentTime.SecondsBetween(sunrise.TimeWithSeconds(sunrise, - (int) (minutesToFade*60)))*opacityChangePerSecond;
				setOpacity(1-decrease); 
			}else if(currentTime.CompareTo(sunrise)>=0 && currentTime.CompareTo(sunset)<0){
				phase = FADED;
				setOpacity(0);
			}else if(currentTime.CompareTo(sunset)>=0 && currentTime.CompareTo(fadeOutEnd)<0){
				phase = FADING_IN;
				float increase =currentTime.SecondsBetween(sunset)*opacityChangePerSecond;
				setOpacity(increase);
			}else{
				phase = SHINING;
                setOpacity(1);
			}
			
			previousTime = currentTime.Clone();
		}
		
		void Update(){
			currentTime = clock.getCurrentTime();
			Time sunset = clock.getSunsetTime();
			Time sunrise = clock.getSunriseTime();
            bool completed;
            float increase = 0;
            float decrease = 0;

            switch (phase){
				case FADING_IN:
					//TODO: falta saber cuando parar de incrementar
					increase = currentTime.SecondsBetween(previousTime)*opacityChangePerSecond;
					completed = changeOpacity(increase);
                    if (completed){
						//TODO: por paranoia, comprobar que currentTime = sunset  o mayor. si es menor la he liado con los calculos 
						if(currentTime.CompareTo(sunset.TimeWithSeconds(sunset, (int)(minutesToFade*60)))<0){
							Debug.LogError("Eva eres tonta y tus matematicas necesitan repaso, La fase FADING_IN ha acabado antes de lo que deberia");
							Debug.Break();
						}
						phase = SHINING;
					}
					break;
				case FADING_OUT:
					decrease = - (currentTime.SecondsBetween(previousTime)*opacityChangePerSecond);
					completed = changeOpacity(decrease);
					if(completed) {
						//TODO: por paranoia, comprobar que currentTime >= sunset + minutesToFade, si no es asi la he liado porque soy tonta :)
						if(currentTime.CompareTo(sunrise)<0){
							Debug.LogError("Eva eres tonta y tus matematicas necesitan repaso, La fase FADING_OUT ha acabado antes de lo que deberia");
							Debug.Break();
						}
						phase = FADED;
					}
					break;
				case FADED:
					if(currentTime.CompareTo(sunset)>=0){
						phase = FADING_IN;
						increase =currentTime.SecondsBetween(sunset)*opacityChangePerSecond;
						changeOpacity(increase);
					}
					break;
				case SHINING:
					if (currentTime.CompareTo(sunrise)>=0 && currentTime.CompareTo(sunset)<0){
						phase = FADING_OUT;
						decrease = - currentTime.SecondsBetween(sunrise.TimeWithSeconds(sunrise, -(int) (minutesToFade*60)))*opacityChangePerSecond;
						changeOpacity(decrease);
					}
					break;
			}

			previousTime = currentTime.Clone();
		}		
    }
}
