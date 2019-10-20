﻿using Syncfusion.Windows.Forms.Grid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;


// TODO Ctrl +v +c +z
// Knitting
// pattern to excel
// test excel
// dragable
// fix nicer gui placeing and size
// fix nicer gui colors

namespace PatternsMaker
{
    public struct Cell
    {
        public int x, y;
        public Color? color;
        public Bitmap symbol;

        public Cell(int x, int y, Color? color, Bitmap symbol)
        {

            this.x = x;
            this.y = y;
            this.color = color;
            this.symbol = symbol;
        }
    }

    public partial class Form1 : Form
    {
        char[] delitem = { ';' }; char[] delcol = { 'C' };
        char[] delrow = { 'R' };
        char[] delsep = { ':' };
        Bitmap loaded_image;
        int pixel_size = 100;
        int x = 50;
        int y = 50;
        int nr_colors = 2;
        int fill_tresh = 10000;


        List<Cell> tmp_saved = new List<Cell>();
        List<Cell> tmp_latest = new List<Cell>();

        Size cell_size = new Size(25, 25);
        string path_to_dmc = @"../../Data/dmc.txt";

        private ToolStripProgressBar toolStripProgressBar; // todo add this progressbar when necessary

        public Form1()
        {
            InitializeComponent();

            initiate_dmc_colorpicker();

            //To prevent the selection from particular range of cells
            this.gridControl1.SelectionChanging += gridControl1_SelectionChanging;

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            toolStripProgressBar = new ToolStripProgressBar();

            this.gridControl1.CommandStack.Enabled = true; // for ctrl-z function

            
        }

        private void add_border(List<Point> cells)
        {
            foreach ( Point p in cells)
            {
                gridControl1[p.Y,p.X].Borders.All = new GridBorder(GridBorderStyle.Solid, Color.Black, GridBorderWeight.Medium);
            }
        }

        private void add_border_all()
        {
            // add border to all cells

            foreach (int i in Enumerable.Range(0, gridControl1.ColStyles.Count))
            {
                gridControl1.ColStyles[i].Borders.All = new GridBorder(GridBorderStyle.Solid, Color.Black, GridBorderWeight.Medium);
            }
        }

