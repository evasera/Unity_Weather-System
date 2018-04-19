using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace weatherSystem
{
    public class Season : MonoBehaviour{
		
		#region Public Atributes
		[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
		public bool debug;
        public string Start_date = "dd/mm/yyyy";
        public string End_date = "dd/mm/yyyy";
        public Season next_season;
		#endregion Public Atributes
		#region Private Atributes
        private Date startDate;
        private Date endDate;
		#endregion Private Atributes
		
		
		#region Get and Set functions
		public Date GetStartDate(){return startDate;}
		public Date GetEndDate(){return endDate;}
		public Season GetNextSeason(){return next_season;}
		#endregion Get and Set functions
		
		public bool Equals(Object o){
			if(debug){
				Debug.Log("Chequing Equality between Season " + this.name + "and Object o:" + o.name);
			}
			Season other = o as Season;
			if(other == null){
				Debug.Log("Object o is not a Season");
				return false;
			}
			else{
				return startDate.Equals(other.GetStartDate()) && endDate.Equals(other.GetStartDate()) && name.Equals(other.name);
			}
		}
		
        private void Awake(){
            //Validate start date
            Start_date.Trim();
            string[] dateSplit = Start_date.Split('/');
            if (dateSplit.Length != 3){
                Debug.LogError("Initial date must follow the format: dd/mm/yyyy");
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
                Debug.LogError("Season " + this.name + " start date's month could not be found");
				Debug.Break();
			}

            startDate = new Date(day, month, -1);

            //validate end date
            End_date.Trim();
            dateSplit = Start_date.Split('/');
            if (dateSplit.Length != 3){
                Debug.LogError("End date must follow the format: dd/mm/yyyy");
				Debug.Break();
			}
            day = int.Parse(dateSplit[0]);
            m = int.Parse(dateSplit[1]);
            month = null;
            //getting the Month object from the number.
            for (int i = 0; i < MonthList.Length; i++)
            {
                if (MonthList[i].GetComponent<Month>().Month_number == m)
                {
                    month = MonthList[i].GetComponent<Month>();
                }
            }
            if (month == null){
                Debug.LogError("Season " + this.name + " end date's month could not be found");
				Debug.Break();
			}

            endDate = new Date(day, month, -1);

			//validate next Season:
			if(next_season == null){
				Debug.LogError("Season " + this.name + ": next month can not be null");
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