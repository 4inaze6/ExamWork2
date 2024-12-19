using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class ExamUser
{
    public int UserId { get; set; }

    public byte RoleId { get; set; }

    public string UserSurname { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string UserPatronymic { get; set; } = null!;

    public string UserLogin { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public virtual ICollection<ExamOrder> ExamOrders { get; set; } = new List<ExamOrder>();

    public virtual ExamRole Role { get; set; } = null!;
}
