using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HoopSystem : MonoBehaviour
{
    public UnityAction hitHoop;

    private void OnCollisionEnter(Collision collision)
    {
        hitHoop?.Invoke();
    }
}
