// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveMarbles.Signals
{
    /// <summary>
    /// Signal.
    /// </summary>
    public static partial class Signal
    {
        private static readonly Dictionary<TimeSpan, Lazy<IConnectableObservable<DateTime>>> _timerList = new();
        /// <summary>
        /// Synchronized timer all instances of this with the same TimeSpan use the same timer.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>An Observable DateTime.</returns>
        public static IObservable<DateTime> SyncTimer(TimeSpan timeSpan)
        {
            if (!_timerList.ContainsKey(timeSpan))
            {
                _timerList.Add(timeSpan, new Lazy<IConnectableObservable<DateTime>>(() => Observable.Timer(TimeSpan.FromMilliseconds(0), timeSpan).Timestamp().Select(x => x.Timestamp.DateTime).Publish()));
                _timerList[timeSpan].Value.Connect();
            }

            return _timerList[timeSpan].Value;
        }
    }
}
