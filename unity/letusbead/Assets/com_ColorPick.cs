using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class com_ColorPick : MonoBehaviour
{
    public Color useColor = Color.white;
    public Transform PickPanel;
    // Use this for initialization

    void Start()
    {
        if (PickPanel == null) PickPanel = this.transform;
        var rC = PickPanel.Find("m/ColorPick").GetComponent<UnityEngine.UI.RawImage>();
        Texture2D picktex;
        picktex = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
        //picktex.wrapMode = TextureWrapMode.Clamp;
        rC.texture = picktex;
        picktex.SetPixel(0, 0, Color.black);
        picktex.SetPixel(1, 0, Color.black);
        picktex.SetPixel(0, 1, Color.white);
        picktex.SetPixel(1, 1, useColor);
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
            var ssC = PickPanel.Find("SliderSC").GetComponent<UnityEngine.UI.Slider>();
            UnityAction<float> onSCChange = (v) =>
            {
                if(v<1.0f)
                {
                    useColor.r = 1;
                    useColor.g = Mathf.Lerp(0, 1, v);
                    useColor.b = 0;
                    
                }
                else if(v<2.0f)
                {
                    useColor.r = Mathf.Lerp(1, 0, v-1);
                    useColor.g = 1;
                    useColor.b = 0;
                }
                else if (v < 3.0f)
                {
                    useColor.r = 0;
                    useColor.g = 1;
                    useColor.b = Mathf.Lerp(0,1,v-2);
                }
                else if (v < 4.0f)
                {
                    useColor.r = 0;
                    useColor.g = Mathf.Lerp(1, 0, v - 3);
                    useColor.b = 1;
                }
                else if (v < 5.0f)
                {
                    useColor.r = Mathf.Lerp(0, 1, v - 4);
                    useColor.g = 0;
                    useColor.b = 1;
                }
                else if (v < 6.0f)
                {
                    useColor.r = 1;
                    useColor.g = 0;
                    useColor.b = Mathf.Lerp(1, 0, v - 5);
                }
                picktex.SetPixel(1, 1, useColor);
                picktex.Apply();
            };
            ssC.onValueChanged.AddListener(onSCChange);
        }
        {
            var sR = PickPanel.Find("SliderR").GetComponent<UnityEngine.UI.Slider>();
            UnityAction<float> onRChange = (v) =>
                {

                    useColor.r = v;
                    picktex.SetPixel(1, 1, useColor);
                    picktex.Apply();
                };
            sR.onValueChanged.AddListener(onRChange);
        }
        {
            var sG = PickPanel.Find("SliderG").GetComponent<UnityEngine.UI.Slider>();
            UnityAction<float> onGChange = (v) =>
            {

                useColor.g = v;
                picktex.SetPixel(1, 1, useColor);
                picktex.Apply();
            };
            sG.onValueChanged.AddListener(onGChange);
        }
        {
            var sB = PickPanel.Find("SliderB").GetComponent<UnityEngine.UI.Slider>();
            UnityAction<float> onBChange = (v) =>
            {

                useColor.b = v;
                picktex.SetPixel(1, 1, useColor);
                picktex.Apply();
            };
            sB.onValueChanged.AddListener(onBChange);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
