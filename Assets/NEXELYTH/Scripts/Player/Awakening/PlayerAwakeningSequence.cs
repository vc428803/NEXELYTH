using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家進入 Nexelyth 世界時的甦醒流程。
///
/// 流程：
/// 全黑
/// → 第一次微微睜眼
/// → 再閉眼
/// → 第二次慢慢睜眼
/// → 眼皮疲倦地微微下沉
/// → 再次撐開
/// → 完全清醒
/// → 顯示雙手與互動射線
/// </summary>
public class PlayerAwakeningSequence : MonoBehaviour
{
    [Header("Eyelid")]

    [Header("Vision Recovery")]
    [SerializeField]
    private Image visionRecoveryMask;

    [SerializeField, Range(0f, 1f)]
    private float wakingDarkness = 0.31f;

    [SerializeField]
    private bool debugEyeOpen = false;

    [SerializeField, Range(0f, 1f)]
    private float debugEyeOpenValue = 0.15f;

    [SerializeField]
    private Material eyelidMaterial;
    

    [Header("First Awakening")]

    [SerializeField]
    private float initialBlackDuration = 1.5f;

    [SerializeField]
    private float firstOpenDuration = 1.2f;

    [SerializeField, Range(0f, 1f)]
    private float firstOpenAmount = 0.18f;

    [SerializeField]
    private float firstOpenHoldDuration = 0.35f;

    [SerializeField]
    private float blinkCloseDuration = 0.35f;

    [SerializeField]
    private float blinkHoldDuration = 0.20f;


    [Header("Final Awakening")]

    // 第二次睜到這裡時，
    // 眼皮會稍微撐不住。
    [SerializeField, Range(0f, 1f)]
    private float tiredOpenAmount = 0.65f;

    // 從閉眼到 65%。
    [SerializeField]
    private float tiredOpenDuration = 2.2f;

    // 眼皮稍微掉回來。
    [SerializeField, Range(0f, 1f)]
    private float tiredDropAmount = 0.57f;

    // 掉下來的速度。
    [SerializeField]
    private float tiredDropDuration = 0.20f;

    // 掉下來後非常短的停頓。
    [SerializeField]
    private float tiredHoldDuration = 0.12f;

    // 再重新撐回去。
    [SerializeField]
    private float recoveryDuration = 0.28f;

    // 從 65% 慢慢完全睜開。
    [SerializeField]
    private float finalOpenDuration = 1.5f;


    [Header("XR Visuals")]

    [SerializeField]
    private GameObject leftControllerVisual;

    [SerializeField]
    private GameObject rightControllerVisual;

    [SerializeField]
    private GameObject leftLineVisual;

    [SerializeField]
    private GameObject rightLineVisual;


    [Header("Interaction Timing")]

    [SerializeField]
    private float interactionEnableDelay = 1.0f;


    private static readonly int EyeOpen =
        Shader.PropertyToID("_EyeOpen");


