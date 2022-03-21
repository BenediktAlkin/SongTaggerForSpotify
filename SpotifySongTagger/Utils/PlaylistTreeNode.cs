using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifySongTagger.Utils
{
    public record PlaylistTreeNode(string Name, bool IsExpanded, IEnumerable<object> Children);
}
