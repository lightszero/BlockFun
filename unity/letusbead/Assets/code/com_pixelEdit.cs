using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class com_pixelEdit : MonoBehaviour
{

    // Use this for initialization
    public com_ColorPick pluginColorPick;
    public com_view pluginView;
    Transform pick = null;

    UnityEngine.UI.RawImage drawpanel;

    //UnityEngine.UI.Image PanelBack;
    void Start()
    {
        pick = this.transform.Find("PanelBack/PixelEdit");
        drawpanel = this.transform.Find("PanelBack/PanelDraw").GetComponent<UnityEngine.UI.RawImage>();


        var PanelBack = this.transform.Find("PanelBack/PanelBack").GetComponent<UnityEngine.UI.RawImage>();
        Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        Color dc = new Color32(204, 255, 204, 255);
        tex.SetPixel(0, 1, dc);
        tex.SetPixel(1, 1, dc);
        tex.SetPixel(0, 0, dc * 0.5f);
        tex.SetPixel(1, 0, dc * 0.5f);
        tex.Apply();
        PanelBack.texture = tex;


        SetDrawArea();

        var btnShowBack = this.transform.Find("PanelTop/BtnShowBack").GetComponent<UnityEngine.UI.Button>();
        btnShowBack.onClick.AddListener(() =>
        {
            PanelBack.enabled = !PanelBack.enabled;
        });
        var btnChangeBack = this.transform.Find("PanelTop/BtnChangeBack").GetComponent<UnityEngine.UI.Button>();
        btnChangeBack.onClick.AddListener(() =>
        {
            tex.SetPixel(0, 1, drawColor);
            tex.SetPixel(1, 1, drawColor);
            tex.SetPixel(0, 0, drawColor * 0.5f);
            tex.SetPixel(1, 0, drawColor * 0.5f);
            tex.Apply();
        }
        );
        for (int i = 0; i < 5; i++)
        {
            var btn = this.transform.Find("PanelTop/BtnDS" + i.ToString()).GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() =>
            {
                drawpanel.material.SetTexture("_SharpTex", btn.GetComponent<UnityEngine.UI.RawImage>().texture);
                drawpanel.material.SetVector("_DrawSize", new Vector4(edit.width, edit.height, 0, 0));
            });
        }
        {//EditColor
            var btn = this.transform.Find("PanelTop/BtnEditColor").GetComponent<UnityEngine.UI.Button>();
            var dp = pluginColorPick.GetComponent<RectTransform>().position;
            btn.onClick.AddListener(() =>
            {
                if (!pluginView.gameObject.activeSelf)
                {
                    pluginView.gameObject.SetActive(true);
                    Vector3 p = dp;
                    pluginColorPick.GetComponent<RectTransform>().position = p;
                }
                else
                {
                    pluginView.gameObject.SetActive(false);
                    Vector3 p = dp;
                    p.y += 342;
                    pluginColorPick.GetComponent<RectTransform>().position = p;
                }
            });
        }
        {
            var btn = this.transform.Find("PanelTop/BtnGen3D").GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() =>
            {
                GenHeight();
            });
        }
        {
            var btn = this.transform.Find("PanelTop/BtnShow3D").GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() =>
            {
                showmode++;
                if (showmode > 3) showmode = 0;
                if (showmode == 1)
                    ShowColorWithLight();
                if (showmode == 2)

                    ShowHeight();
                if (showmode == 3)

                    ShowNormal();
                if (showmode == 0)

                    ShowColor();

            });
        }
        {
            var btn = this.transform.Find("PanelTop/BtnEdit3D").GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() =>
            {

            });
        }
    }
    int showmode = 0;
    public void Resize(int w, int h)
    {
        edit.Resize(w, h, TextureFormat.ARGB32, false);
        editHeight = null;
        editNormal = null;
        {
            var drawpanel = this.transform.Find("PanelBack/PanelDraw").GetComponent<UnityEngine.UI.RawImage>();
            drawpanel.material.SetVector("_DrawSize", new Vector4(edit.width, edit.height, 0, 0));
        }
    }
    void GenHeight()
    {
        try
        {
            Color32[] _bsdata = edit.GetPixels32(0);
            KDTree2D tree = new KDTree2D();
            List<KDTree2D.Train> treedata = new List<KDTree2D.Train>();
            FindBorder(edit.width, edit.height, _bsdata, treedata);//四次采样寻找边界,并把在边界上的点填入点集

            var node = tree.CreatKDTree(treedata);//用KDTree来查找最近点
            int w = edit.width;
            int h = edit.height;
            DateTime t1 = DateTime.Now;

            float maxlen = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (_bsdata[y * w + x].a > 0)//形状内
                    {
                        //var near = tree.KDTreeFindNearest(node, new KDTree2D.Train() { positionX = x, positionY = y });
                        var near = tree.BBFFindNearest(node, new KDTree2D.Train() { positionX = x, positionY = y });
                        float d = (float)Mathf.Sqrt((near.point.positionX - x) * (near.point.positionX - x)
                            + (near.point.positionY - y) * (near.point.positionY - y));
                        float wl = d / 16.0f;
                        if (wl > 1.0f) wl = 1.0f;
                        wl *= Mathf.PI * 0.5f;
                        wl=Mathf.Sin(wl);
                        int l = (int)(wl * 255);
                        if (l > 255) l = 255;
                        _bsdata[y * w + x].a = (byte)(l);
                    }
                    else
                    {
                        _bsdata[y * w + x].a = 0;//形状外
                    }
                }
            }

            editHeight = new Texture2D(w, h, TextureFormat.Alpha8, false);
            editHeight.SetPixels32(_bsdata, 0);
            editHeight.Apply();

            _bsdata = GenNormal(w, h, _bsdata);
            editNormal = new Texture2D(w, h, TextureFormat.ARGB32, false);
            editNormal.filterMode = FilterMode.Point;
            editNormal.SetPixels32(_bsdata, 0);
            editNormal.Apply();

            DateTime t2 = DateTime.Now;
            Debug.Log("GetTime=" + (t2 - t1).TotalSeconds);
        }
        catch
        {

        }
    }
    void FindBorder(int w, int h, Color32[] colors, List<KDTree2D.Train> points)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (colors[y * w + x].a > 0)
                {
                    if (x > 0 && colors[y * w + x - 1].a == 0)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                    if (x < w - 1 && colors[y * w + x + 1].a == 0)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                    if (y > 0 && colors[(y - 1) * w + x].a == 0)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                    if (y < h - 1 && colors[(y + 1) * w + x].a == 0)
                    {
                        points.Add(new KDTree2D.Train() { positionX = x, positionY = y });
                        continue;
                    }
                }
            }
        }
    }
    Color32[] GenNormal(int w, int h, Color32[] src)
    {
        Color32[] normal = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float hnow = (float)(src[y * w + x].a) / 255.0f;

                Vector3 normalleft = new Vector3(-1, 0, 0);
                if (x > 0)
                {
                    float hw = (float)(src[y * w + x - 1].a) / 255.0f;
                    normalleft = new Vector3(-1, 0, (hw - hnow) * 4.0f);
                    normalleft.Normalize();
                }
                Vector3 normalright = new Vector3(1, 0, 0);
                if (x < w - 1)
                {
                    float hw = (float)(src[y * w + x + 1].a) / 255.0f;
                    normalright = new Vector3(1, 0, (hw - hnow) * 4.0f);
                    normalright.Normalize();
                }
                Vector3 normaltop = new Vector3(0, 1, 0);
                if (y < h - 1)
                {
                    float hw = (float)(src[(y + 1) * w + x].a) / 255.0f;
                    normaltop = new Vector3(0, 1, (hw - hnow) * 4.0f);
                    normaltop.Normalize();
                }
                Vector3 normaldown = new Vector3(0, -1, 0);
                if (y > 0)
                {
                    float hw = (float)(src[(y - 1) * w + x].a) / 255.0f;
                    normaldown = new Vector3(0, -1, (hw - hnow) * 4.0f);
                    normaldown.Normalize();
                }
                Vector3 lt = Vector3.Cross(normaltop, normalleft);
                Vector3 rd = Vector3.Cross(normaldown, normalright);
                Vector3 last = lt * 0.5f + rd * 0.5f;
                last += Vector3.one;
                last *= 0.5f;
                normal[y * w + x].r = (byte)(last.x * 255);
                normal[y * w + x].g = (byte)(last.y * 255);
                normal[y * w + x].b = (byte)(last.z * 255);
                normal[y * w + x].a = 1;
            }
        }
        return normal;
    }
    private void SetDrawArea()
    {
        edit = new Texture2D(initwidth, initheight, TextureFormat.Alpha8, false, false);
        Color32[] c = new Color32[initwidth * initheight];
        for (int i = 0; i < initwidth * initheight; i++)
        {
            c[i].a = 0;
        }
        edit.SetPixels32(c);

        edit.filterMode = FilterMode.Point;
        edit.wrapMode = TextureWrapMode.Clamp;
        edit.Apply();

        ShowColor();
    }
    void ShowColor()
    {
        drawpanel.material = new Material(Shader.Find("Custom/pixel_sharp"));
        drawpanel.material.SetTexture("_MainTex", edit);
        drawpanel.material.SetTexture("_ColorTex", palette);
        drawpanel.material.SetVector("_DrawSize", new Vector4(edit.width, edit.height, 0, 0));

        drawpanel.texture = edit;
    }
    void ShowHeight()
    {
        if (editHeight == null) return;
        drawpanel.material = new Material(Shader.Find("Custom/show_alpha"));
        drawpanel.material.SetTexture("_MainTex", editHeight);
        drawpanel.texture = editHeight;
    }
    void ShowNormal()
    {
        if (editNormal == null) return;
        drawpanel.material = new Material(Shader.Find("Unlit/Texture"));
        drawpanel.material.SetTexture("_MainTex", editNormal);
        drawpanel.texture = editNormal;
    }

    void ShowColorWithLight()
    {
        drawpanel.material = new Material(Shader.Find("Custom/pixel_sharp_normal"));
        drawpanel.material.SetTexture("_MainTex", edit);
        drawpanel.material.SetTexture("_ColorTex", palette);
        if (editNormal != null)
        {
            drawpanel.material.SetTexture("_NormalTex", editNormal);
        }
        drawpanel.material.SetVector("_DrawSize", new Vector4(edit.width, edit.height, 0, 0));
        drawpanel.texture = edit;
    }
    public int initwidth = 64;
    public int initheight = 64;
    public Color drawColor = Color.red;
    public byte drawIndex = 0;


    public Texture2D palette
    {
        get
        {
            return pluginColorPick.GetPalette();
        }

    }

    public enum Mode
    {
        Draw,
    }
    public Mode mode = Mode.Draw;
    public Texture2D edit
    {
        get;
        private set;
    }
    public Texture2D editHeight
    {
        get;
        private set;
    }
    public Texture2D editNormal
    {
        get;
        private set;
    }
    // Update is called once per frame
    void Update()
    {
        if (this.drawColor != pluginColorPick.GetPickColor())
            this.drawColor = pluginColorPick.GetPickColor();
        if (this.drawIndex != pluginColorPick.GetPickColorIndex())
            this.drawIndex = pluginColorPick.GetPickColorIndex();


        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == pick.gameObject)
                {
                    Vector2 pos = hit.textureCoord;
                    //pos.y = 1.0f - pos.y;

                    int x = (int)(pos.x * (float)edit.width);
                    int y = (int)(pos.y * (float)edit.height);
                    if (x >= edit.width) x = edit.width - 1;
                    if (y >= edit.height) y = edit.height - 1;
                    edit.SetPixel(x, y, new Color32(0, 0, 0, drawIndex));
                    edit.Apply();
                }
            }


        }
    }
}
