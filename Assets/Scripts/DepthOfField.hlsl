#ifndef PPSDOF_FX
#define PPSDOF_FX

TEXTURE2D_X(_MainTex);
TEXTURE2D_X(_TexReal);
TEXTURE2D_X(_TexImag);
TEXTURE2D_X(_TexOther);

float4 _MainTex_TexelSize;
float4 _Params;
float4 _BlurParams;


float4 Merge(Varyings i) : SV_Target {
    float4 p1 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv);
    float4 p2 = SAMPLE_TEXTURE2D_X(_TexOther, sampler_LinearClamp, i.uv);
    float4 pix = _Params.x * p1 + _Params.y * p2;
    return float4(pix.rgb, 1);
}

float4 Exposure(Varyings i) : SV_Target {
    float4 pix = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv);
    pix = pow(abs(pix), _Params.x);
    return float4(pix.rgb, 1);
}

float4 PassHorizontal(Varyings i) : SV_Target {

    float4 pix;
    int radius = _BlurParams.x;

    for (int j = 0; j < 2 * radius + 1; ++j)
    {
        float x = 1.2 * float(j - radius) / radius;
        float2 o = float2(_MainTex_TexelSize.x * (j - radius), 0);
        float4 p = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv + o);

        float2 k = exp(-_Params.x * x * x) * float2(
            cos(_Params.y * x * x),
            sin(_Params.y * x * x)
        );
     
        pix += _BlurParams.y * (k.x * _Params.z + k.y * _Params.w) * p;
    }
 
    return float4(pix.rgb, 1);
}

float4 PassVertical(Varyings i) : SV_Target {

    float4 pix;
    int radius = _BlurParams.x;

    for (int j = 0; j < 2 * radius + 1; ++j)
    {
        float x = 1.2 * float(j - radius) / radius;
        float2 o = float2(0, _MainTex_TexelSize.y * (j - radius));
        float4 p_real = SAMPLE_TEXTURE2D_X(_TexReal, sampler_LinearClamp, i.uv + o);
        float4 p_imag = SAMPLE_TEXTURE2D_X(_TexImag, sampler_LinearClamp, i.uv + o);

        float2 k = exp(-_Params.x * x * x) * float2(
            cos(_Params.y * x * x),
            sin(_Params.y * x * x)
        );
        float4 k_real = k.xxxx;
        float4 k_imag = k.yyyy;

        float4 pix_real = k_real * p_real - k_imag * p_imag;
        float4 pix_imag = k_real * p_imag + k_imag * p_real;
     
        pix += _BlurParams.y * (pix_real * _Params.z + pix_imag * _Params.w);
    }
 
    return float4(pix.rgb, 1);
}

float4 Gauss(Varyings i) : SV_Target {
    float4 pix;
    int radius = _BlurParams.x;

    for (int j = 0; j < 2 * radius + 1; ++j)
    {
        float x = float(j - radius) / radius;
        float2 o = float2(
            _MainTex_TexelSize.x * (j - radius) * _Params.z,
            _MainTex_TexelSize.y * (j - radius) * _Params.w
        );
        float4 p = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv + o);

        float k = exp(-_Params.x * x * x);
     
        pix += _BlurParams.y * k * p;
    }
 
    return float4(pix.rgb, 1);
}

#endif