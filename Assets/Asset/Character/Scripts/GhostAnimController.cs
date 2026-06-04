using UnityEngine;

public class GhostAnimController : MonoBehaviour
{
    private Animator animator;
    private Opening _opening;
    private WinEnding _ending; 

    void Start()
    {
        animator = GetComponent<Animator>();
        _opening = GetComponent<Opening>();
        _ending = GetComponent<WinEnding>();
        //SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void PlayOpening()
    {
        SetVisible(true);
        animator.SetTrigger("TriggerOpening");
        _opening?.PlayOpeningVFX();          
    }

    public void PlayRandomDance()
    {
        int index = Random.Range(1, 5);
        animator.SetInteger("DanceIndex", index);
    }

    public void PlayRandomAttack()
    {
        int index = Random.Range(1, 2);
        animator.SetInteger("AttackIndex", index);
    }

    public void PlayHurt_01() => animator.SetTrigger("TriggerHurt_01");
    public void PlayHurt_02() => animator.SetTrigger("TriggerHurt_02");

    public void PlayEnding(bool playerWin)
    {
        if (playerWin)
            animator.SetTrigger("TriggerEndWin");
        else
            animator.SetTrigger("TriggerEndLose");
            _ending?.StartDissolve();
    }
}