    private IEnumerator Start()
    {
        if (eyelidMaterial == null)
        {
            Debug.LogError(
                "[PlayerAwakeningSequence] Eyelid Material 尚未設定。",
                this
            );

            yield break;
        }

        // Debug 模式：
        // 不播放正式甦醒動畫，
        // 直接停在指定的睜眼程度。
        if (debugEyeOpen)
        {
            SetXRVisuals(false);

            SetEyeOpen(debugEyeOpenValue);

            yield break;
        }



        // ==================================================
        // 1. 剛進入世界
        // ==================================================

        SetXRVisuals(false);
        SetEyeOpen(0f);
        SetVisionDarkness(wakingDarkness);

        yield return new WaitForSeconds(
            initialBlackDuration
        );


        // ==================================================
        // 2. 第一次微微睜眼
        // ==================================================

        yield return AnimateEye(
            0f,
            firstOpenAmount,
            firstOpenDuration
        );

        yield return new WaitForSeconds(
            firstOpenHoldDuration
        );


        // ==================================================
        // 3. 再閉眼
        // ==================================================

        yield return AnimateEye(
            firstOpenAmount,
            0f,
            blinkCloseDuration
        );

        yield return new WaitForSeconds(
            blinkHoldDuration
        );


        // 第二次睜眼。
        // 眼睛慢慢張開的同時，
        // 世界也開始從昏暗中稍微恢復。
        StartCoroutine(
            AnimateVisionDarkness(
                wakingDarkness,
                0.50f,
                tiredOpenDuration
            )
        );

        yield return AnimateEye(
            0f,
            tiredOpenAmount,
            tiredOpenDuration
        );


        // ==================================================
        // 5. 眼皮稍微撐不住
        //
        // 65% → 57%
        //
        // 幅度非常小，
        // 不應該看起來像完整眨眼。
        // ==================================================

        yield return AnimateEye(
            tiredOpenAmount,
            tiredDropAmount,
            tiredDropDuration
        );


        // 短暫停一下。
        yield return new WaitForSeconds(
            tiredHoldDuration
        );


        // ==================================================
        // 6. 再次把眼皮撐回來
        //
        // 57% → 65%
        // ==================================================

        yield return AnimateEye(
            tiredDropAmount,
            tiredOpenAmount,
            recoveryDuration
        );


        // ==================================================
        // 7. 最後完全睜開
        //
        // 65% → 100%
        // ==================================================

        // 最後完全睜眼。
        //
        // 眼皮繼續張開的同時，
        // 視覺遮罩也逐漸消失。
        float elapsed = 0f;

        while (elapsed < finalOpenDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / finalOpenDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            float eyeOpen =
                Mathf.Lerp(
                    tiredOpenAmount,
                    1f,
                    t
                );

            float darkness =
                Mathf.Lerp(
                    0.50f,
                    0f,
                    t
                );

            SetEyeOpen(eyeOpen);
            SetVisionDarkness(darkness);

            yield return null;
        }

        SetEyeOpen(1f);
        SetVisionDarkness(0f);

        yield return new WaitForSeconds(
            interactionEnableDelay
        );


        // ==================================================
        // 9. 恢復玩家視覺互動
        // ==================================================

        SetXRVisuals(true);
    }


    /// <summary>
    /// 平滑控制眼皮張開程度。
    /// </summary>
    private IEnumerator AnimateEye(
        float from,
        float to,
        float duration)
    {
        if (duration <= 0f)
        {
            SetEyeOpen(to);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );


            // SmoothStep 避免機械式等速移動。
            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            float eyeOpen =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );


            SetEyeOpen(
                eyeOpen
            );

            yield return null;
        }

        SetEyeOpen(to);
    }

    /// <summary>
    /// 平滑改變甦醒遮罩的黑暗程度。
    /// </summary>
    private IEnumerator AnimateVisionDarkness(
        float from,
        float to,
        float duration)
    {
        if (duration <= 0f)
        {
            SetVisionDarkness(to);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            float darkness =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );

            SetVisionDarkness(darkness);

            yield return null;
        }

        SetVisionDarkness(to);
    }


    /// <summary>
    /// 設定 Shader 的 Eye Open。
    /// </summary>
    private void SetEyeOpen(float value)
    {
        eyelidMaterial.SetFloat(
            EyeOpen,
            Mathf.Clamp01(value)
        );
    }


    /// <summary>
    /// 只控制 XR 的視覺物件。
    ///
    /// 不關閉 Controller / Interactor 本體，
    /// 避免破壞 XR Tracking。
    /// </summary>
    private void SetXRVisuals(bool visible)
    {
        SetObjectActive(
            leftControllerVisual,
            visible
        );

        SetObjectActive(
            rightControllerVisual,
            visible
        );

        SetObjectActive(
            leftLineVisual,
            visible
        );

        SetObjectActive(
            rightLineVisual,
            visible
        );
    }


    private void SetObjectActive(
        GameObject target,
        bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    /// <summary>
    /// 設定甦醒時的黑色視覺遮罩。
    /// 0 = 完全透明，世界恢復正常亮度。
    /// 1 = 完全黑。
    /// </summary>
    private void SetVisionDarkness(float alpha)
    {
        if (visionRecoveryMask == null)
        {
            return;
        }

        Color color =
            visionRecoveryMask.color;

        color.a =
            Mathf.Clamp01(alpha);

        visionRecoveryMask.color =
            color;
    }
}