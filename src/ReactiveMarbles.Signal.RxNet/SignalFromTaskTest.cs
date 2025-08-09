// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using ReactiveMarbles.Signals;

namespace ReactiveMarbles.Tests;

/// <summary>
/// SignalFromTaskTest.
/// </summary>
public class SignalFromTaskTest
{
    /// <summary>
    /// Signals from task handles user exceptions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTaskHandlesUserExceptions()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask(
             async (cts) =>
             {
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(10000, cts.Token).HandleCancellation(async () =>
                 {
                     // User Handles cancellation.
                     statusTrail.Add((position++, "starting cancelling command"));

                     // dummy cleanup
                     await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                     statusTrail.Add((position++, "finished cancelling command"));
                 }).ConfigureAwait(true);

                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 throw new Exception("break execution");
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));

        await Task.Delay(10000).ConfigureAwait(true);
        cancel.Dispose();

        // Wait 6000 ms to allow execution and cleanup to complete
        await Task.Delay(6000).ConfigureAwait(false);

        Assert.DoesNotContain("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Should always come here.", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Exception Should Be here", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.False(result);
        //// (0, "started command")
        //// (1, "finished command Normally")
        //// (2, "Exception Should Be here")
        //// (3, "Should always come here.")
    }

    /// <summary>
    /// Signals from task handles cancellation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTaskHandlesCancellation()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask(
             async (cts) =>
        {
            statusTrail.Add((position++, "started command"));
            await Task.Delay(10000, cts.Token).HandleCancellation(async () =>
            {
                // User Handles cancellation.
                statusTrail.Add((position++, "starting cancelling command"));

                // dummy cleanup
                await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                statusTrail.Add((position++, "finished cancelling command"));
            }).ConfigureAwait(true);

            if (!cts.IsCancellationRequested)
            {
                statusTrail.Add((position++, "finished command Normally"));
            }

            return Unit.Default;
        }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));
        cancel.Dispose();

        // Wait 6000 ms to allow execution and cleanup to complete
        await Task.Delay(6000).ConfigureAwait(false);

        Assert.Contains("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Should always come here.", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.False(result);
        //// (0, "started command")
        //// (1, "starting cancelling command")
        //// (2, "Should always come here.")
        //// (3, "finished cancelling command")
    }

    /// <summary>
    /// Signals from task handles token cancellation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTaskHandlesTokenCancellation()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask(
             async (cts) =>
             {
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(1000, cts.Token).HandleCancellation().ConfigureAwait(false);
                 _ = Task.Run(async () =>
                 {
                     // Wait for 1s then cancel
                     await Task.Delay(1000).ConfigureAwait(false);
                     cts.Cancel();
                 });
                 await Task.Delay(5000, cts.Token).HandleCancellation(async () =>
                 {
                     // User Handles cancellation.
                     statusTrail.Add((position++, "starting cancelling command"));

                     // dummy cleanup
                     await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                     statusTrail.Add((position++, "finished cancelling command"));
                 }).ConfigureAwait(true);

                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 return Unit.Default;
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));

        // Wait 8000 ms to allow execution and cleanup to complete
        await Task.Delay(8000).ConfigureAwait(false);

        Assert.Contains("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Should always come here.", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.False(result);
        //// (0, "started command")
        //// (1, "starting cancelling command")
        //// (2, "Should always come here.")
        //// (3, "finished cancelling command")
    }

    /// <summary>
    /// Signals from task handles cancellation in base.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTaskHandlesCancellationInBase()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask(
             async (cts) =>
             {
                 var ex = new Exception();
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(10000, cts.Token).ConfigureAwait(true);
                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 return Unit.Default;
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var cancel = fixture.Subscribe();
        await Task.Delay(500).ConfigureAwait(true);
        Assert.Contains("started command", statusTrail.Select(x => x.Item2));
        cancel.Dispose();

        // Wait 5050 ms to allow execution and cleanup to complete
        await Task.Delay(6000).ConfigureAwait(false);

        Assert.DoesNotContain("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.Equal("Should always come here.", statusTrail.Last().Item2);

        //// (0, "started command")
        //// (1, "Should always come here.")
    }

    /// <summary>
    /// Signals from task handles completion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTaskHandlesCompletion()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask(
             async (cts) =>
             {
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(10000, cts.Token).HandleCancellation(async () =>
                 {
                     // NOT EXPECTED TO ENTER HERE

                     // User Handles cancellation.
                     statusTrail.Add((position++, "starting cancelling command"));

                     // dummy cleanup
                     await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                     statusTrail.Add((position++, "finished cancelling command"));
                 }).ConfigureAwait(true);

                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 return Unit.Default;
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));

        // Wait 11000 ms to allow execution complete
        await Task.Delay(11000).ConfigureAwait(false);

        Assert.DoesNotContain("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.Equal("Should always come here.", statusTrail.Last().Item2);
        Assert.True(result);
        //// (0, "started command")
        //// (2, "finished command Normally")
        //// (1, "Should always come here.")
    }

    /// <summary>
    /// Signals from task t handles user exceptions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTask_T_HandlesUserExceptions()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask<Unit>(
             async (cts) =>
             {
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(10000, cts.Token).HandleCancellation(async () =>
                 {
                     // User Handles cancellation.
                     statusTrail.Add((position++, "starting cancelling command"));

                     // dummy cleanup
                     await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                     statusTrail.Add((position++, "finished cancelling command"));
                 }).ConfigureAwait(true);

                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 throw new Exception("break execution");
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));

        await Task.Delay(10000).ConfigureAwait(true);
        cancel.Dispose();

        // Wait 6000 ms to allow execution and cleanup to complete
        await Task.Delay(6000).ConfigureAwait(false);

        Assert.DoesNotContain("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Should always come here.", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Exception Should Be here", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.False(result);
        //// (0, "started command")
        //// (1, "finished command Normally")
        //// (2, "Exception Should Be here")
        //// (3, "Should always come here.")
    }

    /// <summary>
    /// Signals from task t handles cancellation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTask_T_HandlesCancellation()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask<Unit>(
             async (cts) =>
             {
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(10000, cts.Token).HandleCancellation(async () =>
                 {
                     // User Handles cancellation.
                     statusTrail.Add((position++, "starting cancelling command"));

                     // dummy cleanup
                     await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                     statusTrail.Add((position++, "finished cancelling command"));
                 }).ConfigureAwait(true);

                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 return Unit.Default;
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));
        cancel.Dispose();

        // Wait 6000 ms to allow execution and cleanup to complete
        await Task.Delay(6000).ConfigureAwait(false);

        Assert.Contains("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Should always come here.", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.False(result);
        //// (0, "started command")
        //// (1, "starting cancelling command")
        //// (3, "Should always come here.")
        //// (2, "finished cancelling command")
    }

    /// <summary>
    /// Signals from task t handles token cancellation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTask_T_HandlesTokenCancellation()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask<Unit>(
             async (cts) =>
             {
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(1000, cts.Token).HandleCancellation().ConfigureAwait(false);
                 _ = Task.Run(async () =>
                 {
                     // Wait for 1s then cancel
                     await Task.Delay(1000).ConfigureAwait(false);
                     cts.Cancel();
                 });
                 await Task.Delay(5000, cts.Token).HandleCancellation(async () =>
                 {
                     // User Handles cancellation.
                     statusTrail.Add((position++, "starting cancelling command"));

                     // dummy cleanup
                     await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                     statusTrail.Add((position++, "finished cancelling command"));
                 }).ConfigureAwait(true);

                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 return Unit.Default;
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));

        // Wait 8000 ms to allow execution and cleanup to complete
        await Task.Delay(8000).ConfigureAwait(false);

        Assert.Contains("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("Should always come here.", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.False(result);
        //// (0, "started command")
        //// (1, "starting cancelling command")
        //// (2, "Should always come here.")
        //// (3, "finished cancelling command")
    }

    /// <summary>
    /// Signals from task t handles cancellation in base.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTask_T_HandlesCancellationInBase()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask<Unit>(
             async (cts) =>
             {
                 var ex = new Exception();
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(10000, cts.Token).ConfigureAwait(true);
                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 return Unit.Default;
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var cancel = fixture.Subscribe();
        await Task.Delay(500).ConfigureAwait(true);
        Assert.Contains("started command", statusTrail.Select(x => x.Item2));
        cancel.Dispose();

        // Wait 5050 ms to allow execution and cleanup to complete
        await Task.Delay(6000).ConfigureAwait(false);

        Assert.DoesNotContain("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.Equal("Should always come here.", statusTrail.Last().Item2);

        //// (0, "started command")
        //// (1, "Should always come here.")
    }

    /// <summary>
    /// Signals from task t handles completion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SignalFromTask_T_HandlesCompletion()
    {
        var statusTrail = new List<(int, string)>();
        var position = 0;
        Exception? exception = null;
        var fixture = Signal.FromTask<Unit>(
             async (cts) =>
             {
                 statusTrail.Add((position++, "started command"));
                 await Task.Delay(10000, cts.Token).HandleCancellation(async () =>
                 {
                     // NOT EXPECTED TO ENTER HERE

                     // User Handles cancellation.
                     statusTrail.Add((position++, "starting cancelling command"));

                     // dummy cleanup
                     await Task.Delay(5000, CancellationToken.None).ConfigureAwait(false);
                     statusTrail.Add((position++, "finished cancelling command"));
                 }).ConfigureAwait(true);

                 if (!cts.IsCancellationRequested)
                 {
                     statusTrail.Add((position++, "finished command Normally"));
                 }

                 return Unit.Default;
             }).Catch<Unit, Exception>(
            ex =>
            {
                exception = ex;
                statusTrail.Add((position++, "Exception Should Be here"));
                return Observable.Throw<Unit>(ex);
            }).Finally(() => statusTrail.Add((position++, "Should always come here.")));

        var result = false;
        var cancel = fixture.Subscribe(_ => result = true);
        await Task.Delay(500).ConfigureAwait(true);

        Assert.Contains("started command", statusTrail.Select(x => x.Item2));

        // Wait 11000 ms to allow execution complete
        await Task.Delay(11000).ConfigureAwait(false);

        Assert.DoesNotContain("starting cancelling command", statusTrail.Select(x => x.Item2));
        Assert.DoesNotContain("finished cancelling command", statusTrail.Select(x => x.Item2));
        Assert.Contains("finished command Normally", statusTrail.Select(x => x.Item2));
        Assert.Equal("Should always come here.", statusTrail.Last().Item2);
        Assert.True(result);
        //// (0, "started command")
        //// (2, "finished command Normally")
        //// (1, "Should always come here.")
    }

    /// <summary>
    /// Signals the synchronize timer completes at same t IME.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Signal_SyncTimer_CompletesAtSameTIme()
    {
        var fixture1 = Signal.SyncTimer(TimeSpan.FromMilliseconds(555));
        var fixture2 = Signal.SyncTimer(TimeSpan.FromMilliseconds(555));
        var fixture3 = Signal.SyncTimer(TimeSpan.FromMilliseconds(555));
        var fixture = fixture1.CombineLatest(fixture2, fixture3, (a, b, c) => (a, b, c));
        var result = false;
        var dis = fixture.Take(1).Subscribe(
            t =>
        {
            Assert.Equal(t.a, t.b);
            Assert.Equal(t.a, t.c);
            Assert.Equal(t.b, t.c);
            result = true;
        });

        await Task.Delay(TimeSpan.FromMilliseconds(600)).ConfigureAwait(true);
        Assert.True(result);
        dis.Dispose();
    }
}
