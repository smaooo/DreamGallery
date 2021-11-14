using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitSign : MonoBehaviour
{
    private float timer = -1;
    public Material neonMat;

    void Start()
    {
        neonMat.SetColor("_EmissionColor", Color.red);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1 )
        {
            timer = -1;
        }

        for (int i = 0; i < transform.childCount - 1; i ++)
        {
            transform.GetChild(i).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.red * timer);
        }
    }
}
