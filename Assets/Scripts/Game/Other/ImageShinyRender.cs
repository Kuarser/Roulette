using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageShinyRender : MonoBehaviour
{
    public string paramName = "";
    public float minVal = 0;
    public float maxVal = 1;
    public float speed = 0.01f;
    float val = 0;

    Material _textMaterial;
    Material textMaterial
    {
        get
        {
            if (_textMaterial == null)
            {
                if (this.GetComponent<Image>() != null)
                    _textMaterial = this.GetComponent<Image>().material;
                if (this.GetComponent<Text>() != null)
                    _textMaterial = this.GetComponent<Text>().material;
            }
            return _textMaterial;
        }
    }

    void Update()
    {
        if (speed > 0)
        {
            val += speed;
            if (val >= maxVal)
                val = minVal;
        }
        else
        {
            val += speed;
            if (val < minVal)
                val = maxVal;
        }
        textMaterial.SetFloat(paramName, val);
    }
}
