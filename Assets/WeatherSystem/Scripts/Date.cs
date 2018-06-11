using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem{
    public class Date : System.IComparable {
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
        public Date ParseDate(string s) {
            s.Trim();
            string[] dateSplit = s.Split('/');
            if (dateSplit.Length != 3) {
                Debug.LogError(" date must follow the format: dd/mm/yyyy");
                Debug.Break();
            }
            int d = int.Parse(dateSplit[0]);
            int m = int.Parse(dateSplit[1]);
            Month month = null;
            //getting the Month object from the number.
            GameObject[] MonthList = GameObject.FindGameObjectsWithTag("Month");
            for (int i = 0; i < MonthList.Length; i++) {
                if (MonthList[i].GetComponent<Month>().Month_number == m) {
                    month = MonthList[i].GetComponent<Month>();
                }
            }
            if (month == null) {
                Debug.LogError(" date's month could not be found");
                Debug.Break();
            }

            return new Date(d, month, -1);
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
            if ((day + d)<=month.Length) {
                this.day += d;
            }
            else{
                int leftover = d - (month.Length - day) -1;
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
            string result = day.ToString() ;
			if(day ==1) result += "st";
			else if(day == 2) result += "nd";
			else if(day == 3) result += "rd";
			else{ result += "th"; }
			result+= " of " + month.name;
            if (yearMatters) result += ", " + year.ToString();
            return result;
        }
        public int CompareTo(object o){
            if (o == null) return 1;
            Date other = o as Date;
            if (other == null)
                Debug.LogError("Object " + o.ToString() + " is not a date");
            else{
                if (this.yearMatters && other.yearMatters)
                    if (this.year != other.year) return this.year - other.year;
                if (this.month != other.month) return this.month.Month_number - other.month.Month_number;
                return this.day - other.day;
            }
            return 0;
        }

        public override bool Equals(object obj) {
            var date = obj as Date;
            return date != null &&
                   day == date.day &&
                   EqualityComparer<Month>.Default.Equals(month, date.month);
        }

        public override int GetHashCode() {
            var hashCode = 124811402;
            hashCode = hashCode * -1521134295 + day.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Month>.Default.GetHashCode(month);
            return hashCode;
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
}