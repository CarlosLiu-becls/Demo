using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImageBinarization
{
  public partial class Form1 : Form
  {
    // This is a file name string.
    private string curFileName;
    // This is a bitmap object.
    private System.Drawing.Bitmap curBitmap;

    public Form1()
    {
      InitializeComponent();
      trackBar1.Minimum = 0;
      trackBar1.Maximum = 255;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      // Establish a open file dialog.
      OpenFileDialog openDlg = new OpenFileDialog();
      // Set a filter to arrange picture format.
      openDlg.Filter = "All format | *.bmp; *.pcx; *.png; *.jpg; *.gif;" +
          "*.tif; *.ico; *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf";
      // Set the title of the open file dialog.
      openDlg.Title = "Open a picture file.";
      // if open file is successful.
      if (openDlg.ShowDialog() == DialogResult.OK)
      {
        // Get the open file name to current file name variable.
        curFileName = openDlg.FileName;
        try
        {
          // Establish a bitmap from file through by Image object.
          curBitmap = (Bitmap)Image.FromFile(curFileName);
        }
        // If we get a exception.
        catch (Exception exp)
        {
          // Pop to a message.
          MessageBox.Show(exp.Message);
        }
      }
      // Force to redraw.
      Invalidate();
    }

    private void Form1_Paint(object sender, PaintEventArgs e)
    {
      // Establish a graphics object from external object
      Graphics g = e.Graphics;
      // if it is to open file successful.
      if (curBitmap != null)
      {
        // Draw the picture in the position that we set up.
        g.DrawImage(curBitmap, 140, 10, curBitmap.Width, curBitmap.Height);
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      var threshold = trackBar1.Value;

      // if it is to open file successful.
      if (curBitmap != null)
      {
        // Establish a color object.
        Color curColor;
        int ret;
        // The width of the image.
        for (int iX = 0; iX < curBitmap.Width; iX++)
        {
          // The height of the image.
          for (int iY = 0; iY < curBitmap.Height; iY++)
          {
            // Get the pixel from bitmap object.
            curColor = curBitmap.GetPixel(iX, iY);
            // Transform RGB to Y (gray scale)
            ret = (int)(curColor.R * 0.299 + curColor.G * 0.578 + curColor.B * 0.114);
            // This is our threshold, you can change it and to try what are different.
            if (ret > threshold)
            {
              ret = 255;
            }
            else
            {
              ret = 0;
            }
            // Set the pixel into the bitmap object.
            curBitmap.SetPixel(iX, iY, Color.FromArgb(ret, ret, ret));
          } // The closing 'The height of the image'.
        } // The closing 'The width of the image'.
          // Force to redraw.
        Invalidate();
      }
    }

    private void trackBar1_ValueChanged(object sender, EventArgs e)
    {
      label1.Text = trackBar1.Value.ToString();
    }
  }
}
