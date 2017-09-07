using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventE {}

// EVENTS
public class ButtonPressed : EventE {
    public string button;
    public int playerNum;
    public ButtonPressed(string _button)
    {
        button = _button;
    }
}

public class Reset : EventE { }

public class EventManager {

	public delegate void EventDelegate<T>(T e) where T: EventE;
	private delegate void EventDelegate(EventE e);

	private Dictionary <System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
	private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();
	private List<EventE> queuedEvents = new List<EventE> ();
	private object queueLock = new object();

	public void Register<T> (EventDelegate<T> del) where T: EventE {
		if (delegateLookup.ContainsKey (del)) {
			return;
		}

		EventDelegate internalDelegate = (e) => del ((T)e);
		delegateLookup [del] = internalDelegate;

		EventDelegate tempDel;
		if (delegates.TryGetValue (typeof(T), out tempDel)) {
			delegates [typeof(T)] = tempDel + internalDelegate;
		} else {
			delegates [typeof(T)] = internalDelegate;
		}
	}

	public void Unregister<T> (EventDelegate<T> del) where T: EventE {
		EventDelegate internalDelegate;
		if (delegateLookup.TryGetValue (del, out internalDelegate)) {
			EventDelegate tempDel;
			if (delegates.TryGetValue (typeof(T), out tempDel)) {
				tempDel -= internalDelegate;
				if (tempDel == null) {
					delegates.Remove (typeof(T));
				} else {
					delegates [typeof(T)] = tempDel;
				}
			}
			delegateLookup.Remove (del);
		}
	}

	public void Clear(){
		lock (queueLock) {
			if (delegates != null) {
				delegates.Clear ();
			}
			if (delegateLookup != null) {
				delegateLookup.Clear ();
			}
			if (queuedEvents != null) {
				queuedEvents.Clear ();
			}
		}
	}

	public void Fire(EventE e){
		EventDelegate del;
		if (delegates.TryGetValue (e.GetType (), out del)) {
			del.Invoke (e);
		}
	}

	public void ProcessQueuedEvents(){
		List<EventE> events;
		lock (queueLock) {
			if (queuedEvents.Count > 0) {
				events = new List<EventE> (queuedEvents);
				queuedEvents.Clear ();
			} else {
				return;
			}
		}

		foreach (EventE e in events) {
			Fire (e);
		}
	}

	public void Queue(EventE e){
		lock (queueLock) {
			queuedEvents.Add (e);
		}
	}

}
