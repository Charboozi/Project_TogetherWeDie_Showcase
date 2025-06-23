using UnityEngine;

public class CCTVScreen : MonoBehaviour, IClientOnlyAction, IInteractableAction
{
    [SerializeField] CCTVManager manager;

    public void DoClientAction()
    {
        if (manager != null)
        {
            manager.ActivateCCTV();
        }
    }
    public void DoAction(){}
}