
namespace SharedKernel.Common
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);

        public static Error NotFound(string message) =>
            new("NOT_FOUND", message);

        public static Error Validation(string message) =>
            new("VALIDATION_ERROR", message);

        public static Error Unauthorized(string message) =>
            new("UNAUTHORIZED", message);

        public static Error Conflict(string message) =>
            new("CONFLICT", message);

        public static Error Internal(string message) =>
            new("INTERNAL_ERROR", message);
    }
}
