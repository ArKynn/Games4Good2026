using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIImageAnimation : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 12f;
    [SerializeField] private bool hideWhenFinished = true;

    public void PlayAnimation()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        float delay = 1f / fps;

        for (int i = 0; i < frames.Length; i++)
        {
            targetImage.sprite = frames[i];
            yield return new WaitForSeconds(delay);
        }

        if (hideWhenFinished)
            gameObject.SetActive(false);
    }
}