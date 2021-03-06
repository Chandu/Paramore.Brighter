﻿#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using NUnit.Framework;
using Paramore.Brighter.Policies.Handlers;
using Paramore.Brighter.Tests.ExceptionPolicy.TestDoubles;
using Paramore.Brighter.Tests.TestDoubles;
using Polly;
using Polly.CircuitBreaker;
using TinyIoC;

namespace Paramore.Brighter.Tests.ExceptionPolicy
{
    [TestFixture]
    public class CommandProcessorWithCircuitBreakerTests
    {
        private CommandProcessor _commandProcessor;
        private readonly MyCommand _myCommand = new MyCommand();
        private Exception _thirdException;
        private Exception _firstException;
        private Exception _secondException;

        [SetUp]
        public void Establish()
        {
            var registry = new SubscriberRegistry();
            registry.Register<MyCommand, MyFailsWithDivideByZeroHandler>();

            var container = new TinyIoCContainer();
            var handlerFactory = new TinyIocHandlerFactory(container);
            container.Register<IHandleRequests<MyCommand>, MyFailsWithDivideByZeroHandler>().AsSingleton();
            container.Register<IHandleRequests<MyCommand>, ExceptionPolicyHandler<MyCommand>>().AsSingleton();

            var policyRegistry = new PolicyRegistry();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            policyRegistry.Add("MyDivideByZeroPolicy", policy);

            MyFailsWithDivideByZeroHandler.ReceivedCommand = false;

            _commandProcessor = new CommandProcessor(registry, handlerFactory, new InMemoryRequestContextFactory(), policyRegistry);
        }

        //We have to catch the final exception that bubbles out after retry
        [Test]
        public void When_Sending_A_Command_That_Repeatedely_Fails_Break_The_Circuit()
        {
                //First two should be caught, and increment the count
                _firstException = Catch.Exception(() => _commandProcessor.Send(_myCommand));
                _secondException = Catch.Exception(() => _commandProcessor.Send(_myCommand));
                //this one should tell us that the circuit is broken
                _thirdException = Catch.Exception(() => _commandProcessor.Send(_myCommand));

                //_should_send_the_command_to_the_command_handler
            Assert.True(MyFailsWithDivideByZeroHandler.ShouldReceive(_myCommand));
            //_should_bubble_up_the_first_exception
            Assert.IsInstanceOf<DivideByZeroException>(_firstException);
            //_should_bubble_up_the_second_exception
            Assert.IsInstanceOf<DivideByZeroException>(_secondException);
            //_should_break_the_circuit_after_two_fails
            Assert.IsInstanceOf<BrokenCircuitException>(_thirdException);
        }
    }
}
