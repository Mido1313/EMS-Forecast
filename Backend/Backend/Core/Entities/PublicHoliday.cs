namespace Core.Entities;

using Base.Core.Entities;
using System;

public class PublicHoliday : EntityObject
{
    public required string  Name { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? PeriodEnd { get; set; }
    public bool? IsSchoolBreak   { get; set; }


}