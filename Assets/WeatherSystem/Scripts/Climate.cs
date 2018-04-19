using UnityEngine;

namespace weatherSystem{ //es posible que este mal y sea WearherSystem
public class Climate: MonoBehaviour{
	
	[Tooltip("For testing purposes. If checked debug messages will be writen on the console")]
	public bool debug;
	
	[Header("Button Settings")]	
	[Tooltip("File that includes the probabilities and intensities of each weather variable for any season")]
	public TextAsset Configuration;
	
	//private assets
	private int NUMBER_OF_WEATHER_CONDITIONS;
	private int[][] probabilities;
	private int[][] intensities;
	
	
	
	public void Awake(){
		if(Configuration == null){
                Debug.LogError("Climate " + this.name + "configuration file is missing. Prease add a reference to the appropiate configuration file");
		}
		
		//leer del fichero y rellenar cada variable
		//1º - conseguir el numero de estaciones del fichero	
			//para eso: leer hasta que se encuentre la linea:
			// 			/*VARIABLE DEFINITION*/
			//obtener lista de objetos etiquetados como season
			//por cada lina, hasta que haya linea en blanco:
				//obtener el numero de id
				//buscar en la lista de seasons cual tiene el nombre igual al que hay en el documento
			//si numero de seasons en el documento no es igual a la lista de objetos etiquetados como season devolver error
			
			
	}
	void start(){}
	void update(){}
	
}
	
	
}