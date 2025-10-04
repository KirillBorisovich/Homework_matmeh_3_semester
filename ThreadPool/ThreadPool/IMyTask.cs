// <copyright file="IMyTask.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ThreadPool;

/// <summary>
/// Interface for the result from the thread pool.
/// </summary>
/// <typeparam name="TResult">Type of task result.</typeparam>
public interface IMyTask<out TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task has been completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the task.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Continue the task with the new function.
    /// </summary>
    /// <param name="func">A new function to perform.</param>
    /// <typeparam name="TNewResult">Type of function result.</typeparam>
    /// <returns>The IMyTask interface element.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
}