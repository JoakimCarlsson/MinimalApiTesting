namespace MinimalApiTesting.Dtos;

public record TodoItemOutput(string? Title, bool IsCompleted, DateTimeOffset? createdOn);