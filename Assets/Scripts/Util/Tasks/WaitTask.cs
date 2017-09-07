using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WaitTask : Task
    {
        private float duration;
        private float timeElapsed;

        public WaitTask(float _duration)
        {
            duration = _duration;
        }

        protected override void Init()
        {
            timeElapsed = 0;
        }

        internal override void Update()
        {
            timeElapsed += Time.deltaTime;

            if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
        }

    }
