using System;
using SqlSugar;

namespace Temasek.Auth.Models;

public class User
{
  [SugarColumn(IsPrimaryKey = true)]
  public Guid Id { get; set; }
}
