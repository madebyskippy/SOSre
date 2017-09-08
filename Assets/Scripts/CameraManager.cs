using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        switch (Services.BoardData.numRows){
            case 4:
                GetComponent<Camera>().orthographicSize = 2;
                break;
            case 6:
				GetComponent<Camera>().orthographicSize = 3;
                break;
            case 8:
				GetComponent<Camera>().orthographicSize = 4;
                break;

        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
