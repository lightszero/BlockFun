using UnityEngine;
using System.Collections;

public class savebtn : MonoBehaviour
{

    public com_pixelEdit edit;
    // Use this for

    void Start() {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
           {
               byte[] palette = edit.palette.EncodeToPNG();
               byte[] index = edit.edit.EncodeToPNG();
               var pc = edit.palette.GetPixels32(0);
               var ic = edit.edit.GetPixels32(0);
               Color32[] colordata = new Color32[edit.edit.width * edit.edit.height];
               for (int i = 0; i < ic.Length; i++)
               {
                   colordata[i] = pc[ic[i].a];
               }
               Texture2D tex = new Texture2D(edit.edit.width, edit.edit.height, TextureFormat.ARGB32, false);
               tex.SetPixels32(colordata);
               tex.Apply();
               var color = tex.EncodeToPNG();
               Object.Destroy(tex);
               using (System.IO.Stream s = System.IO.File.Create(Application.temporaryCachePath + "/temp_c.png"))
               {
                   s.Write(color, 0, color.Length);
               }
               using (System.IO.Stream s = System.IO.File.Create(Application.temporaryCachePath + "/temp_p.png"))
               {
                   s.Write(palette, 0, palette.Length);
               }
               using (System.IO.Stream s = System.IO.File.Create(Application.temporaryCachePath + "/temp_i.png"))
               {
                   s.Write(index, 0, index.Length);
               }
           });
	}

    // Update is called once per frame
    void Update()
    {

    }
}
