using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ReactiveDictionarySuggestService.DictionarySuggestService;

namespace ReactiveDictionarySuggestService
{
    public partial class Form1 : Form
    {
        public DictServiceSoap Svc { get; set; }
        private IDisposable _disposableResult;
        private const string DictionaryId = "wn";
        private const string SearchStrategy = "prefix";

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AllocConsole();
            Init();
            //ResponseOrderTest();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _disposableResult?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Init()
        {
            IObservable<string> input = Observable.FromEventPattern<EventArgs>(txt, "TextChanged")
                   .Select(evt => ((TextBox)evt.Sender).Text)
                   .Throttle(TimeSpan.FromSeconds(1))
                   //.Where(x => x.Length > 3)
                   .DistinctUntilChanged()
                   .Do(x => Console.WriteLine($"Sending to web service: {x}"));

            IObservable<DictionaryWord[]> result = input
                .SelectMany(word => LookupAsync(word)
                    .Catch((Exception ex) =>
                    {
                        MessageBox.Show(
                            @"A handled error occurred: " + ex.Message, Text,
                            MessageBoxButtons.OK, MessageBoxIcon.Error
                            );

                        return Observable.Empty<DictionaryWord[]>();
                    })
                    // TakeUntil testing
                    .Finally(() => Console.WriteLine(@"Disposed request for " + word))
                    .TakeUntil(input)
                    );


            // Which approach one choses is solely dependent on personal taste. Both are equally good at solving the issue.
            // ReSharper disable once UnusedVariable
            IObservable<DictionaryWord[]> result1 = input.SelectMany(LookupAsync).TakeUntil(input);
            // ReSharper disable once UnusedVariable
            IObservable<DictionaryWord[]> result2 = input.Select(LookupAsync).Switch();

            _disposableResult = result.ObserveOn(SynchronizationContext.Current)
                // Ignore all exceptions and resubscribe
                //.Retry()
                .Subscribe(words =>
                {
                    lst.Items.Clear();
                    var list = words.Select(word => word.Word).ToArray<object>();
                    lst.Items.AddRange(list);
                },
                ex =>
                {
                    MessageBox.Show(
                        @"An unhandled error occurred: " + ex.Message, Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                        );
                } 
            );
        }

        private IObservable<DictionaryWord[]> LookupAsync(string wordToSearch)
        {
            if (Svc == null)
                Svc = new DictServiceSoapClient();

                // For testing TakeUntil use .Delay(TimeSpan.FromSeconds(5)) for delay emulating
                return Svc.MatchInDictAsync(DictionaryId, wordToSearch, SearchStrategy).ToObservable();//.Delay(TimeSpan.FromSeconds(5));
        }

        // While requests for “rea”, “reac”, “react”, “reacti”, “reactiv” and “reactive” are started in that order, results may come back in a different order as shown below:
        public void ResponseOrderTest()
        {
            if (Svc == null)
                Svc = new DictServiceSoapClient();

            const string input = "reactive";
            for (var len = 3; len <= input.Length; len++)
            {
                var req = input.Substring(0, len);
                LookupAsync(req).Subscribe(
                    words => Console.WriteLine(req + @" --> " + words.Length)
                    );
            }
        }
    }
}
