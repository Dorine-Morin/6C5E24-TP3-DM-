using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCanvas : MonoBehaviour
{
    [SerializeField]
    Canvas Intro;
    [SerializeField]
    Canvas Video;

    public void Start()
    {
        Video.enabled = false;
        Intro.enabled = true;
    }

    public void toggleIntroVideo()
    {
        Intro.enabled = !Intro.enabled;
        Video.enabled = !Intro.enabled;
    }

}
