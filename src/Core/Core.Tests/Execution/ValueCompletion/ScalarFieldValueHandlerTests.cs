﻿using System;
using HotChocolate.Types;
using HotChocolate.Utilities;
using Moq;
using Xunit;

namespace HotChocolate.Execution
{
    public class ScalarFieldValueHandlerTests
    {
        [Fact]
        public void CompleteStringScalarValue_ShouldSerializeValue()
        {
            // arrange
            string expectedResult = Guid.NewGuid().ToString();
            object result = null;
            bool nextHandlerIsRaised = false;

            var stringType = new StringType();

            var context = new Mock<IFieldValueCompletionContext>(
                MockBehavior.Strict);
            context.Setup(t => t.Type).Returns(stringType);
            context.Setup(t => t.Value).Returns(expectedResult);
            context.Setup(t => t.Converter).Returns(TypeConversion.Default);
            context.Setup(t => t.IntegrateResult(Moq.It.IsAny<string>()))
                .Callback(new Action<object>(v =>
                {
                    result = v;
                }));

            // act
            var handler = new ScalarFieldValueHandler();
            handler.CompleteValue(context.Object,
                c => nextHandlerIsRaised = true);

            // assert
            Assert.Equal(expectedResult, result);
            Assert.False(nextHandlerIsRaised);
        }

        [Fact]
        public void CompleteEnumValueValue_ShouldSerializeValue()
        {
            // arrange
            string expectedResult = "ABC";
            object result = null;
            bool nextHandlerIsRaised = false;

            var enumType = new EnumType(d =>
            {
                d.Name("Foo");
                d.Item("ABC");
            });

            var context = new Mock<IFieldValueCompletionContext>(
                MockBehavior.Strict);
            context.Setup(t => t.Type).Returns(enumType);
            context.Setup(t => t.Value).Returns(expectedResult);
            context.Setup(t => t.Converter).Returns(TypeConversion.Default);
            context.Setup(t => t.IntegrateResult(It.IsAny<string>()))
                .Callback(new Action<object>(v =>
                {
                    result = v;
                }));

            // act
            var handler = new ScalarFieldValueHandler();
            handler.CompleteValue(context.Object,
                c => nextHandlerIsRaised = true);

            // assert
            Assert.Equal(expectedResult, result);
            Assert.False(nextHandlerIsRaised);
        }

        [Fact]
        public void CompleteListOfStringValue_ShouldDelegateToNextHandler()
        {
            // arrange
            string resolverValue = Guid.NewGuid().ToString();
            object result = null;
            bool nextHandlerIsRaised = false;

            var listType = new ListType(new StringType());

            var context = new Mock<IFieldValueCompletionContext>(
                MockBehavior.Strict);
            context.Setup(t => t.Type).Returns(listType);
            context.Setup(t => t.Value).Returns(resolverValue);
            context.Setup(t => t.IntegrateResult(Moq.It.IsAny<string>()))
                .Callback(new Action<object>(v =>
                {
                    result = v;
                }));

            // act
            var handler = new ScalarFieldValueHandler();
            handler.CompleteValue(context.Object,
                c => nextHandlerIsRaised = true);

            // assert
            Assert.Null(result);
            Assert.True(nextHandlerIsRaised);
        }
    }
}
