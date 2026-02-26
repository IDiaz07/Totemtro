using UnityEngine;
using System.Collections;

public class LevelUpEffect : MonoBehaviour
{
    public GameObject levelUpPanel;

    public void Play()
    {
        StartCoroutine(LevelRoutine());
    }

    IEnumerator LevelRoutine()
    {
        Time.timeScale = 0f;

        levelUpPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(1f);

        levelUpPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
