using System;
using System.Linq;
using NUnit.Framework;
using Paramore.Brighter.Tests.TestDoubles;

namespace Paramore.Brighter.Tests
{
    [TestFixture]
    public class PipelineCleanupTests
    {
        private PipelineBuilder<MyCommand> _pipeline_Builder;
        private static string s_released;

        [SetUp]
        public void Establish()
        {
            s_released = string.Empty;

            var registry = new SubscriberRegistry();
            registry.Register<MyCommand, MyPreAndPostDecoratedHandler>();
            registry.Register<MyCommand, MyLoggingHandler<MyCommand>>();

            var handlerFactory = new CheapHandlerFactory();

            _pipeline_Builder = new PipelineBuilder<MyCommand>(registry, handlerFactory);
            _pipeline_Builder.Build(new RequestContext()).Any();
        }

        internal class CheapHandlerFactory : IAmAHandlerFactory
        {
            public IHandleRequests Create(Type handlerType)
            {
                if (handlerType == typeof(MyPreAndPostDecoratedHandler))
                {
                    return new MyPreAndPostDecoratedHandler();
                }
                if (handlerType == typeof(MyLoggingHandler<MyCommand>))
                {
                    return new MyLoggingHandler<MyCommand>();
                }
                if (handlerType == typeof(MyValidationHandler<MyCommand>))
                {
                    return new MyValidationHandler<MyCommand>();
                }
                return null;
            }

            public void Release(IHandleRequests handler)
            {
                var disposable = handler as IDisposable;
                disposable?.Dispose();

                s_released += "|" + handler.Name;
            }
        }


        [Test]
        public void When_We_Have_Exercised_The_Pipeline_Cleanup_Its_Handlers()
        {
            _pipeline_Builder.Dispose();

            //_should_have_called_dispose_on_instances_from_ioc
            Assert.True(MyPreAndPostDecoratedHandler.DisposeWasCalled);
            //_should_have_called_dispose_on_instances_from_pipeline_builder
            Assert.True(MyLoggingHandler<MyCommand>.DisposeWasCalled);
            //_should_have_called_release_on_all_handlers
            Assert.AreEqual("|MyValidationHandler`1|MyPreAndPostDecoratedHandler|MyLoggingHandler`1|MyLoggingHandler`1", s_released);
        }
    }
}