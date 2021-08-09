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
    public class SubjectTests
    {
        public class ActiveSubject
        {
            public ActiveSubject()
            {
                Subject = new RxSubject<int>();
            }

            private RxSubject<int> Subject { get; }

            [Fact]
            public void OnNext_PushesEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnNext(7),
                        RxEvent.OnNext(14),
                        RxEvent.OnNext(21),
                        RxEvent.OnNext(28)
                    }
                };

                Subject.OnNext(7);
                Subject.OnNext(14);
                Subject.OnNext(21);
                Subject.OnNext(28);

                test.Verify();
            }

            [Fact]
            public void OnError_PushesEvents()
            {
                var error = new Exception();
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnNext(12345),
                        RxEvent.OnNext(75),
                        RxEvent.OnError(error)
                    }
                };

                Subject.OnNext(12345);
                Subject.OnNext(75);
                Subject.OnError(error);

                test.Verify();
            }

            [Fact]
            public void OnCompleted_PushesEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnNext(-32),
                        RxEvent.OnCompleted
                    }
                };

                Subject.OnNext(-32);
                Subject.OnCompleted();

                test.Verify();
            }

            [Fact]
            public void OnNext_StopsPushing_AfterUnsubscribe()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnNext(25)
                    }
                };

                Subject.OnNext(25);
                test.Unsubscribe();
                Subject.OnNext(400);

                test.Verify();
            }

            [Fact]
            public void OnError_StopsPushing_AfterUnsubscribe()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnNext(77)
                    }
                };

                Subject.OnNext(77);
                test.Unsubscribe();
                Subject.OnError(new Exception());

                test.Verify();
            }

            [Fact]
            public void OnCompleted_StopsPushing_AfterUnsubscribe()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnNext(39)
                    }
                };

                Subject.OnNext(39);
                test.Unsubscribe();
                Subject.OnCompleted();

                test.Verify();
            }
        }

        public class FaultedSubject
        {
            public FaultedSubject()
            {
                Subject = new RxSubject<int>();
                Subject.OnError(new Exception());
            }

            private RxSubject<int> Subject { get; }

            [Fact]
            public void DoesNotPush_OnNextEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected = { }
                };

                Subject.OnNext(71);

                test.Verify();
            }

            [Fact]
            public void DoesNotPush_OnErrorEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected = { }
                };

                Subject.OnError(new Exception());

                test.Verify();
            }

            [Fact]
            public void DoesNotPush_OnCompletedEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected = { }
                };

                Subject.OnCompleted();

                test.Verify();
            }
        }

        public sealed class CompletedSubject
        {
            public CompletedSubject()
            {
                Subject = new RxSubject<int>();
                Subject.OnCompleted();
            }

            private RxSubject<int> Subject { get; }

            [Fact]
            public void NewSubscribers_GetSingleOnCompletedEvent()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnCompleted
                    }
                };

                test.Verify();
            }

            [Fact]
            public void DoesNotPush_OnNextEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnCompleted
                    }
                };

                Subject.OnNext(888);

                test.Verify();
            }

            [Fact]
            public void DoesNotPush_OnErrorEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnCompleted
                    }
                };

                Subject.OnError(new Exception());

                test.Verify();
            }

            [Fact]
            public void DoesNotPush_AdditionalOnCompletedEvents()
            {
                var test = new Verifier<int>
                {
                    Observable = Subject,
                    Expected =
                    {
                        RxEvent.OnCompleted
                    }
                };

                Subject.OnCompleted();
                Subject.OnCompleted();

                test.Verify();
            }
        }
    }
}
