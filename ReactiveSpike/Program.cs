using System;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using ReactiveExercises;

namespace ReactiveSpike
{
    class Program
    {
        static void Main()
        {
            var lbl = new Label();
            var frm = new Form1
            {
                Controls = { lbl }
            };

            IObservable<Point> move = Observable.FromEventPattern<MouseEventArgs>(frm, "MouseMove")
                .Select(x => x.EventArgs.Location)
                .Where(x => x.X == x.Y);

            IObservable<string> input = Observable.FromEventPattern<EventArgs>(frm.textBox1, "TextChanged")
                .Select(x => ((TextBox)x.Sender).Text)
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged();

            // ReSharper disable once RedundantAssignment
            IDisposable inputSubscribtion = input.Subscribe(x =>
            {
                lbl.BeginInvoke((MethodInvoker)delegate { lbl.Text = "User wrote: " + x; });
                Console.WriteLine("User wrote: " + x);
            });
            // Or
            inputSubscribtion = input.ObserveOn(SynchronizationContext.Current).Subscribe(x =>
            {
                lbl.Text = "User wrote: " + x;
                Console.WriteLine("User wrote: " + x);
            });

            IDisposable moveSubscription = move.Subscribe(x =>
            {
                Console.WriteLine(x);
            });

            using (new CompositeDisposable(inputSubscribtion, moveSubscription))
            {
                Application.Run(frm);
            }
        }
    }
}
