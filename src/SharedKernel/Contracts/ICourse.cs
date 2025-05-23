using System;

namespace FluencyHub.SharedKernel.Contracts;

public interface ICourse
{
    Guid Id { get; }
    string Name { get; }
    string Description { get; }
    string Language { get; }
    string Level { get; }
    decimal Price { get; }
    bool IsActive { get; }
} 