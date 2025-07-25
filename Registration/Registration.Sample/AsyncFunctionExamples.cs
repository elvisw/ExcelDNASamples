﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ExcelDna.Integration;
using ExcelDna.Registration;
using ExcelDna.Registration.Utils;

namespace Registration.Sample
{
    public static class AsyncFunctionExamples
    {
        // Will not be registered in Excel by Excel-DNA, since ExplicitExports="true".
        public static string dnaSayHello(string name)
        {
            return "Hello " + name + "!";
        }

        // Will be picked up by our explicit processing, no conversions applied, and normal registration
        [ExcelFunction(Name = "dnaSayHello")]
        public static string dnaSayHello2(string name)
        {
            if (name == "Bang!") throw new ArgumentException("Bad name!");
            return "Hello " + name + "!";
        }

        // A simple function that can take a long time to complete.
        // Will be wrapped to RunAsTask, via Task.Factory.StartNew(...)
        [ExcelAsyncFunction(Name = "dnaDelayedHello")]
        public static string dnaDelayedHello(string name, int msToSleep)
        {
            Thread.Sleep(msToSleep);
            return "Hello " + name + "!";
        }

        // Explicitly marked with ExcelAsyncFunction, so it will be wrapped by the Registration processing
        // If we marked this function with [ExcelFunction] instead of [ExcelAsyncFunction] it would
        // not be wrapped (since it doesn't return Task or IObservable).
        [ExcelAsyncFunction(Name = "dnaDelayedHelloAsync", Description = "A friendly async function")]
        public static string dnaDelayedHello2(string name, int msToSleep)
        {
            Thread.Sleep(msToSleep);
            return "Hello " + name + "!";
        }

        // A function that returns a Task<T> will be wrapped by the Registration processing
        // It doesn't matter if this function is marked with ExcelFunction or ExcelAsyncFunction
        [ExcelFunction]
        public static async Task<string> dnaDelayedTaskHello(string name, int msDelay)
        {
            await Task.Delay(msDelay);
            // Be careful to note that the await continuation will run on the thread pool, not the Excel main thread.
            return "Hello " + name;
        }

        // A function that returns a Task<T>, takes a CancellationToken as last parameter, and will be wrapped by the Registration processing
        // It doesn't matter if this function is marked with ExcelFunction or ExcelAsyncFunction.
        // Whether the registration uses the native async under Excel 2010+ will make a big difference to the cancellation here!
        [ExcelFunction]
        public static async Task<string> dnaDelayedTaskHelloWithCancellation(string name, int msDelay, CancellationToken ct)
        {
            ct.Register(() => Debug.Print("Cancelled!"));

            await Task.Delay(msDelay);
            return "Hello" + name;
        }

        // This is what the Task wrappers that are generated look like.
        // Can use the same Task helper explicitly here.
        [ExcelFunction]
        public static object dnaExplicitWrap(string name, int msDelay)
        {
            if (ExcelDnaUtil.IsInFunctionWizard())
                return "!#WIZARD#!";

            return AsyncTaskUtil.RunTask("dnaExplicitWrap", new object[] { name, msDelay }, async () =>
                await dnaDelayedTaskHello(name, msDelay)
            );
        }
    }
}
