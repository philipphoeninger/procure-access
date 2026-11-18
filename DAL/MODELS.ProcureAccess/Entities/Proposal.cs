// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities;

[Table("Proposals", Schema = "public")]
public partial class Proposal : BaseEntity
{
    #region fields
    /// <summary>
    /// The user id of the proposer, without a relation: the users live in the IdentityAccess
    /// module's context since S6-10, and no foreign key crosses two contexts (ADR-014 rule 5).
    /// </summary>
    [Required]
    public string ProposerId { get; set; } = null!;

    /// <summary>The user id of the reviewer, without a relation like <see cref="ProposerId"/>.</summary>
    public string? ApproverId { get; set; }

    [ForeignKey("Product")]
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public string? ProductSnapshot { get; set; }

    [ForeignKey("Criterion")]
    public int? CriterionId { get; set; }
    public Criterion? Criterion { get; set; }
    public string? CriterionSnapshot { get; set; }

    [Required]
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

    [StringLength(5000)]
    public string? Note { get; set; }

    public DateTime? FinishedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;
    #endregion

    #region ctors
    public Proposal() { }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Proposal has the ID {Id}";
    }
    #endregion
}
