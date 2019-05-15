using System;
using System.ComponentModel.DataAnnotations;
using DataArt.Atlas.EntityFramework.MsSql;

namespace SampleService.DataAccess
{
    public sealed class SampleEntity : IEntity<int>
    {
        public int Id { get; set; }

        [MaxLength(10)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
