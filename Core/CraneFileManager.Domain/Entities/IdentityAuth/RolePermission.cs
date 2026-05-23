using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.IdentityAuth;

[Table("RolePermission")]
public partial class RolePermission : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Method { get; set; }

    public string MethodDescription { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [InverseProperty("RolePermission")]
    public virtual ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
}
