using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom Settings/Wind")]
public class WindSettings : ScriptableObject
{
    public float windSpeed;
    public float frequency;
    public float amplitude;
    public Vector2 period;
    public float turbulencePower;
    public float turbulenceSize;
}
