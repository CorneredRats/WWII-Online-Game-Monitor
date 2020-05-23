/* =============================================================================
 * BEGameMonitor                                    Copyright (c) 2013 Xiperware
 * http://begm.sourceforge.net/                              xiperware@gmail.com
 * 
 * This file is part of Battleground Europe Game Monitor.
 * 
 * Battleground Europe Game Monitor is free software; you can redistribute it
 * and/or modify it under the terms of the GNU General Public License v2 as
 * published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

namespace BEGM
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Drawing.Imaging;
  using System.Windows.Forms;
  using Xiperware.WiretapAPI;

  /// <summary>
  /// A scrollable, zoomable image viewer control capable of handling large images.
  /// </summary>
  /// <remarks>
  /// Based on VB code and article by Mohammed Abd Alla.
  /// http://www.codeproject.com/useritems/Picture_Viewer.asp
  /// </remarks>
  public class MapViewer : UserControl
  {
    #region Variables

    const int WM_ERASEBKGND = 0x0014;

    private float zoomFactor = 1;
    private float prevZoomFactor = 1;
    private float maxZoomFactor = 20;
    private float minZoomFactor = 0.05F;

    private Bitmap newOverlay = null;  // only used between Begin & EndDrawNewOverlay()

    private IntPtr hdcSrcBackground = IntPtr.Zero;
    private IntPtr hBmpSrcBackground = IntPtr.Zero;
    private IntPtr hdcSrcOverlay = IntPtr.Zero;
    private IntPtr hBmpSrcOverlay = IntPtr.Zero;

    private RectangleF rectSrc;
    private RectangleF rectDest;
    private PointF CP;
    private PointF CS;
    private PointF P;

    private bool mouseDown = false;

    private bool Xout = false;
    private bool Yout = false;

    private int fullHeight = 0;
    private int fullWidth = 0;
    private Size prevMapSize;

    private GraphicsPath paths;
    private List<object> pathTags;

    private Timer tmrMomentum;
    private Point prevMouseLocation;
    private Point currMouseLocation;
    private double momentumSpeed;
    private double momuntumAngle;

    private Timer tmrScroll;
    private int scrollIndex = 0;
    private PointF scrollFrom;
    private PointF scrollTo;
    private float zoomFrom;
    private float zoomTo;

    private Timer tmrZoomFastScaling;

    private readonly float[] beizerCurve;

    #endregion

    #region Delegates

    public delegate void ZoomChangedEventHandler( float zoom );

    #endregion

    #region Events

    /// <summary>
    /// The mouse has been clicked once on the map (without dragging).
    /// </summary>
    public event MouseEventHandler MapMouseClick;

    /// <summary>
    /// The mouse has been double clicked on the map.
    /// </summary>
    public event MouseEventHandler MapMouseDoubleClick;

    /// <summary>
    /// The mouse is moving over the map.
    /// </summary>
    public event MouseEventHandler MapMouseMove;

    /// <summary>
    /// The zoom level has been changed.
    /// </summary>
    public event ZoomChangedEventHandler MapZoomChanged;

    /// <summary>
    /// A call to ScrollToPoint() has completed animating.
    /// </summary>
    public event EventHandler MapScrollToPointCompleted;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new MapViewer control.
    /// </summary>
    public MapViewer()
    {
      this.SetStyle( ControlStyles.DoubleBuffer, true );

      paths = new GraphicsPath();
      pathTags = new List<object>();

      tmrScroll = new Timer();
      tmrScroll.Interval = 30;
      tmrScroll.Tick += tmrScroll_Tick;

      tmrMomentum = new Timer();
      tmrMomentum.Interval = 30;
      tmrMomentum.Tick += tmrMomentum_Tick;

      tmrZoomFastScaling = new Timer();
      tmrZoomFastScaling.Interval = 200;
      tmrZoomFastScaling.Tick += tmrZoomFastScaling_Tick;


      // init beizer values, used for smooth scrolling/zooming

      beizerCurve = new float[23];
      beizerCurve[0]  = 0.0079F;
      beizerCurve[1]  = 0.0241F;
      beizerCurve[2]  = 0.0426F;
      beizerCurve[3]  = 0.0629F;
      beizerCurve[4]  = 0.0857F;
      beizerCurve[5]  = 0.1135F;
      beizerCurve[6]  = 0.1442F;
      beizerCurve[7]  = 0.1817F;
      beizerCurve[8]  = 0.2265F;
      beizerCurve[9]  = 0.2840F;
      beizerCurve[10] = 0.3603F;
      beizerCurve[11] = 0.4718F;
      beizerCurve[12] = 0.5910F;
      beizerCurve[13] = 0.6907F;
      beizerCurve[14] = 0.7652F;
      beizerCurve[15] = 0.8208F;
      beizerCurve[16] = 0.8620F;
      beizerCurve[17] = 0.8972F;
      beizerCurve[18] = 0.9264F;
      beizerCurve[19] = 0.9515F;
      beizerCurve[20] = 0.9719F;
      beizerCurve[21] = 0.9912F;
      beizerCurve[22] = 1.0000F;


      // event handlers

      this.Paint            += MapViewer_Paint;
      this.MouseClick       += MapViewer_MouseClick;
      this.MouseDoubleClick += MapViewer_MouseDoubleClick;
      this.MouseDown        += MapViewer_MouseDown;
      this.MouseUp          += MapViewer_MouseUp;
      this.MouseMove        += MapViewer_MouseMove;
      this.MouseWheel       += MapViewer_MouseWheel;
      this.KeyDown          += MapViewer_KeyDown;
    }

    #endregion

    #region Properties

    /// <summary>
    /// True if map data has been loaded into memory ready to be drawn.
    /// </summary>
    public bool MapLoaded
    {
      get { return this.hdcSrcBackground != IntPtr.Zero; }
    }

    /// <summary>
    /// The size of the map in pixels.
    /// </summary>
    public Size MapSize
    {
      get { return new Size( this.fullWidth, this.fullHeight ); }
    }

    /// <summary>
    /// Gets or sets the current zoom factor being displayed.
    /// </summary>
    public float ZoomFactor
    {
      get { return this.zoomFactor; }
      set
      {
        this.zoomFactor = value;
        this.Redraw( new PointF( this.Width / 2F, this.Height / 2F ) );
      }
    }

    /// <summary>
    /// The maximum allowable zoom factor (1.0F is 1:1 ratio).
    /// </summary>
    public float MaxZoom
    {
      get { return this.maxZoomFactor; }
      set { this.maxZoomFactor = value; }
    }

    /// <summary>
    /// The minimum allowable zoom factor (0.1F is zoomed out 10x).
    /// </summary>
    public float MinZoom
    {
      get
      {
        if( !this.MapHighPerformanceMode || this.MapSize.IsEmpty )
          return this.minZoomFactor;
        // else, detatched and larger than default size, limit min zoon to keep map filling frame

        float minZoomWidth = (float)this.Size.Width / (float)this.MapSize.Width;
        float minZoomHeight = (float)this.Size.Height / (float)this.MapSize.Height;

        float max = this.minZoomFactor;
        if( minZoomWidth  > max ) max = minZoomWidth;
        if( minZoomHeight > max ) max = minZoomHeight;
        return max;
      }
      set { this.minZoomFactor = value; }
    }

    /// <summary>
    /// Sets or gets the center position of the map.
    /// </summary>
    public PointF Center
    {
      get { return new PointF( rectSrc.X + ( rectSrc.Width / 2 ), rectSrc.Y + ( rectSrc.Height / 2 ) ); }
      set
      {
        // doesn't work before map has been drawn first time
        // rectSrc.X = value.X - ( rectSrc.Width / 2 );
        // rectSrc.Y = value.Y - ( rectSrc.Height / 2 );

        // note: assumes can't zoom out further than map bounds
        rectSrc.X = value.X - ( this.Width / this.zoomFactor / 2 );
        rectSrc.Y = value.Y - ( this.Height / this.zoomFactor / 2 );

        this.Redraw( new PointF() );  // will handle bounding issues
      }
    }

    /// <summary>
    /// True if the map is still scrolling from the momentum of a user scroll.
    /// </summary>
    public bool IsScrolling
    {
      get { return tmrMomentum.Enabled; }
    }

    /// <summary>
    /// If true, disable momentum scrolling, use fast map scaling during panning.
    /// </summary>
    public bool MapHighPerformanceMode
    {
      get; set;
    }

    /// <summary>
    /// If true, use a faster image scaling algorithm.
    /// </summary>
    public bool MapFastScaling
    {
      get; set;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the size of the map.
    /// </summary>
    /// <param name="size">The map size.</param>
    public void InitSize( Size size )
    {
      this.prevMapSize = size;

      if( this.rectSrc.IsEmpty )  // required to get this.Center
      {
        this.rectSrc.Height = this.Height / this.zoomFactor;
        this.rectSrc.Width = this.Width / this.zoomFactor;
      }
    }

    /// <summary>
    /// Load a new background map image into memory for display.
    /// </summary>
    /// <param name="image">The Bitmap to load (will be destroyed).</param>
    public void LoadMap( Bitmap image )
    {
      if( this.hdcSrcBackground != IntPtr.Zero )
      {
        Gdi32.DeleteDC( this.hdcSrcBackground );
        this.hdcSrcBackground = IntPtr.Zero;
        Gdi32.DeleteObject( this.hBmpSrcBackground );
        this.hBmpSrcBackground = IntPtr.Zero;

        Gdi32.DeleteDC( this.hdcSrcOverlay );
        this.hdcSrcOverlay = IntPtr.Zero;
        Gdi32.DeleteObject( this.hBmpSrcOverlay );
        this.hBmpSrcOverlay = IntPtr.Zero;
      }


      this.hdcSrcBackground = Gdi32.CreateCompatibleDC( IntPtr.Zero );
      if( this.hdcSrcBackground == IntPtr.Zero )
        throw new ApplicationException( "CreateCompatibleDC() failed" );

      this.hBmpSrcBackground = image.GetHbitmap();
      if( this.hBmpSrcBackground == IntPtr.Zero )
        throw new ApplicationException( "GetHbitmap() failed" );

      IntPtr ret = Gdi32.SelectObject( this.hdcSrcBackground, this.hBmpSrcBackground );
      if( ret == IntPtr.Zero )
        throw new ApplicationException( "SelectObject() failed" );


      Size newMapSize = new Size( image.Width, image.Height );
      this.fullHeight = newMapSize.Height;
      this.fullWidth = newMapSize.Width;

      if( newMapSize != prevMapSize )  // recenter
      {
        PointF prevCenter = this.Center;  // must have already called InitSize()
        this.Center = new PointF( ( prevCenter.X / prevMapSize.Width  ) * newMapSize.Width,
                                  ( prevCenter.Y / prevMapSize.Height ) * newMapSize.Height );

        this.prevMapSize = newMapSize;
      }

      
      image.Dispose();
      image = null;

      GC.Collect();


      // update map zoom

      this.Zoom();
    }

    /// <summary>
    /// Unloads the current background and overlay images from memory.
    /// </summary>
    public void UnloadMap()
    {
      if( this.hdcSrcBackground != IntPtr.Zero )
      {
        Gdi32.DeleteDC( this.hdcSrcBackground );
        this.hdcSrcBackground = IntPtr.Zero;
      }
      if( this.hBmpSrcBackground != IntPtr.Zero )
      {
        Gdi32.DeleteObject( this.hBmpSrcBackground );
        this.hBmpSrcBackground = IntPtr.Zero;
      }

      if( this.hdcSrcOverlay != IntPtr.Zero )
      {
        Gdi32.DeleteDC( this.hdcSrcOverlay );
        this.hdcSrcOverlay = IntPtr.Zero;
      }
      if( this.hBmpSrcOverlay != IntPtr.Zero )
      {
        Gdi32.DeleteObject( this.hBmpSrcOverlay );
        this.hBmpSrcOverlay = IntPtr.Zero;
      }

      GC.Collect();
    }

    /// <summary>
    /// Redraw a new map overlay, replacing the previous one.
    /// </summary>
    /// <returns>The Graphics object to use.</returns>
    public Graphics BeginDrawNewOverlay()
    {
      if( this.hdcSrcBackground == IntPtr.Zero )
        return null;


      // delete partially drawn overlay (if cancelled)

      if( this.newOverlay != null )
      {
        this.newOverlay.Dispose();
        this.newOverlay = null;
      }


      // remove any old paths

      if( pathTags.Count > 0 )
      {
        pathTags.Clear();
        paths.Reset();
      }


      GC.Collect();


      // create overlay and return Graphics

      try
      {
        this.newOverlay = new Bitmap( fullWidth, fullHeight, PixelFormat.Format32bppPArgb );
      }
      catch( Exception ex )  // "Parameter is not valid" - if no memory
      {
        if( this.newOverlay != null )
        {
          this.newOverlay.Dispose();
          this.newOverlay = null;
        }

        throw new ApplicationException( "Allocate memory: " + ex.Message, ex );
      }

      return Graphics.FromImage( this.newOverlay );
    }

    /// <summary>
    /// Signals the new overlay in finished being drawn and ready to be used.
    /// </summary>
    public void EndDrawNewOverlay()
    {
      if( this.newOverlay == null ) return;


      // delete previous overlay

      if( this.hdcSrcOverlay != IntPtr.Zero )
      {
        Gdi32.DeleteDC( this.hdcSrcOverlay );
        this.hdcSrcOverlay = IntPtr.Zero;
        Gdi32.DeleteObject( this.hBmpSrcOverlay );
        this.hBmpSrcOverlay = IntPtr.Zero;
      }


      // convert managed Bitmap to unmanaged hBmp

      this.hdcSrcOverlay = Gdi32.CreateCompatibleDC( IntPtr.Zero );
      this.hBmpSrcOverlay = this.newOverlay.GetHbitmap( Color.FromArgb( 0, 0, 0, 0 ) );
      Gdi32.SelectObject( this.hdcSrcOverlay, this.hBmpSrcOverlay );

      this.newOverlay.Dispose();
      this.newOverlay = null;
    }

    /// <summary>
    /// Adds a region with the given key.
    /// </summary>
    /// <param name="pt">The region center (in map pixels).</param>
    /// <param name="radius">The region radius.</param>
    /// <param name="tag">A object to be returned via GetRegionTag().</param>
    public void AddRegionTag( Point pt, int radius, object tag )
    {
      if( pathTags.Count > 0 )
        paths.SetMarkers();

      // circle is too expensive
      // paths.AddEllipse( pt.X - radius, pt.Y - radius, radius * 2, radius * 2 );
      paths.AddRectangle( new Rectangle( pt.X - radius, pt.Y - radius, radius * 2, radius * 2 ) );
      
      pathTags.Add( tag );
    }

    /// <summary>
    /// Gets the key associated with the region at the given point.
    /// </summary>
    /// <param name="point">The Point to look for a region.</param>
    /// <returns>The object tag if point is within a region, otherwise null.</returns>
    public object GetRegionTag( Point point )
    {
      GraphicsPath pathTemp = new GraphicsPath();
      GraphicsPathIterator pathIterator = new GraphicsPathIterator( paths );
      pathIterator.Rewind();

      for( int i = 0; i < pathIterator.SubpathCount; i++ )
      {
        pathIterator.NextMarker( pathTemp );
        if( pathTemp.IsVisible( point ) )
          return pathTags[i];
      }

      return null;  // not found
    }

    /// <summary>
    /// Repaint the control.
    /// </summary>
    /// <param name="zoomcenter">The point on which to center when zooming.</param>
    private void Redraw( PointF zoomcenter )
    {
#if MAC
      return;
#endif
      // get dest handle

      Graphics g = this.CreateGraphics();
      IntPtr hdcDest = g.GetHdc();


      // draw

      Redraw( zoomcenter, hdcDest );


      // free dest handles

      g.ReleaseHdc( hdcDest );
      Gdi32.DeleteDC( hdcDest );
      hdcDest = IntPtr.Zero;

      g.Dispose();
      g = null;
    }

    /// <summary>
    /// Repaint the control.
    /// </summary>
    /// <param name="zoomcenter">The point on which to center when zooming.</param>
    /// <param name="hdcDest">The DC handle to paint on.</param>
    private void Redraw( PointF zoomcenter, IntPtr hdcDest )
    {
#if MAC
        return;
#endif
      // no map loaded

      if( this.hdcSrcBackground == IntPtr.Zero )
      {
        Graphics gr = Graphics.FromHdc( hdcDest );
        try
        {
          gr.Clear( Color.FromArgb( 60, 60, 60 ) );
        }
        catch { }  // known bug in GDI: http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=96873
        gr.Dispose();

        this.prevZoomFactor = this.zoomFactor;

        return;
      }


      // scale & letterbox

      this.Xout = false;
      this.Yout = false;

      if( this.Width > ( fullWidth * this.zoomFactor ) )
      {
        this.rectSrc.X = 0;
        this.rectSrc.Width = fullWidth;
        this.rectDest.X = ( this.Width - ( fullWidth * this.zoomFactor ) ) / 2F;
        this.rectDest.Width = fullWidth * this.zoomFactor;
        Gdi32.BitBlt( hdcDest, 0, 0, (int)Math.Round( this.rectDest.X ), this.Height, this.hdcSrcBackground, 0, 0, Gdi32.TernaryRasterOperations.BLACKNESS );
        Gdi32.BitBlt( hdcDest, (int)Math.Round( this.rectDest.Right ), 0, (int)Math.Round( this.rectDest.X ), this.Height, this.hdcSrcBackground, 0, 0, Gdi32.TernaryRasterOperations.BLACKNESS );
      }
      else
      {
        this.rectSrc.X += ( ( this.Width / this.prevZoomFactor ) - ( this.Width / this.zoomFactor ) ) / ( ( this.Width + 0.001F ) / zoomcenter.X );
        this.rectSrc.Width = this.Width / this.zoomFactor;
        this.rectDest.X = 0;
        this.rectDest.Width = this.Width;
      }

      if( this.Height > ( fullHeight * this.zoomFactor ) )
      {
        this.rectSrc.Y = 0;
        this.rectSrc.Height = fullHeight;
        this.rectDest.Y = ( this.Height - ( fullHeight * this.zoomFactor ) ) / 2F;
        this.rectDest.Height = fullHeight * this.zoomFactor;
        Gdi32.BitBlt( hdcDest, 0, 0, this.Width, (int)Math.Round( this.rectDest.Y ), this.hdcSrcBackground, 0, 0, Gdi32.TernaryRasterOperations.BLACKNESS );
        Gdi32.BitBlt( hdcDest, 0, (int)Math.Round( this.rectDest.Bottom ), this.Width, (int)Math.Round( this.rectDest.Y ), this.hdcSrcBackground, 0, 0, Gdi32.TernaryRasterOperations.BLACKNESS );
      }
      else
      {
        this.rectSrc.Y += ( ( this.Height / this.prevZoomFactor ) - ( this.Height / this.zoomFactor ) ) / ( ( this.Height + 0.001F ) / zoomcenter.Y );
        this.rectSrc.Height = this.Height / this.zoomFactor;
        this.rectDest.Y = 0;
        this.rectDest.Height = this.Height;
      }

      this.prevZoomFactor = this.zoomFactor;

      if( this.rectSrc.Right > fullWidth )
      {
        this.Xout = true;
        this.rectSrc.X = fullWidth - this.rectSrc.Width;
      }
      if( this.rectSrc.X < 0 )
      {
        this.Xout = true;
        this.rectSrc.X = 0;
      }
      if( this.rectSrc.Bottom > fullHeight )
      {
        this.Yout = true;
        this.rectSrc.Y = fullHeight - this.rectSrc.Height;
      }
      if( this.rectSrc.Y < 0 )
      {
        this.Yout = true;
        this.rectSrc.Y = 0;
      }


      // draw

      if( this.MapFastScaling )
        Gdi32.SetStretchBltMode( hdcDest, Gdi32.StretchMode.STRETCH_DELETESCANS );
      else
        Gdi32.SetStretchBltMode( hdcDest, Gdi32.StretchMode.STRETCH_HALFTONE );


      if( this.hdcSrcOverlay == IntPtr.Zero )
      {
        // no overlay, just draw resized

        Gdi32.StretchBlt( hdcDest,
                          (int)Math.Round( rectDest.X ),
                          (int)Math.Round( rectDest.Y ),
                          (int)Math.Round( rectDest.Width ),
                          (int)Math.Round( rectDest.Height ),
                          this.hdcSrcBackground,
                          (int)Math.Round( rectSrc.X ),
                          (int)Math.Round( rectSrc.Y ),
                          (int)Math.Round( rectSrc.Width ),
                          (int)Math.Round( rectSrc.Height ),
                          Gdi32.TernaryRasterOperations.SRCCOPY );
      }
      else
      {
        
        // overlay, create merged image and draw

        IntPtr hdcBuffer = Gdi32.CreateCompatibleDC( IntPtr.Zero );
        //Gdi32.SetStretchBltMode( hdcBuffer, Gdi32.StretchMode.STRETCH_HALFTONE );
        IntPtr hBmpBuffer = Gdi32.CreateCompatibleBitmap( hdcSrcBackground, (int)Math.Round( rectSrc.Width ), (int)Math.Round( rectSrc.Height ) );
        Gdi32.SelectObject( hdcBuffer, hBmpBuffer );


        // copy background source rect into buffer

        Gdi32.BitBlt( hdcBuffer,
                      0,
                      0,
                      (int)Math.Round( rectSrc.Width ),
                      (int)Math.Round( rectSrc.Height ),
                      hdcSrcBackground,
                      (int)Math.Round( rectSrc.X ),
                      (int)Math.Round( rectSrc.Y ),
                      Gdi32.TernaryRasterOperations.SRCCOPY );


        // alpha blend overlay source rect into buffer

        Gdi32.AlphaBlend( hdcBuffer,
                          0,
                          0,
                          (int)Math.Round( rectSrc.Width ),
                          (int)Math.Round( rectSrc.Height ),
                          hdcSrcOverlay,
                          (int)Math.Round( rectSrc.X ),
                          (int)Math.Round( rectSrc.Y ),
                          (int)Math.Round( rectSrc.Width ),
                          (int)Math.Round( rectSrc.Height ),
                          new Gdi32.BLENDFUNCTION( Gdi32.AC_SRC_OVER, 0, 0xff, Gdi32.AC_SRC_ALPHA ) );


        // draw buffer to destination resized

        Gdi32.StretchBlt( hdcDest,
                          (int)Math.Round( rectDest.X ),
                          (int)Math.Round( rectDest.Y ),
                          (int)Math.Round( rectDest.Width ),
                          (int)Math.Round( rectDest.Height ),
                          hdcBuffer,
                          0, 0,
                          (int)Math.Round( rectSrc.Width ),
                          (int)Math.Round( rectSrc.Height ),
                          Gdi32.TernaryRasterOperations.SRCCOPY );
        
        // clean up buffer

        Gdi32.DeleteDC( hdcBuffer );
        hdcBuffer = IntPtr.Zero;
        Gdi32.DeleteObject( hBmpBuffer );
        hBmpBuffer = IntPtr.Zero;
      }

    }

    /// <summary>
    /// Converts mouse position to map coordinates.
    /// </summary>
    /// <param name="mouse">The mouse x/y position, in pixels from control top-left.</param>
    /// <returns>The map coordinates, in pixels.</returns>
    public Point MouseToMapCoords( Point mouse )
    {
      return new Point( (int)( ( ( mouse.X - rectDest.X ) / zoomFactor ) + rectSrc.X ),
                        (int)( ( ( mouse.Y - rectDest.Y ) / zoomFactor ) + rectSrc.Y ) );
    }

    /// <summary>
    /// Given a point that you would like to be centered, returns the actual resulting center point
    /// taking into account the map bounds and zoom level. The two will only differ when near the map
    /// edge.
    /// </summary>
    /// <param name="orig">The Point to be centered.</param>
    /// <param name="atMaxZoom">Calculate bounds at max zoom rather than current zoom level.</param>
    /// <returns>The resulting center point.</returns>
    public PointF GetBoundedCenterPoint( PointF orig, bool atMaxZoom )
    {
      RectangleF rect = rectSrc;

      if( atMaxZoom )
      {
        rect.Width = this.Width / this.maxZoomFactor;
        rect.Height = this.Height / this.maxZoomFactor;
      }

      rect.X = orig.X - ( rect.Width / 2 );
      rect.Y = orig.Y - ( rect.Height / 2 );

      if( rect.Right > fullWidth )
        rect.X = fullWidth - rect.Width;
      if( rect.X < 0 )
        rect.X = 0;
      if( rect.Bottom > fullHeight )
        rect.Y = fullHeight - rect.Height;
      if( rect.Y < 0 )
        rect.Y = 0;

      return new PointF( rect.X + ( rect.Width / 2 ), rect.Y + ( rect.Height / 2 ) );
    }

    /// <summary>
    /// Perform a smooth scroll from the current location to the given point.
    /// </summary>
    /// <param name="point">The destination center Point.</param>
    /// <param name="zoomin">Also zoom in to max zoom level.</param>
    public void ScrollToPoint( Point point, bool zoomin )
    {
      tmrMomentum.Stop();

      this.scrollIndex = 0;

      this.scrollFrom = this.Center;
      this.scrollTo = GetBoundedCenterPoint( point, zoomin );

      if( zoomin && this.zoomFactor != this.maxZoomFactor )
      {
        this.zoomFrom = this.zoomFactor;
        this.zoomTo = this.maxZoomFactor;
      }
      else
      {
        this.zoomFrom = this.zoomTo = -1;
      }

      if( this.MapHighPerformanceMode )
        this.MapFastScaling = true;

      tmrScroll.Start();
    }

    /// <summary>
    /// Zoom the map in/out around its current center point.
    /// </summary>
    /// <param name="direction">Positive to zoom in, negative to zoom out, or 0 to update min zoom.</param>
    public void Zoom( int direction = 0 )
    {
      this.Zoom( direction, this.Width / 2F, this.Height / 2F );
    }

    /// <summary>
    /// Zoom the map in/out around an x,y point.
    /// </summary>
    /// <param name="direction">Positive to zoom in, negative to zoom out, or 0 to update min zoom.</param>
    /// <param name="x">The x coord to zoom around.</param>
    /// <param name="y">The y coord to zoom around.</param>
    private void Zoom( int direction, float x, float y )
    {
      float newZoomFactor = this.zoomFactor;

      if( direction > 0 )
      {
        newZoomFactor *= 1.1F;
        if( newZoomFactor > this.MaxZoom )
          newZoomFactor = this.MaxZoom;
      }
      else if( direction < 0 )
      {
        newZoomFactor /= 1.1F;
        if( newZoomFactor < this.MinZoom )
          newZoomFactor = this.MinZoom;
      }
      else
      {
        if( newZoomFactor < this.MinZoom )
          newZoomFactor = this.MinZoom;
      }

      if( newZoomFactor == this.zoomFactor )
        return;

      if( this.MapHighPerformanceMode && direction != 0 )
      {
        this.MapFastScaling = true;
        tmrZoomFastScaling.Stop();
        tmrZoomFastScaling.Start();  // turn off when finished zooming
      }

      this.prevZoomFactor = this.zoomFactor;
      this.zoomFactor = newZoomFactor;
      this.Redraw( new PointF( x, y ) );

      if( this.MapZoomChanged != null )
        this.MapZoomChanged( this.zoomFactor );
    }

    /// <summary>
    /// Unload the map before disposing.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      UnloadMap();
      base.Dispose( disposing );
    }

    /// <summary>
    /// Ignore the WM_ERASEBKGND message.
    /// </summary>
    /// <param name="m"></param>
    protected override void WndProc( ref Message m )
    {
      if( m.Msg == WM_ERASEBKGND )  // ignore
        return;

      base.WndProc( ref m );
    }

    #endregion

    #region Event Handlers

    // Redraw() on user paint
    private void MapViewer_Paint( object sender, PaintEventArgs e )
    {
      IntPtr hdcDest = e.Graphics.GetHdc();
      this.Redraw( new PointF(), hdcDest );
      e.Graphics.ReleaseHdc( hdcDest );
      e.Graphics.Dispose();
    }

    // click, double-click, click-drag procesing
    private void MapViewer_MouseDown( object sender, MouseEventArgs e )
    {
      if( this.hdcSrcBackground == IntPtr.Zero )
        return;

      tmrScroll.Stop();
      tmrMomentum.Stop();

      this.P.X = e.X;
      this.P.Y = e.Y;
      this.CP.X = 0f;
      this.CP.Y = 0f;
      this.CS.X = e.X;
      this.CS.Y = e.Y;

      this.mouseDown = true;
      this.prevMouseLocation = this.currMouseLocation = e.Location;
    }
    private void MapViewer_MouseMove( object sender, MouseEventArgs e )
    {
      if( this.hdcSrcBackground == IntPtr.Zero )
        return;

      if( this.mouseDown )
      {
        if( this.MapHighPerformanceMode )
          this.MapFastScaling = true;

        this.Cursor = Cursors.NoMove2D;

        if( e.Button == MouseButtons.Right )
        {
          this.CP.X = (float)( ( this.P.X - e.X ) * ( (double)fullWidth  / 1000 ) );
          this.CP.Y = (float)( ( this.P.Y - e.Y ) * ( (double)fullHeight / 1000 ) );
        }

        this.rectSrc.X = ( ( ( this.P.X - e.X ) / this.zoomFactor ) + this.rectSrc.X ) + this.CP.X;
        this.rectSrc.Y = ( ( ( this.P.Y - e.Y ) / this.zoomFactor ) + this.rectSrc.Y ) + this.CP.Y;

        this.Redraw( new PointF() );

        if( !this.Xout )
          this.P.X = e.X;
        if( !this.Yout )
          this.P.Y = e.Y;

        this.prevMouseLocation = this.currMouseLocation;
        this.currMouseLocation = e.Location;
      }
      else if( e.Location != this.prevMouseLocation && this.MapMouseMove != null )  // raise mouse move event
      {
        Point loc = MouseToMapCoords( e.Location );
        this.MapMouseMove( this, new MouseEventArgs( MouseButtons.None, 0, loc.X, loc.Y, 0 ) );
      }
    }
    private void MapViewer_MouseClick( object sender, MouseEventArgs e )
    {
      if( this.hdcSrcBackground == IntPtr.Zero )
        return;

      if( this.CS.X != e.X || this.CS.Y != e.Y )  // mouse moved
        return;

      if( this.MapMouseClick != null )
      {
        Point ptMouse = MouseToMapCoords( e.Location );
        this.MapMouseClick( this, new MouseEventArgs( e.Button, e.Clicks, ptMouse.X, ptMouse.Y, 0 ) );
      }
    }

    private bool skipNextMouseUp = false;
    private void MapViewer_MouseDoubleClick( object sender, MouseEventArgs e )
    {
      if( this.hdcSrcBackground == IntPtr.Zero )
        return;

      if( this.MapMouseDoubleClick != null )
      {
        Point ptMouse = MouseToMapCoords( e.Location );
        this.MapMouseDoubleClick( this, new MouseEventArgs( e.Button, e.Clicks, ptMouse.X, ptMouse.Y, 0 ) );
      }
      this.skipNextMouseUp = true;
    }
    private void MapViewer_MouseUp( object sender, MouseEventArgs e )
    {
      this.mouseDown = false;

      if( skipNextMouseUp )
      {
        this.skipNextMouseUp = false;
        return;
      }

      if( this.hdcSrcBackground == IntPtr.Zero )
        return;

      if( this.MapHighPerformanceMode && this.MapFastScaling )
      {
        this.MapFastScaling = false;
        this.Refresh();
      }

      if( !this.MapHighPerformanceMode && (this.CS.X != e.X || this.CS.Y != e.Y) )  // mouse moved, do momentum
      {
        this.momentumSpeed = Misc.DistanceBetween( this.prevMouseLocation, e.Location );

        if( this.momentumSpeed > 1 )
        {
          this.momuntumAngle = Misc.AngleBetween( this.prevMouseLocation, e.Location );
          tmrMomentum.Start();
        }
      }
    }

    // zooming via mousewheel or +/- keys
    private void MapViewer_MouseWheel( object sender, MouseEventArgs e )
    {
      this.Zoom( e.Delta, e.X, e.Y );
    }
    private void MapViewer_KeyDown( object sender, KeyEventArgs e )
    {
      int direction;

      switch( e.KeyValue )
      {
        case 187:  // +
        case 107:  // numpad +
          direction = 1;
          break;

        case 189:  // -
        case 109:  // numpad -
          direction = -1;
          break;

        default:
          return;
      }

      this.Zoom( direction );
    }

    // play last momentum after mouse release
    private void tmrMomentum_Tick( object sender, EventArgs e )
    {
      PointF prevCenter = this.Center;
      this.Center = Misc.AngleOffset( this.Center, momuntumAngle, momentumSpeed );

      if( this.Center == prevCenter )  // at edge (corner), can't move any further
        tmrMomentum.Stop();

      momentumSpeed -= 1;

      if( momentumSpeed < 1 )
        tmrMomentum.Stop();
    }

    // update position during autoscroll
    private void tmrScroll_Tick( object sender, EventArgs e )
    {
      // failsafe

      if( scrollIndex > 22 )
      {
        tmrScroll.Stop();

        if( this.MapScrollToPointCompleted != null )
          this.MapScrollToPointCompleted( this, null );

        if( this.MapHighPerformanceMode )
        {
          this.MapFastScaling = false;
          this.Refresh();
        }

        return;
      }


      // set new zoom factor

      if( this.zoomTo != -1 )
        this.zoomFactor = ( ( this.zoomTo - this.zoomFrom ) * this.beizerCurve[scrollIndex] ) + this.zoomFrom;


      // redraw new center point

      this.Center = Misc.MidPoint( this.scrollFrom, this.scrollTo, this.beizerCurve[scrollIndex] );


      // update index

      if( scrollIndex == 22 )
      {
        tmrScroll.Stop();

        if( this.MapScrollToPointCompleted != null )
          this.MapScrollToPointCompleted( this, null );

        if( this.MapHighPerformanceMode )
        {
          this.MapFastScaling = false;
          this.Refresh();
        }
      }
      else
      {
        scrollIndex++;
      }
    }

    // turn off map fast scaling
    private void tmrZoomFastScaling_Tick( object sender, EventArgs e )
    {
      this.tmrZoomFastScaling.Stop();
      this.MapFastScaling = false;
      this.Refresh();
    }

    #endregion
  }
}
