using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FailSystem : MonoBehaviour
{
    public UnityAction onFail;
    private void OnCollisionEnter(Collision collision)
    {
        onFail?.Invoke();
    }
}
