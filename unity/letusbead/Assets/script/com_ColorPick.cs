using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public class com_ColorPick : MonoBehaviour
{
    private Color useColor = Color.white;
    public Color GetPickColor()
    {
        if(colorIndex==0)
        {
            useColor.a = 0;
        }
        return useColor;
    }
    public void PickColor(byte index)
    {
        colorIndex = index;
        ChangeColorH();
    }
    public byte GetPickColorIndex()
    {
        return (byte)colorIndex;
    }
    public Texture2D GetPalette()
    {
        return colorIndexTex;
    }
    public Transform PickPanel;
    // Use this for initialization

    Transform m;//彩度亮度设置面板
    Color hColor = Color.white;//色度值
    UnityEngine.UI.RawImage color0;
    UnityEngine.UI.RawImage color1;
    UnityEngine.UI.Slider sc;
    UnityEngine.UI.Slider sr;
    UnityEngine.UI.Slider sg;
    UnityEngine.UI.Slider sb;
    UnityEngine.UI.InputField vr;
    UnityEngine.UI.InputField vg;
    UnityEngine.UI.InputField vb;

    Transform palettePick;
    int colorIndex = 0;
    Texture2D colorIndexTex = null;

    RectTransform paletteSelect;
    Vector3 palettleSelectPos;
    void Start()
    {
        if (PickPanel == null) PickPanel = this.transform;
        color0 = PickPanel.Find("colorLast/color0").GetComponent<UnityEngine.UI.RawImage>();
        color1 = PickPanel.Find("color1").GetComponent<UnityEngine.UI.RawImage>();
        sc = PickPanel.Find("SliderSC").GetComponent<UnityEngine.UI.Slider>();
        sr = PickPanel.Find("SliderR").GetComponent<UnityEngine.UI.Slider>();
        sg = PickPanel.Find("SliderG").GetComponent<UnityEngine.UI.Slider>();
        sb = PickPanel.Find("SliderB").GetComponent<UnityEngine.UI.Slider>();
        vr = PickPanel.Find("ValueR").GetComponent<UnityEngine.UI.InputField>();
        vg = PickPanel.Find("ValueG").GetComponent<UnityEngine.UI.InputField>();
        vb = PickPanel.Find("ValueB").GetComponent<UnityEngine.UI.InputField>();
        m = PickPanel.Find("m/Quad");
        initPicker();

        colorIndexTex = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        colorIndexTex.filterMode = FilterMode.Point;
        InitDefPalette();
        InitPalette();
    }

    private void InitPalette()
    {
        var rP = PickPanel.Find("Palette").GetComponent<UnityEngine.UI.RawImage>();

        palettePick = PickPanel.Find("Palette/Quad");
        paletteSelect = PickPanel.Find("Palette/Panel") as RectTransform;
        palettleSelectPos = paletteSelect.localPosition;
        colorIndexTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
        colorIndexTex.Apply();
        colorIndex = 0;
        rP.texture = colorIndexTex;
        var blast = PickPanel.Find("colorLast").GetComponent<UnityEngine.UI.Button>();
        blast.onClick.AddListener(() =>
        {
            useColor = color0.color;
            ChangeColorH();
        });
        var blight = PickPanel.Find("colorLineLight").GetComponent<UnityEngine.UI.Button>();
        blight.onClick.AddListener(() =>
        {
            int y = (int)(colorIndex / 16);
            if (y == 0) return;
            for (int j = 0; j < 16; j++)
            {
                Color t = useColor;
                Color t1 = t * (float)(15 - j) / 15.0f;
                t1.a = 1.0f;

                Color t2 = t1 + Color.white * (float)j / 15.0f;
                colorIndexTex.SetPixel(j, y, t2);
                //colorIndexTex.SetPixel(j, i * 2 + 3, t2);
            }
            colorIndexTex.Apply();
        });
        var bdark = PickPanel.Find("colorLineDark").GetComponent<UnityEngine.UI.Button>();
        bdark.onClick.AddListener(() =>
        {
            int y = (int)(colorIndex / 16);
            if (y == 0) return;
            for (int j = 0; j < 16; j++)
            {
                Color t = useColor;
                Color t1 = t * (float)(15 - j) / 15.0f;
                t1.a = 1.0f;

                //olor t2 = t1 + Color.white * (float)j / 15.0f;
                colorIndexTex.SetPixel(j, y, t1);
            }
            colorIndexTex.Apply();
        });
    }
    void InitDefPalette()
    {
        colorIndexTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
        colorIndexTex.SetPixel(1, 0, new Color(0, 0, 0, 1.0f));
        for (int i = 0; i < 7; i++)//彩虹
        {
            Color color7 = Get7Color(i);
            switch (i)
            {
                case 0:
                    color7 = new Color32(255, 0, 0, 255);
                    break;
                case 1:
                    color7 = new Color32(255, 165, 0, 255);
                    break;
                case 2:
                    color7 = new Color32(255, 255, 0, 255);
                    break;
                case 3:
                    color7 = new Color32(0, 255, 0, 255);
                    break;
                case 4:
                    color7 = new Color32(0, 127, 255, 255);
                    break;
                case 5:
                    color7 = new Color32(0, 0, 255, 255);
                    break;
                case 6:
                    color7 = new Color32(139, 0, 255, 255);
                    break;
            }
            colorIndexTex.SetPixel(2 * i + 2, 0, color7);
            color7 *= 0.5f;
            color7.a = 1.0f;
            colorIndexTex.SetPixel(2 * i + 3, 0, color7);
        }
        for (int i = 0; i < 16; i++)
        {
            Color t = Color.white * (float)(15 - i) / 15.0f;
            t.a = 1.0f;
            colorIndexTex.SetPixel(i, 1, t);
        }
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                Color t = Get7Color(i);
                Color t1 = t * (float)(15 - j) / 15.0f;
                t1.a = 1.0f;

                Color t2 = t1 + Color.white * (float)j / 15.0f;
                colorIndexTex.SetPixel(j, i * 2 + 2, t1);
                colorIndexTex.SetPixel(j, i * 2 + 3, t2);
            }
        }
    }
    Color Get7Color(int i)
    {
        Color color7 = Color.black;
        switch (i)
        {
            case 0:
                color7 = new Color32(255, 0, 0, 255);
                break;
            case 1:
                color7 = new Color32(255, 165, 0, 255);
                break;
            case 2:
                color7 = new Color32(255, 255, 0, 255);
                break;
            case 3:
                color7 = new Color32(0, 255, 0, 255);
                break;
            case 4:
                color7 = new Color32(0, 127, 255, 255);
                break;
            case 5:
                color7 = new Color32(0, 0, 255, 255);
                break;
            case 6:
                color7 = new Color32(139, 0, 255, 255);
                break;
        }
        return color7;
    }
    private void initPicker()
    {
        var rC = PickPanel.Find("m/ColorPick").GetComponent<UnityEngine.UI.RawImage>();
        Texture2D picktex;
        picktex = new Texture2D(2, 2, TextureFormat.ARGB32, false, false);
        //picktex.filterMode = FilterMode.Point;
        picktex.wrapMode = TextureWrapMode.Clamp;
        rC.texture = picktex;
        picktex.SetPixel(0, 0, Color.black);
        picktex.SetPixel(1, 0, Color.black);
        picktex.SetPixel(0, 1, Color.white);
        picktex.SetPixel(1, 1, hColor);
        picktex.Apply();

        var rC2 = PickPanel.Find("mc/ColorPick").GetComponent<UnityEngine.UI.RawImage>();
        Texture2D pickColor;
        pickColor = new Texture2D(2, 7, TextureFormat.ARGB32, false, true);
        pickColor.wrapMode = TextureWrapMode.Repeat;
        pickColor.SetPixel(0, 0, new Color(1, 0, 0));
        pickColor.SetPixel(0, 1, new Color(1, 1, 0));
        pickColor.SetPixel(0, 2, new Color(0, 1, 0));
        pickColor.SetPixel(0, 3, new Color(0, 1, 1));
        pickColor.SetPixel(0, 4, new Color(0, 0, 1));
        pickColor.SetPixel(0, 5, new Color(1, 0, 1));
        pickColor.SetPixel(0, 6, new Color(1, 0, 0));
        pickColor.SetPixel(1, 0, new Color(1, 0, 0));
        pickColor.SetPixel(1, 1, new Color(1, 1, 0));
        pickColor.SetPixel(1, 2, new Color(0, 1, 0));
        pickColor.SetPixel(1, 3, new Color(0, 1, 1));
        pickColor.SetPixel(1, 4, new Color(0, 0, 1));
        pickColor.SetPixel(1, 5, new Color(1, 0, 1));
        pickColor.SetPixel(1, 6, new Color(1, 0, 0));
        pickColor.Apply();
        rC2.texture = pickColor;
        //rC.color = useColor;
        {
            var ssC = sc;
            UnityAction<float> onSCChange = (v) =>
            {
                if (v < 1.0f)
                {
                    hColor.r = 1;
                    hColor.g = Mathf.Lerp(0, 1, v);
                    hColor.b = 0;

                }
                else if (v < 2.0f)
                {
                    hColor.r = Mathf.Lerp(1, 0, v - 1);
                    hColor.g = 1;
                    hColor.b = 0;
                }
                else if (v < 3.0f)
                {
                    hColor.r = 0;
                    hColor.g = 1;
                    hColor.b = Mathf.Lerp(0, 1, v - 2);
                }
                else if (v < 4.0f)
                {
                    hColor.r = 0;
                    hColor.g = Mathf.Lerp(1, 0, v - 3);
                    hColor.b = 1;
                }
                else if (v < 5.0f)
                {
                    hColor.r = Mathf.Lerp(0, 1, v - 4);
                    hColor.g = 0;
                    hColor.b = 1;
                }
                else if (v < 6.0f)
                {
                    hColor.r = 1;
                    hColor.g = 0;
                    hColor.b = Mathf.Lerp(1, 0, v - 5);
                }
                picktex.SetPixel(1, 1, hColor);
                picktex.Apply();
            };
            ssC.onValueChanged.AddListener(onSCChange);
        }
        {
            var sR = sr;
            UnityAction<float> onRChange = (v) =>
            {

                useColor.r = v;
                ChangeColor();

            };
            sR.onValueChanged.AddListener(onRChange);
        }
        {
            var sG = sg;
            UnityAction<float> onGChange = (v) =>
            {

                useColor.g = v;
                ChangeColor();
            };
            sG.onValueChanged.AddListener(onGChange);
        }
        {
            var sB = sb;
            UnityAction<float> onBChange = (v) =>
            {

                useColor.b = v;
                ChangeColor();
            };
            sB.onValueChanged.AddListener(onBChange);
        }
        {
            UnityAction<string> change = (v) =>
            {
                int vv;
                if (int.TryParse(v, out vv))
                {
                    if (vv < 0) vv = 0;
                    if (vv > 255) vv = 255;
                    useColor.r = (float)vv / 255.0f;
                    ChangeColor();
                }
                else
                {
                    vr.value = ((int)(useColor.r * 255)).ToString();
                }
            };
            vr.onSubmit.AddListener(change);
        }
        {
            UnityAction<string> change = (v) =>
            {
                int vv;
                if (int.TryParse(v, out vv))
                {
                    if (vv < 0) vv = 0;
                    if (vv > 255) vv = 255;
                    useColor.g = (float)vv / 255.0f;
                    ChangeColor();
                }
                else
                {
                    vg.value = ((int)(useColor.g * 255)).ToString();
                }
            };
            vg.onSubmit.AddListener(change);
        }
        {
            UnityAction<string> change = (v) =>
            {
                int vv;
                if (int.TryParse(v, out vv))
                {
                    if (vv < 0) vv = 0;
                    if (vv > 255) vv = 255;
                    useColor.b = (float)vv / 255.0f;
                    ChangeColor();
                }
                else
                {
                    vb.value = ((int)(useColor.b * 255)).ToString();
                }
            };
            vb.onSubmit.AddListener(change);
        }
    }
    void ChangeColor()
    {//用useColor 重填各个界面
        ChangeColorH();
        sc.value = GetHFromRGB(useColor) * 6.0f;
    }

    private float GetHFromRGB(Color color)
    {




        float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);

        float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);

        float distance = max - min;



        float _lightness = (max + min) / 2;

        if (distance == 0)
        {

            return 0;

        }

        else
        {

            float hueTmp;

            //_saturation =

            //    (_lightness < 0.5) ?

            //    (distance / (max + min)) : (distance / ((2 - max) - min));

            float tempR = (((max - color.r) / 6) + (distance / 2)) / distance;

            float tempG = (((max - color.g) / 6) + (distance / 2)) / distance;

            float tempB = (((max - color.b) / 6) + (distance / 2)) / distance;

            if (color.r == max)
            {

                hueTmp = tempB - tempG;

            }

            else if (color.g == max)
            {

                hueTmp = (0.33333333333333331f + tempR) - tempB;

            }

            else
            {

                hueTmp = (0.66666666666666663f + tempG) - tempR;

            }

            if (hueTmp < 0)
            {

                hueTmp += 1;

            }

            if (hueTmp > 1)
            {

                hueTmp -= 1;

            }

            return hueTmp;

        }
    }
    void ChangeColorH()
    {//用useColor 重填各个界面
        int x = colorIndex % 16;
        int y = colorIndex / 16;
        if (colorIndex != 0)
        {
            color1.color = useColor;
            colorIndexTex.SetPixel(x, y, useColor);
            colorIndexTex.Apply();
        }
        sr.value = useColor.r;
        sg.value = useColor.g;
        sb.value = useColor.b;
        vr.value = ((int)(useColor.r * 255)).ToString();
        vg.value = ((int)(useColor.g * 255)).ToString();
        vb.value = ((int)(useColor.b * 255)).ToString();


        //重新定位选中色板位置
        var rect = (paletteSelect.parent as RectTransform).rect;
        Vector3 pos = palettleSelectPos;
        pos.x += (float)x * rect.width / 16.0f;
        pos.y += (float)y * rect.height / 16.0f;
        paletteSelect.localPosition = pos;

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.transform == m) //选色
                {
                    //四边形插值
                    float vx = hit.textureCoord.x;
                    Color hc = Color.white * (1 - vx) + hColor * vx;
                    float vy = hit.textureCoord.y;
                    useColor = Color.black * (1 - vy) + hc * vy;
                    ChangeColorH();
                }
                if (hit.collider.gameObject.transform == palettePick)//调色板选色
                {

                    int x = (int)(hit.textureCoord.x * 16);
                    if (x > 15) x = 15;
                    if (x < 0) x = 0;
                    int y = (int)(hit.textureCoord.y * 16);
                    if (y > 15) y = 15;
                    if (y < 0) y = 0;
                    Debug.Log(hit.textureCoord + "|" + x + "|" + y);
                    useColor = colorIndexTex.GetPixel(x, y);
                    colorIndex = y * 16 + x;
                    color0.color = color1.color = useColor;
                    ChangeColor();
                }


            }
        }
    }
}
