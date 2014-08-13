using UnityEngine;
using System.Collections;

public class CreateBox : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Texture2D tex = Resources.Load("j1") as Texture2D;

        for (int y = 0; y < 50;y++ )
            for (int x = 0; x < 100; x++)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = new Vector3(x, y, 0);
                Material mat = new Material(Shader.Find("Diffuse"));
                mat.mainTexture = tex;
                mat.mainTextureScale = new Vector2(0.25f, 0.25f);
                obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
