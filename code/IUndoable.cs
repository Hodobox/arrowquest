// interface for an object which can Undo.
interface IUndoable
{
    // Save the current state of the object. The next Undo should return to this state.
    public void SaveState();
    // If the object is in an initial state, do nothing.
    // Otherwise, save the state, then apply the initial state.
    // Note that it is not a reset, as undoing should bring us back to the current state.
    public void ApplyInitialState();
    // Return to the previous saved state, if there is one.
    public void Undo();
}