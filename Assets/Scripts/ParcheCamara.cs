using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParcheCamara : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
