using UnityEngine;

public class test : MonoBehaviour
{
    private Animator _animator;

    private static readonly int DoAttack1 = Animator.StringToHash("doAttack1");
    private static readonly int DoAttack2 = Animator.StringToHash("doAttack2");

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _animator.SetTrigger(DoAttack1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            _animator.SetTrigger(DoAttack2);
        }
    }
}
