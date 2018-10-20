Shader "Unlit/Texture" {
   Properties {
       _MainTex ("MainTex", 2D) = "black" {}
   }
   SubShader {
       Tags { "RenderType"="Opaque" }
       LOD 100
       Pass {
           Lighting Off
           SetTexture [_MainTex] { combine texture }
       }
   }
}
