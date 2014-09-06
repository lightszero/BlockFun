using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveShowsTest : MonoBehaviour
{

    public waveshow show;
    public UnityMidiSynth midi;
    // Use this for initialization
    List<string> files = new List<string>();
    void Start()
    {


        //if (Application.isEditor)
        //{
            
        //    var _files = System.IO.Directory.GetFiles(Application.dataPath + "/resources/Midis", "*.mid.txt");
        //    string outstr = "";
        //    foreach (var _f in _files)
        //    {
        //        string ff = "Midis/" + System.IO.Path.GetFileNameWithoutExtension(_f);
        //        files.Add(ff);
        //        outstr += ff + "\n";
        //    }
        //    using (var s = System.IO.File.Create(Application.dataPath + "/resources/midi.list.txt"))
        //    {
        //        byte[] bb = System.Text.Encoding.UTF8.GetBytes(outstr);
        //        s.Write(bb, 0, bb.Length);
        //    }
           
        //}
        //else
        {
            string midis = Resources.Load<TextAsset>("midi.list").text;
            string[] _files = midis.Split(new string[] { "\r", "\n" },System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var _f in _files)
            {
                files.Add(_f);
            }
        }
    }
    Vector2 scrollPos;
    void OnGUI()
    {
        float[] d = show.GetDataSafe();
        float sheight = Screen.height;
        float ss = sheight / 100;
        float sw = (float)Screen.width / (float)d.Length;
        for (int x = 0; x < d.Length; x++)
        {
            float sh = d[x] * 10 * ss + 10;
            GUI.Button(new Rect(x * sw, sheight - sh, sw, sh), "");
        }
        GUI.Label(new Rect(50, 50, 500, 100), "Use Midi,Less than 1 MB for playing music a hour. \n使用Midi一个小时的音乐也超不过1M.");
        scrollPos = GUI.BeginScrollView(new Rect(100, 100, Screen.width - 200, Screen.height - 200), scrollPos, new Rect(0, 0, Screen.width - 240, 50 * files.Count));

        for (int y = 0; y < files.Count; y++)
        {
            if (GUI.Button(new Rect(100, y * 50, Screen.width - 350, 50), files[y]))
            {
                midi.StopAll();
                midi.Play(files[y]);
                Debug.Log("ppp");
            }
        }
        GUI.EndScrollView();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
