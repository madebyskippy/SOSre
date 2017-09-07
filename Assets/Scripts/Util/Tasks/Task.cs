using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Task {

	public enum TaskStatus : byte {
		Detached,
		Pending,
		Working,
		Success,
		Fail,
		Aborted
	}

	public TaskStatus status { get; private set; }

	public bool IsDetached {
		get {
			return (status == TaskStatus.Detached);
		}
	}

	public bool IsAttached {
		get {
			return (status != TaskStatus.Detached);
		}
	}

	public bool IsPending {
		get {
			return (status == TaskStatus.Pending);
		}
	}

	public bool IsWorking {
		get {
			return (status == TaskStatus.Working);
		}
	}

	public bool IsSuccessful {
		get {
			return (status == TaskStatus.Success);
		}
	}

	public bool IsFailure {
		get {
			return (status == TaskStatus.Fail);
		}
	}

	public bool IsAborted {
		get {
			return (status == TaskStatus.Aborted);
		}
	}

	public bool IsFinished{
		get {
			return ((status == TaskStatus.Aborted) || (status == TaskStatus.Fail) || (status == TaskStatus.Success));
		}
	}

	public void Abort(){
		SetStatus (TaskStatus.Aborted);
	}

	internal void SetStatus(TaskStatus newStatus){
		if (status == newStatus) {
			return;
		}
			
		status = newStatus;

		switch (newStatus) {
		case TaskStatus.Working:
			Init ();
			break;
		
		case TaskStatus.Aborted:
			OnAbort ();
			CleanUp ();
				break;
		
		case TaskStatus.Fail:
			OnFail ();
			CleanUp ();
				break;
		
		case TaskStatus.Success:
			OnSuccess ();
			CleanUp ();
			break;

		case TaskStatus.Detached:
		case TaskStatus.Pending:
			break;
			
		default:
			throw new ArgumentOutOfRangeException(newStatus.ToString(), newStatus, null);
		}
	}

	protected virtual void Init(){}

	protected virtual void OnAbort() {}

	protected virtual void OnSuccess() {}

	protected virtual void OnFail() {}

	internal virtual void Update() {}

	protected virtual void CleanUp() {}

	public List<Task> nextTasks{ get; private set; }

	public void Then(params Task[] tasks)
    {
        nextTasks = new List<Task>();
        foreach (Task task in tasks)
        {
            Debug.Assert(!task.IsAttached);
            nextTasks.Add(task);
        }
	}

    public void Then(List<Task> tasks)
    {
        nextTasks = new List<Task>();
        foreach (Task task in tasks)
        {
            Debug.Assert(!task.IsAttached);
            nextTasks.Add(task);
        }
    }

    public void Then(TaskTree taskTree)
    {
        Debug.Assert(!taskTree.root.IsAttached);
        nextTasks = new List<Task>(){ taskTree.DistributedTree() };
    }

    public Task Then(TaskQueue taskQueue)
    {
        foreach (Task task in taskQueue.tasks)
        {
            Debug.Assert(!task.IsAttached);
        }

        nextTasks = new List<Task>();

        nextTasks.Add(taskQueue.tasks[0]);

        for (int i = 1; i < taskQueue.tasks.Count; i++)
        {
            taskQueue.tasks[i - 1].nextTasks.Add(taskQueue.tasks[i]);
        }
        return taskQueue.tasks[taskQueue.tasks.Count - 1];
    }
}

public class TaskTree
{
    public Task root;
    public List<TaskTree> children;

    public TaskTree(Task _root, params TaskTree[] _children)
    {
        root = _root;
        children = new List<TaskTree>();
        foreach (TaskTree child in _children) children.Add(child);
    }

    public TaskTree AddChild(Task child)
    {
        return AddChild(new TaskTree(child));
    }

    public TaskTree AddChild(TaskTree child)
    {
        children.Add(child);
        return this;
    }

    public TaskTree AddChildren(List<TaskTree> children_)
    {
        children.AddRange(children_);
        return this;
    }

    public TaskTree AddChildren(List<Task> children_)
    {
        for (int i = 0; i < children_.Count; i++)
        {
            children.Add(new TaskTree(children_[i]));
        }
        return this;
    }

    public Task DistributedTree()
    {
        List<Task> childrenDistributed = new List<Task>();
        if (children.Count > 0) foreach (TaskTree child in children) childrenDistributed.Add(child.DistributedTree());
        root.Then(childrenDistributed);
        return root;
    }

    public TaskTree Then(TaskTree nextTree)
    {
        if (children.Count > 0)
        {
            return children[0].Then(nextTree);
        }
        else
        {
            return AddChild(nextTree);
        }
    }

    public TaskTree Then(Task nextTask)
    {
        return Then(new TaskTree(nextTask));
    }

    public int Depth()
    {
        if(children.Count == 0)
        {
            return 1;
        }
        else
        {
            return 1 + children[0].Depth();
        }
    }
}

public class EmptyTask : Task
{
    protected override void Init()
    {
        SetStatus(TaskStatus.Success);
    }
}

public class TaskQueue
{
    public List<Task> tasks;

    public TaskQueue(List<Task> taskList)
    {
        tasks = taskList;
    }

    public TaskQueue()
    {
        tasks = new List<Task>();
    }

    public TaskQueue Then(TaskQueue taskQueue)
    {
        tasks.AddRange(taskQueue.tasks);
        return this;
    }

    public TaskQueue Add(Task task)
    {
        tasks.Add(task);
        return this;
    }
}


