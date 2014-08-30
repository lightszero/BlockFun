using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class com_ColorPick : MonoBehaviour
{
    public Color useColor = Color.white;
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
    void Start()
    {
        if (PickPanel == null) PickPanel = this.transform;
        color0 = PickPanel.Find("color0").GetComponent<UnityEngine.UI.RawImage>();
        color1 = PickPanel.Find("color1").GetComponent<UnityEngine.UI.RawImage>();
        sc = PickPanel.Find("SliderSC").GetComponent<UnityEngine.UI.Slider>();
        sr = PickPanel.Find("SliderR").GetComponent<UnityEngine.UI.Slider>();
        sg = PickPanel.Find("SliderG").GetComponent<UnityEngine.UI.Slider>();
        sb = PickPanel.Find("SliderB").GetComponent<UnityEngine.UI.Slider>();
        m = PickPanel.Find("m/Quad");
        var rC = PickPanel.Find("m/ColorPick").GetComponent<UnityEngine.UI.RawImage>();
        Texture2D picktex;
        picktex = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
        //picktex.wrapMode = TextureWrapMode.Clamp;
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
    }
    void ChangeColor()
    {//用useColor 重填各个界面
        color1.color = useColor;
        sr.value = useColor.r;
        sg.value = useColor.g;
        sb.value = useColor.b;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.transform == m)
            {
                float dis = 1.0f;

                float v1 = (new Vector2(0, 0) - hit.textureCoord).magnitude;
                float v2 = (new Vector2(1, 0) - hit.textureCoord).magnitude;
                float v3 = (new Vector2(0, 1) - hit.textureCoord).magnitude;
                float v4 = (new Vector2(1, 1) - hit.textureCoord).magnitude;
                dis = Mathf.Max(v1, v2);
                dis = Mathf.Max(dis, v3);
                dis = Mathf.Max(dis, v4);
                Debug.Log(hit.textureCoord + "|" + v1 + "," + v2 + "," + v3 + "," + v4);
                float t = dis * 4 - v1 - v2 - v3 - v4;
                v1 = (dis - v1) / t;
                v2 = (dis - v2) / t;
                v3 = (dis - v3) / t;
                v4 = (dis - v4) / t;

                useColor = Color.black * v1 + Color.black * v2 + Color.white * v3 + hColor * v4;
                ChangeColor();
            }
        }
    }
}
