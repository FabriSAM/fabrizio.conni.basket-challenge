using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HoopSystem : MonoBehaviour
{
    public UnityAction<int> hitHoop;

    private void OnCollisionEnter(Collision collision)
    {
        int index = -1;

        switch (collision.gameObject.tag)
        {
            case "Player":
                index = 0;
                break;
            case "Enemy":
                index = 1;
                break;
            default:
                break;
        }

        hitHoop?.Invoke(index);
    }
}
