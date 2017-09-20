using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace : MonoBehaviour {

    public int color;
    public int centerColor; // -1 is default, 0-3 is rgby
    public int colNum, rowNum;
    public bool isCenterSpace;
    public bool aboutToCollapse;

    public List<Tile> tileStack;
    public int provisionalTileCount;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetBoardSpace(int c, int col, int row){
        color = c;
        centerColor = -1;
        colNum = col;
        rowNum = row;
    }

	public void PositionNewTile(Tile tileToPosition)
	{
		//juicy.AnimateTileMove(tileToPosition, provisionalTileCount, transform.position);
        tileToPosition.transform.position = new Vector3(transform.position.x, provisionalTileCount * 0.2f + 0.1f, transform.position.z);
		//provisionalTileCount += 1;
	}

    public void AddTile(Tile tileToAdd, bool positionTile){
		if (!isCenterSpace)
		{

           // Debug.Log("before adding: "+colNum + ", " + rowNum + ": " + provisionalTileCount);
			if (tileStack.Count > 0)
			{
				tileStack[tileStack.Count - 1].gameObject.layer = LayerMask.NameToLayer("Default");

			}
			if (positionTile)
			{
				tileToAdd.transform.position = new Vector3(transform.position.x, provisionalTileCount * 0.2f + 0.1f, transform.position.z);
			}

			provisionalTileCount += 1;
			tileStack.Add(tileToAdd);
			tileToAdd.gameObject.layer = LayerMask.NameToLayer("TopTiles");
            //Debug.Log(colNum + ", " + rowNum + ": "+provisionalTileCount);
		}
		else
		{
            //color = tileToAdd.color;
            centerColor = tileToAdd.color;
            //GameObject.FindWithTag("TurnManager").GetComponent<TurnManager>().scoringMode = true;
            GetComponent<MeshRenderer>().material = Services.Materials.TileMats[tileToAdd.color];
			Destroy (tileToAdd.gameObject);
			//juicy.TileSinkAnimation(tileToAdd.gameObject, transform.gameObject);
            Services.BoardManager.centerSpaceChanged = true;

		}
    }

	public void ResetTilesToPosition()
	{
		for (int i = 0; i < tileStack.Count; i++)
        {
			Tile tile = tileStack[i];
            if (tile.gameObject != null)
            {
                //iTween.Stop(tile.gameObject);
                tile.transform.rotation = Quaternion.Euler(Vector3.zero);
                tile.transform.position = new Vector3(transform.position.x, i * 0.2f + 0.1f, transform.position.z);
                if (i == tileStack.Count - 1)
                {
                    tile.gameObject.layer = LayerMask.NameToLayer("TopTiles");
                }
                else
                {
                    tile.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
		}
	}

}
