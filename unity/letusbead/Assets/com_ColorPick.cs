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
        var rC = PickPanel.Find("ColorPick").GetComponent<UnityEngine.UI.RawImage>();
        rC.color = useColor;
        {
            var sR = PickPanel.Find("SliderR").GetComponent<UnityEngine.UI.Slider>();
            UnityAction<float> onRChange = (v) =>
                {

                    useColor.r = v;
                    rC.color = useColor;
                };
            sR.onValueChanged.AddListener(onRChange);
        }
        {
            var sG = PickPanel.Find("SliderG").GetComponent<UnityEngine.UI.Slider>();
            UnityAction<float> onGChange = (v) =>
            {

                useColor.g = v;
                rC.color = useColor;
            };
            sG.onValueChanged.AddListener(onGChange);
        }
        {
            var sB = PickPanel.Find("SliderB").GetComponent<UnityEngine.UI.Slider>();
            UnityAction<float> onBChange = (v) =>
            {

                useColor.b = v;
                rC.color = useColor;
            };
            sB.onValueChanged.AddListener(onBChange);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
