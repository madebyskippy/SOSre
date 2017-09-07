using UnityEngine;

// Scenes are parameterized based on the type of data
// Under this scheme all the scenes associated with a SceneManager
// have to be of the same type.
public class Scene<TTransitionData> : MonoBehaviour
{
    public GameObject Root { get { return gameObject; } }

    // "Hidden" versions of the on enter and exit methods that take care
    // of activating/deactivating the scenes as they transition. They take
    // care of handling that before calling the subclass's methods to avoid
    // saddling all subclasses from having to call super.OnEnter/Exit()
    internal void _OnEnter(TTransitionData data)
    {
        Root.SetActive(true);
        OnEnter(data);
    }

    internal void _OnExit()
    {
        Root.SetActive(false);
        OnExit();
    }

    internal virtual void OnEnter(TTransitionData data) { }
    internal virtual void OnExit() { }
    internal virtual void Init() { }

    // NOTE: If you were doing this outside of Unity you would most likely need to
    // implement some lifecycle management methods (e.g. update, destroy, awake, etc.)
    // but why bother replicating that if Unity does it for you

}