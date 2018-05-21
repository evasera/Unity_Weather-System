using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem{
    public class Time : System.IComparable {
        private int hours;
        private int minutes;
        private int seconds;

		#region Constructors:
        public Time(int hours, int minutes, int seconds){
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }
        public Time TimeWithSeconds(Time time, int seconds){
            int t = time.ToSeconds() + seconds;
            return secondsToTime(t);
        }
		public Time Clone(){
			return new Time(hours, minutes, seconds);
		}
		#endregion Constructors
		//getters and Setters
		
		//other operations
		public int ToSeconds(){
            return hours * 3600 + minutes * 60 + seconds;
        }
        public Time secondsToTime(int seconds){//86440
            int h = seconds / 3600;
            seconds = seconds % 3600;
            int m = seconds / 60;
            seconds = seconds % 60;
            return new Time(h, m, seconds);
        }
        override public string ToString(){
            string result = "";

            result += hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString();
            return result;
        }
        public int add(int s){
            int sec = ToSeconds() + s;
            int daysChanged = 0;
			while(sec <0){
				daysChanged --; 
				sec += 24*3600;
			}
            Time newtime = secondsToTime(sec);
            while (newtime.hours >= 24)
            {
                daysChanged++;
                newtime.hours -= 24;
            }
            hours = newtime.hours;
            minutes = newtime.minutes;
            seconds = newtime.seconds;
            return daysChanged;
        }

        public int CompareTo(object o)
        {
            int result = 0;
            if (o == null) return 1;

            Time other = o as Time;
            if (o == null)
            {
                Debug.LogError("Object " + o.ToString() + " could not be converted to Time");
                Debug.Break();
            }
            if (this.hours > other.hours) result = 1;
            else if (this.hours < other.hours) result = -1;
            else if (this.minutes > other.minutes) result = 1;
            else if (this.minutes < other.minutes) result = -1;
            else { result = this.seconds - other.seconds;}

            return result;
        }

        public int SecondsBetween(Time other)        {
            if (other == null)            {
                Debug.LogError("Time daysBeween recived a null object ");
                Debug.Break();
            }
            int compare = CompareTo(other);
            if (compare == 0) return 0;
            if (compare > 0) return (24*3600) - other.SecondsBetween(this);
            // ahora que sabemos que es menor:
            int result = 0;

            result = (other.seconds - seconds) + (other.minutes - minutes) * 60 + (other.hours - hours) * 3600;
            return result;
        }
    }
}