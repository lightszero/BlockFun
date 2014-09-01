using UnityEngine;
using System.Collections;

public class loadbtn : MonoBehaviour {

    public com_pixelEdit edit;
    public com_ColorPick pick;
	// Use this for initialization
	void Start () {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            try
            {
                
                using (System.IO.Stream s = System.IO.File.OpenRead(Application.persistentDataPath + "/temp_p.png"))
                {
                    byte[] bb = new byte[s.Length];
                    s.Read(bb, 0, bb.Length);
                    pick.GetPalette().LoadImage(bb);
                }
                using (System.IO.Stream s = System.IO.File.OpenRead(Application.persistentDataPath + "/temp_i.png"))
                {
                    byte[] bb = new byte[s.Length];
                    s.Read(bb, 0, bb.Length);
                    edit.edit.LoadImage(bb);
                }
            }
            catch
            {

            }
          
        });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
