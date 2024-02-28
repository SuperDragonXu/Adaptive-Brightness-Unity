using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

[VolumeComponentMenu("Custom/Adaptive Brightness")]
public class AdaptiveBrightnessVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter TargetColor_L = new(0.3f);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
