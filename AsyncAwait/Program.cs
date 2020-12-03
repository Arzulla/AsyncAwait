using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwait
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Main : {Thread.CurrentThread.ManagedThreadId}");
            SomeClass someClass = new SomeClass();

            someClass.SomeOperationAsync();
            //someClass.SomeOperation();

            Console.WriteLine($"-------   {Thread.CurrentThread.ManagedThreadId}");            
            Console.ReadLine();
        }
    }

    class SomeClass
    {
        public int SomeOperation()
        {
            Console.WriteLine($"Operation : {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine("Begin");

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine($"End : {Thread.CurrentThread.ManagedThreadId}");

            return 44;
        }

        public Task SomeOperationAsync()
        {
            AsyncStateMachine stateMachine = new AsyncStateMachine();
            stateMachine.outer = this;
            stateMachine.state = -1;
            stateMachine.builder = AsyncTaskMethodBuilder.Create();
            stateMachine.builder.Start(ref stateMachine);

            return stateMachine.builder.Task;
        }

        private struct AsyncStateMachine : IAsyncStateMachine
        {
            public int state;
            public SomeClass outer;

            private TaskAwaiter<int> awaiter;
            public AsyncTaskMethodBuilder builder;

            public void MoveNext()
            {
                if (state == -1)
                {
                    Console.WriteLine($"Operation par1 : {Thread.CurrentThread.ManagedThreadId}");

                    Task<int> t = Task.Factory.StartNew(outer.SomeOperation);
                    awaiter = t.GetAwaiter();
                    state = 0;
                    builder.AwaitOnCompleted(ref awaiter, ref this);
                }
                else
                {
                    
                    var result = awaiter.GetResult();
                    Console.WriteLine($"Operation par2  : {Thread.CurrentThread.ManagedThreadId}  result: {result}");
                }
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                builder.SetStateMachine(stateMachine);
            }
        }
    }
}
