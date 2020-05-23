/* 
 * Enhanced TrackBar Control by Mick Doherty
 * http://www.dotnetrix.co.uk/controls.html
 * v1.0.2516.574
 * 
 * Disassembled, with permission, from VisualBasic DLL.
 */


using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;


namespace Dotnetrix.Controls
{
  [Flags]
  public enum TrackBarOwnerDrawParts
  {
    Channel = 4,
    None = 0,
    Thumb = 2,
    Ticks = 1
  }

  public enum TrackBarItemState
  {
    Active = 3,
    Disabled = 5,
    Hot = 2,
    Normal = 1
  }

  [ToolboxBitmap( typeof( System.Windows.Forms.TrackBar ) )]
  public class TrackBar : System.Windows.Forms.TrackBar
  {
    private Rectangle ChannelBounds;
    private TrackBarOwnerDrawParts m_OwnerDrawParts = TrackBarOwnerDrawParts.None;
    private Rectangle ThumbBounds;
    private int ThumbState;

    public event TrackBarDrawItemEventHandler DrawChannel;
    public event TrackBarDrawItemEventHandler DrawThumb;
    public event TrackBarDrawItemEventHandler DrawTicks;

    public TrackBar()
    {
      this.SetStyle( ControlStyles.SupportsTransparentBackColor, true );
    }

    private void DrawHorizontalTicks( Graphics g, Color color )
    {
      RectangleF ef3;
      int num = ( ( this.Maximum - this.Minimum ) / this.TickFrequency ) - 1;
      Pen pen = new Pen( color );
      RectangleF ef4 = new RectangleF( (float)( this.ChannelBounds.Left + ( this.ThumbBounds.Width / 2 ) ), (float)( this.ThumbBounds.Top - 5 ), 0f, 3f );
      RectangleF ef2 = ef4;
      ef4 = new RectangleF( (float)( ( this.ChannelBounds.Right - ( this.ThumbBounds.Width / 2 ) ) - 1 ), (float)( this.ThumbBounds.Top - 5 ), 0f, 3f );
      RectangleF ef = ef4;
      float x = ( ef.Right - ef2.Left ) / ( (float)( num + 1 ) );
      if( this.TickStyle != TickStyle.BottomRight )
      {
        g.DrawLine( pen, ef2.Left, ef2.Top, ef2.Right, ef2.Bottom );
        g.DrawLine( pen, ef.Left, ef.Top, ef.Right, ef.Bottom );
        ef3 = ef2;
        ef3.Height -= 1f;
        ef3.Offset( x, 1f );
        int num6 = num - 1;
        for( int i = 0; i <= num6; i++ )
        {
          g.DrawLine( pen, ef3.Left, ef3.Top, ef3.Right, ef3.Bottom );
          ef3.Offset( x, 0f );
        }
      }
      ef2.Offset( 0f, (float)( this.ThumbBounds.Height + 6 ) );
      ef.Offset( 0f, (float)( this.ThumbBounds.Height + 6 ) );
      if( this.TickStyle != TickStyle.TopLeft )
      {
        g.DrawLine( pen, ef2.Left, ef2.Top, ef2.Right, ef2.Bottom );
        g.DrawLine( pen, ef.Left, ef.Top, ef.Right, ef.Bottom );
        ef3 = ef2;
        ef3.Height -= 1f;
        ef3.Offset( x, 0f );
        int num5 = num - 1;
        for( int j = 0; j <= num5; j++ )
        {
          g.DrawLine( pen, ef3.Left, ef3.Top, ef3.Right, ef3.Bottom );
          ef3.Offset( x, 0f );
        }
      }
      pen.Dispose();
    }

