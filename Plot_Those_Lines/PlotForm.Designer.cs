namespace Plot_Those_Lines
{
    partial class PlotForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
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
    /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
    /// le contenu de cette méthode avec l'éditeur de code.
    /// </summary>
    private void InitializeComponent()
        {
            this.pltMain = new ScottPlot.WinForms.FormsPlot();
            this.btnImport = new System.Windows.Forms.Button();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.pltTeams = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pltMain
            // 
            this.pltMain.DisplayScale = 0F;
            this.pltMain.Location = new System.Drawing.Point(61, 50);
            this.pltMain.Name = "pltMain";
            this.pltMain.Size = new System.Drawing.Size(1268, 612);
            this.pltMain.TabIndex = 0;
            this.pltMain.Load += new System.EventHandler(this.pltMain_Load);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(12, 12);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 1;
            this.btnImport.Text = "Importer";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(93, 14);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(120, 20);
            this.txtTitle.TabIndex = 3;
            this.txtTitle.TextChanged += new System.EventHandler(this.txtTitle_TextChanged);
            // 
            // pltTeams
            // 
            this.pltTeams.AutoSize = true;
            this.pltTeams.Location = new System.Drawing.Point(-1, 71);
            this.pltTeams.Name = "pltTeams";
            this.pltTeams.Size = new System.Drawing.Size(35, 13);
            this.pltTeams.TabIndex = 4;
            this.pltTeams.Text = "label1";
            this.pltTeams.Click += new System.EventHandler(this.pltTeams_Click);
            // 
            // PlotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1341, 764);
            this.Controls.Add(this.pltTeams);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.pltMain);
            this.Name = "PlotForm";
            this.Text = "PlotForm";
            this.Load += new System.EventHandler(this.PlotForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScottPlot.WinForms.FormsPlot pltMain;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label pltTeams;
    }
}
