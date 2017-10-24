using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Diagnostics;
using UnityEngine.UI;

using DG.Tweening;
using Shuffler;

public class BoardManager
{

    private FSM<BoardManager> fsm;

    public GameObject mainBoard;

    public int score;
    public int numRows, numCols;

    public int currentNumRows, currentNumCols;

    private int currentLowestColIndex, currentHighestColIndex, currentLowestRowIndex, currentHighestRowIndex;

    public BoardSpace[,] board;
    public List<BoardSpace> centerSpaces;
    public List<Tile> tileBag;
    public GameObject spillUI;
    private Component[] spillArrowRenderers;

    public bool centerSpaceChanged;
    public bool undoSpill;

    public int rotationIndex;
    private float tileSpillIndivDuration = 0.5f;

    public GameObject pivotPoint;

    private Vector3[] spawnLocs = { new Vector3(-4.14f, 0, 0.27f),
                                 //   new Vector3()
                                    };

    public Tile spawnedTile;
    public Tile selectedTile;
    public List<Tile> tilesQueuedToSpill;
    public BoardSpace spaceQueuedToSpillFrom;

    private BoardSpace selectedSpace;
    public BoardSpace spaceToSpill;

    public int numSidesCollapsed;
    private int highestStackIndexInSpacesToCollapse;

    private int spillDirectionX, spillDirectionZ;

    public bool boardFinishedEntering = false;
    public bool stackSelected = false;
    public bool startSpill = false;
    public bool finalizeSpill = false;
    public bool boardFalling = false;
    public bool lastTileInBoardFall = false;
    public bool boardFinishedFalling = false;
    public bool finishedCheckingScore = false;
    private bool scoreAnimationStarted = false;

    public bool tileInPosition = false;
    public int sideAboutToCollapse;

    public BoardSpace highlightedSpace;

    private List<Tile> initialTilesOnBoard;

    private Sequence tileFloatSequence;
    private Sequence boardCollapseSequence;

    private Vector3 oneLocation = new Vector3(-2.5f, 0.5f, -2.5f);
    private Vector3 location = new Vector3(-2.5f, 0.5f, -2.5f);

    private Image[] previewTiles;



    public enum Brightness {Bright,Dark,Normal}

    public void Update()
    {
        fsm.Update();
    }

    public void InitializeBoard()
    {
        previewTiles = new Image[3];
        previewTiles[0] = Services.Main.Previews.transform.GetChild(0).GetComponent<Image>();
		previewTiles[1] = Services.Main.Previews.transform.GetChild(1).GetComponent<Image>();
		previewTiles[2] = Services.Main.Previews.transform.GetChild(2).GetComponent<Image>();

        score = 0;
        numSidesCollapsed = 0;
        mainBoard = GameObject.FindWithTag("Board");
        Services.Main.ConfirmUndoUI.SetActive(false);

        CreateBoard();
        CreateTileBag();

        pivotPoint = GameObject.FindGameObjectWithTag("PivotPoint");

        currentNumRows = numRows;
        currentNumCols = numCols;

        currentLowestColIndex = 0;
        currentLowestRowIndex = 0;
        currentHighestRowIndex = numRows - 1;
        currentHighestColIndex = numCols - 1;

        rotationIndex = 0;
        initialTilesOnBoard = new List<Tile>();

        if (Services.BoardData.randomTiles)
        {
            for (int b = 0; b < 2; b++)
            {
                for (int i = 0; i < numCols; i++)
                {
                    for (int j = 0; j < numRows; j++)
                    {
                        if ((!IsCentered(i, numCols) && IsEdge(j, numRows)) || (!IsCentered(j, numRows) && IsEdge(i, numCols)))
                        {
                            Tile tileToPlace;
                            tileToPlace = DrawTile();
                            board[i, j].AddTile(tileToPlace, true);
                            tileToPlace.GetComponent<MeshRenderer>().enabled = false;
                            initialTilesOnBoard.Add(tileToPlace);
                        }
                    }
                }
            }

        }
        else
        {


        }

        SetPreviews();

        fsm = new FSM<BoardManager>(this);
        fsm.TransitionTo<EnterBoard>();

    }

    private void SetPreviews(){
        int[] previewColors = FirstThreeColors();
        for (int i = 0; i < 3; ++i)
        {
            if (previewColors[i] == -1)
            {
                previewTiles[i].sprite = null;
            }
            else
            {
                previewTiles[i].sprite = Services.Materials.PreviewSprites[previewColors[i]];
            }
        }


    }

