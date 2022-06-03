using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gene 
{
    public static int NumGenes = 0;

    public float HorizontalSteering;
    public float Rotation;
    public float Time;

    public Gene()
    {
        HorizontalSteering = Random.Range(-1f, 1f);
        Rotation = Random.Range(-1f, 1f);
        Time = Random.Range(0.5f, 1f);

        NumGenes++;
    }
    
    ~Gene()
    {
        NumGenes--;
    }
}
