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
using Nito.AsyncEx;
using NUnit.Framework;
using Paramore.Brighter.Tests.TestDoubles;
using TinyIoC;

namespace Paramore.Brighter.Tests
{
    [TestFixture]
    public class CommandProcessorNoHandlersMatchAsyncTests
    {
        private CommandProcessor _commandProcessor;
        private readonly MyCommand _myCommand = new MyCommand();
        private Exception _exception;

        [SetUp]
        public void Establish()
        {
            _commandProcessor = new CommandProcessor(new SubscriberRegistry(), new TinyIocHandlerFactoryAsync(new TinyIoCContainer()), new InMemoryRequestContextFactory(), new PolicyRegistry());
        }

        //Ignore any errors about adding System.Runtime from the IDE. See https://social.msdn.microsoft.com/Forums/en-US/af4dc0db-046c-4728-bfe0-60ceb93f7b9f/vs2012net-45-rc-compiler-error-when-using-actionblock-missing-reference-to?forum=tpldataflow
        [Test]
        public void When_There_Are_No_Command_Handlers_Async()
        {
            _exception = Catch.Exception(() => AsyncContext.Run(async () => await _commandProcessor.SendAsync(_myCommand)));

            //_should_fail_because_multiple_receivers_found
            Assert.IsAssignableFrom(typeof(ArgumentException), _exception);
            //_should_have_an_error_message_that_tells_you_why
            Assert.NotNull(_exception);
            StringAssert.Contains("No command handler was found for the typeof command Paramore.Brighter.Tests.TestDoubles.MyCommand - a command should have exactly one handler.", _exception.Message);
        }
    }
}
