using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem{
    public class Season : MonoBehaviour{
		
		#region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
        public string Start_date = "dd/mm/yyyy";
        public string End_date = "dd/mm/yyyy";
        public Season next_season;
		public int index;
		#endregion Public Atributes
		
		#region Private Atributes
        private Date startDate;
        private Date endDate;
		#endregion Private Atributes
		
		#region Get and Set functions
		public Date GetStartDate(){
            if(startDate == null){
                Debug.LogError("SEASON " + this.name + ": Start date is null");
                Debug.Break();
            }
            return startDate;
        }
		public Date GetEndDate(){
            if (startDate == null) {
                Debug.LogError("SEASON " + this.name + ": End date is null");
                Debug.Break();
            }
            return endDate;
        }
		public Season GetNextSeason(){return next_season;}
		public int GetIndex(){return index;}
		#endregion Get and Set functions
		
		public bool Equals(UnityEngine.Object o){
			if(debug){
				Debug.Log("Checking Equality between Season " + this.name + "and Object o:" + o.name);
			}
			Season other = o as Season;
			if(other == null){
				Debug.Log("Object o is not a Season");
				return false;
			}
			else{
				bool result = startDate.Equals(other.GetStartDate()) && endDate.Equals(other.GetStartDate()) && name.Equals(other.name);
				if(debug){
					Debug.Log("Result: " + result);
				}
				return result;
			}
		}
        
        private void Awake(){
			//Index is not negative:
			if(index <0){
				Debug.LogError("SEASON " + this.name + ": Index number can not be negative");
			}
			
            //Validate start date
            Start_date.Trim();
            if (debug) {
                Debug.Log("SEASON " + name + ":    Calculating start of season " + name + " ...");
                Debug.Break();
            }
            string[] dateSplit = Start_date.Split('/');
            if (dateSplit.Length != 3){
                Debug.LogError("SEASON " + name + ":    Initial date must follow the format: dd/mm/yyyy");
				Debug.Break();
			}
            int day = int.Parse(dateSplit[0]);
            int m = int.Parse(dateSplit[1]);
            Month month = null;
            //getting the Month object from the number.
            GameObject[] MonthList = GameObject.FindGameObjectsWithTag("Month");
            for (int i = 0; i < MonthList.Length; i++)
            {
                if (MonthList[i].GetComponent<Month>().Month_number == m)
                {
                    month = MonthList[i].GetComponent<Month>();
                }
            }
            if (month == null){
                Debug.LogError("SEASON " + this.name + ": start date's month could not be found");
				Debug.Break();
			}

            startDate = new Date(day, month, -1);

            //validate end date
            if (debug) {
                Debug.Log("SEASON " + name + ":    Calculating end of season " + name + "...");
                Debug.Break();
            }
            End_date.Trim();
            if (debug) {
                Debug.Log("SEASON " + name + ":  \n end Received: " + End_date + "\t start recived: " + Start_date);
                Debug.Break();
            }
            string[] a = End_date.Split('/');
            if (a.Length != 3){
                Debug.LogError("SEASON " + name + ":    End date must follow the format: dd/mm/yyyy");
				Debug.Break();
			}
            if (debug) {
                Debug.Log("End date split \n " + a[0] + "\t" + a[1] + "\t" + a[2]);
            }
            day = int.Parse(a[0]);
            m = int.Parse(a[1]);
            if (debug) {
                Debug.Log("SEASON " + name + ":    day number: " + day + "\n" +
                        "month number: " + m);
                Debug.Log("Loop to find correct object Month: ");
                Debug.Break();
            }
            month = null;

            //getting the Month object from the number.
            for (int i = 0; i < MonthList.Length; i++)
            {
                Month aux = MonthList[i].GetComponent<Month>();
                if (debug) {
                    Debug.Log("SEASON " + name + ":    Month " + aux.name + " month number: " + aux.Month_number + "\n" +
                            "is correct month? " +( aux.Month_number == m));
                    Debug.Break();
                }
                if (aux.Month_number == m){
                    month = aux;
                }
            }
            if (month == null){
                Debug.LogError("SEASON " + this.name + ": end date's month could not be found");
				Debug.Break();
			}

            Date k = new Date(day, month, -1);
            endDate = k;
            
			//validate next Season:
			if(next_season == null){
				Debug.LogError("SEASON " + this.name + ": next month can not be null");
				Debug.Break();
			}
        }
        
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}