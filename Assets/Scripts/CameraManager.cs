using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public bool Shaking;
    private float ShakeDecay;
    private float ShakeIntensity;
    private Vector3 OriginalPos;
    private Quaternion OriginalRot;

	// Use this for initialization
	void Start () {
		Shaking = false;
        switch (Services.BoardData.numRows){
            case 4:
                GetComponent<Camera>().orthographicSize = 2;
                break;
            case 6:
				GetComponent<Camera>().orthographicSize = 3;
                break;
            case 8:
				GetComponent<Camera>().orthographicSize = 4;
                break;

        }
	}

	// Update is called once per frame
	void Update()
	{
		if (ShakeIntensity > 0)
		{
			transform.position = OriginalPos + Random.insideUnitSphere * ShakeIntensity;
			transform.rotation = new Quaternion(OriginalRot.x + Random.Range(-ShakeIntensity, ShakeIntensity) * .2f,
									  OriginalRot.y + Random.Range(-ShakeIntensity, ShakeIntensity) * .2f,
									  OriginalRot.z + Random.Range(-ShakeIntensity, ShakeIntensity) * .2f,
									  OriginalRot.w + Random.Range(-ShakeIntensity, ShakeIntensity) * .2f);

			ShakeIntensity -= ShakeDecay;
		}
		else if (Shaking)
		{
			Shaking = false;
		}
	}

	public void DoShake()
	{
		OriginalPos = transform.position;
		OriginalRot = transform.rotation;

		ShakeIntensity = 0.1f;
		ShakeDecay = 0.002f;
		Shaking = true;
	}
}
