
using UnityEngine;
public static class SpawnerFactory
{
    public static Vector3 GetPosition(Vector3 center, float radius)
    {
        float angle = Random.Range(-60, 60);
        float radiant = angle * Mathf.Deg2Rad;

        return new Vector3(
            center.x + radius * Mathf.Cos(radiant),
            0.5f,
            center.z + radius * Mathf.Sin(radiant)
        );
    }
}
