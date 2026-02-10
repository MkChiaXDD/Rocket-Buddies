using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private string sceneToLoad = "GameScene";

    [Header("Fake Loading Settings")]
    [SerializeField] private float fakeLoadDuration = 2.5f; // how long fake bar takes
    [SerializeField] private float endHoldTime = 0.4f;      // pause at 100%

    private float fakeProgress;
    private bool realLoadDone;

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        fakeProgress = 0f;
        realLoadDone = false;

        while (!op.isDone)
        {
            // REAL loading check
            if (op.progress >= 0.9f)
                realLoadDone = true;

            // FAKE progress moves smoothly to 1
            fakeProgress += Time.deltaTime / fakeLoadDuration;
            fakeProgress = Mathf.Clamp01(fakeProgress);

            // Combine fake + real (fake leads, real caps it)
            float displayProgress = Mathf.Min(fakeProgress, realLoadDone ? 1f : 0.9f);

            if (progressBar != null)
                progressBar.value = displayProgress;

            // Only finish when BOTH are ready
            if (fakeProgress >= 1f && realLoadDone)
            {
                yield return new WaitForSeconds(endHoldTime);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