    private void DrawPointerDown( Graphics g )
    {
      Point[] points = new Point[6];
      Point point = new Point( this.ThumbBounds.Left + ( this.ThumbBounds.Width / 2 ), this.ThumbBounds.Bottom - 1 );
      points[0] = point;
      point = new Point( this.ThumbBounds.Left, ( this.ThumbBounds.Bottom - ( this.ThumbBounds.Width / 2 ) ) - 1 );
      points[1] = point;
      points[2] = this.ThumbBounds.Location;
      point = new Point( this.ThumbBounds.Right - 1, this.ThumbBounds.Top );
      points[3] = point;
      point = new Point( this.ThumbBounds.Right - 1, ( this.ThumbBounds.Bottom - ( this.ThumbBounds.Width / 2 ) ) - 1 );
      points[4] = point;
      points[5] = points[0];
      GraphicsPath path = new GraphicsPath();
      path.AddLines( points );
      Region region = new Region( path );
      g.Clip = region;
      if( ( this.ThumbState == 3 ) || !this.Enabled )
      {
        ControlPaint.DrawButton( g, this.ThumbBounds, ButtonState.All );
      }
      else
      {
        g.Clear( SystemColors.Control );
      }
      g.ResetClip();
      region.Dispose();
      path.Dispose();
      Point[] pointArray2 = new Point[] { points[0], points[1], points[2], points[3] };
      g.DrawLines( SystemPens.ControlLightLight, pointArray2 );
      pointArray2 = new Point[] { points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDarkDark, pointArray2 );
      points[0].Offset( 0, -1 );
      points[1].Offset( 1, 0 );
      points[2].Offset( 1, 1 );
      points[3].Offset( -1, 1 );
      points[4].Offset( -1, 0 );
      points[5] = points[0];
      pointArray2 = new Point[] { points[0], points[1], points[2], points[3] };
      g.DrawLines( SystemPens.ControlLight, pointArray2 );
      pointArray2 = new Point[] { points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDark, pointArray2 );
    }

    private void DrawPointerLeft( Graphics g )
    {
      Point[] points = new Point[6];
      Point point = new Point( this.ThumbBounds.Left, this.ThumbBounds.Top + ( this.ThumbBounds.Height / 2 ) );
      points[0] = point;
      point = new Point( this.ThumbBounds.Left + ( this.ThumbBounds.Height / 2 ), this.ThumbBounds.Top );
      points[1] = point;
      point = new Point( this.ThumbBounds.Right - 1, this.ThumbBounds.Top );
      points[2] = point;
      point = new Point( this.ThumbBounds.Right - 1, this.ThumbBounds.Bottom - 1 );
      points[3] = point;
      point = new Point( this.ThumbBounds.Left + ( this.ThumbBounds.Height / 2 ), this.ThumbBounds.Bottom - 1 );
      points[4] = point;
      points[5] = points[0];
      GraphicsPath path = new GraphicsPath();
      path.AddLines( points );
      Region region = new Region( path );
      g.Clip = region;
      if( ( this.ThumbState == 3 ) || !this.Enabled )
      {
        ControlPaint.DrawButton( g, this.ThumbBounds, ButtonState.All );
      }
      else
      {
        g.Clear( SystemColors.Control );
      }
      g.ResetClip();
      region.Dispose();
      path.Dispose();
      Point[] pointArray2 = new Point[] { points[0], points[1], points[2] };
      g.DrawLines( SystemPens.ControlLightLight, pointArray2 );
      pointArray2 = new Point[] { points[2], points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDarkDark, pointArray2 );
      points[0].Offset( 1, 0 );
      points[1].Offset( 0, 1 );
      points[2].Offset( -1, 1 );
      points[3].Offset( -1, -1 );
      points[4].Offset( 0, -1 );
      points[5] = points[0];
      pointArray2 = new Point[] { points[0], points[1], points[2] };
      g.DrawLines( SystemPens.ControlLight, pointArray2 );
      pointArray2 = new Point[] { points[2], points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDark, pointArray2 );
    }

    private void DrawPointerRight( Graphics g )
    {
      Point[] points = new Point[6];
      Point point = new Point( this.ThumbBounds.Left, this.ThumbBounds.Bottom - 1 );
      points[0] = point;
      point = new Point( this.ThumbBounds.Left, this.ThumbBounds.Top );
      points[1] = point;
      point = new Point( ( this.ThumbBounds.Right - ( this.ThumbBounds.Height / 2 ) ) - 1, this.ThumbBounds.Top );
      points[2] = point;
      point = new Point( this.ThumbBounds.Right - 1, this.ThumbBounds.Top + ( this.ThumbBounds.Height / 2 ) );
      points[3] = point;
      point = new Point( ( this.ThumbBounds.Right - ( this.ThumbBounds.Height / 2 ) ) - 1, this.ThumbBounds.Bottom - 1 );
      points[4] = point;
      points[5] = points[0];
      GraphicsPath path = new GraphicsPath();
      path.AddLines( points );
      Region region = new Region( path );
      g.Clip = region;
      if( ( this.ThumbState == 3 ) || !this.Enabled )
      {
        ControlPaint.DrawButton( g, this.ThumbBounds, ButtonState.All );
      }
      else
      {
        g.Clear( SystemColors.Control );
      }
      g.ResetClip();
      region.Dispose();
      path.Dispose();
      Point[] pointArray2 = new Point[] { points[0], points[1], points[2], points[3] };
      g.DrawLines( SystemPens.ControlLightLight, pointArray2 );
      pointArray2 = new Point[] { points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDarkDark, pointArray2 );
      points[0].Offset( 1, -1 );
      points[1].Offset( 1, 1 );
      points[2].Offset( 0, 1 );
      points[3].Offset( -1, 0 );
      points[4].Offset( 0, -1 );
      points[5] = points[0];
      pointArray2 = new Point[] { points[0], points[1], points[2], points[3] };
      g.DrawLines( SystemPens.ControlLight, pointArray2 );
      pointArray2 = new Point[] { points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDark, pointArray2 );
    }

