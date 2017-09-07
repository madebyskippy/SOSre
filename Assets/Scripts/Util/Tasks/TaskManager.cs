using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager {
	private readonly List<Task> tasks = new List<Task>();

	public void AddTask(Task task){
		Debug.Assert (task != null);
		Debug.Assert (!task.IsAttached);
		tasks.Add (task);
		task.SetStatus (Task.TaskStatus.Pending);
	}

    public void AddTask(TaskTree taskTree)
    {
        Debug.Assert(taskTree.root != null);
        Debug.Assert(!taskTree.root.IsAttached);
        tasks.Add(taskTree.DistributedTree());
        taskTree.root.SetStatus(Task.TaskStatus.Pending);
    }

    public void AddTask(TaskQueue taskQueue)
    {
        AddTask(taskQueue.tasks[0]);
        for (int i = 1; i < taskQueue.tasks.Count; i++)
        {
            taskQueue.tasks[i - 1].Then(taskQueue.tasks[i]);
        }
    }

    public void Update(){
		Task task;
		for (int i = tasks.Count - 1; i >= 0; i--) {
			task = tasks [i];

			if (task.IsPending) {
				task.SetStatus (Task.TaskStatus.Working);
			}

			if (task.IsFinished) {
				HandleCompletion (task, i);
			} else {
				task.Update ();
				if (task.IsFinished) {
					HandleCompletion (task, i);
				}
			}
		}
	}

	private void HandleCompletion(Task task, int taskIndex){
		if ((task.nextTasks != null) && task.IsSuccessful) {
            foreach (Task nextTask in task.nextTasks) AddTask(nextTask);
		}

		tasks.RemoveAt (taskIndex);
		task.SetStatus (Task.TaskStatus.Detached);
	}

}
