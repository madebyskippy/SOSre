using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using UnityStandardAssets.ImageEffects;

using DG.Tweening;

public class LevelManager {
    private FSM<LevelManager> fsm;


    public GameObject mainBoard;
    public GameObject pivotPoint;
    public BoardSpace[,] board;


    public List<BoardSpace> centerSpaces;

    public int numRows, numCols;

    public void Update()
    {
        fsm.Update();
    }

    public void InitializeBoard()
    {

        Time.timeScale = 1;
        mainBoard = GameObject.FindWithTag("Board");

        CreateBoard();

        pivotPoint = GameObject.FindGameObjectWithTag("PivotPoint");
    }
    private void CreateBoardSpace(int colNum, int rowNum, int color)
    {
        Vector3 location = new Vector3(colNum - numCols / 2 + 0.5f, 0, rowNum - numRows / 2 + 0.5f);
        GameObject boardSpace = Object.Instantiate(Services.Prefabs.BoardSpace, location, Quaternion.identity) as GameObject;
        boardSpace.transform.SetParent(mainBoard.transform);
        boardSpace.GetComponent<MeshRenderer>().material = Services.Materials.BoardMats[color];

        boardSpace.GetComponent<BoardSpace>().SetBoardSpace(color, colNum, rowNum);

        if (Services.BoardManager.IsCentered(colNum, numCols) && Services.BoardManager.IsCentered(rowNum, numRows))
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

    private void CreateBoard()
    {
        centerSpaces = new List<BoardSpace>();

        numCols = Services.BoardData.numCols;
        numRows = Services.BoardData.numRows;
        board = new BoardSpace[numCols, numRows];
        for (int i = 0; i < numCols; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                int spaceColor;
                if (Services.BoardManager.IsCentered(i, numCols) && Services.BoardManager.IsCentered(j, numRows))
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
}
