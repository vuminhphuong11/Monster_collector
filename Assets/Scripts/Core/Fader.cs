using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fader : MonoBehaviour
{
    Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public IEnumerator FadeIn(float fadeInTime)
    {
        yield return image.DOFade(1f,fadeInTime).WaitForCompletion();
    }
    public IEnumerator FadeOut(float fadeOutTime)
    {
        yield return image.DOFade(0f, fadeOutTime).WaitForCompletion();
    }
}
