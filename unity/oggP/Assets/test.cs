using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var bs = (Resources.Load("1.ogg") as TextAsset).bytes;

        System.IO.MemoryStream ms = new System.IO.MemoryStream(bs);
        OggPlayer p = new OggPlayer(ms, true);
        this.GetComponent<DSPPlayer>().Play(p);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