        private List<Point> get_selected_cells()
        {
            List<Point> res = new List<Point>();
            var selected_cells = gridControl1.Selections.Ranges;
            foreach (var selected_cell_block in selected_cells.ToString().Split(delitem))
            {
                if (selected_cells.ToString().Length > 0)
                {
                    if (selected_cell_block.Contains(":"))
                    {
                        int min_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[1].Split(delsep)[0]);
                        int max_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[2].Split(delcol)[0]);

                        int min_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                        int max_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[2].Split(delcol)[0]);

                        for (int i = min_cols; i <= max_cols; i++)
                        {
                            for (int j = min_rows; j <= max_rows; j++)
                            {
                                res.Add(new Point(i, j));
                            }
                        }
                    }
                    else
                    {
                        int cols = int.Parse(selected_cell_block.ToString().Split(delcol)[1]);
                        int rows = int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                        res.Add(new Point(rows, cols));
                    }
                }
                else
                {
                    // get current active cell
                    if (gridControl1.CurrentCell.ColIndex > 0)
                    {
                        res.Add(new Point(gridControl1.CurrentCell.RowIndex, gridControl1.CurrentCell.ColIndex));
                    }
                }

            }
            return res;
        }


        private void do_changes(List<Cell> changes)
        {
            if (changes == null)
            {
                return;
            }

            // Preform changes to gridControl and remember last change
            List<Cell> old = new List<Cell>();

            List<Point> marked = get_selected_cells();



            foreach (Cell cell in changes)
            {
                old.Add(new Cell(cell.x, cell.y, gridControl1[cell.x, cell.y].BackColor, (Bitmap)gridControl1[cell.x, cell.y].BackgroundImage));
                Console.WriteLine(cell.x.ToString() + " " + cell.y.ToString() + " " + gridControl1[cell.x, cell.y].BackColor);

                Console.WriteLine(cell.x.ToString() + " " + cell.y.ToString() + " " + cell.color);

                //Console.WriteLine(cell.x.ToString() + " " + cell.y.ToString() + " "+cell.symbol.ToString());
                gridControl1[cell.x, cell.y].BackgroundImage = cell.symbol; // something wrong with this type!
                gridControl1[cell.x, cell.y].BackColor = (Color)cell.color;
            }
            tmp_latest = old;

        }

        private void initiate_dmc_colorpicker()
        {
            int counter = 0;
            string line;

            char[] del = { '	' };

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(path_to_dmc);
            while ((line = file.ReadLine()) != null)
            {
                if (counter > 0)
                {
                    string[] splitted = line.Split(del);
                    listView3.Items.Add(line);
                    listView3.Items[listView3.Items.Count - 1].BackColor = ColorTranslator.FromHtml("0x" + splitted[splitted.Length - 2]);
                }
                counter++;
            }

            file.Close();

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Save pattern gridControl1

            // open save dialog

            // Save to xml
            gridControl1.FileName = "test.xml";
            gridControl1.SaveXml();

            // Save to Excel


        }

        private void showimage(Bitmap img)
        {
            // show image in new form
            using (Form form = new Form())
            {

                form.StartPosition = FormStartPosition.CenterScreen;
                form.Size = img.Size;

                PictureBox pb = new PictureBox();
                pb.Dock = DockStyle.Fill;
                pb.Image = img;

                form.Controls.Add(pb);
                form.ShowDialog();
            }
        }

        public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return bmp1 == bmp2;
            if (object.Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // add selected image to each selected cell

            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0]; // item to set
                Bitmap img = (Bitmap)item.ImageList.Images[item.ImageIndex];

                img = new Bitmap(img, cell_size);
                pictureBox2.Image = img;
                img.MakeTransparent();

                if (FillCheckBox.Checked)
                {
                    //item to replace
                    Bitmap img_to_replace = (Bitmap)gridControl1[gridControl1.CurrentCell.RowIndex, gridControl1.CurrentCell.ColIndex].BackgroundImage;
                    List<Point> fill_points = new List<Point>();
                    bool all_found = false;
                    Point current = new Point(gridControl1.CurrentCell.RowIndex, gridControl1.CurrentCell.ColIndex);
                    fill_points.Add(current);
                    int i = 0;


                    while (!all_found)
                    {
                        if (i >= fill_points.Count || i > fill_tresh)
                        {
                            all_found = true;
                            continue;
                        }

                        current = fill_points[i];
                        if (current.X > 0)
                        {
                            if (!fill_points.Contains(new Point(current.X - 1, current.Y)))
                            {
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X - 1, current.Y].BackgroundImage, img_to_replace))
                                {
                                    fill_points.Add(new Point(current.X - 1, current.Y));
                                }
                            }
                        }

                        if (current.X > 0 && current.Y > 0)
                            if (!fill_points.Contains(new Point(current.X - 1, current.Y - 1)))
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X - 1, current.Y - 1].BackgroundImage, img_to_replace))
                                {

                                    fill_points.Add(new Point(current.X - 1, current.Y - 1));
                                }

                        if (current.Y > 0)
                            if (!fill_points.Contains(new Point(current.X, current.Y - 1)))
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X, current.Y - 1].BackgroundImage, img_to_replace))
                                {

                                    fill_points.Add(new Point(current.X, current.Y - 1));
                                }
                        if (current.Y > 0 && current.X < gridControl1.RowCount)
                            if (!fill_points.Contains(new Point(current.X + 1, current.Y - 1)))
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X + 1, current.Y - 1].BackgroundImage, img_to_replace))
                                {

                                    fill_points.Add(new Point(current.X + 1, current.Y - 1));
                                }
                        if (current.X < gridControl1.RowCount)
                            if (!fill_points.Contains(new Point(current.X + 1, current.Y)))
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X + 1, current.Y].BackgroundImage, img_to_replace))
                                {

                                    fill_points.Add(new Point(current.X + 1, current.Y));
                                }
                        if (current.X < gridControl1.RowCount && current.Y < gridControl1.ColCount)
                            if (!fill_points.Contains(new Point(current.X + 1, current.Y + 1)))
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X + 1, current.Y + 1].BackgroundImage, img_to_replace))
                                {

                                    fill_points.Add(new Point(current.X + 1, current.Y + 1));
                                }
                        if (current.Y < gridControl1.ColCount)
                            if (!fill_points.Contains(new Point(current.X, current.Y + 1)))
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X, current.Y + 1].BackgroundImage, img_to_replace))
                                {

                                    fill_points.Add(new Point(current.X, current.Y + 1));
                                }
                        if (current.X > 0 && current.Y < gridControl1.ColCount)
                            if (!fill_points.Contains(new Point(current.X - 1, current.Y + 1)))
                                if (CompareBitmapsFast((Bitmap)gridControl1[current.X - 1, current.Y + 1].BackgroundImage, img_to_replace))
                                {

                                    fill_points.Add(new Point(current.X - 1, current.Y + 1));
                                }

                        i++;
                    }


                    foreach (Point p in fill_points)
                    {
                        gridControl1[p.X, p.Y].BackgroundImage = img;
                    }

                }
                else
                {
                    var selected_cells = gridControl1.Selections.Ranges;
                    foreach (var selected_cell_block in selected_cells.ToString().Split(delitem))
                    {
                        if (selected_cells.ToString().Length > 0)
                        {
                            if (selected_cell_block.Contains(":"))
                            {
                                int min_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[1].Split(delsep)[0]);
                                int max_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[2].Split(delcol)[0]);

                                int min_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                                int max_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[2].Split(delcol)[0]);

                                for (int i = min_cols; i <= max_cols; i++)
                                {
                                    for (int j = min_rows; j <= max_rows; j++)
                                    {

                                        gridControl1[j, i].BackgroundImage = img;
                                    }
                                }
                            }
                            else
                            {
                                int cols = int.Parse(selected_cell_block.ToString().Split(delcol)[1]);
                                int rows = int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                                gridControl1[rows, cols].BackgroundImage = img;
                            }
                        }
                        else
                        {
                            // get current active cell
                            if (gridControl1.CurrentCell.ColIndex > 0)
                            {

                                gridControl1[gridControl1.CurrentCell.RowIndex, gridControl1.CurrentCell.ColIndex].BackgroundImage = img;
                            }
                        }
                    }
                }
            }
        }
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        void gridControl1_SelectionChanging(object sender, GridSelectionChangingEventArgs e)
        {
            // Update selected Size
            var selected_cells = gridControl1.Selections.Ranges;
            foreach (var selected_cell_block in selected_cells.ToString().Split(delitem))
            {
                if (selected_cells.ToString().Length > 0)
                {
                    if (selected_cell_block.Contains(":"))
                    {
                        int min_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[1].Split(delsep)[0]);
                        int max_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[2].Split(delcol)[0]);

                        int cols = max_cols - min_cols + 1;

                        int min_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                        int max_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[2].Split(delcol)[0]);

                        int rows = max_rows - min_rows + 1;

                        SelectedSize.Text = "x = " + cols + ", y = " + rows;
                    }
                    else
                    {
                        // todo fix when a row or columns is selected!
                        try
                        {
                            int cols = 1 + int.Parse(selected_cell_block.ToString().Split(delcol)[1]);
                            int rows = 1 + int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                            SelectedSize.Text = "x = " + cols + ", y = " + rows;
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("WARNING: TODO fix when row or column is selected!");
                        }
                    }

                }
                else
                {
                    SelectedSize.Text = "x = " + 1 + ", y = " + 1;
                }
            }
        }

        private void gridControl1_CellClick(object sender, Syncfusion.Windows.Forms.Grid.GridCellClickEventArgs e)
        {
            label13.Text = "x = " +e.ColIndex +", y = " + e.RowIndex;
        }

        private void colorUIControl1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // set selected color
            // add selected image to each selected cell

            if (listView3.SelectedItems.Count > 0)
            {
                var item = listView3.SelectedItems[0];
                Color color = item.BackColor;
                pictureBox3.BackColor = color;


                if (FillCheckBox.Checked)
                {
                    //item to replace
                    Color color_to_replace = gridControl1[gridControl1.CurrentCell.RowIndex, gridControl1.CurrentCell.ColIndex].BackColor;
                    List<Point> fill_points = new List<Point>();
                    bool all_found = false;
                    Point current = new Point(gridControl1.CurrentCell.RowIndex, gridControl1.CurrentCell.ColIndex);
                    fill_points.Add(current);
                    int i = 0;

                    while (!all_found)
                    {
                        if (i >= fill_points.Count || i > fill_tresh)
                        {
                            all_found = true;
                            continue;
                        }
                        current = fill_points[i];
                        if (current.X > 0)
                        {
                            if (!fill_points.Contains(new Point(current.X - 1, current.Y)))
                            {
                                if (gridControl1[current.X - 1, current.Y].BackColor == color_to_replace)
                                {
                                    fill_points.Add(new Point(current.X - 1, current.Y));
                                }
                            }
                        }

                        if (current.X > 0 && current.Y > 0)
                            if (!fill_points.Contains(new Point(current.X - 1, current.Y - 1)))
                                if (gridControl1[current.X - 1, current.Y - 1].BackColor == color_to_replace)
                                {

                                    fill_points.Add(new Point(current.X - 1, current.Y - 1));
                                }

                        if (current.Y > 0)
                            if (!fill_points.Contains(new Point(current.X, current.Y - 1)))
                                if (gridControl1[current.X, current.Y - 1].BackColor == color_to_replace)
                                {

                                    fill_points.Add(new Point(current.X, current.Y - 1));
                                }
                        if (current.Y > 0 && current.X < gridControl1.RowCount)
                            if (!fill_points.Contains(new Point(current.X + 1, current.Y - 1)))
                                if (gridControl1[current.X + 1, current.Y - 1].BackColor == color_to_replace)
                                {

                                    fill_points.Add(new Point(current.X + 1, current.Y - 1));
                                }
                        if (current.X < gridControl1.RowCount)
                            if (!fill_points.Contains(new Point(current.X + 1, current.Y)))
                                if (gridControl1[current.X + 1, current.Y].BackColor == color_to_replace)
                                {

                                    fill_points.Add(new Point(current.X + 1, current.Y));
                                }
                        if (current.X < gridControl1.RowCount && current.Y < gridControl1.ColCount)
                            if (!fill_points.Contains(new Point(current.X + 1, current.Y + 1)))
                                if (gridControl1[current.X + 1, current.Y + 1].BackColor == color_to_replace)
                                {

                                    fill_points.Add(new Point(current.X + 1, current.Y + 1));
                                }
                        if (current.Y < gridControl1.ColCount)
                            if (!fill_points.Contains(new Point(current.X, current.Y + 1)))
                                if (gridControl1[current.X, current.Y + 1].BackColor == color_to_replace)
                                {

                                    fill_points.Add(new Point(current.X, current.Y + 1));
                                }
                        if (current.X > 0 && current.Y < gridControl1.ColCount)
                            if (!fill_points.Contains(new Point(current.X - 1, current.Y + 1)))
                                if (gridControl1[current.X - 1, current.Y + 1].BackColor == color_to_replace)
                                {

                                    fill_points.Add(new Point(current.X - 1, current.Y + 1));
                                }

                        i++;
                    }


                    foreach (Point p in fill_points)
                    {
                        gridControl1[p.X, p.Y].BackColor = color;
                    }

                }
                else
                {
                    var selected_cells = gridControl1.Selections.Ranges;
                    char[] delitem = { ';' };
                    foreach (var selected_cell_block in selected_cells.ToString().Split(delitem))
                    {
                        if (selected_cells.ToString().Length > 0)
                        {
                            if (selected_cell_block.Contains(":"))
                            {

                                int min_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[1].Split(delsep)[0]);
                                int max_cols = int.Parse(selected_cell_block.ToString().Split(delcol)[2].Split(delcol)[0]);

                                int min_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                                int max_rows = int.Parse(selected_cell_block.ToString().Split(delrow)[2].Split(delcol)[0]);


                                for (int i = min_cols; i <= max_cols; i++)
                                {
                                    for (int j = min_rows; j <= max_rows; j++)
                                    {
                                        gridControl1[j, i].BackColor = color;
                                    }
                                }
                            }
                            else
                            {
                                int cols = int.Parse(selected_cell_block.ToString().Split(delcol)[1]);
                                int rows = int.Parse(selected_cell_block.ToString().Split(delrow)[1].Split(delcol)[0]);
                                gridControl1[rows, cols].BackColor = color;

                            }
                        }
                        else
                        {
                            if (gridControl1.CurrentCell.ColIndex > 0)
                            {
                                gridControl1[gridControl1.CurrentCell.RowIndex, gridControl1.CurrentCell.ColIndex].BackColor = color;

                            }
                        }
                    }
                }
            }
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void SelectedSize_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            // load our selected image
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string loaded_image_path = openFileDialog1.FileName;

                using (Stream bmpStream = System.IO.File.Open(loaded_image_path, System.IO.FileMode.Open))
                {
                    Image image = Image.FromStream(bmpStream);

                    loaded_image = new Bitmap(image);

                    pictureBox1.Image = loaded_image;
                }
                // loaded_image
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // TODO add x and y

            // generate several pixelized images and show results
            listView2.Items.Clear();
            imageList2.Images.Clear();

            if (loaded_image != null)
            {

                // pixelize 1
                Bitmap image1 = Pixelate(loaded_image, new Rectangle(0, 0, loaded_image.Width, loaded_image.Height), pixel_size);

                string key = listView2.Items.Count.ToString();

                imageList2.ImageSize = new Size(200, 200);

                imageList2.Images.Add(key, image1);

                // add an item
                var listViewItem = listView2.Items.Add("");
                // and tell the item which image to use
                listViewItem.ImageKey = key;

                // palett color 1 
                Bitmap image2 = loaded_image;

                key = listView2.Items.Count.ToString();

                // todo fix with colors here!
                Color[] colors = new Color[] { Color.Black, Color.White };
                Bitmap newImage = ConvertToColors(image2, colors);
                ColorPalette pal = newImage.Palette;
                pal.Entries[0] = Color.Blue;
                pal.Entries[1] = Color.Yellow;
                newImage.Palette = pal;

                imageList2.ImageSize = new Size(200, 200);
                imageList2.Images.Add(key, newImage);

                // add an item
                listViewItem = listView2.Items.Add(" ");
                // and tell the item which image to use
                listViewItem.ImageKey = key;

                // combine first two 1
                Bitmap image3 = image1;
                key = listView2.Items.Count.ToString();

                colors = new Color[] { Color.Black, Color.White };
                newImage = ConvertToColors(image3, colors);
                pal = newImage.Palette;
                pal.Entries[0] = Color.Blue;
                pal.Entries[1] = Color.Yellow;
                newImage.Palette = pal;

                imageList2.ImageSize = new Size(200, 200);
                imageList2.Images.Add(key, newImage);

                // add an item
                listViewItem = listView2.Items.Add(" ");
                // and tell the item which image to use
                listViewItem.ImageKey = key;
            }

        }

        public static Bitmap PaintOn32bpp(Image image, Color? transparencyFillColor)
        {
            Bitmap bp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(bp))
            {
                if (transparencyFillColor.HasValue)
                    using (System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(Color.FromArgb(255, transparencyFillColor.Value)))
                        gr.FillRectangle(myBrush, new Rectangle(0, 0, image.Width, image.Height));
                gr.DrawImage(image, new Rectangle(0, 0, bp.Width, bp.Height));
            }
            return bp;
        }
        public static Byte[] GetImageData(Bitmap sourceImage, out Int32 stride)
        {
            BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, sourceImage.PixelFormat);
            stride = sourceData.Stride;
            Byte[] data = new Byte[stride * sourceImage.Height];
            Marshal.Copy(sourceData.Scan0, data, 0, data.Length);
            sourceImage.UnlockBits(sourceData);
            return data;
        }
        public static Byte[] Convert32BitTo8Bit(Byte[] imageData, Int32 width, Int32 height, Color[] palette, ref Int32 stride)
        {
            if (stride < width * 4)
                throw new ArgumentException("Stride is smaller than one pixel line!", "stride");
            Byte[] newImageData = new Byte[width * height];
            for (Int32 y = 0; y < height; y++)
            {
                Int32 inputOffs = y * stride;
                Int32 outputOffs = y * width;
                for (Int32 x = 0; x < width; x++)
                {
                    // 32bppArgb: Order of the bytes is Alpha, Red, Green, Blue, but
                    // since this is actually in the full 4-byte value read from the offset,
                    // and this value is considered little-endian, they are actually in the
                    // order BGRA. Since we're converting to a palette we ignore the alpha
                    // one and just give RGB.
                    Color c = Color.FromArgb(imageData[inputOffs + 2], imageData[inputOffs + 1], imageData[inputOffs]);
                    // Match to palette index
                    newImageData[outputOffs] = (Byte)GetClosestPaletteIndexMatch(c, palette);
                    inputOffs += 4;
                    outputOffs++;
                }
            }
            stride = width;
            return newImageData;
        }

        public static Int32 GetClosestPaletteIndexMatch(Color col, Color[] colorPalette)
        {
            Int32 colorMatch = 0;
            Int32 leastDistance = Int32.MaxValue;
            Int32 red = col.R;
            Int32 green = col.G;
            Int32 blue = col.B;
            for (Int32 i = 0; i < colorPalette.Length; i++)
            {
                Color paletteColor = colorPalette[i];
                Int32 redDistance = paletteColor.R - red;
                Int32 greenDistance = paletteColor.G - green;
                Int32 blueDistance = paletteColor.B - blue;
                Int32 distance = (redDistance * redDistance) + (greenDistance * greenDistance) + (blueDistance * blueDistance);
                if (distance >= leastDistance)
                    continue;
                colorMatch = i;
                leastDistance = distance;
                if (distance == 0)
                    return i;
            }
            return colorMatch;
        }

        public static Bitmap ConvertToColors(Bitmap image, Color[] colors)
        {
            Int32 width = image.Width;
            Int32 height = image.Height;
            Int32 stride;
            Byte[] hiColData;
            // use "using" to properly dispose of temporary image object.
            using (Bitmap hiColImage = PaintOn32bpp(image, colors[0]))
                hiColData = GetImageData(hiColImage, out stride);
            Byte[] eightBitData = Convert32BitTo8Bit(hiColData, width, height, colors, ref stride);
            return BuildImage(eightBitData, width, height, stride, PixelFormat.Format8bppIndexed, colors, Color.Black);
        }

        public static Bitmap BuildImage(Byte[] sourceData, Int32 width, Int32 height, Int32 stride, PixelFormat pixelFormat, Color[] palette, Color? defaultColor)
        {
            Bitmap newImage = new Bitmap(width, height, pixelFormat);
            BitmapData targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
            Int32 newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;
            // Compensate for possible negative stride on BMP format.
            Boolean isFlipped = stride < 0;
            stride = Math.Abs(stride);
            // Cache these to avoid unnecessary getter calls.
            Int32 targetStride = targetData.Stride;
            Int64 scan0 = targetData.Scan0.ToInt64();
            for (Int32 y = 0; y < height; y++)
                Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
            newImage.UnlockBits(targetData);
            // Fix negative stride on BMP format.
            if (isFlipped)
                newImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            // For indexed images, set the palette.
            if ((pixelFormat & PixelFormat.Indexed) != 0 && palette != null)
            {
                ColorPalette pal = newImage.Palette;
                for (Int32 i = 0; i < pal.Entries.Length; i++)
                {
                    if (i < palette.Length)
                        pal.Entries[i] = palette[i];
                    else if (defaultColor.HasValue)
                        pal.Entries[i] = defaultColor.Value;
                    else
                        break;
                }
                newImage.Palette = pal;
            }
            return newImage;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                nr_colors = int.Parse(textBox1.Text);
            }
            catch (Exception)
            {
                // will pass this for now
            }
        }
        private static Bitmap Pixelate(Bitmap image, Rectangle rectangle, Int32 pixelateSize)
        {
            Bitmap pixelated = new System.Drawing.Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = System.Drawing.Graphics.FromImage(pixelated))
                graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            // look at every pixel in the rectangle while making sure we're within the image bounds
            for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width && xx < image.Width; xx += pixelateSize)
            {
                for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height && yy < image.Height; yy += pixelateSize)
                {
                    Int32 offsetX = pixelateSize / 2;
                    Int32 offsetY = pixelateSize / 2;

                    // make sure that the offset is within the boundry of the image
                    while (xx + offsetX >= image.Width) offsetX--;
                    while (yy + offsetY >= image.Height) offsetY--;

                    // get the pixel color in the center of the soon to be pixelated area
                    Color pixel = pixelated.GetPixel(xx + offsetX, yy + offsetY);

                    // for each pixel in the pixelate size, set it to the center color
                    for (Int32 x = xx; x < xx + pixelateSize && x < image.Width; x++)
                        for (Int32 y = yy; y < yy + pixelateSize && y < image.Height; y++)
                            pixelated.SetPixel(x, y, pixel);
                }
            }

            return pixelated;
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                x = int.Parse(textBox2.Text);
            }
            catch (Exception)
            {
                // will pass this for now
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

            try
            {
                //pixel size
                pixel_size = int.Parse(textBox4.Text);
            }
            catch (Exception)
            {
                // will pass this for now
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                y = int.Parse(textBox3.Text);
            }
            catch (Exception)
            {
                // will pass this for now
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // open dialog
            // load our selected image
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string pattern_path = openFileDialog1.FileName;

                gridControl1.InitializeFromXml(pattern_path);

                // open xml or excel document

                // visualize on gridControl1
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // resize gridcontrol cells
            gridControl1.ResetColWidthEntries();
            gridControl1.ResetRowHeightEntries();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // reset gridcontrol
            gridControl1.ClearCells(GridRangeInfo.Table(), true);
        }

        private void FillCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void integerTextBox1_TextChanged(object sender, EventArgs e)
        {
            fill_tresh = int.Parse(integerTextBox1.Text);
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            gridControl1.ClearCells(gridControl1.Selections.Ranges, true);
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            add_border_all();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            add_border(get_selected_cells());
        }

        private void button12_Click(object sender, EventArgs e)
        {
            int i = int.Parse(integerTextBox2.Text);
            cell_size = new Size(i, i);
            gridControl1.SetColWidth(0,gridControl1.ColCount,i);
            gridControl1.SetRowHeight(0, gridControl1.RowCount, i);
        }

        private void integerTextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }

}
class PixelEditor : Panel
{
    public Color DrawColor { get; set; }
    public Color GridColor { get; set; }
    int pixelSize = 8;
    public int PixelSize
    {
        get { return pixelSize; }
        set
        {
            pixelSize = value;
            Invalidate();
        }
    }


