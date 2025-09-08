// interface for an object which can Undo.
interface IUndoable 
{
    // Save the current state of the object. The next Undo should return to this state.
    public void SaveState();
    // Applies the initial state to the object.
    // Note that it does not reset - Undo should go back to the current state, for example.
    public void ApplyInitialState();
    // Return to the previous saved state, if there is one.
	public void Undo();
}