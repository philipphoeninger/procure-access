// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Tests.Integration.Infrastructure;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// One PostgreSQL container and one API host for the whole test run (TESTING §5.2).
[assembly: AssemblyFixture(typeof(ApiFixture))]

// All test classes share one database, and Respawn empties it before each class.
// Classes running in parallel would delete each other's data.
[assembly: Parallelization(Mode = ParallelMode.None)]
