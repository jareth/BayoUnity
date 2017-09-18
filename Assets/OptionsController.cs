using System;
using UnityEngine;

public class OptionsController : MonoBehaviour
{
    public float DebugUIScale = 1.0f;

    public bool BounancyEnabled = true;
    public bool BounancyDebugEnabled = true;
    
    public bool ViscosityEnabled = true;
    public bool ViscosityDebugEnabled = true;
    
    public bool PressureEnabled = true;
    public bool PressureDebugEnabled = true;
    public PressureOptions PressureOptions;
    
    public bool SlammingEnabled = true;
    public bool SlammingDebugEnabled = true;
    public SlammingOptions SlammingOptions;
}

[Serializable]
public class PressureOptions
{
    public float ReferenceSpeed = 1f;
    
    public float PressureCoefficient1 = 10f;
    public float PressureCoefficient2 = 10f;
    public float PressureFalloff = 0.5f;
    
    public float SuctionCoefficient1 = 10f;
    public float SuctionCoefficient2 = 10f;
    public float SuctionFalloff = 0.5f;
}

[Serializable]
public class SlammingOptions
{
    public float RampingPower = 4f;
    public float SlammingCheat = 4f;
}
