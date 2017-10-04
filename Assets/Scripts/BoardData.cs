using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardData {

    public int numRows;
    public int numCols;

    public int[] initialNumberOfEachTileColor;
    public bool randomTiles;

    public bool hasPreviews;

    public void InitializeBoardData(){
        numRows = 6;
        numCols = numRows;

        hasPreviews = true;

        initialNumberOfEachTileColor = new int[4] { 20, 20, 20, 20 };

        randomTiles = true;
    }
}