    private void DrawPointerUp( Graphics g )
    {
      Point[] points = new Point[6];
      Point point = new Point( this.ThumbBounds.Left, this.ThumbBounds.Bottom - 1 );
      points[0] = point;
      point = new Point( this.ThumbBounds.Left, this.ThumbBounds.Top + ( this.ThumbBounds.Width / 2 ) );
      points[1] = point;
      point = new Point( this.ThumbBounds.Left + ( this.ThumbBounds.Width / 2 ), this.ThumbBounds.Top );
      points[2] = point;
      point = new Point( this.ThumbBounds.Right - 1, this.ThumbBounds.Top + ( this.ThumbBounds.Width / 2 ) );
      points[3] = point;
      point = new Point( this.ThumbBounds.Right - 1, this.ThumbBounds.Bottom - 1 );
      points[4] = point;
      points[5] = points[0];
      GraphicsPath path = new GraphicsPath();
      path.AddLines( points );
      Region region = new Region( path );
      g.Clip = region;
      if( ( this.ThumbState == 3 ) || !this.Enabled )
      {
        ControlPaint.DrawButton( g, this.ThumbBounds, ButtonState.All );
      }
      else
      {
        g.Clear( SystemColors.Control );
      }
      g.ResetClip();
      region.Dispose();
      path.Dispose();
      Point[] pointArray2 = new Point[] { points[0], points[1], points[2] };
      g.DrawLines( SystemPens.ControlLightLight, pointArray2 );
      pointArray2 = new Point[] { points[2], points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDarkDark, pointArray2 );
      points[0].Offset( 1, -1 );
      points[1].Offset( 1, 0 );
      points[2].Offset( 0, 1 );
      points[3].Offset( -1, 0 );
      points[4].Offset( -1, -1 );
      points[5] = points[0];
      pointArray2 = new Point[] { points[0], points[1], points[2] };
      g.DrawLines( SystemPens.ControlLight, pointArray2 );
      pointArray2 = new Point[] { points[2], points[3], points[4], points[5] };
      g.DrawLines( SystemPens.ControlDark, pointArray2 );
    }

    private void DrawVerticalTicks( Graphics g, Color color )
    {
      RectangleF ef3;
      int num = ( ( this.Maximum - this.Minimum ) / this.TickFrequency ) - 1;
      Pen pen = new Pen( color );
      RectangleF ef4 = new RectangleF( (float)( this.ThumbBounds.Left - 5 ), (float)( ( this.ChannelBounds.Bottom - ( this.ThumbBounds.Height / 2 ) ) - 1 ), 3f, 0f );
      RectangleF ef2 = ef4;
      ef4 = new RectangleF( (float)( this.ThumbBounds.Left - 5 ), (float)( this.ChannelBounds.Top + ( this.ThumbBounds.Height / 2 ) ), 3f, 0f );
      RectangleF ef = ef4;
      float y = ( ef.Bottom - ef2.Top ) / ( (float)( num + 1 ) );
      if( this.TickStyle != TickStyle.BottomRight )
      {
        g.DrawLine( pen, ef2.Left, ef2.Top, ef2.Right, ef2.Bottom );
        g.DrawLine( pen, ef.Left, ef.Top, ef.Right, ef.Bottom );
        ef3 = ef2;
        ef3.Width -= 1f;
        ef3.Offset( 1f, y );
        int num6 = num - 1;
        for( int i = 0; i <= num6; i++ )
        {
          g.DrawLine( pen, ef3.Left, ef3.Top, ef3.Right, ef3.Bottom );
          ef3.Offset( 0f, y );
        }
      }
      ef2.Offset( (float)( this.ThumbBounds.Width + 6 ), 0f );
      ef.Offset( (float)( this.ThumbBounds.Width + 6 ), 0f );
      if( this.TickStyle != TickStyle.TopLeft )
      {
        g.DrawLine( pen, ef2.Left, ef2.Top, ef2.Right, ef2.Bottom );
        g.DrawLine( pen, ef.Left, ef.Top, ef.Right, ef.Bottom );
        ef3 = ef2;
        ef3.Width -= 1f;
        ef3.Offset( 0f, y );
        int num5 = num - 1;
        for( int j = 0; j <= num5; j++ )
        {
          g.DrawLine( pen, ef3.Left, ef3.Top, ef3.Right, ef3.Bottom );
          ef3.Offset( 0f, y );
        }
      }
      pen.Dispose();
    }

