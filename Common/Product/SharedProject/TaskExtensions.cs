/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudioTools {
    static class TaskExtensions {
        /// <summary>
        /// Suppresses warnings about unawaited tasks and ensures that unhandled
        /// errors will cause the process to terminate.
        /// </summary>
        public static async void DoNotWait(this Task task) {
            await task;
        }
        
        /// <summary>
        /// Waits for a task to complete. If an exception occurs, the exception
        /// will be raised without being wrapped in a
        /// <see cref="AggregateException"/>.
        /// </summary>
        public static void WaitAndUnwrapExceptions(this Task task) {
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for a task to complete. If an exception occurs, the exception
        /// will be raised without being wrapped in a
        /// <see cref="AggregateException"/>.
        /// </summary>
        public static T WaitAndUnwrapExceptions<T>(this Task<T> task) {
            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for a task to complete and logs all exceptions except those
        /// that return true from <see cref="IsCriticalException"/>, which are
        /// rethrown.
        /// </summary>
        public static T WaitAndHandleAllExceptions<T>(
            this Task<T> task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        ) {
            return task.HandleAllExceptions(productTitle, callerType, callerFile, callerLineNumber, callerName)
                .WaitAndUnwrapExceptions();
        }


        /// <summary>
        /// Logs all exceptions from a task except those that return true from
        /// <see cref="IsCriticalException"/>, which are rethrown.
        /// If an exception is thrown, <c>default(T)</c> is returned.
        /// </summary>
        public static async Task<T> HandleAllExceptions<T>(
            this Task<T> task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        ) {
            var result = default(T);
            try {
                result = await task;
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }

                var message = SR.GetUnhandledExceptionString(ex, callerType, callerFile, callerLineNumber, callerName);
                // Send the message to the trace listener in case there is
                // somebody out there listening.
                Trace.TraceError(message);

                try {
                    ActivityLog.LogError(productTitle, message);
                } catch (InvalidOperationException) {
                    // Activity Log is unavailable.
                }
            }
            return result;
        }

        /// <summary>
        /// Waits for a task to complete and logs all exceptions except those
        /// that return true from <see cref="IsCriticalException"/>, which are
        /// rethrown.
        /// </summary>
        public static void WaitAndHandleAllExceptions(
            this Task task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        ) {
            task.HandleAllExceptions(productTitle, callerType, callerFile, callerLineNumber, callerName)
                .WaitAndUnwrapExceptions();
        }


        /// <summary>
        /// Logs all exceptions from a task except those that return true from
        /// <see cref="IsCriticalException"/>, which are rethrown.
        /// </summary>
        public static async Task HandleAllExceptions(
            this Task task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        ) {
            try {
                await task;
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }

                var message = SR.GetUnhandledExceptionString(ex, callerType, callerFile, callerLineNumber, callerName);
                // Send the message to the trace listener in case there is
                // somebody out there listening.
                Trace.TraceError(message);

                try {
                    ActivityLog.LogError(productTitle, message);
                } catch (InvalidOperationException) {
                    // Activity Log is unavailable.
                }
            }
        }
    }
}