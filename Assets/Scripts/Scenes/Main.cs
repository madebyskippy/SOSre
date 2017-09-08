using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : Scene<TransitionData> {

	// Use this for initialization
	void Start () {
        Services.BoardManager.GenerateBoard(Services.BoardData.numRows, Services.BoardData.numCols);
	}
	
	// Update is called once per frame
	void Update () {
		
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
}