    protected virtual void OnDrawChannel( IntPtr hdc )
    {
      Graphics graphics = Graphics.FromHdc( hdc );
      if( ( ( this.OwnerDrawParts & TrackBarOwnerDrawParts.Channel ) == TrackBarOwnerDrawParts.Channel ) && !this.DesignMode )
      {
        TrackBarDrawItemEventArgs e = new TrackBarDrawItemEventArgs( graphics, this.ChannelBounds, (TrackBarItemState)this.ThumbState );
        if( this.DrawChannel != null )
        {
          this.DrawChannel( this, e );
        }
      }
      else
      {
        if( this.ChannelBounds.Equals( Rectangle.Empty ) )
        {
          return;
        }
        if( VisualStylesEnabled )
        {
          IntPtr hTheme = NativeMethods.OpenThemeData( this.Handle, "TRACKBAR" );
          if( !hTheme.Equals( IntPtr.Zero ) )
          {
            NativeMethods.RECT pRect = new NativeMethods.RECT( this.ChannelBounds );
            bool flag = NativeMethods.DrawThemeBackground( hTheme, hdc, 1, 1, ref pRect, ref pRect ) == 0;
            NativeMethods.CloseThemeData( hTheme );
            if( flag )
            {
              return;
            }
          }
        }
        ControlPaint.DrawBorder3D( graphics, this.ChannelBounds, Border3DStyle.Sunken );
      }
      graphics.Dispose();
    }

    protected virtual void OnDrawThumb( IntPtr hdc )
    {
      Graphics graphics = Graphics.FromHdc( hdc );
      graphics.Clip = new Region( this.ThumbBounds );
      if( ( ( this.OwnerDrawParts & TrackBarOwnerDrawParts.Thumb ) == TrackBarOwnerDrawParts.Thumb ) && !this.DesignMode )
      {
        TrackBarDrawItemEventArgs e = new TrackBarDrawItemEventArgs( graphics, this.ThumbBounds, (TrackBarItemState)this.ThumbState );
        if( this.DrawThumb != null )
        {
          this.DrawThumb( this, e );
        }
      }
      else
      {
        int iPartId = 0;
        if( this.ThumbBounds.Equals( Rectangle.Empty ) )
        {
          return;
        }
        switch( this.TickStyle )
        {
          case TickStyle.None:
          case TickStyle.BottomRight:
            if( this.Orientation != Orientation.Horizontal )
            {
              iPartId = 8;
              break;
            }
            iPartId = 4;
            break;

          case TickStyle.TopLeft:
            if( this.Orientation != Orientation.Horizontal )
            {
              iPartId = 7;
              break;
            }
            iPartId = 5;
            break;

          case TickStyle.Both:
            if( this.Orientation != Orientation.Horizontal )
            {
              iPartId = 6;
              break;
            }
            iPartId = 3;
            break;
        }
        if( VisualStylesEnabled )
        {
          IntPtr hTheme = NativeMethods.OpenThemeData( this.Handle, "TRACKBAR" );
          if( !hTheme.Equals( IntPtr.Zero ) )
          {
            NativeMethods.RECT pRect = new NativeMethods.RECT( this.ThumbBounds );
            bool flag = NativeMethods.DrawThemeBackground( hTheme, hdc, iPartId, this.ThumbState, ref pRect, ref pRect ) == 0;
            NativeMethods.CloseThemeData( hTheme );
            if( flag )
            {
              graphics.ResetClip();
              graphics.Dispose();
              return;
            }
          }
        }
        switch( iPartId )
        {
          case 4:
            this.DrawPointerDown( graphics );
            goto Label_01AD;

          case 5:
            this.DrawPointerUp( graphics );
            goto Label_01AD;

          case 7:
            this.DrawPointerLeft( graphics );
            goto Label_01AD;

          case 8:
            this.DrawPointerRight( graphics );
            goto Label_01AD;
        }
        if( ( this.ThumbState == 3 ) || !this.Enabled )
        {
          ControlPaint.DrawButton( graphics, this.ThumbBounds, ButtonState.All );
        }
        else
        {
          graphics.FillRectangle( SystemBrushes.Control, this.ThumbBounds );
        }
        ControlPaint.DrawBorder3D( graphics, this.ThumbBounds, Border3DStyle.Raised );
      }
    Label_01AD:
      graphics.ResetClip();
      graphics.Dispose();
    }

