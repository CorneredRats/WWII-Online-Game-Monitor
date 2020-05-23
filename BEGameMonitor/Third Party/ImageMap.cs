// ImageMap control by Ryan LaNeve (modified)
// http://www.codeproject.com/KB/miscctrl/imagemapcontrol.aspx

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;  // [xiperware]

namespace ImageMap
{
	/// <summary>
	/// Summary description for ImageMap.
	/// </summary>
	[ToolboxBitmap(typeof(ImageMap))]
	public class ImageMap : System.Windows.Forms.UserControl
	{
		private System.Drawing.Drawing2D.GraphicsPath _pathData;
		private int _activeIndex = -1;
		private ArrayList _pathsArray;
		private ToolTip _toolTip;
		private Graphics _graphics;
    private List<bool> _clickable;  // [xiperware]
    private List<object> _tag;  // [xiperware]

		private System.Windows.Forms.PictureBox pictureBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

    //public delegate void RegionClickDelegate(int index, string key);
		public delegate void RegionClickDelegate( int index, object tag, MouseButtons button );  // [xiperware]
		[Category("Action")]
		public event RegionClickDelegate RegionClick;

    public event EventHandler MyMouseEnter;  // [xiperware]
    public event EventHandler MyMouseLeave;  // [xiperware]

		public ImageMap()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
      this.SetStyle( ControlStyles.Selectable, true );  // [xiperware]

			// TODO: Add any initialization after the InitForm call
			this._pathsArray = new ArrayList();
			this._pathData = new System.Drawing.Drawing2D.GraphicsPath();
			this._pathData.FillMode = System.Drawing.Drawing2D.FillMode.Winding;

			this.components = new Container();
			this._toolTip = new ToolTip(this.components);
			this._toolTip.AutoPopDelay = 5000;
			this._toolTip.InitialDelay = 1000;
			this._toolTip.ReshowDelay = 500;

			this._graphics = Graphics.FromHwnd(this.pictureBox.Handle);

      this._clickable = new List<bool>();  // [xiperware]
      this._tag = new List<object>();  // [xiperware]
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.pictureBox = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(150, 150);
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			//this.pictureBox.Click += new System.EventHandler(this.pictureBox_Click);
      this.pictureBox.MouseClick += new MouseEventHandler( this.pictureBox_Click );  // [xiperware]
			this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
			this.pictureBox.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
      this.pictureBox.MouseEnter += new EventHandler( pictureBox_MouseEnter );  // [xiperware]
			// 
			// ImageMap
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.pictureBox});
			this.Name = "ImageMap";
			this.ResumeLayout(false);

		}

		#endregion

		[Category("Appearance")]
		public Image Image
		{
			get
			{
				return this.pictureBox.Image;
			}
			set
			{
				this.pictureBox.Image = value;
			}
		}

		public int AddElipse(string key, Point center, int radius)
		{
			return this.AddElipse(key, center.X, center.Y, radius);
		}

    //public int AddElipse(string key, int x, int y, int radius)
		public int AddElipse(string key, int x, int y, int radius, bool clickable, object tag)  // [xiperware]
		{
			if(this._pathsArray.Count > 0)
				this._pathData.SetMarkers();
			this._pathData.AddEllipse(x - radius, y - radius, radius * 2, radius * 2);
      this._clickable.Add(clickable);  // [xiperware]
      this._tag.Add(tag);  // [xiperware]
			return this._pathsArray.Add(key);
		}

    public int AddElipse( string key, int x, int y, int radius )  // [xiperware]
    {
      return AddElipse( key, x, y, radius, false, null );
    }

		public int AddRectangle(string key, int x1, int y1, int x2, int y2)
		{
			return this.AddRectangle(key, new Rectangle(x1, y1, (x2 - x1), (y2 - y1)));
		}

    //public int AddRectangle(string key, Rectangle rectangle)
		public int AddRectangle(string key, Rectangle rectangle, bool clickable, object tag)  // [xiperware]
		{
			if(this._pathsArray.Count > 0)
				this._pathData.SetMarkers();
			this._pathData.AddRectangle(rectangle);
      this._clickable.Add(clickable);  // [xiperware]
      this._tag.Add(tag);  // [xiperware]
			return this._pathsArray.Add(key);
		}

    public int AddRectangle( string key, Rectangle rectangle )  // [xiperware]
    {
      return AddRectangle( key, rectangle, false, null );
    }

		public int AddPolygon(string key, Point[] points)
		{
			if(this._pathsArray.Count > 0)
				this._pathData.SetMarkers();
			this._pathData.AddPolygon(points);
			return this._pathsArray.Add(key);
		}

    public void RemoveAll()  // [xiperware]
    {
      this._pathData.Reset();
      this._pathsArray.Clear();
      this._clickable.Clear();
      this._tag.Clear();
    }

		private void pictureBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int newIndex = this.getActiveIndexAtPoint(new Point(e.X, e.Y));
			if(newIndex > -1)
			{
        if(this._clickable[newIndex])  // [xiperware]
          pictureBox.Cursor = Cursors.Hand;
        if( this._activeIndex != newIndex )
        {
          this._toolTip.Hide( this.pictureBox ); // [xiperware]
          this._toolTip.SetToolTip( this.pictureBox, this._pathsArray[newIndex].ToString() );
        }
			}
			else
			{
        pictureBox.Cursor = Cursors.Default;
				this._toolTip.RemoveAll();
			}
			this._activeIndex = newIndex;
		}

		private void pictureBox_MouseLeave(object sender, System.EventArgs e)
		{
			this._activeIndex = -1;
      this.Cursor = Cursors.Default;

      if( this.MyMouseLeave != null )  // [xiperware]
        this.MyMouseLeave( sender, e );
		}

    
    private void pictureBox_MouseEnter( object sender, EventArgs e )  // [xiperware]
    {
      if( this.MyMouseEnter != null )
        this.MyMouseEnter( sender, e );
    }

		//private void pictureBox_Click(object sender, System.EventArgs e)
    private void pictureBox_Click( object sender, MouseEventArgs e )  // [xiperware]
		{
			Point p = this.PointToClient(Cursor.Position);
			if(this._activeIndex == -1)
                this.getActiveIndexAtPoint(p);
			//if(this._activeIndex > -1 && this.RegionClick != null)
      //  this.RegionClick(this._activeIndex, this._pathsArray[this._activeIndex].ToString());
      if(this._activeIndex > -1 && this.RegionClick != null && this._clickable[this._activeIndex])  // [xiperware]
				this.RegionClick(this._activeIndex, this._tag[this._activeIndex], e.Button);
		}

		private int getActiveIndexAtPoint(Point point)
		{
			System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
			System.Drawing.Drawing2D.GraphicsPathIterator iterator = new System.Drawing.Drawing2D.GraphicsPathIterator(_pathData);
			iterator.Rewind();
			for(int current=0; current < iterator.SubpathCount; current++)
			{
				iterator.NextMarker(path);
				if(path.IsVisible(point, this._graphics))
					return current;
			}
			return -1;
		}

		[Browsable(false)]
		public override Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}
	}
}
