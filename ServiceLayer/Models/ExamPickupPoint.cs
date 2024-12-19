using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class ExamPickupPoint
{
    public int OrderPickupPoint { get; set; }

    public string Address { get; set; } = null!;

    public virtual ICollection<ExamOrder> ExamOrders { get; set; } = new List<ExamOrder>();
}
