using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifySongTagger.Utils
{
    public interface ISelectable
    {
        bool IsSelected{ get; set; }
    }
}
