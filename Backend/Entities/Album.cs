using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Backend.Entities
{
    public class Album
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string ReleaseDate { get; set; }
        public string ReleaseDatePrecision { get; set; }
    }
}
