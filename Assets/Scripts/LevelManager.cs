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

    public Tile selectedStackTile;
    public bool trashSelectedTile;

    public void Update()
    {
        fsm.Update();
    }

    public void CleanBoard(){
        for (int i = 0; i < numCols; ++i){
            for (int j = 0; j < numRows; j++){
                for (int n = 0; n < board[i, j].tileStack.Count; ++n){
                    Object.Destroy(board[i, j].tileStack[n].gameObject);
                }
                Object.Destroy(board[i, j].gameObject);
            }
        }

    }

    public void InitializeBoard()
    {
        DOTween.Clear(true);
        DOTween.ClearCachedTweens();
        DOTween.Validate();
        Time.timeScale = 1;
        mainBoard = GameObject.FindWithTag("Board");

        CreateBoard();

        pivotPoint = GameObject.FindGameObjectWithTag("PivotPoint");

        Services.LevelEditor.selectedTileMenu.SetActive(false);;

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

    private Tile CreateTile(int materialIndex)
    {
        Vector3 targetLocation = new Vector3(0, 0, 0);
        GameObject tile = Object.Instantiate(Services.Prefabs.Tile, targetLocation, Quaternion.identity) as GameObject;
        tile.transform.SetParent(mainBoard.transform);
        tile.GetComponent<MeshRenderer>().material = Services.Materials.TileMats[materialIndex];
        tile.GetComponent<Tile>().SetTile(materialIndex);

        return tile.GetComponent<Tile>();
    }

    public void ParseLevel(string lvl){
        string read = HandleTextFile.ReadString(lvl);
        string[] strs = read.Split('-');
        //tileBag = new List<Tile>();

        for (int i = 0; i < 8; ++i)
        {
            Services.LevelEditor.dropdowns[i].value = int.Parse(strs[i]);
        }

        for (int j = 8; j < strs.Length; ++j)
        {
            if (strs[j].Equals("."))
            {
                break;
            }
            else if (strs[j].Equals("[") && j + 5 < strs.Length)
            { //0=[, 1=c,2=comma,3=r,4=], possibly 5=.
                int c = int.Parse(strs[j + 1]);
                int r = int.Parse(strs[j + 3]);

                int k = j + 5;
                while (k < strs.Length - 1 && !strs[k].Equals("["))
                {
                    
                    int n = int.Parse(strs[k]);
                    //make tile, don't add to tilebag
                    Tile tileToPlace = CreateTile(n);
                    board[c, r].AddTile(tileToPlace, true);
                    k++;
                }
            }
        }
    }

    public void TrashTile(){

        trashSelectedTile = true;
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

    public void DefaultStateCheck(){
        StackSelectCheck();
    }

    public void StackSelectCheck(){
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.LevelEditor.tileLayer))
            {
                if (hit.collider.gameObject.GetComponent<Tile>() != null)
                {
                    //selectedStackTile = .GetComponent<Tile>();
                    selectedStackTile = hit.collider.gameObject.GetComponent<Tile>();
                    selectedStackTile.gameObject.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[1];
                }
            }
        }
    }

    public void SelectTileAction(){
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.LevelEditor.spawnedTileLayer))
            {
                
                selectedTile = currentSpawnedTile.GetComponent<Tile>();

            }
        }

    }

    public void PlaceTileAction(){
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.LevelEditor.topTileLayer))
        {

            Vector3 pointOnBoard = hit.transform.position;
            selectedTile.transform.position = new Vector3(pointOnBoard.x, pointOnBoard.y + 0.2f, pointOnBoard.z);
            //selectedTile.transform.parent = null;
            selectedTile.transform.parent = Services.LevelEditor.transform;
            tileInPosition = true;


        }
        else
        {
            tileInPosition = false;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.LevelEditor.invisPlane))
            {
                selectedTile.transform.position = hit.point;
            }

        }


        //finalize tile placement
        if (Input.GetMouseButtonDown(0) && tileInPosition)
        {
            tileInPosition = false;

            BoardSpace space = CalculateSpaceFromLocation(selectedTile.transform.position);
            space.AddTile(selectedTile, false);
            space.gameObject.layer = LayerMask.NameToLayer("Default");

            for (int i = 0; i < space.tileStack.Count; ++i){
                space.tileStack[i].gameObject.layer = LayerMask.NameToLayer("TileLayer");
            }

            selectedTile.GetComponent<MeshRenderer>().sortingOrder = 0;

            selectedTile.transform.SetParent(mainBoard.transform);
            selectedTile.gameObject.layer = LayerMask.NameToLayer("TopTiles");
            selectedTile = null;
            currentSpawnedTile = null;

        }

    }

    public void SelectStackTileAction(){
        if(trashSelectedTile){
            trashSelectedTile = false;
            //Services.

            //selectedStackTile

            BoardSpace space = CalculateSpaceFromLocation(selectedStackTile.transform.position);
            int index = space.tileStack.IndexOf(selectedStackTile);
            for (int i = space.tileStack.Count - 1; i > index; --i){
                space.tileStack[i].transform.position = space.tileStack[i - 1].transform.position;
            }
            space.tileStack.Remove(selectedStackTile);
            Object.Destroy(selectedStackTile.gameObject);
            if (space.tileStack.Count > 0)
            {
                space.tileStack[space.tileStack.Count - 1].gameObject.layer = LayerMask.NameToLayer("TopTiles");
            } else{
                space.gameObject.layer = LayerMask.NameToLayer("TopTiles");
            }
            selectedStackTile = null;
        }
    }

    private class Phase : FSM<LevelManager>.State { }

    private class DefaultState : Phase
    {
        public override void OnEnter()
        {
            Services.LevelEditor.selectedTileMenu.SetActive(false);
        }
        public override void Update()
        {
            if (Services.LevelEditor.currentTileColor >= 0)
            {
                Context.previousTileColor = Services.LevelEditor.currentTileColor;
                TransitionTo<SpawnTile>();
                return;
            } else if(Context.selectedStackTile != null){
                Services.LevelEditor.selectedTileMenu.SetActive(true);
                TransitionTo<SelectStackTile>();
                return;
                
            } 
            else{
                Context.DefaultStateCheck();
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

    private class SelectStackTile : Phase{
        public override void OnEnter(){
            Debug.Log("select stack tile");
        }
        public override void Update(){
            if (Context.selectedStackTile == null)
            {
                TransitionTo<DefaultState>();
                return;
            }
            else
            {
                Context.SelectStackTileAction();
            }
        }
    }



}
