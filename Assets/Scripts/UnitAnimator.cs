using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animator Params")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isOverwatchParam = "IsOverwatch";
    [SerializeField] private string isDeadParam = "IsDead";
    [SerializeField] private string shootTriggerParam = "IsShooting";
    [SerializeField] private string hitTriggerParam = "GetHit";

    private Unit unit;
    private bool deathTriggered = false; // Ölümün bir kez tetiklendiðini kontrol etmek için

    private void Awake()
    {
        unit = GetComponent<Unit>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (animator == null || unit == null) return;

        // Karakter öldüyse sadece bir kez ölüm parametresini gönder ve döngüden çýk
        if (unit.IsDead)
        {
            if (!deathTriggered)
            {
                animator.SetBool(isDeadParam, true);
                animator.SetBool(isMovingParam, false);
                animator.SetBool(isOverwatchParam, false);
                deathTriggered = true;
            }
            return; // Sürekli Any State'i tetiklemesini engellemek için Update'i burada kesiyoruz
        }

        animator.SetBool(isMovingParam, unit.Anim_IsMoving);
        animator.SetBool(isOverwatchParam, unit.isOverwatch && !unit.Anim_IsMoving);
    }

    public void PlayShootAnimation()
    {
        if (animator != null && !unit.IsDead)
        {
            animator.SetTrigger(shootTriggerParam);
        }
    }

    public void TriggerHitAnimation()
    {
        if (animator != null && !unit.IsDead)
        {
            animator.SetTrigger(hitTriggerParam);
        }
    }
}