using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class pixeltest : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

        LoadTexDistanceField("ll1");
    }

    Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>();

    void LoadTexDistanceField(string name)
    {
#if UNITY_STANDALONE
        string filename = System.IO.Path.Combine(Application.streamingAssetsPath, name + ".png");
        string filename_df = System.IO.Path.Combine(Application.streamingAssetsPath, name + "_df.png");
        //if (System.IO.File.Exists(filename_df))
        //{
        //    Texture2D tex = new Texture2D(1, 1);
        //    tex.LoadImage(System.IO.File.ReadAllBytes(filename_df));
        //    texs[name] = tex;
        //}
        //else
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(System.IO.File.ReadAllBytes(filename));//加载原始图片
            Color32[] _bsdata = tex.GetPixels32(0);
            KDTree2D tree = new KDTree2D();
            List<KDTree2D.Train> treedata = new List<KDTree2D.Train>();
            FindBorder(tex.width,tex.height,_bsdata, treedata);//四次采样寻找边界,并把在边界上的点填入点集

            var node = tree.CreatKDTree(treedata);//用KDTree来查找最近点
            int w = tex.width;
            int h = tex.height;
            DateTime t1 = DateTime.Now;
            
            float maxlen = (float)Mathf.Sqrt(w * w + h * h) / 4;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var near = tree.KDTreeFindNearest(node, new KDTree2D.Train() { positionX = x, positionY = y });
                    float d = (float)Mathf.Sqrt((near.point.positionX - x) * (near.point.positionX - x) 
                        + (near.point.positionY - y) * (near.point.positionY - y));
                    if (_bsdata[y * w + x].a < 128)
                    {
                        d *= -1;

                        _bsdata[y * w + x]= _bsdata[(int)near.point.positionY * w + (int)near.point.positionX];

                    }
                    float dist = d / maxlen;
                    if (dist < -1) dist = -1;
                    if (dist > 1) dist = 1;
                    var b = (byte)(128 + 127.0f * dist);

                    _bsdata[y * w + x].a = b;//替换原alpha值为距离值，形状内>128,形状外<128

                }
            }
            DateTime t2 = DateTime.Now;
            Debug.Log("t=" + (t2 - t1).TotalSeconds);
            tex.SetPixels32(_bsdata);

            tex.Apply();
            
            System.IO.File.WriteAllBytes(filename_df,  tex.EncodeToPNG());//保存为新文件
            texs[name] = tex;
        }
#endif
    }
    void FindBorder(int w, int h, Color32[] colors, List<KDTree2D.Train> points)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (colors[y * w + x].a > 128)
                {
                    if (x > 0 && colors[y * w + x - 1].a <= 128)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                    if (x < w - 1 && colors[y * w + x + 1].a <= 128)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                    if (y > 0 && colors[(y - 1) * w + x].a <= 128)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                    if (y < h - 1 && colors[(y + 1) * w + x].a <= 128)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
