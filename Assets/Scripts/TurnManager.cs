using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager {

  /*  private FSM<TurnManager> fsm;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SpawnTileAction(){
		DrawTileToPlace();
		if (mode != "Game Over")
		{
			mode = "Select Tile";
			if (!collapseSideIndicated && firstTileFinalized)
			{
				List<BoardSpace> boardspaces = boardManager.GetSpaceListFromSideNum();
				foreach (BoardSpace bs in boardspaces)
				{
					bs.gameObject.GetComponent<Renderer>().material = boardManager.mats[7];
					bs.aboutToCollapse = true;
				}
				collapseSideIndicated = true;
			}

    }

    private class Turn : FSM<TurnManager>.State{}

    private class SpawnTile : Turn {
        public override void OnEnter(){
            //Context. ___
        }
        public override void Update(){
            
        }
    }

    private class SelectTile : Turn {
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
    }

    private class PlaceTile : Turn {
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
    }

    private class SelectStack : Turn {
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
    }

    private class QueueSpill : Turn {
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
    }

    private class Interim : Turn {
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

    private class GameOver : Turn{
		public override void OnEnter()
		{
			//Context. ___
		}
		public override void Update()
		{

		}
    }*/
}
