﻿using System;
using System.Diagnostics;
using ExcelDna.Integration;
using ExcelDna.Registration;

namespace Registration.Sample
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TimingAttribute : Attribute
    {
    }

    public class TimingFunctionExecutionHandler : FunctionExecutionHandler
    {
        public override void OnEntry(FunctionExecutionArgs args)
        {
            args.Tag = Stopwatch.StartNew();
        }

        public override void OnExit(FunctionExecutionArgs args)
        {
            var sw = (Stopwatch)args.Tag;
            sw.Stop();

            Debug.WriteLine("{0} executed in {1} milliseconds", args.FunctionName, sw.ElapsedMilliseconds);
        }


        /////////////////////// Registration handler //////////////////////////////////

        // In this case, we only ever make one 'handler' object
        static readonly Lazy<TimingFunctionExecutionHandler> _handler =
            new Lazy<TimingFunctionExecutionHandler>(() => new TimingFunctionExecutionHandler());

        [ExcelFunctionExecutionHandlerSelector]
        public static IFunctionExecutionHandler TimingHandlerSelector(IExcelFunctionInfo functionRegistration)
        {
            // Eat the TimingAttributes, and return a timer handler if there were any
            if (functionRegistration.CustomAttributes.RemoveAll(att => att is TimingAttribute) == 0)
            {
                // No attributes
                return null;
            }
            return _handler.Value;
        }
    }
}
