using System.Collections;
using UnityEngine;

public class MagicCircleVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform outerRing;
    [SerializeField] private Transform innerRune;

    [Header("Rotation")]
    [SerializeField] private float outerRotationSpeed = 20f;
    [SerializeField] private float innerRotationSpeed = -35f;

    [Header("Lifecycle")]
    [SerializeField] private float appearDuration = 0.6f;
    [SerializeField] private float stayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Vector3 targetScale;

    private Material outerMaterial;
    private Material innerMaterial;

    private void Awake()
    {
        targetScale = transform.localScale;

        // 一開始縮到 0，等等再展開。
        transform.localScale = Vector3.zero;

        // 取得獨立材質實例，避免改到 Project 裡的共用 Material。
        if (outerRing != null)
        {
            MeshRenderer renderer = outerRing.GetComponent<MeshRenderer>();

            if (renderer != null)
                outerMaterial = renderer.material;
        }

        if (innerRune != null)
        {
            MeshRenderer renderer = innerRune.GetComponent<MeshRenderer>();

            if (renderer != null)
                innerMaterial = renderer.material;
        }

        SetAlpha(0f);
    }

    private void Start()
    {
        StartCoroutine(LifecycleRoutine());
    }

    private void Update()
    {
        if (outerRing != null)
        {
            outerRing.Rotate(
                Vector3.forward,
                outerRotationSpeed * Time.deltaTime,
                Space.Self
            );
        }

        if (innerRune != null)
        {
            innerRune.Rotate(
                Vector3.forward,
                innerRotationSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }

    private IEnumerator LifecycleRoutine()
    {
        // 1. 展開 + 淡入
        float timer = 0f;

        while (timer < appearDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / appearDuration);

            transform.localScale =
                Vector3.Lerp(Vector3.zero, targetScale, t);

            SetAlpha(t);

            yield return null;
        }

        transform.localScale = targetScale;
        SetAlpha(1f);

        // 2. 維持一段時間
        yield return new WaitForSeconds(stayDuration);

        // 3. 淡出
        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);

            SetAlpha(1f - t);

            yield return null;
        }

        SetAlpha(0f);

        // 4. 法陣自己銷毀
        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        SetMaterialAlpha(outerMaterial, alpha);
        SetMaterialAlpha(innerMaterial, alpha);
    }

    private void SetMaterialAlpha(Material material, float alpha)
    {
        if (material == null)
            return;

        Color color = material.color;
        color.a = alpha;
        material.color = color;
    }
}