using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject {
    [SerializeField]
    private GameObject[] scenes;
    public GameObject[] Scenes { get { return scenes; } }

    [SerializeField]
    private GameObject tile;
    public GameObject Tile { get { return tile; }}

    [SerializeField]
    private GameObject boardspace;
    public GameObject BoardSpace { get { return boardspace; }}

	[SerializeField]
	private GameObject spillUI;
	public GameObject SpillUI { get { return spillUI; } }

    [SerializeField]
    private GameObject scoreImg;
    public GameObject ScoreImg { get { return scoreImg; }}

    [SerializeField]
    private GameObject finalScoreImg;
    public GameObject FinalScoreImg { get { return finalScoreImg; }}
}
