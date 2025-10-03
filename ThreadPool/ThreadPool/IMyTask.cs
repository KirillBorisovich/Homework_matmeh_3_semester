// <copyright file="IMyTask.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ThreadPool;

public interface IMyTask<out TResult>
{
    bool IsCompleted { get; }

    TResult Result { get; }

    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
}