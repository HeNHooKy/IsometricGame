using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBackground : MonoBehaviour
{
    public float ColormaticSpeed = 1f;
    public float ColormaticShift = 0.1f;

    Image backgroundImage;

    private void Start()
    {
        backgroundImage = transform.GetComponent<Image>();
        StartCoroutine(Colormat());
    }


    float FunctionColormatic(float x)
    {
        return 1f - (float)((ColormaticShift) * Math.Abs(Math.Sin(Math.PI / 2 * x)));
    }



    IEnumerator Colormat()
    {
        float i = 0;
        while (true)
        {
            Color sColor = backgroundImage.color;

            for(; i <= 1; i += Time.deltaTime * ColormaticSpeed)
            {
                sColor.r = FunctionColormatic(i);
                sColor.g = FunctionColormatic(i);
                sColor.b = FunctionColormatic(i);
                backgroundImage.color = sColor;
                yield return null;
            }

            for(; i >= 0; i -= Time.deltaTime * ColormaticSpeed)
            {
                sColor.r = FunctionColormatic(i);
                sColor.g = FunctionColormatic(i);
                sColor.b = FunctionColormatic(i);
                backgroundImage.color = sColor;
                yield return null;
            }
        }
    }


}
