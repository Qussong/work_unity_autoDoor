using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;

public static class CanvasGroupExtensions
{
    public static IEnumerator FadeIn(this CanvasGroup canvasGroup, float duration, Action onComplete = null)
    {
        return Fade(canvasGroup, 1f, duration, onComplete);
    }

    public static IEnumerator FadeOut(this CanvasGroup canvasGroup, float duration, Action onComplete = null)
    {
        return Fade(canvasGroup, 0f, duration, onComplete);
    }

    public static IEnumerator Fade(this CanvasGroup canvasGroup, float targetAlpha, float duration, Action onComplete = null)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0f;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;

        onComplete?.Invoke();
    }

    public static void Activate(this CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public static void DeActivate(this CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

}