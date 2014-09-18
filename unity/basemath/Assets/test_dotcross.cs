using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class test_dotcross : MonoBehaviour {

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
        //红音军团
        for(int i=0;i<10;i++)
        {
            hongyinarmy army = new hongyinarmy();
            army.obj = GameObject.Instantiate(hong) as GameObject;
            army.obj.transform.parent = objhong.transform;
            float x = Random.Range(0, 2.0f) - 1;
            float y = Random.Range(0, 2.0f) - 1;
            army.pos = new Vector3(x, y, -1);
            army.obj.transform.localScale = new Vector3(0.2f, 0.2f);
            arrayhong.Add(army);
        }
	}
    class hongyinarmy
    {
        public GameObject obj;
        public Vector3 pos;
    }
    
    List<MeshRenderer> arraycang = new List<MeshRenderer>();

    List<hongyinarmy> arrayhong = new List<hongyinarmy>();
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

        Vector3 hongright = Vector3.Cross(honglookat, new Vector3(0,0,-1));//求出红音的侧面
        Debug.Log(hongright);
        foreach(var hong in arrayhong)
        {


            hong.obj.transform.position = honglookat*hong.pos.y + hongright*hong.pos.x;
        }
	}
}
