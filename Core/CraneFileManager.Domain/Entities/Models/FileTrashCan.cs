using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.Models;

[Table("FileTrashCan")]
public partial class FileTrashCan : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime? ThrowTrashDate { get; set; }

    public DateTime? TakeofTrashDate { get; set; }

    public Guid? FileId { get; set; }

    [ForeignKey("FileId")]
    [InverseProperty("FileTrashCans")]
    public virtual File File { get; set; }
}