    protected virtual void OnDrawTicks( IntPtr hdc )
    {
      Graphics graphics = Graphics.FromHdc( hdc );
      if( ( ( this.OwnerDrawParts & TrackBarOwnerDrawParts.Ticks ) == TrackBarOwnerDrawParts.Ticks ) && !this.DesignMode )
      {
        Rectangle bounds;
        Rectangle rectangle2;
        if( this.Orientation == Orientation.Horizontal )
        {
          rectangle2 = new Rectangle( this.ChannelBounds.Left + ( this.ThumbBounds.Width / 2 ), this.ThumbBounds.Top - 6, this.ChannelBounds.Width - this.ThumbBounds.Width, this.ThumbBounds.Height + 10 );
          bounds = rectangle2;
        }
        else
        {
          rectangle2 = new Rectangle( this.ThumbBounds.Left - ( this.ThumbBounds.Height / 2 ), this.ChannelBounds.Top + 6, this.ThumbBounds.Width + 10, this.ChannelBounds.Height - this.ThumbBounds.Height );
          bounds = rectangle2;
        }
        TrackBarDrawItemEventArgs e = new TrackBarDrawItemEventArgs( graphics, bounds, (TrackBarItemState)this.ThumbState );
        if( this.DrawTicks != null )
        {
          this.DrawTicks( this, e );
        }
      }
      else
      {
        if( this.TickStyle == TickStyle.None )
        {
          return;
        }
        if( this.ThumbBounds.Equals( Rectangle.Empty ) )
        {
          return;
        }
        Color black = Color.Black;
        if( VisualStylesEnabled )
        {
          IntPtr hTheme = NativeMethods.OpenThemeData( this.Handle, "TRACKBAR" );
          if( !hTheme.Equals( IntPtr.Zero ) )
          {
            int pColor = 0;
            if( NativeMethods.GetThemeColor( hTheme, 9, this.ThumbState, 0xcc, ref pColor ) == 0 )
            {
              black = ColorTranslator.FromWin32( pColor );
            }
            NativeMethods.CloseThemeData( hTheme );
          }
        }
        if( this.Orientation == Orientation.Horizontal )
        {
          this.DrawHorizontalTicks( graphics, black );
        }
        else
        {
          this.DrawVerticalTicks( graphics, black );
        }
      }
      graphics.Dispose();
    }

    protected override void OnMouseMove( MouseEventArgs e )
    {
      base.OnMouseMove( e );
      if( ( e.Button == MouseButtons.None ) && this.ThumbBounds.Contains( e.X, e.Y ) )
      {
        this.ThumbState = 2;
        this.Invalidate( new Region( this.ThumbBounds ) );
      }
    }

