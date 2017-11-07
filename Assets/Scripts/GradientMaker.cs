using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GradientMaker : MonoBehaviour
{

   public static GradientMaker Instance { get; private set; }

   void Awake()
   {
      Instance = this;
   }

   public void UpdateGradient(Color startColor1, Color endColor1)
   {
      Mesh mesh = GetComponent<MeshFilter>().mesh;
      Vector3[] vertices = mesh.vertices;
      Color[] colors = new Color[vertices.Length];
      for (int i = 0; i < colors.Length; i++)
      {
         if (i % 2 == 0)
            colors[i] = startColor1;
         else
            colors[i] = endColor1;
      }
      mesh.colors = colors;
   }
}