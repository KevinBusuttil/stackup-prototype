namespace StackUp
{
    /// <summary>
    /// Anything the player can walk up to and act on (rack slot, dock lane,
    /// verification station, …). See CLAUDE_CODE_SPEC.md Section 13.3.
    /// </summary>
    public interface IInteractable
    {
        string GetPrompt();
        bool CanInteract(PlayerController player);
        void Interact(PlayerController player);
    }
}
