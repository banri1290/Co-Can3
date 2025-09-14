using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimeController : MonoBehaviour
{
    [Tooltip("拖带 Animator 的 Sprite 节点（必须有 Animator）")]
    public Transform spriteTransform;
    [Tooltip("要观察 X 值变化的 Transform（默认是本物体）")]
    public Transform observedTransform;
    public float deadzone = 0.001f;

    public string paramFacing = "IsFacingRight";
    // public string paramMoving = "IsMoving"; // 不要使用
    // public bool setIsMoving = true;         // 不要使用

    private Animator animator;
    private float lastX;

    void Start()
    {
        if (observedTransform == null) observedTransform = transform;
        if (spriteTransform == null) spriteTransform = transform;

        animator = spriteTransform.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[SpriteDirectionByX] Animator 未找到：{spriteTransform.name}");
        }

        lastX = observedTransform.position.x;
    }

    void Update()
    {
        float currentX = observedTransform.position.x;
        float deltaX = currentX - lastX;

        if (Mathf.Abs(deltaX) > deadzone)
        {
            bool facingRight = deltaX > 0f;

            if (animator != null)
            {
                animator.SetBool(paramFacing, facingRight);
            }

            // Sprite左右反転
            Vector3 scale = spriteTransform.localScale;
            scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            spriteTransform.localScale = scale;

            Debug.Log($"[SpriteDirectionByX] deltaX={deltaX:F4} -> facingRight={facingRight}");
        }

        lastX = currentX;
    }
}
