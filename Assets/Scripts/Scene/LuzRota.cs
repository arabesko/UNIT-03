using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuzRota : MonoBehaviour
{
    public bool _titilo = false;
    public float _timeDelay;

    void Update()
    {
        if(_titilo == false)
        {
            StartCoroutine(LuzTitilante());
        }
    }

    IEnumerator LuzTitilante()
    {
        _titilo = true;
        this.gameObject.GetComponent<Light>().enabled = false;
        _timeDelay = Random.Range(0.01f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);
        this.gameObject.GetComponent<Light>().enabled = true;
        _timeDelay = Random.Range(0.02f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);
        _titilo = false;
    }
}
