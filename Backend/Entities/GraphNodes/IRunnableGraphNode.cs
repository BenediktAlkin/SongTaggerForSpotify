using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public interface IRunnableGraphNode
    {
        Task<bool> Run();
    }
}
