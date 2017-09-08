using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager {

    public int numRows, numCols;

    public int currentNumRows, currentNumCols;

    public BoardSpace[,] board;
    public List<BoardSpace> centerSpaces;

    public void GenerateBoard(int c, int r){
        centerSpaces = new List<BoardSpace>();
        numCols = c;
        numRows = r;
		board = new BoardSpace[numCols, numRows];
		for (int i = 0; i < numCols; i++)
		{
			for (int j = 0; j < numRows; j++)
			{
				int spaceColor;
				if (IsCentered(i, numCols) && IsCentered(j, numRows))
				{
					spaceColor = 0;
				}
				else if ((i + j) % 2 == 0)
				{
					spaceColor = 1;
				}
				else
				{
					spaceColor = 2;
				}
				CreateBoardSpace(i, j, spaceColor);
			}
		}
    }

	public bool IsCentered(int index, int sideLength)
	{
		bool centered = (index == sideLength / 2 - 1) || (index == sideLength / 2);
		return centered;
	}

    private void CreateBoardSpace(int colNum, int rowNum, int color){
        Vector3 location = new Vector3(colNum - numCols / 2 + 0.5f, 0, rowNum - numRows / 2 + 0.5f);
        GameObject boardSpace = Object.Instantiate(Services.Prefabs.BoardSpace, location, Quaternion.LookRotation(Vector3.down)) as GameObject;
        boardSpace.GetComponent<MeshRenderer>().material = Services.Materials.BoardMats[color];
        boardSpace.GetComponent<BoardSpace>().SetBoardSpace(color, colNum, rowNum);
		if (IsCentered(colNum, numCols) && IsCentered(rowNum, numRows))
		{
			boardSpace.GetComponent<BoardSpace>().isCenterSpace = true;
			centerSpaces.Add(boardSpace.GetComponent<BoardSpace>());
		}
		else
		{
			boardSpace.GetComponent<BoardSpace>().isCenterSpace = false;
		}
		board[colNum, rowNum] = boardSpace.GetComponent<BoardSpace>();
    }
}
