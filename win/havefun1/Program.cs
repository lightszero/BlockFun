using System;
using System.Collections.Generic;
using System.Text;

namespace havefun
{
    class Program
    {
        class MyVertexPoint//一个VertexShader顶点
        {
            public MyVertexPoint()
            {
                this.position = new Vector3(0, 0, 0);
                this.tag = '0';
            }
            public MyVertexPoint(Vector3 pos, char tag)
            {
                this.position = pos;
                this.tag = tag;
            }
            public Vector3 position;//包含一个坐标
            public char tag;//和一个标志
        }
        class MyShader
        {
            public Matrix mat;
            public MyVertexPoint VertexShader(MyVertexPoint pin)
            {
                MyVertexPoint pout = new MyVertexPoint();
                pout.position = mat.Transform(pin.position);
                pout.tag = pin.tag;
                return pout;
            }
            public char PixelShader(MyVertexPoint pin)
            {
                return pin.tag;
            }
        }
        static void Main(string[] args)
        {
            int w = 80;
            int h = 24;
            Random r = new Random();

            MyVertexPoint t1 = new MyVertexPoint(new Vector3(-5, -5, 0), '0');
            MyVertexPoint t2 = new MyVertexPoint(new Vector3(-5, 5, 0), '0');
            MyVertexPoint t3 = new MyVertexPoint(new Vector3(5, 5, 0), '0');
            MyVertexPoint t11 = new MyVertexPoint(new Vector3(-5, -5, 0), '1');
            MyVertexPoint t12 = new MyVertexPoint(new Vector3(5, -5, 0), '1');
            MyVertexPoint t13 = new MyVertexPoint(new Vector3(5, 5, 0), '1');
            List<MyVertexPoint> vec3 = new List<MyVertexPoint>();
            vec3.Add(t1);
            vec3.Add(t2);
            vec3.Add(t3);
            vec3.Add(t11);
            vec3.Add(t12);
            vec3.Add(t13);

            MyShader shader = new MyShader();
            shader.mat = new Matrix();
            shader.mat.m14 = 40;
            shader.mat.m24 = 10;
            float rotate = 0;

            while (true)
            {
                rotate += 0.1f;
                shader.mat.m11 = (float)Math.Cos(rotate);//旋转
                shader.mat.m12 = (float)-Math.Sin(rotate);
                shader.mat.m21 = (float)Math.Sin(rotate);
                shader.mat.m22 = (float)Math.Cos(rotate);

                Console.Clear();
                for (int i = 0; i < vec3.Count / 3; i++)
                {
                    DrawTri(vec3[i * 3 + 0], vec3[i * 3 + 1], vec3[i * 3 + 2], shader);
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        static void DrawTri(MyVertexPoint _p0, MyVertexPoint _p1, MyVertexPoint _p2, MyShader shader)
        {

            Vector3 p0 = shader.VertexShader(_p0).position;
            Vector3 p1 = shader.VertexShader(_p1).position;
            Vector3 p2 = shader.VertexShader(_p2).position;
            char tag = _p0.tag;
            float minx = Math.Min(p0.x, p1.x);
            float maxx = Math.Max(p0.x, p1.x);
            float miny = Math.Min(p0.y, p1.y);
            float maxy = Math.Max(p0.y, p1.y);
            minx = Math.Min(minx, p2.x);
            maxx = Math.Max(maxx, p2.x);
            miny = Math.Min(miny, p2.y);
            maxy = Math.Max(maxy, p2.y);
            for (int i = (int)minx; i < (int)maxx; i++)
            {
                for (int j = (int)miny; j < (int)maxy; j++)
                {
                    if (i < 0 || i >= 80) continue;
                    if (j < 0 || j >= 25) continue;
                    Vector3 p = new Vector3(i, j, 0);
                    if (PointinTriangle(p0, p1, p2, p))
                    {
                        MyVertexPoint _p = new MyVertexPoint(p, tag);

                        Console.SetCursorPosition(i, j);

                        Console.Write(shader.PixelShader(_p));
                    }
                }
            }
        }

        class Matrix
        {
            public float m11 = 1;
            public float m12 = 0;
            public float m13 = 0;
            public float m14 = 0;
            public float m21 = 0;
            public float m22 = 1;
            public float m23 = 0;
            public float m24 = 0;
            public float m31 = 0;
            public float m32 = 0;
            public float m33 = 1;
            public float m34 = 0;
            public Vector3 Transform(Vector3 src)
            {
                return new Vector3(src.x * m11 + src.y * m12 + src.z * m13 + m14,
                    src.x * m21 + src.y * m22 + src.z * m23 + m24,
                    src.x * m31 + src.y * m32 + src.z * m33 + m34);
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
