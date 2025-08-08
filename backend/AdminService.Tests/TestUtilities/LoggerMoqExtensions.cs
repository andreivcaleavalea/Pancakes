using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace AdminService.Tests.TestUtilities;

public static class LoggerMoqExtensions
{
    public static void VerifyLogContains<T>(
        this Mock<ILogger<T>> loggerMock,
        LogLevel level,
        string expectedMessageSubstring,
        Exception? expectedException,
        Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() != null && state.ToString()!.Contains(expectedMessageSubstring)),
                It.Is<Exception?>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}


