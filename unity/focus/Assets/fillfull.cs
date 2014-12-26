using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class fillfull : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.localScale = new Vector3((float)Screen.width / (float)Screen.height, 1, 0);
	}
}
