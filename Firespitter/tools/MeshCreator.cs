using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Firespitter.tools
{
    class MeshCreator
    {
        public static Mesh createDisc(float radius, int sides) // Based off SirJodelsteins cone creator in Peristent Trails
        {
            Mesh disc = new Mesh();

            //create vertices
            Vector3[] vertices = new Vector3[sides + 1];
            vertices[0] = new Vector3(0f, 0f, 0f);

            float angleIncrement = Mathf.PI * 2 / sides;

            for (int i = 1; i <= sides; ++i)
            {
                float angle = angleIncrement * i; //angle from 0 to 2PI
                vertices[i] = new Vector3((float)Math.Cos(angle) * radius,
                                          (float)Math.Sin(angle) * radius, 0f);
                Debug.Log("vert " + i + " : " + vertices[i]);
            }

            //create triangles (three indices per face)
            int[] triangles = new int[sides * 3];
            
            //base
            for (int i = 0; i < sides; ++i)
            {
                if (i < sides - 1)
                {
                    Debug.Log("i == " + i + " / tri: 0, " + (i + 1) + ", " + (i + 2));
                    triangles[3 * i + 0] = 0; //first corner is always the top
                    triangles[3 * i + 1] = i + 1;
                    triangles[3 * i + 2] = i + 2;
                }
                else
                {
                    Debug.Log("i == " + i + " / tri: 0, " + (i + 1) + ", " + 1);
                    triangles[3 * i + 0] = 0; //first corner is always the top
                    triangles[3 * i + 1] = i + 1;
                    triangles[3 * i + 2] = 1;
                }
            }            

            //put it all together
            disc.vertices = vertices;
            disc.triangles = triangles;

            //Auto-Normals
            disc.RecalculateNormals();

            return disc;
        }        
    }
}
