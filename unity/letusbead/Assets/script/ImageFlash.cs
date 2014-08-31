using UnityEngine;
using System.Collections;

public class ImageFlash : MonoBehaviour {

	// Use this for initialization
	void Start () {
        img = this.GetComponent<UnityEngine.UI.Image>();
	}
    UnityEngine.UI.Image img;
	// Update is called once per frame
    float t = 0;
	void Update () {
        t += Time.deltaTime;
        if (t > 2.0f) t = 0;
        float a = Mathf.Abs(t - 1);

        var c = img.color;
        c.a = a;
        img.color = c;
	}
}
