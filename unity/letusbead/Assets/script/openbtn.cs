using UnityEngine;
using System.Collections;

public class openbtn : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                Application.OpenURL(Application.temporaryCachePath);
            });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
