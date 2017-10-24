using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
       /* tileToPosition.transform.DOMove(new Vector3(transform.position.x,
                                                    provisionalTileCount * 0.2f + 0.1f,
                                                    transform.position.z),
                                       0.5f);*/
        //provisionalTileCount += 1;
	}

    public void AddTile(Tile tileToAdd, bool positionTile)
    {
        if (tileStack.Count > 0)
        {
            tileStack[tileStack.Count - 1].gameObject.layer = LayerMask.NameToLayer("Default");

        }
        if (positionTile)
        {
            tileToAdd.transform.position = new Vector3(transform.position.x, provisionalTileCount * 0.2f + 0.1f, transform.position.z);
            //tileToAdd.transform.DOMove(new Vector3(transform.position.x, provisionalTileCount * 0.2f + 0.1f, transform.position.z), 0.5f);

        }

        provisionalTileCount += 1;
        tileStack.Add(tileToAdd);
        tileToAdd.gameObject.layer = LayerMask.NameToLayer("TopTiles");

    }

	public void ResetTilesToPosition()
	{
        tileStack.Reverse();
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
