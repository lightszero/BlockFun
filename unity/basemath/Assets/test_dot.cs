using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class test_dot : MonoBehaviour {

	// Use this for initialization
	void Start () {

        var cang = Resources.Load<GameObject>("QuadCang");
        GameObject obj =new GameObject("cangs");
        //摆一屏幕的苍老师
        for(int i=0;i<300;i++)
        {
            float x = Random.Range(0, 10.0f) - 5;
            float y = Random.Range(0, 10.0f) - 5;
            var c = GameObject.Instantiate(cang) as GameObject;
            c.transform.parent = obj.transform;
            c.transform.position = new Vector3(x, y, 0);
            c.transform.localScale = new Vector3(0.2f, 0.2f);
            var  m = c.GetComponent<MeshRenderer>().material;
            c.GetComponent<MeshRenderer>().material = new Material(m);
            arraycang.Add(c.GetComponent<MeshRenderer>());
        }

        //摆一个红音老师
        var hong = Resources.Load<GameObject>("QuadHong");
        var objhong =GameObject.Instantiate(hong) as GameObject;
	}
    
    List<MeshRenderer> arraycang = new List<MeshRenderer>();
	// Update is called once per frame
	void Update () {
        var ray =  Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 honglookat = (ray.origin-Vector3.zero);//红音之目光
        honglookat.z = 0;
        honglookat.Normalize();

    
        foreach(var cang in arraycang)
        {
            Vector3 cangNormal = (cang.transform.position - Vector3.zero).normalized;//苍井之方向
            if(Vector3.Dot(cangNormal,honglookat)<Mathf.Cos(Mathf.PI/6.0f))
            {
                cang.material.color = Color.red;//背后苍井红眼
            }
            else
            {
                cang.material.color = Color.white;//当面正常
            }
        }
	}
}
