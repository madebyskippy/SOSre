using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelect : Scene<TransitionData>
{

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    void InitializeServices()
    {
        Services.LevelSelect = this;
    }

    internal override void OnEnter(TransitionData data)
    {
        InitializeServices();
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();

    }

    public void StartLevel(int n)
    {
        if (n == 0)
        {
            Services.BoardData.levelNum = -1;
            Services.BoardData.levelName = "test";
        }
        else
        {
            Services.BoardData.levelNum = n;
            Services.BoardData.levelName = "level" + n;
        }

        Services.SceneStackManager.Swap<Main>();
    }

}
