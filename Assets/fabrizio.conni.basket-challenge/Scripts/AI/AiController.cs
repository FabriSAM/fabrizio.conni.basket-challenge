using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct ShootAbility
{
    public string DifficoultyName;
    public AiDifficoulty Difficoulty;
}

[RequireComponent(typeof(Rigidbody))]
public class AiController : MonoBehaviour
{
    [SerializeField] 
    private GameObject HoopCenter;
    [SerializeField]
    private List<ShootAbility> difficoulties;

    private AiDifficoulty currentDifficolutySelected;
    public string Difficoulty { get; private set; }
    private Rigidbody rb;
    private bool hasShot = true;
    private float timeToShot = 1.0f;
    private float currentTime = 0.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }
    private void Update()
    {
        if ( hasShot ) return;

        currentTime += Time.deltaTime;

        if ( currentTime >= timeToShot )
        {
            Shoot();
            currentTime = 0.0f;
        }
    }

    private void ShootTrajectory(ref Vector3 force)
    {
        Vector3 targetPos = HoopCenter.transform.position;
        Vector3 startPos = transform.position;

        // Distanza orizzontale (XZ)
        Vector3 directionXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);
        float d = directionXZ.magnitude;

        // Differenza di altezza
        float h = targetPos.y - startPos.y;

        // Angolo scelto (es. 45°)
        float angle = 75f * Mathf.Deg2Rad;

        // Gravità positiva
        float g = Mathf.Abs(Physics.gravity.y);

        // Formula della velocità
        float v2 = (g * d * d) / (2 * (d * Mathf.Tan(angle) - h) * Mathf.Cos(angle) * Mathf.Cos(angle));
        if (v2 <= 0f) return; // nessuna soluzione valida

        float v = Mathf.Sqrt(v2);

        // Direzione normalizzata sul piano XZ
        Vector3 dirXZ = directionXZ.normalized;

        // Composizione della velocità iniziale
        Vector3 velocity = dirXZ * v * Mathf.Cos(angle) + Vector3.up * (v * Mathf.Sin(angle) + currentDifficolutySelected.arcHeight);

        force = velocity;
    }

    private void Shoot()
    {
        // Forza finale
        Vector3 force = Vector3.zero;
        ShootTrajectory(ref force);
        force.x += UnityEngine.Random.Range(-1f, 1f) * (1f - currentDifficolutySelected.accuracy);
        force.z += UnityEngine.Random.Range(-1f, 1f) * (1f - currentDifficolutySelected.accuracy);

        // Spara
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(force, ForceMode.VelocityChange);
        hasShot = true;
    }

    public void ResetBall()
    {
        hasShot = false;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = SpawnerFactory.GetPosition(HoopCenter.transform.position, UnityEngine.Random.Range(1.2f, 1.5f));
        Vector3 direction = HoopCenter.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }

    public void Init(string diffcoulty)
    {
        Difficoulty = diffcoulty;
        foreach (var value in difficoulties)
        {
            if (value.DifficoultyName == Difficoulty)
            {
                currentDifficolutySelected = value.Difficoulty;
                break;
            }
        }
        hasShot = false;
    }
}
