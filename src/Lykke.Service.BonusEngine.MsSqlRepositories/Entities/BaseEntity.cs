using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Service.BonusEngine.MsSqlRepositories.Entities
{
    public class BaseEntity
    {
        // This is the base class for all entities.
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
    }
}
