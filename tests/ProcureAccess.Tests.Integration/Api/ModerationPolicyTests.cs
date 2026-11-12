// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.Entities.Requests;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Builders;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// TEN-06, the part that exists before step 3 (owner decision O-S4-84): the only endpoint of a
/// platform role today, the legacy <c>POST /api/Proposals/approve</c>, needs the permission
/// <c>moderation:review</c> of Moderator and PlatformAdmin through the policy
/// <c>ModerationReview</c> (O-S4-85). Before S4-25 a missing permission threw and answered 500.
/// </summary>
public sealed class ModerationPolicyTests(ApiFixture api) : IntegrationTest(api)
{
    private const string ApprovePath = "/api/Proposals/approve";

    [Fact]
    public async Task ApproveProposal_Anonymous_Returns401ProblemDetails()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(ApprovePath, Command(1), cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
    }

    [Fact(DisplayName = "INV:TEN-06 " + nameof(ApproveProposal_SignedInWithoutModerationReview_Returns403AndProposalStays))]
    [Trait("Invariant", "TEN-06")]
    public async Task ApproveProposal_SignedInWithoutModerationReview_Returns403AndProposalStays()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var proposalId = await StorePendingProposalAsync();
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PostAsJsonAsync(ApprovePath, Command(proposalId), cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
        (await StatusAsync(proposalId, cancellationToken)).ShouldBe(ProposalStatus.Pending);
    }

    [Fact(DisplayName = "INV:TEN-06 " + nameof(ApproveProposal_Moderator_DecidesTheProposal))]
    [Trait("Invariant", "TEN-06")]
    public async Task ApproveProposal_Moderator_DecidesTheProposal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var proposalId = await StorePendingProposalAsync();
        using var client = CreateClient(TestUsers.Moderator);

        // A rejection: an approval would apply the proposal's snapshot, which is not the point here.
        using var response = await client.PostAsJsonAsync(ApprovePath, Command(proposalId), cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await StatusAsync(proposalId, cancellationToken)).ShouldBe(ProposalStatus.Rejected);
    }

    private static ApproveProposalCommand Command(int proposalId) =>
        new() { ProposalId = proposalId, IsApproved = false, Note = "Not now" };

    /// <summary>Stores the two fixture users as rows and a pending proposal of the member.</summary>
    private async Task<int> StorePendingProposalAsync()
    {
        await Api.ResetDatabaseAsync();
        // A product proposal; a rejection never reads its snapshot.
        var proposal = new Proposal { ProposerId = TestUsers.Member.Id, ProductSnapshot = "{}", CreatedAt = TestData.DefaultCreatedAt };
        await StoreAsync(UserRow(TestUsers.Member), UserRow(TestUsers.Moderator), proposal);
        return proposal.Id;
    }

    private static User UserRow(TestUser user) => new()
    {
        Id = user.Id,
        UserName = user.Email,
        NormalizedUserName = user.Email.ToUpperInvariant(),
        Email = user.Email,
        NormalizedEmail = user.Email.ToUpperInvariant(),
    };

    private async Task<ProposalStatus> StatusAsync(int proposalId, CancellationToken cancellationToken)
    {
        using var scope = Api.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        return await db.Proposals.AsNoTracking().Where(p => p.Id == proposalId).Select(p => p.Status).SingleAsync(cancellationToken);
    }
}
