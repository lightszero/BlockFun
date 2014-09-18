using UnityEngine;
using System.Collections;

public class test_pi : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        tex = this.GetComponent<MeshRenderer>().material.mainTexture as Texture2D;
    }
    Texture2D tex;
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 1000; i++)
        {
            int x = Random.Range(0, tex.width);//求随机数
            int y = Random.Range(0, tex.height);
        
           
            float fx =Mathf.Abs((float)x/(float)(tex.width)-0.5f)*2.0f;
            float fy =Mathf.Abs((float)y/(float)(tex.height)-0.5f)*2.0f;
            if ((fx * fx + fy * fy) <= 1)//在圆内
            {
                incount++;
                Color c = tex.GetPixel(x, y);
                c = Color.red * 0.01f + c * 0.99f;
                tex.SetPixel(x, y, c);
            }
            else
            {
                outcount++;
            }
        }
        tex.Apply();
    }
    int outcount = 0;
    int incount = 0;
    void OnGUI()
    {
        GUI.Label(new Rect(0, 50, 500, 50), "PI=" +(float)incount/(float)(incount+ outcount) *4.0f);
    }
}
