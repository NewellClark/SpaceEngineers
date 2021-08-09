using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IngameScript.Program;
using Xunit;
using VRage.Library.Utils;

namespace IngameScript
{
    public class DisposableTests
    {
        [Fact]
        public void Create_InvokesDisposeAction()
        {
            bool invoked = false;
            var disposable = RxDisposable.Create(() => invoked = true);

            disposable.Dispose();

            Assert.True(invoked);
        }

        [Fact]
        public void Create_IsIdempotent()
        {
            int count = 0;
            var disposable = RxDisposable.Create(() => count++);

            disposable.Dispose();
            disposable.Dispose();
            disposable.Dispose();

            Assert.Equal(1, count);
        }
    }
}
