﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

namespace Jumia_Clone.Models.Entities;

public class ProductAttributeValue
{
    public int ValueId { get; set; }

    public int ProductId { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; }

    public virtual ProductAttribute Attribute { get; set; }

    public virtual Product Product { get; set; }
}