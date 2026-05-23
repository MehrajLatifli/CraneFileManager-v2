using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper.Configuration.Annotations;
using CraneFileManager.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.Models;

[Table("File")]
public partial class File : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string OrginalName { get; set; }
    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string Size { get; set; }

    public string Path { get; set; }

    [Column("isRemove")]
    public bool? IsRemove { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    //[NoMap]
    //[Ignore]
    public Guid? FileTypeId { get; set; }

    [InverseProperty("File")]
    public virtual ICollection<FileShare> FileShares { get; set; } = new List<FileShare>();

    [InverseProperty("File")]
    public virtual ICollection<FileTrashCan> FileTrashCans { get; set; } = new List<FileTrashCan>();

    [ForeignKey("FileTypeId")]
    [InverseProperty("Files")]
    public virtual FileType FileType { get; set; }

    [InverseProperty("File")]
    public virtual ICollection<UserFile> UserFiles { get; set; } = new List<UserFile>();
}
