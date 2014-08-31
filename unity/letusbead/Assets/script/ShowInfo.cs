using UnityEngine;
using System.Collections;

public class ShowInfo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.GetComponent<UnityEngine.UI.Text>().text ="cache in:"+ Application.temporaryCachePath;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
