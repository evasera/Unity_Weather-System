using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace weatherSystem {
    public class ObjectYRotation : MonoBehaviour{

        //public string name;
        public float rotationSpeed;
        public Vector3 rotationCenter;
		
		void update(){
			float timePased = UnityEngine.Time.deltaTime;
			float rotationAngle = rotationSpeed*timePased;
            //transform.Rotate(Vector3.zero, Vector3.right, rotationAngle);
			//transform.RotateAround(rotationCenter, Vector3.right, rotationAngle);
		}
    }
}
