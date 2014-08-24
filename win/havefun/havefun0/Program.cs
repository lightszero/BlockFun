using System;
using System.Collections.Generic;
using System.Text;
namespace havefun
{
    class Program
    {
        static void Main(string[] args)
        {
            Vector3 t1 = new Vector3(0, 0, 0);
            Vector3 t2 = new Vector3(0, 10, 0);
            Vector3 t3 = new Vector3(15, 5, 0);
            Vector3 t11 = new Vector3(-5, -5, 0);
            Vector3 t12 = new Vector3(5, -5, 0);
            Vector3 t13 = new Vector3(5, 5, 0);
            List<Vector3> vec3 = new List<Vector3>();
            vec3.Add(t1);
            vec3.Add(t2);
            vec3.Add(t3);
            vec3.Add(t11);
            vec3.Add(t12);
            vec3.Add(t13);
            while (true)//主循环
            {
                Console.Clear();//清除画面
                for (int i = 0; i < vec3.Count / 3; i++)//绘制模型
                {
                    DrawTri(vec3[i * 3], vec3[i * 3 + 1], vec3[i * 3 + 2], i%2 == 0 ? '.' : '0');
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        static void DrawTri(Vector3 p0, Vector3 p1, Vector3 p2, char tag)
        {
            float minx = Math.Min(p0.x, p1.x);//求出三角形的包围盒
            float maxx = Math.Max(p0.x, p1.x);
            float miny = Math.Min(p0.y, p1.y);
            float maxy = Math.Max(p0.y, p1.y);
            minx = Math.Min(minx, p2.x);
            maxx = Math.Max(maxx, p2.x);
            miny = Math.Min(miny, p2.y);
            maxy = Math.Max(maxy, p2.y);
            for (int i = (int)minx; i < (int)maxx; i++)//扫描包围盒
            {
                for (int j = (int)miny; j < (int)maxy; j++)
                {
                    if (i < 0 || i >= 80) continue;//超出屏幕的不画
                    if (j < 0 || j >= 25) continue;
                    Vector3 p = new Vector3(i, j, 0);
                    if (PointinTriangle(p0, p1, p2, p))//在三角型内？
                    {
                        Console.SetCursorPosition(i, j);//画一个字符（替代像素）
                        Console.Write(tag);
                    }
                }
            }
        }
        class Vector3
        {
            public Vector3(float fx, float fy, float fz)
            {
                x = fx;
                y = fy;
                z = fz;
            }
            public float x;
            public float y;
            public float z;
            public static Vector3 operator -(Vector3 l, Vector3 r)
            {
                return new Vector3(l.x - r.x, l.y - r.y, l.z - r.z);
            }
            public float Dot(Vector3 v)//求点积
            {
                return x * v.x + y * v.y + z * v.z;
            }
            public Vector3 Cross(Vector3 v)//求差积
            {
                return new Vector3(y * v.z - z * v.y, z * v.x - x * v.z, x * v.y - y * v.x);
            }
        };
        static bool SameSide(Vector3 A, Vector3 B, Vector3 C, Vector3 P)//判断点P是否在边AB的C一侧
        {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AP = P - A;
            Vector3 v1 = AB.Cross(AC);
            Vector3 v2 = AB.Cross(AP);
            return v1.Dot(v2) >= 0;
        }
        static bool PointinTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)//判断点P是否在三角形ABC中
        {
            return SameSide(A, B, C, P) && SameSide(B, C, A, P) && SameSide(C, A, B, P);
        }
    }
}
