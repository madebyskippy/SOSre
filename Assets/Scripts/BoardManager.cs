using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager {

    private FSM<BoardManager> fsm;

    public int numRows, numCols;

    public int currentNumRows, currentNumCols;

    public BoardSpace[,] board;
    public List<BoardSpace> centerSpaces;
    public List<Tile> tileBag;

    public GameObject pivotPoint;

    public Tile spawnedTile;
    public Tile selectedTile;
    public LayerMask spawnedTileLayer;



    public void Update(){
        fsm.Update();
    }

	public void InitializeBoard()
    {
        spawnedTileLayer = LayerMask.NameToLayer("DrawnTile");
        CreateBoard();
        CreateTileBag();

        pivotPoint = GameObject.FindGameObjectWithTag("PivotPoint");

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



		fsm = new FSM<BoardManager>(this);
		fsm.TransitionTo<SpawnTile>();
        //Debug.Log(fsm);

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

	public void DrawTileToPlace()
	{
		Tile tileToPlace;
		tileToPlace = DrawTile();
		if (tileToPlace == null)
		{
            //yield return new WaitForSeconds (1f);
            //mode = "Game Over";
		}
		else
		{
			SetupSpawnedTile(tileToPlace);
			spawnedTile = tileToPlace;
			spawnedTile.GetComponent<MeshRenderer>().sortingOrder = 2;
		}
	}

	void SetupSpawnedTile(Tile tileToPlace)
	{
		tileToPlace.transform.SetParent(pivotPoint.transform);
		tileToPlace.transform.localPosition = new Vector3(-5, 0, 0);
		tileToPlace.gameObject.layer = LayerMask.NameToLayer("DrawnTile");
		//juicyManager.spawnTileAnimation(tileToPlace.gameObject);
	}






    public void SpawnTileAction(){

        DrawTileToPlace();

    }

    public void SelectTileAction(){

        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.layer == spawnedTileLayer)
                {
                    //ToggleGlow(spawnedTile, "bright");
                    // SetSpaceGlow("dark");
                    selectedTile = spawnedTile;
                    spawnedTile = null;
                }
            }
        }
    }

    public void PlaceTileAction(){
        //Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit = new RaycastHit();
        selectedTile.transform.position = Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);

    }




	private class Turn : FSM<BoardManager>.State { }

	private class SpawnTile : Turn
	{
		public override void OnEnter()
		{
            Context.SpawnTileAction();
		}
		public override void Update()
		{
            TransitionTo<SelectTile>();
            return;
		}
	}

	private class SelectTile : Turn
	{
		public override void OnEnter()
		{
		}
		public override void Update()
		{
            if (Context.spawnedTile != null)
            {
                Context.SelectTileAction();
            } else{
                TransitionTo<PlaceTile>();
                return;
            }



		}
	}

	private class PlaceTile : Turn
	{
		public override void OnEnter()
		{
		}
		public override void Update()
		{
            if (Context.selectedTile != null)
            {
                Context.PlaceTileAction();
            } else{
                TransitionTo<SelectStack>();
                return;
            }
		}
	}

	private class SelectStack : Turn
	{
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
	}

	private class QueueSpill : Turn
	{
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
	}

	private class Interim : Turn
	{
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
	}

	private class FinalizeSpill : Turn
	{
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
	}

	private class GameOver : Turn
	{
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
	}
}
