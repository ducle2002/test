﻿using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YOOTEK.EntityDb
{
    [Table("payment", Schema = "payments")]
    public class Payment : Entity<int>
    {
        [Key][Column("id")] public int Id { get; set; }

        [Column("createdAt")] public DateTime CreatedAt { get; set; }

        [Column("createdById")] public int CreatedById { get; set; }

        [Column("updatedAt")] public DateTime UpdatedAt { get; set; }

        [Column("updatedById")] public int UpdatedById { get; set; }

        [Column("deletedAt")] public DateTime? DeletedAt { get; set; }

        [Column("deletedById")] public int? DeletedById { get; set; }

        [Column("tenantId")] public int TenantId { get; set; }

        [Column("code")] public string Code { get; set; }

        [Column("status")] public int Status { get; set; }

        [Column("method")] public int Method { get; set; }

        [Column("type")] public int Type { get; set; }

        [Column("transactionId")] public string TransactionId { get; set; }

        [Column("transactionProperties")] public string TransactionProperties { get; set; }

        [Column("amount")] public double Amount { get; set; }

        [Column("currency")] public string Currency { get; set; }

        [Column("description")] public string Description { get; set; }

        [Column("properties")] public string Properties { get; set; }
    }

    [Table("payment", Schema = "payments")]
    public class ThirdPartyPayment: Entity<int>
    {
        [Key][Column("id")] public int Id { get; set; }

        [Column("createdAt")] public DateTime CreatedAt { get; set; }

        [Column("createdById")] public int? CreatedById { get; set; }

        [Column("updatedAt")] public DateTime UpdatedAt { get; set; }

        [Column("updatedById")] public int? UpdatedById { get; set; }

        [Column("deletedAt")] public DateTime? DeletedAt { get; set; }

        [Column("deletedById")] public int? DeletedById { get; set; }

        [Column("tenantId")] public int? TenantId { get; set; }

        [Column("status")] public EPaymentStatus Status { get; set; }

        [Column("method")] public EPaymentMethod Method { get; set; }

        [Column("type")] public EPaymentType Type { get; set; }

        [Column("transactionId")] public string TransactionId { get; set; }

        [Column("transactionProperties")] public string TransactionProperties { get; set; }

        [Column("amount")] public double Amount { get; set; }

        [Column("currency")] public string Currency { get; set; }

        [Column("description")] public string Description { get; set; }

        [Column("properties")] public string Properties { get; set; }
    }

    public enum EPaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
    }

    public enum EPaymentMethod
    {
        Other = 0,
        Cod = 1,
        Momo = 2,
        Onepay = 3,
    }

    public enum EPaymentType
    {
        Shopping = 1,
        Booking = 2,
        Invoice = 3,
        Ecofarm = 4,
    }
}