using Backend.Errors;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public interface IRunnableGraphNode
    {
        Task<Error> Run();
    }
}
