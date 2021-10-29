using Backend.Entities.GraphNodes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Util;

namespace Backend.Entities
{
    public class GraphGeneratorPage : NotifyPropertyChangedBase
    {
        public int Id { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, nameof(Name));
        }
        public List<GraphNode> GraphNodes { get; set; } = new();


        private bool isRunning;
        [NotMapped]
        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value, nameof(IsRunning));
        }
    }
}
