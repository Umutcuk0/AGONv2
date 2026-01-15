using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animator Params")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isOverwatchParam = "IsOverwatch";
    [SerializeField] private string isDeadParam = "IsDead";

    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (animator == null || unit == null) return;

        animator.SetBool(isDeadParam, unit.IsDead);

        if (unit.IsDead)
        {
            animator.SetBool(isMovingParam, false);
            animator.SetBool(isOverwatchParam, false);
            return;
        }

        animator.SetBool(isMovingParam, unit.Anim_IsMoving);
        animator.SetBool(isOverwatchParam, unit.isOverwatch && !unit.Anim_IsMoving);
    }
}
