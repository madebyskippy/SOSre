using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

   // private new Renderer render = GetComponent<Renderer>();
	//public BoardSpace spaceQueuedToSpillOnto;
	public int color;

    void Start(){
        //render = 
        SetColor(3);
    }

    public void SetColor(int c){
        color = c;
        GetComponent<MeshRenderer>().material = Services.Materials.TileMats[c];
    }
}