    private int[] FirstThreeColors(){
        int[] firstthree = new int[3];
        if (tileBag.Count > 3)
        {
            for (int i = 0; i < 3; ++i)
            {
                firstthree[i] = tileBag[i].color;
            }
        } else if(tileBag.Count == 2){
            firstthree[0] = tileBag[0].color;
            firstthree[1] = tileBag[1].color;
            firstthree[2] = -1;
        } else if(tileBag.Count == 1){
			firstthree[0] = tileBag[0].color;
            firstthree[1] = -1;
			firstthree[2] = -1;
        } else{
            firstthree[0] = -1;
			firstthree[1] = -1;
			firstthree[2] = -1;
        }

        return firstthree;
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

    public void AnimateEnterBoard()
    {
        Vector3[,] targetLocations = new Vector3[numCols, numRows];
        Sequence boardSequence = DOTween.Sequence();

        for (int i = 0; i < numCols; ++i)
        {
            for (int j = 0; j < numRows; ++j)
            {
                // targetLocations[i, j] = board[i, j].transform.position;
                Vector3 targetLocation = board[i, j].transform.position;
                board[i, j].transform.position = new Vector3(targetLocation.x, -10f, targetLocation.z);
                board[i, j].GetComponent<MeshRenderer>().enabled = true;
                boardSequence.Insert(0.07f * (j + i * numRows), board[i, j].transform.DOMoveY(targetLocation.y, 0.6f).SetEase(Ease.InOutBack));
            }
        }

        Sequence tileSequence = DOTween.Sequence();
        for (int t = 0; t < initialTilesOnBoard.Count; ++t)
        {
            Vector3 targetLocation = initialTilesOnBoard[t].transform.position;
            initialTilesOnBoard[t].transform.position = new Vector3(targetLocation.x, 10f, targetLocation.z);
            initialTilesOnBoard[t].GetComponent<MeshRenderer>().enabled = true;
            boardSequence.Insert((0.07f * t) + 2f, initialTilesOnBoard[t].transform.DOMoveY(targetLocation.y, 0.5f).SetEase(Ease.Linear));
        }
    

		boardSequence.OnComplete(OnCompleteEnterBoard);
		boardSequence.Play();

        //boardSequence.Append

    }

    private void OnCompleteEnterBoard(){
        boardFinishedEntering = true;
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


    private void CreateBoardSpace(int colNum, int rowNum, int color)
    {
        Vector3 location = new Vector3(colNum - numCols / 2 + 0.5f, 0, rowNum - numRows / 2 + 0.5f);
        GameObject boardSpace = Object.Instantiate(Services.Prefabs.BoardSpace, location, Quaternion.identity) as GameObject;
        boardSpace.transform.SetParent(mainBoard.transform);
        boardSpace.GetComponent<MeshRenderer>().material = Services.Materials.BoardMats[color];
		boardSpace.GetComponent<MeshRenderer>().enabled = false; //temporarily invisible for animation

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

    private void CreateTile(int materialIndex)
    {
        //GameObject tile;
        Vector3 offscreen = new Vector3(-1000, -1000, -1000);
        GameObject tile = Object.Instantiate(Services.Prefabs.Tile, offscreen, Quaternion.identity) as GameObject;
        tile.transform.SetParent(mainBoard.transform);
        tile.GetComponent<MeshRenderer>().material = Services.Materials.TileMats[materialIndex];
        tile.GetComponent<Tile>().SetTile(materialIndex);
        tileBag.Add(tile.GetComponent<Tile>());
    }

    private void CreateTileBag()
    {
        tileBag = new List<Tile>();
        for (int i = 0; i < 4; ++i)
        {
            CreateTilesOfAColor(i);
        }
        if(Services.BoardData.randomTiles){
            //ShuffleTileBag();
            tileBag.Shuffle();
        }

    }



    private void CreateTilesOfAColor(int materialIndex)
    {
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
            Tile drawnTile = tileBag[0];
           /* int tileIndexToDraw;
            if (Services.BoardData.randomTiles)
            {
                tileIndexToDraw = Random.Range(0, numTilesInBag);
            }
            else
            {
                tileIndexToDraw = 0;
            }*/
            //drawnTile = tileBag[0];
            tileBag.Remove(drawnTile);
            SetPreviews();
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

        tileToPlace.transform.position = new Vector3(-10, 0, 0);
        tileToPlace.transform.SetParent(pivotPoint.transform, false);
        //tileToPlace.transform.SetParent(mainBoard.transform);

        //spawnloc
        tileToPlace.transform.DOLocalMove(new Vector3(-4.14f, 0, 0.27f), 0.5f).SetEase(Ease.OutBounce).OnComplete(PlayFloatSequence);
        tileToPlace.gameObject.layer = LayerMask.NameToLayer("DrawnTile");


        //setup floating sequence for the spawnedtile
		tileFloatSequence = DOTween.Sequence();
        tileFloatSequence.Append(tileToPlace.transform.DOMoveY(0.1f, 0.3f).SetEase(Ease.OutSine))
                         .Append(tileToPlace.transform.DOMoveY(-0.1f, 0.3f).SetEase(Ease.OutSine))
                         .Append(tileToPlace.transform.DOMoveY(0, 0.2f).SetEase(Ease.Linear));
        tileFloatSequence.SetLoops(-1);

    }

    private void PlayFloatSequence(){
		tileFloatSequence.Play();
    }

    BoardSpace CalculateSpaceFromLocation(Vector3 location)
    {
        int col = Mathf.RoundToInt(location.x - 0.5f + numCols / 2);
        int row = Mathf.RoundToInt(location.z - 0.5f + numRows / 2);
        return board[col, row];
    }

    public List<BoardSpace> GetSpaceListFromSideNum()
    {
        List<BoardSpace> spaceList = new List<BoardSpace>();
        int indexToCollapse = 0;
        if (sideAboutToCollapse == 0)
        {
            indexToCollapse = currentLowestColIndex;
        }
        else if (sideAboutToCollapse == 1)
        {
            indexToCollapse = currentHighestRowIndex;
        }
        else if (sideAboutToCollapse == 2)
        {
            indexToCollapse = currentHighestColIndex;
        }
        else
        {
            indexToCollapse = currentLowestRowIndex;
        }
        if ((sideAboutToCollapse % 2) == 0)
        {
            for (int i = currentLowestRowIndex; i < currentHighestRowIndex + 1; i++)
            {
                spaceList.Add(board[indexToCollapse, i]);
            }
        }
        else
        {
            for (int i = currentLowestColIndex; i < currentHighestColIndex + 1; i++)
            {
                spaceList.Add(board[i, indexToCollapse]);
            }
        }
        return spaceList;
    }

    private bool IsGoingOverEdge(int x, int z, int xDirection, int zDirection){
        int targetX = x + xDirection;
        int targetZ = z + zDirection;
        if(targetX > currentHighestColIndex || targetX < currentLowestColIndex
           || targetZ > currentHighestRowIndex || targetZ < currentLowestRowIndex){
            return true;
        }
        return false;

    }

    int[] CalculateAdjacentSpace(int x, int z, int xDirection, int zDirection)
    {
        int[] coords = new int[2];
        int targetX = x + xDirection;
        if (targetX > currentHighestColIndex)
        {
            targetX = currentLowestColIndex;
        }
        else if (targetX < currentLowestColIndex)
        {
            targetX = currentHighestColIndex;
        }
        int targetZ = z + zDirection;
        if (targetZ > currentHighestRowIndex)
        {
            targetZ = currentLowestRowIndex;
        }
        else if (targetZ < currentLowestRowIndex)
        {
            targetZ = currentHighestRowIndex;
        }
        coords[0] = targetX;
        coords[1] = targetZ;
        return coords;
    }

    int[] GetDirectionFromSideNum()
    {
        int[] coords = new int[2];
        if ((sideAboutToCollapse % 2) == 0)
        {
            coords[0] = 1 - sideAboutToCollapse;
            coords[1] = 0;
        }
        else
        {
            coords[0] = 0;
            coords[1] = sideAboutToCollapse - 2;
        }
        return coords;
    }

    Vector3[] BeyondEitherEdges(int x, int z, int xDirection, int zDirection){
        //index 0 = first stop, index 1 = the other side
        Vector3[] targetLocs = new Vector3[2];
        if(xDirection > 0){ //going up

            targetLocs[0] = board[currentHighestColIndex, z].transform.position + new Vector3(1, 0, 0);
            targetLocs[1] = board[currentLowestColIndex, z].transform.position + new Vector3(-1, 0, 0);
        } else if(xDirection < 0){ //going down

            targetLocs[0] = board[currentLowestColIndex, z].transform.position + new Vector3(-1, 0, 0);
            targetLocs[1] = board[currentHighestColIndex, z].transform.position + new Vector3(1, 0, 0);
        }

        if(zDirection > 0) { //going left

            targetLocs[0] = board[x, currentHighestRowIndex].transform.position + new Vector3(0, 0, 1);
            targetLocs[1] = board[x, currentLowestRowIndex].transform.position + new Vector3(0, 0, -1);
        } else if(zDirection < 0){ //going right

            targetLocs[0] = board[x, currentLowestRowIndex].transform.position + new Vector3(0, 0, -1);
            targetLocs[1] = board[x, currentHighestRowIndex].transform.position + new Vector3(0, 0, 1);
        }

        return targetLocs;
    }

    private void IndicateCollapsibleSide()
    {
        List<BoardSpace> boardspaces = GetSpaceListFromSideNum();
        foreach (BoardSpace bs in boardspaces)
        {
            bs.gameObject.GetComponent<MeshRenderer>().material = Services.Materials.BoardMats[3];
            //Debug.Log("recolor collapsible boardspaces");
            bs.aboutToCollapse = true;
        }
    }


    public void SpawnTileAction()
    {

        DrawTileToPlace();
        if (currentNumCols < numCols || currentNumRows < numRows)
        {
            IndicateCollapsibleSide();
        }

    }


    public void SelectTileAction()
    {
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.Main.spawnedTileLayer))
            {
                ToggleTileGlow(spawnedTile, Brightness.Bright);
                SetSpaceGlow(Brightness.Dark);
                selectedTile = spawnedTile;
                spawnedTile = null;
                tileFloatSequence.Kill();
            }
        }
    }

