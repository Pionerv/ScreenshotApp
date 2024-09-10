using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ScreenshotApp
{
    public class CaptureForm : Form
    {
        private static CaptureForm instance;

        private Point startPoint;
        private Rectangle selectionRectangle;
        private Bitmap screenBitmap;
        private readonly Screen targetScreen;

        private CaptureForm()
        {
            targetScreen = GetTargetScreen();

            InitializeFormForScreen(targetScreen);

            CaptureScreenFromMonitor(targetScreen);

            this.MouseDown += new MouseEventHandler(OnMouseDown);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseUp += new MouseEventHandler(OnMouseUp);
            this.Paint += new PaintEventHandler(OnPaint);
            this.KeyDown += new KeyEventHandler(OnKeyDown);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.KeyPreview = true;
        }

        public static CaptureForm GetInstance()
        {
            instance ??= new CaptureForm();
            return instance;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            instance = null;
        }

        private static Screen GetTargetScreen()
        {
            Point cursorPos = Cursor.Position;
            return Screen.AllScreens.FirstOrDefault(screen => screen.Bounds.Contains(cursorPos))
                   ?? Screen.PrimaryScreen;
        }

        private void InitializeFormForScreen(Screen screen)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = screen.Bounds.Location;
            this.Size = screen.Bounds.Size;
            this.TopMost = false;
            this.Opacity = 1;
        }

        private void CaptureScreenFromMonitor(Screen screen)
        {
            screenBitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, PixelFormat.Format24bppRgb);
            using Graphics g = Graphics.FromImage(screenBitmap);
            g.CopyFromScreen(screen.Bounds.Location, Point.Empty, screenBitmap.Size);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
                this.Invalidate();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x1 = Math.Min(startPoint.X, e.X);
                int y1 = Math.Min(startPoint.Y, e.Y);
                int x2 = Math.Max(startPoint.X, e.X);
                int y2 = Math.Max(startPoint.Y, e.Y);

                selectionRectangle = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                this.Invalidate();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (selectionRectangle.Width > 0 && selectionRectangle.Height > 0)
            {
                this.Hide();

                CaptureSelection();
                this.Close();
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (screenBitmap != null)
            {
                e.Graphics.DrawImage(screenBitmap, Point.Empty);

                if (selectionRectangle != Rectangle.Empty)
                {
                    using (var pen = new Pen(Color.Red, 2))
                    {
                        e.Graphics.DrawRectangle(pen, selectionRectangle);
                    }

                    using var brush = new SolidBrush(Color.FromArgb(0, Color.Blue));
                    e.Graphics.FillRectangle(brush, selectionRectangle);
                }
            }
        }

        private void CaptureSelection()
        {
            var bmp = new Bitmap(selectionRectangle.Width, selectionRectangle.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(screenBitmap, new Rectangle(0, 0, bmp.Width, bmp.Height),
                            selectionRectangle, GraphicsUnit.Pixel);
            }

            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(exeDirectory, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            Clipboard.SetImage(bmp);
            bmp.Save(filePath, ImageFormat.Png);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}