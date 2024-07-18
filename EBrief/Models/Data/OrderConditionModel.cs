﻿using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Models.Data;

public class OrderConditionModel
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public int Number { get; set; }
    public string Condition { get; set; } = string.Empty;

    public OrderCondition ToUIModel()
    {
        return new OrderCondition(Number, Condition);
    }
}