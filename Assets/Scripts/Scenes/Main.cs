using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : Scene<TransitionData> {

	public LayerMask spawnedTileLayer;
	public LayerMask topTileLayer;
	public LayerMask invisPlane;
    public LayerMask spillUILayer;
    public LayerMask topTilesAndSpillUI;

    public GameObject ConfirmUndoUI;
    public Text Score;
    public GameObject GameOverText;

	// Use this for initialization
	void Start () {

		Services.BoardData.InitializeBoardData();
        Services.BoardManager.InitializeBoard();
	}
	
	// Update is called once per frame
	void Update () {
        Services.BoardManager.Update();
	}

	void InitializeServices()
	{
		Services.Main = this;
	}

	internal override void OnEnter(TransitionData data)
	{
		InitializeServices();
		Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
	}

    public void ConfirmButton(){
        Services.BoardManager.ConfirmSpill();
    }
}
