using System;
using System.Collections.Generic;

using System.Text;

using UnityEngine;

public class PixelWorld:MonoBehaviour
{
    public int width =1024;
    public int height =1024;

    void Start()
    {
        wTex = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
        wordData = new worldData[width, height]; 
    }
    public class worldData
    {

    }
    worldData[,] wordData
    {
        get;
        private set;
    }
    public Texture2D wTex
    {
        get;
        private set;
    }
}

