using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using CraneFileManager.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.IdentityAuth;

[Table("User")]
public partial class User : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool? IsBlcok { get; set; }

    public bool? IsActive { get; set; }

    public string ConfirmPassword { get; set; }

    public string Email { get; set; }

    public string ProfileImage { get; set; }

    public DateTime? Birthday { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? SecretKey { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<CraneFileManager.Domain.Entities.Models.FileShare> FileShares { get; set; } = new List<CraneFileManager.Domain.Entities.Models.FileShare>();

    [InverseProperty("User")]
    public virtual ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

    [InverseProperty("User")]
    public virtual ICollection<UserFile> UserFiles { get; set; } = new List<UserFile>();

    [InverseProperty("User")]
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
