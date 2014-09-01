using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class savebtn : MonoBehaviour
{

    public com_pixelEdit edit;
    // Use this for
    public UnityEngine.UI.InputField input;
    void Start() {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
           {
               List<byte> bytes = new List<byte>();
               var pc = edit.palette.GetPixels32(0);
               var ic = edit.edit.GetPixels32(0);
               Dictionary<byte, Color32> usep = new Dictionary<byte, Color32>();
               bytes.Add((byte)edit.edit.width);
               bytes.Add((byte)edit.edit.height);
               for (int i = 0; i < ic.Length; i++)
               {
                   byte ind= ic[i].a;
                   bytes.Add(ind);
                   if(ind>0)
                   {
                       if(usep.ContainsKey(ind)==false)
                       {
                           usep.Add(ind,pc[ind]);
                       }
                   }
               }
               bytes.Add((byte)usep.Count);
               foreach(var up in usep)
               {
                   bytes.Add(up.Key);
                   bytes.Add(up.Value.r);
                   bytes.Add(up.Value.g);
                   bytes.Add(up.Value.b);
               }

               var s =           LZMAHelper.Compress(new System.IO.MemoryStream(bytes.ToArray()), (uint)bytes.Count);
               byte[] nb = new byte[s.Length];
               s.Read(nb, 0, nb.Length);
               string str = System.Convert.ToBase64String(nb);
               string str2 = System.Uri.EscapeDataString(str);
               input.value = str2;
               Debug.Log(input.value.Length);
           });
        input.onSubmit.AddListener((str)=>
        {
            ReadByte(str, edit.edit, edit.palette);

        });
	}
    public static void ReadByte(string scode,Texture2D src,Texture2D p)
    {
        try
        {
            string strbase64 = System.Uri.UnescapeDataString(scode);
            byte[] bb = System.Convert.FromBase64String(strbase64);
            var s = LZMAHelper.DeCompress(new System.IO.MemoryStream(bb), (uint)bb.Length);
            bb = new byte[s.Length];
            s.Read(bb, 0, bb.Length);

            int seek = 0;
            int width = bb[seek]; seek++;
            if (width == 0) width = 256;
            int height = bb[seek]; seek++;
            if (height == 0) height = 256;
            Debug.Log("w=" + width + ",h=" + height);
            Color32[] pp = src.GetPixels32(0);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte ind = bb[seek]; seek++;
                    pp[y * width + x].a = ind;
                }
            }
            src.SetPixels32(pp, 0);
            src.Apply();
            Color32[] ppp = p.GetPixels32(0);
            int c = bb[seek]; seek++;
            for (int i = 0; i < c; i++)
            {
                byte inde = bb[seek]; seek++;
                byte r = bb[seek]; seek++;
                byte g = bb[seek]; seek++;
                byte b = bb[seek]; seek++;
                ppp[inde].r = r;
                ppp[inde].g = g;
                ppp[inde].b = b;

            }
            p.SetPixels32(ppp);
            p.Apply();
        }
        catch
        {

        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
