using UnityEngine;
using System.Collections;


public class GetURL : MonoBehaviour
{
    public com_pixelEdit edit;
    // Use this for
    public UnityEngine.UI.InputField input;
    public static string fullUrl;
    void Start() {
        Application.ExternalEval("u.getUnity().SendMessage(\"" + name + "\", \"ReceiveURL\", document.URL);");
    }
 
    public void ReceiveURL(string url) {
        // this will include the full URL, including url parameters etc.
        fullUrl = url;
        int i = fullUrl.IndexOf('#');
        if(i<=0)return;

        input.text = fullUrl.Substring(i + 1);
        fullUrl = fullUrl.Substring(0, i);
        savebtn.ReadByte(input.text, edit);
    }
}