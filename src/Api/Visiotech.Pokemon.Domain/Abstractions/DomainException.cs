namespace Visiotech.Pokemon.Domain.Abstractions;

public sealed class DomainException(string message) : Exception(message);

