using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public GameObject sceneRoot;
    public Camera currentCamera;
	void Awake()
	{
		InitializeServices();
	}

	// Use this for initialization
	void Start()
	{
		Services.EventManager.Register<Reset>(Reset);
		Services.SceneStackManager.PushScene<TitleScreen>();
	}

	// Update is called once per frame
	void Update()
	{
		Services.TaskManager.Update();
	}

	void InitializeServices()
	{
		Services.GameManager = this;
		Services.EventManager = new EventManager();
		Services.TaskManager = new TaskManager();
		Services.Prefabs = Resources.Load<PrefabDB>("Prefabs/Prefabs");
        Services.Materials = Resources.Load<MaterialDB>("Art/Materials");
		Services.SceneStackManager = new SceneStackManager<TransitionData>(sceneRoot, Services.Prefabs.Scenes);
		Services.InputManager = new InputManager();

        Services.BoardData = new BoardData();
        Services.BoardManager = new BoardManager();
        Services.TurnManager = new TurnManager();
	}

	void Reset(Reset e)
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

    //UI buttons

}
