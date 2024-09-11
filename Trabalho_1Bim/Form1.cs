using System;
using System.Drawing;
using System.Windows.Forms;


namespace Trabalho_1Bim
{
    public partial class Form1 : Form
    {
        private Image image;         // Atributo para armazenar a imagem carregada
        private Bitmap imageBitmap;  // Atributo para armazenar a imagem convertida em Bitmap

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (imageBitmap != null)
            {
                Bitmap imgDest = new Bitmap(image);
                imageBitmap = (Bitmap)image;
                new ZhangSuen().AfinamentoComDMA(imageBitmap, imgDest);
                pictureBox2.Image = imgDest;
            }
            else
            {
                MessageBox.Show("Por favor, carregue uma imagem primeiro.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (imageBitmap != null)
            {
                imageBitmap = (Bitmap)image;
                Bitmap imgDest = new Bitmap(image);
                new ContourExtraction().Countour(imageBitmap, imgDest);
                pictureBox2.Image = imgDest;
            }
            else
            {
                MessageBox.Show("Por favor, carregue uma imagem primeiro.");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (imageBitmap != null)
            {
                Bitmap imgDest = new Bitmap(imageBitmap);
                ContourRectangle contourRectangle = new ContourRectangle();
                contourRectangle.ProcessImageAndDrawRectangles(imageBitmap, imgDest);
                pictureBox2.Image = imgDest;
            }
            else
            {
                MessageBox.Show("Por favor, carregue uma imagem primeiro.");
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                image = Image.FromFile(openFileDialog.FileName); 
                imageBitmap = (Bitmap)image;
                pictureBox1.Image = image;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                saveFileDialog.Title = "Salvar imagem como";
                saveFileDialog.FileName = "imagem_resultante";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show("Imagem salva com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Nenhuma imagem para salvar!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
