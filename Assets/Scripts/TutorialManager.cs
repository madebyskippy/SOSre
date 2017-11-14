using UnityEngine;
using System.Collections;

public class TutorialManager: MonoBehaviour {

	public GameObject[] arrows;

	public GameObject[] boards;

	public AudioSource blip;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void clickArrowSpawn2(){
		blip.Play ();
		arrows [0].SetActive (false);
		arrows[1].SetActive(true);
		boards [1].SetActive (true);
	}

	public void clickArrowSpawn3(){
		blip.Play ();
		arrows [1].SetActive (false);
		arrows[2].SetActive(true);
		boards [2].SetActive (true);
	}

	public void clickArrowSpawn4(){
		blip.Play ();
		arrows [2].SetActive (false);
		//arrows[3].SetActive(true);
		boards [3].SetActive (true);
	}
}
