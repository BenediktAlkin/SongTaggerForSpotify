using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class Utils
    {
        public static void SyncLists<T>(IList<T> list, IList<T> newList) where T : class
        {
            // can't just assign new list because that would remove the reference to the treeviews
            // also clearing the lists gives the comboboxes no elements which in turn clears
            // the selected playlist of a GraphNode
            var i = 0;
            var j = 0;
            while (i < newList.Count)
            {
                if (i < list.Count && !newList.Contains(list[i]))
                {
                    // list[i] was removed
                    list.RemoveAt(i);
                    continue;
                }
                if (list.Contains(newList[j]) && list[i] == newList[j])
                {
                    // nothing changed for list[i]
                    i++;
                    j++;
                }
                else
                {
                    // newList[j] is a new element
                    list.Insert(i, newList[j]);
                    i++;
                    j++;
                }
            }
        }
    }
}
