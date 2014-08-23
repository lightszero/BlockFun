using System;
using System.Collections.Generic;

using System.Text;

using UnityEngine;

public class PixelWorld : MonoBehaviour
{
    public Texture2D srcTex;
    public Texture2D tileTex;
    public int tileSplit = 4;
    List<Texture2D> mipTileTex = new List<Texture2D>();
    void Start()
    {
        Texture2D mip = new Texture2D(tileTex.width, tileTex.height, TextureFormat.ARGB32, true, false);
        {
            tileTex.filterMode = FilterMode.Point;
            var pdata = tileTex.GetPixels32(0);
            mip.SetPixels32(pdata, 0);
            mip.Apply(true);
        }
        int width = tileTex.width;
        int layer = 0;
        while (width > tileSplit)
        {
            width /= 2;
            layer++;
            Debug.Log("p layer:" + layer);

            Texture2D m = new Texture2D(width, width, TextureFormat.ARGB32, false, true);
            m.filterMode = FilterMode.Point;
            var d = mip.GetPixels32(layer);
            m.SetPixels32(d, 0);
            m.Apply();
            mipTileTex.Add(m);
        }
        GameObject.Destroy(mip);

        wordData = new worldData[srcTex.width * srcTex.height];
        scale = srcTex.width;
    }

    void Update()
    {
        this.transform.localScale = new Vector3(scale,scale,1);
        int layer =0;
        int width = srcTex.width / 2;
        while (scale < width)
        {
            layer++;
            width /= 2;
        }
        //Debug.Log("layer=" + layer);
        if(layer==0)
        {
            this.GetComponent<MeshRenderer>().material.SetTexture("_BlockTex", tileTex);
        }
        else
        {
            int c = layer - 1;
            if (c >= mipTileTex.Count) c = mipTileTex.Count - 1;
            this.GetComponent<MeshRenderer>().material.SetTexture("_BlockTex", mipTileTex[c]);
        }
        RP();
    }
    void RP()
    {
        for(int i=0;i<512;i++)
        {
           int x= UnityEngine.Random.Range(0, 1024);
           int y = UnityEngine.Random.Range(0, 1024);
           int a = UnityEngine.Random.Range(0, 255);
           srcTex.SetPixel(x, y, new Color32(0, 0, 0, (byte)a));
        }
        srcTex.Apply();
    }
    float scale = 0f;

    void OnGUI()
    {

        scale = GUI.VerticalScrollbar(new Rect(0, 100, 24, Screen.height-150), scale, 5.0f, 1024.0f, 10.0f);
        GUI.Label(new Rect(0, 0, 300, 100), "fps=" + Time.frameCount/Time.timeSinceLevelLoad);

    }
    public class worldData
    {

    }
    worldData[] wordData
    {
        get;
        set;
    }
    public Texture2D wTex
    {
        get;
        private set;
    }
}

