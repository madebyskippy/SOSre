using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using UnityStandardAssets.ImageEffects;

using DG.Tweening;

public class LevelManager {
    private FSM<LevelManager> fsm;


    public bool tileInPosition = false;
    public GameObject mainBoard;
    public GameObject pivotPoint;
    public BoardSpace[,] board;


    public List<BoardSpace> centerSpaces;

    public int numRows, numCols;

    private GameObject currentSpawnedTile;
    private int previousTileColor;
    public Tile selectedTile;

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


        fsm = new FSM<LevelManager>(this);
        fsm.TransitionTo<DefaultState>();
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
        boardSpace.layer = LayerMask.NameToLayer("TopTiles");
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
    BoardSpace CalculateSpaceFromLocation(Vector3 location)
    {
        int col = Mathf.RoundToInt(location.x - 0.5f + numCols / 2);
        int row = Mathf.RoundToInt(location.z - 0.5f + numRows / 2);
        return board[col, row];
    }

    public void SpawnTileAction(){
        
        Vector3 offscreen = new Vector3(-1000, -1000, -1000);
        int tileColor = Services.LevelEditor.currentTileColor;
       // if (tileColor >= 0)
       // {
            
           // Services.LevelEditor.currentTileColor = -1;
            if (currentSpawnedTile != null)
            {
            //Debug.Log("what");
                Object.Destroy(currentSpawnedTile.gameObject);
            }
            currentSpawnedTile = Object.Instantiate(Services.Prefabs.Tile, offscreen, Quaternion.identity) as GameObject;
            currentSpawnedTile.transform.SetParent(mainBoard.transform);
            currentSpawnedTile.GetComponent<MeshRenderer>().material = Services.Materials.TileMats[tileColor];
            currentSpawnedTile.GetComponent<Tile>().SetTile(tileColor);



            //spawnloc
            currentSpawnedTile.transform.DOLocalMove(new Vector3(-4.14f, 0, 0.27f), 0.5f).SetEase(Ease.OutBounce);
            currentSpawnedTile.gameObject.layer = LayerMask.NameToLayer("DrawnTile");
            //Services.Main.audioController.tilespawnentry.PlayOneShot(Services.Main.audioController.tilespawnentry.clip, 1f);



       // }

    }

    public void SelectTileAction(){
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.LevelEditor.spawnedTileLayer))
            {
               // ToggleTileGlow(spawnedTile, Brightness.Bright);
               // SetSpaceGlow(Brightness.Dark);
                selectedTile = currentSpawnedTile.GetComponent<Tile>();
                //spawnedTile = null;
                //tileFloatSequence.Kill();
                //Services.Main.audioController.select.Play();
            }
        }

    }

    public void PlaceTileAction(){
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.LevelEditor.topTileLayer))
        {
            //if (!CalculateSpaceFromLocation(hit.collider.transform.position).isCenterTile)
            //{
            Vector3 pointOnBoard = hit.transform.position;
            selectedTile.transform.position = new Vector3(pointOnBoard.x, pointOnBoard.y + 0.2f, pointOnBoard.z);
            //selectedTile.transform.parent = null;
            selectedTile.transform.parent = Services.LevelEditor.transform;
            tileInPosition = true;
            //BoardSpace space = CalculateSpaceFromLocation(pointOnBoard);
           /* ToggleSpaceGlow(space, Brightness.Bright);
            if (highlightedSpace != null)
            {
                if (highlightedSpace != space)
                {
                    ToggleSpaceGlow(highlightedSpace, Brightness.Normal);
                }
            }
            highlightedSpace = space;*/

        }
        else
        {
            tileInPosition = false;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.LevelEditor.invisPlane))
            {
                selectedTile.transform.position = hit.point;
            }
            //selectedTile.transform.position = Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);
           /* if (highlightedSpace != null)
            {
                ToggleSpaceGlow(highlightedSpace, Brightness.Normal);
                highlightedSpace = null;
            }*/
        }


        //finalize tile placement
        if (Input.GetMouseButtonDown(0) && tileInPosition)
        {
           // Services.Main.audioController.select.Play();
            tileInPosition = false;

            BoardSpace space = CalculateSpaceFromLocation(selectedTile.transform.position);
            space.AddTile(selectedTile, false);
            space.gameObject.layer = LayerMask.NameToLayer("Default");
            selectedTile.GetComponent<MeshRenderer>().sortingOrder = 0;
           /* ToggleTileGlow(selectedTile, Brightness.Normal);
           / SetSpaceGlow(Brightness.Normal);
            if (highlightedSpace != null)
            {
                ToggleSpaceGlow(highlightedSpace, Brightness.Normal);
            }*/
            //selectedTile.GetComponent<AudioSource>().Play();
            selectedTile.transform.SetParent(mainBoard.transform);
            selectedTile = null;
            currentSpawnedTile = null;

        }

    }

    private class Phase : FSM<LevelManager>.State { }

    private class DefaultState : Phase
    {
        public override void OnEnter()
        {
            Debug.Log("default");
        }
        public override void Update()
        {
            if (Services.LevelEditor.currentTileColor >= 0)
            {
                Context.previousTileColor = Services.LevelEditor.currentTileColor;
                TransitionTo<SpawnTile>();
                return;
            }
        }

    }

    private class SpawnTile : Phase{
        public override void OnEnter(){
            Debug.Log("spawn");

        }
        public override void Update(){
            Context.SpawnTileAction();
            //TransitionTo<DefaultState>();
            TransitionTo<SelectTile>();
            return;
        }
    }
    private class SelectTile : Phase
    {
        public override void OnEnter()
        {

            Services.LevelEditor.currentTileColor = -1;
            Debug.Log("SelectTile");
        }
        public override void Update()
        {
            if (Services.LevelEditor.currentTileColor >= 0)
            {
                Context.previousTileColor = Services.LevelEditor.currentTileColor;
                TransitionTo<SpawnTile>();
                return;
            }
            else
            {
                if (Context.selectedTile == null)
                {
                    Context.SelectTileAction();
                }
                else
                {
                    TransitionTo<PlaceTile>();
                    return;
                }
            }



        }
    }

    private class PlaceTile : Phase //click a tile button to spawntile
    {
        public override void OnEnter()
        {
            Debug.Log("place");
        }
        public override void Update()
        {
            /*if (Context.previousTileColor != Services.LevelEditor.currentTileColor)
            {
                TransitionTo<DefaultState>();
            } else{
                if (Context.selectedTile != null)
                {
                    Context.PlaceTileAction();
                }
            }*/
            if (Services.LevelEditor.currentTileColor >= 0)
            {
                Context.previousTileColor = Services.LevelEditor.currentTileColor;
                TransitionTo<SpawnTile>();
                return;
            }
            else
            {
                if (Context.selectedTile != null)
                {
                    Context.PlaceTileAction();
                }
                else
                {
                    TransitionTo<DefaultState>();
                    return;
                }
            }
            
        }
    }

}
