using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using CraneFileManager.Domain.Entities.IdentityAuth;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.Models;

[Table("FileShare")]
public partial class FileShare : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public Guid? FileId { get; set; }

    public Guid? UserId { get; set; }

    [ForeignKey("FileId")]
    [InverseProperty("FileShares")]
    public virtual File File { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("FileShares")]
    public virtual User User { get; set; }
}
