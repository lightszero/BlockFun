using UnityEngine;
using System.Collections;

public class com_pixelEdit : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        edit = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
        edit.filterMode = FilterMode.Point;
        this.GetComponent<MeshRenderer>().material.mainTexture = edit;
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
                if (hit.collider.gameObject == this.gameObject)
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