    public Bitmap TgtBitmap { get; set; }
    public Point TgtMousePos { get; set; }

    Point lastPoint = Point.Empty;

    PictureBox aPBox = null;
    public PictureBox APBox
    {
        get { return aPBox; }
        set
        {
            if (value == null) return;
            aPBox = value;
            aPBox.MouseClick -= APBox_MouseClick;
            aPBox.MouseClick += APBox_MouseClick;
        }
    }

    private void APBox_MouseClick(object sender, MouseEventArgs e)
    {
        TgtMousePos = e.Location;
        Invalidate();
    }


    public PixelEditor()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        GridColor = Color.DimGray;
        DrawColor = Color.Red;
        PixelSize = 10;
        TgtMousePos = Point.Empty;

        if (APBox != null && APBox.Image != null)
            TgtBitmap = (Bitmap)APBox.Image;

        MouseClick += PixelEditor_MouseClick;
        MouseMove += PixelEditor_MouseMove;
        Paint += PixelEditor_Paint;
    }

    private void PixelEditor_Paint(object sender, PaintEventArgs e)
    {
        if (DesignMode) return;

        Graphics g = e.Graphics;

        int cols = ClientSize.Width / PixelSize;
        int rows = ClientSize.Height / PixelSize;

        if (TgtMousePos.X < 0 || TgtMousePos.Y < 0) return;

        for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
            {
                int sx = TgtMousePos.X + x;
                int sy = TgtMousePos.Y + y;

                if (sx > TgtBitmap.Width || sy > TgtBitmap.Height) continue;

                Color col = TgtBitmap.GetPixel(sx, sy);

                using (SolidBrush b = new SolidBrush(col))
                using (Pen p = new Pen(GridColor))
                {
                    Rectangle rect = new Rectangle(x * PixelSize, y * PixelSize,
                                                       PixelSize, PixelSize);
                    g.FillRectangle(b, rect);
                    g.DrawRectangle(p, rect);
                }
            }
    }

    private void PixelEditor_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        int x = TgtMousePos.X + e.X / PixelSize;
        int y = TgtMousePos.Y + e.Y / PixelSize;

        if (new Point(x, y) == lastPoint) return;

        Bitmap bmp = (Bitmap)APBox.Image;
        bmp.SetPixel(x, y, DrawColor);
        APBox.Image = bmp;
        Invalidate();
        lastPoint = new Point(x, y);
    }

    private void PixelEditor_MouseClick(object sender, MouseEventArgs e)
    {
        int x = TgtMousePos.X + e.X / PixelSize;
        int y = TgtMousePos.Y + e.Y / PixelSize;
        Bitmap bmp = (Bitmap)APBox.Image;
        bmp.SetPixel(x, y, DrawColor);
        APBox.Image = bmp;
        Invalidate();
    }


}