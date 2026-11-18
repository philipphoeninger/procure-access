// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Workspace.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Workspace;

/// <summary>
/// The rules of a procurement project (SPEC R-PRJ-01, R-PRJ-05, §7.2; S6-11): its name, its three
/// statuses, and that an archived project only changes its status.
/// </summary>
public class ProcurementProjectTests
{
    private static readonly Guid Organization = Guid.NewGuid();
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = CreatedAt.AddHours(1);

    [Fact]
    public void Create_ValidName_IsOpenAndUnchangedSinceItsCreation()
    {
        var project = ProcurementProject.Create(Organization, new string('a', ProcurementProject.NameMaxLength), "user-1", CreatedAt);

        project.ShouldSatisfyAllConditions(
            p => p.Id.ShouldNotBe(Guid.Empty),
            p => p.OrganizationId.ShouldBe(Organization),
            p => p.Status.ShouldBe(ProjectStatus.Open),
            p => p.IsArchived.ShouldBeFalse(),
            p => p.CreatedByUserId.ShouldBe("user-1"),
            p => p.CreatedAt.ShouldBe(CreatedAt),
            p => p.ModifiedAt.ShouldBe(CreatedAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_Throws(string name) =>
        Should.Throw<ArgumentException>(() => ProcurementProject.Create(Organization, name, "user-1", CreatedAt));

    [Fact]
    public void Create_NameAboveTheLimitOrNoOrganizationOrNoCreator_Throws()
    {
        var tooLong = new string('a', ProcurementProject.NameMaxLength + 1);

        Should.Throw<ArgumentException>(() => ProcurementProject.Create(Organization, tooLong, "user-1", CreatedAt));
        Should.Throw<ArgumentException>(() => ProcurementProject.Create(Guid.Empty, "Projekt", "user-1", CreatedAt));
        Should.Throw<ArgumentException>(() => ProcurementProject.Create(Organization, "Projekt", " ", CreatedAt));
    }

    [Fact]
    public void Rename_NewName_ChangesNameAndLastChange()
    {
        var project = Project();

        project.Rename("Neuer Name", Later);

        project.Name.ShouldBe("Neuer Name");
        project.ModifiedAt.ShouldBe(Later);
        project.CreatedAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void Rename_SameName_ChangesNothing()
    {
        var project = Project();

        project.Rename(project.Name, Later);

        project.ModifiedAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void Rename_EmptyOrTooLongName_ThrowsAndKeepsTheName()
    {
        var project = Project();

        Should.Throw<ArgumentException>(() => project.Rename(" ", Later));
        Should.Throw<ArgumentException>(() => project.Rename(new string('a', ProcurementProject.NameMaxLength + 1), Later));

        project.Name.ShouldBe("Projekt");
        project.ModifiedAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void Rename_ArchivedProject_Throws()
    {
        var project = Project();
        project.ChangeStatus(ProjectStatus.Archived, Later);

        Should.Throw<InvalidOperationException>(() => project.Rename("Neuer Name", Later.AddHours(1)));

        project.Name.ShouldBe("Projekt");
    }

    [Theory]
    [InlineData("Open", "Done")]
    [InlineData("Open", "Archived")]
    [InlineData("Done", "Open")]
    [InlineData("Done", "Archived")]
    [InlineData("Archived", "Open")]
    [InlineData("Archived", "Done")]
    public void ChangeStatus_AnyOtherStatus_ChangesStatusAndLastChange(string fromName, string toName)
    {
        var (from, to) = (Enum.Parse<ProjectStatus>(fromName), Enum.Parse<ProjectStatus>(toName));
        var project = Project();
        project.ChangeStatus(from, CreatedAt);

        project.ChangeStatus(to, Later);

        project.Status.ShouldBe(to);
        project.IsArchived.ShouldBe(to == ProjectStatus.Archived);
        project.ModifiedAt.ShouldBe(Later);
    }

    [Fact]
    public void ChangeStatus_SameOrUnknownStatus_ChangesNothing()
    {
        var project = Project();

        project.ChangeStatus(ProjectStatus.Open, Later);
        Should.Throw<ArgumentOutOfRangeException>(() => project.ChangeStatus((ProjectStatus)99, Later));

        project.Status.ShouldBe(ProjectStatus.Open);
        project.ModifiedAt.ShouldBe(CreatedAt);
    }

    private static ProcurementProject Project() => ProcurementProject.Create(Organization, "Projekt", "user-1", CreatedAt);
}