    public void PlaceTileAction()
    {
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.Main.topTileLayer))
        {
            //if (!CalculateSpaceFromLocation(hit.collider.transform.position).isCenterTile)
            //{
            Vector3 pointOnBoard = hit.transform.position;
            selectedTile.transform.position = new Vector3(pointOnBoard.x, pointOnBoard.y + 0.2f, pointOnBoard.z);
            //selectedTile.transform.parent = null;
            selectedTile.transform.parent = Services.Main.transform;
            tileInPosition = true;
            BoardSpace space = CalculateSpaceFromLocation(pointOnBoard);
            ToggleSpaceGlow(space, Brightness.Bright);
            if (highlightedSpace != null)
            {
                if (highlightedSpace != space)
                {
                    ToggleSpaceGlow(highlightedSpace, Brightness.Normal);
                }
            }
            highlightedSpace = space;

        }
        else
        {
            tileInPosition = false;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.Main.invisPlane))
            {
                selectedTile.transform.position = hit.point;
            }
			//selectedTile.transform.position = Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);
			if (highlightedSpace != null)
			{
                ToggleSpaceGlow(highlightedSpace, Brightness.Normal);
				highlightedSpace = null;
			}
        }


        //finalize tile placement
        if (Input.GetMouseButtonDown(0) && tileInPosition)
        {
            tileInPosition = false;

            BoardSpace space = CalculateSpaceFromLocation(selectedTile.transform.position);
            space.AddTile(selectedTile, false);
            selectedTile.GetComponent<MeshRenderer>().sortingOrder = 0;
            ToggleTileGlow(selectedTile, Brightness.Normal);
            SetSpaceGlow(Brightness.Normal);
            if (highlightedSpace != null)
			{
                ToggleSpaceGlow(highlightedSpace, Brightness.Normal);
			}
            //CheckForScore();

            if (currentNumCols == numCols && currentNumRows == numRows) //!firstTileFinalized)
            {

                //firstTileFinalized = true;
                if ((space.colNum == 0) && (space.rowNum != 0))
                {
                    sideAboutToCollapse = 0;
                }
                else if (space.rowNum == numRows - 1)
                {
                    sideAboutToCollapse = 1;
                }
                else if (space.colNum == numCols - 1)
                {
                    sideAboutToCollapse = 2;
                }
                else
                {
                    sideAboutToCollapse = 3;
                }

                IndicateCollapsibleSide();
            }
            //selectedTile.GetComponent<AudioSource>().Play();
            selectedTile.transform.SetParent(mainBoard.transform);
            selectedTile = null;

        }

    }

    public void SelectStackAction()
    {
        Ray ray = Services.GameManager.currentCamera.ScreenPointToRay(Input.mousePosition);
      /*  if(!stackSelected){
			SetIneligibleSpaceGlow(Brightness.Dark);
        } else{
            SetIneligibleSpaceGlow(Brightness.Normal);
        }*/
        if (Input.GetMouseButtonDown(0))
        {

            RaycastHit hit;

            if (stackSelected)
            {
                //highlightspillarrow
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.Main.spillUILayer))
                {
                    spillDirectionX = 0;
                    spillDirectionZ = 0;
                    //soundplayer.transform.GetChild(5).gameObject.GetComponent<AudioSource>().Play();
                    switch (hit.collider.transform.name)
                    {
                        case "MinusX":
                            spillDirectionX = -1;
                            break;
                        case "PlusX":
                            spillDirectionX = 1;
                            break;
                        case "MinusZ":
                            spillDirectionZ = -1;
                            break;
                        case "PlusZ":
                            spillDirectionZ = 1;
                            break;
                    }

                    startSpill = true;

                    spillUI.SetActive(false);
                    return;
                }
                else
                {
                    startSpill = false;
                }
            }


            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.Main.topTileLayer))
            {
                Vector3 tileHitLocation = hit.transform.position;
                BoardSpace space = CalculateSpaceFromLocation(tileHitLocation);
                if (space.tileStack.Count > 1)
                {
                    stackSelected = true;

                    if (selectedSpace != null)
                    {
                        if (selectedSpace != space)
                        {
                            ToggleTileGlow(selectedSpace.tileStack, Brightness.Normal);
                            ToggleTileGlow(space.tileStack, Brightness.Bright);
                        }
                    }
                    else
                    {
                        ToggleTileGlow(space.tileStack, Brightness.Bright);
                    }
                    selectedSpace = space;
                    spaceToSpill = selectedSpace;
                    Vector3 topTileLocation = selectedSpace.tileStack[selectedSpace.tileStack.Count - 1].transform.position;
                    Object.Destroy(spillUI);
                    spillUI = Object.Instantiate(Services.Prefabs.SpillUI,
                        new Vector3(topTileLocation.x, topTileLocation.y, topTileLocation.z), Quaternion.identity) as GameObject;
                    spillUI.transform.SetParent(mainBoard.transform);
                    spillArrowRenderers = spillUI.GetComponentsInChildren<MeshRenderer>();
                    /*foreach(MeshRenderer mr in spillArrowRenderers){
                        mr.material.renderQueue = 5000;
                    }*/
                    //previously spillDirectionUI was: -9.2, 9.2, -9.2
                    spillUI.transform.eulerAngles = new Vector3(0, rotationIndex * 90, 0);
                    spillUI.transform.GetChild(0).transform.localEulerAngles = new Vector3(0, -rotationIndex * 90, 0);
                }
            }
        }
        HighlightSpillArrow(ray);
    }

   /* private void */

    public void ConfirmSpill()
    {
        finalizeSpill = true;

        ToggleTileGlow(tilesQueuedToSpill,Brightness.Normal);
    }

    public void UndoSpill(){ 
        undoSpill = true;
        spillUI.SetActive(true);

        for (int i = 0; i < tilesQueuedToSpill.Count; ++i){
            Tile tile = tilesQueuedToSpill[i];
            tile.spaceQueuedToSpillOnto.tileStack.Remove(tile);

            if (tile.spaceQueuedToSpillOnto.tileStack.Count > 0)
            {
                tile.spaceQueuedToSpillOnto.tileStack[tile.spaceQueuedToSpillOnto.tileStack.Count - 1].gameObject.layer = LayerMask.NameToLayer("TopTiles");
            }
            tile.spaceQueuedToSpillOnto.provisionalTileCount = tile.spaceQueuedToSpillOnto.tileStack.Count;
            tile.spaceQueuedToSpillOnto = null;


        }
		spaceQueuedToSpillFrom.tileStack = tilesQueuedToSpill;
		spaceQueuedToSpillFrom.ResetTilesToPosition(); 
		spaceQueuedToSpillFrom.provisionalTileCount = spaceQueuedToSpillFrom.tileStack.Count;
        spaceToSpill.provisionalTileCount = spaceToSpill.tileStack.Count;
        spillUI.transform.eulerAngles = new Vector3(0, rotationIndex * 90, 0);
        spillUI.transform.GetChild(0).transform.localEulerAngles = new Vector3(0, -rotationIndex * 90, 0);

    }

    private void QueueSpillHelper(BoardSpace toBeSpilled, int xDirection, int zDirection)
    {

        int boardSpaceX = toBeSpilled.colNum;
        int boardSpaceZ = toBeSpilled.rowNum;

        //get tiles that are queued to spill
        tilesQueuedToSpill = new List<Tile>();

        //the space to be spilled: how many tiles it got left
        int numTilesToMove = toBeSpilled.tileStack.Count;
        toBeSpilled.provisionalTileCount = 0;
        spaceQueuedToSpillFrom = toBeSpilled;


		Sequence tileSpillSequence = DOTween.Sequence();
        tileSpillSequence.Pause(); //make sure it's not playing prematurely

        int maxStackHeight = 1;

        bool startsGoingOverEdge = false;

        //gets the locations off the lowest/highest boardspace in the row/column
        Vector3[] eitherEdgeTargetLocs = BeyondEitherEdges(boardSpaceX, boardSpaceZ, xDirection, zDirection);
        Vector3 midTargetLoc = (eitherEdgeTargetLocs[0] + eitherEdgeTargetLocs[1]) / 2f;
        midTargetLoc = new Vector3(midTargetLoc.x, -2f, midTargetLoc.z);

        //spill each tile in the stack
        for (int i = 0; i < numTilesToMove; i++)
        {
            int index = numTilesToMove - 1 - i;
            Tile tileToMove = toBeSpilled.tileStack[index];
            tilesQueuedToSpill.Add(tileToMove);
            int[] targetCoords = CalculateAdjacentSpace(boardSpaceX, boardSpaceZ, xDirection, zDirection);
            bool isGoingOverEdge = IsGoingOverEdge(boardSpaceX, boardSpaceZ, xDirection, zDirection);

            boardSpaceX = targetCoords[0];
            boardSpaceZ = targetCoords[1];
            BoardSpace spaceToSpillOnto = board[boardSpaceX, boardSpaceZ];
            tileToMove.spaceQueuedToSpillOnto = spaceToSpillOnto;
            toBeSpilled.tileStack.Remove(tileToMove);

			if (isGoingOverEdge){
				startsGoingOverEdge = true;
			}

            // ANIMATE SPILL


            if (spaceToSpillOnto.tileStack.Count > maxStackHeight) {
                maxStackHeight = spaceToSpillOnto.tileStack.Count;
            }
            float jumpHeight = maxStackHeight;
            if(maxStackHeight > 1){
                jumpHeight *= 0.65f;
            }
            Vector3 targetLocation = new Vector3(spaceToSpillOnto.transform.position.x,
                        spaceToSpillOnto.provisionalTileCount * 0.2f + 0.1f,
                                                 spaceToSpillOnto.transform.position.z);
            if (startsGoingOverEdge)
            {
				/* tileSpillSequence.Append(
					 tileToMoveTransform.DOJump(eitherEdgeTargetLocs[0],
													   jumpHeight, 1, tileSpillIndivDuration / 3f))
								  .Append(tileToMoveTransform.DOJump(eitherEdgeTargetLocs[1],
												 -2, 1, tileSpillIndivDuration / 3f))
								  .Append(tileToMoveTransform.DOJump(targetLocation,
												 jumpHeight, 1, tileSpillIndivDuration / 3f));*/

				tileSpillSequence.Append(
					tileToMove.transform.DOMove(eitherEdgeTargetLocs[0], tileSpillIndivDuration / 2f))
                                 .Append(tileToMove.transform.DOMove(midTargetLoc,tileSpillIndivDuration / 2f))
								 .Append(tileToMove.transform.DOMove(eitherEdgeTargetLocs[1], tileSpillIndivDuration / 2f))
								 .Append(tileToMove.transform.DOMove(targetLocation,tileSpillIndivDuration / 2f));
            }
            else
            {
                //jumping animation
                tileSpillSequence.Append(
                    tileToMove.transform.DOJump(new Vector3(
                        spaceToSpillOnto.transform.position.x,
                        spaceToSpillOnto.provisionalTileCount * 0.2f + 0.1f,
                        spaceToSpillOnto.transform.position.z), jumpHeight,
                                                1, tileSpillIndivDuration, false)
                );

                //rotation animation
                Vector3 rotateTileVector = new Vector3(0, 0, 0);
                if (xDirection == 0 && zDirection == 1)
                { //up
                    rotateTileVector = new Vector3(180, 0, 0);
                }
                else if (xDirection == 0 && zDirection == -1)
                { // down
                    rotateTileVector = new Vector3(-180, 0, 0);
                }
                else if (xDirection == -1 && zDirection == 0)
                { // left
                    rotateTileVector = new Vector3(0, 0, 180);
                }
                else if (xDirection == 1 && zDirection == 0)
                { // right
                    rotateTileVector = new Vector3(0, 0, -180);
                }
                tileToMove.transform.localRotation = Quaternion.identity;

                tileSpillSequence.Join(tileToMove.transform.DORotate(rotateTileVector, tileSpillIndivDuration, RotateMode.LocalAxisAdd));
            }

            spaceToSpillOnto.AddTile(tileToMove, false); //officially add to the space

        }
        if (!boardFalling){
            tileSpillSequence.OnComplete(OnCompleteSpillAnimation);
        }

        if(lastTileInBoardFall){
            tileSpillSequence.OnComplete(OnCompleteSpillAnimationForLastTile);
        }

		tileSpillSequence.Play();

    }


    public void QueueSpillAction()
    {
        QueueSpillHelper(spaceToSpill, spillDirectionX, spillDirectionZ);
    }

    private void OnCompleteSpillAnimation(){
        Services.Main.ConfirmUndoUI.SetActive(true);
	}

    private void OnCompleteSpillAnimationForLastTile(){
        boardCollapseSequence.Play();
		Services.GameManager.currentCamera.DOShakePosition(0.3f, 0.5f, 20, 90, true);
     //   boardFinishedFalling = true;
    }


    public void BoardFallAction()
    {
        //Services.GameManager.currentCamera.GetComponent<CameraManager>().DoShake();

        List<BoardSpace> spacesToCollapse = GetSpaceListFromSideNum();

        int[] coords = GetDirectionFromSideNum();
        int xDirection = coords[0];
        int zDirection = coords[1];

        if ((sideAboutToCollapse % 2) == 0)
        {
            currentNumCols -= 1;
            if (sideAboutToCollapse == 0)
            {
                currentLowestColIndex += 1;
            }
            else
            {
                currentHighestColIndex -= 1;
            }
        }
        else
        {
            currentNumRows -= 1;
            if (sideAboutToCollapse == 3)
            {
                currentLowestRowIndex += 1;
            }
            else
            {
                currentHighestRowIndex -= 1;
            }
        }
        highestStackIndexInSpacesToCollapse = 0;
        for (int i = 1; i < spacesToCollapse.Count; ++i)
        {
            if (spacesToCollapse[highestStackIndexInSpacesToCollapse].tileStack.Count < spacesToCollapse[i].tileStack.Count)
                highestStackIndexInSpacesToCollapse = i;
        }


		boardCollapseSequence = DOTween.Sequence();
		for (int b = 0; b < spacesToCollapse.Count; ++b)
		{
			if (b > 0)
			{
				boardCollapseSequence.Insert(b * 0.2f, spacesToCollapse[b].transform.DOMoveY(-6f, 0.8f));
			}
			else
			{
				boardCollapseSequence.Append(spacesToCollapse[b].transform.DOMoveY(-6f, 0.8f));
			}
		}

        for (int s = 0; s < spacesToCollapse.Count; ++s){
            if(s == highestStackIndexInSpacesToCollapse){
                lastTileInBoardFall = true;
            } else {
                lastTileInBoardFall = false;
            }
            QueueSpillHelper(spacesToCollapse[s], xDirection, zDirection);
        }


        boardCollapseSequence.OnComplete(() => OnCompleteBoardFall(spacesToCollapse));
        boardCollapseSequence.Pause();
        sideAboutToCollapse = (sideAboutToCollapse + 1) % 4;
        numSidesCollapsed++;

    }

    private void OnCompleteBoardFall(List<BoardSpace> spaces){
		for (int j = spaces.Count - 1; j >= 0; --j)
		{
			if (spaces[j] != null)
			{
				Object.Destroy(spaces[j].gameObject);
			}
		}
        boardFinishedFalling = true;
    }

   // private void 

    public void CheckScoreAction(){
       // Debug.Log("Enter CheckScoreAction");
        //perhaps check score every time a new tile is placed in the center, to reward tall stacks.
        for (int i = 0; i < centerSpaces.Count; ++i){
            if (centerSpaces[i].tileStack.Count > 0)
            {
                centerSpaces[i].centerColor = centerSpaces[i].tileStack[centerSpaces[i].tileStack.Count - 1].color;
                centerSpaces[i].GetComponent<MeshRenderer>().material = Services.Materials.TileMats[centerSpaces[i].tileStack[centerSpaces[i].tileStack.Count - 1].color];
                //Object.Destroy(centerSpaces[i].tileStack[centerSpaces[i].tileStack.Count - 1].gameObject);
 
                for (int j = centerSpaces[i].tileStack.Count - 1; j >= 0; --j){
                    // Object.Destroy(centerSpaces[i].tileStack[j].gameObject);
                    centerSpaces[i].tileStack[j].transform.DOMoveY(0, 0.3f);
                    centerSpaces[i].tileStack[j].transform.DOScaleY(0, 0.3f);
                    centerSpaces[i].tileStack[j].gameObject.layer = LayerMask.NameToLayer("Default");
                }
                centerSpaces[i].tileStack.Clear();
                centerSpaces[i].provisionalTileCount = centerSpaces[i].tileStack.Count;
                centerSpaceChanged = true;

            }
        }


        if (centerSpaceChanged)
        {
            if (!scoreAnimationStarted)
            {
                //Debug.Log("enter centerSpaceChanged");
                bool colorred = false;
                bool colorblue = false;
                bool coloryellow = false;
                bool colorgreen = false;
                foreach (BoardSpace bs in centerSpaces)
                {
                    int color = bs.centerColor;
                    switch (color)
                    {
                        case 0:
                            colorred = true;
                            break;
                        case 1:
                            colorgreen = true;
                            break;
                        case 2:
                            colorblue = true;
                            break;
                        case 3:
                            coloryellow = true;
                            break;
                    }

                }

                Sequence scoringSequence = DOTween.Sequence();
                if (colorred && colorblue && coloryellow && colorgreen)
                {
                    //Debug.Log("enter scoring");
                    scoringSequence.AppendInterval(0.5f);
                    for (int i = 0; i < centerSpaces.Count; ++i)
                    {
                        scoringSequence.Append(centerSpaces[i].transform.DOPunchScale(new Vector3(0, Random.Range(80,120), 0), 0.35f, 1, 1));
                    }
                    scoringSequence.AppendInterval(0.6f); //score wait time
                    scoringSequence.OnComplete(OnCompleteScoringSequence);
                    scoringSequence.Play();
                    scoreAnimationStarted = true;
                    score += 1;
                    Services.Main.Score.text = "SCORE: " + score;
                }
                else
                {
                   // Debug.Log("enter not scoring");
                    finishedCheckingScore = true;
                    centerSpaceChanged = false;
                }
            }

        } else {
          //  Debug.Log("enter center space not changed");
            finishedCheckingScore = true;
        }
    }

    private void OnCompleteScoringSequence(){
        finishedCheckingScore = true;
        scoreAnimationStarted = false;
		centerSpaceChanged = false;
    }

    public void ToggleTileGlow(List<Tile> tiles, Brightness brightness){
        switch(brightness){
            case Brightness.Normal:
                foreach (Tile tile in tiles)
                {
                    if (tile != null)
                    {
                        tile.transform.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[0];
                    }
                }
                break;
            case Brightness.Bright:
                foreach(Tile tile in tiles){
                    if (tile != null)
                    {
                        tile.transform.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[1];
                    }
                }
                break;
            case Brightness.Dark:
				foreach (Tile tile in tiles)
				{
                    if (tile != null)
                    {
                        tile.transform.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[2];
                    }
                }
                break;
        }
    }

    public void ToggleTileGlow(Tile tile, Brightness brightness){
		List<Tile> tiles = new List<Tile>();
		tiles.Add(tile);
		ToggleTileGlow(tiles, brightness);
    }

    public void ToggleSpaceGlow(BoardSpace space, Brightness brightness){
        //if(!space.aboutToCollapse){
			switch (brightness)
			{
				case Brightness.Normal:
                    space.transform.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[0];
					break;
				case Brightness.Bright:
                    space.transform.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[1];
					break;
				case Brightness.Dark:
                    space.transform.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[2];
					break;
			}
            ToggleTileGlow(space.tileStack,brightness);
      //  }
    }

	public void ToggleRendererGlow(Renderer renderer, Brightness brightness)
	{
		switch (brightness)
		{
			case Brightness.Normal:
				renderer.material.shader = Services.Materials.HighlightShaders[0];
				break;
			case Brightness.Bright:
                renderer.material.shader = Services.Materials.HighlightShaders[1];
				break;
			case Brightness.Dark:
                renderer.material.shader = Services.Materials.HighlightShaders[2];
				break;
		}
		
	}

	public void ToggleGameObjGlow(GameObject obj, Brightness brightness)
	{
		switch (brightness)
		{
			case Brightness.Normal:
				obj.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[0];
				break;
			case Brightness.Bright:
				obj.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[1];
				break;
			case Brightness.Dark:
				obj.GetComponent<Renderer>().material.shader = Services.Materials.HighlightShaders[2];
				break;
		}

	}
	void SetSpaceGlow(Brightness brightness)
	{
		foreach (BoardSpace space in board)
		{
			if (space != null)
			{
                if (!space.isCenterSpace && (space.tileStack.Count < 1))
				{
					ToggleSpaceGlow(space, brightness);
				}
			}
		}
	}

    void SetIneligibleSpaceGlow(Brightness brightness){
        foreach(BoardSpace space in board){
            if (space != null){
                if(!space.isCenterSpace && (space.tileStack.Count < 2)){
                    ToggleSpaceGlow(space, brightness);
                }
            }
        }
    }

    public void HighlightSpillArrow(Ray ray){
        if (spillArrowRenderers != null)
        {
            foreach (Renderer arrowRenderer in spillArrowRenderers)
            {
                if (arrowRenderer != null)
                {
                    arrowRenderer.material.shader = Services.Materials.ArrowShaders[0];
                }
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.Main.spillUILayer))
            {
                hit.collider.gameObject.GetComponent<Renderer>().material.shader = Services.Materials.ArrowShaders[1];
               // ToggleGameObjGlow(hit.collider.gameObject, Brightness.Bright);
            }
        }
    }

	public bool GameOverCheck()
	{
        if (numSidesCollapsed == (4 * ((numCols * 2) / 4 - 1)))
		{
            return true;
		}
        return false;
	}



	private class Turn : FSM<BoardManager>.State { }

    private class EnterBoard : Turn{
        public override void OnEnter(){
           // Debug.Log("EnterBoard");
            Context.boardFinishedEntering = false;
			Context.AnimateEnterBoard();
        }
        public override void Update(){
            if (Context.boardFinishedEntering)
            {
                TransitionTo<SpawnTile>();
                return;
            }
        }

    }

	private class SpawnTile : Turn
	{
		public override void OnEnter()
		{
           // Debug.Log("SpawnTile");
            Context.boardFinishedFalling = false;
            Context.lastTileInBoardFall = false;
            Context.boardFalling = false;
            Services.Main.ConfirmUndoUI.SetActive(false);
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
            //Debug.Log("SelectTile");
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
          //  Debug.Log("PlaceTile");
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
            Services.Main.ConfirmUndoUI.SetActive(false);
           // Debug.Log("SelectStack");
            if (Context.undoSpill)
            {
				Context.undoSpill = false;
				Context.startSpill = false;
            }
            else
            {
                Context.undoSpill = false;
                Context.stackSelected = false;
                Context.startSpill = false;
            }
		}
		public override void Update()
		{
            if (!Context.startSpill)
            {
                Context.SelectStackAction();
            } else{
                TransitionTo<QueueSpill>();
                return;
            }
		}
	}

	private class QueueSpill : Turn
	{
		public override void OnEnter()
		{
            
           // Debug.Log("QueueSpill");
			//Services.Main.ConfirmUndoUI.SetActive(true); //insert this line OnComplete spill animation
            Context.finalizeSpill = false;
            Context.QueueSpillAction();
		}
		public override void Update()
		{
            if(Context.undoSpill){
                TransitionTo<SelectStack>();
                return;
            }
            if (Context.finalizeSpill){

                Context.CheckScoreAction();
                if (Context.finishedCheckingScore)
                {
                    Context.finishedCheckingScore = false;
                    TransitionTo<BoardFall>();
                    return;
                }
            } 
		}
	}


    private class BoardFall : Turn //interim
    {
        public override void OnEnter()
        {
          //  Debug.Log("BoardFall");
            Context.boardFalling = true;
            Services.Main.ConfirmUndoUI.SetActive(false);
            Context.BoardFallAction();

            //Context. ___
        }
        public override void Update()
        {
            if (Context.boardFinishedFalling)
            {

				Context.CheckScoreAction();
                if (Context.finishedCheckingScore)
                {
                    Context.finishedCheckingScore = false;
                    if (Context.GameOverCheck())
                    {
                        TransitionTo<GameOver>();
                        return;
                    }
                    else
                    {
                        TransitionTo<SpawnTile>();
                        return;
                    }
                }

            }
        }

	}


	private class GameOver : Turn
	{
		public override void OnEnter()
		{
            Services.Main.GameOverText.SetActive(true);
			//Context. ___
		}
		public override void Update()
		{

		}
	}
}
