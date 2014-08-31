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
        if(_edit.drawColor!= pick.GetPickColor())
            _edit.drawColor = pick.GetPickColor();
        if (_edit.drawIndex != pick.GetPickColorIndex()) 
            _edit.drawIndex = pick.GetPickColorIndex();
        if (_edit.palette != pick.GetPalette()) 
            _edit.palette = pick.GetPalette();
	}
}
