namespace DevCesio.DevForm
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.rtbContext = new System.Windows.Forms.RichTextBox();
            this.lblRuning = new System.Windows.Forms.Label();
            this.timerShow = new System.Windows.Forms.Timer(this.components);
            this.lblDayTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // rtbContext
            // 
            this.rtbContext.BackColor = System.Drawing.SystemColors.Control;
            this.rtbContext.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbContext.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rtbContext.Location = new System.Drawing.Point(138, 12);
            this.rtbContext.Name = "rtbContext";
            this.rtbContext.ReadOnly = true;
            this.rtbContext.Size = new System.Drawing.Size(350, 210);
            this.rtbContext.TabIndex = 3;
            this.rtbContext.Text = "";
            // 
            // lblRuning
            // 
            this.lblRuning.AutoSize = true;
            this.lblRuning.Font = new System.Drawing.Font("宋体", 16F);
            this.lblRuning.Location = new System.Drawing.Point(208, 294);
            this.lblRuning.Name = "lblRuning";
            this.lblRuning.Size = new System.Drawing.Size(0, 22);
            this.lblRuning.TabIndex = 4;
            // 
            // timerShow
            // 
            this.timerShow.Tick += new System.EventHandler(this.trShow_Tick);
            // 
            // lblDayTime
            // 
            this.lblDayTime.AutoSize = true;
            this.lblDayTime.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Bold);
            this.lblDayTime.Location = new System.Drawing.Point(219, 259);
            this.lblDayTime.Name = "lblDayTime";
            this.lblDayTime.Size = new System.Drawing.Size(0, 22);
            this.lblDayTime.TabIndex = 4;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 362);
            this.Controls.Add(this.lblDayTime);
            this.Controls.Add(this.lblRuning);
            this.Controls.Add(this.rtbContext);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(600, 400);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "金蝶云星空 仓库条码管理软件专用";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbContext;
        private System.Windows.Forms.Label lblRuning;
        private System.Windows.Forms.Timer timerShow;
        private System.Windows.Forms.Label lblDayTime;
    }
}