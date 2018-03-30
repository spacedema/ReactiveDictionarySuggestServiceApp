using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ReactiveDictionarySuggestService;
using ReactiveDictionarySuggestService.DictionarySuggestService;

namespace ReactiveTestProject
{
    [TestClass]
    public class GenerateSequencesTest
    {
        [TestMethod]
        public void TestMethod()
        {
            var mockRepo = new Mock<DictServiceSoap>();
            mockRepo
                .Setup(x => x.MatchInDictAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string dictId, string word, string strategy) => MatchInDictMock(dictId, word, strategy));

            var inputMock = new UserInputMock();

            var frm = new Form1 {Svc = mockRepo.Object};
            var input = inputMock.Input();
            using (input
                .Finally(frm.Close)
                .Subscribe(x =>
                {
                    frm.txt.Text = x;
                }))
            {
                Application.Run(frm);
            }

        }

        public Task<DictionaryWord[]> MatchInDictMock(string dictId, string word, string strategy)
        {
            var rand = new Random();
            var list = new List<DictionaryWord>();
            foreach (var i in Enumerable.Range(0, rand.Next(5, 10)))
                list.Add(new DictionaryWord { Word = word + i });

            return Observable.Return(list.ToArray()).Delay(TimeSpan.FromMilliseconds(rand.Next(0, 500))).ToTask();
        }
    }
}
