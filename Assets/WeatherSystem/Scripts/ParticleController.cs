using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleController : MonoBehaviour {

    public ParticleSystem particeSystem;

    public int particleCount_1;
    public int particleCount_2;
    public int particleCount_3;
    public int particleCount_4;
    public int particleCount_5;

    
    public void SetIntensity(int intensity) {
        var em = particeSystem.emission;

        switch (intensity) {
            case 0:
                em.rateOverTime = 0;
                break;
            case 1:
                em.rateOverTime = particleCount_1;
                break;
            case 2:
                em.rateOverTime = particleCount_2;
                break;
            case 3:
                em.rateOverTime = particleCount_3;
                break;
            case 4:
                em.rateOverTime = particleCount_4;
                break;
            case 5:
                em.rateOverTime = particleCount_5;
                break;
            //default:
            //    Debug.LogError("PARTICLECONTROLLER " + this.name + ": received a incorrect intensity value (" +  intensity + ")");
        }
    }
    void Awake () {
        var emision = particeSystem.emission;
        emision.rateOverTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
