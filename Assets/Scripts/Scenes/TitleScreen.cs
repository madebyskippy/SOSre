using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : Scene<TransitionData> {

	void Start()
	{

        Services.BoardData.InitializeBoardData();
       // Time.timeScale = 1;
	}

	// Update is called once per frame
	void Update()
	{

    }
    void InitializeServices()
    {
        Services.TitleScreen = this;
    }

    internal override void OnEnter(TransitionData data)
    {
        InitializeServices();
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();

    }


	public void StartLevelGame()
	{
        Services.BoardData.randomTiles = false;
		Services.SceneStackManager.Swap<LevelSelect>();
	}

    public void StartLevelEditor(){
        Services.SceneStackManager.Swap<LevelEditor>();
    }

    public void StartTutorial(){
        Services.SceneStackManager.Swap<Tutorial>();
    }

    public void StartRandomGame(){
        Services.BoardData.randomTiles = true;
        Services.SceneStackManager.Swap<Main>();
    }
}
