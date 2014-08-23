using System;
using System.Collections.Generic;

using System.Text;

namespace havefun
{
    class Program
    {
        static void Main(string[] args)
        {
            int w = 80;
            int h = 24;
            Random r = new Random();

            Vector3 t1 = new Vector3(-5,-5,0);
            Vector3 t2 = new Vector3(-5,5,0);
            Vector3 t3 = new Vector3(5,5,0);
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
            Matrix mat = new Matrix();
            mat.m14 = 40;
            mat.m24 = 10;
            float rotate = 0;

            Console.ForegroundColor = ConsoleColor.Yellow;
            while (true)
            {
                rotate += 0.1f;
                mat.m11 = (float)Math.Cos(rotate);
                mat.m12 = (float)-Math.Sin(rotate);
                mat.m21 = (float)Math.Sin(rotate);
                mat.m22 = (float)Math.Cos(rotate);
                Console.Clear();


                
                for (int i = 0; i < vec3.Count / 3;i++ )
                {

                    DrawTri(vec3[i * 3], vec3[i * 3 + 1], vec3[i * 3 + 2],mat,i==0?'.':'0');
                }
          
                System.Threading.Thread.Sleep(100);
            }
        }
        static void DrawTri(Vector3 p0,Vector3 p1,Vector3 p2,Matrix mat,char tag)
        {
            p0 = mat.Transform(p0);
            p1 = mat.Transform(p1);
            p2 = mat.Transform(p2);
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
                    if(PointinTriangle(p0,p1,p2,p))
                    {
                        Console.SetCursorPosition(i, j);

                        Console.Write(tag);
                    }
                }
            }
        }

        class Matrix
        {
            public float m11=1;
            public float m12=0;
            public float m13=0;
            public float m14=0;
            public float m21=0;
            public float m22=1;
            public float m23=0;
            public float m24=0;
            public float m31=0;
            public float m32=0;
            public float m33=1;
            public float m34=0;
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


            public static Vector3 operator- (Vector3 l,Vector3 r)
            {
                return new Vector3(l.x - r.x, l.y - r.y, l.z - r.z);
            }

            public float Dot(Vector3 v)
            {

                return x * v.x + y * v.y + z * v.z;

            }

            public Vector3 Cross(Vector3 v)
            {

                return new Vector3(

                    y * v.z - z * v.y,

                    z * v.x - x * v.z,

                    x * v.y - y * v.x);

            }





        };

        static bool SameSide(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {

            Vector3 AB = B - A;

            Vector3 AC = C - A;

            Vector3 AP = P - A;

            Vector3 v1 = AB.Cross(AC);

            Vector3 v2 = AB.Cross(AP);

            return v1.Dot(v2) >= 0;

        }

        static bool PointinTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {

            return SameSide(A, B, C, P) &&

                SameSide(B, C, A, P) &&

                SameSide(C, A, B, P);

        }

    }
}
