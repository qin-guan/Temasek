using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace Temasek.Auth.Models;

public class ClerkAccount
{
  /// <summary>
  /// Clerk User ID
  /// </summary>
  [SugarColumn(IsPrimaryKey = true)]
  public string Id { get; set; }

  public Guid UserId { get; set; }

  [Navigate(NavigateType.OneToOne, nameof(UserId))]
  public User User { get; set; }
}
