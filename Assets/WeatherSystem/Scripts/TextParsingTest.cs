using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextParsingTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string text = "bla: \n bla, bla, bla,bla \n";
        string[] split = text.Split('\n');

        Debug.Log(text);
        for (int i = 0; i<split.GetLength(0); i++) {
            Debug.Log("Line " + i + ": " + split[i]);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
