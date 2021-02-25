Shader "LowPoly/SimpleLit_emissive"
{
Properties {
	_TintColor ("Tint Color", Color) = (.5,.5,.5,.5)
	_Emission("Emission", Range(0,1)) = 0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200
	Cull Off
CGPROGRAM
#pragma surface surf Lambert
#pragma vertex vert
fixed4 _TintColor;
fixed _Emission;
struct Input {
	
	float4 color;
};

 void vert (inout appdata_full v, out Input o) {
   UNITY_INITIALIZE_OUTPUT(Input,o);
   o.color = v.color*_TintColor;
 }
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c =  float4(IN.color.r,IN.color.g,IN.color.b,IN.color.a);//_TintColor;
	o.Albedo = c.rgb*_Emission;
	o.Alpha = 1;
	o.Emission =c.rgb*c.a*(1+_Emission);
}
ENDCG
}

Fallback "Legacy Shaders/VertexLit"
}