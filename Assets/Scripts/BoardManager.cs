using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager {

    public int numRows, numCols;

    public int currentNumRows, currentNumCols;

    public BoardSpace[,] board;
    public List<BoardSpace> centerSpaces;
    public List<Tile> tileBag;

	public void InitializeBoard()
    {
        CreateBoard();
        CreateTileBag();

        if(Services.BoardData.randomTiles){
			for (int i = 0; i < numCols; i++)
			{
				for (int j = 0; j < numRows; j++)
				{
					if ((!IsCentered(i, numCols) && IsEdge(j, numRows)) || (!IsCentered(j, numRows) && IsEdge(i, numCols)))
					{
						Tile firstTileToPlace;
						Tile secondTileToPlace;
						firstTileToPlace = DrawTile();
						secondTileToPlace = DrawTile();
						board[i, j].AddTile(firstTileToPlace, true);
						board[i, j].AddTile(secondTileToPlace, true);
					}
				}
			}

        } else{


        }

	}

    private void CreateBoard(){
        centerSpaces = new List<BoardSpace>();

        numCols = Services.BoardData.numCols;
        numRows = Services.BoardData.numRows;
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

	private bool IsEdge(int index, int sideLength)
	{
		bool edge = (index == 0) || (index == sideLength - 1);
		return edge;
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

    private void CreateTile(int materialIndex){
		//GameObject tile;
		Vector3 offscreen = new Vector3(-1000, -1000, -1000);
        GameObject tile = Object.Instantiate(Services.Prefabs.Tile, offscreen, Quaternion.identity) as GameObject;
        tile.GetComponent<MeshRenderer>().material = Services.Materials.TileMats[materialIndex];
        tile.GetComponent<Tile>().SetTile(materialIndex);
		tileBag.Add(tile.GetComponent<Tile>());
    }

	private void CreateTileBag()
	{
		tileBag = new List<Tile>();
        for (int i = 0; i < 4; ++i){
            CreateTilesOfAColor(i);
        }
	}

    private void CreateTilesOfAColor(int materialIndex){
        for (int i = 0; i < Services.BoardData.initialNumberOfEachTileColor[materialIndex]; ++i)
		{
			CreateTile(materialIndex);
		}
    }

	public Tile DrawTile()
	{
		//prevent out of range exception
		if (tileBag.Count > 0)
		{
			int numTilesInBag = tileBag.Count;
			Tile drawnTile;
			int tileIndexToDraw;
            if (Services.BoardData.randomTiles)
			{
				tileIndexToDraw = Random.Range(0, numTilesInBag);
			}
			else
			{
				tileIndexToDraw = 0;
			}
			drawnTile = tileBag[tileIndexToDraw];
			tileBag.Remove(drawnTile);
			return drawnTile;
		}
		return null;
	}

}
