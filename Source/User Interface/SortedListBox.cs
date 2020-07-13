using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ZippyBackup.User_Interface
{
    public interface ISortable
    {
        long GetSortOrder();
    }

    public class SortedListBox : ListBox
    {
        public SortedListBox() : base()
        {
            //Sorted = true;
        }

#       if false
        public void SortNow() { Sort(); }

        protected override void Sort()
        {
            for (int ii = 0; ii < Items.Count; ii++)
            {
                ISortable SortableItemI = Items[ii] as ISortable;
                if (SortableItemI == null) continue;

                for (int jj = ii + 1; jj < Items.Count; jj++)
                {
                    ISortable SortableItemJ = Items[jj] as ISortable;
                    if (SortableItemJ == null) continue;

                    if (SortableItemI.GetSortOrder() < SortableItemJ.GetSortOrder())
                    {
                        object temp = Items[ii];
                        Items[ii] = Items[jj];
                        Items[jj] = temp;

                        SortableItemI = SortableItemJ;
                    }
                }
            }
        }
#       endif

        public void AddSorted(ISortable NewItem)
        {
            for (int ii = 0; ii < Items.Count; ii++)
            {
                ISortable ItemI = Items[ii] as ISortable;
                if (ItemI == null) continue;

                if (ItemI.GetSortOrder() < NewItem.GetSortOrder())
                {
                    Items.Insert(ii, NewItem);
                    return;
                }
            }

            Items.Add(NewItem);
        }
    }
}
