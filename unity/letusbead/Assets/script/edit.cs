using UnityEngine;
using System.Collections;

public class edit : MonoBehaviour {

    public com_ColorPick pick;
    public com_pixelEdit _edit;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        _edit.drawColor = pick.GetPickColor();
	}
}
