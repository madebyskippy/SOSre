using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PivotController : MonoBehaviour {

    private Vector3 currentRotation;

    private bool spinning;
	// Use this for initialization
	void Start () {
        currentRotation = Vector3.zero;
        spinning = false;
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void RotateLeft(){
        if (!spinning)
        {
            Services.Main.audioController.rotatecamera.PlayOneShot(Services.Main.audioController.rotatecamera.clip,1f);
            spinning = true;
            //currentRotation += Vector3.up * 90f;
            transform.DORotate(Vector3.up * 90f, 0.6f, RotateMode.WorldAxisAdd)
                     .OnComplete(() => spinning = false);
            
        }

    }
    public void RotateRight(){
        if (!spinning)
        {
            Services.Main.audioController.rotatecamera.PlayOneShot(Services.Main.audioController.rotatecamera.clip, 1f);
            spinning = true;
            //currentRotation += Vector3.down * 90f;
            transform.DORotate(Vector3.down * 90f, 0.6f, RotateMode.WorldAxisAdd)
                     .OnComplete(() => spinning = false);
        }
    }

}
