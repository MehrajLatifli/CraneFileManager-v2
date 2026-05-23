using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraneFileManager.Domain.Entities.Base;
using CraneFileManager.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace CraneFileManager.Domain.Entities.IdentityAuth;

[Table("UserNotification")]
public partial class UserNotification : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public Guid? UserId { get; set; }

    public Guid? NotificationId { get; set; }

    [ForeignKey("NotificationId")]
    [InverseProperty("UserNotifications")]
    public virtual Notification Notification { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserNotifications")]
    public virtual User User { get; set; }
}
