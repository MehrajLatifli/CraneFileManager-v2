using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.IdentityAuth;

[Table("RoleClaim")]
public partial class RoleClaim : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public Guid? RoleId { get; set; }

    public Guid? RolePermissionId { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("RoleClaims")]
    public virtual Role Role { get; set; }

    [ForeignKey("RolePermissionId")]
    [InverseProperty("RoleClaims")]
    public virtual RolePermission RolePermission { get; set; }
}
