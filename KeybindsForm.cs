﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TitanixClient___Forms
{
    public partial class KeybindsForm : Form
    {
        public KeybindsForm()
        {
            InitializeComponent();
            UpdateBox();
        }
        private void UpdateBox()
        {
            picker.Items.Clear();
            foreach (var kb in Data.keybinds.keybinds)
                picker.Items.Add(kb);
        }

        private void confirm_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void edit_Click(object sender, EventArgs e)
        {
            Keybind toEdit = (Keybind)picker.SelectedItem;
            using (SingleKeybindEditor edit = new SingleKeybindEditor(toEdit))
            {
                edit.ShowDialog();
                ApplyKeybindAfterEditing(edit, true, toEdit);
            }
        }
        private void add_Click(object sender, EventArgs e)
        {
            using (SingleKeybindEditor edit = new SingleKeybindEditor())
            {
                edit.ShowDialog();
                ApplyKeybindAfterEditing(edit, false, null);
            }
        }

        private void picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            edit.Enabled = true;
        }

        private void ApplyKeybindAfterEditing(SingleKeybindEditor editor, bool edited, Keybind toReplace)
        {
            if (!editor.confirmed) return;
            Keybind kb = editor.keybind;
            if(edited)
                Data.keybinds.ReplaceKeybind(toReplace, kb);
            else
                Data.keybinds.AddKeybind(kb);
            UpdateBox();
        }
    }
}
