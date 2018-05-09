using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem {
    public class CentralClock : MonoBehaviour {
       
	   #region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
        
		[Header ("Initial time data:")]
        public string Initial_date = "01/01/0001";
        public string Initial_time = "12:00:00";
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
		
		private Season currentSeason;
		private int daysSinceSeasonChange;
        
		private Date currentDate;
		private Date winterSolstice;
        private Date summerSolstice;
        private Date springEquinox;
        private Date autumnEquinox;
        
		private double timeChangeRate;
        private double seconds = 0.0;
		
        private Time currentTime;
        private Time midday= new Time(12,0,0);
        private Time midnight= new Time(0,0,0);
        bool dayTime = false;
        
        private double winterToSpringRate;
        private double springToSummerRate;
        private double summerToAutumnRate;
        private double autumnToWinterRate;
		
		private Dictionary<Date, Time>  sunriseTimes;
		private Dictionary<Date, Time>  sunsetTimes;
		#endregion Private Atributes

        #region Get and Set Methods
        public int GetNumberOfMonths() { return monthList.Length; }
        public Month[] getMonthList() {return monthList;}
		public Month getMonth () {return currentDate.GetMonth();}
		
        public Time getCurrentTime() { return currentTime; }
        
		public Time GetMidnightTime() { return midnight; }
		public Time GetMiddayTime(){ return midday.Clone();}
		public int GetDay(){return currentDate.GetDay();}
		
		public int GetYear() {return currentDate.GetYear();}
		
		public Date getCurrentDate() { return currentDate; }
		public Date GetWinterSolstice(){return winterSolstice.Clone();}
        public Date GetSpringEquinox() { return springEquinox.Clone(); }
		public double GetWinterToSpringRate(){return winterToSpringRate;}
		public Season GetSeason(){return currentSeason;}
		
        /*Returns Sunrise time of a given date*/
        public Time getSunriseTime(Date d){			
            Time result = null;
            sunriseTimes.TryGetValue(d, out result);
			if(result == null){
                int daysPast = 0;
				int changeToAply;
				int compareSummer = d.CompareTo(summerSolstice);
				int compareWinter = d.CompareTo(winterSolstice);
				int compareSpring = d.CompareTo(springEquinox);
				int compareAutumn = d.CompareTo(autumnEquinox);		
				if (compareWinter > 0 && compareSpring < 0){
					daysPast = d.DaysBetween(winterSolstice);
					changeToAply = -(int) winterToSpringRate * daysPast;
                    Time winterSolstice_Sunrise;
                    sunriseTimes.TryGetValue(winterSolstice, out winterSolstice_Sunrise);
                    if (winterSolstice_Sunrise ==null) {
                        Debug.LogError("CentralClock: CentralClock: Winter solstice sunrise could not be found");
                        Debug.Break();
                    }
						result = winterSolstice_Sunrise.TimeWithSeconds(winterSolstice_Sunrise, changeToAply); 
				}
				if(compareSpring>0 && compareSummer < 0){
					daysPast = d.DaysBetween(springEquinox);
					changeToAply = -(int)springToSummerRate * daysPast;
                    Time springEquinox_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out springEquinox_Sunrise);
                    if (springEquinox_Sunrise ==null) {
                        Debug.LogError("CentralClock: spring equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = springEquinox_Sunrise.TimeWithSeconds(springEquinox_Sunrise, changeToAply);
				}
				if (compareSummer > 0 && compareAutumn < 0){
					daysPast = d.DaysBetween(summerSolstice);
					changeToAply = -(int)summerToAutumnRate * daysPast;
                    Time summerSolstice_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out summerSolstice_Sunrise);
                    if (summerSolstice_Sunrise==null) {
                        Debug.LogError("CentralClock: summer solstice sunrise could not be found");
                        Debug.Break();
                    }
                    result = summerSolstice_Sunrise.TimeWithSeconds(summerSolstice_Sunrise, changeToAply);
				}
				if (compareAutumn > 0){
					daysPast = d.DaysBetween(autumnEquinox);
					changeToAply =- (int)autumnToWinterRate * daysPast;
                    Time autumnEquinox_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out autumnEquinox_Sunrise);
                    if (autumnEquinox_Sunrise == null) {
                        Debug.LogError("CentralClock: autumn equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = autumnEquinox_Sunrise.TimeWithSeconds(autumnEquinox_Sunrise, changeToAply);
				}
				if(compareWinter < 0) {
					daysPast = d.DaysBetween(winterSolstice);
					changeToAply = (int)autumnToWinterRate * daysPast;
                    Time autumnEquinox_Sunrise;
                    sunriseTimes.TryGetValue(summerSolstice, out autumnEquinox_Sunrise);
                    if (autumnEquinox_Sunrise == null) {
                        Debug.LogError("CentralClock: autumn equinox sunrise could not be found");
                        Debug.Break();
                    }
                    result = autumnEquinox_Sunrise.TimeWithSeconds(autumnEquinox_Sunrise, changeToAply);
					}
				if (result == null){
					Debug.LogError("CentralClock: Date " + d.ToString() + " sunrise time could not be calculated");
					Debug.Break();
				}
				sunriseTimes.Add(d, result);
			}
            return result.Clone();
        }
        
		/*Returns Sunrise time for the clock current date*/
        public Time getSunriseTime() { return getSunriseTime(currentDate); }
        
		/*Returns the time of the next Sunrise*/
        public Time getNextSunriseTime(){
			Time result = null;
			if (dayTime) {
				result = getSunriseTime(currentDate.DateWithDays(1));
			} else {
                if (currentTime.CompareTo(new Time(23,59,59)) > 0) {
					Time sunrise = getSunriseTime();
                    result = sunrise;
                } else {
                    result = getSunriseTime( currentDate.DateWithDays(1));
                }
            }

            return result;
        }
		
        /*Returns Sunset time of a given date*/
        public Time getSunsetTime(Date d){
            Time result = null;
            sunsetTimes.TryGetValue(d, out result);            
			if(result == null){
                int daysPast = 0;
				int changeToAply;
				int compareSummer = d.CompareTo(summerSolstice);
				int compareWinter = d.CompareTo(winterSolstice);
				int compareSpring = d.CompareTo(springEquinox);
				int compareAutumn = d.CompareTo(autumnEquinox);
				if (compareWinter > 0 && compareSpring < 0){
					daysPast = d.DaysBetween(winterSolstice);
					changeToAply = (int)winterToSpringRate * daysPast;
                    Time winterSolstice_Sunset;
                    sunsetTimes.TryGetValue(winterSolstice, out winterSolstice_Sunset);
                    if (winterSolstice_Sunset.Equals(null)) {
                        Debug.LogError("CentralClock: Winter solstice sunrise could not be found");
                        Debug.Break();
                    }
                    result = winterSolstice_Sunset.TimeWithSeconds(winterSolstice_Sunset, changeToAply);
                }
                if (compareSpring > 0 && compareSummer < 0){
					daysPast = d.DaysBetween(springEquinox);
					changeToAply = (int)springToSummerRate * daysPast;
                    Time springEquinox_Sunset;
                    sunsetTimes.TryGetValue(summerSolstice, out springEquinox_Sunset);
                    result = springEquinox_Sunset.TimeWithSeconds(springEquinox_Sunset, changeToAply);
				}
				if (compareSummer > 0 && compareAutumn < 0){
					daysPast = d.DaysBetween(summerSolstice);
					changeToAply = (int)summerToAutumnRate * daysPast;
                    Time summerSolstice_Sunset;
                    sunsetTimes.TryGetValue(summerSolstice, out summerSolstice_Sunset);
                    result = summerSolstice_Sunset.TimeWithSeconds(summerSolstice_Sunset, changeToAply);
				}
				if (compareAutumn > 0) {
					daysPast = d.DaysBetween(autumnEquinox);
					changeToAply = (int)autumnToWinterRate * daysPast;
                    Time autumnEquinox_Sunset;
                    sunsetTimes.TryGetValue(summerSolstice, out autumnEquinox_Sunset);
                    result = autumnEquinox_Sunset.TimeWithSeconds(autumnEquinox_Sunset, changeToAply);
				}
				if (compareWinter < 0) {
					daysPast = d.DaysBetween(winterSolstice);
					changeToAply = -(int)autumnToWinterRate * daysPast;
                    Time winterSolstice_Sunset;
                    sunsetTimes.TryGetValue(winterSolstice, out winterSolstice_Sunset);
                    result = winterSolstice_Sunset.TimeWithSeconds(winterSolstice_Sunset, changeToAply);
				}
				if(result == null){
					Debug.LogError("CentralClock: Date " + d.ToString() + " sunset time could not be calculated");
					Debug.Break();
				}
				sunsetTimes.Add(d, result);
			}
            return result.Clone();
        }
        
		/*Returns Sunset time for clock current date*/
        public Time getSunsetTime() { return getSunsetTime(currentDate); }
        
		/*Returns the time of the next Sunrise*/
		public Time getNextSunsetTime(){
            Time result = null;
			if(dayTime) {
				result = getSunsetTime();
			}
			else {
				result = getSunsetTime(currentDate.DateWithDays(1));
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
                Debug.LogError("CENTRALCLOCK: The specified number of days in a year and the sum of the lengths of all months do not match \n " +
                    "   Days in a year       = " + Days_in_Year + "Sum of month lengths = " + daySum);
                Debug.Break();
            }

            //No null values in day names
            for (int i = 0; i < Day_names.Length; i++)
            {
                if (Day_names[i] == "")
                    Debug.LogError("CENTRALCLOCK: All day names must be declared");
            }

            //initial date
            Initial_date.Trim();
            string[] split = Initial_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("CENTRALCLOCK: Initial date must follow the format: dd/mm/yyyy");
            int day = int.Parse(split[0]);
            int month_num = int.Parse(split[1]);
            int year = int.Parse(split[2]);
			if(year < 0){
				Debug.LogError("CENTRALCLOCK: Negative year numbers are not allowed");
				Debug.Break();
			}
			if(year<2){
				Debug.LogWarning("CENTRALCLOCK: It is recomended to introduce a year number greater than 2 for the initial date");
			}

            Month month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num)
                {
                    month = monthList[i];
                }
            }
            if (month == null) {
                Debug.LogError("CENTRALCLOCK: Initial date's month could not be found");
            }
 
            currentDate = new Date(day, month, year);
			
            //initial time
            Initial_time.Trim();
            split = Initial_time.Split(':');
            if (split.Length != 3)
                Debug.LogError("CENTRALCLOCK: Initial time must follow the format: hh:mm:ss");
            int hours = int.Parse(split[0]);
            int minutes = int.Parse(split[1]);
            int seconds = int.Parse(split[2]);
            currentTime = new Time(hours, minutes, seconds);
            

            //Summer Solstice
            summer_solstice_date.Trim();
            split = summer_solstice_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("CENTRALCLOCK: Summer solstice's date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++) {
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("CENTRALCLOCK: summer solstice's month could not be found");
            summerSolstice = new Date(day, month, -1);
            if (summer_solstice_hours_of_light <= 0)
                Debug.LogError("CENTRALCLOCK: Summer solstice cannot have negative or 0 hous of light");
            if (summer_solstice_hours_of_light >= 24)
                Debug.LogError("CENTRALCLOCK: Summer solstice cannot have 24 or more hours of light");
            Time summerSolstice_Sunrise = midday.TimeWithSeconds(midday, -(int)((summer_solstice_hours_of_light * 3600) / 2));
            Time summerSolstice_Sunset = midday.TimeWithSeconds(midday, (int)((summer_solstice_hours_of_light * 3600) / 2));
            sunriseTimes.Add(summerSolstice, summerSolstice_Sunrise);
			sunsetTimes.Add(summerSolstice, summerSolstice_Sunset);
			
            //Winter solstice
            winter_solstice_date.Trim();
            split = winter_solstice_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("CENTRALCLOCK: Winter solstice's date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("CENTRALCLOCK: Winter solstice's month could not be found");
            winterSolstice = new Date(day, month, -1);
            if (winter_solstice_hours_of_light <= 0)
                Debug.LogError("CENTRALCLOCK: Winter solstice cannot have negative or 0 hous of light");
            if (winter_solstice_hours_of_light >= 24)
                Debug.LogError("CENTRALCLOCK: Winter solstice cannot have 24 or more hours of light");
            Time winterSolstice_Sunrise = midday.TimeWithSeconds(midday, -(int)((winter_solstice_hours_of_light * 3600) / 2));
            Time winterSolstice_Sunset = midday.TimeWithSeconds(midday, (int)((winter_solstice_hours_of_light * 3600) / 2));
            sunriseTimes.Add(winterSolstice, winterSolstice_Sunrise);
			sunsetTimes.Add(winterSolstice, winterSolstice_Sunset);

            //Spring equinox
            spring_equinox_date.Trim();
            split = spring_equinox_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("CENTRALCLOCK: Spring equinox date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("CENTRALCLOCK: Spring equinox month could not be found");
            springEquinox = new Date(day, month, -1);
            if (spring_equinox_hours_of_light <= 0)
                Debug.LogError("CENTRALCLOCK: Spring equinox cannot have negative or 0 hous of light");
            if (spring_equinox_hours_of_light >= 24)
                Debug.LogError("CENTRALCLOCK: Spring equinox cannot have 24 or more hours of light");
            Time springEquinox_Sunrise = midday.TimeWithSeconds(midday, -(int)((spring_equinox_hours_of_light * 3600) / 2));
            Time springEquinox_Sunset = midday.TimeWithSeconds(midday, (int)((spring_equinox_hours_of_light * 3600) / 2));
            sunriseTimes.Add(springEquinox, springEquinox_Sunrise);
            sunsetTimes.Add(springEquinox, springEquinox_Sunset);
			
            //Autumn equinox
            autumn_equinox_date.Trim();
            split = autumn_equinox_date.Split('/');
            if (split.Length != 3)
                Debug.LogError("CENTRALCLOCK: autumn equinox date must follow the format: dd/mm/yyyy");
            day = int.Parse(split[0]);
            month_num = int.Parse(split[1]);
            month = null;
            for (int i = 0; i < monthList.Length; i++){
                if (monthList[i].Month_number == month_num){
                    month = monthList[i];
                }
            }
            if (month == null)
                Debug.LogError("CENTRALCLOCK: Spring equinox month could not be found");
            autumnEquinox = new Date(day, month, -1);
            if (autumn_equinox_hours_of_light <= 0)
                Debug.LogError("CENTRALCLOCK: autumn equinox cannot have negative or 0 hous of light");
            if (autumn_equinox_hours_of_light >= 24)
                Debug.LogError("CENTRALCLOCK: autumn equinox cannot have 24 or more hours of light");
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


            if (currentTime.CompareTo(getSunriseTime()) >= 0 && currentTime.CompareTo(getSunsetTime()) < 0) {
                dayTime = true;
            }
        }
        
        // Initialization thar requires comunication with other scripts
        void Start(){
			//TODO: comprobar los indices de cada estacion, no puede haber duplicados ni numeros mayores a numero de estaciones -1; 
            GameObject[] s = GameObject.FindGameObjectsWithTag("Season");
			bool[] indexesUsed = new bool[s.Length];
            if(s.Length == 0) {
                Debug.LogError("CENTRALCLOCK: No object with tag 'Season' found");
            }
            for (int i =0; i<s.Length; i++) {
                Season aux = s[i].GetComponent<Season>();
				int index = aux.GetIndex();
				if(index>= s.Length){
					Debug.LogError("CENTRALCLOCK: Season " + s + "has an index number to high. \n Please Remember that season indexes need to start in 0 and be consecutive");
				}else { 
					if(indexesUsed[i]){
						Debug.LogError("CENTRALCLOCK: Season index " + i + " is duplicated");
					}else{
                    indexesUsed[i] = true;
					}
				}
                if(aux == null) {
                    Debug.LogError("CENTRALCLOCK: GameObject " + s[i].name + "has 'Season' tag but no Season script");
                }
                if (aux.GetEndDate().CompareTo(aux.GetStartDate()) < 0) {
                    if(currentDate.CompareTo(aux.GetStartDate())>=0 || currentDate.CompareTo(aux.GetEndDate()) < 0) {
                        currentSeason = aux;
                        break;
                    }
                }
                else if(currentDate.CompareTo(aux.GetStartDate())>=0 && currentDate.CompareTo(aux.GetEndDate())<0) {
                    currentSeason = aux;
                    break;
                }
            }
            if (currentSeason == null) {
                Debug.LogError("CENTRALCLOCK: Initial season could not be found");
            }
            Season next = currentSeason.GetNextSeason();
            int counter = 1;
            while (counter <s.Length) {
                next = next.next_season;
                counter++;
            }
            if(next != currentSeason) {
                Debug.LogError("CENTRALCLOCK: Season Loop is not closed");
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
                if (debug){
					Debug.Log("CENTRALCLOCK: Midnight reached: " + currentTime + "\n" + 
                            "New date: " + currentDate);
                    Debug.Break();
				}
                //Updating season
                if (currentDate.CompareTo(currentSeason.GetEndDate()) == 0) {
                    currentSeason = currentSeason.next_season;
                    if (debug) {
                        Debug.Log("CENTRALCLOCK: Season updated, new Season: " + currentSeason.name);
                        Debug.Break();
                    }
                }

            }
			if(previousTime.CompareTo(midday)<0 && currentTime.CompareTo(midday)>= 0){
				if(debug){
					Debug.Log("CENTRALCLOCK: Miday reached: " + currentTime);
                    Debug.Break();
                }
			}

            //TODO: eventos de aviso amanecer/atardecer
            Time todaySunrise = getSunriseTime();
            Time todaySunset = getSunsetTime();
            if (!dayTime && currentTime.CompareTo(todaySunrise) >= 0 && currentTime.CompareTo(todaySunset) < 0){
                if (debug) {
                    Debug.Log("CENTRALCLOCK: It's Sunrise time: \n " +
                            "Sunrise time: " + todaySunrise.ToString() + "\t Current time: " + currentTime.ToString());
                    Debug.Break();
                }
                dayTime = true;

            }else{
                if (dayTime && currentTime.CompareTo(todaySunset) >= 0){
                    if (debug) {
                        Debug.Log("CENTRALCLOCK: It's sunset time \n " +
                            "Sunset time: " + todaySunset.ToString() + "\t Current time: " + currentTime.ToString());
                        Debug.Break();
                    }
                    dayTime = false;
                }
            }
        }
    }
}
