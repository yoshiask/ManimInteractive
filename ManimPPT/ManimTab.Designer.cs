namespace ManimPPT
{
    partial class ManimRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public ManimRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl1 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl2 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl3 = this.Factory.CreateRibbonDropDownItem();
            this.ManimTab1 = this.Factory.CreateRibbonTab();
            this.ManimRenderGroup = this.Factory.CreateRibbonGroup();
            this.QualityBox = this.Factory.CreateRibbonComboBox();
            this.RenderButton = this.Factory.CreateRibbonButton();
            this.ManimGenerateGroup = this.Factory.CreateRibbonGroup();
            this.GenerateButton = this.Factory.CreateRibbonButton();
            this.ManimTab1.SuspendLayout();
            this.ManimRenderGroup.SuspendLayout();
            this.ManimGenerateGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // ManimTab1
            // 
            this.ManimTab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.ManimTab1.Groups.Add(this.ManimRenderGroup);
            this.ManimTab1.Groups.Add(this.ManimGenerateGroup);
            this.ManimTab1.Label = "Manim";
            this.ManimTab1.Name = "ManimTab1";
            // 
            // ManimRenderGroup
            // 
            this.ManimRenderGroup.Items.Add(this.QualityBox);
            this.ManimRenderGroup.Items.Add(this.RenderButton);
            this.ManimRenderGroup.Label = "Render";
            this.ManimRenderGroup.Name = "ManimRenderGroup";
            this.ManimRenderGroup.Tag = "";
            // 
            // QualityBox
            // 
            ribbonDropDownItemImpl1.Label = "Low";
            ribbonDropDownItemImpl2.Label = "Medium";
            ribbonDropDownItemImpl3.Label = "Production";
            this.QualityBox.Items.Add(ribbonDropDownItemImpl1);
            this.QualityBox.Items.Add(ribbonDropDownItemImpl2);
            this.QualityBox.Items.Add(ribbonDropDownItemImpl3);
            this.QualityBox.Label = "Quality";
            this.QualityBox.Name = "QualityBox";
            this.QualityBox.Text = null;
            // 
            // RenderButton
            // 
            this.RenderButton.Description = "Render the presentation at the selected quality";
            this.RenderButton.Label = "Render";
            this.RenderButton.Name = "RenderButton";
            this.RenderButton.OfficeImageId = "MoviePlayAutomatically";
            this.RenderButton.ShowImage = true;
            // 
            // ManimGenerateGroup
            // 
            this.ManimGenerateGroup.Items.Add(this.GenerateButton);
            this.ManimGenerateGroup.Label = "Generate";
            this.ManimGenerateGroup.Name = "ManimGenerateGroup";
            // 
            // GenerateButton
            // 
            this.GenerateButton.Description = "Generate the presentation";
            this.GenerateButton.Label = "Generate";
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.OfficeImageId = "MoviePlayAutomatically";
            this.GenerateButton.ShowImage = true;
            this.GenerateButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.GenerateButton_Click);
            // 
            // ManimRibbon
            // 
            this.Name = "ManimRibbon";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.ManimTab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.ManimTab_Load);
            this.ManimTab1.ResumeLayout(false);
            this.ManimTab1.PerformLayout();
            this.ManimRenderGroup.ResumeLayout(false);
            this.ManimRenderGroup.PerformLayout();
            this.ManimGenerateGroup.ResumeLayout(false);
            this.ManimGenerateGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab ManimTab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup ManimRenderGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonComboBox QualityBox;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton RenderButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup ManimGenerateGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton GenerateButton;
    }

    partial class ThisRibbonCollection
    {
        internal ManimRibbon ManimTab {
            get { return this.GetRibbon<ManimRibbon>(); }
        }
    }
}
