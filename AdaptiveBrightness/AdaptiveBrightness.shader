Shader "Custom/AdaptiveBrightness"
{
    Properties
    {
        
    }
    SubShader
    {
        Pass
        {
            ZTEST OFF
            ZWRITE OFF
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            float avgL;
            float _TargetL;
            float _AdaptingSpeed;

            TEXTURE2D(_GrabbedColorTex);
            SAMPLER(sampler_GrabbedColorTex);            
            half3 RGB2HSL(half3 AColor)
            {
                half R, G, B, Max, Min, del_R, del_G, del_B, del_Max, H, S, L;
                R = AColor.r;       //Where RGB values = 0 ÷ 255
                G = AColor.g;
                B = AColor.b;

                Min = min(R, min(G, B));    //Min. value of RGB
                Max = max(R, max(G, B));    //Max. value of RGB
                del_Max = Max - Min;        //Delta RGB value

                L = (Max + Min) / 2.0;

                if (del_Max == 0)           //This is a gray, no chroma...
                {
                    //H = 2.0/3.0;          //Windows下S值为0时，H值始终为160（2/3*240）
                    H = 0;                  //HSL results = 0 ÷ 1
                    S = 0;
                }
                else                        //Chromatic data...
                {
                    if (L < 0.5) S = del_Max / (Max + Min);
                    else         S = del_Max / (2 - Max - Min);

                    del_R = (((Max - R) / 6.0) + (del_Max / 2.0)) / del_Max;
                    del_G = (((Max - G) / 6.0) + (del_Max / 2.0)) / del_Max;
                    del_B = (((Max - B) / 6.0) + (del_Max / 2.0)) / del_Max;

                    if (R == Max) H = del_B - del_G;
                    else if (G == Max) H = (1.0 / 3.0) + del_R - del_B;
                    else if (B == Max) H = (2.0 / 3.0) + del_G - del_R;

                    if (H < 0)  H += 1;
                    if (H > 1)  H -= 1;
                }              
                return half3(H, S, L);
            }

            half Hue2RGB(half v1, half v2, half vH)
            {
                if (vH < 0) vH += 1;
                if (vH > 1) vH -= 1;
                if (6.0 * vH < 1) return v1 + (v2 - v1) * 6.0 * vH;
                if (2.0 * vH < 1) return v2;
                if (3.0 * vH < 2) return v1 + (v2 - v1) * ((2.0 / 3.0) - vH) * 6.0;
                return (v1);
            }

            half3 HSL2RGB(half H, half S, half L)
            {
                half R, G, B;
                half var_1, var_2;
                if (S == 0)                       //HSL values = 0 ÷ 1
                {
                    R = L*255;                   //RGB results = 0 ÷ 255
                    G = L*255;
                    B = L*255;
                }
                else
                {
                    if (L < 0.5) var_2 = L * (1 + S);
                    else         var_2 = (L + S) - (S * L);

                    var_1 = 2.0 * L - var_2;

                    R = 255 * Hue2RGB(var_1, var_2, H + (1.0 / 3.0));
                    G = 255 * Hue2RGB(var_1, var_2, H);
                    B = 255 * Hue2RGB(var_1, var_2, H - (1.0 / 3.0));
                }
                return half3(R, G, B);
            }
            //---------------------------------------------------------------------------
            

            half4 frag(Varyings i) : SV_Target
            {                
                half4 col = SAMPLE_TEXTURE2D(_GrabbedColorTex, sampler_GrabbedColorTex, i.texcoord);
                
                half3 hsl = RGB2HSL(col);
                
                hsl.b =lerp(0,1,lerp(hsl.b ,pow(hsl.b *_TargetL / avgL,1), saturate(_AdaptingSpeed)));
                                
                return half4(HSL2RGB(hsl.r, hsl.g, hsl.b)/255,1);//lerp(col, dest, 0.5);                
            }
            ENDHLSL
        }
    }
}
