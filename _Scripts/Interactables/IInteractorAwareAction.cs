/// <summary>
/// An interactable action that needs to know which client triggered the interaction.
/// </summary>
public interface IInteractorAwareAction : IInteractableAction
{
    /// <param name="interactorClientId">the NetworkClientId of the player who interacted</param>
    void DoAction(ulong interactorClientId);
}
