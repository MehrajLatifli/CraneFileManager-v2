using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.IdentityAuth;

[Table("UserClaim")]
public partial class UserClaim : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public Guid? UserId { get; set; }

    public Guid? UserPermitionId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserClaims")]
    public virtual User User { get; set; }

    [ForeignKey("UserPermitionId")]
    [InverseProperty("UserClaims")]
    public virtual UserPermission UserPermition { get; set; }
}
