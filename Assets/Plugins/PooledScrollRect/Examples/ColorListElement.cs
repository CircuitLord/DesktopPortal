using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ColorListElement : PooledScrollRectElement<Color> {

    public UnityEngine.UI.Image Img;

    public override void Setup(int placement, List<Color> allData)
    {
        Img.color = allData[placement];
    }
}
