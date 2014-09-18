using UnityEngine;
using System.Collections;

public class menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.DontDestroyOnLoad(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,200,50),"随机数求圆周率"))
        {
            Application.LoadLevel("1.pi");
        }
        if (GUI.Button(new Rect(200, 0, 200, 50), "点积"))
        {
            Application.LoadLevel("2.dot");
        }
        if (GUI.Button(new Rect(400, 0, 200, 50), "点积与叉积"))
        {
            Application.LoadLevel("3.dotcross");
        }
        if (GUI.Button(new Rect(600, 0, 200, 50), "插值"))
        {
            Application.LoadLevel("4.lerp");
        }
    }
}
