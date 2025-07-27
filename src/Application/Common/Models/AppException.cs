namespace VerticalSliceArchitecture.Application.Common.Models;

public class AppException : Exception
{
    public AppException(string message)
        : base(message) { }
}