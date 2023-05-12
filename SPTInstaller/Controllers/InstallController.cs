﻿using SharpCompress;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPTInstaller.Controllers
{
    public class InstallController
    {
        public event EventHandler<IProgressableTask> TaskChanged = delegate { };

        private IPreCheck[] _preChecks { get; set; }
        private IProgressableTask[] _tasks { get; set; }

        public InstallController(IProgressableTask[] tasks, IPreCheck[] preChecks = null)
        {
            _tasks = tasks;
            _preChecks = preChecks;
        }

        public async Task<IResult> RunPreChecks()
        {
            var requiredResults = new List<IResult>();

            _preChecks.ForEach(x => x.IsPending = true);

            foreach (var check in _preChecks)
            {
                var result = await check.RunCheck();

                if (check.IsRequired)
                {
                    requiredResults.Add(result);
                }
            }

            foreach(var result in requiredResults)
            {
                if (!result.Succeeded)
                    return Result.FromError("Some required checks have failed");
            }

            return Result.FromSuccess();
        }

        public async Task<IResult> RunTasks()
        {
            foreach (var task in _tasks)
            {
                TaskChanged?.Invoke(null, task);

                var result = await task.RunAsync();

                if (!result.Succeeded) return result;
            }

            return Result.FromSuccess("Install Complete. Happy Playing!");
        }
    }
}
