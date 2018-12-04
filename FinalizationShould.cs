namespace GCFinalize
{
    using System;

    using NUnit.Framework;

    public class MyDisposable : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            {
                // if (disposing)
                TestContext.WriteLine("dispose called");
            }
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        ~MyDisposable()
        {
            Dispose(false);
        }
    }

    public class MyFinalizeObject
    {
        // Make this number very large to cause the finalizer to
        // do more work.
        private const int maxIterations = 10000;

        private readonly MyDisposable _disposable;

        public MyFinalizeObject(MyDisposable disposable)
        {
            _disposable = disposable;
        }

        ~MyFinalizeObject()
        {
            Console.WriteLine("Finalizing a MyFinalizeObject");

            // Do some work.
            for (int i = 0; i < maxIterations; i++)
            {
                // This method performs no operation on i, but prevents 
                // the JIT compiler from optimizing away the code inside 
                // the loop.
                GC.KeepAlive(i);
            }
        }
    }

    [TestFixture]
    public class FinalizationShould
    {
        [Test]
        public void Call_finalizer()
        {
            DoSomethingMF();

            // Force garbage collection.
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            // Without this call to GC.WaitForPendingFinalizers, 
            // the worker loop below might execute at the same time 
            // as the finalizers.
            // With this call, the worker loop executes only after
            // all finalizers have been called.
            GC.WaitForPendingFinalizers();
        }

        private static void DoSomethingMF()
        {
            var myFinalizeObject = new MyFinalizeObject(new MyDisposable());
            var toto = myFinalizeObject;

            // toto = null;
        }
    }
}