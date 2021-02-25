Shader "LowPoly/SimpleLit"
{
Properties {
	_TintColor ("Tint Color", Color) = (.5,.5,.5,.5)

}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200

CGPROGRAM
#pragma surface surf Lambert
#pragma vertex vert
fixed4 _TintColor;

struct Input {
	float2 uv_MainTex;
	float3 color;
};

 void vert (inout appdata_full v, out Input o) {
   UNITY_INITIALIZE_OUTPUT(Input,o);
   o.color = v.color*_TintColor;
 }
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c =  float4(IN.color.r,IN.color.g,IN.color.b,1);//_TintColor;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Legacy Shaders/VertexLit"
}