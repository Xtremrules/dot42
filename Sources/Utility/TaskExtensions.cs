﻿using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Dot42.Utility
{
    public static class TaskExtensions
    {
        /// <summary>Downloads the resource with the specified URI to a local file, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <param name="fileName">The name of the local file that is to receive the data.</param>
        /// <returns>A Task that contains the downloaded data.</returns>
        public static Task DownloadFileTask(this WebClient webClient, Uri address, string fileName, DownloadProgressChangedEventHandler progressChanged)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<object>(address);

            // Setup the callback event handler
            AsyncCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, () => null, () => { 
                webClient.DownloadFileCompleted -= handler;
                if (progressChanged != null)
                {
                    webClient.DownloadProgressChanged -= progressChanged;
                }
            });
            webClient.DownloadFileCompleted += handler;
            if (progressChanged != null)
            {
                webClient.DownloadProgressChanged += progressChanged;
            }

            // Start the async work
            try
            {
                webClient.DownloadFileAsync(address, fileName, tcs);
            }
            catch (Exception exc)
            {
                // If something goes wrong kicking off the async work,
                // unregister the callback and cancel the created task
                webClient.DownloadFileCompleted -= handler;
                tcs.TrySetException(exc);
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        internal static void HandleCompletion<T>(TaskCompletionSource<T> tcs, AsyncCompletedEventArgs e, Func<T> getResult, Action unregisterHandler)
        {
            // Transfers the results from the AsyncCompletedEventArgs and getResult() to the
            // TaskCompletionSource, but only AsyncCompletedEventArg's UserState matches the TCS
            // (this check is important if the same WebClient is used for multiple, asynchronous
            // operations concurrently).  Also unregisters the handler to avoid a leak.

            if (e.UserState == tcs)
            {
                if (e.Cancelled) tcs.TrySetCanceled();
                else if (e.Error != null) tcs.TrySetException(e.Error);
                else tcs.TrySetResult(getResult());
                unregisterHandler();
            }
        }
    }
}
