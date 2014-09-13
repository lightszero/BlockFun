using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var bs = (Resources.Load("2.ogg") as TextAsset).bytes;

        System.IO.MemoryStream ms = new System.IO.MemoryStream(bs);
        //CSVOggPlayer p = new CSVOggPlayer(ms);
        NVOggPlayer p = new NVOggPlayer(ms);
        this.GetComponent<DSPPlayer>().Play(p);
	}
	
	// Update is called once per frame
	void Update () {
	    StartCoroutine
	}

    void OnGUI()
    {

    }
}
