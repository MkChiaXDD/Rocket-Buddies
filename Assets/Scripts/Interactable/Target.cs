using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    private bool isHit = false;
    public void OnHit()
    {
        if (isHit) return;

        isHit = true;
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        StartCoroutine(Reset());
    }

    public bool GetIsHit()
    {
        return isHit;
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(2f);

        isHit = false;
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }
}