    [PermissionSet( SecurityAction.Demand, Unrestricted = true )]
    protected override void WndProc( ref Message m )
    {
      IntPtr ptr;
      if( m.Msg == 20 )
      {
        ptr = new IntPtr( 1 );
        m.Result = ptr;
      }
      base.WndProc( ref m );
      if( m.Msg == 0x204e )
      {
        NativeMethods.NMHDR structure = (NativeMethods.NMHDR)Marshal.PtrToStructure( m.LParam, typeof( NativeMethods.NMHDR ) );
        if( structure.code == -12 )
        {
          Marshal.StructureToPtr( structure, m.LParam, false );
          NativeMethods.NMCUSTOMDRAW nmcustomdraw = (NativeMethods.NMCUSTOMDRAW)Marshal.PtrToStructure( m.LParam, typeof( NativeMethods.NMCUSTOMDRAW ) );
          if( nmcustomdraw.dwDrawStage == NativeMethods.CustomDrawDrawStage.CDDS_PREPAINT )
          {
            Graphics graphics = Graphics.FromHdc( nmcustomdraw.hdc );
            PaintEventArgs e = new PaintEventArgs( graphics, this.Bounds );
            e.Graphics.TranslateTransform( (float)( 0 - this.Left ), (float)( 0 - this.Top ) );
            this.InvokePaintBackground( this.Parent, e );
            this.InvokePaint( this.Parent, e );
            SolidBrush brush = new SolidBrush( this.BackColor );
            e.Graphics.FillRectangle( brush, this.Bounds );
            brush.Dispose();
            e.Graphics.ResetTransform();
            e.Dispose();
            graphics.Dispose();
            ptr = new IntPtr( 0x30 );
            m.Result = ptr;
          }
          else if( nmcustomdraw.dwDrawStage == NativeMethods.CustomDrawDrawStage.CDDS_POSTPAINT )
          {
            this.OnDrawTicks( nmcustomdraw.hdc );
            this.OnDrawChannel( nmcustomdraw.hdc );
            this.OnDrawThumb( nmcustomdraw.hdc );
          }
          else if( nmcustomdraw.dwDrawStage == NativeMethods.CustomDrawDrawStage.CDDS_ITEMPREPAINT )
          {
            if( nmcustomdraw.dwItemSpec.ToInt32() == 2 )
            {
              this.ThumbBounds = nmcustomdraw.rc.ToRectangle();
              if( this.Enabled )
              {
                if( nmcustomdraw.uItemState == NativeMethods.CustomDrawItemState.CDIS_SELECTED )
                {
                  this.ThumbState = 3;
                }
                else
                {
                  this.ThumbState = 1;
                }
              }
              else
              {
                this.ThumbState = 5;
              }
              this.OnDrawThumb( nmcustomdraw.hdc );
            }
            else if( nmcustomdraw.dwItemSpec.ToInt32() == 3 )
            {
              this.ChannelBounds = nmcustomdraw.rc.ToRectangle();
              this.OnDrawChannel( nmcustomdraw.hdc );
            }
            else if( nmcustomdraw.dwItemSpec.ToInt32() == 1 )
            {
              this.OnDrawTicks( nmcustomdraw.hdc );
            }
            ptr = new IntPtr( 4 );
            m.Result = ptr;
          }
        }
      }
    }

    [DefaultValue( typeof( TrackBarOwnerDrawParts ), "None" ), Description( "Gets/sets the trackbar parts that will be OwnerDrawn." ), DesignerSerializationVisibility( DesignerSerializationVisibility.Visible ), Editor( typeof( TrackDrawModeEditor ), typeof( UITypeEditor ) )]
    public TrackBarOwnerDrawParts OwnerDrawParts
    {
      get
      {
        return this.m_OwnerDrawParts;
      }
      set
      {
        this.m_OwnerDrawParts = value;
      }
    }

    private static bool VisualStylesEnabled
    {
      get
      {
        NativeMethods.DLLVERSIONINFO structure = new NativeMethods.DLLVERSIONINFO();
        structure.cbSize = Marshal.SizeOf( structure );
        NativeMethods.CommonControlsGetVersion( ref structure );
        if( structure.dwMajorVersion >= 6 )
        {
          return NativeMethods.IsAppThemed();
        }
        return false;
      }
    }
  }




  public delegate void TrackBarDrawItemEventHandler( object sender, TrackBarDrawItemEventArgs e );

  public class TrackBarDrawItemEventArgs : EventArgs
  {
    private Rectangle m_Bounds;
    private System.Drawing.Graphics m_Graphics;
    private TrackBarItemState m_State;

    public TrackBarDrawItemEventArgs( System.Drawing.Graphics graphics, Rectangle bounds, TrackBarItemState state )
    {
      this.m_Graphics = graphics;
      this.m_Bounds = bounds;
      this.m_State = state;
    }

    public Rectangle Bounds
    {
      get
      {
        return this.m_Bounds;
      }
    }

    public System.Drawing.Graphics Graphics
    {
      get
      {
        return this.m_Graphics;
      }
    }

