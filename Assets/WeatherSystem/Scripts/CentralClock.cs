using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
	//TODO: IMPORTANTE: recordar utilizar los metodos clone() cuando se quiere hacer una copia de una de las clases de datos (Tiempo y fecha)
	
    public class CentralClock : MonoBehaviour {
        #region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
        
		[Header ("Initial time data:")]
        public string Initial_date = "01/01/0001";
        public string Initial_time = "12:00:00";
        //public Season Initial_season; //no es necesario, se calcula a partir de la fecha inicial
        public string Initial_day_of_week;

        [Header ("Calendar settings:")]
        public double Day_length_in_minutes;
        public int Number_of_seasons;

        public int Days_in_Year;
        public string[] Day_names;

        [Header ("Day length settings")]
        public string summer_solstice_date = "dd/mm/yyyy";
        public int summer_solstice_hours_of_light;

        public string autumn_equinox_date = "dd/mm/yyyy";
        public int autumn_equinox_hours_of_light = 12;

        public string winter_solstice_date = "dd/mm/yyyy";
        public int winter_solstice_hours_of_light;

        public string spring_equinox_date = "dd/mm/yyyy";
        public int spring_equinox_hours_of_light = 12;
		#endregion Public Atributes
		#region Private Atributes
        private Month[] monthList;
        private Date currentDate;
        private double timeChangeRate;
		private Season currentSeason;
		private int daysSinceSeasonChange;
        private double seconds = 0.0;
        private Time currentTime;
        private Time midday= new Time(12,0,0);
        private Time midnight= new Time(0,0,0); //not quite midnight, but simplifies comparisons
        bool dayTime = false;

        private Date winterSolstice;
        private Date summerSolstice;
        private Date springEquinox;
        private Date autumnEquinox;
        //TODO: borrar estas lineas si el codigo funciona sin ellas (si el diccionario funciona, vamos)
		//private Time summerSolstice_Sunrise;
        //private Time summerSolstice_Sunset;
        //private Time winterSolstice_Sunrise;
        //private Time winterSolstice_Sunset;
        //private Time springEquinox_Sunrise;
        //private Time springEquinox_Sunset;
        //private Time autumnEquinox_Sunrise;
        //private Time autumnEquinox_Sunset;
        private double winterToSpringRate;
        private double springToSummerRate;
        private double summerToAutumnRate;
        private double autumnToWinterRate;
		private Dictionary<Date, Time>  sunriseTimes;
		private Dictionary<Date, Time>  sunsetTimes;
		#endregion Private Atributes
		#region Event System
		public event EventHandler sunsetReached;
		public event EventHandler sunriseReached;
		public event EventHandler middayReached;
		public event EventHandler midnightReached;
		
		//events: rising them is done on update
		protected virtual void OnSunsetReached(EventArgs e){
			EventHandler handler = sunsetReached;
			if(handler != null){
				handler(this, e);
			}
		}
		protected virtual void OnSunriseReached(EventArgs e){
            EventHandler handler = sunriseReached;
            if (handler != null){
				handler(this, e);
			}
		}
		protected virtual void OnMiddayReached(EventArgs e){
            EventHandler handler = middayReached;
            if (handler != null){
				handler(this, e);
			}
		}
		protected virtual void OnMidnightReached(EventArgs e){
            EventHandler handler = midnightReached;
            if (handler != null){
				handler(this, e);
			}
		}
		#endregion Event System
		
       #region Get and Set Methods
        public int GetNumberOfMonths() { return monthList.Length; }
        public Month[] getMonthList() {return monthList;}
        public Time getCurrentTime() { return currentTime; }
        public Date getCurrentDate() { return currentDate; }
		public Time GetMidnightTime() { return midnight; }
		public Time GetMiddayTime(){ return midday.Clone();}
		public int GetDay(){return currentDate.GetDay();}
		public Month getMonth () {return currentDate.GetMonth();}
		public int GetYear() {return currentDate.GetYear();}
		public Date GetWinterSolstice(){return winterSolstice.Clone();}
        public Date GetSpringEquinox() { return springEquinox.Clone(); }
		public double GetWinterToSpringRate(){return winterToSpringRate;}
        /*Returns Sunrise time of a given date*/
        public Time getSunriseTime(Date d){	
			if(debug)
				Debug.Log("---GETING SUNRISE FOR DATE: " + d.ToString()+"---------------");
            Time result = null;
            sunriseTimes.TryGetValue(d, out result);
			if(debug && result != null){
				Debug.Log("Date already on the collection, no need to calculate. \n Result: " + result.ToString());
                Debug.Break();
			}
			if(result == null){
				int daysPast = 0;
				
				int changeToAply;
				int compareSummer = d.CompareTo(summerSolstice);
				int compareWinter = d.CompareTo(winterSolstice);
				int compareSpring = d.CompareTo(springEquinox);
				int compareAutumn = d.CompareTo(autumnEquinox);
				if (debug){
					Debug.Log("Is date equal to Summer Solstice " + summerSolstice.ToString() + " ?: " + (compareSummer == 0) + "\n" +
							"Is date equal to Spring equinox " + springEquinox.ToString() + " ?: " + (compareSpring == 0));
					Debug.Log("Is date equal to Winter Solstice " + winterSolstice.ToString() + " ?: " + (compareWinter == 0) + "\n" + 
							"Is date equal to Autumn equinox " + autumnEquinox.ToString() + " ?: " + (compareAutumn == 0));
					Debug.Break();
				}			
				if (debug && result == null) {
					Debug.Log(d.ToString() + " Compared to winter solstice: " + compareWinter + "\n" +
							d.ToString() + " Compared to Spring eqinox: " + compareSpring);
					Debug.Log(d.ToString() + " Compared to summer solstice: " + compareSummer + "\n" +
							d.ToString() + "Compared to Autumn equinox" + compareAutumn);
					Debug.Break();
				}
				if (compareWinter > 0 && compareSpring < 0){
					if(debug){
						Debug.Log("Date is between winterSolstice and spring Equinox");
						Debug.Break();
					}
					daysPast = d.DaysBetween(winterSolstice);
					if(debug){
						Debug.Log("Days past since Winter solstice: " + daysPast+"\n" +
								"Change rate (seconds per day): " + winterToSpringRate);
						Debug.Break();
					}
					changeToAply = -(int) winterToSpringRate * daysPast;
                    Time winterSolstice_Sunrise;
                    sunriseTimes.TryGetValue(winterSolstice, out winterSolstice_Sunrise);
                    if (winterSolstice_Sunrise ==null) {
                        Debug.LogError("Winter solstice sunrise could not be found");
                        Debug.Break();
                    }
                    result = winterSolstice_Sunrise.TimeWithSeconds(winterSolstice_Sunrise, changeToAply); 
					if(debug){
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if(compareSpring>0 && compareSummer < 0){
					if(debug){
						Debug.Log("Date is between spring Equinox and Summer solstice");
						Debug.Break();
					}
					daysPast = d.DaysBetween(springEquinox);
					if(debug){
						Debug.Log("Days past since Spring Equinox: " + daysPast+"\n" +
								"Change rate (seconds per day): " + springToSummerRate);
						Debug.Break();
					}
					changeToAply = -(int)springToSummerRate * daysPast;
                    Time springEquinox_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out springEquinox_Sunrise);
                    if (springEquinox_Sunrise ==null) {
                        Debug.LogError("spring equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = springEquinox_Sunrise.TimeWithSeconds(springEquinox_Sunrise, changeToAply);
					if(debug){
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if (compareSummer > 0 && compareAutumn < 0){
					if (debug){
						Debug.Log("Date is between summer solstice and Autumn Equinox");
						Debug.Break();
					}
					daysPast = d.DaysBetween(summerSolstice);
					if (debug){
						Debug.Log("Days past since summer solstice: " + daysPast+"\n" +
								"Change rate (seconds per day): " + summerToAutumnRate);
						Debug.Break();
					}
					changeToAply = -(int)summerToAutumnRate * daysPast;
                    Time summerSolstice_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out summerSolstice_Sunrise);
                    if (summerSolstice_Sunrise==null) {
                        Debug.LogError("summer solstice sunrise could not be found");
                        Debug.Break();
                    }
                    result = summerSolstice_Sunrise.TimeWithSeconds(summerSolstice_Sunrise, changeToAply);
					if(debug){
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if (compareAutumn > 0){
					if (debug){
						Debug.Log("Date is between Autumn Equinox and the end of the year");
						Debug.Break();
					}
					daysPast = d.DaysBetween(autumnEquinox);
					if (debug)
						Debug.Log("Days past since autumn Equinox: " + daysPast+"\n" +
								"Change rate (seconds per day): " + autumnToWinterRate);
					changeToAply =- (int)autumnToWinterRate * daysPast;
                    Time autumnEquinox_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out autumnEquinox_Sunrise);
                    if (autumnEquinox_Sunrise == null) {
                        Debug.LogError("autumn equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = autumnEquinox_Sunrise.TimeWithSeconds(autumnEquinox_Sunrise, changeToAply);
					if(debug){
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if(compareWinter < 0) {
					if (debug) {
						Debug.Log("Date is between the start of the year and winter solstice");
						Debug.Break();
					}
					daysPast = d.DaysBetween(winterSolstice);
					if (debug) {
						Debug.Log("Days past since autumn Equinox: " + daysPast + "\n" +
								"Change rate (seconds per day): " + autumnToWinterRate);
					}
					changeToAply = (int)autumnToWinterRate * daysPast;
                    Time autumnEquinox_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out autumnEquinox_Sunrise);
                    if (autumnEquinox_Sunrise == null) {
                        Debug.LogError("autumn equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = autumnEquinox_Sunrise.TimeWithSeconds(autumnEquinox_Sunrise, changeToAply);
					if (debug) {
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if (result == null){
					Debug.LogError("Date " + d.ToString() + "sunrise time could not be calculated");
					Debug.Break();
				}
				sunriseTimes.Add(d, result);
				if(debug){
					Debug.Log("new Entry added to sunriseTimes: (" + d.ToString() + " , "+ result.ToString() + ")");
				}
			}
            return result.Clone();
        }
        /*Returns Sunrise time for the clock current date*/
        public Time getSunriseTime() { return getSunriseTime(currentDate); }
        /*Returns time of the next Sunrise. If it is past Sunrise it will return the time of the next day's Sunrise*/
        public Time getNextSunriseTime(){
			Time result = null;
			if(debug){
				Debug.Log("calculating next Sunrise, currentTime: " + currentTime.ToString());
			}
            if (dayTime) {
				result = getSunriseTime(currentDate.DateWithDays(1));
				if(debug){
					Debug.Log("it is past the current day Sunrise (its daytime): " + getSunriseTime() + " therefore, next sunrise is tomorrow's sunrise");
					Debug.Break();
				}
			}
            else {
                if (currentTime.CompareTo(midnight) > 0) {
					Time sunrise = getSunriseTime();
                    result = sunrise;
					if(debug){
						Debug.Log("it is night and aftermidnight, therefore next sunrise is currentDate's sunrise"); 
						Debug.Break();
					}
                } else {
                    result = getSunriseTime( currentDate.DateWithDays(1));
						Debug.Log("it is nighttime and it is before midnight, therefore next sunrise is tomorrow's sunrise");
						Debug.Break();
                }
            }

            return result;
        }
        /*Returns Sunset time of a given date*/
        public Time getSunsetTime(Date d){
            if (debug)
                Debug.Log("---CALCULATING SUNSET FOR DATE: " + d.ToString() + "---------------");
            Time result = null;
            sunsetTimes.TryGetValue(d, out result);            
			if(debug && result != null){
				Debug.Log("Date already on the collection, no need to calculate. \n Result: " + result.ToString());
				Debug.Break();
			}
			if(result == null){
				int daysPast = 0;
				int changeToAply;
				int compareSummer = d.CompareTo(summerSolstice);
				int compareWinter = d.CompareTo(winterSolstice);
				int compareSpring = d.CompareTo(springEquinox);
				int compareAutumn = d.CompareTo(autumnEquinox);
				if (compareWinter > 0 && compareSpring < 0){
					if (debug) {
						Debug.Log("Date is between Winter Solstice and Spring Equinox");
						Debug.Break();
					}
					daysPast = d.DaysBetween(winterSolstice);
					if (debug) {
						Debug.Log("Days past since Winter solstice: " + daysPast + "\n" +
								"Change rate (seconds per day): " + winterToSpringRate);
						Debug.Break();
					}
					changeToAply = (int)winterToSpringRate * daysPast;
                    Time winterSolstice_Sunset;
                    sunsetTimes.TryGetValue(winterSolstice, out winterSolstice_Sunset);
                    if (winterSolstice_Sunset.Equals(null)) {
                        Debug.LogError("Winter solstice sunrise could not be found");
                        Debug.Break();
                    }
                    result = winterSolstice_Sunset.TimeWithSeconds(winterSolstice_Sunset, changeToAply);
					if (debug) {
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if (compareSpring > 0 && compareSummer < 0){
					if (debug) {
						Debug.Log("Date is between spring Equinox and Summer solstice");
						Debug.Break();
					}
					daysPast = d.DaysBetween(springEquinox);
					if (debug) {
						Debug.Log("Days past since Spring Equinox: " + daysPast + "\n" +
								"Change rate (seconds per day): " + springToSummerRate);
						Debug.Break();
					}
					changeToAply = (int)springToSummerRate * daysPast;
                    Time springEquinox_Sunset;
                    sunsetTimes.TryGetValue(summerSolstice, out springEquinox_Sunset);
                    if (springEquinox_Sunset.Equals(null)) {
                        Debug.LogError("spring equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = springEquinox_Sunset.TimeWithSeconds(springEquinox_Sunset, changeToAply);
					if (debug) {
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if (compareSummer > 0 && compareAutumn < 0){
					if (debug) {
						Debug.Log("Date is between Sumemr solstice and Autumn Equinox");
						Debug.Break();
					}
					daysPast = d.DaysBetween(summerSolstice);
					if (debug) {
						Debug.Log("Days past since summer solstice: " + daysPast + "\n" +
								"Change rate (seconds per day): " + summerToAutumnRate);
						Debug.Break();
					}
					changeToAply = (int)summerToAutumnRate * daysPast;
                    Time summerSolstice_Sunset;
                    sunsetTimes.TryGetValue(summerSolstice, out summerSolstice_Sunset);
                    if (summerSolstice_Sunset.Equals(null)) {
                        Debug.LogError("summer solstice sunrise could not be found");
                        Debug.Break();
                    }
                    result = summerSolstice_Sunset.TimeWithSeconds(summerSolstice_Sunset, changeToAply);
					if (debug) {
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if (compareAutumn > 0) {
					if (debug) {
						Debug.Log("Date is between Autumn Equinox and the end of the year");
						Debug.Break();
					}
					daysPast = d.DaysBetween(autumnEquinox);
					if (debug)
						Debug.Log("Days past since autumn Equinox: " + daysPast + "\n" +
								"Change rate (seconds per day): " + autumnToWinterRate);
					changeToAply = (int)autumnToWinterRate * daysPast;
                    Time autumnEquinox_Sunset;
                    sunsetTimes.TryGetValue(summerSolstice, out autumnEquinox_Sunset);
                    if (autumnEquinox_Sunset.Equals(null)) {
                        Debug.LogError("autumn equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = autumnEquinox_Sunset.TimeWithSeconds(autumnEquinox_Sunset, changeToAply);
					if (debug) {
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if (compareWinter < 0) {
					if (debug) {
						Debug.Log("Date is between the start of the year and winter solstice");
					}
					daysPast = d.DaysBetween(winterSolstice);
					if (debug)
						Debug.Log("Days before winter solstice: " + daysPast + "\n" +
								"Change rate (seconds per day): " + autumnToWinterRate);
					changeToAply = -(int)autumnToWinterRate * daysPast;
                    Time winterSolstice_Sunset;
                    sunsetTimes.TryGetValue(winterSolstice, out winterSolstice_Sunset);
                    if (winterSolstice_Sunset.Equals(null)) {
                        Debug.LogError("Winter solstice sunrise could not be found");
                        Debug.Break();
                    }
                    result = winterSolstice_Sunset.TimeWithSeconds(winterSolstice_Sunset, changeToAply);
					if (debug) {
						Debug.Log("Result: " + result.ToString());
						Debug.Break();
					}
				}
				if(result == null){
					Debug.LogError("Date " + d.ToString() + " sunset time could not be calculated");
					Debug.Break();
				}
				sunsetTimes.Add(d, result);
			}
            return result.Clone();
        }
        /*Returns Sunset time for clock current date*/
        public Time getSunsetTime() { return getSunsetTime(currentDate); }
        public Time getNextSunsetTime(){
            Time result = null;
			if(debug){
				Debug.Log("calculating next Sunset, currentTime: " + currentTime.ToString());
			}
			if(dayTime) {
				result = getSunsetTime();
				if(debug){
					Debug.Log("it is past the current day sunrise (its daytime): " + getSunriseTime() + " therefore, next sunset is today's");
					Debug.Break();
				}
			}
			else {
				result = getSunsetTime(currentDate.DateWithDays(1));
				if(debug){
					Debug.Log("it is past the current day sunset (its nighttime): " + getSunriseTime() + " therefore, next sunset is tomorrow's");
					Debug.Break();
				}
			}
            return result;
        }
		#endregion Get and Set Methods
       
		//Parameter checkup and initialization
        void Awake(){
            //initializing SunriseAndSunsetCollection
            sunriseTimes = new Dictionary<Date, Time>();
			sunsetTimes = new Dictionary<Date, Time>();
            //time change rate
            timeChangeRate= (Day_length_in_minutes/24.0)/60.0;
            
            //Month checkup
            GameObject[] m = GameObject.FindGameObjectsWithTag("Month");
            monthList = new Month[m.Length];
            int daySum = 0;
            Month auxiliar;
            for (int i = 0; i < m.Length; i++)
            {
                auxiliar = m[i].GetComponent<Month>();
                monthList[auxiliar.Month_number - 1] = auxiliar;
                daySum += auxiliar.Length;
                // Debug.Log("Month " + auxiliar.name + " set to position " + (auxiliar.Month_number - 1));
            }
            if (daySum != Days_in_Year)
            {
                Debug.LogError("The specified number of days in a year and the sum of the lengths of all months do not match \n " +
                    "   Days in a year       = " + Days_in_Year + "Sum of month lengths = " + daySum);
                Debug.Break();
            }

            //No null values in day names
            for (int i = 0; i < Day_names.Length; i++)
            {
                if (Day_names[i] == "")
                    Debug.LogError("All day names mus be declared");
            }

            //initial date
            Initial_date.Trim();
            string[] split = Initial_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("Initial date must follow the format: dd/mm/yyyy");
            int day = int.Parse(split[0]);
            int month_num = int.Parse(split[1]);
            int year = int.Parse(split[2]);
			if(year < 0){
				Debug.LogError("Negative year numbers are not allowed");
				Debug.Break();
			}
			if(year<2){
				Debug.LogWarning("It is recomended to introduce a year number greater than 2 for the initial date");
			}

            Month month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num)
                {
                    month = monthList[i];
                }
            }
            if (month == null) {
                Debug.LogError("Initial date's month could not be found");
            }
 
            currentDate = new Date(day, month, year);
			
            //initial time
            Initial_time.Trim();
            split = Initial_time.Split(':');
            if (split.Length != 3)
                Debug.LogError("Initial time must follow the format: hh:mm:ss");
            int hours = int.Parse(split[0]);
            int minutes = int.Parse(split[1]);
            int seconds = int.Parse(split[2]);
            currentTime = new Time(hours, minutes, seconds);       
            

            //Summer Solstice
            summer_solstice_date.Trim();
            split = summer_solstice_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("Summer solstice's date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++) {
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("summer solstice's month could not be found");
            summerSolstice = new Date(day, month, -1);
            if (summer_solstice_hours_of_light <= 0)
                Debug.LogError("Summer solstice cannot have negative or 0 hous of light");
            if (summer_solstice_hours_of_light >= 24)
                Debug.LogError("Summer solstice cannot have 24 or more hours of light");
            Time summerSolstice_Sunrise = midday.TimeWithSeconds(midday, -(int)((summer_solstice_hours_of_light * 3600) / 2));
            Time summerSolstice_Sunset = midday.TimeWithSeconds(midday, (int)((summer_solstice_hours_of_light * 3600) / 2));
            sunriseTimes.Add(summerSolstice, summerSolstice_Sunrise);
			sunsetTimes.Add(summerSolstice, summerSolstice_Sunset);
			
            //Winter solstice
            winter_solstice_date.Trim();
            split = winter_solstice_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("Winter solstice's date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("Winter solstice's month could not be found");
            winterSolstice = new Date(day, month, -1);
            if (winter_solstice_hours_of_light <= 0)
                Debug.LogError("Winter solstice cannot have negative or 0 hous of light");
            if (winter_solstice_hours_of_light >= 24)
                Debug.LogError("Winter solstice cannot have 24 or more hours of light");
            Time winterSolstice_Sunrise = midday.TimeWithSeconds(midday, -(int)((winter_solstice_hours_of_light * 3600) / 2));
            Time winterSolstice_Sunset = midday.TimeWithSeconds(midday, (int)((winter_solstice_hours_of_light * 3600) / 2));
            sunriseTimes.Add(winterSolstice, winterSolstice_Sunrise);
			sunsetTimes.Add(winterSolstice, winterSolstice_Sunset);

            //Spring equinox
            spring_equinox_date.Trim();
            split = spring_equinox_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("Spring equinox date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("Spring equinox month could not be found");
            springEquinox = new Date(day, month, -1);
            if (spring_equinox_hours_of_light <= 0)
                Debug.LogError("Spring equinox cannot have negative or 0 hous of light");
            if (spring_equinox_hours_of_light >= 24)
                Debug.LogError("Spring equinox cannot have 24 or more hours of light");
            Time springEquinox_Sunrise = midday.TimeWithSeconds(midday, -(int)((spring_equinox_hours_of_light * 3600) / 2));
            Time springEquinox_Sunset = midday.TimeWithSeconds(midday, (int)((spring_equinox_hours_of_light * 3600) / 2));
            sunriseTimes.Add(springEquinox, springEquinox_Sunrise);
            sunsetTimes.Add(springEquinox, springEquinox_Sunset);
			
            //Autumn equinox
            autumn_equinox_date.Trim();
            split = autumn_equinox_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("autumn equinox date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("Spring equinox month could not be found");
            autumnEquinox = new Date(day, month, -1);
            if (autumn_equinox_hours_of_light <= 0)
                Debug.LogError(" autumn equinox cannot have negative or 0 hous of light");
            if (autumn_equinox_hours_of_light >= 24)
                Debug.LogError(" autumn equinox cannot have 24 or more hours of light");
            Time autumnEquinox_Sunrise = midday.TimeWithSeconds(midday, -(int)((spring_equinox_hours_of_light * 3600) / 2));
            Time autumnEquinox_Sunset = midday.TimeWithSeconds(midday, (int)((spring_equinox_hours_of_light * 3600) / 2));
            sunriseTimes.Add(autumnEquinox, autumnEquinox_Sunrise);
            sunsetTimes.Add(autumnEquinox, autumnEquinox_Sunset);
			
            int daysbetween = 0;
            int LengthDifference = 0;
            //Winter to Spring time change rate:
            daysbetween = winterSolstice.DaysBetween(springEquinox);
            LengthDifference = spring_equinox_hours_of_light - winter_solstice_hours_of_light;
            winterToSpringRate = ((LengthDifference / 2.0) / daysbetween) * 3600;
            //spring to summer time change rate:
            daysbetween = springEquinox.DaysBetween(summerSolstice);
            LengthDifference = summer_solstice_hours_of_light - spring_equinox_hours_of_light;
            springToSummerRate = ((LengthDifference / 2.0) / daysbetween) * 3600;
            //summer to autumn time change rate:
            daysbetween = summerSolstice.DaysBetween(autumnEquinox);
            LengthDifference = autumn_equinox_hours_of_light - summer_solstice_hours_of_light;
            summerToAutumnRate = ((LengthDifference / 2.0) / daysbetween) * 3600;
            //autumn to winter time change rate:
            daysbetween = autumnEquinox.DaysBetween(winterSolstice);
            LengthDifference = winter_solstice_hours_of_light - autumn_equinox_hours_of_light;
            autumnToWinterRate = ((LengthDifference / 2.0) / daysbetween) * 3600;

            
        }

        // Use this for initialization
        void Start(){
            //TODO: Check Season Loop
			GameObject[] seasons = GameObject.FindGameObjectsWithTag("Season");
            if (seasons == null) {
				Debug.LogError("No Seasons could be found, please remember to tag the apriopuate Objects with the tag 'Season'");
				Debug.Break();
			}
            Season[] seasonList = new Season[seasons.Length];
            Date start, end;
            for (int i = 0; i<seasonList.Length; i++){
                Season s = seasons[i].GetComponent<Season>() as Season;
                seasonList[i] = s;
				start = s.GetStartDate();
				end = s.GetEndDate();
				if(currentDate.CompareTo(start)>=0 && currentDate.CompareTo(end)<0){
					currentSeason = seasonList[i];
					break;
				}
			}
			if(debug){
				Debug.Log("initial season found to be: " + currentSeason.name);
				Debug.Break();
			}
			//once the initial date has been found: 
			if(debug){
                Debug.Log("Chequing season loop...");
			}
			bool loopComplete;
			int seasonsInYear = 1;
			Season season=currentSeason.next_season;
			while(seasonsInYear <= seasonList.Length){
				Season next = season.next_season;
				if(debug){
					Debug.Log("Checking season " + season.name + "\t" + "next season: " + next.name + "\n" +
							"CurrentSeason: " + currentSeason.name);
					Debug.Break();
				}
				if(season.GetEndDate().CompareTo(next.GetStartDate())!=0){
					Debug.LogError("Season " + season.name + "end date("+ season.GetStartDate()+") must match Season " + next.name + " start date (" + next.GetStartDate() + ")");
					Debug.Break();
				}
			}
			if(!season.Equals(currentSeason)){
				Debug.LogError("the seasons do not form a loop");
			}			
			
			
			//TODO: ¿Esto se puede mover a awake?
            if (currentTime.CompareTo(getSunriseTime()) >= 0 && currentTime.CompareTo(getSunsetTime()) < 0){
                dayTime = true;
            }
            if (debug){
				Debug.Log("Current date: " + currentDate + "\n" + 
						"Current time: " + currentTime);
				Debug.Log("today's sunrise: ");
				Time currentSunrise = getSunriseTime();
				Debug.Log("next sunrise: ");
				Time nextSunrise = getNextSunriseTime();
				Debug.Log("today's sunset: ");
				Time currentSunset = getSunsetTime(); 
                Debug.Log("next sunset: ");
				Time nextSunset = getNextSunsetTime();
				Time aux = currentTime.Clone();
							
				Debug.Log("changing current time to 1 hour before current day's sunrise");
				currentTime = currentSunrise.Clone();
				currentTime.add(-3600);
				Debug.Log("new currentTime: " + currentTime.ToString()); 
				Debug.Log("Next Sunrise: ");
				nextSunrise =  getNextSunriseTime();
				Debug.Log("is next sunrise equal to todays sunrise? (it should) " + (nextSunrise == currentSunrise));
				
				Debug.Log("moving currentTime to 1 hour past sunset:" );
				currentTime = currentSunset.Clone();
				currentTime.add(3600);
				Debug.Log("new CurrentTime: " + currentTime.ToString());
				Debug.Log("next sunset: ");
				nextSunset = getNextSunsetTime();
				Debug.Log("is it equal to today's sunset? (it shoudn't) " + (nextSunset != currentSunset));
				
				currentTime = aux; 
            }
            
        }

        // Update is called once per frame
        void Update(){
			Time previousTime = currentTime.Clone();
            int dayChange = 0;
            seconds += UnityEngine.Time.deltaTime/timeChangeRate;
            int s = (int)seconds;
            seconds -= s;
            dayChange = currentTime.add(s);
            
            //updating date
            if (dayChange > 0){
				currentDate.AddDay(dayChange);
				OnMidnightReached(EventArgs.Empty);
				if(debug){
					Debug.Log("Midnight reached: " + currentTime);           
				}					
            }
			if(previousTime.CompareTo(midday)<0 && currentTime.CompareTo(midday)>= 0){
				if(debug){
					Debug.Log("Miday reached: " + currentTime);
				}
				OnMiddayReached(EventArgs.Empty);
			}

            //TODO: eventos de aviso amanecer/atardecer
            Time todaySunrise = getSunriseTime();
            Time todaySunset = getSunsetTime();
            if (!dayTime && currentTime.CompareTo(todaySunrise) >= 0 && currentTime.CompareTo(todaySunset) < 0){
                if (debug) {
                    Debug.Log("It's Sunrise time: " + todaySunrise.ToString());
                    Debug.Log("current time: " + currentTime.ToString());
                }
                dayTime = true;
                OnSunriseReached(EventArgs.Empty);
                if (debug){
                    Debug.Log("daytime: " + dayTime);
                }
            }else{
                if (dayTime && currentTime.CompareTo(todaySunset) >= 0){
                    if (debug) {
                        Debug.Log("It's Dkusk time: " + todaySunset.ToString());
                        Debug.Log("current time: " + currentTime.ToString());
                    }
                    dayTime = false;
                    OnSunsetReached(EventArgs.Empty);
                    if(debug)
                        Debug.Log("daytime: " + dayTime);
                }
            }
        }
    }

    public class Date : IComparable{
        private int day;
        private Month month;
        private int year;
        private CentralClock clock = null;
        private bool yearMatters;

		#region Constructors
        public Date(int day, Month month, int year){
            
            this.day = day;
            this.month = month;
            this.year = year;
            if (year == -1)
                yearMatters = false;
            else
                yearMatters = true;
        }
        public Date DateWithDays(int days){
            Date result =this.Clone();
            result.AddDay(days);
            return result;
        }
		public Date Clone(){
			return new Date(day, month, year);
		}
		#endregion Constructors
		#region getters and setters
        public int GetDay() { return day; }
        public Month GetMonth() { return month; }
        public int GetMonthNumber() { return month.Month_number; }
        public int GetYear() { return year; }
        public bool YearMatters() { return yearMatters; }
		public void setClock(){
			clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<CentralClock>();
		}
		#endregion getters and setters
		#region Transformation Methods
        public void AddYear(int y) { year += y; }
		public void SubstractYear(int y){
			year -= y;
			if(year <0){
				year = 0;
				Debug.LogError("Substracting too many years, negative year numbers are reserved. Year has been set to 0");
				Debug.Break();
			}
		}
        public void AddMonth(int m){
            if (m < 0) SubstractMonth(-m);
            else{
				if(clock == null) setClock();
				for (int i = m; i > 0; i--) {
                    if(month.Month_number == clock.GetNumberOfMonths()){
                        AddYear(1);
                    }
                    month = month.next_month;
                }
			}
        }
        public void SubstractMonth(int m){
            if (m < 0) AddMonth(-m);
			else{
				if(clock == null) setClock();
				Month[] monthList = clock.getMonthList();
				int newMonth = month.Month_number - m;
				if(newMonth >=0) month = monthList[newMonth];
				else{
					while(newMonth<0){
						newMonth += monthList.Length;
						if(yearMatters)
							SubstractYear(1);
					}
				}
			}
            
        }  
        public void AddDay(int d){
            if ((day + d)<=month.Length) { this.day += d; }
            else{
                int leftover = d - (month.Length - day);
                AddMonth(1);
                day = 1;
                while (leftover +1 > month.Length){
                    leftover -= month.Length;
                    AddMonth(1);
                }
                day += leftover;
            }
        }
		public void SubstractDay(int d){
			if(d<0) AddDay(-d);
			else{
				int newDay = day - d;
				while(newDay<1){
					SubstractMonth(1);
					newDay += month.Length;
				}
                day = newDay;
			}
		}
		#endregion Transformation Methods
		
        override public string ToString(){
            //TODO: falta añadir st, nd, th ... al final del numero de dia
            string result = day.ToString() + " of " + month.name;
            if (yearMatters) result += ", " + year.ToString();
            return result;
        }
        public int CompareTo(object o){
            if (o == null) return 1;
            Date other = (o as Date);
            if (other == null)
                Debug.LogError("Object " + o.ToString() + " is not a date");
            else
            {
                if (this.yearMatters && other.yearMatters)
                    if (this.year != other.year) return this.year - other.year;
                if (this.month != other.month) return this.month.Month_number - other.month.Month_number;
                return this.day - other.day;
            }
            return 0;
        }
        override public bool Equals(object o){
			Date other = o as Date;
			if(other == null){
				return false;
			}else{
                return this.CompareTo(other)==0;
			}
		}

        override public int GetHashCode() {
            int result = day;
            if (clock == null) setClock();
            Month[] monthList = clock.getMonthList();
            for (int i =0; i<month.Month_number-1; i++) {
                result += monthList[i].Length;
            }
            return result;
        }
        public int DaysBetween(Date other) {
            int compareTo = this.CompareTo(other);
            if (compareTo > 0) return other.DaysBetween(this);
            if (compareTo == 0) return 0;

            int result = 0;
            Date auxiliar = new Date (this.day, this.month, this.year);
            if (auxiliar.yearMatters && other.yearMatters)
            {
                int difference = other.year - auxiliar.year;
                if (difference < 0)
                    Debug.LogError("inconsistency between compareTo() and daysBeween() on dates " + auxiliar.ToString() + " and " + other.ToString());
                if (clock == null) setClock();
			    while (difference > 1)
                {
                    result += clock.Days_in_Year;
                    auxiliar.AddYear(1);
                    difference = other.year - auxiliar.year;
                }
                //hay que tener en cuenta que se puede estar comparando el 1 de marzo con el i feb del año siguiente
            }
            while (auxiliar.month != other.month)
            {
                result += auxiliar.month.Length;
                auxiliar.month = auxiliar.month.next_month;
            }
            result += other.day - auxiliar.day;
            return result;
            
        }
    }

    public class Time : IComparable{
        private int hours;
        private int minutes;
        private int seconds;

		#region Constructors:
        public Time(int hours, int minutes, int seconds){
            if (seconds > 59||seconds<0) {
                Debug.LogError("Trying to create a new Time with invalid seconds: " + seconds);
            }
            if (minutes > 59 || minutes < 0) {
                Debug.LogError("Trying to create a new Time with invalid minutes: " + minutes);
            }
            if(hours >24 || hours < 0) {
                Debug.LogError("Trying to create a new Time with invalid hours: " + hours);
            }
            if (hours == 24) {
                hours = 0;
            }

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
        public Time secondsToTime(int seconds){
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
