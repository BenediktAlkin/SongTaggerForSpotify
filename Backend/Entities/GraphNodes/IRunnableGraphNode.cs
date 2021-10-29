using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public interface IRunnableGraphNode
    {
        Task<bool> Run();
    }
}
