// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx
// Modified by Peisen Xu and Nanyang Yang 

#include "UnityCG.cginc"
#include "Common.cginc"

// Uniforms
half4 _Tint;
half _PointSize = 0.01f;
float4x4 _Transform;

half _ModelScalingFactor;
float _PointScalingFactor;
int _ShaderInterpolation;
int _AdaptivePoint;


#if _COMPUTE_BUFFER
StructuredBuffer<float4> _PointBuffer;
#endif


// Vertex input attributes
struct Attributes
{
	float4 uv: TEXCOORD1;
#if _COMPUTE_BUFFER
    uint vertexID : SV_VertexID;
#else
    float4 position : POSITION;
    half3 color : COLOR;
#endif
};


// Fragment varyings
struct Varyings
{
    float4 position : SV_POSITION;
	float4 uv: TEXCOORD1;
#if !PCX_SHADOW_CASTER
    half3 color : COLOR;
    UNITY_FOG_COORDS(2)
#endif
};


struct VertOut
{
	float4 position : SV_POSITION;
	float4 viewposition: TEXCOORD1;
	float2 uv : TEXCOORD0;
	float size : POINTSIZE;
#if !PCX_SHADOW_CASTER
	half3 color : COLOR;
	UNITY_FOG_COORDS(2)
#endif
};

struct FragOut {
	half4 color : SV_Target;
	float depth : SV_Depth;
};

float4x4 inverse(float4x4 input)
{
#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))

	float4x4 cofactors = float4x4(
		minor(_22_23_24, _32_33_34, _42_43_44),
		-minor(_21_23_24, _31_33_34, _41_43_44),
		minor(_21_22_24, _31_32_34, _41_42_44),
		-minor(_21_22_23, _31_32_33, _41_42_43),

		-minor(_12_13_14, _32_33_34, _42_43_44),
		minor(_11_13_14, _31_33_34, _41_43_44),
		-minor(_11_12_14, _31_32_34, _41_42_44),
		minor(_11_12_13, _31_32_33, _41_42_43),

		minor(_12_13_14, _22_23_24, _42_43_44),
		-minor(_11_13_14, _21_23_24, _41_43_44),
		minor(_11_12_14, _21_22_24, _41_42_44),
		-minor(_11_12_13, _21_22_23, _41_42_43),

		-minor(_12_13_14, _22_23_24, _32_33_34),
		minor(_11_13_14, _21_23_24, _31_33_34),
		-minor(_11_12_14, _21_22_24, _31_32_34),
		minor(_11_12_13, _21_22_23, _31_32_33)
		);
#undef minor
	return transpose(cofactors) / determinant(input);
}


// Vertex phase
Varyings Vertex(Attributes input)
{
    // Retrieve vertex attributes.
	half3 col;

#if _COMPUTE_BUFFER
	float4 pt = _PointBuffer[input.vertexID];
	float4 pos = mul(_Transform, float4(pt.xyz, 1));
	col = PcxDecodeColor(asuint(pt.w));
#else
	float4 pos = input.position;
	col = input.color;
#endif


#if !PCX_SHADOW_CASTER
    // Color space convertion & applying tint
    #if UNITY_COLORSPACE_GAMMA
        col *= _Tint.rgb * 2;
    #else
        col *= LinearToGammaSpace(_Tint.rgb) * 2;
        col = GammaToLinearSpace(col);
    #endif
#endif

    // Set vertex output.
    Varyings o;
    o.position = UnityObjectToClipPos(pos);
	o.uv = input.uv;
#if !PCX_SHADOW_CASTER
    o.color = col;
    UNITY_TRANSFER_FOG(o, o.position);
#endif
    return o;
}


// Geometry phase
[maxvertexcount(4)]
void Geometry(point Varyings input[1], inout TriangleStream<VertOut> outStream)
{
	if (input[0].uv.x == 0 || _AdaptivePoint < 0.5) {

	}
	else {
		_PointSize = input[0].uv.x;
	}

    float4 origin = input[0].position;
	float2 extent = abs(UNITY_MATRIX_P._11_22 * _PointSize * _PointScalingFactor * _ModelScalingFactor);
	float4x4 _InverseProjMatrix = inverse(UNITY_MATRIX_P);

	// Copy the basic information.
    Varyings o = input[0];
	VertOut vo;
	vo.size = _PointSize;
#if !PCX_SHADOW_CASTER
	vo.color = o.color;
	UNITY_TRANSFER_FOG(vo, o.position);
#endif

    vo.position.y = origin.y + extent.y;
	vo.position.x = origin.x - extent.x;
    vo.position.zw = origin.zw;
	vo.uv = float2(-1.0f, 1.0f);
	vo.viewposition = mul(_InverseProjMatrix, vo.position);
    outStream.Append(vo);

	vo.position.y = origin.y + extent.y;
	vo.position.x = origin.x + extent.x;
	vo.position.zw = origin.zw;
	vo.uv = float2(1.0f, 1.0f);
	vo.viewposition = mul(_InverseProjMatrix, vo.position);
	outStream.Append(vo);

	vo.position.y = origin.y - extent.y;
	vo.position.x = origin.x - extent.x;
	vo.position.zw = origin.zw;
	vo.uv = float2(-1.0f, -1.0f);
	vo.viewposition = mul(_InverseProjMatrix, vo.position);
	outStream.Append(vo);

	vo.position.y = origin.y - extent.y;
	vo.position.x = origin.x + extent.x;
	vo.position.zw = origin.zw;;
	vo.uv = float2(1.0f, -1.0f);
	vo.viewposition = mul(_InverseProjMatrix, vo.position);
	outStream.Append(vo);

    //outStream.RestartStrip();
}


FragOut Fragment(VertOut input)
{
	FragOut o;
	half4 c;
#if PCX_SHADOW_CASTER
	c = 0;
#else
    c = half4(input.color, _Tint.a);
    UNITY_APPLY_FOG(input.fogCoord, c);
#endif
	
	if (_ShaderInterpolation > 0.5) {
		float uvlen = input.uv.x*input.uv.x + input.uv.y*input.uv.y;
		input.viewposition.z += (1 - uvlen) * input.size * _PointScalingFactor * _ModelScalingFactor;
	}
	
	float4 pos = mul(UNITY_MATRIX_P, input.viewposition);
	pos /= pos.w;

	o.color = c;
	o.depth = pos.z;
	return o;
}
