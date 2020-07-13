/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace ZippyBackup.User_Interface
{
    /// <summary>
    /// Provides the interface which must be implemented by any object
    /// added to the ListBoxWithIcons Items collection in order for that
    /// item to display an icon.  The state selects the icon from the
    /// list provided to ListBoxWithIcons.Icons.
    /// </summary>
    public interface IDisplayStates
    {
        int GetState();
    }

    /// <summary>
    /// Provides a simple implementation of a list box item class
    /// compatible with ListBoxWithIcons.  The IDisplayStates interface
    /// can also be implemented directly by a class representing a 
    /// list box item and be supported by ListBoxWithIcons.
    /// </summary>
    public class ListBoxItemWithState : IDisplayStates
    {
        public int State = 0;
        public string Text;

        public ListBoxItemWithState(string Text, int State)
        {
            this.Text = Text;
            this.State = State;
        }

        public override string ToString()
        {
            return Text;
        }

        public int GetState()
        {
            return State;
        }
    }

    /// <summary>
    /// Accepts a list of bitmaps which can be indexed by items added to the listbox.  The items
    /// must implement the IDisplayStates interface in order to display an icon.  All bitmaps
    /// are drawn at the size given by IconWidth and IconHeight.
    /// </summary>
    public class ListBoxWithIcons : ListBox
    {
        public List<Bitmap> Icons = new List<Bitmap>();

        public int IconWidth = 32;
        public int IconHeight = 32;
        public int IconRightPadding = 12;

        public ListBoxWithIcons()
        {
            DrawMode = DrawMode.OwnerDrawVariable;
            DrawItem += new DrawItemEventHandler(OnDrawItem);
            MeasureItem += new MeasureItemEventHandler(OnMeasureItem);
        }

        public ListBoxWithIcons(ListBox Duplicate)
        {
            // Copy all events from Duplicate to here...bit of a hack.
            var eventsField = typeof(System.ComponentModel.Component).GetField("events", BindingFlags.NonPublic | BindingFlags.Instance);
            var eventHandlerList = eventsField.GetValue(Duplicate);
            eventsField.SetValue(this, eventHandlerList);

            DrawMode = DrawMode.OwnerDrawVariable;
            DrawItem += new DrawItemEventHandler(OnDrawItem);
            MeasureItem += new MeasureItemEventHandler(OnMeasureItem);

            Left = Duplicate.Left;
            Top = Duplicate.Top;
            Width = Duplicate.Width;
            Height = Duplicate.Height;
            Font = Duplicate.Font;

            for (int ii = 0; ii < Duplicate.Items.Count; ii++)
                Items.Add(Duplicate.Items[ii]);
        }

        void OnMeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = Math.Max(10, IconHeight);
        }

        void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            int iState = -1;
            IDisplayStates ds = Items[e.Index] as IDisplayStates;
            if (ds != null) iState = ds.GetState();
            string Text = Items[e.Index].ToString();

            Bitmap StateImage = null;
            if (iState >= 0 && iState < Icons.Count) StateImage = Icons[iState];

            e.DrawBackground();
            e.DrawFocusRectangle();

            Rectangle IconRect = new Rectangle(e.Bounds.Left, e.Bounds.Top, IconWidth, IconHeight);
            if (StateImage != null)
            {
                ImageAttributes attr = new ImageAttributes();

                // Set the transparency color key based on the upper-left pixel 
                // of the image.                
                attr.SetColorKey(StateImage.GetPixel(0, 0), StateImage.GetPixel(0, 0));
                //e.Graphics.DrawImage(StateImage, IconRect);
                e.Graphics.DrawImage(StateImage, IconRect, 0, 0, StateImage.Width, StateImage.Height, GraphicsUnit.Pixel, attr);
            }

            SizeF sz = e.Graphics.MeasureString(Text, e.Font);

            Rectangle TextRect = new Rectangle(IconRect.Right + IconRightPadding, (int)(e.Bounds.Top + e.Bounds.Height / 2 - sz.Height / 2),
                e.Bounds.Width - IconRect.Width - IconRightPadding, e.Bounds.Height);
            e.Graphics.DrawString(Text,
                e.Font,
                new SolidBrush(e.ForeColor), TextRect);
        }
    }
}
