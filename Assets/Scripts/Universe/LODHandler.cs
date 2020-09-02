using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LODHandler : MonoBehaviour {
	[Header ("LOD screen heights")]
	// LOD level is determined by body's screen height (1 = taking up entire screen, 0 = teeny weeny speck) 
	public float lod1Threshold = .5f;
	public float lod2Threshold = .2f;

	[Header ("Debug")]
	public bool debug;
	public CelestialBody debugBody;

	Camera cam;
	Transform camT;
	CelestialBody[] bodies;
	LODSpheresGenerator[] generators;

    bool hasBeenInitialized;

    void Awake()
    {
        hasBeenInitialized = false;
    }

	public void InitLODHandler () {
		if (Application.isPlaying) {
			bodies = FindObjectsOfType<CelestialBody>();
			generators = new LODSpheresGenerator[bodies.Length];
			for (int i = 0; i < generators.Length; i++) {
				generators[i] = bodies[i].GetComponentInChildren<LODSpheresGenerator>();
                Debug.Log("generator from Start = " + generators[i]);
			}
		}
        hasBeenInitialized = true;
	}

	void Update () {
        if(!hasBeenInitialized)
            return;

		DebugLODInfo();
		if (Application.isPlaying) {
			HandleLODs();
		}

	}

	void HandleLODs () {
		for (int i = 0; i < bodies.Length; i++) {
            Debug.Log("generator = " + generators[i]);
			if (generators[i] != null) {
				float screenHeight = CalculateScreenHeight (bodies[i]);
				int lodIndex = CalculateLODIndex (screenHeight);
                Debug.Log("lodIndex = " + lodIndex);
				generators[i].SetLOD(lodIndex);
				if (Input.GetKeyDown(KeyCode.Q)) {
					Debug.Log(screenHeight);
				}
			}
		}
	}

	int CalculateLODIndex (float screenHeight) {
		if (screenHeight > lod1Threshold) {
			return 0;
		} else if (screenHeight > lod2Threshold) {
			return 1;
		}
		return 2;
	}

	void DebugLODInfo () {
		if (debugBody && debug) {
			float h = CalculateScreenHeight (debugBody);
			int index = CalculateLODIndex (h);
			Debug.Log ($"Screen height of {debugBody.name}: {h} (lod = {index})");
		}
	}

	float CalculateScreenHeight (CelestialBody body) {
		if (cam == null) {
			cam = GameObject.Find("Main Camera").GetComponent<Camera>();
			camT = cam.transform;
		}
		Quaternion originalRot = camT.rotation;
		Vector3 bodyCentre = body.transform.position;
		camT.LookAt (bodyCentre);

		Vector3 viewA = cam.WorldToViewportPoint(bodyCentre - camT.up * (float)body.settings.radiusU);
		Vector3 viewB = cam.WorldToViewportPoint(bodyCentre + camT.up * (float)body.settings.radiusU);
		float screenHeight = Mathf.Abs (viewA.y - viewB.y);
		camT.rotation = originalRot;

		return screenHeight;
	}
}