﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive.Disposables;
using System.Threading;

namespace ReactiveMarbles.Signals;

/// <summary>
/// IAsyncSignal.
/// </summary>
/// <typeparam name="T">The object that provides notification information.</typeparam>
/// <seealso cref="IObservable&lt;T&gt;" />
public interface IAsyncSignal<out T> : IObservable<T>, ICancelable
{
    /// <summary>
    /// Gets the cancellation token source.
    /// </summary>
    /// <value>
    /// The cancellation token source.
    /// </value>
    CancellationTokenSource? CancellationTokenSource { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is cancellation requested.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is cancellation requested; otherwise, <c>false</c>.
    /// </value>
    bool IsCancellationRequested { get; }

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    IObservable<T>? Source { get; }

    /// <summary>
    /// Gets the operation canceled.
    /// </summary>
    /// <param name="observer">The observer.</param>
    void GetOperationCanceled(IObserver<Exception> observer);
}
