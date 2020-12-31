using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All credit goes to Sebastian Lague for this script
public class StarDome : MonoBehaviour
{
	public MeshRenderer starPrefab;
	public Vector2 radiusMinMax;
	public int count = 1000;
	const float calibrationDst = 2000;

	Camera cam;

	void Start () {
		cam = Camera.main;
		if (cam) {
			float starDst = 500_000f;//cam.farClipPlane - radiusMinMax.y;
			float scale = starDst / calibrationDst;

			for (int i = 0; i < count; i++) {
				MeshRenderer star = Instantiate (starPrefab, Random.onUnitSphere * starDst, Quaternion.identity, transform);
                star.transform.tag = "StarDome";
                star.gameObject.layer = 11; // StarDome layer
				float t = SmallestRandomValue (6);
				star.transform.localScale = Vector3.one * Mathf.Lerp (radiusMinMax.x, radiusMinMax.y, t) * scale;
			}
		}
	}

	float SmallestRandomValue (int iterations) {
		float r = 1;
		for (int i = 0; i < iterations; i++) {
			r = Mathf.Min (r, Random.value);
		}
		return r;
	}

	void LateUpdate () {
		/*if (cam != null) {
			transform.position = cam.transform.position;
		}*/
	}
}