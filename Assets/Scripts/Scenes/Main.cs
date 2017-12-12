using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;


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
    public GameObject GameOverScoreText;
    public GameObject PauseScreen;

    public Text objective;

    public GameObject HighlightCenter;

    public SpriteRenderer gradient;

    public AudioController audioController;

    public GameObject levelEndButtons;

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
        Services.BoardManager.InitializeBoard();
	}

	internal override void OnEnter(TransitionData data)
	{
		InitializeServices();
		Services.GameManager.currentCamera = GetComponentInChildren<Camera>();

	}

    public void ConfirmButton(){
        audioController.confirm.PlayOneShot(audioController.confirm.clip, 1f);
        Services.BoardManager.ConfirmSpill();
    }

    public void UndoButton(){
        Services.Main.audioController.select.Play();
        Services.BoardManager.UndoSpill();
    }

    public void Pause(){
        Services.Main.audioController.select.Play();
        Time.timeScale = 0;
        PauseScreen.SetActive(true);
        Services.GameManager.currentCamera.GetComponent<BlurOptimized>().enabled = true;

    }

    public void Resume(){

        Services.Main.audioController.select.Play();
        PauseScreen.SetActive(false);
        Time.timeScale = 1;
        Services.GameManager.currentCamera.GetComponent<BlurOptimized>().enabled = false;
    }

    public void Restart(){
        // Services.SceneStackManager.Swap<Main>();

        Services.Main.audioController.select.Play();
        Time.timeScale = 1;
        Services.SceneStackManager.PopScene();
        Services.SceneStackManager.PushScene<Main>();
    }

    public void MainMenu(){

        Services.Main.audioController.select.Play();
        Services.SceneStackManager.Swap<TitleScreen>();

    }

    public void NextLevel(){
        //Services.BoardData.levelName = "level" + n;
        Services.BoardData.levelNum++;
        Services.BoardData.levelName = "level" + Services.BoardData.levelNum;
        Services.SceneStackManager.PopScene();
        Services.SceneStackManager.PushScene<Main>();
    }
}
