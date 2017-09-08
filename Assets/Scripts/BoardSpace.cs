using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace : MonoBehaviour {

    public int color;
    public int colNum, rowNum;
    public bool isCenterSpace;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetBoardSpace(int c, int col, int row){
        color = c;
        colNum = col;
        rowNum = row;
    }


}
