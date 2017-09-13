using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardData {

    public int numRows;
    public int numCols;

    public int[] initialNumberOfEachTileColor;
    public bool randomTiles;

    public void InitializeBoardData(){
        numRows = 8;
        numCols = numRows;

        initialNumberOfEachTileColor = new int[4] { 20, 20, 20, 20 };

        randomTiles = true;
    }
}
