using UnityEngine;
using System.Collections;

public class tcam : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    float t = 0;
	// Update is called once per frame
	void Update () {
        //t += Time.deltaTime;
        //if(t>15.0f)
        //{
        //    t = 0;
        //}
        //float d =Mathf.Abs( 7.5f - t) /7.5f;
        //float v = 0.1f + d * 50;

        this.GetComponent<Camera>().orthographicSize = s +s2;
	}
    float s = 0.1f;
    float s2 = 0f;

    void OnGUI()
    {
        s = GUI.VerticalScrollbar(new Rect(50, 0, 24, Screen.height), s, 0.1f, 0.1f, 1.0f);

        s2 = GUI.VerticalScrollbar(new Rect(0, 0, 24, Screen.height), s2, 5.0f, 0f, 50.0f);

    }
}
