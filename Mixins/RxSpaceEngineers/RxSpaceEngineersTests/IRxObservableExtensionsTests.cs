using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IngameScript.Program;
using Xunit;
using IngameScript.Utilities;

namespace IngameScript
{
    public class IRxObservableExtensionsTests
    {
        private RxSubject<int> Subject { get; } = new RxSubject<int>();

        [Fact]
        public void Where_OnNextOnCompleted()
        {
            var test = new Verifier<int>
            {
                Observable = Subject.Where(x => x % 3 == 0),
                Expected =
                {
                    RxEvent.OnNext(3),
                    RxEvent.OnNext(6),
                    RxEvent.OnNext(9),
                    RxEvent.OnNext(12),
                    RxEvent.OnCompleted
                }
            };

            Subject.OnNext(1);
            Subject.OnNext(2);
            Subject.OnNext(3);
            Subject.OnNext(6);
            Subject.OnNext(25);
            Subject.OnNext(9);
            Subject.OnNext(12);
            Subject.OnNext(31);
            Subject.OnCompleted();
            Subject.OnNext(15);
            Subject.OnError(new Exception());

            test.Verify();
        }

        [Fact]
        public void Where_OnNextOnError()
        {
            var error = new Exception();

            var test = new Verifier<int>
            {
                Observable = Subject.Where(x => x % 3 == 0),
                Expected =
                {
                    RxEvent.OnNext(3),
                    RxEvent.OnNext(6),
                    RxEvent.OnNext(9),
                    RxEvent.OnNext(12),
                    RxEvent.OnError(error)
                }
            };

            Subject.OnNext(1);
            Subject.OnNext(2);
            Subject.OnNext(3);
            Subject.OnNext(6);
            Subject.OnNext(25);
            Subject.OnNext(9);
            Subject.OnNext(12);
            Subject.OnNext(31);
            Subject.OnError(error);
            Subject.OnNext(15);
            Subject.OnNext(14);
            Subject.OnCompleted();

            test.Verify();
        }

        [Fact]
        public void Select_OnNextOnCompleted()
        {
            var test = new Verifier<string>
            {
                Observable = Subject.Select(x => x.ToString()),
                Expected =
                {
                    RxEvent.OnNext("1"),
                    RxEvent.OnNext("7"),
                    RxEvent.OnNext("91"),
                    RxEvent.OnNext("-4096"),
                    RxEvent.OnCompleted
                }
            };

            Subject.OnNext(1);
            Subject.OnNext(7);
            Subject.OnNext(91);
            Subject.OnNext(-4096);
            Subject.OnCompleted();
            Subject.OnNext(-666);
            Subject.OnError(new Exception());
            Subject.OnNext(445);

            test.Verify();
        }

        [Fact]
        public void Select_OnNextOnError()
        {
            var error = new Exception();
            var test = new Verifier<string>
            {
                Observable = Subject.Select(x => x.ToString()),
                Expected =
                {
                    RxEvent.OnNext("1"),
                    RxEvent.OnNext("7"),
                    RxEvent.OnNext("91"),
                    RxEvent.OnNext("-4096"),
                    RxEvent.OnError(error)
                }
            };

            Subject.OnNext(1);
            Subject.OnNext(7);
            Subject.OnNext(91);
            Subject.OnNext(-4096);
            Subject.OnError(error);
            Subject.OnNext(-666);
            Subject.OnCompleted();
            Subject.OnNext(445);

            test.Verify();
        }
    }
}
