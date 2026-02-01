using UnityEngine;

public class SlashVFX : MonoBehaviour
{
    public float lifetime = 0.15f;
    public float offset = 0.8f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Play(Vector2 origin, Vector2 direction)
    {
        transform.position = origin + direction * offset;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float scale = Random.Range(0.9f, 1.1f);
        transform.localScale = Vector3.one * scale;

        transform.position += Vector3.back * 0.01f;

        if (direction.x < 0)
            sr.flipY = true;
        else
            sr.flipY = false;

        Destroy(gameObject, lifetime);
    }
}