    public TrackBarItemState State
    {
      get
      {
        return this.m_State;
      }
    }
  }




  [PermissionSet( SecurityAction.LinkDemand, Unrestricted = true ), PermissionSet( SecurityAction.InheritanceDemand, Unrestricted = true )]
  public class TrackDrawModeEditor : UITypeEditor
  {
    public override object EditValue( ITypeDescriptorContext context, IServiceProvider provider, object value )
    {
      TrackBarOwnerDrawParts parts = TrackBarOwnerDrawParts.None;
      IEnumerator enumerator = null;
      if( !( value is TrackBarOwnerDrawParts ) || ( provider == null ) )
      {
        return value;
      }
      IWindowsFormsEditorService service = (IWindowsFormsEditorService)provider.GetService( typeof( IWindowsFormsEditorService ) );
      if( service == null )
      {
        return value;
      }
      CheckedListBox control = new CheckedListBox();
      control.BorderStyle = BorderStyle.None;
      control.CheckOnClick = true;
      control.Items.Add( "Ticks", ( ( (Dotnetrix.Controls.TrackBar)context.Instance ).OwnerDrawParts & TrackBarOwnerDrawParts.Ticks ) == TrackBarOwnerDrawParts.Ticks );
      control.Items.Add( "Thumb", ( ( (Dotnetrix.Controls.TrackBar)context.Instance ).OwnerDrawParts & TrackBarOwnerDrawParts.Thumb ) == TrackBarOwnerDrawParts.Thumb );
      control.Items.Add( "Channel", ( ( (Dotnetrix.Controls.TrackBar)context.Instance ).OwnerDrawParts & TrackBarOwnerDrawParts.Channel ) == TrackBarOwnerDrawParts.Channel );
      service.DropDownControl( control );
      try
      {
        enumerator = control.CheckedItems.GetEnumerator();
        while( enumerator.MoveNext() )
        {
          object objectValue = RuntimeHelpers.GetObjectValue( enumerator.Current );
          parts |= (TrackBarOwnerDrawParts)Enum.Parse( typeof( TrackBarOwnerDrawParts ), objectValue.ToString() );
        }
      }
      finally
      {
        if( enumerator is IDisposable )
        {
          ( (IDisposable)enumerator ).Dispose();
        }
      }
      control.Dispose();
      service.CloseDropDown();
      return parts;
    }

    public override UITypeEditorEditStyle GetEditStyle( ITypeDescriptorContext context )
    {
      return UITypeEditorEditStyle.DropDown;
    }
  }





  internal class NativeMethods
  {
    public const int NM_CUSTOMDRAW = -12;
    public const int NM_FIRST = 0;
    public const int S_OK = 0;
    public const int TMT_COLOR = 0xcc;
    public const int WM_ERASEBKGND = 20;
    public const int WM_NOTIFY = 0x4e;
    public const int WM_REFLECT = 0x2000;
    public const int WM_USER = 0x400;

    // [xiperware] fix for unbalanced stack

    //[DllImport( "UxTheme.dll", CallingConvention = CallingConvention.Cdecl )]
    //public static extern int CloseThemeData( IntPtr hTheme );
    [DllImport( "UxTheme.dll", ExactSpelling = true )]
    public static extern Int32 CloseThemeData( IntPtr hTheme );

    // [DllImport( "Comctl32.dll", EntryPoint = "DllGetVersion", CallingConvention = CallingConvention.Cdecl )]
    // public static extern int CommonControlsGetVersion( ref DLLVERSIONINFO pdvi );
    [DllImport( "shell32.dll", EntryPoint = "DllGetVersion" )]
    public static extern int CommonControlsGetVersion( ref DLLVERSIONINFO pdvi );

    //[DllImport( "UxTheme.dll", CallingConvention = CallingConvention.Cdecl )]
    //public static extern int DrawThemeBackground( IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, ref RECT pClipRect );
    [DllImport( "UxTheme.dll", ExactSpelling = true )]
    public extern static Int32 DrawThemeBackground( IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, ref RECT pClipRect );

    //[DllImport( "UxTheme.dll", CallingConvention = CallingConvention.Cdecl )]
    //public static extern int GetThemeColor( IntPtr hTheme, int iPartId, int iStateId, int iPropId, ref int pColor );
    [DllImport( "UxTheme.dll", ExactSpelling = true )]
    public extern static Int32 GetThemeColor( IntPtr hTheme, int iPartId, int iStateId, int iPropId, ref int pColor );

