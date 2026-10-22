// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Tests.Shared.Builders;

/// <summary>
/// Base for the fluent test-data builders (TESTING §6.1). A builder starts with valid,
/// deterministic defaults, so a test sets only the values it is about.
/// </summary>
/// <remarks>
/// Builders create objects in memory. Persisting them goes through the real <c>DbContext</c>
/// (never raw SQL), added together with the integration fixture.
/// </remarks>
/// <typeparam name="T">The type the builder creates.</typeparam>
public abstract class Builder<T>
{
    /// <summary>Creates a new instance from the current builder state.</summary>
    public abstract T Build();

    /// <summary>Lets a builder be passed where the built object is expected.</summary>
    public static implicit operator T(Builder<T> builder) => builder.Build();
}
