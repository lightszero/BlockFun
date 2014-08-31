using UnityEngine;
using System.Collections;

public class com_pixelEdit : MonoBehaviour
{

    // Use this for initialization

    MeshRenderer mesh = null;
    UnityEngine.UI.Image PanelBack;

    void Start()
    {
        mesh = this.transform.Find("PixelEdit").GetComponent<MeshRenderer>();
        PanelBack = this.transform.Find("PanelBack").GetComponent<UnityEngine.UI.Image>();
        var btnShowBack = this.transform.Find("PanelTop/BtnShowBack").GetComponent<UnityEngine.UI.Button>();
        btnShowBack.onClick.AddListener(() =>
            {
                PanelBack.fillCenter = !PanelBack.fillCenter;
            });
        var btnChangeBack = this.transform.Find("PanelTop/BtnChangeBack").GetComponent<UnityEngine.UI.Button>();
        btnChangeBack.onClick.AddListener(() =>
        {
            PanelBack.color = drawColor;
        }
        );
        edit = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
        Color32[] c = new Color32[width * height];
        for (int i = 0; i < width * height; i++)
        {
            c[i].a = 0;
        }
        edit.SetPixels32(c);

        edit.filterMode = FilterMode.Point;
        edit.wrapMode = TextureWrapMode.Clamp;
        edit.Apply();
        mesh.material.mainTexture = edit;
    }
    public int width = 64;
    public int height = 64;
    public Color drawColor = Color.red;
    public enum Mode
    {
        Draw,
    }
    public Mode mode = Mode.Draw;
    Texture2D edit;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == mesh.gameObject)
                {
                    Vector2 pos = hit.textureCoord;
                    //pos.y = 1.0f - pos.y;

                    int x = (int)(pos.x * (float)width);
                    int y = (int)(pos.y * (float)height);
                    if (x >= width) x = width - 1;
                    if (y >= height) y = height - 1;
                    edit.SetPixel(x, y, drawColor);
                    edit.Apply();
                }
            }


        }
    }
}
