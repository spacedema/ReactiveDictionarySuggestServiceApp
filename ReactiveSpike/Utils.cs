using System;
using System.Reactive;
using System.Reactive.Linq;

namespace ReactiveExercises
{
    public static class Utils
    {
        public static IObservable<T> RemoveTimestamp<T>(this IObservable<Timestamped<T>> This)
        {
            return This.Select(x => x.Value);
        }

        public static IObservable<T> LogTimestampedValues<T>(this IObservable<T> source, Action<Timestamped<T>> onNext)
        {
            return source.Timestamp().Do(onNext).RemoveTimestamp();
        }
    }
}