    //[return: MarshalAs( UnmanagedType.Bool )]
    //[DllImport( "UxTheme.dll", CallingConvention = CallingConvention.Cdecl )]
    //public static extern bool IsAppThemed();
    [return: MarshalAs( UnmanagedType.Bool )]
    [DllImport( "UxTheme.dll", ExactSpelling = true )]
    public extern static bool IsAppThemed();

    //[DllImport( "UxTheme.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode )]
    //public static extern IntPtr OpenThemeData( IntPtr hwnd, string pszClassList );
    [DllImport( "UxTheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode )]
    public static extern IntPtr OpenThemeData( IntPtr hWnd, String classList );

    public enum CustomDrawDrawStage
    {
      CDDS_ITEM = 0x10000,
      CDDS_ITEMPOSTERASE = 0x10004,
      CDDS_ITEMPOSTPAINT = 0x10002,
      CDDS_ITEMPREERASE = 0x10003,
      CDDS_ITEMPREPAINT = 0x10001,
      CDDS_POSTERASE = 4,
      CDDS_POSTPAINT = 2,
      CDDS_PREERASE = 3,
      CDDS_PREPAINT = 1,
      CDDS_SUBITEM = 0x20000
    }

    public enum CustomDrawItemState
    {
      CDIS_CHECKED = 8,
      CDIS_DEFAULT = 0x20,
      CDIS_DISABLED = 4,
      CDIS_FOCUS = 0x10,
      CDIS_GRAYED = 2,
      CDIS_HOT = 0x40,
      CDIS_INDETERMINATE = 0x100,
      CDIS_MARKED = 0x80,
      CDIS_SELECTED = 1,
      CDIS_SHOWKEYBOARDCUES = 0x200
    }

    public enum CustomDrawReturnFlags
    {
      CDRF_DODEFAULT = 0,
      CDRF_NEWFONT = 2,
      CDRF_NOTIFYITEMDRAW = 0x20,
      CDRF_NOTIFYPOSTERASE = 0x40,
      CDRF_NOTIFYPOSTPAINT = 0x10,
      CDRF_NOTIFYSUBITEMDRAW = 0x20,
      CDRF_SKIPDEFAULT = 4
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct DLLVERSIONINFO
    {
      public int cbSize;
      public int dwMajorVersion;
      public int dwMinorVersion;
      public int dwBuildNumber;
      public int dwPlatformID;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct NMCUSTOMDRAW
    {
      public NativeMethods.NMHDR hdr;
      public NativeMethods.CustomDrawDrawStage dwDrawStage;
      public IntPtr hdc;
      public NativeMethods.RECT rc;
      public IntPtr dwItemSpec;
      public NativeMethods.CustomDrawItemState uItemState;
      public IntPtr lItemlParam;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct NMHDR
    {
      public IntPtr HWND;
      public int idFrom;
      public int code;
      public override string ToString()
      {
        return string.Format( CultureInfo.InvariantCulture, "Hwnd: {0}, ControlID: {1}, Code: {2}", new object[] { this.HWND, this.idFrom, this.code } );
      }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
      public RECT( Rectangle rect )
      {
        this = new NativeMethods.RECT();
        this.Left = rect.Left;
        this.Top = rect.Top;
        this.Right = rect.Right;
        this.Bottom = rect.Bottom;
      }

      public override string ToString()
      {
        return string.Format( CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", new object[] { this.Left, this.Top, this.Right, this.Bottom } );
      }

      public Rectangle ToRectangle()
      {
        return Rectangle.FromLTRB( this.Left, this.Top, this.Right, this.Bottom );
      }
    }

    public enum TrackBarCustomDrawPart
    {
      TBCD_CHANNEL = 3,
      TBCD_THUMB = 2,
      TBCD_TICS = 1
    }

    public enum TrackBarParts
    {
      TKP_THUMB = 3,
      TKP_THUMBBOTTOM = 4,
      TKP_THUMBLEFT = 7,
      TKP_THUMBRIGHT = 8,
      TKP_THUMBTOP = 5,
      TKP_THUMBVERT = 6,
      TKP_TICS = 9,
      TKP_TICSVERT = 10,
      TKP_TRACK = 1,
      TKP_TRACKVERT = 2
    }
  }
}

