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
            this.NetworkInterfaceSwitchingControlTab = new System.Windows.Forms.TabPage();
            this.WifiDoNotAutoDiscardCheckBox = new System.Windows.Forms.CheckBox();
            this.EthernetDoNotAutoDiscardCheckBox = new System.Windows.Forms.CheckBox();
            this.NetworkInterfaceSwitchingEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.ReDetectNetworkInterfacesButton = new System.Windows.Forms.Button();
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox = new System.Windows.Forms.CheckBox();
            this.wifiConnectionLabel = new System.Windows.Forms.Label();
            this.ethernetConnectionLabel = new System.Windows.Forms.Label();
            this.WifiComboBox = new System.Windows.Forms.ComboBox();
            this.EthernetComboBox = new System.Windows.Forms.ComboBox();
            this.StartWithWindowsCheckbox = new System.Windows.Forms.CheckBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.NetworkInterfaceSwitchingControlTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.NetworkInterfaceSwitchingControlTab);
            this.tabControl1.Location = new System.Drawing.Point(20, 73);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1450, 367);
            this.tabControl1.TabIndex = 0;
            // 
            // NetworkInterfaceSwitchingControlTab
            // 
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.ShowCurrentConnectionTypeInSystemTrayCheckBox);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.WifiDoNotAutoDiscardCheckBox);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.EthernetDoNotAutoDiscardCheckBox);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.NetworkInterfaceSwitchingEnabledCheckBox);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.ReDetectNetworkInterfacesButton);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.wifiConnectionLabel);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.ethernetConnectionLabel);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.WifiComboBox);
            this.NetworkInterfaceSwitchingControlTab.Controls.Add(this.EthernetComboBox);
            this.NetworkInterfaceSwitchingControlTab.Location = new System.Drawing.Point(8, 39);
            this.NetworkInterfaceSwitchingControlTab.Margin = new System.Windows.Forms.Padding(2);
            this.NetworkInterfaceSwitchingControlTab.Name = "NetworkInterfaceSwitchingControlTab";
            this.NetworkInterfaceSwitchingControlTab.Padding = new System.Windows.Forms.Padding(2);
            this.NetworkInterfaceSwitchingControlTab.Size = new System.Drawing.Size(1434, 320);
            this.NetworkInterfaceSwitchingControlTab.TabIndex = 0;
            this.NetworkInterfaceSwitchingControlTab.Text = "Wifi/Ethernet Switcher";
            this.NetworkInterfaceSwitchingControlTab.UseVisualStyleBackColor = true;
            // 
            // WifiDoNotAutoDiscardCheckBox
            // 
            this.WifiDoNotAutoDiscardCheckBox.AutoSize = true;
            this.WifiDoNotAutoDiscardCheckBox.Location = new System.Drawing.Point(266, 270);
            this.WifiDoNotAutoDiscardCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.WifiDoNotAutoDiscardCheckBox.Name = "WifiDoNotAutoDiscardCheckBox";
            this.WifiDoNotAutoDiscardCheckBox.Size = new System.Drawing.Size(360, 29);
            this.WifiDoNotAutoDiscardCheckBox.TabIndex = 12;
            this.WifiDoNotAutoDiscardCheckBox.Text = "Don\'t auto-discard if not detected";
            this.WifiDoNotAutoDiscardCheckBox.UseVisualStyleBackColor = true;
            // 
            // EthernetDoNotAutoDiscardCheckBox
            // 
            this.EthernetDoNotAutoDiscardCheckBox.AutoSize = true;
            this.EthernetDoNotAutoDiscardCheckBox.Location = new System.Drawing.Point(266, 190);
            this.EthernetDoNotAutoDiscardCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.EthernetDoNotAutoDiscardCheckBox.Name = "EthernetDoNotAutoDiscardCheckBox";
            this.EthernetDoNotAutoDiscardCheckBox.Size = new System.Drawing.Size(360, 29);
            this.EthernetDoNotAutoDiscardCheckBox.TabIndex = 11;
            this.EthernetDoNotAutoDiscardCheckBox.Text = "Don\'t auto-discard if not detected";
            this.EthernetDoNotAutoDiscardCheckBox.UseVisualStyleBackColor = true;
            // 
            // NetworkInterfaceSwitchingEnabledCheckBox
            // 
            this.NetworkInterfaceSwitchingEnabledCheckBox.AutoSize = true;
            this.NetworkInterfaceSwitchingEnabledCheckBox.Location = new System.Drawing.Point(25, 14);
            this.NetworkInterfaceSwitchingEnabledCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.NetworkInterfaceSwitchingEnabledCheckBox.Name = "NetworkInterfaceSwitchingEnabledCheckBox";
            this.NetworkInterfaceSwitchingEnabledCheckBox.Size = new System.Drawing.Size(123, 29);
            this.NetworkInterfaceSwitchingEnabledCheckBox.TabIndex = 10;
            this.NetworkInterfaceSwitchingEnabledCheckBox.Text = "Enabled";
            this.NetworkInterfaceSwitchingEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // ReDetectNetworkInterfacesButton
            // 
            this.ReDetectNetworkInterfacesButton.Location = new System.Drawing.Point(1196, 272);
            this.ReDetectNetworkInterfacesButton.Margin = new System.Windows.Forms.Padding(6);
            this.ReDetectNetworkInterfacesButton.Name = "ReDetectNetworkInterfacesButton";
            this.ReDetectNetworkInterfacesButton.Size = new System.Drawing.Size(188, 48);
            this.ReDetectNetworkInterfacesButton.TabIndex = 9;
            this.ReDetectNetworkInterfacesButton.Text = "Re-detect";
            this.ReDetectNetworkInterfacesButton.UseVisualStyleBackColor = true;
            this.ReDetectNetworkInterfacesButton.Click += new System.EventHandler(this.ReDetectNetworkInterfacesButton_Click);
            // 
            // AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox
            // 
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.AutoSize = true;
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Location = new System.Drawing.Point(24, 57);
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Name = "AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox";
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Size = new System.Drawing.Size(261, 29);
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.TabIndex = 6;
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.Text = "Auto enable on startup";
            this.AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox.UseVisualStyleBackColor = true;
            // 
            // wifiConnectionLabel
            // 
            this.wifiConnectionLabel.AutoSize = true;
            this.wifiConnectionLabel.Location = new System.Drawing.Point(20, 233);
            this.wifiConnectionLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.wifiConnectionLabel.Name = "wifiConnectionLabel";
            this.wifiConnectionLabel.Size = new System.Drawing.Size(169, 25);
            this.wifiConnectionLabel.TabIndex = 3;
            this.wifiConnectionLabel.Text = "Wifi Connection:";
            // 
            // ethernetConnectionLabel
            // 
            this.ethernetConnectionLabel.AutoSize = true;
            this.ethernetConnectionLabel.Location = new System.Drawing.Point(20, 153);
            this.ethernetConnectionLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ethernetConnectionLabel.Name = "ethernetConnectionLabel";
            this.ethernetConnectionLabel.Size = new System.Drawing.Size(214, 25);
            this.ethernetConnectionLabel.TabIndex = 2;
            this.ethernetConnectionLabel.Text = "Ethernet Connection:";
            // 
            // WifiComboBox
            // 
            this.WifiComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WifiComboBox.FormattingEnabled = true;
            this.WifiComboBox.Location = new System.Drawing.Point(266, 227);
            this.WifiComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.WifiComboBox.Name = "WifiComboBox";
            this.WifiComboBox.Size = new System.Drawing.Size(1118, 33);
            this.WifiComboBox.TabIndex = 1;
            this.WifiComboBox.SelectedIndexChanged += new System.EventHandler(this.WifiComboBox_SelectedIndexChanged);
            // 
            // EthernetComboBox
            // 
            this.EthernetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.EthernetComboBox.FormattingEnabled = true;
            this.EthernetComboBox.Location = new System.Drawing.Point(266, 147);
            this.EthernetComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.EthernetComboBox.Name = "EthernetComboBox";
            this.EthernetComboBox.Size = new System.Drawing.Size(1118, 33);
            this.EthernetComboBox.TabIndex = 0;
            this.EthernetComboBox.SelectedIndexChanged += new System.EventHandler(this.EthernetComboBox_SelectedIndexChanged);
            // 
            // StartWithWindowsCheckbox
            // 
            this.StartWithWindowsCheckbox.AutoSize = true;
            this.StartWithWindowsCheckbox.Location = new System.Drawing.Point(636, 42);
            this.StartWithWindowsCheckbox.Margin = new System.Windows.Forms.Padding(6);
            this.StartWithWindowsCheckbox.Name = "StartWithWindowsCheckbox";
            this.StartWithWindowsCheckbox.Size = new System.Drawing.Size(226, 29);
            this.StartWithWindowsCheckbox.TabIndex = 7;
            this.StartWithWindowsCheckbox.Text = "Start with Windows";
            this.StartWithWindowsCheckbox.UseVisualStyleBackColor = true;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(634, 448);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(6);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(108, 52);
            this.SaveButton.TabIndex = 8;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(754, 448);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(6);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(108, 52);
            this.CancelButton.TabIndex = 9;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ShowCurrentConnectionTypeInSystemTrayCheckBox
            // 
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.AutoSize = true;
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.Location = new System.Drawing.Point(24, 102);
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.Name = "ShowCurrentConnectionTypeInSystemTrayCheckBox";
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.Size = new System.Drawing.Size(467, 29);
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.TabIndex = 13;
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.Text = "Show current connection type in system tray";
            this.ShowCurrentConnectionTypeInSystemTrayCheckBox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelButton;
            this.ClientSize = new System.Drawing.Size(1491, 517);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.StartWithWindowsCheckbox);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.NetworkInterfaceSwitchingControlTab.ResumeLayout(false);
            this.NetworkInterfaceSwitchingControlTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage NetworkInterfaceSwitchingControlTab;
        private System.Windows.Forms.Label ethernetConnectionLabel;
        private System.Windows.Forms.ComboBox WifiComboBox;
        private System.Windows.Forms.ComboBox EthernetComboBox;
        private System.Windows.Forms.Label wifiConnectionLabel;
        private System.Windows.Forms.CheckBox AutoEnableNetworkInterfaceSwitchingOnStartupCheckBox;
        private System.Windows.Forms.CheckBox StartWithWindowsCheckbox;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button ReDetectNetworkInterfacesButton;
        private System.Windows.Forms.CheckBox NetworkInterfaceSwitchingEnabledCheckBox;
        private System.Windows.Forms.CheckBox WifiDoNotAutoDiscardCheckBox;
        private System.Windows.Forms.CheckBox EthernetDoNotAutoDiscardCheckBox;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.CheckBox ShowCurrentConnectionTypeInSystemTrayCheckBox;
    }
}