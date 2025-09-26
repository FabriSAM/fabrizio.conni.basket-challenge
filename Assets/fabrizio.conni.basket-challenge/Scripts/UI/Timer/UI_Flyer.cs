using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using TMPro;

public class UI_Flyer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    private GameObject player; // Reference to the player's transform
    private Vector3 startScale;
    private bool isAnimating = false;

    public string Text
    {
        set
        {
            text.text = value;
            gameObject.SetActive(true);
            isAnimating = true;
        }
    }

    private void Start()
    {
        startScale = gameObject.transform.localScale;
    }

    void Update()
    {
        if (!isAnimating) return;

        // Rotazione verso il player, se assegnato
        if (player == null) return;

        Vector3 direction = transform.position - player.transform.position;
        direction.y = 0; // Mantieni la rotazione solo sull'asse Y
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        transform.localScale -= Vector3.one * Time.deltaTime * 0.005f;

        if (transform.localScale.x <= 0)
        {
            isAnimating = false;
            gameObject.SetActive(false);
            gameObject.transform.localScale = startScale;
        }
    }

    public void SetPlayer(GameObject player)
    {
        if (player == null) return;
        this.player = player;
    }
}
