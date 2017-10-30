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

    public GameObject Previews;
    public GameObject ConfirmUndoUI;
    public Text Score;
    public GameObject GameOverText;
    public GameObject PauseScreen;

    public GameObject HighlightCenter;

    public SpriteRenderer gradient;

	// Use this for initialization
	void Start () {

       // Time.timeScale = 1;

	}
	
	// Update is called once per frame
	void Update () {
        Services.BoardManager.Update();
	}

	void InitializeServices()
	{
		Services.Main = this;
        Services.BoardData.InitializeBoardData();
        Services.BoardManager.InitializeBoard();
	}

	internal override void OnEnter(TransitionData data)
	{
		InitializeServices();
		Services.GameManager.currentCamera = GetComponentInChildren<Camera>();

	}

    public void ConfirmButton(){
        Services.BoardManager.ConfirmSpill();
    }

    public void UndoButton(){
        Services.BoardManager.UndoSpill();
    }

    public void Pause(){
        Time.timeScale = 0;
        PauseScreen.SetActive(true);

    }

    public void Resume(){
        PauseScreen.SetActive(false);
        Time.timeScale = 1;
    }

    public void Restart(){
        // Services.SceneStackManager.Swap<Main>();
        Time.timeScale = 1;
        Services.SceneStackManager.PopScene();
        Services.SceneStackManager.PushScene<Main>();
    }

    public void MainMenu(){

        Services.SceneStackManager.Swap<TitleScreen>();

    }
}
