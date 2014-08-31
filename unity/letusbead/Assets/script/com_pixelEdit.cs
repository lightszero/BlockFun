using UnityEngine;
using System.Collections;

public class com_pixelEdit : MonoBehaviour
{

    // Use this for initialization

    Transform pick = null;
    //UnityEngine.UI.Image PanelBack;
    void Start()
    {
        pick = this.transform.Find("PanelBack/PixelEdit");
      

        var PanelBack = this.transform.Find("PanelBack/PanelBack").GetComponent<UnityEngine.UI.RawImage>();
        Texture2D tex = new Texture2D(2, 2,TextureFormat.ARGB32,false);
        Color dc = new Color32(204, 255, 204, 255);
        tex.SetPixel(0, 1, dc);
        tex.SetPixel(1, 1, dc);
        tex.SetPixel(0, 0, dc*0.5f);  
        tex.SetPixel(1, 0, dc*0.5f);
        tex.Apply();
        PanelBack.texture = tex;
        var btnShowBack = this.transform.Find("PanelTop/BtnShowBack").GetComponent<UnityEngine.UI.Button>();
        btnShowBack.onClick.AddListener(() =>
            {
                PanelBack.enabled = !PanelBack.enabled;
            });
        var btnChangeBack = this.transform.Find("PanelTop/BtnChangeBack").GetComponent<UnityEngine.UI.Button>();
        btnChangeBack.onClick.AddListener(() =>
        {
            tex.SetPixel(0, 1,drawColor);
            tex.SetPixel(1, 1, drawColor);
            tex.SetPixel(0, 0, drawColor*0.5f);
            tex.SetPixel(1, 0, drawColor*0.5f);
            tex.Apply();
        }
        );

        SetDrawArea();


        for (int i = 0; i < 5;i++ )
        {
            var btn = this.transform.Find("PanelTop/BtnDS"+i.ToString()).GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() =>
            {
               
                var drawpanel = this.transform.Find("PanelBack/PanelDraw").GetComponent<UnityEngine.UI.RawImage>();
                drawpanel.material.SetTexture("_SharpTex", btn.GetComponent<UnityEngine.UI.RawImage>().texture);
            });
        }

    }

    private void SetDrawArea()
    {
        var drawpanel = this.transform.Find("PanelBack/PanelDraw").GetComponent<UnityEngine.UI.RawImage>();
        edit = new Texture2D(width, height, TextureFormat.Alpha8, false, false);
        Color32[] c = new Color32[width * height];
        for (int i = 0; i < width * height; i++)
        {
            c[i].a = 0;
        }
        edit.SetPixels32(c);

        edit.filterMode = FilterMode.Point;
        edit.wrapMode = TextureWrapMode.Clamp;
        edit.Apply();
        drawpanel.texture = edit;
    }
    public int width = 64;
    public int height = 64;
    public Color drawColor = Color.red;
    public byte drawIndex = 0;

    Texture2D _palette = null;
    public Texture2D palette
    {
        get
        {
            return _palette;
        }
        set
        {
            _palette = value;
            var drawpanel = this.transform.Find("PanelBack/PanelDraw").GetComponent<UnityEngine.UI.RawImage>();
            drawpanel.material.SetTexture("_ColorTex", _palette);
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
    // Update is called once per frame
    void Update()
    {
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

                    int x = (int)(pos.x * (float)width);
                    int y = (int)(pos.y * (float)height);
                    if (x >= width) x = width - 1;
                    if (y >= height) y = height - 1;
                    edit.SetPixel(x, y, new Color32(0,0,0,drawIndex));
                    edit.Apply();
                }
            }


        }
    }
}
