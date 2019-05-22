﻿using Giant.Log;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Giant.DB
{
    class TaskQueue
    {
        private TaskCompletionSource<DBTask> tcs;
        private readonly Queue<DBTask> tasks = new Queue<DBTask>();

        public int TaskCount => tasks.Count;

        public async void Start()
        {
            while (true)
            {
                DBTask task = await Get();

                try
                {
                    await task.Run();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        public void Add(DBTask task)
        {
            //有等待获取任务的任务，则直接将该获取任务的任务设置为完成状态
            if (this.tcs != null)
            {
                var willFinish = this.tcs;
                this.tcs = null;
                willFinish.SetResult(task);
                return;
            }
            else
            {
                this.tasks.Enqueue(task);
            }
        }

        private Task<DBTask> Get()
        {
            this.tcs = new TaskCompletionSource<DBTask>();
            if (this.tasks.TryDequeue(out var task))
            {
                this.tcs.SetResult(task);
            }
            return this.tcs.Task;
        }
    }
}