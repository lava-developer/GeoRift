using System.Collections;
using UnityEngine;

public class ObstacleTemp : MonoBehaviour
{
    [SerializeField] float flickerInterval = 1f;

    GameObject obstacleObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        obstacleObject = transform.GetChild(0).gameObject;

        StartCoroutine(Flickering());
    }

    IEnumerator Flickering()
    {
        while (true)
        {
            obstacleObject.SetActive(true);
            yield return new WaitForSeconds(flickerInterval);
            obstacleObject.SetActive(false);
            yield return new WaitForSeconds(flickerInterval);
        }
    }
}
