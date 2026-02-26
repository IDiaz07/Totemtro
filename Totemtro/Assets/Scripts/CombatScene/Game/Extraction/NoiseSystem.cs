using UnityEngine;

public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance;

    void Awake()
    {
        Instance = this;
    }

    public void EmitNoise(Vector2 position, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);

        foreach (var hit in hits)
        {
            INoiseListener listener =
                hit.GetComponent<INoiseListener>();

            if (listener != null)
            {
                listener.OnNoiseHeard(position);
            }
        }
    }
}