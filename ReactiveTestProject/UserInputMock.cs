using System;
using System.Reactive.Linq;

namespace ReactiveTestProject
{
    public class UserInputMock
    {
        public IObservable<string> Input()
        {
            const string input = "reactive";
            var timer = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
            var source = Observable.Range(1, 6);
            var timedSource = source.Zip(timer, (s, t) => input.Substring(0, 2) + input.Substring(2, s));
            return timedSource;
        }
    }
}
