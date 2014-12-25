using UnityEngine;
using System.Collections;

public class openbtn : MonoBehaviour {

    public UnityEngine.UI.InputField input;
	// Use this for initialization
	void Start () {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                try
                {
                    Application.OpenURL(GetURL.fullUrl + "#" + input.text);
                }
                catch
                {

                }
            });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
