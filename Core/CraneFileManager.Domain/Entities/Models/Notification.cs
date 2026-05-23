using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using CraneFileManager.Domain.Entities.IdentityAuth;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.Models;

[Table("Notification")]
public partial class Notification : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime? NotificationDate { get; set; }

    [InverseProperty("Notification")]
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
