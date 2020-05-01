namespace NetworkAssistantNamespace
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.wifiEthernetSwitchTab = new System.Windows.Forms.TabPage();
            this.wifiDoNotAutoDiscardCheckBox = new System.Windows.Forms.CheckBox();
            this.ethernetDoNotAutoDiscardCheckBox = new System.Windows.Forms.CheckBox();
            this.nicSwitchEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.redetectNICsButton = new System.Windows.Forms.Button();
            this.autoEnableSwitcherOnStartupCheckBox = new System.Windows.Forms.CheckBox();
            this.wifiConnectionLabel = new System.Windows.Forms.Label();
            this.ethernetConnectionLabel = new System.Windows.Forms.Label();
            this.wifiComboBox = new System.Windows.Forms.ComboBox();
            this.ethernetComboBox = new System.Windows.Forms.ComboBox();
            this.startWithWindowsCheckbox = new System.Windows.Forms.CheckBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.wifiEthernetSwitchTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.wifiEthernetSwitchTab);
            this.tabControl1.Location = new System.Drawing.Point(27, 91);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1933, 410);
            this.tabControl1.TabIndex = 0;
            // 
            // wifiEthernetSwitchTab
            // 
            this.wifiEthernetSwitchTab.Controls.Add(this.wifiDoNotAutoDiscardCheckBox);
            this.wifiEthernetSwitchTab.Controls.Add(this.ethernetDoNotAutoDiscardCheckBox);
            this.wifiEthernetSwitchTab.Controls.Add(this.nicSwitchEnabledCheckBox);
            this.wifiEthernetSwitchTab.Controls.Add(this.redetectNICsButton);
            this.wifiEthernetSwitchTab.Controls.Add(this.autoEnableSwitcherOnStartupCheckBox);
            this.wifiEthernetSwitchTab.Controls.Add(this.wifiConnectionLabel);
            this.wifiEthernetSwitchTab.Controls.Add(this.ethernetConnectionLabel);
            this.wifiEthernetSwitchTab.Controls.Add(this.wifiComboBox);
            this.wifiEthernetSwitchTab.Controls.Add(this.ethernetComboBox);
            this.wifiEthernetSwitchTab.Location = new System.Drawing.Point(4, 40);
            this.wifiEthernetSwitchTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.wifiEthernetSwitchTab.Name = "wifiEthernetSwitchTab";
            this.wifiEthernetSwitchTab.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.wifiEthernetSwitchTab.Size = new System.Drawing.Size(1925, 366);
            this.wifiEthernetSwitchTab.TabIndex = 0;
            this.wifiEthernetSwitchTab.Text = "Wifi/Ethernet Switcher";
            this.wifiEthernetSwitchTab.UseVisualStyleBackColor = true;
            // 
            // wifiDoNotAutoDiscardCheckBox
            // 
            this.wifiDoNotAutoDiscardCheckBox.AutoSize = true;
            this.wifiDoNotAutoDiscardCheckBox.Location = new System.Drawing.Point(355, 274);
            this.wifiDoNotAutoDiscardCheckBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.wifiDoNotAutoDiscardCheckBox.Name = "wifiDoNotAutoDiscardCheckBox";
            this.wifiDoNotAutoDiscardCheckBox.Size = new System.Drawing.Size(431, 35);
            this.wifiDoNotAutoDiscardCheckBox.TabIndex = 12;
            this.wifiDoNotAutoDiscardCheckBox.Text = "Don\'t auto-discard if not detected";
            this.wifiDoNotAutoDiscardCheckBox.UseVisualStyleBackColor = true;
            // 
            // ethernetDoNotAutoDiscardCheckBox
            // 
            this.ethernetDoNotAutoDiscardCheckBox.AutoSize = true;
            this.ethernetDoNotAutoDiscardCheckBox.Location = new System.Drawing.Point(355, 175);
            this.ethernetDoNotAutoDiscardCheckBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.ethernetDoNotAutoDiscardCheckBox.Name = "ethernetDoNotAutoDiscardCheckBox";
            this.ethernetDoNotAutoDiscardCheckBox.Size = new System.Drawing.Size(431, 35);
            this.ethernetDoNotAutoDiscardCheckBox.TabIndex = 11;
            this.ethernetDoNotAutoDiscardCheckBox.Text = "Don\'t auto-discard if not detected";
            this.ethernetDoNotAutoDiscardCheckBox.UseVisualStyleBackColor = true;
            this.ethernetDoNotAutoDiscardCheckBox.CheckedChanged += new System.EventHandler(this.ethernetDoNotAutoDiscardCheckBox_CheckedChanged);
            // 
            // nicSwitchEnabledCheckBox
            // 
            this.nicSwitchEnabledCheckBox.AutoSize = true;
            this.nicSwitchEnabledCheckBox.Location = new System.Drawing.Point(33, 17);
            this.nicSwitchEnabledCheckBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.nicSwitchEnabledCheckBox.Name = "nicSwitchEnabledCheckBox";
            this.nicSwitchEnabledCheckBox.Size = new System.Drawing.Size(132, 35);
            this.nicSwitchEnabledCheckBox.TabIndex = 10;
            this.nicSwitchEnabledCheckBox.Text = "Enabled";
            this.nicSwitchEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // redetectNICsButton
            // 
            this.redetectNICsButton.Location = new System.Drawing.Point(1600, 250);
            this.redetectNICsButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.redetectNICsButton.Name = "redetectNICsButton";
            this.redetectNICsButton.Size = new System.Drawing.Size(251, 55);
            this.redetectNICsButton.TabIndex = 9;
            this.redetectNICsButton.Text = "Re-detect";
            this.redetectNICsButton.UseVisualStyleBackColor = true;
            this.redetectNICsButton.Click += new System.EventHandler(this.redetectNICsButton_Click);
            // 
            // autoEnableSwitcherOnStartupCheckBox
            // 
            this.autoEnableSwitcherOnStartupCheckBox.AutoSize = true;
            this.autoEnableSwitcherOnStartupCheckBox.Location = new System.Drawing.Point(32, 71);
            this.autoEnableSwitcherOnStartupCheckBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.autoEnableSwitcherOnStartupCheckBox.Name = "autoEnableSwitcherOnStartupCheckBox";
            this.autoEnableSwitcherOnStartupCheckBox.Size = new System.Drawing.Size(305, 35);
            this.autoEnableSwitcherOnStartupCheckBox.TabIndex = 6;
            this.autoEnableSwitcherOnStartupCheckBox.Text = "Auto enable on startup";
            this.autoEnableSwitcherOnStartupCheckBox.UseVisualStyleBackColor = true;
            // 
            // wifiConnectionLabel
            // 
            this.wifiConnectionLabel.AutoSize = true;
            this.wifiConnectionLabel.Location = new System.Drawing.Point(27, 228);
            this.wifiConnectionLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.wifiConnectionLabel.Name = "wifiConnectionLabel";
            this.wifiConnectionLabel.Size = new System.Drawing.Size(212, 31);
            this.wifiConnectionLabel.TabIndex = 3;
            this.wifiConnectionLabel.Text = "Wifi Connection:";
            // 
            // ethernetConnectionLabel
            // 
            this.ethernetConnectionLabel.AutoSize = true;
            this.ethernetConnectionLabel.Location = new System.Drawing.Point(27, 129);
            this.ethernetConnectionLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.ethernetConnectionLabel.Name = "ethernetConnectionLabel";
            this.ethernetConnectionLabel.Size = new System.Drawing.Size(270, 31);
            this.ethernetConnectionLabel.TabIndex = 2;
            this.ethernetConnectionLabel.Text = "Ethernet Connection:";
            // 
            // wifiComboBox
            // 
            this.wifiComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wifiComboBox.FormattingEnabled = true;
            this.wifiComboBox.Location = new System.Drawing.Point(355, 221);
            this.wifiComboBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.wifiComboBox.Name = "wifiComboBox";
            this.wifiComboBox.Size = new System.Drawing.Size(1489, 39);
            this.wifiComboBox.TabIndex = 1;
            this.wifiComboBox.SelectedIndexChanged += new System.EventHandler(this.wifiComboBox_SelectedIndexChanged);
            // 
            // ethernetComboBox
            // 
            this.ethernetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ethernetComboBox.FormattingEnabled = true;
            this.ethernetComboBox.Location = new System.Drawing.Point(355, 122);
            this.ethernetComboBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.ethernetComboBox.Name = "ethernetComboBox";
            this.ethernetComboBox.Size = new System.Drawing.Size(1489, 39);
            this.ethernetComboBox.TabIndex = 0;
            this.ethernetComboBox.SelectedIndexChanged += new System.EventHandler(this.ethernetComboBox_SelectedIndexChanged);
            // 
            // startWithWindowsCheckbox
            // 
            this.startWithWindowsCheckbox.AutoSize = true;
            this.startWithWindowsCheckbox.Location = new System.Drawing.Point(848, 52);
            this.startWithWindowsCheckbox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.startWithWindowsCheckbox.Name = "startWithWindowsCheckbox";
            this.startWithWindowsCheckbox.Size = new System.Drawing.Size(264, 35);
            this.startWithWindowsCheckbox.TabIndex = 7;
            this.startWithWindowsCheckbox.Text = "Start with Windows";
            this.startWithWindowsCheckbox.UseVisualStyleBackColor = true;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(883, 513);
            this.saveButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(251, 52);
            this.saveButton.TabIndex = 8;
            this.saveButton.Text = "Save SettingsForm";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1624, 579);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.startWithWindowsCheckbox);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SettingsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.wifiEthernetSwitchTab.ResumeLayout(false);
            this.wifiEthernetSwitchTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage wifiEthernetSwitchTab;
        private System.Windows.Forms.Label ethernetConnectionLabel;
        private System.Windows.Forms.ComboBox wifiComboBox;
        private System.Windows.Forms.ComboBox ethernetComboBox;
        private System.Windows.Forms.Label wifiConnectionLabel;
        private System.Windows.Forms.CheckBox autoEnableSwitcherOnStartupCheckBox;
        private System.Windows.Forms.CheckBox startWithWindowsCheckbox;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button redetectNICsButton;
        private System.Windows.Forms.CheckBox nicSwitchEnabledCheckBox;
        private System.Windows.Forms.CheckBox wifiDoNotAutoDiscardCheckBox;
        private System.Windows.Forms.CheckBox ethernetDoNotAutoDiscardCheckBox;
    }
}