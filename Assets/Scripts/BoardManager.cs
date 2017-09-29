using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

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

    public Tile spawnedTile;
    public Tile selectedTile;
    public List<Tile> tilesQueuedToSpill;
    public BoardSpace spaceQueuedToSpillFrom;

    private BoardSpace selectedSpace;
    public BoardSpace spaceToSpill;

    public int numSidesCollapsed;
    private int highestStackIndexInSpacesToCollapse;

    private int spillDirectionX, spillDirectionZ;

    public bool stackSelected;
    public bool startSpill;
    public bool finalizeSpill;
    public bool boardFalling;
    public bool lastTileInBoardFall;
    public bool boardFinishedFalling;

    public bool tileInPosition;
    public int sideAboutToCollapse;

    public BoardSpace highlightedSpace;

    private Sequence tileFloatSequence;




    public enum Brightness {Bright,Dark,Normal}

    public void Update()
    {
        fsm.Update();
    }

    public void InitializeBoard()
    {
        /*  spawnedTileLayer = LayerMask.NameToLayer("DrawnTile");
          topTileLayer = LayerMask.NameToLayer("TopTiles");
          invisPlane = LayerMask.NameToLayer("InvisBoardPlane");*/

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

        if (Services.BoardData.randomTiles)
        {
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

        }
        else
        {


        }



        fsm = new FSM<BoardManager>(this);
        fsm.TransitionTo<SpawnTile>();

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
        // tileToPlace.transform.localPosition = new Vector3(-5, 0, 0);
        tileToPlace.transform.position = new Vector3(-10, 0, 0);
        tileToPlace.transform.DOMove(new Vector3(-5, 0, 0), 0.5f).SetEase(Ease.OutBounce).OnComplete(PlayFloatSequence);
        tileToPlace.gameObject.layer = LayerMask.NameToLayer("DrawnTile");
		//juicyManager.spawnTileAnimation(tileToPlace.gameObject);


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

        int maxStackHeight = 1;

        for (int i = 0; i < numTilesToMove; i++)
        {
            //toBeSpilled.provisionalTileCount = 0;
            int index = numTilesToMove - 1 - i;
            Tile tileToMove = toBeSpilled.tileStack[index];
            tilesQueuedToSpill.Add(tileToMove);
            int[] targetCoords = CalculateAdjacentSpace(boardSpaceX, boardSpaceZ, xDirection, zDirection);
            boardSpaceX = targetCoords[0];
            boardSpaceZ = targetCoords[1];
            BoardSpace spaceToSpillOnto = board[boardSpaceX, boardSpaceZ];
            tileToMove.spaceQueuedToSpillOnto = spaceToSpillOnto;

            //toBeSpilled.PositionNewTile(tileToMove);
            toBeSpilled.tileStack.Remove(tileToMove);

            // ANIMATE SPILL

            if (spaceToSpillOnto.tileStack.Count > maxStackHeight)
            {
                maxStackHeight = spaceToSpillOnto.tileStack.Count;
            }
            float jumpHeight = maxStackHeight;
            if(maxStackHeight > 1){
                jumpHeight *= 0.65f;
            }
            tileSpillSequence.Append(
                tileToMove.transform.DOJump(new Vector3(
                    spaceToSpillOnto.transform.position.x,
                    spaceToSpillOnto.provisionalTileCount * 0.2f + 0.1f,
                    spaceToSpillOnto.transform.position.z), jumpHeight,
                                            1, tileSpillIndivDuration, false)
            );

            Vector3 rotateTileVector = new Vector3(0, 0, 0);
            if(xDirection == 0 && zDirection == 1){ //up
                rotateTileVector = new Vector3(180, 0, 0);
            } else if(xDirection == 0 && zDirection == -1){ // down
                rotateTileVector = new Vector3(-180, 0, 0);
			} else if (xDirection == -1 && zDirection == 0){ // left
				rotateTileVector = new Vector3(0, 0, 180);
			} else if (xDirection == 1 && zDirection == 0){ // right
				rotateTileVector = new Vector3(0, 0, -180);
			}
            tileToMove.transform.localRotation = Quaternion.identity;

            tileSpillSequence.Insert(tileSpillIndivDuration*i,
                                     tileToMove.transform.DORotate(rotateTileVector, tileSpillIndivDuration, RotateMode.LocalAxisAdd));


            spaceToSpillOnto.AddTile(tileToMove, false);

        }
        if (!boardFalling)
        {
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
        boardFinishedFalling = true;
    }


    public void BoardFallAction()
    {
        //Services.GameManager.currentCamera.GetComponent<CameraManager>().DoShake();
        Services.GameManager.currentCamera.DOShakePosition(0.3f, 0.5f,13,90,true);

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

        for (int s = 0; s < spacesToCollapse.Count; ++s){
            if(s == highestStackIndexInSpacesToCollapse){
                lastTileInBoardFall = true;
            } else {
                lastTileInBoardFall = false;
            }
            QueueSpillHelper(spacesToCollapse[s], xDirection, zDirection);
        }

        Sequence boardCollapseSequence = DOTween.Sequence();
        for (int b = 0; b < spacesToCollapse.Count; ++b){
            if (b > 0)
            {
                boardCollapseSequence.Insert(b*0.2f,spacesToCollapse[b].transform.DOMoveY(-10f, 1f));
            }
            else
            {
                boardCollapseSequence.Append(spacesToCollapse[b].transform.DOMoveY(-10f, 1f));
            }
        }

        boardCollapseSequence.OnComplete(() => OnCompleteBoardFall(spacesToCollapse));

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
    }

    public void CheckScoreAction(){
        //perhaps check score every time a new tile is placed in the center, to reward tall stacks.

        for (int i = 0; i < centerSpaces.Count; ++i){
            if (centerSpaces[i].tileStack.Count > 0)
            {
                centerSpaces[i].centerColor = centerSpaces[i].tileStack[centerSpaces[i].tileStack.Count - 1].color;
                centerSpaces[i].GetComponent<MeshRenderer>().material = Services.Materials.TileMats[centerSpaces[i].tileStack[centerSpaces[i].tileStack.Count - 1].color];
                //Object.Destroy(centerSpaces[i].tileStack[centerSpaces[i].tileStack.Count - 1].gameObject);
 
                for (int j = centerSpaces[i].tileStack.Count - 1; j >= 0; --j){
                    Object.Destroy(centerSpaces[i].tileStack[j].gameObject);
                }
                centerSpaces[i].tileStack.Clear();
                centerSpaces[i].provisionalTileCount = centerSpaces[i].tileStack.Count;
                centerSpaceChanged = true;

            }
        }


        if (centerSpaceChanged)
        {
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
            if (colorred && colorblue && coloryellow && colorgreen)
            {
                score += 1;
                //scoring = true;
                //juicy.ScoreAnimation();
                /*GameObject pre = Instantiate(scorePrefab,
                    new Vector3(scorePrefab.transform.position.x + 45f * (score - 1), scorePrefab.transform.position.y, scorePrefab.transform.position.z),
                    Quaternion.identity) as GameObject;
                pre.transform.SetParent(GameObject.FindWithTag("ScoreSymbolsGroup").transform, false);
                pre.GetComponent<Animator>().SetTrigger("actualEntry");*/
                Services.Main.Score.text = "SCORE: " + score;
            }

            centerSpaceChanged = false;
        }
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
                    ToggleRendererGlow(arrowRenderer, Brightness.Dark);
                }
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Services.Main.spillUILayer))
            {
                ToggleGameObjGlow(hit.collider.gameObject, Brightness.Bright);
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

	private class SpawnTile : Turn
	{
		public override void OnEnter()
		{
            Debug.Log("SpawnTile");
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
            Debug.Log("SelectTile");
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
            Debug.Log("PlaceTile");
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
            Debug.Log("SelectStack");
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
            
            Debug.Log("QueueSpill");
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
                //Context.CheckScoreAction();
                Context.CheckScoreAction();
				TransitionTo<BoardFall>();
				return;
            } else{
            }
		}
	}

    private class BoardFall : Turn //interim
    {
        public override void OnEnter()
        {
            Debug.Log("BoardFall");
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
