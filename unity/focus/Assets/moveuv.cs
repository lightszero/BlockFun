using UnityEngine;
using System.Collections;

public class moveuv : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        mainmat = this.GetComponent<MeshRenderer>().material;
    }
    Material mainmat;

    // Update is called once per frame
    float size = 0.1f;
    void Update()
    {
        Vector3 pos = Input.mousePosition;
        size += Input.mouseScrollDelta.y / 100.0f;
        if (size < 0.1f) size = 0.1f;
        if (size > 0.5f) size = 0.5f;
        mainmat.mainTextureScale = new Vector2(1 / size, 1 / size);
        float xmax = 1 / size;
        float ymax = 1 / size;
        float x = 0.5f - pos.x / Screen.width * xmax;
        float y = 0.5f - pos.y / Screen.height * ymax;
        mainmat.mainTextureOffset = new Vector2(x, y);
    }
}
